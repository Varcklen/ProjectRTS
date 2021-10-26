using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Project.Inventory;
using Project.BuffSystem;
using Project.AbilitySystem;
using TMPro;
using System.Collections.Generic;

//Responsible for the player's UI on the screen.
public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance { get; private set; }

    public UnitInfo unitInfo { get; private set; }

    private ItemInfo itemInfo;

    public ObjectInfo selectedObject { get; private set; }

    private Coroutine barsCoroutine;

    private Camera choosedCamera;

    #region UI
    private CanvasGroup infoPanel;
    private Image icon;
    
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI attackText;
    private TextMeshProUGUI armorText;
    
    private RectTransform extraStats;
    private TextMeshProUGUI spellPowerText;
    private TextMeshProUGUI luckText;

    private RectTransform heroStats;
    private TextMeshProUGUI strText;
    private TextMeshProUGUI agiText;
    private TextMeshProUGUI intText;
    private TextMeshProUGUI levelText;

    private Slider healthBar;
    private Slider shieldBar;
    private TextMeshProUGUI healthText;

    private Slider manaBar;
    private TextMeshProUGUI manaText;

    private UI_Inventory inventoryUI;
    private UI_BuffPanel buffPanel;

    private TextMeshProUGUI itemDescriptionText;

    private RectTransform unitStats;
    private RectTransform itemStats;

    private RectTransform upgradeAbilityPanel;
    private TextMeshProUGUI upgradesText;

    private RectTransform characterInfo;

    private RectTransform squadPanelParent;
    private SquadPanel squadPanel;
    #endregion

    private const string INVUL_TEXT = "Invul.";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        #region UI set
        infoPanel = GameObject.Find("UnitPanel").GetComponent<CanvasGroup>();
        icon = GameObject.Find("IconUI").GetComponent<Image>();

        nameText = GameObject.Find("NameUI").GetComponent<TextMeshProUGUI>();
        attackText = GameObject.Find("AttackText").GetComponent<TextMeshProUGUI>();
        armorText = GameObject.Find("ArmorText").GetComponent<TextMeshProUGUI>();
        
        extraStats = GameObject.Find("ExtraStatsUI").GetComponent<RectTransform>();
        spellPowerText = GameObject.Find("SpellPowerText").GetComponent<TextMeshProUGUI>();
        luckText = GameObject.Find("LuckText").GetComponent<TextMeshProUGUI>();

        heroStats = GameObject.Find("HeroStatsUI").GetComponent<RectTransform>();
        strText = GameObject.Find("STRText").GetComponent<TextMeshProUGUI>();
        agiText = GameObject.Find("AGIText").GetComponent<TextMeshProUGUI>();
        intText = GameObject.Find("INTText").GetComponent<TextMeshProUGUI>();
        levelText = GameObject.Find("LevelText").GetComponent<TextMeshProUGUI>();

        healthBar = GameObject.Find("HealthBarUI").GetComponent<Slider>();
        shieldBar = GameObject.Find("ShieldBarUI").GetComponent<Slider>();
        healthText = GameObject.Find("HealthDisplay").GetComponent<TextMeshProUGUI>();

        manaBar = GameObject.Find("ManaBarUI").GetComponent<Slider>();
        manaText = GameObject.Find("ManaDisplay").GetComponent<TextMeshProUGUI>();

        inventoryUI = GameObject.Find("UI_Inventory").GetComponent<UI_Inventory>();
        buffPanel = GameObject.Find("BuffPanel").GetComponent<UI_BuffPanel>();

        itemDescriptionText = GameObject.Find("ItemDescriptionText").GetComponent<TextMeshProUGUI>();

        unitStats = GameObject.Find("UnitStatsUI").GetComponent<RectTransform>();
        itemStats = GameObject.Find("ItemStatsUI").GetComponent<RectTransform>();

        upgradeAbilityPanel = GameObject.Find("UpgradeAbilityPanel").GetComponent<RectTransform>();
        upgradesText = GameObject.Find("UpgradesText").GetComponent<TextMeshProUGUI>();

        characterInfo = GameObject.Find("CharacterInfoUI").GetComponent<RectTransform>();

        squadPanelParent = GameObject.Find("SquadPanelUI").GetComponent<RectTransform>();
        squadPanel = squadPanelParent.Find("SquadContentUI").GetComponent<SquadPanel>();
        #endregion

        squadPanel.gameObject.SetActive(true);
        squadPanelParent.gameObject.SetActive(false);
    }

    public void RefreshInfoPanel()
    {
        if (selectedObject != null)
        {
            if (unitInfo != null)
            {
                UIDisplay(unitInfo);
            } 
            else if (itemInfo != null)
            {
                UIDisplay(itemInfo);
            }
        }
    }

    //Show/Hide UI interface (for unit)
    public void UIDisplay(UnitInfo unit)
    {
        selectedObject = unit;
        unitInfo = unit;
        itemInfo = null;

        UIDisplayGeneral(false, false);
        SetUnitStats();
    }

    //Show/Hide UI interface (for item)
    public void UIDisplay(ItemInfo item)
    {
        selectedObject = item;
        unitInfo = null;
        itemInfo = item;
        UIDisplayGeneral(true, false);

        SetItemStats();
    }

    //Show/Hide UI interface (for squad)
    public void UIDisplaySquad(UnitInfo choosedUnit, UnitInfo deletedUnit = null, bool chooseFirst = false)
    {
        List<ObjectInfo> units = UnitSelections.Instance.unitSelected;
        if (units.Count == 0)
        {
            ClearInfoPanel();
            return;
        }
        if (chooseFirst)
        {
            choosedUnit = (UnitInfo)units[0];
        }
        else if (choosedUnit == null)
        {
            if (unitInfo == null || deletedUnit == unitInfo)
            {
                choosedUnit = (UnitInfo)units[0];
            }
            else
            {
                choosedUnit = unitInfo;
            }
        }
        selectedObject = choosedUnit;
        unitInfo = choosedUnit;
        itemInfo = null;
        UIDisplayGeneral(false, units.Count > 1);
        squadPanel.SetSquadDisplay(units, choosedUnit);
        SetUnitStats();
    }

    void UIDisplayGeneral(bool isItem, bool isSquad)
    {
        unitStats.gameObject.SetActive(!isItem && !isSquad);
        squadPanelParent.gameObject.SetActive(isSquad);
        characterInfo.gameObject.SetActive(!isItem);
        itemStats.gameObject.SetActive(isItem);

        infoPanel.alpha = 1;
        infoPanel.blocksRaycasts = true;
        infoPanel.interactable = true;

        if (choosedCamera != null)
        {
            choosedCamera?.gameObject?.SetActive(false);
        }
        choosedCamera = selectedObject.iconCam;
        choosedCamera?.gameObject?.SetActive(true);
    } 

    //Specifies information on the unit info panel
    void SetUnitStats()
    {
        nameText.text = unitInfo.GetStats().name;
        icon.sprite = unitInfo.GetStats().icon; 

        attackText.text = unitInfo.GetAttack().Damage.ToString();
        if (unitInfo.damageManager.IsInvulnerable)
        {
            armorText.text = INVUL_TEXT;
        }
        else
        {
            armorText.text = Mathf.CeilToInt(unitInfo.Stats.Armor).ToString();
        }

        //Sets parameters for spell power and luck
        float spellPower = unitInfo.GetStats().SpellPower;
        int luck = unitInfo.GetStats().LuckPoint;
        if (unitInfo.IsUnitHasFlag(ObjectType.Hero) || spellPower > 0 || luck > 0)
        {
            spellPowerText.text = spellPower.ToString() + "%";
            luckText.text = luck.ToString() + "%";
            extraStats.gameObject.SetActive(true);
        }
        else
        {
            extraStats.gameObject.SetActive(false);
        }

        //Sets the parameters of the hero
        if (unitInfo.TryGetComponent(out HeroInfo heroInfo))
        {
            HeroObjectStats stats = heroInfo.HeroStats;
            strText.text = stats.Strength.ToString();
            agiText.text = stats.Agility.ToString();
            intText.text = stats.Intelligence.ToString();
            levelText.text = stats.Level.ToString();
            heroStats.gameObject.SetActive(true);

            if (stats.UpgradePoints > 0 && heroInfo.IsCanUpgradeAbility() && unitInfo.IsPlayerOwnerToUnit())
            {
                upgradesText.text = Mathf.Clamp(stats.UpgradePoints, -9, 99).ToString();
                upgradeAbilityPanel.gameObject.SetActive(true);
            }
            else
            {
                upgradeAbilityPanel.gameObject.SetActive(false);
            }
        }
        else
        {
            heroStats.gameObject.SetActive(false);
            upgradeAbilityPanel.gameObject.SetActive(false);
        }

        inventoryUI.SetInventory(unitInfo);
        buffPanel.SetUnit(unitInfo);

        //If it is the owner of the unit, shows the buttons. Otherwise, hides them.
        if (unitInfo.IsPlayerOwnerToUnit())
        {
            ButtonManager.Instance.UpdateButtons(unitInfo);
        }
        else
        {
            ButtonManager.Instance.HideAllButtons();
        }

        //#if !UNITY_EDITOR
        //if (unitInfo != null && unitInfo.IsUnitControlled())
        //{
        //    ButtonManager.Instance.UpdateButtons(unitInfo);
        //}
        //#else
        //        ButtonManager.Instance.UpdateButtons(unitInfo);
        //#endif

        SetUIHealthMana();
        if (barsCoroutine != null)
        {
            StopCoroutine(barsCoroutine);
        }
        barsCoroutine = StartCoroutine(HealthManaChange());
    }

    //Specifies information on the item info panel
    void SetItemStats()
    {
        nameText.text = itemInfo.Stats.name;
        icon.sprite = itemInfo.Stats.icon;

        healthBar.gameObject.SetActive(false);
        manaBar.gameObject.SetActive(false);

        itemDescriptionText.text = itemInfo.Stats.GetTooltipInfoText(new DescriptionInfo { withoutName = true });
    }

    //Turns off the panel when no units are selected
    public void ClearInfoPanel()
    {
        selectedObject = null;
        unitInfo = null;
        itemInfo = null;
        infoPanel.alpha = 0;
        infoPanel.blocksRaycasts = false;
        infoPanel.interactable = false;
    }

#region Health and Mana Bars
    //Bars UI change per frame
    IEnumerator HealthManaChange()
    {
        ObjectInfo target = selectedObject;
        while (infoPanel.alpha > 0 && selectedObject == target)
        {
            SetUIHealthMana();
            yield return null;
        }
    }

    //Bars UI change
    void SetUIHealthMana()
    {
        //float version is needed to smoothly change the bar
        //int version is needed for integer mappings of the current resource

        float health = unitInfo.GetHealth();
        float maxHealth = unitInfo.GetStats().MaxHealth;

        int healthInt = (int)health;
        int maxHealthInt = (int)maxHealth;

        float shield = unitInfo.damageManager.Shield;
        int shieldInt = (int)shield;
        if (shieldInt > 0)
        {
            float maxShield = unitInfo.damageManager.MaxShield;
            int maxShieldInt = (int)maxShield;
            healthText.text = healthInt.ToString() + "/" + maxHealthInt.ToString() + " (" + shieldInt.ToString() + "/" + maxShieldInt.ToString() + ")";
            shieldBar.maxValue = maxShield;
            shieldBar.value = shield;
            shieldBar.gameObject.SetActive(true);
        }
        else
        {
            healthText.text = healthInt.ToString() + "/" + maxHealthInt.ToString();
            shieldBar.gameObject.SetActive(false);
        }
        healthBar.maxValue = maxHealth;
        healthBar.value = health;
        healthBar.gameObject.SetActive(true);

        float maxMana = unitInfo.GetStats().maxMana;
        int maxManaInt = (int)maxMana;
        if (maxManaInt > 0)
        {
            float mana = unitInfo.GetMana();
            int manaInt = (int)mana;
            manaText.text = manaInt.ToString() + "/" + maxManaInt.ToString();
            manaBar.maxValue = maxMana;
            manaBar.value = mana;
            manaBar.gameObject.SetActive(true);
        }
        else
        {
            manaBar.gameObject.SetActive(false);
        }
    }
#endregion
}
