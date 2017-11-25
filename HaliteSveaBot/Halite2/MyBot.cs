using System;
using Halite2.hlt;
using System.Collections.Generic;
using System.Linq;
using Halite2.Tactics;

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
            var turn = 1;
            for (; ; )
            {
                Log.LogMessage($"=======================================================");
                Log.LogMessage($"New turn: {turn++}");
                _gameMap.UpdateMap(Networking.ReadLineIntoMetadata());

                var moves = myBot.GetAllMoves();

                Networking.SendMoves(moves);
            }
        }

        private IEnumerable<Move> GetAllMoves()
        {
            var turnMoves = new List<Move>();
            try
            {
                var player = _gameMap.GetMyPlayer();
                var ships = player.GetShips().Values;
                var availableShips = ships.Where(ship => ship.GetDockingStatus() == Ship.DockingStatus.Undocked)
                    .OrderByDescending(ownedShip => ownedShip.GetId())
                    .ToList();


                var tacticExpander = TacticFactory.GetTactic(Tactics.Tactics.ExpandGrasshopper);
                var tacticDefender = TacticFactory.GetTactic(Tactics.Tactics.DefenceTorAttackClosestShip);
                var tacticAttacker = TacticFactory.GetTactic(Tactics.Tactics.AttackTorClosestDocked);

                Log.LogMessage($"Total availableShips: {availableShips.Count}");
                var expandShipMoves = tacticExpander.UseShips(_gameMap, availableShips);
                AddMovesToTurnMoves(expandShipMoves, availableShips, turnMoves);
                Log.LogMessage($"After Expand availableShips: {availableShips.Count}");
                if (_gameMap.GetAllPlayers().Count > 2)
                {
                    var defenders = availableShips.Take(availableShips.Count / 2).ToList();
                    Log.LogMessage($"Defenders availableShips: {defenders.Count}");
                    var defenceShipMoves = tacticDefender.UseShips(_gameMap, defenders);
                    AddMovesToTurnMoves(defenceShipMoves, availableShips, turnMoves);
                }
                Log.LogMessage($"After Defence availableShips2: {availableShips.Count}");
                var attackShipMoves = tacticAttacker.UseShips(_gameMap, availableShips);
                AddMovesToTurnMoves(attackShipMoves, availableShips, turnMoves);
                Log.LogMessage($"After Attack availableShips: {availableShips.Count}");
            }
            catch (Exception e)
            {
                Log.LogMessage(e.Message);
                throw e;
            }

            return turnMoves;
        }

        private static void AddMovesToTurnMoves(IEnumerable<ShipMove> shipMoves, List<Ship> availableShips, List<Move> turnMoves)
        {
            foreach (var shipMove in shipMoves)
            {
                availableShips.Remove(shipMove.ship);
                if (shipMove.move != null)
                {
                    turnMoves.Add(shipMove.move);
                }
            }
        }
    }
}
