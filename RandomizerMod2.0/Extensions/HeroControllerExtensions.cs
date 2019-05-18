namespace RandomizerMod.Extensions
{
    internal static class HeroControllerExtensions
    {
        public static float GetRunSpeed(this HeroController self)
        {
            // Sprintmaster and dashmaster
            if (self.playerData.equippedCharm_37 && self.playerData.equippedCharm_31)
            {
                return self.RUN_SPEED_CH_COMBO;
            }

            // Sprintmaster
            if (self.playerData.equippedCharm_37)
            {
                return self.RUN_SPEED_CH;
            }

            return self.RUN_SPEED;
        }

        public static string GetRunAnimName(this HeroController self)
        {
            // Sprintmaster
            if (self.playerData.equippedCharm_37)
            {
                return "Sprint";
            }

            return "Run";
        }
    }
}