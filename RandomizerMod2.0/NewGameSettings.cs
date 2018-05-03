using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod
{
    internal struct NewGameSettings
    {
        //Randomization variables
        public bool shadeSkips;
        public bool acidSkips;
        public bool spikeTunnels;
        public bool miscSkips;
        public bool magolorSkips;

        //Additional restrictions
        public bool allBosses;
        public bool allSkills;
        public bool allCharms;

        //Quality of life
        public bool charmNotch;
        public bool lemm;

        public void SetDefaults()
        {
            this = default(NewGameSettings);
            charmNotch = true;
            lemm = true;
        }

        public void SetEasy()
        {
            shadeSkips = false;
            acidSkips = false;
            spikeTunnels = false;
            miscSkips = false;
            magolorSkips = false;
        }

        public void SetHard()
        {
            shadeSkips = true;
            acidSkips = true;
            spikeTunnels = true;
            miscSkips = true;
            magolorSkips = false;
        }

        public void SetMagolor()
        {
            shadeSkips = true;
            acidSkips = true;
            spikeTunnels = true;
            miscSkips = true;
            magolorSkips = true;
        }
    }
}
