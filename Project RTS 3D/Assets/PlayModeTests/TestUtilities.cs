using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.AbilitySystem;

namespace UnitTests
{
    public class TestUtilities
    {
        static GameObject peasantPrefab = Resources.Load<GameObject>("Unit/Minion/Peasant");

        public static GameObject createPeasant(Vector3 posn)
        {
            Vector3 peasantPosn = Vector3.zero;
            Quaternion peasantRot = Quaternion.identity;
            GameObject peasantGo = GameObject.Instantiate(peasantPrefab, peasantPosn, peasantRot);

            return peasantGo;
        }
    }

    public class UnitInfoMock : UnitInfo
    {
        new protected void Awake()
		{
            base.Awake();
            Stats.inventoryData = ScriptableObject.CreateInstance<InventoryData>();
            Stats.inventoryLimit = 6;
            Stats.attack = new Attack();
            Stats.attack.projectile = new Projectile();

            abilityData = new List<AbilityData>();
        }

        new public void AwakeInfo()
        {
        }
    }

    public class ItemInfoMock : ItemInfo
    {
        new public void AwakeInfo()
        {
        }
    }
}