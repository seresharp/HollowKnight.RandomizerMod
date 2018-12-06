using System;
using System.Reflection;
using HutongGames.PlayMaker;
using UnityEngine;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerAddGeo : FsmStateAction
    {
        private const int GEO_VALUE_LARGE = 25;
        private const int GEO_VALUE_MEDIUM = 5;

        private static FieldInfo smallGeoPrefabField = typeof(HealthManager).GetField("smallGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo mediumGeoPrefabField = typeof(HealthManager).GetField("mediumGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo largeGeoPrefabField = typeof(HealthManager).GetField("largeGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);

        private static GameObject smallGeoPrefab;
        private static GameObject mediumGeoPrefab;
        private static GameObject largeGeoPrefab;

        private GameObject gameObject;
        private int count;

        public RandomizerAddGeo(GameObject baseObj, int amount)
        {
            count = amount;
            gameObject = baseObj;
        }

        public static bool GetGeoPrefabs(GameObject enemy, bool isAlreadyDead)
        {
            if (smallGeoPrefab == null || mediumGeoPrefab == null || largeGeoPrefab == null)
            {
                HealthManager hm = enemy.GetComponent<HealthManager>();

                if (hm != null)
                {
                    smallGeoPrefab = (GameObject)smallGeoPrefabField.GetValue(hm);
                    mediumGeoPrefab = (GameObject)mediumGeoPrefabField.GetValue(hm);
                    largeGeoPrefab = (GameObject)largeGeoPrefabField.GetValue(hm);
                }
            }

            return isAlreadyDead;
        }

        public override void OnEnter()
        {
            if (smallGeoPrefab != null && mediumGeoPrefab != null && largeGeoPrefab != null)
            {
                System.Random random = new System.Random();

                int smallNum = random.Next(0, count / 10);
                count -= smallNum;
                int largeNum = random.Next(count / (GEO_VALUE_LARGE * 2), (count / GEO_VALUE_LARGE) + 1);
                count -= largeNum * GEO_VALUE_LARGE;
                int medNum = count / GEO_VALUE_MEDIUM;
                count -= medNum * 5;
                smallNum += count;

                FlingUtils.Config flingConfig = new FlingUtils.Config()
                {
                    Prefab = smallGeoPrefab,
                    AmountMin = smallNum,
                    AmountMax = smallNum,
                    SpeedMin = 15f,
                    SpeedMax = 30f,
                    AngleMin = 80f,
                    AngleMax = 115f
                };

                FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));

                flingConfig.Prefab = mediumGeoPrefab;
                flingConfig.AmountMin = flingConfig.AmountMax = medNum;
                FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));

                flingConfig.Prefab = largeGeoPrefab;
                flingConfig.AmountMin = flingConfig.AmountMax = largeNum;
                FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));
            }
            else
            {
                HeroController.instance.AddGeo(count);
            }

            Finish();
        }
    }
}
