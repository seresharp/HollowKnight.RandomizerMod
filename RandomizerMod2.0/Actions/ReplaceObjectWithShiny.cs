using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        public override ActionType Type => ActionType.GameObject;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != sceneName)
            {
                return;
            }

            Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject obj = currentScene.FindGameObject(objectName);

            // Put a shiny in the same location as the original
            GameObject shiny = ShinyPrefab;
            shiny.name = newShinyName;
            if (obj.transform.parent != null)
            {
                shiny.transform.SetParent(obj.transform.parent);
            }

            shiny.transform.position = obj.transform.position;
            shiny.transform.localPosition = obj.transform.localPosition;
            shiny.SetActive(obj.activeSelf);

            // Force the new shiny to fall straight downwards
            PlayMakerFSM fsm = FSMUtility.LocateFSM(shiny, "Shiny Control");
            FsmState fling = fsm.GetState("Fling?");
            fling.ClearTransitions();
            fling.AddTransition("FINISHED", "Fling R");
            FlingObject flingObj = fsm.GetState("Fling R").GetActionsOfType<FlingObject>()[0];
            flingObj.angleMin = flingObj.angleMax = 270;

            // For some reason not setting speed manually messes with the object position
            flingObj.speedMin = flingObj.speedMax = 0.1f;

            // Destroy the original
            Object.Destroy(obj);
        }
    }
}
