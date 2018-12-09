using System.Reflection;
using HutongGames.PlayMaker;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerChangeScene : FsmStateAction
    {
        private static FieldInfo sceneLoad = typeof(GameManager).GetField("sceneLoad", BindingFlags.NonPublic | BindingFlags.Instance);

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
                ChangeToScene(sceneName, gateName);
            }

            Finish();
        }

        private void ChangeToScene(string sceneName, string gateName, float delay = 0f)
        {
            if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(gateName))
            {
                Log("Empty string passed into ChangeToScene, ignoring");
                return;
            }

            void loadScene()
            {
                GameManager.instance.StopAllCoroutines();
                sceneLoad.SetValue(GameManager.instance, null);

                GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo()
                {
                    IsFirstLevelForPlayer = false,
                    SceneName = sceneName,
                    HeroLeaveDirection = GetGatePosition(gateName),
                    EntryGateName = gateName,
                    EntryDelay = delay,
                    PreventCameraFadeOut = false,
                    WaitForSceneTransitionCameraFade = true,
                    Visualization = GameManager.SceneLoadVisualizations.Default,
                    AlwaysUnloadUnusedAssets = false
                });
            }

            SceneLoad load = (SceneLoad)sceneLoad.GetValue(GameManager.instance);
            if (load != null)
            {
                load.Finish += loadScene;
            }
            else
            {
                loadScene();
            }
        }

        private GlobalEnums.GatePosition GetGatePosition(string name)
        {
            if (name.Contains("top"))
            {
                return GlobalEnums.GatePosition.top;
            }

            if (name.Contains("bot"))
            {
                return GlobalEnums.GatePosition.bottom;
            }

            if (name.Contains("left"))
            {
                return GlobalEnums.GatePosition.left;
            }

            if (name.Contains("right"))
            {
                return GlobalEnums.GatePosition.right;
            }

            if (name.Contains("door"))
            {
                return GlobalEnums.GatePosition.door;
            }

            return GlobalEnums.GatePosition.unknown;
        }
    }
}
