using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace UnitTests
{
    public class UnitInfoTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestRemoveHealth()
        {
            GameObject peasantGo = TestUtilities.createPeasant(Vector3.zero);
            UnitInfo peasantUnitInfo = peasantGo.GetComponent<UnitInfo>();

            // Use the Assert class to test conditions
            Assert.That(peasantUnitInfo, Is.Not.EqualTo(null));

            float initialPeasantHealth = peasantUnitInfo.GetHealth();

            peasantUnitInfo.RemoveHealth(1);

            Assert.That(peasantUnitInfo.GetHealth(), Is.EqualTo(initialPeasantHealth - 1f));
        }

        [UnityTest]
        public IEnumerator TestTakeDamagePure()
        {
            SceneManager.LoadScene("Scenes/SampleScene");

            yield return null;

            GameObject peasantGo = TestUtilities.createPeasant(Vector3.zero);
            UnitInfo peasantUnitInfo = peasantGo.GetComponent<UnitInfo>();

            yield return null;

            float initialPeasantHealth = peasantUnitInfo.GetHealth();
            peasantUnitInfo.TakeDamage(peasantUnitInfo, 1f, AttackType.Pure);
            float PeasantHealthAfterDamage = peasantUnitInfo.GetHealth();
            Assert.That(PeasantHealthAfterDamage, Is.EqualTo(initialPeasantHealth - 1f));

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestTakeDamagePhysical()
        {
            SceneManager.LoadScene("Scenes/SampleScene");

            yield return null;

            GameObject peasantGo = TestUtilities.createPeasant(Vector3.zero);
            UnitInfo peasantUnitInfo = peasantGo.GetComponent<UnitInfo>();

            yield return null;

            float initialPeasantHealth = peasantUnitInfo.GetHealth();
            float damageToPeasant = 3f;
            peasantUnitInfo.TakeDamage(peasantUnitInfo, damageToPeasant, AttackType.Physical);
            float PeasantHealthAfterDamage = peasantUnitInfo.GetHealth();
            float expectedDamage = damageToPeasant * (1 - peasantUnitInfo.GetStats().ArmorPercent);
            Assert.That(PeasantHealthAfterDamage, Is.EqualTo(initialPeasantHealth - expectedDamage));

            yield return null;
        }
    }
}