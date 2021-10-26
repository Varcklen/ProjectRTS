using Project.BuffSystem;
using UnityEngine;

//Loads data about objects.
//Not used yet
public class LoadResourceManager : MonoBehaviour
{
    public static LoadResourceManager Instance { get; private set; }

    public InventoryData baseInventory;

    public GameObject baseItemPrefab;

    public BuffData baseShieldBuff;

    public Texture2D cursorTargetSprite;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

}
