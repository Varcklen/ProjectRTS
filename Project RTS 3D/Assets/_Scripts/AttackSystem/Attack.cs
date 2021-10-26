using UnityEngine;

//Basic data for the projectile of range attack
[System.Serializable]
public class Projectile
{
    public string model;
    public float speed;
    public float arcAngle;
    public Vector3 startDeviation;

    public void SetProjectileStatsFromUnit(Unit unit)
    {
        Projectile projectile = unit.stats.attack.projectile;
        model = projectile.model;
        speed = projectile.speed;
        arcAngle = projectile.arcAngle;
        startDeviation = projectile.startDeviation;
    }
}

//Basic data for the type of attack
[System.Serializable]
public class Attack
{
    [SerializeField]
    private float damage;
    public float Damage
    {
        get { return damage; }
        set
        {
            damage = value;
            UI_Manager.Instance?.RefreshInfoPanel();
        }
    }
    public AttackType attackType;
    public AttackStyle attackStyle;
    public AttackState attackState;
    public float attackCooldown;
    public float attackDelay;
    public float range;
    public float aggroRange;

    public Projectile projectile;

    public void SetAttackStatsFromUnit(Unit unit)
    {
        Attack attack = unit.stats.attack;
        damage = attack.damage;
        attackType = attack.attackType;
        attackStyle = attack.attackStyle;
        attackCooldown = Mathf.Clamp(attack.attackCooldown, 0.1f, Mathf.Infinity);
        attackDelay = Mathf.Clamp(attack.attackDelay, 0.1f, Mathf.Infinity);
        aggroRange = attack.aggroRange;
        range = attack.range;
        projectile.SetProjectileStatsFromUnit(unit);
    }
}
