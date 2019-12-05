using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    public class UIWeaponInfoTip
    {
        public SpriteFont RobotoItalic20_0, RobotoRegular20_0, RobotoBold40_2, RobotoRegular20_2;
        Vector2 BoxSize, NameBoxSize, Position;
        UIBar UIBar1, UIBar2, UIBar3, UIBar4, UIBar5;
        string BarText1, BarText2, BarText3, BarText4, BarText5;

        public Texture2D DamageIcon, WeaponIcon, ConcussiveDamageIcon, KineticDamageIcon, FireDamageIcon, RadiationDamageIcon, ElectricDamageIcon;

        public Texture2D CurrencyIcon;

        public string WeaponDescription, WeaponTip, WeaponName, WeaponType;
        public bool Visible;
        
        VertexPositionColor[] NameBoxVertices = new VertexPositionColor[4];
        //Change this colour based on the damage type of the weapon - fire=orange, energy=purple etc.
        Color NameBoxColor = Color.Lerp(Color.Lerp(Color.White, Color.DodgerBlue, 0.85f), Color.Transparent, 0.5f);

        VertexPositionColor[] WeaponBoxVertices = new VertexPositionColor[4];
        Color WeaponBoxColor = Color.Lerp(Color.Black, Color.Transparent, 0.5f);
        
        VertexPositionColor[] DividerVertices1 = new VertexPositionColor[4];
        VertexPositionColor[] DividerVertices2 = new VertexPositionColor[4];

        int[] BoxIndices = new int[6];
        int[] NameBoxIndices = new int[6];

        int[] DividerIndices1 = new int[6];
        int[] DividerIndices2 = new int[6];

        int WeaponCost;
        int WeaponDamage;

        public UIWeaponInfoTip(Vector2 position, TurretType? turret = null, TrapType? trap = null)
        {
            Visible = false;
            Position = position;
            BoxSize = new Vector2(250, 330);
            NameBoxSize = new Vector2(250, 40);

            #region Default turret bar names
            BarText1 = "Rate of Fire";
            BarText2 = "Impact";
            BarText3 = "Range";
            BarText4 = "Stability";
            BarText5 = "Reload";
            #endregion

            WeaponName = "Weapon";
            
            //Set up the values that the weapon info will show based on what the trap/turret is
            if (turret != null)
            {
                #region Default turret bar names
                BarText1 = "Rate of Fire";
                BarText2 = "Impact";
                BarText3 = "Range";
                BarText4 = "Stability";
                BarText5 = "Reload";
                #endregion

                WeaponCost = TurretCost(turret.Value);
                WeaponType = "Turret";
                WeaponTip = "Fire this";
                switch (turret.Value)
                {
                    case TurretType.Cannon:
                        WeaponTip = "Arc into a crowd to maximise effectiveness. Blah blah blah. Blah bleh blurgh. Blah blah blah. Blah bleh blurgh.";
                        WeaponType = "Heavy arcing projectile";
                        break;

                    case TurretType.MachineGun:
                        WeaponTip = "Place on lower turret slots for devestation at close range";
                        WeaponType = "Machine Gun";
                        break;

                    case TurretType.Lightning:
                        WeaponType = "Beam";
                        break;
                }
            }

            if (trap != null)
            {
                #region Default trap bar names
                BarText1 = "Cooldown";
                BarText2 = "Damage";
                BarText3 = "Charges";
                BarText4 = "Stability";
                BarText5 = "Reload";
                #endregion

                WeaponCost = TrapCost(trap.Value);
                WeaponType = "Trap";
                WeaponTip = "Place this";
                switch (trap.Value)
                {
                    case TrapType.Fire:
                        WeaponName = "Immolation Trap";
                        break;

                    case TrapType.SawBlade:

                        break;

                    case TrapType.Spikes:

                        break;
                }
            }

            for (int i = 0; i < WeaponTip.Length; i++)
            {
                if ((i % 40) == 0)
                {
                    int k;
                    k = WeaponTip.LastIndexOf(" ", i);
                    WeaponTip = WeaponTip.Insert(k + 1, Environment.NewLine);
                }
            }

            UpdateQuads();
            SetUpBars();
            
        }

        public void SetUpBars()
        {
            UIBar1 = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 60),
                                   new Vector2(BoxSize.X - 125, 15), Color.White, false);

            UIBar2 = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 85),
                                   new Vector2(BoxSize.X - 125, 15), Color.White, false);

            UIBar3 = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 110),
                                   new Vector2(BoxSize.X - 125, 15), Color.White, false);

            UIBar4 = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 135),
                                   new Vector2(BoxSize.X - 125, 15), Color.White, false);

            UIBar5 = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 160),
                                   new Vector2(BoxSize.X - 125, 15), Color.White, false);

            UIBar1.Update(10, 2);
            UIBar2.Update(10, 3);
            UIBar3.Update(10, 6);
            UIBar4.Update(10, 8);
            UIBar5.Update(10, 4);
        }

        public void LoadContent(ContentManager contentManager)
        {
            ConcussiveDamageIcon = contentManager.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
            KineticDamageIcon = contentManager.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
            FireDamageIcon = contentManager.Load<Texture2D>("Icons/DamageTypeIcons/FireDamageIcon");
            RadiationDamageIcon = contentManager.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
            ElectricDamageIcon = contentManager.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
        }

        //public void Update(GameTime gameTime)
        //{
        //    Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        //    UpdateQuads();
        //    SetUpBars();
        //}

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect basicEffect)
        {
            if (Visible == true)
            {
                //Draw the weapon name
                spriteBatch.DrawString(RobotoBold40_2, WeaponName, new Vector2(Position.X + 4, Position.Y - BoxSize.Y - NameBoxSize.Y + 2),
                    Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

                //Draw the weapon type
                spriteBatch.DrawString(RobotoRegular20_2, WeaponType, new Vector2(Position.X + 5, Position.Y - BoxSize.Y - NameBoxSize.Y + 23),
                    Color.White, 0, new Vector2(0, 0), 0.7f, SpriteEffects.None, 0);

                //Draw the weapon damage
                spriteBatch.DrawString(RobotoBold40_2, "248", new Vector2(Position.X + 5, Position.Y - BoxSize.Y + 2),
                    Color.DodgerBlue, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                //Draw the weapon tip, e.g. "Fire into a closely packed crowed for maximum carnage"
                spriteBatch.DrawString(RobotoItalic20_0, WeaponTip, new Vector2(Position.X + 5, Position.Y - 62),
                    Color.White, 0, new Vector2(0, 0), 0.7f, SpriteEffects.None, 0);


                spriteBatch.Draw(CurrencyIcon, new Rectangle((int)(Position.X + 5), (int)(Position.Y - 80), 24, 24), Color.White);

                spriteBatch.Draw(FireDamageIcon, new Rectangle((int)Position.X + 5, (int)(Position.Y - BoxSize.Y + 5), FireDamageIcon.Width, FireDamageIcon.Height), Color.Orange);

                //Draw the resource cost of the weapon
                //*****CHANGE THIS COLOUR TO GRAY IF THE PLAYER CANNOT AFFORD THE WEAPON*****//
                spriteBatch.DrawString(RobotoRegular20_0, WeaponCost.ToString(), new Vector2(Position.X + 35, Position.Y - 78),
                    Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                #region Draw the names of the bars
                Vector2 rateOfFireSize = RobotoRegular20_0.MeasureString(BarText1);
                spriteBatch.DrawString(RobotoRegular20_0, BarText1, new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 60),
                    Color.White, 0, new Vector2(rateOfFireSize.X, 0), 0.8f, SpriteEffects.None, 0);

                Vector2 impactSize = RobotoRegular20_0.MeasureString(BarText2);
                spriteBatch.DrawString(RobotoRegular20_0, BarText2, new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 85),
                    Color.White, 0, new Vector2(impactSize.X, 0), 0.8f, SpriteEffects.None, 0);

                Vector2 rangeSize = RobotoRegular20_0.MeasureString(BarText3);
                spriteBatch.DrawString(RobotoRegular20_0, BarText3, new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 110),
                    Color.White, 0, new Vector2(rangeSize.X, 0), 0.8f, SpriteEffects.None, 0);

                Vector2 ResponseSize = RobotoRegular20_0.MeasureString(BarText4);
                spriteBatch.DrawString(RobotoRegular20_0, BarText4, new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 135),
                    Color.White, 0, new Vector2(ResponseSize.X, 0), 0.8f, SpriteEffects.None, 0);

                Vector2 reloadSize = RobotoRegular20_0.MeasureString(BarText5);
                spriteBatch.DrawString(RobotoRegular20_0, BarText5, new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 160),
                    Color.White, 0, new Vector2(reloadSize.X, 0), 0.8f, SpriteEffects.None, 0);
                #endregion

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        NameBoxVertices.ToArray(), 0, 4,
                        NameBoxIndices, 0, 2,
                        VertexPositionColor.VertexDeclaration);

                    graphics.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        WeaponBoxVertices.ToArray(), 0, 4,
                        BoxIndices, 0, 2,
                        VertexPositionColor.VertexDeclaration);

                    graphics.DrawUserIndexedPrimitives(
                       PrimitiveType.TriangleList,
                       DividerVertices1.ToArray(), 0, 4,
                       DividerIndices1, 0, 2,
                       VertexPositionColor.VertexDeclaration);

                    graphics.DrawUserIndexedPrimitives(
                       PrimitiveType.TriangleList,
                       DividerVertices2.ToArray(), 0, 4,
                       DividerIndices2, 0, 2,
                       VertexPositionColor.VertexDeclaration);

                    UIBar1.Draw(graphics);
                    UIBar2.Draw(graphics);
                    UIBar3.Draw(graphics);
                    UIBar4.Draw(graphics);
                    UIBar5.Draw(graphics);
                }
            }
        }

        public void UpdateQuads()
        {
            #region Setting up the name box
            NameBoxVertices[0] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X, Position.Y - BoxSize.Y, 0),
                Color = NameBoxColor
            };

            NameBoxVertices[1] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X, Position.Y - BoxSize.Y - NameBoxSize.Y, 0),
                Color = NameBoxColor
            };

            NameBoxVertices[2] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X, Position.Y - BoxSize.Y, 0),
                Color = NameBoxColor
            };

            NameBoxVertices[3] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X, Position.Y - BoxSize.Y - NameBoxSize.Y, 0),
                Color = NameBoxColor
            };

            NameBoxIndices[0] = 1;
            NameBoxIndices[1] = 2;
            NameBoxIndices[2] = 0;
            NameBoxIndices[3] = 1;
            NameBoxIndices[4] = 3;
            NameBoxIndices[5] = 2;
            #endregion

            #region Setting up the weapon box
            WeaponBoxVertices[0] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X, Position.Y, 0),
                Color = WeaponBoxColor
            };

            WeaponBoxVertices[1] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X, Position.Y - BoxSize.Y, 0),
                Color = WeaponBoxColor
            };

            WeaponBoxVertices[2] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X, Position.Y, 0),
                Color = WeaponBoxColor
            };

            WeaponBoxVertices[3] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X, Position.Y - BoxSize.Y, 0),
                Color = WeaponBoxColor
            };

            BoxIndices[0] = 1;
            BoxIndices[1] = 2;
            BoxIndices[2] = 0;
            BoxIndices[3] = 1;
            BoxIndices[4] = 3;
            BoxIndices[5] = 2;
            #endregion


            #region Set up the first divider
            DividerVertices1[0] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + 5, Position.Y - 50, 0),
                Color = Color.White
            };

            DividerVertices1[1] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + 5, Position.Y - 52, 0),
                Color = Color.White
            };

            DividerVertices1[2] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X - 5, Position.Y - 50, 0),
                Color = Color.White
            };

            DividerVertices1[3] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X - 5, Position.Y - 52, 0),
                Color = Color.White
            };

            DividerIndices1[0] = 1;
            DividerIndices1[1] = 2;
            DividerIndices1[2] = 0;
            DividerIndices1[3] = 1;
            DividerIndices1[4] = 3;
            DividerIndices1[5] = 2;
            #endregion

            #region Set up the second divider
            DividerVertices2[0] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + 5, Position.Y - BoxSize.Y + 47, 0),
                Color = Color.White
            };

            DividerVertices2[1] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + 5, Position.Y - BoxSize.Y + 45, 0),
                Color = Color.White
            };

            DividerVertices2[2] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X - 5, Position.Y - BoxSize.Y + 47, 0),
                Color = Color.White
            };

            DividerVertices2[3] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X - 5, Position.Y - BoxSize.Y + 45, 0),
                Color = Color.White
            };

            DividerIndices2[0] = 1;
            DividerIndices2[1] = 2;
            DividerIndices2[2] = 0;
            DividerIndices2[3] = 1;
            DividerIndices2[4] = 3;
            DividerIndices2[5] = 2;
            #endregion
        }

        private int TurretCost(TurretType turretType)
        {
            int cost = 0;

            switch (turretType)
            {
                case TurretType.MachineGun:
                    cost = new MachineGunTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Cannon:
                    cost = new CannonTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.FlameThrower:
                    cost = new FlameThrowerTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Lightning:
                    cost = new LightningTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Cluster:
                    cost = new ClusterTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.FelCannon:
                    cost = new FelCannonTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Beam:
                    cost = new BeamTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Freeze:
                    cost = new FreezeTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Boomerang:
                    cost = new BoomerangTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Grenade:
                    cost = new GrenadeTurret(Vector2.Zero).ResourceCost;
                    break;
            }

            return cost;
        }
        private int TrapCost(TrapType trapType)
        {
            int cost = 0;

            switch (trapType)
            {
                case TrapType.Fire:
                    cost = new FireTrap(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.Catapult:
                    cost = new CatapultTrap(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.Ice:
                    cost = new IceTrap(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.SawBlade:
                    cost = new SawBladeTrap(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.Spikes:
                    cost = new SpikeTrap(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.Tar:
                    cost = new TarTrap(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.Wall:
                    cost = new Wall(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.Barrel:
                    cost = new BarrelTrap(Vector2.Zero).ResourceCost;
                    break;
            }

            return cost;
        }
    }
}
