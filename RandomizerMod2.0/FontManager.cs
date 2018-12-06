using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;

namespace RandomizerMod
{
    internal static class FontManager
    {
        private static Dictionary<string, Font> fonts;
        private static Font perpetua;

        public static void LoadFonts()
        {
            CanvasUtil.CreateFonts();

            fonts = new Dictionary<string, Font>();
            foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (!fonts.ContainsKey(f.name))
                {
                    fonts.Add(f.name, f);

                    if (perpetua == null && f.name == "Perpetua")
                    {
                        perpetua = f;
                    }
                }
            }
        }

        public static Font GetFont(string name)
        {
            if (fonts.TryGetValue(name, out Font font))
            {
                return font;
            }

            RandomizerMod.Instance.LogWarn($"Non-existent font \"{name}\" requested");

            // Default to perpetua if the name doesn't exist
            return perpetua;
        }
    }
}
