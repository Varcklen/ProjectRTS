using Photon.Pun;
using TMPro;
using UnityEngine;

//All information about the current player
[RequireComponent(typeof(PhotonView))]
public class PlayerInfo : MonoBehaviourPunCallbacks
{

    [SerializeField] private int gold;
    public int Gold
    {
        get
        {
            return gold;
        }
        set
        {
            gold = Mathf.Clamp(value, 0, 9999);
            if (goldText != null && IsLocal())
            {
                goldText.text = gold.ToString();
            }
        }
    }
    [field: SerializeField] public Player PlayerOwn { get; private set; }
    [SerializeField] private bool[] isPlayerAlly = new bool[8];

    private TextMeshProUGUI goldText;

    private Photon.Realtime.Player localPlayer;

    private void Awake()
    {
        localPlayer = PhotonNetwork.LocalPlayer;

        //Debug.Log("localPlayer.ActorNumber = " + localPlayer.ActorNumber + ", (int)PlayerOwn = " + (int)PlayerOwn + ", transform.parent.gameObject.name = " + transform.parent.gameObject.name);

        if (localPlayer.ActorNumber == ((int)PlayerOwn + 1))
        {
            Debug.Log("Requesting ownership. gameObject.name = " + gameObject.name);
            this.photonView.RequestOwnership();
        }
        //Debug.Log("photonView.CreatorActorNr: " + photonView.CreatorActorNr);
    }

    private void Start()
    {
        goldText = GameObject.Find("GoldDisplayUI").GetComponent<TextMeshProUGUI>();
        if (IsLocal())
        {
            goldText.text = gold.ToString();
        }
    }

    public bool GetPlayerRelation(int i)
    {
        if (i > isPlayerAlly.Length)
        {
            Debug.LogWarning("The specified number for PlayerInfo/GetPlayerRelation is above the player limit.");
            return false;
        } else if (i < 0)
        {
            Debug.LogWarning("The specified number for PlayerInfo/GetPlayerRelation is less than 0.");
            return false;
        }
        else
        {
            return isPlayerAlly[i];
        }
    }

    public bool IsThisPlayerAllyToPlayer(PlayerInfo secondPlayer)
    {
        return isPlayerAlly[(int)secondPlayer.PlayerOwn];
    }

    public bool IsLocal()
    {
        //For now photonView.CreatorActorNr is always 0.
        if (PhotonNetwork.IsConnected)
        {
            return photonView.CreatorActorNr == (int)PlayerOwn + 1;
        }
        else
        {
            return Player.Player01 == PlayerOwn;
        }
    }

    /// <summary>
    /// Searches the scene for a GameObject and returns the PlayerInfo component if it has one.
    /// </summary>
    public static PlayerInfo FindPlayerInfoByGameObjectName(string name)
    {
        GameObject myObject = GameObject.Find(name);
        if (myObject == null)
        {
            return null;
        }
        PlayerInfo playerInfo = myObject.GetComponent<PlayerInfo>();
        if (playerInfo == null)
        {
            Debug.LogWarning("The GameObject you are looking for PlayerInfo does not have this component.");
        }
        return playerInfo;
    }
}
