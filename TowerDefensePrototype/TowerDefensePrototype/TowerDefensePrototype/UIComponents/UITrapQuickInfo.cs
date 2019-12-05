using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class UITrapQuickInfo
    {
        public SpriteFont Italics, Font, BoldFont;

        Vector2 BoxSize, NameBoxSize, Position;

        public bool Visible;

        VertexPositionColor[] NameBoxVertices = new VertexPositionColor[4];
        Color NameBoxColor = Color.Lerp(Color.Lerp(Color.White, Color.DodgerBlue, 0.85f), Color.Transparent, 0.5f);

        VertexPositionColor[] WeaponBoxVertices = new VertexPositionColor[4];
        Color WeaponBoxColor = Color.Lerp(Color.Black, Color.Transparent, 0.5f);

        VertexPositionColor[] DividerVertices1 = new VertexPositionColor[4];
        VertexPositionColor[] DividerVertices2 = new VertexPositionColor[4];

        public PartitionedBar Detontations;

        UIBar HealthBar, TimingBar;

        int[] BoxIndices = new int[6];
        int[] NameBoxIndices = new int[6];

        int[] DividerIndices1 = new int[6];
        int[] DividerIndices2 = new int[6];

        public Trap Trap;

        Color RemoveTrapTextColor;

        float CurrentVisibilityDelay;

        //Info needed:
        //HP, cooldown time, detonations remaining, 
        //damage type, damage amount, quick description
        
        //Text:
        //Middle-click to remove trap - grayed out if it cannot be removed

        public UITrapQuickInfo(Vector2 position, Trap trap)
        {
            Visible = false;
            Trap = trap;
            Position = position;
            BoxSize = new Vector2(250, 150);
            NameBoxSize = new Vector2(250, 40);

            Detontations = new PartitionedBar(2, 5, new Vector2(BoxSize.X - 10, 12), new Vector2(Position.X + 5, Position.Y - 45));

            HealthBar = new UIBar(new Vector2(Position.X, Position.Y - BoxSize.Y + 5), new Vector2(BoxSize.X, 12), Color.White);
            TimingBar = new UIBar(new Vector2(Position.X, Position.Y - BoxSize.Y + 5 + 12 + 5), new Vector2(BoxSize.X, 12), Color.Red);
            

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

        public void Update(GameTime gameTime)
        {
            if (Trap.CurrentDetonateLimit < Trap.DetonateLimit)
            {
                RemoveTrapTextColor = Color.Gray;
            }
            else
            {
                RemoveTrapTextColor = Color.White;
            }

            if (CurrentVisibilityDelay < 500)
                CurrentVisibilityDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentVisibilityDelay > 500)
            {
                Visible = true;
            }

            Detontations.Update(Trap.CurrentDetonateLimit, Trap.DetonateLimit);
            HealthBar.Update(Trap.MaxHP, Trap.MaxHP);
            TimingBar.Update(Trap.DetonateDelay, Trap.CurrentDetonateDelay);
        }

        public void Draw(GraphicsDevice graphics, BasicEffect basicEffect, SpriteBatch spriteBatch)
        {
            if (Visible == true)
            {
                if (Trap.Active == true &&
                    Trap.CurrentHP > 0 &&
                    Trap.CurrentDetonateLimit > 0)
                {
                    Detontations.Draw(spriteBatch);

                    spriteBatch.DrawString(BoldFont, Trap.TrapType.ToString(),
                        new Vector2(Position.X + 4, Position.Y - BoxSize.Y - NameBoxSize.Y + 2), Color.White,
                        0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

                    spriteBatch.DrawString(Font, "Middle-click to remove this trap", new Vector2(Position.X + 5, Position.Y - 23),
                            RemoveTrapTextColor, 0, new Vector2(0, 0), 0.7f, SpriteEffects.None, 0);

                    //DRAW BARS HERE INSTEAD OF TEXT - Detontations.Draw(spriteBatch);

                    //spriteBatch.DrawString(Font, Trap.CurrentDetonateLimit + "/" + Trap.DetonateLimit + " detonations remaining",
                    //    new Vector2(Position.X + 5, Position.Y - 50), Color.White, 0, new Vector2(0, 0), 0.7f, SpriteEffects.None, 0);

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

                        HealthBar.Draw(graphics);
                        TimingBar.Draw(graphics);
                    }
                }
            }
        }
    }
}
