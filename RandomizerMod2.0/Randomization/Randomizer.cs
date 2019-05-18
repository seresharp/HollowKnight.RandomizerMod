using System;
using System.Collections.Generic;
using System.Linq;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Randomization
{
    internal static class Randomizer
    {
        private static Dictionary<string, List<string>> _shopItems;

        private static List<string> _unobtainedLocations;
        private static List<string> _unobtainedItems;
        private static List<string> _obtainedItems;

        public static void Randomize()
        {
            SetupVariables();
            RandomizerMod.Instance.Settings.ResetItemPlacements();

            Log("Randomizing with seed: " + RandomizerMod.Instance.Settings.Seed);
            Log("Mode - " + (RandomizerMod.Instance.Settings.NoClaw ? "No Claw" : "Standard"));
            Log("Shade skips - " + RandomizerMod.Instance.Settings.ShadeSkips);
            Log("Acid skips - " + RandomizerMod.Instance.Settings.AcidSkips);
            Log("Spike tunnel skips - " + RandomizerMod.Instance.Settings.SpikeTunnels);
            Log("Misc skips - " + RandomizerMod.Instance.Settings.MiscSkips);
            Log("Fireball skips - " + RandomizerMod.Instance.Settings.FireballSkips);
            Log("Mag skips - " + RandomizerMod.Instance.Settings.MagSkips);

            Random rand = new Random(RandomizerMod.Instance.Settings.Seed);

            // For use in weighting item placement
            Dictionary<string, int> locationDepths = new Dictionary<string, int>();
            int currentDepth = 1;

            _unobtainedItems.Remove("Wayward_Compass");
            _unobtainedLocations.Remove("Grubberfly's_Elegy");
            LogItemPlacement("Wayward_Compass", "Grubberfly's_Elegy");

            // Early game sucks too much if you don't get any geo, and the fury spot is weird anyway
            // Two birds with one stone
            Log("Placing initial geo pickup");

            string[] furyGeoContenders = _unobtainedItems.Where(item =>
                    LogicManager.GetItemDef(item).type == ItemType.Geo && LogicManager.GetItemDef(item).geo > 100)
                .ToArray();
            string furyGeoItem = furyGeoContenders[rand.Next(furyGeoContenders.Length)];

            _unobtainedItems.Remove(furyGeoItem);
            _unobtainedLocations.Remove("Fury_of_the_Fallen");
            LogItemPlacement(furyGeoItem, "Fury_of_the_Fallen");

            Log("Beginning first pass of progression item placement");

            // Choose where to place progression items
            while (true)
            {
                // Get currently reachable locations
                List<string> reachableLocations = new List<string>();
                string[] obtained = _obtainedItems.ToArray();
                int reachableCount = 0;

                foreach (string loc in _unobtainedLocations)
                {
                    if (!LogicManager.ParseLogic(loc, obtained))
                    {
                        continue;
                    }

                    if (!locationDepths.ContainsKey(loc))
                    {
                        locationDepths[loc] = currentDepth;
                    }

                    // This way further locations will be more likely to be picked
                    for (int j = 0; j < currentDepth; j++)
                    {
                        reachableLocations.Add(loc);
                    }

                    reachableCount++;
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

                _unobtainedLocations.Remove(placeLocation);
                _unobtainedItems.Remove(placeItem);
                _obtainedItems.Add(placeItem);

                LogItemPlacement(placeItem, placeLocation);

                if (_shopItems.ContainsKey(placeLocation))
                {
                    _shopItems[placeLocation].Add(placeItem);
                }

                currentDepth++;
            }

            Log("Beginning second pass of progression item placement");

            // Place remaining potential progression items
            List<string> unusedProgressionItems = new List<string>();

            foreach (string str in _unobtainedItems)
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
                foreach (string str in _unobtainedLocations)
                {
                    // Items tagged as requiring "EVERYTHING" will not be in this dict
                    if (!locationDepths.ContainsKey(str))
                    {
                        continue;
                    }

                    // Using weight^2 to heavily bias towards late locations
                    for (int i = 0; i < locationDepths[str] * locationDepths[str]; i++)
                    {
                        weightedLocations.Add(str);
                    }
                }

                string placeLocation = weightedLocations[rand.Next(weightedLocations.Count)];
                string placeItem = unusedProgressionItems[rand.Next(unusedProgressionItems.Count)];

                _unobtainedLocations.Remove(placeLocation);
                unusedProgressionItems.Remove(placeItem);
                _unobtainedItems.Remove(placeItem);
                _obtainedItems.Add(placeItem);

                LogItemPlacement(placeItem, placeLocation);

                if (_shopItems.ContainsKey(placeLocation))
                {
                    _shopItems[placeLocation].Add(placeItem);
                }
            }

            Log("Beginning placement of good items into remaining shops");

            // Place good items into shops that lack progression items
            List<string> goodItems = new List<string>();
            foreach (string str in _unobtainedItems)
            {
                if (LogicManager.GetItemDef(str).isGoodItem)
                {
                    goodItems.Add(str);
                }
            }

            foreach (string shopName in _shopItems.Keys.ToList())
            {
                if (_shopItems[shopName].Count != 0)
                {
                    continue;
                }

                string placeItem = goodItems[rand.Next(goodItems.Count)];

                _unobtainedItems.Remove(placeItem);
                goodItems.Remove(placeItem);
                _obtainedItems.Add(placeItem);

                LogItemPlacement(placeItem, shopName);

                _shopItems[shopName].Add(placeItem);
                _unobtainedLocations.Remove(shopName);
            }

            // Place geo drops first to guarantee they don't end up in shops
            Log("Beginning placement of geo drops");

            List<string> geoItems = _unobtainedItems.Where(name => LogicManager.GetItemDef(name).type == ItemType.Geo)
                .ToList();
            List<string> geoLocations =
                _unobtainedLocations.Where(name => LogicManager.GetItemDef(name).cost == 0).ToList();
            foreach (string geoItem in geoItems)
            {
                string placeLocation = geoLocations[rand.Next(geoLocations.Count)];

                _unobtainedLocations.Remove(placeLocation);
                geoLocations.Remove(placeLocation);

                _unobtainedItems.Remove(geoItem);
                _obtainedItems.Add(geoItem);

                LogItemPlacement(geoItem, placeLocation);
            }

            Log("Beginning full random placement into remaining locations");

            // Randomly place into remaining locations
            while (_unobtainedLocations.Count > 0)
            {
                string placeLocation = _unobtainedLocations[rand.Next(_unobtainedLocations.Count)];
                string placeItem = _unobtainedItems[rand.Next(_unobtainedItems.Count)];

                _unobtainedLocations.Remove(placeLocation);
                _unobtainedItems.Remove(placeItem);
                _obtainedItems.Add(placeItem);

                LogItemPlacement(placeItem, placeLocation);

                if (_shopItems.ContainsKey(placeLocation))
                {
                    _shopItems[placeLocation].Add(placeItem);
                }
            }

            Log("Beginning placement of leftover items into shops");

            string[] shopNames = _shopItems.Keys.ToArray();

            // Put remaining items in shops
            while (_unobtainedItems.Count > 0)
            {
                string placeLocation = shopNames[rand.Next(shopNames.Length)];
                string placeItem = _unobtainedItems[rand.Next(_unobtainedItems.Count)];

                _unobtainedItems.Remove(placeItem);
                _obtainedItems.Add(placeItem);

                LogItemPlacement(placeItem, placeLocation);

                _shopItems[placeLocation].Add(placeItem);
            }

            Log("Randomization complete");
        }

        private static void SetupVariables()
        {
            _shopItems = new Dictionary<string, List<string>>();
            foreach (string shopName in LogicManager.ShopNames)
            {
                _shopItems.Add(shopName, new List<string>());
            }
            ////shopItems.Add("Lemm", new List<string>()); TODO: Custom shop component to handle lemm

            _unobtainedLocations = new List<string>();
            foreach (string itemName in LogicManager.ItemNames)
            {
                if (LogicManager.GetItemDef(itemName).type != ItemType.Shop)
                {
                    _unobtainedLocations.Add(itemName);
                }
            }

            _unobtainedLocations.AddRange(_shopItems.Keys);
            _unobtainedItems = LogicManager.ItemNames.ToList();
            _obtainedItems = new List<string>();

            // Don't place claw in no claw mode, obviously
            if (RandomizerMod.Instance.Settings.NoClaw)
            {
                _unobtainedItems.Remove("Mantis_Claw");
            }
        }

        private static List<string> GetProgressionItems(int reachableCount)
        {
            List<string> progression = new List<string>();
            string[] obtained = new string[_obtainedItems.Count + 1];
            _obtainedItems.CopyTo(obtained);

            foreach (string str in _unobtainedItems)
            {
                if (!LogicManager.GetItemDef(str).progression)
                {
                    continue;
                }

                obtained[obtained.Length - 1] = str;

                int hypothetical = 0;
                foreach (string item in _unobtainedLocations)
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

            return progression;
        }

        private static void LogItemPlacement(string item, string location)
        {
            RandomizerMod.Instance.Settings.AddItemPlacement(item, location);
            Log(
                $"Putting item \"{item.Replace('_', ' ')}\" at \"{location.Replace('_', ' ')}\"");
        }
    }
}