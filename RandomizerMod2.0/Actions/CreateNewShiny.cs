using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using UnityEngine;

using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    internal class CreateNewShiny : RandomizerAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private float x;
        [SerializeField] private float y;
        [SerializeField] private string newShinyName;

        public CreateNewShiny(string sceneName, float x, float y, string newShinyName)
        {
            this.sceneName = sceneName;
            this.x = x;
            this.y = y;
            this.newShinyName = newShinyName;
        }

        public override ActionType Type => ActionType.GameObject;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != sceneName)
            {
                return;
            }

            // Put a shiny in the same location as the original
            GameObject shiny = ObjectCache.ShinyItem;
            shiny.name = newShinyName;

            shiny.transform.position = new Vector3(x, y, shiny.transform.position.z);
            shiny.SetActive(true);

            // Force the new shiny to fall straight downwards
            PlayMakerFSM fsm = FSMUtility.LocateFSM(shiny, "Shiny Control");
            FsmState fling = fsm.GetState("Fling?");
            fling.ClearTransitions();
            fling.AddTransition("FINISHED", "Fling R");
            FlingObject flingObj = fsm.GetState("Fling R").GetActionsOfType<FlingObject>()[0];
            flingObj.angleMin = flingObj.angleMax = 270;

            // For some reason not setting speed manually messes with the object position
            flingObj.speedMin = flingObj.speedMax = 0.1f;
        }
    }
}
