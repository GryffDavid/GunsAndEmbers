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
        Vector2 MiddleScale, MiddleOrigin, CapOrigin;

        public BulletTrail(Vector2 source, Vector2 destination, float thickness = 1f)
        {
            Source = destination;
            Destination = source;
            Thickness = thickness;
            Alpha = 1f;
            AlphaMultiplier = 0.5f;
            FadeOutRate = 0.4f;
            Direction = Destination - Source;
            Angle = (float)Math.Atan2(Direction.Y, Direction.X);
            const float ImageThickness = 8;
            ThicknessScale = Thickness / ImageThickness;
            Color = Color.Lerp(Color, Color.Transparent, AlphaMultiplier);
            Color.A = 255;
        }

        public void LoadContent(ContentManager contentManager)
        {

        }

        public void Update(GameTime gameTime)
        {
            Color = Color.Lerp(Color, Color.Transparent, FadeOutRate * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Segment, Source, null, Color, Angle, MiddleOrigin, MiddleScale, SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(Cap, Source, null, Color, Angle, CapOrigin, ThicknessScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(Cap, Destination, null, Color, Angle + MathHelper.Pi, CapOrigin, ThicknessScale, SpriteEffects.None, 0f);
        }

        public void SetUp()
        {
            //This is here so that I don't have to call LoadContent
            //It gets called after Cap and Segment are set using preloaded textures
            CapOrigin = new Vector2(Cap.Width, Cap.Height / 2);
            MiddleOrigin = new Vector2(0, Segment.Height / 2);
            MiddleScale = new Vector2(Direction.Length(), ThicknessScale);
        }
    }
}
