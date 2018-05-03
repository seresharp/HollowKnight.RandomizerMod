using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modding;
using UnityEngine;
using RandomizerMod.Actions;

namespace RandomizerMod
{
    public class SaveSettings : IModSettings, ISerializationCallbackReceiver
    {
        public bool allBosses
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool allSkills
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool allCharms
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool charmNotch
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        public bool lemm
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        private static Type[] types;

        public List<RandomizerAction> actions = new List<RandomizerAction>();

        //Serialize actions list into string dict because Unity serializer can't handle inheritance
        public void OnBeforeSerialize()
        {
            for (int i = 0; i < actions.Count; i++)
            {
                string json = JsonUtility.ToJson(actions[i]);
                StringValues.Add($"RandomizerAction:{i}:{actions[i].GetType()}", json);
            }
        }

        //Load the actions back into their list
        public void OnAfterDeserialize()
        {
            if (types == null) types = Assembly.GetAssembly(typeof(RandomizerAction)).GetTypes().Where(t => t.IsSubclassOf(typeof(RandomizerAction))).ToArray();
            Dictionary<int, RandomizerAction> dict = new Dictionary<int, RandomizerAction>();

            //Load the actions into a dict with numbers as keys
            foreach (string key in StringValues.Keys.ToList())
            {
                if (key.StartsWith("RandomizerAction"))
                {
                    string type = key.Split(':')[2];
                    int num = Convert.ToInt32(key.Split(':')[1]);
                    foreach (Type t in types)
                    {
                        if (type == t.ToString())
                        {
                            dict.Add(num, (RandomizerAction)JsonUtility.FromJson(StringValues[key], t));
                            break;
                        }
                    }
                    StringValues.Remove(key);
                }
            }

            //Put them back into the list in order
            //This should be unnecessary in theory but I was having issues with order
            for (int i = 0; i < dict.Count; i++)
            {
                actions.Add(dict[i]);
            }
        }
    }
}
