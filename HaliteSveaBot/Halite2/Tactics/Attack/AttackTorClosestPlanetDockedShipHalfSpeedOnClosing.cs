using System.Collections.Generic;
using System.Linq;
using Halite2.hlt;

namespace Halite2.Tactics.Attack
{
    public class AttackTorClosestPlanetDockedShipHalfSpeedOnClosing : Tactic
    {
        public IEnumerable<ShipMove> UseShips(GameMap gameMap, IList<Ship> availableShips)
        {
            var moves = new List<ShipMove>();
            var playerId = gameMap.GetMyPlayer().GetId();

            var targetPlanet = GetTargetPlanet(gameMap);

            if (targetPlanet == null)
                return moves;

            foreach (var ship in availableShips.Take(50))
            {
                Ship targetDockedShip = null;
                var closestDistance = 1e9;
                foreach (var dockedShipId in targetPlanet.GetDockedShips())
                {
                    var dockedShip = gameMap.GetShip(targetPlanet.GetOwner(), dockedShipId);
                    var dockedShipDistance = ship.GetDistanceTo(dockedShip.GetClosestPosition());
                    if (dockedShipDistance < closestDistance)
                    {
                        closestDistance = dockedShipDistance;
                        targetDockedShip = dockedShip;
                    }
                }
                var shipMaxSpeed = closestDistance < 50 ? 3 : Constants.MAX_SPEED;
                var attackMove = Navigation.AttackShip(gameMap, ship, targetDockedShip, shipMaxSpeed, playerId);
                if (attackMove != null)
                    moves.Add(new ShipMove(ship, attackMove));
            }

            return moves;
        }

        private Planet GetTargetPlanet(GameMap gameMap)
        {
            var ownedAreaCenter = gameMap.OwnedAreaCenter;
            var playerId = gameMap.GetMyPlayer().GetId();
            var targetPlanet = gameMap.GetAllOwnedByOthers(playerId).OrderBy(planet => planet.GetDistanceTo(ownedAreaCenter)).FirstOrDefault();
            return targetPlanet;
        }
    }
}
