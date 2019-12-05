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
        public SpriteFont Italics, Font, BoldFont;
        Vector2 BoxSize, NameBoxSize, Position;
        UIBar RateOfFire, Impact, Range, Response, Reload;

        Texture2D DamageIcon, WeaponIcon;

        string WeaponDescription, WeaponTip, WeaponName;
        bool Visible;
        
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

        public UIWeaponInfoTip(Vector2 position, TurretType? turret = null, TrapType? trap = null)
        {
            Visible = false;
            Position = position;
            BoxSize = new Vector2(250, 330);
            NameBoxSize = new Vector2(250, 40);

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
                Color = NameBoxColor
            };

            DividerVertices1[1] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + 5, Position.Y - 52, 0),
                Color = NameBoxColor
            };

            DividerVertices1[2] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X - 5, Position.Y - 50, 0),
                Color = NameBoxColor
            };

            DividerVertices1[3] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X - 5, Position.Y - 52, 0),
                Color = NameBoxColor
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
                Color = NameBoxColor
            };

            DividerVertices2[1] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + 5, Position.Y - BoxSize.Y + 45, 0),
                Color = NameBoxColor
            };

            DividerVertices2[2] =
            new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X - 5, Position.Y - BoxSize.Y + 47, 0),
                Color = NameBoxColor
            };

            DividerVertices2[3] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X + NameBoxSize.X - 5, Position.Y - BoxSize.Y + 45, 0),
                Color = NameBoxColor
            };

            DividerIndices2[0] = 1;
            DividerIndices2[1] = 2;
            DividerIndices2[2] = 0;
            DividerIndices2[3] = 1;
            DividerIndices2[4] = 3;
            DividerIndices2[5] = 2;
            #endregion

            WeaponName = "Donar";
        }

        public void SetUpBars()
        {
            RateOfFire = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 60),
                                   new Vector2(BoxSize.X - 125, 15), Color.White, false);

            Impact = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 85),
                                   new Vector2(BoxSize.X - 125, 15), Color.White, false);

            Range = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 110),
                                   new Vector2(BoxSize.X - 125, 15), Color.White, false);

            Response = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 135),
                                   new Vector2(BoxSize.X - 125, 15), Color.White, false);

            Reload = new UIBar(new Vector2(Position.X + 105, Position.Y - BoxSize.Y + 160),
                                   new Vector2(BoxSize.X - 125, 15), Color.White, false);

            RateOfFire.Update(10, 2);
            Impact.Update(10, 3);
            Range.Update(10, 6);
            Response.Update(10, 8);
            Reload.Update(10, 4);
        }

        public void Update(GameTime gameTime)
        {
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            UpdateQuads();
            SetUpBars();
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect basicEffect)
        {
            if (Visible == true)
            {
                //Draw the weapon name
                spriteBatch.DrawString(BoldFont, WeaponName, new Vector2(Position.X + 5, Position.Y - BoxSize.Y - NameBoxSize.Y + 2),
                    Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

                //Draw the weapon type
                spriteBatch.DrawString(Font, "Machine Gun", new Vector2(Position.X + 6, Position.Y - BoxSize.Y - NameBoxSize.Y + 23),
                    Color.White, 0, new Vector2(0, 0), 0.7f, SpriteEffects.None, 0);

                //Draw the weapon damage
                spriteBatch.DrawString(BoldFont, "248", new Vector2(Position.X + 5, Position.Y - BoxSize.Y + 2),
                    Color.DodgerBlue, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                //Draw the weapon tip, e.g. "Fire into a closely packed crowed for maximum carnage"
                spriteBatch.DrawString(Italics, "Place on lower tower slots for \ndevestation at close range.", new Vector2(Position.X + 5, Position.Y - 35),
                    Color.White, 0, new Vector2(0, 0), 0.7f, SpriteEffects.None, 0);

                //Draw the resource cost of the weapon
                //*****CHANGE THIS COLOUR TO GRAY IF THE PLAYER CANNOT AFFORD THE WEAPON*****//
                spriteBatch.DrawString(Font, "1400", new Vector2(Position.X + BoxSize.X - Font.MeasureString("1400").X - 5, Position.Y - 75),
                    Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                #region Draw the names of the bars
                Vector2 rateOfFireSize = Font.MeasureString("Rate of Fire");
                spriteBatch.DrawString(Font, "Rate of Fire", new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 60),
                    Color.White, 0, new Vector2(rateOfFireSize.X, 0), 0.8f, SpriteEffects.None, 0);

                Vector2 impactSize = Font.MeasureString("Impact");
                spriteBatch.DrawString(Font, "Impact", new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 85),
                    Color.White, 0, new Vector2(impactSize.X, 0), 0.8f, SpriteEffects.None, 0);

                Vector2 rangeSize = Font.MeasureString("Range");
                spriteBatch.DrawString(Font, "Range", new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 110),
                    Color.White, 0, new Vector2(rangeSize.X, 0), 0.8f, SpriteEffects.None, 0);

                Vector2 ResponseSize = Font.MeasureString("Response");
                spriteBatch.DrawString(Font, "Response", new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 135),
                    Color.White, 0, new Vector2(ResponseSize.X, 0), 0.8f, SpriteEffects.None, 0);

                Vector2 reloadSize = Font.MeasureString("Reload");
                spriteBatch.DrawString(Font, "Reload", new Vector2(Position.X + 100, Position.Y - BoxSize.Y + 160),
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


                    RateOfFire.Draw(graphics);
                    Impact.Draw(graphics);
                    Range.Draw(graphics);
                    Response.Draw(graphics);
                    Reload.Draw(graphics);
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
    }
}
