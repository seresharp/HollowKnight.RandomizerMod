using HutongGames.PlayMaker.Actions;
using RandomizerMod.FsmStateActions;
using SeanprCore;
using UnityEngine;

namespace RandomizerMod.Actions
{
    internal class ChangeBoolTest : RandomizerAction
    {
        private readonly string _boolName;
        private readonly string _fsmName;
        private readonly string _objectName;
        private readonly bool _playerdata;
        private readonly string _sceneName;
        private readonly string _stateName;

        public ChangeBoolTest(string sceneName, string objectName, string fsmName, string stateName, string boolName,
            bool playerdata = false)
        {
            _sceneName = sceneName;
            _objectName = objectName;
            _fsmName = fsmName;
            _stateName = stateName;
            _boolName = boolName;
            _playerdata = playerdata;
        }

        public override ActionType Type => ActionType.PlayMakerFSM;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != _sceneName || !(changeObj is PlayMakerFSM fsm) || fsm.FsmName != _fsmName ||
                fsm.gameObject.name != _objectName)
            {
                return;
            }

            PlayerDataBoolTest pdBoolTest = fsm.GetState(_stateName).GetActionsOfType<PlayerDataBoolTest>()[0];

            if (_playerdata)
            {
                pdBoolTest.boolName = _boolName;
            }
            else
            {
                RandomizerBoolTest boolTest = new RandomizerBoolTest(_boolName, pdBoolTest.isFalse, pdBoolTest.isTrue);
                fsm.GetState(_stateName).RemoveActionsOfType<PlayerDataBoolTest>();
                fsm.GetState(_stateName).AddFirstAction(boolTest);
            }
        }
    }
}