using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;
using UnityEngine;

namespace RandomizerMod.Actions
{
    public class ChangeShinyIntoGeo : RandomizerAction
    {
        private readonly string _boolName;
        private readonly string _fsmName;
        private readonly int _geoAmount;
        private readonly string _objectName;
        private readonly string _sceneName;

        public ChangeShinyIntoGeo(string sceneName, string objectName, string fsmName, string boolName, int geoAmount)
        {
            _sceneName = sceneName;
            _objectName = objectName;
            _fsmName = fsmName;
            _boolName = boolName;
            _geoAmount = geoAmount;
        }

        public override ActionType Type => ActionType.PlayMakerFSM;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != _sceneName || !(changeObj is PlayMakerFSM fsm) || fsm.FsmName != _fsmName ||
                fsm.gameObject.name != _objectName)
            {
                return;
            }

            FsmState pdBool = fsm.GetState("PD Bool?");
            FsmState charm = fsm.GetState("Charm?");

            // Remove actions that stop shiny from spawning
            pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
            pdBool.RemoveActionsOfType<StringCompare>();

            // Add our own check to stop the shiny from being grabbed twice
            pdBool.AddAction(new RandomizerBoolTest(_boolName, null, "COLLECTED"));

            // The "Charm?" state is a good entry point for our geo spawning
            charm.AddAction(new RandomizerSetBool(_boolName, true));
            charm.AddAction(new RandomizerAddGeo(fsm.gameObject, _geoAmount));

            // Skip all the other type checks
            charm.ClearTransitions();
            charm.AddTransition("FINISHED", "Flash");
        }
    }
}