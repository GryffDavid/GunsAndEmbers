using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class WeaponBox
    {
        public Vector2 CurrentPosition, NextPosition, Scale, NextScale;
        public Rectangle SourceRectangle, DestinationRectangle;
        public Texture2D BoxTexture;
        public string Text;
        public SpriteFont Font;
        public Button InfoButton;
        public Color Color;

        public WeaponBox(Vector2 postion, Vector2 scale)
        {
            CurrentPosition = postion;
            NextPosition = postion;
            Color = Color.White;
            Scale = scale;
            NextScale = Scale;         
        }

        public void LoadContent(ContentManager contentManager)
        {
            BoxTexture = contentManager.Load<Texture2D>("WeaponBox");
        }

        public void Update()
        {
            if (NextPosition != CurrentPosition)
            {
                CurrentPosition = Vector2.Lerp(CurrentPosition, NextPosition, 0.15f);

                if (Math.Abs(CurrentPosition.X - NextPosition.X) < 0.5f)
                {
                    CurrentPosition.X = NextPosition.X;
                }
            }

            if (NextScale != Scale)
            {
                Scale = Vector2.Lerp(Scale, NextScale, 0.15f);
            }

            DestinationRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)(BoxTexture.Width * Scale.X), (int)(BoxTexture.Height * Scale.Y));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BoxTexture, DestinationRectangle, Color);
        }
    }
}