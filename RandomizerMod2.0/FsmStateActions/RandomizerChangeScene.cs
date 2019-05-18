using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker;
using SeanprCore;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerChangeScene : FsmStateAction
    {
        private static readonly FieldInfo sceneLoad =
            typeof(GameManager).GetField("sceneLoad", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly string gateName;

        private readonly string sceneName;

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
                Ref.GM.StopAllCoroutines();
                sceneLoad.SetValue(Ref.GM, null);

                Ref.GM.BeginSceneTransition(new GameManager.SceneLoadInfo
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

            SceneLoad load = (SceneLoad) sceneLoad.GetValue(Ref.GM);
            if (load != null)
            {
                load.Finish += loadScene;
            }
            else
            {
                loadScene();
            }
        }

        private GatePosition GetGatePosition(string name)
        {
            if (name.Contains("top"))
            {
                return GatePosition.top;
            }

            if (name.Contains("bot"))
            {
                return GatePosition.bottom;
            }

            if (name.Contains("left"))
            {
                return GatePosition.left;
            }

            if (name.Contains("right"))
            {
                return GatePosition.right;
            }

            if (name.Contains("door"))
            {
                return GatePosition.door;
            }

            return GatePosition.unknown;
        }
    }
}