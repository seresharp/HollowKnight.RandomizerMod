using System.Collections.Generic;
using System.Reflection;
using Modding;
using SeanprCore;
using UnityEngine;

namespace RandomizerLib
{
    public class RandomizerLib : Mod
    {
        private static bool _initialized;
        private static Dictionary<string, Sprite> _sprites;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloaded)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            // Cache objects
            ObjectCache.GetPrefabs(preloaded[SceneNames.Tutorial_01]);

            // Load embedded resources
            _sprites = ResourceHelper.GetSprites("RandomizerLib.Resources.");

            // Parse XML files
            Assembly asm = GetType().Assembly;
            LogicManager.ParseXML(asm.GetManifestResourceStream("RandomizerLib.Resources.items.xml"));
            LanguageStringManager.LoadLanguageXML(
                asm.GetManifestResourceStream("RandomizerLib.Resources.language.xml"));
        }

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                (SceneNames.Tutorial_01, "_Props/Chest/Item/Shiny Item (1)"),
                (SceneNames.Tutorial_01, "_Enemies/Crawler 1"),
                (SceneNames.Tutorial_01, "_Props/Cave Spikes (1)")
            };
        }

        public static Sprite GetSprite(string spriteName)
        {
            if (_sprites != null && _sprites.TryGetValue(spriteName, out Sprite sprite))
            {
                return sprite;
            }

            return null;
        }
    }
}
