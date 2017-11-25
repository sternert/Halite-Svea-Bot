using System.Collections.Generic;
using System.Linq;
using Halite2.hlt;

namespace Halite2.Tactics.Defence
{
    public class DefenceTorAttackClosestShip : Tactic
    {
        public IEnumerable<ShipMove> UseShips(GameMap gameMap, IList<Ship> availableShips)
        {
            Log.LogMessage($"Enter Defence");
            var defendMoves = new List<ShipMove>();
            var enemyShips = gameMap.GetAllShips().Where(ship =>
                ship.GetOwner() != gameMap.GetMyPlayerId() &&
                ship.GetDockingStatus() == Ship.DockingStatus.Undocked).ToList();
            var targetEnemyShip = enemyShips.FirstOrDefault();
            var closestDistance = 1e9;

            Log.LogMessage($"Number of enemyShips: {enemyShips.Count}");

            foreach (var enemyShip in enemyShips)
            {
                var enemyDistance = enemyShip.GetDistanceTo(gameMap.OwnedAreaCenter);
                if (enemyDistance < closestDistance)
                {
                    closestDistance = enemyDistance;
                    targetEnemyShip = enemyShip;
                }
            }

            Log.LogMessage($"Number of defenders: {availableShips.Count}");
            Log.LogMessage($"Target (true/false): {targetEnemyShip != null}");
            foreach (var defender in availableShips)
            {
                if (targetEnemyShip != null)
                {
                    var shipMaxSpeed = closestDistance < 30 ? 3 : Constants.MAX_SPEED;
                    Log.LogMessage($"ShipMaxSpeed: {shipMaxSpeed}");
                    var attackMove = Navigation.AttackShip(gameMap, defender, targetEnemyShip, shipMaxSpeed,
                        gameMap.GetMyPlayerId());
                    Log.LogMessage($"AttackMove: {attackMove}");
                    defendMoves.Add(new ShipMove(defender, attackMove));
                    Log.LogMessage($"DefendMoves count: {defendMoves.Count}");
                }
            }
            return defendMoves;
        }
    }
}
