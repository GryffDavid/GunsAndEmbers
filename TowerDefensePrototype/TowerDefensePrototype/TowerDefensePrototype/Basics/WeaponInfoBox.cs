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
        public string NameText;
        SpriteFont NameFont;

        List<StaticSprite> HitPointsBoxes = new List<StaticSprite>();
        List<StaticSprite> DamageBoxes = new List<StaticSprite>();
        List<StaticSprite> AccuracyBoxes = new List<StaticSprite>();
        List<StaticSprite> FireRateBoxes = new List<StaticSprite>();

        public bool Visible;

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
            NameFont = contentManager.Load<SpriteFont>("Fonts/DefaultFont");

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
                if (NameText != null)
                    spriteBatch.DrawString(NameFont, NameText, new Vector2(100, 400), Color.White);

                for (int i = 0; i < 10; i++)
                {
                    HitPointsBoxes[i].Draw(spriteBatch);
                    DamageBoxes[i].Draw(spriteBatch);
                    AccuracyBoxes[i].Draw(spriteBatch);
                    FireRateBoxes[i].Draw(spriteBatch);
                }
            }
        }

        public void WeaponStats(Nullable<TurretType> turretType, Nullable<TrapType> trapType)
        {
            switch (turretType)
            {
                case TurretType.MachineGun:
                    {
                        NameText = "Machine Gun";
                        UpdateStats(5, 1, 1, 5);                        
                    }
                    break;

                case TurretType.Cannon:
                    {
                        NameText = "Cannon";
                        UpdateStats(5, 8, 5, 2);
                    }
                    break;

                case TurretType.FlameThrower:
                    {
                        NameText = "Flame Thrower";
                        UpdateStats(1, 1, 1, 1);
                    }
                    break;

                case TurretType.Lightning:
                    {
                        NameText = "Lightning Gun";
                        UpdateStats(2, 1, 1, 1);
                    }
                    break;

                case TurretType.Cluster:
                    {
                        NameText = "Cluster Bomb";
                        UpdateStats(3, 1, 1, 1);
                    }
                    break;

                case TurretType.FelCannon:
                    {
                        NameText = "fel Cannon";
                        UpdateStats(4, 1, 1, 1);
                    }
                    break;

                case TurretType.Beam:
                    {
                        NameText = "Beam Cannon";
                        UpdateStats(5, 1, 1, 1);
                    }
                    break;

                case TurretType.Freeze:
                    {
                        NameText = "Freeze Cannon";
                        UpdateStats(6, 1, 1, 1);
                    }
                    break;

                case TurretType.Boomerang:
                    {
                        NameText = "Boomerang Bomb";
                        UpdateStats(7, 1, 1, 1);
                    }
                    break;

                case TurretType.Grenade:
                    {
                        NameText = "Grenade Launcher";
                        UpdateStats(8, 1, 1, 1);
                    }
                    break;

                case TurretType.Shotgun:
                    {
                        NameText = "Shotgun";
                        UpdateStats(8, 1, 1, 1);
                    }
                    break;
            }

            switch (trapType)
            {
                case TrapType.Fire:
                    {
                        NameText = "Fire Trap";
                        UpdateStats(8, 1, 1, 1);
                    }
                    break;

                case TrapType.Barrel:
                    {
                        NameText = "Barrel Trap";
                        UpdateStats(8, 1, 1, 1);
                    }
                    break;

                case TrapType.Catapult:
                    {
                        NameText = "Catapult Trap";
                        UpdateStats(8, 1, 1, 1);
                    }
                    break;

                case TrapType.Ice:
                    {
                        NameText = "Ice Trap";
                        UpdateStats(8, 1, 1, 1);
                    }
                    break;

                case TrapType.SawBlade:
                    {
                        NameText = "Saw Trap";
                        UpdateStats(8, 1, 1, 1);
                    }
                    break;

                case TrapType.Spikes:
                    {
                        NameText = "Spikes Trap";
                        UpdateStats(8, 1, 1, 1);
                    }
                    break;

                case TrapType.Tar:
                    {
                        NameText = "Tar Trap";
                        UpdateStats(8, 1, 1, 1);
                    }
                    break;

                case TrapType.Wall:
                    {
                        NameText = "Wall";
                        UpdateStats(8, 1, 1, 1);
                    }
                    break;
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
