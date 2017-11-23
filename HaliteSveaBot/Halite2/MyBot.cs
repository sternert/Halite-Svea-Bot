using Halite2.hlt;
using System.Collections.Generic;
using System.Linq;

namespace Halite2
{
    public class MyBot
    {
        private static List<Move> _moveList;
        private static GameMap _gameMap;
        private static Networking _networking;

        private static IEnumerable<Move> GetAllMoves()
        {
            var player = _gameMap.GetMyPlayer();
            var ships = player.GetShips().Values;
            var availableShips = ships.Where(ship => ship.GetDockingStatus() == Ship.DockingStatus.Undocked)
                .ToList();

            var notOwnedPlanets = _gameMap.GetAllNotFullNotOwnedByUsPlanets(player.GetId());
            var targetPlanets = notOwnedPlanets.ToList();

            var moveList = new List<Move>();

            for (int i = 0; i < notOwnedPlanets.Count; i++)
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

                var bestGlobalPlanetShipDistance = planetShipDistances.OrderBy(planetShipDistance => planetShipDistance.Distance).First();

                bestGlobalPlanetShipDistance.Planet.BookedCount++;
                if (bestGlobalPlanetShipDistance.Planet.IsFullyBooked())
                {
                    targetPlanets.Remove(bestGlobalPlanetShipDistance.Planet);
                }
                availableShips.Remove(bestGlobalPlanetShipDistance.Ship);

                var move = MoveOrDockShipToPlanet(bestGlobalPlanetShipDistance);
                if (move != null)
                {
                    moveList.Add(move);
                }

            }

            return moveList;
        }

        public static void Main(string[] args)
        {
            string name = args.Length > 0 ? args[0] : "Grasshopper1";

            _networking = new Networking();
            _gameMap = _networking.Initialize(name);

            _moveList = new List<Move>();
            for (; ; )
            {
                _moveList.Clear();
                _gameMap.UpdateMap(Networking.ReadLineIntoMetadata());

                //var player = _gameMap.GetMyPlayer();
                //var ships = player.GetShips().Values;
                //var availableShips = ships.Where(ship => ship.GetDockingStatus() == Ship.DockingStatus.Undocked)
                //    .ToList();

                //var notOwnedPlanets = _gameMap.GetAllNotFullNotOwnedByUsPlanets(player.GetId());
                //var targetPlanets = notOwnedPlanets.ToList();

                //for (int i = 0; i < notOwnedPlanets.Count; i++)
                //{
                //    if (!availableShips.Any())
                //    {
                //        break;
                //    }
                //    var planetShipDistances = new List<PlanetShipDistance>();

                //    foreach (var targetPlanet in targetPlanets)
                //    {
                //        var bestDistanceTo = 1e9;
                //        PlanetShipDistance bestPlanetShipDistance = null;

                //        foreach (var availableShip in availableShips)
                //        {
                //            var distance = availableShip.GetDistanceTo(targetPlanet.GetCenterPosition()) -
                //                           targetPlanet.GetRadius();
                //            if (distance < bestDistanceTo)
                //            {
                //                bestPlanetShipDistance = new PlanetShipDistance
                //                {
                //                    Distance = distance,
                //                    Planet = targetPlanet,
                //                    Ship = availableShip
                //                };
                //                bestDistanceTo = distance;
                //            }
                //        }
                //        planetShipDistances.Add(bestPlanetShipDistance);
                //    }

                //    var bestGlobalPlanetShipDistance = planetShipDistances.OrderBy(planetShipDistance => planetShipDistance.Distance).First();

                //    bestGlobalPlanetShipDistance.Planet.BookedCount++;
                //    if (bestGlobalPlanetShipDistance.Planet.IsFullyBooked())
                //    {
                //        targetPlanets.Remove(bestGlobalPlanetShipDistance.Planet);
                //    }
                //    availableShips.Remove(bestGlobalPlanetShipDistance.Ship);

                //    var move = MoveOrDockShipToPlanet(bestGlobalPlanetShipDistance);
                //    if (move != null)
                //    {
                //        _moveList.Add(move);
                //    }
                //}
                Networking.SendMoves(_moveList);
            }
        }

        private static Move MoveOrDockShipToPlanet(PlanetShipDistance bestPlanetShipDistance)
        {
            var ship = bestPlanetShipDistance.Ship;
            var planet = bestPlanetShipDistance.Planet;

            if (ship.CanDock(planet))
            {
                return new DockMove(ship, planet);
            }

            ThrustMove newThrustMove = Navigation.NavigateShipToDock(_gameMap, ship, planet, Constants.MAX_SPEED);
            
            return newThrustMove;
        }
    }

    public class PlanetShipDistance
    {
        public Planet Planet { get; set; }
        public Ship Ship { get; set; }
        public double Distance { get; set; }
    }
}
