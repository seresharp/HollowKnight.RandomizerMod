using HutongGames.PlayMaker;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerSellRelics : FsmStateAction
    {
        public override void OnEnter()
        {
            if (!PlayerData.instance.GetBool("equippedCharm_10"))
            {
                int money = PlayerData.instance.trinket1 * 200;
                money += PlayerData.instance.trinket2 * 450;
                money += PlayerData.instance.trinket3 * 800;
                money += PlayerData.instance.trinket4 * 1200;

                if (money > 0)
                {
                    HeroController.instance.AddGeo(money);
                }

                PlayerData.instance.soldTrinket1 += PlayerData.instance.trinket1;
                PlayerData.instance.soldTrinket2 += PlayerData.instance.trinket2;
                PlayerData.instance.soldTrinket3 += PlayerData.instance.trinket3;
                PlayerData.instance.soldTrinket4 += PlayerData.instance.trinket4;

                PlayerData.instance.trinket1 = 0;
                PlayerData.instance.trinket2 = 0;
                PlayerData.instance.trinket3 = 0;
                PlayerData.instance.trinket4 = 0;
            }

            Finish();
        }
    }
}
