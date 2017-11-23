using System;
using System.Linq;

namespace Halite2.hlt
{
    public class Navigation
    {
        public static ThrustMove NavigateShipToDock(
                GameMap gameMap,
                Ship ship,
                Entity dockTarget,
                int maxThrust)
        {
            int maxCorrections = Constants.MAX_NAVIGATION_CORRECTIONS;
            bool avoidObstacles = true;
            double angularStepRad = Math.PI / 180.0;
            Position targetPos = ship.GetClosestPoint(dockTarget);

            return NavigateShipTowardsTarget(gameMap, ship, targetPos, false, maxThrust, avoidObstacles, maxCorrections, angularStepRad);
        }

        public static ThrustMove CrashInPlanet(
            GameMap gameMap,
            Ship ship,
            Planet crashTarget,
            int maxThrust,
            int playerId)
        {
            int maxCorrections = Constants.MAX_NAVIGATION_CORRECTIONS;
            bool avoidObstacles = true;
            double angularStepRad = Math.PI / 180.0;
            Position targetPos = crashTarget.GetCenterPosition();

            return NavigateShipTowardsTarget(gameMap, ship, targetPos, true, maxThrust, avoidObstacles, maxCorrections, angularStepRad, playerId);
        }

        public static ThrustMove NavigateShipTowardsTarget(
                GameMap gameMap,
                Ship ship,
                Position targetPos,
                bool crash,
                int maxThrust,
                bool avoidObstacles,
                int maxCorrections,
                double angularStepRad,
                int playerId = -2)
        {
            if (maxCorrections <= 0)
            {
                return null;
            }

            double distance = ship.GetDistanceTo(targetPos);
            double angleRad = ship.OrientTowardsInRad(targetPos);
            var objectsBetween = crash ? gameMap.ObjectsBetween(ship, targetPos).Any(x => x.GetOwner() == playerId) : gameMap.ObjectsBetween(ship, targetPos).Any();
            if (avoidObstacles && objectsBetween)
            {
                double newTargetDx = Math.Cos(angleRad + angularStepRad) * distance;
                double newTargetDy = Math.Sin(angleRad + angularStepRad) * distance;
                Position newTarget = new Position(ship.GetXPos() + newTargetDx, ship.GetYPos() + newTargetDy);

                return NavigateShipTowardsTarget(gameMap, ship, newTarget, crash, maxThrust, true, (maxCorrections - 1), angularStepRad, playerId);
            }

            int thrust;
            if (distance < maxThrust && !crash)
            {
                // Do not round up, since overshooting might cause collision.
                thrust = (int)distance;
            }
            else
            {
                thrust = maxThrust;
            }

            int angleDeg = Util.AngleRadToDegClipped(angleRad);

            return new ThrustMove(ship, angleDeg, thrust);
        }
    }
}
