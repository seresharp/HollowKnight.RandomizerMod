using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker.Actions;

namespace RandomizerMod
{
    internal static class BenchHandler
    {
        public static void HandleBenchSave(On.PlayerData.orig_SetBenchRespawn_RespawnMarker_string_int orig, PlayerData self, RespawnMarker spawnMarker, string sceneName, int spawnType)
        {
            if (CanSaveInRoom(sceneName))
            {
                orig(self, spawnMarker, sceneName, spawnType);
            }
        }

        public static void HandleBenchSave(On.PlayerData.orig_SetBenchRespawn_string_string_bool orig, PlayerData self, string spawnMarker, string sceneName, bool facingRight)
        {
            if (CanSaveInRoom(sceneName))
            {
                orig(self, spawnMarker, sceneName, facingRight);
            }
        }

        public static void HandleBenchSave(On.PlayerData.orig_SetBenchRespawn_string_string_int_bool orig, PlayerData self, string spawnMarker, string sceneName, int spawnType, bool facingRight)
        {
            if (CanSaveInRoom(sceneName))
            {
                orig(self, spawnMarker, sceneName, spawnType, facingRight);
            }
        }

        public static void HandleBenchBoolTest(On.HutongGames.PlayMaker.Actions.BoolTest.orig_OnEnter orig, BoolTest self)
        {
            if (self.State?.Name == "Rest Burst" && self.boolVariable?.Name == "Set Respawn")
            {
                self.boolVariable.Value = CanSaveInRoom(GameManager.instance.GetSceneNameString());
            }

            orig(self);
        }

        private static bool CanSaveInRoom(string sceneName)
        {
            PlayerData pd = PlayerData.instance;

            RandomizerMod.instance.Log("\"" + sceneName + "\"" + " " + (sceneName == "Room_Slug_Shrine"));

            switch (sceneName)
            {
                case "Abyss_18": // Basin bench
                case "GG_Waterways": // Godhome
                case "Room_Colosseum_02": // Colo bench
                    return pd.hasWalljump;
                case "Room_Slug_Shrine": // Unn bench
                    return pd.hasDash || pd.hasDoubleJump || (pd.hasAcidArmour && pd.hasWalljump);
                case "Waterways_02": // Waterways bench
                    return pd.hasWalljump || pd.hasDoubleJump;
                default:
                    return true;
            }
        }
    }
}
