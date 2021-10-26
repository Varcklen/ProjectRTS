using System;
using UnityEngine;
using UnityEngine.AI;
using Project.Inventory;
using Project.BuffSystem;
using Project.AbilitySystem;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView), typeof(PhotonTransformViewClassic))]
public class UnitInfo : ObjectInfo, IPunObservable, IPunOwnershipCallbacks
{
    [field: Header("Stats"), SerializeField, ConditionalHide("unique", false)]
    public UnitStats Stats { get; private set; }

    [Header("Parameters")]
    [SerializeField] protected Unit unit;

    [field: SerializeField]
    //private Player playerOwning;
    public Player PlayerOwning { get; private set; }
    //{
    //    get
    //    {
    //        return playerOwning;
    //    }
    //    private set
    //    {
    //        playerOwning = value;
    //        if (Application.isPlaying)
    //        {
    //            return;
    //        }
    //        PlayerInfo playerInfo = PlayerInfo.FindPlayerInfoByGameObjectName(playerOwning.ToString());
    //        if (playerInfo != null)
    //        {
    //            transform.SetParent(playerInfo.transform);
    //        }
    //    }
    //}

    [SerializeField] private float selectionCircleSizePercent = 1f;
    [SerializeField] private float healthBarDeviationY;

    #region Delegates/Actions
    public delegate void MoveDelegate(Vector3 point, bool isCommand = false);
    public MoveDelegate moveDelegate { get; private set; } //Replace to Move()
    public Action stopDelegate { get; private set; } //Replace to Stop()
    #endregion

    [HideInInspector] public List<AbilityData> abilityData;

    private TaskList task;

    private Vector3 movePoint;

    private Coroutine turnToSideCoroutine;

    private ActionList actionList;

    private NavMeshAgent agent;

    private WorldHealthBar worldHealthBar;

    private Photon.Realtime.Player localPlayer;

    public Animator Animator { get; private set; }

    public PlayerInfo Owner { get; private set; }

    //public PhotonView view { get; private set; }

    //Effects
    private int silencePoints;
    public bool IsSilenced { get { return silencePoints > 0; } }

    private int stunPoints;
    public bool IsStunned { get { return stunPoints > 0; } }

    //Modules
    public AttackManager attackManager { get; private set; }
    public AbilityManager abilityManager { get; private set; }
    public InventoryManager inventoryManager { get; private set; }
    public BuffManager buffManager { get; private set; }
    public DamageManager damageManager { get; private set; }

    private const float AGENT_ACCELERATION = 60f;
    private const float DEGREES_TO_ROTATE_END = 5f;

    protected void Awake()
    {
        // Initializing Stats if it wasn't already in order to make unit testing easier
        if (Stats == null) 
        {
            Stats = new UnitStats();
        }
        worldHealthBar = transform?.Find("HealthBarCanvas")?.GetComponent<WorldHealthBar>();
        agent = GetComponent<NavMeshAgent>();
        AwakeInfo();
        SetUnit();
        Animator = GetComponent<Animator>();
        actionList = FindObjectOfType<ActionList>();
        //view = GetComponent<PhotonView>();
        stopDelegate = Stop;
        moveDelegate = Move;
        selectionDelegate = Selection;

        localPlayer = PhotonNetwork.LocalPlayer;

        //Debug.Log("localPlayer.ActorNumber = " + localPlayer.ActorNumber + ", (int)playerOwning = " + (int)PlayerOwning + ", transform.parent.gameObject.name = " + transform.parent.gameObject.name);

        if (PlayerIdUtils.doesPlayerNumMatchLocalPlayer(localPlayer, PlayerOwning))
		{
            Debug.Log("Requesting ownership. gameObject.name = " + gameObject.name);
            photonView.RequestOwnership();
        }
    }
    
    protected void Start()
    {
        Owner = PlayerManager.Instance.GetPlayer(PlayerOwning);
        transform.SetParent(Owner.transform);
        StartInfo();
        if (Stats.inventoryData == null)
        {
            Stats.inventoryData = LoadResourceManager.Instance.baseInventory;
        }
        //Modules
        damageManager = new DamageManager(this);
        attackManager = new AttackManager(this);
        inventoryManager = new InventoryManager(this, Stats.inventoryData, Stats.inventoryLimit);
        buffManager = new BuffManager(this);
        abilityManager = new AbilityManager(this, abilityData);
    }

    private new void OnDisable()//Delete "new"?
    {
        abilityManager.OnDisable();
    }

    private void OnDestroy()
    {
        damageManager.OnDestroy();
        Stats.OnDestroy();
        UnitSelections.Instance.Deselect(this);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && isSelected)
        {
            RightClick();
        }
        attackManager?.Update();
        HealthAndManaRegeneration();
        WorldHealthBar();
        SetSpeedAnimation();
    }

    private void RightClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            RightClickUse(hit);
        }
    }
    private void HealthAndManaRegeneration()
    {
        if (damageManager.isDead)
        {
            return;
        }
        if (Stats.MaxHealth > 0)
        {
            Stats.Health = Mathf.Clamp(Stats.Health + (Stats.healthRegeneration * Time.deltaTime), 0, Stats.MaxHealth);
        }
        if (Stats.maxMana > 0)
        {
            Stats.Mana = Mathf.Clamp(Stats.Mana + (Stats.manaRegeneration * Time.deltaTime), 0, Stats.maxMana); ;
        }
        if (Stats.Health <= 0)
        {
            damageManager.Kill();
        }
    }
    private void WorldHealthBar()
    {
        if (worldHealthBar == null)
        {
            return;
        }
        worldHealthBar.IsManaBarVisible = Stats.maxMana > 0;
        worldHealthBar.SetHealthBar(Stats.Health, Stats.MaxHealth, damageManager.Shield, damageManager.MaxShield);
        worldHealthBar.SetManaBar(Stats.Mana, Stats.maxMana);
    }
    private void SetSpeedAnimation()
    {
        if (Animator == null || agent == null)
        {
            return;
        }
        byte animation = 0;
        if (agent.speed > 5f)
        {
            animation = 1;
        }
        Animator.SetFloat("Speed", animation);
    }

    ///<summary>
    ///Return true, if unit alive.
    ///</summary>
    public bool IsUnitAlive()
    {
        return Stats.Health > 0;
    }

    ///<summary>
    ///Returns the distance between two units.
    ///</summary>
    public float DistanceBetweenUnits(UnitInfo secondUnit)
    {
        return (transform.position - secondUnit.transform.position).sqrMagnitude;
    }

    ///<summary>
    ///Returns true if the owner of the unit is the player.
    ///</summary>
    public bool IsPlayerOwnerToUnit()
    {
        if (PhotonNetwork.IsConnected)
        {
            return photonView.ControllerActorNr == (int)PlayerOwning + 1;
        }
        else
        {
            return Player.Player01 == PlayerOwning;
        }
    }
    ///<summary>
    ///Returns true if the owner of the unit is the player.
    ///</summary>
    public bool IsPlayerOwnerToUnit(PlayerInfo player)
    {
        if (PhotonNetwork.IsConnected)
        {
            return photonView.ControllerActorNr == (int)player.PlayerOwn + 1;
        }
        else
        {
            return Player.Player01 == player.PlayerOwn;
        }
    }

    ///<summary>
    ///Returns true, if the units are allies.
    ///</summary>
    public bool IsUnitAllyToUnit(UnitInfo secondUnit)
    {
        return Owner.IsThisPlayerAllyToPlayer(secondUnit.Owner);
    }

    ///<summary>
    ///The unit goes over to the side of the other player.
    ///</summary>
    public void SetOwnerPlayer(PlayerInfo newOwner)
    {
        if (Owner == newOwner)
        {
            return;
        }
        Owner = newOwner;
        PlayerOwning = newOwner.PlayerOwn;
        transform.SetParent(newOwner.transform);
        if (UI_Manager.Instance.selectedObject == this)
        {
            UI_Manager.Instance.RefreshInfoPanel();
        }
    }

    ///<summary>
    ///Rotates the unit Y in the direction of the specified vector.
    ///</summary>
    public void TurnToSide(Vector3 side)
    {
        if (transform.position == side)
        {
            return;
        }
        if (turnToSideCoroutine != null)
        {
            StopCoroutine(turnToSideCoroutine);
        }
        turnToSideCoroutine = StartCoroutine(Rotate(side));
    }
    private IEnumerator Rotate(Vector3 side)
    {
        Quaternion rotation = Quaternion.LookRotation(side - transform.position);
        float speed = AGENT_ACCELERATION / 10;
        while (Quaternion.Angle(transform.rotation, rotation) > DEGREES_TO_ROTATE_END)
        {
            yield return null;
            rotation = Quaternion.LookRotation(side - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * speed);
        }
    }

    ///<summary>
    ///Hides the unit.
    ///</summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    ///<summary>
    ///Unhides the unit.
    ///</summary>
    public void Unhide()
    {
        gameObject.SetActive(true);
    }

    ///<summary>
    ///Destroys the unit.
    ///</summary>
    public void Destroy()
    {
        Destroy(gameObject);
    }

    ///<summary>
    ///The unit stops any action.
    ///</summary>
    public void Stop()
    {
        //if (turnToSideCoroutine != null)
        //{
        //    StopCoroutine(turnToSideCoroutine);
        //}
        actionList.Stop(agent);
    }

    ///<summary>
    ///Adds or removes silence points. If silence points are greater than 0, the unit cannot use abilities.
    ///</summary>
    public void AddSilencePoints(int point)
    {
        silencePoints += point;
        if (UI_Manager.Instance.selectedObject == this)
        {
            UI_Manager.Instance.RefreshInfoPanel();
        }
    }

    ///<summary>
    ///Adds or removes stun points. If stun points are greater than 0, the hero cannot control the hero.
    ///</summary>
    public void AddStunPoints(int point)
    {
        int oldPoints = stunPoints;
        stunPoints += point;
        if (UI_Manager.Instance.selectedObject == this)
        {
            ButtonManager.Instance.HideButtonsForUpdate();
            UI_Manager.Instance.RefreshInfoPanel();
        }
        if (stunPoints > oldPoints && oldPoints == 0 && stopDelegate != null)
        {
            stopDelegate();
        }
        Animator.SetBool("IsDizzy", IsStunned);
    }

    #region SetRegion
    public void SetTask(TaskList task)
    {
        this.task = task;
    }
    public void SetMovePoint(Vector3 movePoint)
    {
        this.movePoint = movePoint;
    }
    public void RemoveMana(int mana)
    {
        Stats.Mana -= mana;
    }
    public void RemoveHealth(int health)
    {
        Stats.Health -= health;
    }
    #endregion

    #region GetRegion
    public Attack GetAttack()
    {
        return Stats.attack;
    }
    //[Obsolete("This method has been deprecated. Please, use Stats")]
    public Stats GetStats()
    {
        return Stats;
    }
    public float GetHealth()
    {
        return Stats.Health;
    }
    public float GetMana()
    {
        return Stats.Mana;
    }
    public TaskList GetTask()
    {
        return task;
    }
    public Vector3 GetMovePoint()
    {
        return movePoint;
    }
    #endregion

    private void RightClickUse(RaycastHit hit)
    {
        if (AbilityManager.IsFindTarget)
        {
            return;
        }
        Transform target = hit.collider.transform;
        if (hit.collider.tag == "Ground")
        {
            if (moveDelegate != null)
            {
                moveDelegate(hit.point, true);
            }
        }
        else if (target.TryGetComponent(out ItemInfo itemInfo))
        {
            inventoryManager?.RightClick(itemInfo);
        }
    }

    private void Move(Vector3 point, bool isCommand)
    {
        if (agent == null || GlobalMethods.IsPointerOverUIObject() || IsStunned)
        {
            return;
        }
        actionList.Move(agent, point, isCommand);
    }

    //Sets parameters for a unit in the editor.
    protected void OnDrawGizmos()
    {
        if (unit == null || Application.isPlaying)
        {
            return;
        }
        SetUnit();
    }

    //Sets parameters from a Scriptible Object
    private void SetUnit()
    {
        if (unit == null)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("\"Unit\" for " + gameObject + " is not setted.");
            }
            return;
        }
        objectType = unit.objectType;
        if (minimapIcon != null)
        {
            minimapIcon.sprite = GlobalMethods.GetMinimapSprite(objectType);
        }
        else
        {
            minimapIcon = transform.Find("MinimapImage")?.GetComponent<SpriteRenderer>();
        }
        if (!unique)
        {
            Stats.SetStatsFromData(unit);
            if (unit.abilities.Count > 0)
            {
                abilityData.Clear();
                for (int i = 0; i < unit.abilities.Count; i++)
                {
                    abilityData.Add(unit.abilities[i]);
                }
            }
        }
        if (agent != null)
        {
            Stats.SetNavMeshParameters(agent);
            agent.acceleration = AGENT_ACCELERATION;
        }
        if (selectionIndicator != null)
        {
            selectionIndicator.transform.localScale *= selectionCircleSizePercent;
        }
        if (worldHealthBar != null)
        {
            worldHealthBar.transform.position += new Vector3(0, healthBarDeviationY, 0);
        }
        Stats.SetUnitStatsFromStats(Stats, this);
    }

    //Object selection
    private void Selection()
    {
        if (UnitSelections.Instance.unitSelected.Count > 1)
        {
            UI_Manager.Instance.UIDisplaySquad(this, chooseFirst: true);
        }
        else
        {
            UI_Manager.Instance.UIDisplay(this);
        }
    }

    public void TakeDamage(UnitInfo dealer, float damage, AttackType attackType)
    {
        damageManager?.TakeDamage(dealer, damage, attackType);
    }

    public bool IsUnitControlled()
	{
        return photonView.IsMine && PhotonNetwork.IsConnected;
    }

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Stats.Health);
            stream.SendNext(Stats.Mana);
        }
        else
        {
            Stats.Health = (float)stream.ReceiveNext();
            Stats.Mana = (float)stream.ReceiveNext();
        }
    }


    #endregion

    #region IPunOwnershipCallbacks implementation
    public void OnOwnershipRequest(PhotonView targetView, Photon.Realtime.Player requestingPlayer)
    {
        if(PlayerIdUtils.doesPlayerNumMatchLocalPlayer(requestingPlayer, PlayerOwning))
		{
            Debug.Log("UnitInfo::OnOwnershipRequest - " + gameObject.name + " ownership will be transferred to " + requestingPlayer.NickName);
            targetView.TransferOwnership(requestingPlayer);
        }
    }
    public void OnOwnershipTransfered(PhotonView targetView, Photon.Realtime.Player requestingPlayer)
    {
        Debug.Log("UnitInfo::OnOwnershipTransfered - " + requestingPlayer.NickName + " now owns " + gameObject.name);
    }
    public void OnOwnershipTransferFailed(PhotonView targetView, Photon.Realtime.Player requestingPlayer)
	{
        Debug.Log("UnitInfo::OnOwnershipTransferFailed - " + requestingPlayer.NickName + " failed to get " + gameObject.name);
    }
    #endregion
}
