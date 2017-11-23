using Halite2.hlt;
using System.Collections.Generic;
using System.Linq;

namespace Halite2
{
    public class MyBot
    {
        private static GameMap _gameMap;
        private static Networking _networking;

        public static void Main(string[] args)
        {
            string name = args.Length > 0 ? args[0] : "Grasshopper1";

            _networking = new Networking();
            _gameMap = _networking.Initialize(name);

            var myBot = new MyBot();

            for (; ; )
            {
                _gameMap.UpdateMap(Networking.ReadLineIntoMetadata());

                var moves = myBot.GetAllMoves();

                Networking.SendMoves(moves);
            }
        }

        private IEnumerable<Move> GetAllMoves()
        {
            var player = _gameMap.GetMyPlayer();
            var ships = player.GetShips().Values;
            var availableShips = ships.Where(ship => ship.GetDockingStatus() == Ship.DockingStatus.Undocked)
                .ToList();

            var explorerMoves = Explore(availableShips);
            //var defenseMoves = StaffansStupidDefenseStrategy(availableShips);
            var attackMoves = StaffanCrazyStrategy(availableShips);


            return explorerMoves.Concat(attackMoves); //Concat(defenseMoves).Concat(attackMoves);
        }

        private List<Move> StaffansStupidDefenseStrategy(List<Ship> availableShips)
        {
            var defenders = availableShips.OrderByDescending(x => x.GetId()).Take(availableShips.Count / 2);
            foreach (var defender in defenders)
                availableShips.Remove(defender);
            return new List<Move>();
        }

        private Planet GetTargetPlanet()
        {
            var playerId = _gameMap.GetMyPlayer().GetId();
            var targetPlanet = _gameMap.GetAllOwnedByOthers(playerId).OrderBy(x => x.GetHealth()).FirstOrDefault();

            return targetPlanet;
        }

        private IEnumerable<Move> StaffanCrazyStrategy(List<Ship> availableShips)
        {
            var moves = new List<Move>();
            var playerId = _gameMap.GetMyPlayer().GetId();
            if (availableShips.Count < 10)
                return moves;

            var targetPlanet = GetTargetPlanet();
            if (targetPlanet == null)
                return moves;

            foreach (var ship in availableShips.Take(35))
            {
                var crashMove = Navigation.CrashInPlanet(_gameMap, ship, targetPlanet, Constants.MAX_SPEED, playerId);
                if (crashMove != null)
                    moves.Add(crashMove);
            }

            return moves;
        }

        private IEnumerable<Move> Explore(IList<Ship> availableShips)
        {

            var notOwnedPlanets = _gameMap.GetAllNotFullNotOwnedByOtherPlanets(_gameMap.GetMyPlayer().GetId());
            var targetPlanets = notOwnedPlanets.ToList();

            var moveList = new List<Move>();

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

        private Move MoveOrDockShipToPlanet(PlanetShipDistance bestPlanetShipDistance)
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
