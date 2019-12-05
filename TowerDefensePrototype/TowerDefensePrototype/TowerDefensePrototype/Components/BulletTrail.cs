using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class BulletTrail
    {
        public Texture2D Segment, Cap;
        public Vector2 Source, Destination, Direction;
        public float Thickness, Alpha, AlphaMultiplier, FadeOutRate, Angle, ThicknessScale;
        public Color Color = Color.White;
        public Vector2 MiddleScale, MiddleOrigin, CapOrigin;
        public Turret SourceTurret;
        float length;
        bool drawn = false;

        public BulletTrail(Vector2 source, Vector2 destination, float thickness = 1f)
        {
            Source = destination;
            Destination = source;
            Thickness = thickness;
            Alpha = 1f;
            AlphaMultiplier = 0.5f;
            //FadeOutRate = 0.015f;
            FadeOutRate = 0.4f;
            Direction = Destination - Source;
            //length = Direction.Length();
            //Direction.Normalize();
            Angle = (float)Math.Atan2(Direction.Y, Direction.X);
            const float ImageThickness = 8;
            ThicknessScale = Thickness / ImageThickness;
            Color = Color.Lerp(Color, Color.Transparent, AlphaMultiplier);
            Color.A = 255;
        }

        public void Update(GameTime gameTime)
        {
            Color = Color.Lerp(Color, Color.Transparent, FadeOutRate * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));
            //MiddleScale.X -= (length/10) * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);

            //MiddleScale.X = MathHelper.SmoothStep(MiddleScale.X, 0, 0.3f);
            //if (drawn == true)
            //{
            //    MiddleScale.X -= (length / 5f);
            //    MiddleScale.X = MathHelper.Clamp(MiddleScale.X, 0, length);
            //}
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //drawn = true;
            spriteBatch.Draw(Segment, Source, null, Color, Angle, MiddleOrigin, MiddleScale, SpriteEffects.None, 0f);

            //spriteBatch.Draw(Segment, new Rectangle((int)Destination.X, (int)Destination.Y, (int)MiddleScale.X, (int)MiddleScale.Y), null,
            //    Color.White, Angle, new Vector2(0, Segment.Height/2), SpriteEffects.None, 0);

            //spriteBatch.Draw(Cap, Source, null, Color, Angle, CapOrigin, ThicknessScale, SpriteEffects.None, 0f);
            //spriteBatch.Draw(Cap, Destination, null, Color, Angle + MathHelper.Pi, CapOrigin, ThicknessScale, SpriteEffects.None, 0f);
        }

        public void SetUp()
        {
            //This is here so that I don't have to call LoadContent
            //It gets called after Cap and Segment are set using preloaded textures
            CapOrigin = new Vector2(Cap.Width, Cap.Height / 2);
            MiddleOrigin = new Vector2(0, Segment.Height / 2);
            //Vector2 len = Destination - Source;
            MiddleScale = new Vector2(Direction.Length(), ThicknessScale);
            //length = MiddleScale.X;
        }
    }
}
