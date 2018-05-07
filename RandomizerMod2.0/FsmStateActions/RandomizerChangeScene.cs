using HutongGames.PlayMaker;
using GlobalEnums;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerChangeScene : FsmStateAction
    {
        private string sceneName;
        private string gateName;

        public RandomizerChangeScene(string scene, string gate)
        {
            sceneName = scene;
            gateName = gate;
        }

        public override void OnEnter()
        {
            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo()
            {
                SceneName = sceneName,
                EntryGateName = gateName,
                HeroLeaveDirection = GetGatePosition(gateName),
                EntryDelay = 0,
                WaitForSceneTransitionCameraFade = true,
                Visualization = GameManager.SceneLoadVisualizations.Default,
                AlwaysUnloadUnusedAssets = false
            });

            Finish();
        }

        private GatePosition GetGatePosition(string name)
        {
            if (name.Contains("top")) return GatePosition.top;
            if (name.Contains("bot")) return GatePosition.bottom;
            if (name.Contains("left")) return GatePosition.left;
            if (name.Contains("right")) return GatePosition.right;
            if (name.Contains("door")) return GatePosition.door;

            return GatePosition.unknown;
        }
    }
}
