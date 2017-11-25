using System.Collections.Generic;
using System.Linq;
using Halite2.hlt;

namespace Halite2.Tactics.Defence
{
    public class DefenceStaffanStupidCrazy : Tactic
    {
        public IEnumerable<ShipMove> UseShips(GameMap gameMap, IList<Ship> availableShips)
        {
            var defendMoves = new List<ShipMove>();
            foreach (var defender in availableShips)
            {
                defendMoves.Add(new ShipMove(defender, null));
            }
            return defendMoves;
        }
    }
}
