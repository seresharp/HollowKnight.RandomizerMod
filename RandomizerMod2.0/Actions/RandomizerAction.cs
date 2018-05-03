using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    [Serializable]
    public abstract class RandomizerAction
    {
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
                    if (fsm.FsmName == "Shiny Control")
                    {
                        shinyPrefab = Object.Instantiate(fsm.gameObject);
                        shinyPrefab.SetActive(false);
                        Modding.Logger.Log(shinyPrefab.name);
                        shinyPrefab.name = "Randomizer Shiny";
                        Object.DontDestroyOnLoad(shinyPrefab);
                        break;
                    }
                }
            }
        }
    }
}
