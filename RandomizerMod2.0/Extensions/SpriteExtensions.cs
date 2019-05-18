using UnityEngine;

namespace RandomizerMod.Extensions
{
    internal static class SpriteExtensions
    {
        public static Vector2 Size(this Sprite self)
        {
            return new Vector2(self.texture.width, self.texture.height);
        }
    }
}