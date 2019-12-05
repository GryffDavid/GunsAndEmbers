using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    class WeaponInfoBox
    {
        public float HitPoints, Damage, Accuracy, FireRate;
        public float MaxHitPoint, MaxDamage, MaxAccuracy, MaxFireRate;

        List<StaticSprite> HitPointsBoxes = new List<StaticSprite>();
        List<StaticSprite> DamageBoxes = new List<StaticSprite>();
        List<StaticSprite> AccuracyBoxes = new List<StaticSprite>();
        List<StaticSprite> FireRateBoxes = new List<StaticSprite>();

        public bool Overheat, Visible;

        public WeaponInfoBox()
        {
            Visible = false;

            for (int i = 0; i < 10; i++)
            {
                HitPointsBoxes.Add(new StaticSprite("StatBlock", new Vector2(100+(8*i), 100), Vector2.One, Color.Lerp(Color.White, Color.Transparent, 0.5f)));
                DamageBoxes.Add(new StaticSprite("StatBlock", new Vector2(100 + (8 * i), 100+18), Vector2.One, Color.Lerp(Color.White, Color.Transparent, 0.5f)));
                AccuracyBoxes.Add(new StaticSprite("StatBlock", new Vector2(100 + (8 * i), 100+18+18), Vector2.One, Color.Lerp(Color.White, Color.Transparent, 0.5f)));
                FireRateBoxes.Add(new StaticSprite("StatBlock", new Vector2(100 + (8 * i), 100+18+18+18), Vector2.One, Color.Lerp(Color.White, Color.Transparent, 0.5f)));
            }
        }

        public void LoadContent(ContentManager contentManager)
        {
            for (int i = 0; i < 10; i++)
            {
                HitPointsBoxes[i].LoadContent(contentManager);
                DamageBoxes[i].LoadContent(contentManager);
                AccuracyBoxes[i].LoadContent(contentManager);
                FireRateBoxes[i].LoadContent(contentManager);
            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Visible == true)
            {
                for (int i = 0; i < 10; i++)
                {
                    HitPointsBoxes[i].Draw(spriteBatch);
                    DamageBoxes[i].Draw(spriteBatch);
                    AccuracyBoxes[i].Draw(spriteBatch);
                    FireRateBoxes[i].Draw(spriteBatch);
                }
            }
        }

        public void UpdateStats(float hitPoints, float damage, float accuracy, float fireRate)
        {
            HitPoints = hitPoints;
            Damage = damage;
            Accuracy = accuracy;
            FireRate = fireRate;


            for (int i = 0; i < 10; i++)
            {
                HitPointsBoxes[i].Color = Color.Lerp(Color.White, Color.Transparent, 0.5f);
                DamageBoxes[i].Color = Color.Lerp(Color.White, Color.Transparent, 0.5f);
                AccuracyBoxes[i].Color = Color.Lerp(Color.White, Color.Transparent, 0.5f);
                FireRateBoxes[i].Color = Color.Lerp(Color.White, Color.Transparent, 0.5f);
            }

            for (int i = 0; i < HitPoints; i++)
            {
                HitPointsBoxes[i].Color = Color.White;
            }

            for (int i = 0; i < Damage; i++)
            {
                DamageBoxes[i].Color = Color.White;
            }

            for (int i = 0; i < Accuracy; i++)
            {
                AccuracyBoxes[i].Color = Color.White;
            }

            for (int i = 0; i < FireRate; i++)
            {
                FireRateBoxes[i].Color = Color.White;
            }
        }
    }
}
