using HutongGames.PlayMaker;

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
            if (!string.IsNullOrEmpty(sceneName) && !string.IsNullOrEmpty(gateName))
            {
                RandomizerMod.Instance.ChangeToScene(sceneName, gateName);
            }

            Finish();
        }
    }
}
