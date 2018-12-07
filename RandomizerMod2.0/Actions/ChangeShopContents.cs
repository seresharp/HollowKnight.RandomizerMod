using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    [Serializable]
    public struct ShopItemDef
    {
        // Values from ShopItemStats
        [SerializeField] public string PlayerDataBoolName;
        [SerializeField] public string NameConvo;
        [SerializeField] public string DescConvo;
        [SerializeField] public string RequiredPlayerDataBool;
        [SerializeField] public string RemovalPlayerDataBool;
        [SerializeField] public bool DungDiscount;
        [SerializeField] public string NotchCostBool;
        [SerializeField] public int Cost;

        // Sprite name in resources
        [SerializeField] public string SpriteName;
    }

    [Serializable]
    public class ChangeShopContents : RandomizerAction, ISerializationCallbackReceiver
    {
        // Variables that actually get used
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        private ShopItemDef[] items;

        // Variable for serialization hack
        [SerializeField] private List<string> itemDefStrings;

        public ChangeShopContents(string sceneName, string objectName, ShopItemDef[] items)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.items = items;
        }

        public override ActionType Type => ActionType.GameObject;

        public string SceneName => sceneName;

        public string ObjectName => objectName;

        public void AddItemDefs(ShopItemDef[] newItems)
        {
            if (items == null)
            {
                items = newItems;
                return;
            }

            if (newItems == null)
            {
                return;
            }

            ShopItemDef[] combined = new ShopItemDef[items.Length + newItems.Length];
            items.CopyTo(combined, 0);
            newItems.CopyTo(combined, items.Length);
            items = combined;
        }

        public override void Process(string scene, Object changeObj)
        {
            if (scene != sceneName)
            {
                return;
            }

            // Find the shop and save an item for use later
            GameObject shopObj = GameObject.Find(objectName);
            ShopMenuStock shop = shopObj.GetComponent<ShopMenuStock>();
            GameObject itemPrefab = Object.Instantiate(shop.stock[0]);
            itemPrefab.SetActive(false);
                
            // Remove all charm type items from the store
            List<GameObject> newStock = new List<GameObject>();
                
            foreach (ShopItemDef itemDef in items)
            {
                // Create a new shop item for this item def
                GameObject newItemObj = Object.Instantiate(itemPrefab);
                newItemObj.SetActive(false);

                // Apply all the stored values
                ShopItemStats stats = newItemObj.GetComponent<ShopItemStats>();
                stats.playerDataBoolName = itemDef.PlayerDataBoolName;
                stats.nameConvo = itemDef.NameConvo;
                stats.descConvo = itemDef.DescConvo;
                stats.requiredPlayerDataBool = itemDef.RequiredPlayerDataBool;
                stats.removalPlayerDataBool = itemDef.RemovalPlayerDataBool;
                stats.dungDiscount = itemDef.DungDiscount;
                stats.notchCostBool = itemDef.NotchCostBool;
                stats.cost = itemDef.Cost;

                // Need to set all these to make sure the item doesn't break in one of various ways
                stats.priceConvo = string.Empty;
                stats.specialType = 2;
                stats.charmsRequired = 0;
                stats.relic = false;
                stats.relicNumber = 0;
                stats.relicPDInt = string.Empty;

                // Apply the sprite for the UI
                stats.transform.Find("Item Sprite").gameObject.GetComponent<SpriteRenderer>().sprite = RandomizerMod.GetSprite(itemDef.SpriteName);

                newStock.Add(newItemObj);
            }

            // Save unchanged list for potential alt stock
            List<GameObject> altStock = new List<GameObject>();
            altStock.AddRange(newStock);

            // Update normal stock
            foreach (GameObject item in shop.stock)
            {
                // It would be cleaner to destroy the unused objects, but that breaks the shop on subsequent loads
                // TC must be reusing the shop items rather than destroying them on load
                if (item.GetComponent<ShopItemStats>().specialType != 2)
                {
                    newStock.Add(item);
                }
            }

            shop.stock = newStock.ToArray();

            // Update alt stock
            if (shop.stockAlt != null)
            {
                foreach (GameObject item in shop.stockAlt)
                {
                    if (item.GetComponent<ShopItemStats>().specialType != 2)
                    {
                        altStock.Add(item);
                    }
                }

                shop.stockAlt = altStock.ToArray();
            }
        }

        public void OnBeforeSerialize()
        {
            itemDefStrings = new List<string>();
            foreach (ShopItemDef item in items)
            {
                itemDefStrings.Add(JsonUtility.ToJson(item));
            }
        }

        public void OnAfterDeserialize()
        {
            List<ShopItemDef> itemDefList = new List<ShopItemDef>();

            foreach (string item in itemDefStrings)
            {
                itemDefList.Add(JsonUtility.FromJson<ShopItemDef>(item));
            }

            items = itemDefList.ToArray();
        }
    }
}
