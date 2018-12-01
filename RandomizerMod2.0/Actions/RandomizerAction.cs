using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker;

using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    [Serializable]
    public abstract class RandomizerAction
    {
        private static FieldInfo fsmStartState = typeof(Fsm).GetField("startState", BindingFlags.NonPublic | BindingFlags.Instance);

        protected static List<PlayMakerFSM> fsmList;
        protected static GameObject shinyPrefab;

        public abstract void Process();
        
        //Always call before processing
        //PlayMakerFSM.FsmList does not contain inactive FSMs
        public static void FetchFSMList(Scene scene)
        {
            fsmList = new List<PlayMakerFSM>();
            scene.GetRootGameObjects().ToList().ForEach(obj => fsmList.AddRange(obj.GetComponentsInChildren<PlayMakerFSM>(true)));
            
            if (shinyPrefab == null)
            {
                foreach (PlayMakerFSM fsm in fsmList)
                {
                    if (fsm.FsmName == "Shiny Control" && (fsm.gameObject.name == "Shiny Item" || fsm.gameObject.name == "Shiny Item (1)") && fsm.gameObject.GetComponent<Rigidbody2D>() != null)
                    {
                        SetShinyPrefab(fsm.gameObject);
                        break;
                    }
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

        protected static void ResetFSM(PlayMakerFSM fsm)
        {
            fsm.SetState((string)fsmStartState.GetValue(fsm.Fsm));
        }
    }
}
