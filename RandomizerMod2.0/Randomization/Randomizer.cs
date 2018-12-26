﻿using System;
using System.Collections.Generic;
using System.Linq;
using RandomizerMod.Actions;

using Random = System.Random;

namespace RandomizerMod.Randomization
{
    internal static class Randomizer
    {
        private static Dictionary<string, int> additiveCounts;

        private static Dictionary<string, List<string>> shopItems;
        private static Dictionary<string, string> nonShopItems;

        private static List<string> unobtainedLocations;
        private static List<string> unobtainedItems;
        private static List<string> obtainedItems;

        private static List<RandomizerAction> actions;

        public static bool Done { get; private set; }

        public static RandomizerAction[] Actions => actions.ToArray();

        public static void Randomize()
        {
            SetupVariables();

            RandomizerMod.Instance.Log("Randomizing with seed: " + RandomizerMod.Instance.Settings.Seed);
            RandomizerMod.Instance.Log("Mode - " + (RandomizerMod.Instance.Settings.NoClaw ? "No Claw" : "Standard"));
            RandomizerMod.Instance.Log("Shade skips - " + RandomizerMod.Instance.Settings.ShadeSkips);
            RandomizerMod.Instance.Log("Acid skips - " + RandomizerMod.Instance.Settings.AcidSkips);
            RandomizerMod.Instance.Log("Spike tunnel skips - " + RandomizerMod.Instance.Settings.SpikeTunnels);
            RandomizerMod.Instance.Log("Misc skips - " + RandomizerMod.Instance.Settings.MiscSkips);
            RandomizerMod.Instance.Log("Fireball skips - " + RandomizerMod.Instance.Settings.FireballSkips);
            RandomizerMod.Instance.Log("Mag skips - " + RandomizerMod.Instance.Settings.MagSkips);

            Random rand = new Random(RandomizerMod.Instance.Settings.Seed);

            // For use in weighting item placement
            Dictionary<string, int> locationDepths = new Dictionary<string, int>();
            int currentDepth = 1;

            unobtainedItems.Remove("Wayward_Compass");
            unobtainedLocations.Remove("Grubberfly's_Elegy");
            nonShopItems.Add("Grubberfly's_Elegy", "Wayward_Compass");
            LogItemPlacement("Wayward_Compass", "Grubberfly's_Elegy");

            // Early game sucks too much if you don't get any geo, and the fury spot is weird anyway
            // Two birds with one stone
            RandomizerMod.Instance.Log("Placing initial geo pickup");

            string[] furyGeoContenders = unobtainedItems.Where(item => LogicManager.GetItemDef(item).type == ItemType.Geo && LogicManager.GetItemDef(item).geo > 100).ToArray();
            string furyGeoItem = furyGeoContenders[rand.Next(furyGeoContenders.Length)];

            unobtainedItems.Remove(furyGeoItem);
            unobtainedLocations.Remove("Fury_of_the_Fallen");
            nonShopItems.Add("Fury_of_the_Fallen", furyGeoItem);
            LogItemPlacement(furyGeoItem, "Fury_of_the_Fallen");

            RandomizerMod.Instance.Log("Beginning first pass of progression item placement");

            // Choose where to place progression items
            while (true)
            {
                // Get currently reachable locations
                List<string> reachableLocations = new List<string>();
                string[] obtained = obtainedItems.ToArray();
                int reachableCount = 0;

                for (int i = 0; i < unobtainedLocations.Count; i++)
                {
                    if (LogicManager.ParseLogic(unobtainedLocations[i], obtained))
                    {
                        if (!locationDepths.ContainsKey(unobtainedLocations[i]))
                        {
                            locationDepths[unobtainedLocations[i]] = currentDepth;
                        }

                        // This way further locations will be more likely to be picked
                        for (int j = 0; j < currentDepth; j++)
                        {
                            reachableLocations.Add(unobtainedLocations[i]);
                        }

                        reachableCount++;
                    }
                }

                List<string> progressionItems = GetProgressionItems(reachableCount);

                // We only need complex randomization until all progression items are placed
                // After that everything can just be placed completely randomly
                if (progressionItems.Count == 0)
                {
                    break;
                }

                string placeLocation = reachableLocations[rand.Next(reachableLocations.Count)];
                string placeItem = progressionItems[rand.Next(progressionItems.Count)];

                unobtainedLocations.Remove(placeLocation);
                unobtainedItems.Remove(placeItem);
                obtainedItems.Add(placeItem);

                LogItemPlacement(placeItem, placeLocation);

                if (shopItems.ContainsKey(placeLocation))
                {
                    shopItems[placeLocation].Add(placeItem);
                }
                else
                {
                    nonShopItems.Add(placeLocation, placeItem);
                }

                currentDepth++;
            }

            RandomizerMod.Instance.Log("Beginning second pass of progression item placement");

            // Place remaining potential progression items
            List<string> unusedProgressionItems = new List<string>();

            foreach (string str in unobtainedItems)
            {
                if (LogicManager.GetItemDef(str).progression)
                {
                    unusedProgressionItems.Add(str);
                }
            }

            while (unusedProgressionItems.Count > 0)
            {
                // TODO: Make extension to remove all of a string from a list so I don't have to recalculate this every time
                List<string> weightedLocations = new List<string>();
                foreach (string str in unobtainedLocations)
                {
                    // Items tagged as requiring "EVERYTHING" will not be in this dict
                    if (locationDepths.ContainsKey(str))
                    {
                        // Using weight^2 to heavily bias towards late locations
                        for (int i = 0; i < locationDepths[str] * locationDepths[str]; i++)
                        {
                            weightedLocations.Add(str);
                        }
                    }
                }

                string placeLocation = weightedLocations[rand.Next(weightedLocations.Count)];
                string placeItem = unusedProgressionItems[rand.Next(unusedProgressionItems.Count)];

                unobtainedLocations.Remove(placeLocation);
                unusedProgressionItems.Remove(placeItem);
                unobtainedItems.Remove(placeItem);
                obtainedItems.Add(placeItem);

                LogItemPlacement(placeItem, placeLocation);

                if (shopItems.ContainsKey(placeLocation))
                {
                    shopItems[placeLocation].Add(placeItem);
                }
                else
                {
                    nonShopItems.Add(placeLocation, placeItem);
                }
            }

            RandomizerMod.Instance.Log("Beginning placement of good items into remaining shops");

            // Place good items into shops that lack progression items
            List<string> goodItems = new List<string>();
            foreach (string str in unobtainedItems)
            {
                if (LogicManager.GetItemDef(str).isGoodItem)
                {
                    goodItems.Add(str);
                }
            }
            
            foreach (string shopName in shopItems.Keys.ToList())
            {
                if (shopItems[shopName].Count == 0)
                {
                    string placeItem = goodItems[rand.Next(goodItems.Count)];

                    unobtainedItems.Remove(placeItem);
                    goodItems.Remove(placeItem);
                    obtainedItems.Add(placeItem);

                    LogItemPlacement(placeItem, shopName);

                    shopItems[shopName].Add(placeItem);
                    unobtainedLocations.Remove(shopName);
                }
            }

            // Place geo drops first to guarantee they don't end up in shops
            RandomizerMod.Instance.Log("Beginning placement of geo drops");

            List<string> geoItems = unobtainedItems.Where(name => LogicManager.GetItemDef(name).type == ItemType.Geo).ToList();
            List<string> geoLocations = unobtainedLocations.Where(name => LogicManager.GetItemDef(name).cost == 0).ToList();
            foreach (string geoItem in geoItems)
            {
                string placeLocation = geoLocations[rand.Next(geoLocations.Count)];

                unobtainedLocations.Remove(placeLocation);
                geoLocations.Remove(placeLocation);

                unobtainedItems.Remove(geoItem);
                obtainedItems.Add(geoItem);

                nonShopItems.Add(placeLocation, geoItem);
                LogItemPlacement(geoItem, placeLocation);
            }

            RandomizerMod.Instance.Log("Beginning full random placement into remaining locations");

            // Randomly place into remaining locations
            while (unobtainedLocations.Count > 0)
            {
                string placeLocation = unobtainedLocations[rand.Next(unobtainedLocations.Count)];
                string placeItem = unobtainedItems[rand.Next(unobtainedItems.Count)];

                unobtainedLocations.Remove(placeLocation);
                unobtainedItems.Remove(placeItem);
                obtainedItems.Add(placeItem);

                LogItemPlacement(placeItem, placeLocation);

                if (shopItems.ContainsKey(placeLocation))
                {
                    shopItems[placeLocation].Add(placeItem);
                }
                else
                {
                    nonShopItems.Add(placeLocation, placeItem);
                }
            }

            RandomizerMod.Instance.Log("Beginning placement of leftover items into shops");

            string[] shopNames = shopItems.Keys.ToArray();

            // Put remaining items in shops
            while (unobtainedItems.Count > 0)
            {
                string placeLocation = shopNames[rand.Next(shopNames.Length)];
                string placeItem = unobtainedItems[rand.Next(unobtainedItems.Count)];
                
                unobtainedItems.Remove(placeItem);
                obtainedItems.Add(placeItem);

                LogItemPlacement(placeItem, placeLocation);

                shopItems[placeLocation].Add(placeItem);
            }

            actions = new List<RandomizerAction>();
            int newShinies = 0;

            foreach (KeyValuePair<string, string> kvp in nonShopItems)
            {
                string newItemName = kvp.Value;

                ReqDef oldItem = LogicManager.GetItemDef(kvp.Key);
                ReqDef newItem = LogicManager.GetItemDef(newItemName);

                if (oldItem.replace)
                {
                    actions.Add(new ReplaceObjectWithShiny(oldItem.sceneName, oldItem.objectName, "Randomizer Shiny"));
                    oldItem.objectName = "Randomizer Shiny";
                    oldItem.fsmName = "Shiny Control";
                    oldItem.type = ItemType.Charm;
                }
                else if (oldItem.newShiny)
                {
                    string newShinyName = "New Shiny " + newShinies++;
                    actions.Add(new CreateNewShiny(oldItem.sceneName, oldItem.x, oldItem.y, newShinyName));
                    oldItem.objectName = newShinyName;
                    oldItem.fsmName = "Shiny Control";
                    oldItem.type = ItemType.Charm;
                }
                else if (oldItem.type == ItemType.Geo && newItem.type != ItemType.Geo)
                {
                    actions.Add(new AddShinyToChest(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, "Randomizer Chest Shiny"));
                    oldItem.objectName = "Randomizer Chest Shiny";
                    oldItem.fsmName = "Shiny Control";
                    oldItem.type = ItemType.Charm;
                }

                string randomizerBoolName = GetAdditiveBoolName(newItemName);
                bool playerdata = false;
                if (string.IsNullOrEmpty(randomizerBoolName))
                {
                    randomizerBoolName = newItem.boolName;
                    playerdata = newItem.type != ItemType.Geo;
                }

                // Dream nail needs a special case
                if (oldItem.boolName == "hasDreamNail")
                {
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "Binding Shield Activate", "FSM", "Check", randomizerBoolName, playerdata));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "Dreamer Plaque Inspect", "Conversation Control", "End", randomizerBoolName, playerdata));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "Dreamer Scene 2", "Control", "Init", randomizerBoolName, playerdata));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "PreDreamnail", "FSM", "Check", randomizerBoolName, playerdata));
                    actions.Add(new ChangeBoolTest("RestingGrounds_04", "PostDreamnail", "FSM", "Check", randomizerBoolName, playerdata));
                }

                // Good luck to anyone trying to figure out this horrifying switch
                switch (oldItem.type)
                {
                    case ItemType.Charm:
                    case ItemType.Big:
                        switch (newItem.type)
                        {
                            case ItemType.Charm:
                            case ItemType.Shop:
                                actions.Add(new ChangeShinyIntoCharm(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItem.boolName));
                                if (!string.IsNullOrEmpty(oldItem.altObjectName))
                                {
                                    actions.Add(new ChangeShinyIntoCharm(oldItem.sceneName, oldItem.altObjectName, oldItem.fsmName, newItem.boolName));
                                }

                                break;
                            case ItemType.Big:
                            case ItemType.Spell:
                                BigItemDef[] newItemsArray = GetBigItemDefArray(newItemName);

                                actions.Add(new ChangeShinyIntoBigItem(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItemsArray, randomizerBoolName, playerdata));
                                if (!string.IsNullOrEmpty(oldItem.altObjectName))
                                {
                                    actions.Add(new ChangeShinyIntoBigItem(oldItem.sceneName, oldItem.altObjectName, oldItem.fsmName, newItemsArray, randomizerBoolName, playerdata));
                                }

                                break;
                            case ItemType.Geo:
                                if (oldItem.inChest)
                                {
                                    actions.Add(new ChangeChestGeo(oldItem.sceneName, oldItem.chestName, oldItem.chestFsmName, newItem.geo));
                                }
                                else
                                {
                                    actions.Add(new ChangeShinyIntoGeo(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItem.boolName, newItem.geo));
                                }

                                break;
                            default:
                                throw new Exception("Unimplemented type in randomization: " + oldItem.type);
                        }

                        break;
                    case ItemType.Geo:
                        switch (newItem.type)
                        {
                            case ItemType.Geo:
                                actions.Add(new ChangeChestGeo(oldItem.sceneName, oldItem.objectName, oldItem.fsmName, newItem.geo));
                                break;
                            default:
                                throw new Exception("Unimplemented type in randomization: " + oldItem.type);
                        }

                        break;
                    default:
                        throw new Exception("Unimplemented type in randomization: " + oldItem.type);
                }

                if (oldItem.cost != 0)
                {
                    actions.Add(new AddYNDialogueToShiny(
                        oldItem.sceneName,
                        oldItem.objectName,
                        oldItem.fsmName,
                        newItem.nameKey,
                        oldItem.cost,
                        oldItem.sceneName == SceneNames.RestingGrounds_07 ? AddYNDialogueToShiny.TYPE_ESSENCE : AddYNDialogueToShiny.TYPE_GEO));
                }
            }

            int shopAdditiveItems = 0;
            List<ChangeShopContents> shopActions = new List<ChangeShopContents>();

            // TODO: Change to use additiveItems rather than hard coded
            // No point rewriting this before making the shop component
            foreach (KeyValuePair<string, List<string>> kvp in shopItems)
            {
                string shopName = kvp.Key;
                List<string> newShopItems = kvp.Value;

                List<ShopItemDef> newShopItemStats = new List<ShopItemDef>();

                foreach (string item in newShopItems)
                {
                    ReqDef newItem = LogicManager.GetItemDef(item);

                    if (newItem.type == ItemType.Spell)
                    {
                        switch (newItem.boolName)
                        {
                            case "hasVengefulSpirit":
                            case "hasShadeSoul":
                                newItem.boolName = "RandomizerMod.ShopFireball" + shopAdditiveItems++;
                                break;
                            case "hasDesolateDive":
                            case "hasDescendingDark":
                                newItem.boolName = "RandomizerMod.ShopQuake" + shopAdditiveItems++;
                                break;
                            case "hasHowlingWraiths":
                            case "hasAbyssShriek":
                                newItem.boolName = "RandomizerMod.ShopScream" + shopAdditiveItems++;
                                break;
                            default:
                                throw new Exception("Unknown spell name: " + newItem.boolName);
                        }
                    }
                    else if (newItem.boolName == "hasDash" || newItem.boolName == "hasShadowDash")
                    {
                        newItem.boolName = "RandomizerMod.ShopDash" + shopAdditiveItems++;
                    }
                    else if (newItem.boolName == nameof(PlayerData.hasDreamNail) || newItem.boolName == nameof(PlayerData.hasDreamGate))
                    {
                        newItem.boolName = "RandomizerMod.ShopDreamNail" + shopAdditiveItems++;
                    }

                    newShopItemStats.Add(new ShopItemDef()
                    {
                        PlayerDataBoolName = newItem.boolName,
                        NameConvo = newItem.nameKey,
                        DescConvo = newItem.shopDescKey,
                        RequiredPlayerDataBool = LogicManager.GetShopDef(shopName).requiredPlayerDataBool,
                        RemovalPlayerDataBool = string.Empty,
                        DungDiscount = LogicManager.GetShopDef(shopName).dungDiscount,
                        NotchCostBool = newItem.notchCost,
                        Cost = 100 + (rand.Next(41) * 10),
                        SpriteName = newItem.shopSpriteKey
                    });
                }

                ChangeShopContents existingShopAction = shopActions.Where(action => action.SceneName == LogicManager.GetShopDef(shopName).sceneName && action.ObjectName == LogicManager.GetShopDef(shopName).objectName).FirstOrDefault();

                if (existingShopAction == null)
                {
                    shopActions.Add(new ChangeShopContents(LogicManager.GetShopDef(shopName).sceneName, LogicManager.GetShopDef(shopName).objectName, newShopItemStats.ToArray()));
                }
                else
                {
                    existingShopAction.AddItemDefs(newShopItemStats.ToArray());
                }
            }

            shopActions.ForEach(action => actions.Add(action));

            Done = true;
            RandomizerMod.Instance.Log("Randomization done");
        }

        private static void SetupVariables()
        {
            nonShopItems = new Dictionary<string, string>();

            shopItems = new Dictionary<string, List<string>>();
            foreach (string shopName in LogicManager.ShopNames)
            {
                shopItems.Add(shopName, new List<string>());
            }
            ////shopItems.Add("Lemm", new List<string>()); TODO: Custom shop component to handle lemm

            unobtainedLocations = new List<string>();
            foreach (string itemName in LogicManager.ItemNames)
            {
                if (LogicManager.GetItemDef(itemName).type != ItemType.Shop)
                {
                    unobtainedLocations.Add(itemName);
                }
            }

            unobtainedLocations.AddRange(shopItems.Keys);
            unobtainedItems = LogicManager.ItemNames.ToList();
            obtainedItems = new List<string>();

            // Don't place claw in no claw mode, obviously
            if (RandomizerMod.Instance.Settings.NoClaw)
            {
                unobtainedItems.Remove("Mantis_Claw");
            }

            Done = false;
        }

        private static List<string> GetProgressionItems(int reachableCount)
        {
            List<string> progression = new List<string>();
            string[] obtained = new string[obtainedItems.Count + 1];
            obtainedItems.CopyTo(obtained);

            foreach (string str in unobtainedItems)
            {
                if (LogicManager.GetItemDef(str).progression)
                {
                    obtained[obtained.Length - 1] = str;

                    int hypothetical = 0;
                    foreach (string item in unobtainedLocations)
                    {
                        if (LogicManager.ParseLogic(item, obtained))
                        {
                            hypothetical++;
                        }
                    }

                    if (hypothetical > reachableCount)
                    {
                        progression.Add(str);
                    }
                }
            }

            return progression;
        }

        private static string GetAdditivePrefix(string boolName)
        {
            foreach (string itemSet in LogicManager.AdditiveItemNames)
            {
                if (LogicManager.GetAdditiveItems(itemSet).Contains(boolName))
                {
                    return itemSet;
                }
            }

            return null;
        }

        private static BigItemDef[] GetBigItemDefArray(string boolName)
        {
            string prefix = GetAdditivePrefix(boolName);
            if (prefix != null)
            {
                List<BigItemDef> itemDefs = new List<BigItemDef>();
                foreach (string str in LogicManager.GetAdditiveItems(prefix))
                {
                    ReqDef item = LogicManager.GetItemDef(str);
                    itemDefs.Add(new BigItemDef()
                    {
                        BoolName = item.boolName,
                        SpriteKey = item.bigSpriteKey,
                        TakeKey = item.takeKey,
                        NameKey = item.nameKey,
                        ButtonKey = item.buttonKey,
                        DescOneKey = item.descOneKey,
                        DescTwoKey = item.descTwoKey
                    });
                }

                return itemDefs.ToArray();
            }
            else
            {
                ReqDef item = LogicManager.GetItemDef(boolName);
                return new BigItemDef[]
                {
                    new BigItemDef()
                    {
                        BoolName = item.boolName,
                        SpriteKey = item.bigSpriteKey,
                        TakeKey = item.takeKey,
                        NameKey = item.nameKey,
                        ButtonKey = item.buttonKey,
                        DescOneKey = item.descOneKey,
                        DescTwoKey = item.descTwoKey
                    }
                };
            }
        }

        private static string GetAdditiveBoolName(string boolName)
        {
            if (additiveCounts == null)
            {
                additiveCounts = new Dictionary<string, int>();
                foreach (string str in LogicManager.AdditiveItemNames)
                {
                    additiveCounts.Add(str, 0);
                }
            }

            string prefix = GetAdditivePrefix(boolName);
            if (!string.IsNullOrEmpty(prefix))
            {
                additiveCounts[prefix] = additiveCounts[prefix] + 1;
                return prefix + additiveCounts[prefix];
            }

            return null;
        }

        private static void LogItemPlacement(string item, string location)
        {
            RandomizerMod.Instance.Settings.itemPlacements.Add(item, location);
            RandomizerMod.Instance.Log($"Putting item \"{item.Replace('_', ' ')}\" at \"{location.Replace('_', ' ')}\"");
        }
    }
}
