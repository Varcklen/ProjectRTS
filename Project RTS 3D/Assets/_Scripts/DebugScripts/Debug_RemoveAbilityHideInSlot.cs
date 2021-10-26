using Project.AbilitySystem;
using UnityEngine;

namespace Project.DebugWorld
{

    public class Debug_RemoveAbilityHideInSlot : MonoBehaviour
    {
        [SerializeField] UnitInfo unitInfo;
        [SerializeField, Range(1, 100)] int slot = 1;
        [SerializeField] KeyCode key;

    #if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(key))
            {
                DestroyItem();
            }
        }
    #endif

        void DestroyItem()
        {
            Debug.Log("Hide ability deleted!");
            if (unitInfo == null) return;
            AbilityObject ability = unitInfo.abilityManager.abilitiesHide[slot - 1];
            if (ability != null)
                unitInfo.abilityManager.RemoveAbility(ability);
        }
    }

}
