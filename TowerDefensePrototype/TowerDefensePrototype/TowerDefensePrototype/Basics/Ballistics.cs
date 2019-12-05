using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    class Ballistics
    {
        double PredictedTimeToMax,
               PredictedDistToOrigin, DistToGround, TimeMaxToGround,
               TotalPredictedTime, PredictedXDist;

        HeavyProjectile HeavyProjectile;

        public Ballistics()
        {

        }

        public float GetXDist(HeavyProjectile heavyProjectile)
        {
            HeavyProjectile = heavyProjectile;

            PredictedTimeToMax = (0 - HeavyProjectile.Velocity.Y) / (HeavyProjectile.Gravity / 16.666d);
            PredictedDistToOrigin = -((HeavyProjectile.Velocity.Y * PredictedTimeToMax) + ((0.5f * HeavyProjectile.Gravity / 16.6666d) * Math.Pow(PredictedTimeToMax, 2))) / 16.6666d;
            DistToGround = (PredictedDistToOrigin + (720 / 2));
            TimeMaxToGround = Math.Sqrt((DistToGround / ((HeavyProjectile.Gravity * 0.5f) / 16.6666d)) * 16.6666d);

            TotalPredictedTime = TimeMaxToGround + PredictedTimeToMax;

            PredictedXDist = 0 + (HeavyProjectile.Velocity.X * TotalPredictedTime) / 16.666d;

            return (float)PredictedXDist;
        }
    }
}
