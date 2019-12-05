using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    public class Ballistics
    {
        public Vector2 ApexPosition, EndPosition;

        public double CurrentTime, MaxTime, ActualTime, PredictedTimeToMax, ActualTimeToMax,
               PredictedDistToOrigin, ActualDistToOrigin, DistToGround, TimeMaxToGround,
               TotalPredictedTime, PredictedXDist, ActualXDist;

        public Ballistics(HeavyProjectile projectile)
        {
            PredictedTimeToMax = (0 - projectile.Velocity.Y) / (projectile.Gravity / 16.666d);
            PredictedDistToOrigin = -((projectile.Velocity.Y * PredictedTimeToMax) + ((0.5f * projectile.Gravity / 16.6666d) * Math.Pow(PredictedTimeToMax, 2))) / 16.6666d;

            DistToGround = (PredictedDistToOrigin + projectile.StartPosition.Y) - ((projectile.StartPosition.Y * 2) - projectile.MaxY);

            TimeMaxToGround = Math.Sqrt((DistToGround / ((projectile.Gravity * 0.5f) / 16.6666d)) * 16.6666d);
            TotalPredictedTime = TimeMaxToGround + PredictedTimeToMax;
            PredictedXDist = projectile.StartPosition.X + (projectile.Velocity.X * TotalPredictedTime) / 16.666d;

            EndPosition = new Vector2((float)PredictedXDist, projectile.MaxY);
        }
    }
}
