using System.Collections.Generic;
using Halite2.hlt;

namespace Halite2.Tactics
{
    public interface Tactic
    {
        IEnumerable<ShipMove> UseShips(GameMap gameMap, IList<Ship> availableShips);
    }

    public class ShipMove
    {
        public ShipMove(Ship ship, Move move)
        {
            this.ship = ship;
            this.move = move;
        }
        public Ship ship { get; set; }
        public Move move { get; set; }
    }
}
