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
    class GrassBlade
    {
        public Texture2D BladeTexture;
        Rectangle DestinationRectangle;
        Vector2 BladeSize, Origin;
        float DrawDepth, CurrentRotation, OriginalRotation;
        static Random Random = new Random();
        SpriteEffects CurrentOrientation;

        public GrassBlade(Texture2D bladeTexture, Vector2 originPosition, Vector2 size)
        {
            BladeTexture = bladeTexture;
            BladeSize = size;
            Origin = originPosition;
            DestinationRectangle = new Rectangle((int)(originPosition.X - BladeSize.X),
                                                 (int)(originPosition.Y - BladeSize.Y),
                                                 (int)BladeSize.X, (int)BladeSize.Y);

            DrawDepth = DestinationRectangle.Bottom / 1080f;
            CurrentOrientation = RandomOrientation(SpriteEffects.None, SpriteEffects.FlipHorizontally);
            CurrentRotation = Random.Next(-15, 15);
        }

        public void Update(GameTime gameTime)
        {
            //CurrentRotation += 0.1f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BladeTexture, DestinationRectangle, 
                new Rectangle(0, 0, BladeTexture.Width, BladeTexture.Height), 
                Color.White, MathHelper.ToRadians(CurrentRotation),
                new Vector2(0, 0), 
                CurrentOrientation, DrawDepth);

            //spriteBatch.Draw(BladeTexture, new Rectangle((int)Origin.X - 10, (int)(DrawDepth * 1080f), 25, 5), Color.Red);
        }

        private SpriteEffects RandomOrientation(params SpriteEffects[] Orientations)
        {
            List<SpriteEffects> OrientationList = new List<SpriteEffects>();

            foreach (SpriteEffects orientation in Orientations)
            {
                OrientationList.Add(orientation);
            }

            return OrientationList[Random.Next(0, OrientationList.Count)];
        }
    }
}
