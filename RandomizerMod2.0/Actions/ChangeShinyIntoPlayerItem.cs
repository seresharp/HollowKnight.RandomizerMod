using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.FsmStateActions;
using SeanprCore;
using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    public class ChangeShinyIntoPlayerItem : RandomizerAction
    {
        public delegate void ItemReceived(string location);

        public static event ItemReceived OnItemReceived;

        private readonly string _location;
        private readonly string _sceneName;
        private readonly string _objectName;
        private readonly string _fsmName;

        public ChangeShinyIntoPlayerItem(string location, string sceneName, string objectName, string fsmName)
        {
            _location = location;
            _sceneName = sceneName;
            _objectName = objectName;
            _fsmName = fsmName;
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
            FsmState bigGetFlash = fsm.GetState("Big Get Flash");

            // Remove actions that stop shiny from spawning
            pdBool.RemoveActionsOfType<StringCompare>();

            // Force the FSM to show the big item flash
            charm.ClearTransitions();
            charm.AddTransition("FINISHED", "Big Get Flash");

            // Set bool and show the popup after the flash
            bigGetFlash.AddAction(new RandomizerExecuteLambda(() => OnItemReceived?.Invoke(_location)));

            // Exit the fsm after the popup
            bigGetFlash.ClearTransitions();
            bigGetFlash.AddTransition("FINISHED", "Hero Up");
            bigGetFlash.AddTransition("HERO DAMAGED", "Finish");
        }
    }
}
