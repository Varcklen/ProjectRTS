using Project.Projectile;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

//Everything has been moved to Object Info. This script is currently inactive.
public class AttackManager : IProjectileEnd
{

    private UnitInfo unit;
    private UnitInfo target;

    private GameObject gameObject;

    private NavMeshAgent agent;

    private Animator animator;

    ///<summary>
    ///Unit return point in Defensive mode
    ///</summary>
    public Vector3 standPost;

    ///<summary>
    ///The condition of whether the order is executed by the player (true) or independently (for example, with an aggro unit)
    ///</summary>
    private bool isOrder;

    ///<summary>
    ///Active when a unit is reloading an attack
    ///</summary>
    private bool isReloading = false;

    private float timeLeft = 0;

    public Action attackDelegate { get; private set; }

    //Coroutines
    private Coroutine moveCoroutine;
    private Coroutine delayCoroutine;
    private Coroutine cooldownCoroutine;
    private Coroutine waitForEndCoroutine;

    public AttackManager(UnitInfo unit)
    {
        this.unit = unit;
        animator = unit.Animator;
        gameObject = unit.gameObject;
        agent = gameObject.GetComponent<NavMeshAgent>();
        attackDelegate = SetAttackStyleDelegate(unit.GetAttack().attackStyle);
    }

    public void Update()
    {
        Aggro();
    }

    //Forces a unit to attack an enemy unit within an aggro radius.
    private void Aggro()
    {
        if (unit.GetTask() != TaskList.Idle || unit.GetAttack().attackStyle == AttackStyle.None || unit.GetAttack().Damage <= 0 || unit.IsStunned)
        {
            return;
        }
        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, Mathf.Pow(unit.GetAttack().aggroRange, 0.5f), 64);//64=Selectable
        UnitInfo closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in colliders)
        {
            GameObject target = collider.gameObject;
            if (!target.TryGetComponent(out UnitInfo unitInfo))
            {
                continue;
            }
            if (unitInfo.IsUnitAlive() && !unitInfo.IsUnitAllyToUnit(unit))
            {
                float distance = (target.transform.position - gameObject.transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = unitInfo;
                }
            }
        }
        if (closestEnemy != null)
        {
            Attack(closestEnemy, false);
        }
    }

    ///<summary>
    ///Switch between attack modes
    ///</summary>
    public AttackState SwitchAttackState()
    {
        AttackState attackState = unit.GetAttack().attackState;
        switch (attackState)
        {
            case AttackState.Defensive:
                unit.GetAttack().attackState = AttackState.Agressive;
                break;
            case AttackState.Agressive:
                unit.GetAttack().attackState = AttackState.Stand;
                break;
            case AttackState.Stand:
                unit.GetAttack().attackState = AttackState.Defensive;
                break;
        }
        return unit.GetAttack().attackState;
    }

    ///<summary>
    ///Forces a unit to attack another unit.
    ///</summary>
    public void AttackTarget(UnitInfo target)
    {
        if (target == null)
        {
            Debug.LogWarning("target in AttackManager/ChooseUnitToAttack is null (" + gameObject + ").");
            return;
        }
        if (target == unit)
        {
            return;
        }
        Attack(target, true);
    }

    ///<summary>
    ///Sets a delegate to UnitInfo. Other types of attacks can be added in the future.
    ///</summary>
    private Action SetAttackStyleDelegate(AttackStyle attackStyle)
    {
        switch (attackStyle)
        {
            case AttackStyle.Melee:
                return MeleeAttack;
            case AttackStyle.Range:
                return RangeAttack;
            default:
                return null;
        }
    }

    //Basic conditions necessary for attacking a target.
    private bool AttackCondition()
    {
        if (target == null) return false;
        if (unit.GetTask() != TaskList.Attack) return false;
        if (!unit.IsUnitAlive()) return false;
        if (!target.IsUnitAlive()) return false;
        return true;
    }

    #region Attack Basis
    //Attack start
    private void Attack(UnitInfo target, bool isOrder)
    {
        AttackState attackState = unit.GetAttack().attackState;
        this.isOrder = isOrder;
        switch (attackState)
        {
            case AttackState.Defensive:
                AttackDefensive(target);
                break;
            case AttackState.Agressive:
                AttackAgressive(target);
                break;
            case AttackState.Stand:
                AttackStand(target);
                break;
        }
    }

    //In Defensive mode, the unit will attack the enemy, but will return to the place if the target moves a certain distance.
    void AttackDefensive(UnitInfo target)
    {
        if (!isOrder)
        {
            standPost = gameObject.transform.position;
        }
        BasisStartAttack(target);
    }

    //In Aggressive mode, the unit attacks any enemy nearby.
    void AttackAgressive(UnitInfo target)
    {
        BasisStartAttack(target);
    }

    //In Stand mode, the unit does not attack opponents, but attacks them if it can.
    void AttackStand(UnitInfo target)
    {
        if (unit.GetAttack().range < unit.DistanceBetweenUnits(target) && !isOrder)
        {
            return;
        }
        BasisStartAttack(target);
    }

    //Sets parameters that can be set in any mode.
    void BasisStartAttack(UnitInfo target)
    {
        StopAllCoroutines();
        if (timeLeft > 0)
        {
            cooldownCoroutine = unit.StartCoroutine(AttackCooldown());
            waitForEndCoroutine = unit.StartCoroutine(WaitForEndAttackCooldown());
        }
        unit.SetTask(TaskList.Attack);
        this.target = target;
        unit.TurnToSide(target.transform.position);
        if (unit.GetAttack().range >= unit.DistanceBetweenUnits(target))
        {
            StartAttack();
        }
        else
        {
            MoveToTarget();
        }
    }

    //Turns off all currently active coroutines from this script.
    public void StopAllCoroutines()
    {
        if (moveCoroutine != null) unit.StopCoroutine(moveCoroutine);
        if (delayCoroutine != null) unit.StopCoroutine(delayCoroutine);
        if (cooldownCoroutine != null) unit.StopCoroutine(cooldownCoroutine);
        if (waitForEndCoroutine != null) unit.StopCoroutine(waitForEndCoroutine);
    }

    //If the unit's attack range is less than the distance between units, it starts moving towards the target.
    void MoveToTarget()
    {
        if (moveCoroutine != null)
        {
            unit.StopCoroutine(moveCoroutine);
        }
        moveCoroutine = unit.StartCoroutine(Move());
    }
    
    //Movement towards the target until the attack range is greater than the distance between units.
    IEnumerator Move()
    {
        //Debug.Log("Move");
        unit.Animator.SetBool("IsMoved", true);
        while (AttackCondition() && unit.GetAttack().range < unit.DistanceBetweenUnits(target) && DefensiveCondition())
        {
            agent.destination = target.transform.position;
            yield return null;
        }
        unit.Animator.SetBool("IsMoved", false);
        //Debug.Log("Move: End");
        if (!DefensiveCondition())
        {
            //Debug.Log("Move: moveDelegate");
            if (unit.moveDelegate != null)
            {
                unit.moveDelegate(standPost);
            }
        }
        else if (AttackCondition() && unit.GetAttack().range >= unit.DistanceBetweenUnits(target))
        {
            //Debug.Log("Move: StartAttack()");
            StartAttack();
        }
        else if (unit.GetTask() == TaskList.Attack)
        {
            //Debug.Log("Move: stopDelegate()");
            if (unit.stopDelegate != null)
            {
                unit.stopDelegate();
            }  
        }
    }

    //Starts unit attack
    void StartAttack()
    {
        if (timeLeft > 0)
        {
            //Debug.Log("Return");
            //if (unit.stopDelegate != null)
            //{
            //    unit.stopDelegate();
            //}
            if (waitForEndCoroutine != null)
            {
                unit.StopCoroutine(waitForEndCoroutine);
            }
            waitForEndCoroutine = unit.StartCoroutine(WaitForEndAttackCooldown());
            return;
        }
        //Debug.Log("StartAttack");
        unit.Animator.SetFloat("AttackAnimation", UnityEngine.Random.Range(0,1));
        unit.Animator.SetBool("IsAttack", true);
        agent.destination = gameObject.transform.position;
        delayCoroutine = unit.StartCoroutine(AttackDelay());
        cooldownCoroutine = unit.StartCoroutine(AttackCooldown());
    }
    
    //Delay before starting an attack
    IEnumerator AttackDelay()
    {
        //Debug.Log("AttackDelay");
        yield return new WaitForSeconds(unit.GetAttack().attackDelay);
        //Debug.Log("AttackDelay: End");
        unit.Animator.SetBool("IsAttack", false);
        if (!DefensiveCondition())
        {
            //Debug.Log("AttackDelay: DefensiveCondition");
            if (unit.moveDelegate != null)
                unit.moveDelegate(standPost);
        }
        else if (AttackCondition() && unit.GetAttack().range >= unit.DistanceBetweenUnits(target) && DefensiveCondition())
        {
            //Debug.Log("AttackDelay: attackDelegate");
            if (attackDelegate != null)
            {
                attackDelegate();
            }
            if (!isReloading)
                StartAttack();
            else
                waitForEndCoroutine = unit.StartCoroutine(WaitForEndAttackCooldown());
        }
        else if (unit.GetAttack().attackState != AttackState.Stand)
        {
            //Debug.Log("AttackDelay: MoveToTarget");
            MoveToTarget();
        }
        else
        {
            //Debug.Log("AttackDelay: stopDelegate");
            if (unit.stopDelegate != null)
                unit.stopDelegate();
        }
    }

    //The condition for the unit in Defensive mode to return to the defense point after the enemy moves a certain distance.
    bool DefensiveCondition()
    {
        if (unit.GetAttack().attackState != AttackState.Defensive || standPost == Vector3.zero)
        {
            return true;
        }
        if (unit == null || target == null)
        {
            return false;
        }
        float range = Mathf.Pow(unit.GetAttack().range, 2);
        float distance = (standPost - target.transform.position).sqrMagnitude;
        return range > distance;
    }

    //Attack cooldown
    IEnumerator AttackCooldown()
    {
        //Debug.Log("AttackCooldown");
        Attack attack = unit.GetAttack();
        isReloading = true;
        float time = Time.time;
        if (timeLeft > 0)
        {
            time += timeLeft;
        }
        else
        {
            time += attack.attackCooldown;
        }
        while (Time.time < time )
        {
            yield return null;
            timeLeft = time - Time.time;
        }
        timeLeft = 0;
        isReloading = false;
        //Debug.Log("AttackCooldown End");
    }

    //Waiting for the end of attack cooldown
    IEnumerator WaitForEndAttackCooldown()
    {
        //Debug.Log("WaitForEndAttackCooldown");
        while (isReloading && DefensiveCondition())
        {
            yield return null;
        }
        //Debug.Log("WaitForEndAttackCooldown End");
        if (!DefensiveCondition())
        {
            //Debug.Log("WaitForEndAttackCooldown: DefensiveCondition()");
            if (unit.moveDelegate != null)
                unit.moveDelegate(standPost);
        }
        else if (AttackCondition())
        {
            //Debug.Log("WaitForEndAttackCooldown: AttackCondition");
            if (unit.GetAttack().range >= unit.DistanceBetweenUnits(target))
            {
                //Debug.Log("WaitForEndAttackCooldown: StartAttack();");
                StartAttack();
            }
            else if (unit.GetAttack().attackState == AttackState.Agressive || isOrder)
            {
                //Debug.Log("WaitForEndAttackCooldown: MoveToTarget();");
                MoveToTarget();
            }
            else
            {
                //Debug.Log("WaitForEndAttackCooldown: stopDelegate;");
                if (unit.stopDelegate != null)
                    unit.stopDelegate();
            }
        }
        else if (unit.GetTask() == TaskList.Attack)
        {
            //Debug.Log("WaitForEndAttackCooldown: unit.GetTask() == TaskList.Attack");
            if (unit.stopDelegate != null)
                unit.stopDelegate();
        }
    }
    #endregion

    #region Melee Attack
    private void MeleeAttack()
    {
        target.damageManager.TakeDamage(unit, unit.GetAttack().Damage, unit.GetAttack().attackType);
    }
    #endregion

    #region Range Attack
    private void RangeAttack()
    {
        Projectile projectile = unit.GetAttack().projectile;
        ProjectileData projectileData = new ProjectileData 
        { 
            target = target,
            damage = unit.GetAttack().Damage,
            attackType = unit.GetAttack().attackType
        };
        ProjectileInfo.CreateAndLaunch(unit.transform.position, projectile.model, unit.transform.rotation, // + unit.GetAttack().projectile.startDeviation
            target, this, projectileData, projectile.speed, projectile.arcAngle);
    }

    public void ProjectileEnd(ProjectileData projectileData)
    {
        projectileData.target.damageManager.TakeDamage(unit, projectileData.damage, projectileData.attackType);
    }
    #endregion
}