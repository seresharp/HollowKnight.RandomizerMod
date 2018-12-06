using System;
using System.Collections.Generic;
using System.Reflection;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    [Serializable]
    public abstract class RandomizerAction
    {
        private static List<PlayMakerFSM> fsmList;
        private static GameObject shinyPrefab;

        private static FieldInfo fsmStartState = typeof(Fsm).GetField("startState", BindingFlags.NonPublic | BindingFlags.Instance);

        protected static PlayMakerFSM[] FsmList => fsmList.ToArray();

        protected static GameObject ShinyPrefab => Object.Instantiate(shinyPrefab);
        
        // Always call before processing
        // PlayMakerFSM.FsmList does not contain inactive FSMs
        public static void FetchFSMList(Scene scene)
        {
            fsmList = new List<PlayMakerFSM>();

            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                foreach (PlayMakerFSM fsm in obj.GetComponentsInChildren<PlayMakerFSM>(true))
                {
                    AddToFsmList(fsm);
                }
            }
        }

        public static void SetShinyPrefab(GameObject obj)
        {
            shinyPrefab = Object.Instantiate(obj);
            shinyPrefab.SetActive(false);
            shinyPrefab.name = "Randomizer Shiny";
            Object.DontDestroyOnLoad(shinyPrefab);
        }

        public static void AddToFsmList(PlayMakerFSM fsm)
        {
            if (fsm != null && fsmList != null && !fsmList.Contains(fsm))
            {
                fsmList.Add(fsm);
            }
        }

        public abstract void Process();
    }
}
