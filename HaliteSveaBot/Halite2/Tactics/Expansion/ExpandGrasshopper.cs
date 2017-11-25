using System.Collections.Generic;
using System.Linq;
using Halite2.hlt;

namespace Halite2.Tactics.Expansion
{
    public class ExpandGrasshopper : Tactic
    {
        public IEnumerable<ShipMove> UseShips(GameMap gameMap, IList<Ship> availableShips)
        {
            var notOwnedPlanets = gameMap.GetAllNotFullNotOwnedByOtherPlanets(gameMap.GetMyPlayer().GetId());
            var targetPlanets = notOwnedPlanets.ToList();

            var shipMoveList = new List<ShipMove>();

            for (int i = 0; i < notOwnedPlanets.Count(); i++)
            {
                if (!availableShips.Any())
                {
                    break;
                }
                var planetShipDistances = new List<PlanetShipDistance>();

                foreach (var targetPlanet in targetPlanets)
                {
                    var bestDistanceTo = 1e9;
                    PlanetShipDistance bestPlanetShipDistance = null;

                    foreach (var availableShip in availableShips)
                    {
                        var distance = availableShip.GetDistanceTo(targetPlanet.GetCenterPosition()) -
                                       targetPlanet.GetRadius();
                        if (distance < bestDistanceTo)
                        {
                            bestPlanetShipDistance = new PlanetShipDistance
                            {
                                Distance = distance,
                                Planet = targetPlanet,
                                Ship = availableShip
                            };
                            bestDistanceTo = distance;
                        }
                    }
                    planetShipDistances.Add(bestPlanetShipDistance);
                }

                var bestGlobalPlanetShipDistance = planetShipDistances
                    //.OrderBy(planetShipDistance => planetShipDistance.Planet.GetId() < 4 ? 1 : 2)
                    .OrderBy(planetShipDistance => planetShipDistance.Distance).First();

                bestGlobalPlanetShipDistance.Planet.BookedCount++;
                if (bestGlobalPlanetShipDistance.Planet.IsFullyBooked())
                {
                    targetPlanets.Remove(bestGlobalPlanetShipDistance.Planet);
                }
                availableShips.Remove(bestGlobalPlanetShipDistance.Ship);

                var move = MoveOrDockShipToPlanet(gameMap, bestGlobalPlanetShipDistance);
                if (move != null)
                {
                    shipMoveList.Add(new ShipMove(bestGlobalPlanetShipDistance.Ship, move));
                }
            }

            return shipMoveList;
        }

        private Move MoveOrDockShipToPlanet(GameMap gameMap, PlanetShipDistance bestPlanetShipDistance)
        {
            var ship = bestPlanetShipDistance.Ship;
            var planet = bestPlanetShipDistance.Planet;

            if (ship.CanDock(planet))
            {
                return new DockMove(ship, planet);
            }

            ThrustMove newThrustMove = Navigation.NavigateShipToDock(gameMap, ship, planet, Constants.MAX_SPEED, gameMap.GetMyPlayerId());

            return newThrustMove;
        }

        public class PlanetShipDistance
        {
            public Planet Planet { get; set; }
            public Ship Ship { get; set; }
            public double Distance { get; set; }
        }
    }
}
