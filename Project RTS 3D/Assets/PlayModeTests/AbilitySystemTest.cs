using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Project.Inventory;
using Project.BuffSystem;
using Project.AbilitySystem;

namespace UnitTests
{
    public class AbilitySystemTest
    {
        GameObject peasantInstance = null;

        public delegate void CheckAbilityObjectFn(AbilityObject abilityObject);

        [SetUp]
        public void Setup()
        {
            SceneManager.LoadScene("Scenes/SampleScene");
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.Destroy(peasantInstance);
        }

        public void GoThroughAbilityList(AbilityManager abilityManager, CheckAbilityObjectFn checkAbilityObjectFn)
		{
            for (int i = 0; i < abilityManager.abilities.Length; ++i)
            {
                if (abilityManager.abilities[i] != null)
                {
                    checkAbilityObjectFn(abilityManager.abilities[i]);
                }
            }
        }

        [UnityTest]
        public IEnumerator TestRemoveAbility()
        {
            peasantInstance = TestUtilities.createPeasant(Vector3.zero);
            UnitInfo peasantUnitInfo = peasantInstance.GetComponent<UnitInfo>();

            yield return null;

            AbilityManager abilityManager = peasantUnitInfo.abilityManager;

            // Get initial state
            int numInitialAbilitiesInList = 0;
            AbilityObject firstAbility = null;

            GoThroughAbilityList(abilityManager,
                (abilityObject) => {
                    ++numInitialAbilitiesInList;

                    if (firstAbility == null)
                    {
                        firstAbility = abilityObject;
                    }
                });

            // Remove an ability
            abilityManager.RemoveAbility(firstAbility);

            // Get state after ability removal
            int numAbilitiesInListAfterRemoval = 0;
            GoThroughAbilityList(abilityManager,
                (abilityObject) => {
                    ++numAbilitiesInListAfterRemoval;
                    Assert.That(firstAbility.name, Is.Not.EqualTo(abilityObject.name));
                });

            Assert.That(numAbilitiesInListAfterRemoval, Is.EqualTo(numInitialAbilitiesInList - 1));

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestAddAbility()
        {
            peasantInstance = TestUtilities.createPeasant(Vector3.zero);
            UnitInfo peasantUnitInfo = peasantInstance.GetComponent<UnitInfo>();

            yield return null;

            AbilityManager abilityManager = peasantUnitInfo.abilityManager;

            AbilityData newAbility = ScriptableObject.CreateInstance<Ability_Attack>();
            newAbility.ability = new Ability();
            newAbility.ability.name = "testAttackName";

            // Get initial state
            int numInitialAbilitiesInList = 0;
            GoThroughAbilityList(abilityManager,
                (abilityObject) => {
                    ++numInitialAbilitiesInList;
                    Assert.That(newAbility.name, Is.Not.EqualTo(abilityObject.name));
                });

            // Remove an ability
            abilityManager.AddAbility(newAbility);

            // Get state after ability removal
            int numAbilitiesInListAfterAddition = 0;
            bool isNewAbilityFound = false;
            GoThroughAbilityList(abilityManager,
                (abilityObject) => {
                    ++numAbilitiesInListAfterAddition;
                    if (newAbility.ability.name.Equals(abilityObject.name))
                    {
                        isNewAbilityFound = true;
                    }
                });

            Assert.That(numAbilitiesInListAfterAddition, Is.EqualTo(numInitialAbilitiesInList + 1));
            Assert.That(isNewAbilityFound, Is.EqualTo(true));

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestReplaceAbility()
        {
            peasantInstance = TestUtilities.createPeasant(Vector3.zero);
            UnitInfo peasantUnitInfo = peasantInstance.GetComponent<UnitInfo>();

            yield return null;

            AbilityManager abilityManager = peasantUnitInfo.abilityManager;

            AbilityData newAbility = ScriptableObject.CreateInstance<Ability_Attack>();
            newAbility.ability = new Ability();
            newAbility.ability.name = "testAttackName";

            // Get initial state
            int numInitialAbilitiesInList = 0;
            AbilityObject firstAbility = null;
            GoThroughAbilityList(abilityManager,
                (abilityObject) => {
                    ++numInitialAbilitiesInList;
                    Assert.That(newAbility.name, Is.Not.EqualTo(abilityObject.name));
                    if (firstAbility == null)
                    {
                        firstAbility = abilityObject;
                    }
                });

            // Remove an ability
            abilityManager.ReplaceAbility(firstAbility, newAbility);

            // Get state after ability removal
            int numAbilitiesInListAfterReplace = 0;
            bool isNewAbilityFound = false;
            bool isOldAbilityFound = false;
            GoThroughAbilityList(abilityManager,
                (abilityObject) => {
                    ++numAbilitiesInListAfterReplace;
                    if (newAbility.ability.name.Equals(abilityObject.name))
                    {
                        isNewAbilityFound = true;
                    }

                    if (firstAbility.name.Equals(abilityObject.name))
                    {
                        isOldAbilityFound = true;
                    }
                });

            Assert.That(numAbilitiesInListAfterReplace, Is.EqualTo(numInitialAbilitiesInList));
            Assert.That(isNewAbilityFound, Is.EqualTo(true));
            Assert.That(isOldAbilityFound, Is.EqualTo(false));

            yield return null;
        }
    }
}