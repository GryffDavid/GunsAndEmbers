using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    class Ballistics
    {
        HeavyProjectile HeavyProjectile;
        float TimeInAir;
        Vector2 LandingPosition;

        double PredictedDistToOrigin, PredictedTimeToMax, DistToGround, TimeMaxToGround, TotalPredictedTime, PredictedXDist;

        public Ballistics(HeavyProjectile projectile)
        {
            PredictedTimeToMax = (0 - projectile.Velocity.Y) / (projectile.Gravity / 16.666d);
            PredictedDistToOrigin = -((projectile.Velocity.Y * PredictedTimeToMax) + ((0.5f * projectile.Gravity / 16.6666d) * Math.Pow(PredictedTimeToMax, 2))) / 16.6666d;
            DistToGround = (PredictedDistToOrigin + (projectile.MaxY - projectile.Position.Y));
            TimeMaxToGround = Math.Sqrt((DistToGround / ((projectile.Gravity * 0.5f) / 16.6666d)) * 16.6666d);

            TotalPredictedTime = TimeMaxToGround + PredictedTimeToMax;

            ////PredictedXDist = ((projectile.Velocity.X * TotalPredictedTime) / 16.666d) - (TotalPredictedTime / 16.666d * 0.999f) - projectile.Position.X;
        }
    }
}
