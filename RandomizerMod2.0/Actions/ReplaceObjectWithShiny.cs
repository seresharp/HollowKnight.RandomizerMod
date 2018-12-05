using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Components;
using RandomizerMod.Extensions;

using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    [Serializable]
    public class ReplaceObjectWithShiny : RandomizerAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string newShinyName;

        public ReplaceObjectWithShiny(string sceneName, string objectName, string newShinyName)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.newShinyName = newShinyName;
        }

        public override void Process()
        {
            if (GameManager.instance.GetSceneNameString() == sceneName)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                GameObject obj = scene.FindGameObject(objectName);

                if (obj != null)
                {
                    //Put a shiny in the same location as the original
                    GameObject shiny = Object.Instantiate(shinyPrefab);
                    shiny.name = newShinyName;
                    if (obj.transform.parent != null) shiny.transform.SetParent(obj.transform.parent);
                    shiny.transform.position = obj.transform.position;
                    shiny.transform.localPosition = obj.transform.localPosition;
                    shiny.SetActive(obj.activeSelf);

                    //Force the new shiny to fall straight downwards
                    PlayMakerFSM fsm = FSMUtility.LocateFSM(shiny, "Shiny Control");
                    FsmState fling = fsm.GetState("Fling?");
                    fling.ClearTransitions();
                    fling.AddTransition("FINISHED", "Fling R");
                    FlingObject flingObj = fsm.GetState("Fling R").GetActionsOfType<FlingObject>()[0];
                    flingObj.angleMin = flingObj.angleMax = 270;

                    //For some reason not setting speed manually messes with the object position
                    flingObj.speedMin = flingObj.speedMax = 0.1f;

                    //Gotta put our new shiny into the fsm list
                    fsmList.Add(fsm);

                    //Destroy the original
                    Object.Destroy(obj);
                }
                else throw new ArgumentException($"Could not find object {objectName} in scene {scene.name}");
            }
        }
    }
}
