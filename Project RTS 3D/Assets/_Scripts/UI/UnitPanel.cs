using Project.AbilitySystem;
using UnityEngine;

//Some UnitPanel properties
public class UnitPanel : MonoBehaviour
{//It might be worth moving the ButtonManager.cs into this script.

    private CanvasGroup InfoPanel;

    void Start()
    {
        InfoPanel = GetComponent<CanvasGroup>();
        InfoPanel.alpha = 0;
        InfoPanel.blocksRaycasts = false;
        InfoPanel.interactable = false;
    }

    //Used in a button in the "PlusLevelButton" GameObject. Launches a trigger to start selecting an upgrade ability.
    public void ChooseAbilityToUpgrade()
    {
        ButtonManager buttonManager = ButtonManager.Instance;
        UI_Manager UIManager = UI_Manager.Instance;
        if (UIManager.unitInfo.IsStunned)
        {
            return;
        }
        if (!buttonManager.isUpgradeMode)
        {
            buttonManager.RevealButtonsForUpdate();
        }
        else
        {
            buttonManager.HideButtonsForUpdate();
        }
        
    }
}
