using HutongGames.PlayMaker;
using SeanprCore;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerSellRelics : FsmStateAction
    {
        public override void OnEnter()
        {
            if (!Ref.PD.GetBool("equippedCharm_10"))
            {
                int money = Ref.PD.trinket1 * 200;
                money += Ref.PD.trinket2 * 450;
                money += Ref.PD.trinket3 * 800;
                money += Ref.PD.trinket4 * 1200;

                if (money > 0)
                {
                    Ref.Hero.AddGeo(money);
                }

                Ref.PD.soldTrinket1 += Ref.PD.trinket1;
                Ref.PD.soldTrinket2 += Ref.PD.trinket2;
                Ref.PD.soldTrinket3 += Ref.PD.trinket3;
                Ref.PD.soldTrinket4 += Ref.PD.trinket4;

                Ref.PD.trinket1 = 0;
                Ref.PD.trinket2 = 0;
                Ref.PD.trinket3 = 0;
                Ref.PD.trinket4 = 0;
            }

            Finish();
        }
    }
}