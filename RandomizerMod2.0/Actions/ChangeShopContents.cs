using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    [Serializable]
    public struct ShopItemDef
    {
        //Values from ShopItemStats
        [SerializeField] public string playerDataBoolName;
        [SerializeField] public string nameConvo;
        [SerializeField] public string descConvo;
        [SerializeField] public string requiredPlayerDataBool;
        [SerializeField] public string removalPlayerDataBool;
        [SerializeField] public bool dungDiscount;
        [SerializeField] public string notchCostBool;
        [SerializeField] public int cost;

        //Sprite name in resources
        [SerializeField] public string spriteName;
    }

    [Serializable]
    public class ChangeShopContents : RandomizerAction, ISerializationCallbackReceiver
    {
        //Variables that actually get used
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        private ShopItemDef[] items;

        //Variable for serialization hack
        [SerializeField] private List<string> itemDefStrings;

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

        public ChangeShopContents(string sceneName, string objectName, ShopItemDef[] items)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.items = items;
        }

        public override void Process()
        {
            if (GameManager.instance.GetSceneNameString() == sceneName)
            {
                //Find the shop and save an item for use later
                GameObject shopObj = GameObject.Find(objectName);
                ShopMenuStock shop = shopObj.GetComponent<ShopMenuStock>();
                GameObject itemPrefab = Object.Instantiate(shop.stock[0]);
                itemPrefab.SetActive(false);
                
                //Remove all charm type items from the store
                List<GameObject> newStock = new List<GameObject>();
                
                foreach (ShopItemDef itemDef in items)
                {
                    //Create a new shop item for this item def
                    GameObject newItemObj = Object.Instantiate(itemPrefab);
                    newItemObj.SetActive(false);

                    //Apply all the stored values
                    ShopItemStats stats = newItemObj.GetComponent<ShopItemStats>();
                    stats.playerDataBoolName = itemDef.playerDataBoolName;
                    stats.nameConvo = itemDef.nameConvo;
                    stats.descConvo = itemDef.descConvo;
                    stats.requiredPlayerDataBool = itemDef.requiredPlayerDataBool;
                    stats.removalPlayerDataBool = itemDef.removalPlayerDataBool;
                    stats.dungDiscount = itemDef.dungDiscount;
                    stats.notchCostBool = itemDef.notchCostBool;
                    stats.cost = itemDef.cost;

                    //Need to set all these to make sure the item doesn't break in one of various ways
                    stats.priceConvo = "";
                    stats.specialType = 2;
                    stats.charmsRequired = 0;
                    stats.relic = false;

                    //Apply the sprite for the UI
                    stats.transform.Find("Item Sprite").gameObject.GetComponent<SpriteRenderer>().sprite = RandomizerMod.sprites[itemDef.spriteName];

                    newStock.Add(newItemObj);
                }

                //Save unchanged list for potential alt stock
                List<GameObject> altStock = new List<GameObject>();
                altStock.AddRange(newStock);

                //Update normal stock
                foreach (GameObject item in shop.stock)
                {
                    //It would be cleaner to destroy the unused objects, but that breaks the shop on subsequent loads
                    //TC must be reusing the shop items rather than destroying them on load
                    if (item.GetComponent<ShopItemStats>().specialType != 2)
                    {
                        newStock.Add(item);
                    }
                }

                shop.stock = newStock.ToArray();

                //Update alt stock
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
        }
    }
}
