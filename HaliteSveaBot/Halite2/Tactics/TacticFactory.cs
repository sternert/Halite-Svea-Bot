using Halite2.Tactics.Attack;
using Halite2.Tactics.Defence;
using Halite2.Tactics.Expansion;

namespace Halite2.Tactics
{
    public enum Tactics
    {
        ExpandGrasshopper,
        DefenceStaffanCrazy,
        DefenceTorAttackClosestShip,
        AttackTorClosestDocked
    }

    public static class TacticFactory
    {
        public static Tactic GetTactic(Tactics tactic)
        {
            switch (tactic)
            {
                case Tactics.ExpandGrasshopper:
                    return new ExpandGrasshopper();
                case Tactics.DefenceStaffanCrazy:
                    return new DefenceStaffanStupidCrazy();
                case Tactics.AttackTorClosestDocked:
                    return new AttackTorClosestPlanetDockedShipHalfSpeedOnClosing();
                case Tactics.DefenceTorAttackClosestShip:
                    return new DefenceTorAttackClosestShip();
            }
            return null;
        }
    }
}
