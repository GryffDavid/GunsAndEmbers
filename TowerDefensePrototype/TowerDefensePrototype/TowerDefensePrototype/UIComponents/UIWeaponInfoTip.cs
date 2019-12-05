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
        public Button DetailsButton;
        public SpriteFont RobotoItalic20_0, RobotoRegular20_0, RobotoBold40_2, RobotoRegular20_2;
        public Vector2 BoxSize, NameBoxSize, Position;
        UIBar UIBar1, UIBar2, UIBar3, UIBar4, UIBar5;
        string BarText1, BarText2, BarText3, BarText4, BarText5;

        public Texture2D DamageIcon, WeaponIcon, ConcussiveDamageIcon, KineticDamageIcon, 
                         FireDamageIcon, RadiationDamageIcon, ElectricDamageIcon;

        public Texture2D CurrencyIcon, PowerUnitIcon;

        public string WeaponDescription, WeaponTip, WeaponName, WeaponType;
        public bool Visible;
        
        VertexPositionColor[] NameBoxVertices = new VertexPositionColor[4];
        //Change this colour based on the damage type of the weapon - fire=orange, energy=purple etc.
        public Color NameBoxColor = Color.Lerp(Color.Lerp(Color.White, Color.DodgerBlue, 0.85f), Color.Transparent, 0.5f);

        VertexPositionColor[] WeaponBoxVertices = new VertexPositionColor[4];
        public Color WeaponBoxColor = Color.Lerp(Color.Black, Color.Transparent, 0.5f);
        
        VertexPositionColor[] DividerVertices1 = new VertexPositionColor[4];
        VertexPositionColor[] DividerVertices2 = new VertexPositionColor[4];

        int[] BoxIndices = new int[6];
        int[] NameBoxIndices = new int[6];

        int[] DividerIndices1 = new int[6];
        int[] DividerIndices2 = new int[6];

        int WeaponCost;
        int WeaponDamage;

        int Bar1Value, Bar2Value, Bar3Value, Bar4Value, Bar5Value;

        int Charges;

        public Nullable<TurretType> ContainsTurret = null;
        public Nullable<TrapType> ContainsTrap = null;

        //This is only used in the profile management screen to lock weapons that aren't available
        public bool Locked = false;

        public UIWeaponInfoTip(Vector2 position, Turret turret = null, Trap trap = null)
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
            WeaponTip = "Place this";

            //Set up the values that the weapon info will show based on what the trap/turret is
            SetUpBars();

            if (turret != null)
            {
                #region Default turret bar names
                BarText1 = "Rate of Fire";
                BarText2 = "Impact";
                BarText3 = "Range";
                BarText4 = "Stability";
                BarText5 = "Reload";
                #endregion

                WeaponCost = TurretCost(turret.TurretType);
                WeaponType = "Turret";
                WeaponTip = "Fire this";
                WeaponDamage = turret.Damage;

                switch (turret.TurretType)
                {
                    case TurretType.Cannon:
                        WeaponName = "Cannon";
                        WeaponTip = "Arc into a crowd to maximise effectiveness. Blah blah blah. Blah bleh blurgh. Blah blah blah. Blah bleh blurgh.";
                        WeaponType = "Heavy arcing projectile";     
                        
                        BarText1 = "Rate of Fire";
                        UIBar1.Update(300, 7.5f);

                        BarText2 = "Blast Radius";
                        UIBar2.Update(1080, turret.BlastRadius);

                        BarText3 = "Velocity";
                        UIBar3.Update(40, turret.LaunchVelocity);
                        break;

                    case TurretType.MachineGun:
                        WeaponName = "Machine Gun";
                        WeaponTip = "Place on lower turret slots for devestation at close range";
                        WeaponType = "Machine Gun";

                        BarText1 = "Rate of Fire";
                        UIBar1.Update(300, 240);
                        break;

                    case TurretType.Lightning:
                        WeaponName = "Lightning Cannon";
                        WeaponType = "Beam";
                        Charges = turret.Charges;
                        break;

                    case TurretType.Cluster:
                        WeaponName = "Cluster Cannon";
                        break;

                    case TurretType.FlameThrower:
                        WeaponName = "Flamethrower";
                        break;

                    case TurretType.Shotgun:
                        WeaponName = "Shotgun";
                        break;

                    case TurretType.Freeze:
                        WeaponName = "Freeze Cannon";
                        break;

                    case TurretType.Grenade:
                        WeaponName = "Grenade Launcher";
                        break;

                    case TurretType.GasGrenade:
                        WeaponName = "Gas Grenade Launcher";
                        break;

                    case TurretType.Beam:
                        WeaponName = "Beam Cannon";
                        break;

                    case TurretType.Boomerang:
                        WeaponName = "Boomerang";
                        break;

                    case TurretType.FelCannon:
                        WeaponName = "Fel Cannon";
                        break;

                    case TurretType.PersistentBeam:
                        WeaponName = "Persistent Beam";
                        break;
                }

                Bar1Value = (int)(100f / (float)UIBar1.MaxValue * (float)UIBar1.CurrentValue);
                Bar2Value = (int)(100f / (float)UIBar2.MaxValue * (float)UIBar2.CurrentValue);
                Bar3Value = (int)(100f / (float)UIBar3.MaxValue * (float)UIBar3.CurrentValue);
                Bar4Value = (int)(100f / (float)UIBar4.MaxValue * (float)UIBar4.CurrentValue);
                Bar5Value = (int)(100f / (float)UIBar5.MaxValue * (float)UIBar5.CurrentValue);
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

                WeaponCost = TrapCost(trap.TrapType);
                WeaponName = "TRAP";
                WeaponType = "Trap";
                WeaponTip = "Place this";

                switch (trap.TrapType)
                {
                    case TrapType.Barrel:
                        WeaponName = "Barrel Trap";
                        break;

                    case TrapType.Fire:
                        WeaponName = "Immolation Trap";
                        break;

                    case TrapType.SawBlade:
                        WeaponName = "Sawblade Trap";
                        break;

                    case TrapType.Spikes:
                        WeaponName = "Spikes Trap";
                        break;

                    case TrapType.Catapult:
                        WeaponName = "Catapult Trap";
                        break;

                    case TrapType.Ice:
                        WeaponName = "Ice Trap";
                        break;

                    case TrapType.Wall:
                        WeaponName = "Wall";
                        break;

                    case TrapType.LandMine:
                        WeaponName = "Land Mine";
                        break;

                    case TrapType.Trigger:
                        WeaponName = "Trigger";
                        break;

                    case TrapType.Line:
                        WeaponName = "Line";
                        break;
                }
            }

            for (int i = 0; i < WeaponTip.Length; i++)
            {
                if ((i % 38) == 1 && i != 1)
                {
                    int k;
                    k = WeaponTip.LastIndexOf(" ", i);
                    WeaponTip = WeaponTip.Insert(k + 1, Environment.NewLine);
                }
            }

            UpdateQuads();
            //SetUpBars();            
        }

        public void SetUpBars()
        {
            UIBar1 = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 60),
                                   new Vector2(BoxSize.X - 135, 15), Color.White, false);
            UIBar1.Update(100, 99);


            UIBar2 = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 85),
                                   new Vector2(BoxSize.X - 135, 15), Color.White, false);
            UIBar2.Update(100, 99);


            UIBar3 = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 110),
                                   new Vector2(BoxSize.X - 135, 15), Color.White, false);
            UIBar3.Update(100, 99);


            UIBar4 = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 135),
                                   new Vector2(BoxSize.X - 135, 15), Color.White, false);
            UIBar4.Update(100, 100);


            UIBar5 = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 160),
                                   new Vector2(BoxSize.X - 135, 15), Color.White, false);
            UIBar5.Update(100, 100);
        }

        public void LoadContent(ContentManager contentManager)
        {
            //ConcussiveDamageIcon = contentManager.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
            //KineticDamageIcon = contentManager.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
            //FireDamageIcon = contentManager.Load<Texture2D>("Icons/DamageTypeIcons/FireDamageIcon");
            //RadiationDamageIcon = contentManager.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
            //ElectricDamageIcon = contentManager.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect basicEffect)
        {
            if (Visible == true)
            {
                //Draw the weapon name
                spriteBatch.DrawString(RobotoBold40_2, WeaponName, new Vector2(Position.X + 4, Position.Y - BoxSize.Y - NameBoxSize.Y + 2),
                    Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

                if (Locked == false)
                {
                    //Draw the weapon type
                    spriteBatch.DrawString(RobotoRegular20_2, WeaponType, new Vector2(Position.X + 5, Position.Y - BoxSize.Y - NameBoxSize.Y + 23),
                        Color.White, 0, new Vector2(0, 0), 0.7f, SpriteEffects.None, 0);

                    //Draw the weapon damage
                    spriteBatch.DrawString(RobotoBold40_2, WeaponDamage.ToString(), new Vector2(Position.X + 30, Position.Y - BoxSize.Y + 2),
                        Color.DodgerBlue, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                    //Draw the weapon tip, e.g. "Fire into a closely packed crowed for maximum carnage"
                    spriteBatch.DrawString(RobotoItalic20_0, WeaponTip, new Vector2(Position.X + 5, DividerVertices1[0].Position.Y + 4),
                        Color.White, 0, new Vector2(0, 0), 0.7f, SpriteEffects.None, 0);


                    spriteBatch.Draw(CurrencyIcon,
                        new Rectangle((int)(Position.X + 5), (int)(Position.Y - 80), 24, 24),
                        Color.White);

                    if (ContainsTrap != null)
                    {
                        spriteBatch.Draw(PowerUnitIcon,
                            new Rectangle((int)(Position.X + BoxSize.X - 5 - 24 - 35), (int)(Position.Y - 80), 24, 24),
                            Color.White);

                        spriteBatch.DrawString(RobotoRegular20_0, WeaponCost.ToString(), new Vector2(Position.X + BoxSize.X - 35, Position.Y - 78),
                        Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                    }

                    if (DamageIcon != null)
                    {
                        Vector2 NewSize = new Vector2(20, (int)((20f / DamageIcon.Width) * DamageIcon.Height));
                        spriteBatch.Draw(DamageIcon,
                            new Rectangle(
                                (int)Position.X + 5, (int)(Position.Y - BoxSize.Y + 23),
                                (int)NewSize.X, (int)NewSize.Y), null,
                                Color.Orange, 0, new Vector2(0, DamageIcon.Height / 2), SpriteEffects.None, 0);
                    }

                    //Draw the resource cost of the weapon
                    //*****CHANGE THIS COLOUR TO GRAY IF THE PLAYER CANNOT AFFORD THE WEAPON*****//
                    spriteBatch.DrawString(RobotoRegular20_0, WeaponCost.ToString(), new Vector2(Position.X + 35, Position.Y - 78),
                        Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                    
                    #region Draw the names and values of the bars
                    #region Bar 1
                    //Name
                    Vector2 rateOfFireSize = RobotoRegular20_0.MeasureString(BarText1);
                    spriteBatch.DrawString(RobotoRegular20_0, BarText1, new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 60),
                        Color.White, 0, new Vector2(rateOfFireSize.X, 0), 0.8f, SpriteEffects.None, 0);

                    //Value
                    Vector2 rateOfFireValueSize = RobotoRegular20_0.MeasureString(Bar1Value.ToString());
                    spriteBatch.DrawString(RobotoRegular20_0, Bar1Value.ToString(), new Vector2(Position.X + 250 - 25, Position.Y - BoxSize.Y + 60),
                        Color.White, 0, new Vector2(0, 0), 0.8f, SpriteEffects.None, 0);
                    #endregion

                    #region Bar 2
                    Vector2 impactSize = RobotoRegular20_0.MeasureString(BarText2);
                    spriteBatch.DrawString(RobotoRegular20_0, BarText2, new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 85),
                        Color.White, 0, new Vector2(impactSize.X, 0), 0.8f, SpriteEffects.None, 0);

                    Vector2 impactValueSize = RobotoRegular20_0.MeasureString(Bar2Value.ToString());
                    spriteBatch.DrawString(RobotoRegular20_0, Bar2Value.ToString(), new Vector2(Position.X + 250 - 25, Position.Y - BoxSize.Y + 85),
                        Color.White, 0, new Vector2(0, 0), 0.8f, SpriteEffects.None, 0);
                    #endregion

                    #region Bar 3
                    Vector2 rangeSize = RobotoRegular20_0.MeasureString(BarText3);
                    spriteBatch.DrawString(RobotoRegular20_0, BarText3, new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 110),
                        Color.White, 0, new Vector2(rangeSize.X, 0), 0.8f, SpriteEffects.None, 0);

                    Vector2 rangeValueSize = RobotoRegular20_0.MeasureString(Bar3Value.ToString());
                    spriteBatch.DrawString(RobotoRegular20_0, Bar3Value.ToString(), new Vector2(Position.X + 250 - 25, Position.Y - BoxSize.Y + 110),
                        Color.White, 0, new Vector2(0, 0), 0.8f, SpriteEffects.None, 0);
                    #endregion

                    #region Bar 4
                    Vector2 ResponseSize = RobotoRegular20_0.MeasureString(BarText4);
                    spriteBatch.DrawString(RobotoRegular20_0, BarText4, new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 135),
                        Color.White, 0, new Vector2(ResponseSize.X, 0), 0.8f, SpriteEffects.None, 0);

                    Vector2 ResponseValueSize = RobotoRegular20_0.MeasureString(Bar4Value.ToString());
                    spriteBatch.DrawString(RobotoRegular20_0, Bar4Value.ToString(), new Vector2(Position.X + 250 - 25, Position.Y - BoxSize.Y + 135),
                        Color.White, 0, new Vector2(0, 0), 0.8f, SpriteEffects.None, 0);
                    #endregion

                    #region Bar 5
                    Vector2 reloadSize = RobotoRegular20_0.MeasureString(BarText5);
                    spriteBatch.DrawString(RobotoRegular20_0, BarText5, new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 160),
                        Color.White, 0, new Vector2(reloadSize.X, 0), 0.8f, SpriteEffects.None, 0);

                    Vector2 reloadValueSize = RobotoRegular20_0.MeasureString(Bar5Value.ToString());
                    spriteBatch.DrawString(RobotoRegular20_0, Bar5Value.ToString(), new Vector2(Position.X + 250 - 25, Position.Y - BoxSize.Y + 160),
                        Color.White, 0, new Vector2(0, 0), 0.8f, SpriteEffects.None, 0);
                    #endregion
                    #endregion


                    Vector2 chargesSize = RobotoRegular20_0.MeasureString(Charges.ToString());
                    spriteBatch.DrawString(RobotoRegular20_0, Charges.ToString(), new Vector2(Position.X + 250 - 5, Position.Y - BoxSize.Y + 175),
                        Color.White, 0, new Vector2(chargesSize.X, 0), 0.8f, SpriteEffects.None, 0);
                }

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

                    if (Locked == false)
                    {
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
                    cost = MachineGunTurret.ResourceCost;
                    break;

                case TurretType.Cannon:
                    cost = CannonTurret.ResourceCost;
                    break;

                case TurretType.FlameThrower:
                    cost = FlameThrowerTurret.ResourceCost;
                    break;

                case TurretType.Lightning:
                    cost = LightningTurret.ResourceCost;
                    break;

                case TurretType.Cluster:
                    cost = ClusterTurret.ResourceCost;
                    break;

                case TurretType.FelCannon:
                    cost = FelCannonTurret.ResourceCost;
                    break;

                case TurretType.Beam:
                    cost = BeamTurret.ResourceCost;
                    break;

                case TurretType.Freeze:
                    cost = FreezeTurret.ResourceCost;
                    break;

                case TurretType.Boomerang:
                    cost = BoomerangTurret.ResourceCost;
                    break;

                case TurretType.Grenade:
                    cost = GrenadeTurret.ResourceCost;
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
                    cost = FireTrap.ResourceCost;
                    break;

                case TrapType.Catapult:
                    cost = CatapultTrap.ResourceCost;
                    break;

                case TrapType.Ice:
                    cost = IceTrap.ResourceCost;
                    break;

                case TrapType.SawBlade:
                    cost = SawBladeTrap.ResourceCost;
                    break;

                case TrapType.Spikes:
                    cost = SpikesTrap.ResourceCost;
                    break;

                case TrapType.Wall:
                    cost = WallTrap.ResourceCost;
                    break;

                case TrapType.Barrel:
                    cost = BarrelTrap.ResourceCost;
                    break;
            }

            return cost;
        }
    }
}
