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
            string name = args.Length > 0 ? args[0] : "ExpandDefendAttack";

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

            var ownedAreaCenter = CalculateOwnedAreaCenter();

            var explorerMoves = Expand(availableShips);
            var defenseMoves = new List<Move>();
            if (_gameMap.GetAllPlayers().Count > 2)
            {
                defenseMoves = StaffansStupidDefenseStrategy(availableShips);
            }
            var attackMoves = TorCrazyStrategy(availableShips, GetTargetPlanet(ownedAreaCenter));

            return explorerMoves.Concat(defenseMoves).Concat(attackMoves);
        }

        private List<Move> StaffansStupidDefenseStrategy(List<Ship> availableShips)
        {
            var defendMoves = new List<Move>();
            var defenders = availableShips.OrderByDescending(x => x.GetId()).Take(availableShips.Count / 2);
            var enemyShips = _gameMap.GetAllShips().Where(ship => ship.GetOwner() != _gameMap.GetMyPlayerId()).ToList();
            foreach (var defender in defenders)
            {
                Ship closestEnemy = null;
                var closestDistance = 1e9;
                foreach (var enemyShip in enemyShips)
                {
                    var enemyDistance = defender.GetDistanceTo(enemyShip.GetClosestPosition());
                    if (enemyDistance < closestDistance)
                    {
                        closestDistance = enemyDistance;
                        closestEnemy = enemyShip;
                    }
                }
                if (closestEnemy != null)
                {
                    var shipMaxSpeed = closestDistance < 50 ? 3 : Constants.MAX_SPEED;
                    var attackMove = Navigation.AttackShip(_gameMap, defender, closestEnemy, shipMaxSpeed, _gameMap.GetMyPlayerId());
                    defendMoves.Add(attackMove);
                }
                availableShips.Remove(defender);
            }
            return defendMoves;
        }

        private Planet GetTargetPlanet(Position ownedAreaCenter)
        {
            var playerId = _gameMap.GetMyPlayer().GetId();
            var targetPlanet = _gameMap.GetAllOwnedByOthers(playerId).OrderBy(planet => planet.GetDistanceTo(ownedAreaCenter)).FirstOrDefault();
            return targetPlanet;
        }

        private IEnumerable<Move> TorCrazyStrategy(List<Ship> availableShips, Planet targetPlanet)
        {
            var moves = new List<Move>();
            var playerId = _gameMap.GetMyPlayer().GetId();
            //if (availableShips.Count < 10)
            //    return moves;

            if (targetPlanet == null)
                return moves;

            foreach (var ship in availableShips.Take(50))
            {
                Ship targetDockedShip = null;
                var closestDistance = 1e9;
                foreach (var dockedShipId in targetPlanet.GetDockedShips())
                {
                    var dockedShip = _gameMap.GetShip(targetPlanet.GetOwner(), dockedShipId);
                    var dockedShipDistance = ship.GetDistanceTo(dockedShip.GetClosestPosition());
                    if (dockedShipDistance < closestDistance)
                    {
                        closestDistance = dockedShipDistance;
                        targetDockedShip = dockedShip;
                    }
                }
                var shipMaxSpeed = closestDistance < 50 ? 3 : Constants.MAX_SPEED;
                var attackMove = Navigation.AttackShip(_gameMap, ship, targetDockedShip, shipMaxSpeed, playerId);
                if (attackMove != null)
                    moves.Add(attackMove);
            }

            return moves;
        }

        private IEnumerable<Move> Expand(IList<Ship> availableShips)
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

                var bestGlobalPlanetShipDistance = planetShipDistances
                    //.OrderBy(planetShipDistance => planetShipDistance.Planet.GetId() < 4 ? 1 : 2)
                    .OrderBy(planetShipDistance => planetShipDistance.Distance).First();

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

        private static Position CalculateOwnedAreaCenter()
        {
            var allOwnedPlanets = _gameMap.GetAllPlanets().Values.Where(planet => planet.GetOwner() == _gameMap.GetMyPlayerId());
            if (!allOwnedPlanets.Any())
            {
                return new Position(0, 0);
            }
            var centerX = allOwnedPlanets.Select(planet => planet.GetXPos()).Sum() / allOwnedPlanets.Count();
            var centerY = allOwnedPlanets.Select(planet => planet.GetYPos()).Sum() / allOwnedPlanets.Count();
            return new Position(centerX, centerY);
        }

        private Move MoveOrDockShipToPlanet(PlanetShipDistance bestPlanetShipDistance)
        {
            var ship = bestPlanetShipDistance.Ship;
            var planet = bestPlanetShipDistance.Planet;

            if (ship.CanDock(planet))
            {
                return new DockMove(ship, planet);
            }

            ThrustMove newThrustMove = Navigation.NavigateShipToDock(_gameMap, ship, planet, Constants.MAX_SPEED, _gameMap.GetMyPlayerId());
            
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
