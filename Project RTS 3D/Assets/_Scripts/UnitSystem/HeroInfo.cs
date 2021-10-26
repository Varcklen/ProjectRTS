using UnityEngine;
using Project.AbilitySystem;

//Any hero must have this script. Contains data that heroes should have.
public class HeroInfo : UnitInfo
{
    [field: Header("Hero Stats"), SerializeField, ConditionalHide("unique", false)]
    public HeroObjectStats HeroStats { get; private set; }

    [SerializeField] private HeroData hero;

    private new void Awake()
    {
        unit = hero;
        base.Awake();
        SetHero();
    }

    private new void Start()
    {
        base.Start();
    }

    private new void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (hero == null || Application.isPlaying)
            return;
        SetHero();
    }

    void SetHero()
    {
        if (hero == null)
        {
            Debug.LogWarning("\"Hero\" for " + gameObject + " is not setted.");
            return;
        }
        if (!objectType.HasFlag(ObjectType.Hero)) objectType += (int)ObjectType.Hero;
        if (minimapIcon != null)
        {
            minimapIcon.sprite = GlobalMethods.GetMinimapSprite(objectType);
        }
        else
        {
            minimapIcon = transform.Find("MinimapImage")?.GetComponent<SpriteRenderer>();
        }
        if (unique)
        {
            return;
        }
        HeroStats.SetStatsFromData(hero);
    }

    //The condition of whether the hero has abilities that can be improved
    public bool IsCanUpgradeAbility()
    {
        AbilityObject[] list = abilityManager.abilities;
        foreach (var ability in list)
        {
            if (ability == null)
            {
                continue;
            }
            if (ability.isUpgraded && ability.upgradeLimit > ability.level)
            {
                return true;
            }
        }
        return false;
    }

    //Improves the chosen ability
    public void UpgradeAbility(AbilityObject ability, KeyCode key = KeyCode.None)
    {
        ability.level++;
        HeroStats.ReduceAbilityUpgradePoint();
        if (HeroStats.UpgradePoints <= 0 || !IsCanUpgradeAbility())
        {
            ButtonManager.Instance.HideButtonsForUpdate();
        }
        else
        {
            ButtonManager.Instance.RevealButtonsForUpdate();
        }
        UI_Manager.Instance.RefreshInfoPanel();

        //Refreshes data when the ability upgrades.
        if (ability.component is IUpgradeAbility myInterface)
        {
            myInterface.UpgradeAbility();
        }
        //Should be no earlier than data refresh
        TooltipPopup.Instance.DisplayTooltip(ability, new DescriptionInfo { key = key });
    }
}

public interface IUpgradeAbility
{
    void UpgradeAbility();
}
