using System.Collections.Generic;
using Modding;
using UnityEngine;
using static RandomizerMod.LogHelper;

namespace RandomizerMod
{
    internal static class FontManager
    {
        private static Dictionary<string, Font> _fonts;
        private static Font _perpetua;

        public static void LoadFonts()
        {
            CanvasUtil.CreateFonts();

            _fonts = new Dictionary<string, Font>();
            foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (_fonts.ContainsKey(f.name))
                {
                    continue;
                }

                _fonts.Add(f.name, f);

                if (_perpetua == null && f.name == "Perpetua")
                {
                    _perpetua = f;
                }
            }
        }

        public static Font GetFont(string name)
        {
            if (_fonts.TryGetValue(name, out Font font))
            {
                return font;
            }

            LogWarn($"Non-existent font \"{name}\" requested");

            // Default to perpetua if the name doesn't exist
            return _perpetua;
        }
    }
}