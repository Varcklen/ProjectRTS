using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Project.Inventory;
using Project.AbilitySystem;

namespace UnitTests
{
    public class InventoryTest
    {
        [SetUp]
        public void Setup()
        {
            SceneManager.LoadScene("Scenes/SampleScene");
        }
        public GameObject createNewItemMock(string newItemName)
		{
            GameObject itemGo = new GameObject();
            itemGo.transform.position = Vector3.zero;
            ItemInfoMock mockIteminfoMock = itemGo.AddComponent<ItemInfoMock>();
            mockIteminfoMock.Stats.abilities = new List<AbilityObject>();
            mockIteminfoMock.Stats.name = newItemName;
            mockIteminfoMock.Stats.itemType = ItemType.Item;

            return itemGo;
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestAddAndRemoveItemFromInventory()
        {
            // Initialize the fake unit
            GameObject obj = new GameObject();
            UnitInfoMock mockUnitInfo = obj.AddComponent<UnitInfoMock>();
            obj.transform.position = Vector3.zero;

            yield return null;

            // Getting the inventory manager and try adding an item to it
            InventoryManager inventoryManager = mockUnitInfo.inventoryManager;

            Assert.That(inventoryManager.GetInventoryLimit(), Is.EqualTo(6));
            Assert.That(inventoryManager.GetNumItemsInInventory(), Is.EqualTo(0));

            string newItemName = "TestItem1";

            GameObject itemGo = createNewItemMock(newItemName);

            inventoryManager.RightClick(itemGo.GetComponent<ItemInfo>());

            yield return null;

            // Now check that the item is in the inventory
            Assert.That(inventoryManager.GetNumItemsInInventory(), Is.EqualTo(1));
            ItemObject[] items = inventoryManager.GetItemList();

            bool newItemInInventory = false;
            ItemObject newlyAddedItem = null;
            for(int i = 0; i < items.Length; ++i)
			{
                if(items[i] != null)
				{
                    if(items[i].name == newItemName)
					{
                        newlyAddedItem = items[i];
                        newItemInInventory = true;
                    }
				}
			}

            Assert.That(newItemInInventory, Is.EqualTo(newItemInInventory));

            // Now remove the item from the inventory
            inventoryManager.RemoveItem(newlyAddedItem);
            for (int i = 0; i < items.Length; ++i)
            {
                if (items[i] != null)
                {
                    Assert.That(items[i].name, Is.Not.EqualTo(newItemName));
                }
            }

            Assert.That(inventoryManager.GetNumItemsInInventory(), Is.EqualTo(0));
        }
    }
}