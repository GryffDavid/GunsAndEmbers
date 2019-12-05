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
        public Vector2 Source, Destination, Tangent;
        public float Thickness, Alpha, AlphaMultiplier, FadeOutRate, Theta, ThicknessScale;
        public Color Color = Color.White;
        Vector2 MiddleScale, MiddleOrigin, CapOrigin;

        public BulletTrail(Vector2 source, Vector2 destination, float thickness = 1f)
        {
            Source = destination;
            Destination = source;
            Thickness = thickness;
            Alpha = 1f;
            AlphaMultiplier = 0.5f;
            FadeOutRate = 0.004f;

            Tangent = Destination - Source;
            Theta = (float)Math.Atan2(Tangent.Y, Tangent.X);
            const float ImageThickness = 8;
            ThicknessScale = Thickness / ImageThickness;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Segment = contentManager.Load<Texture2D>("Particles/Segment");
            Cap = contentManager.Load<Texture2D>("Particles/Cap");

            CapOrigin = new Vector2(Cap.Width, Cap.Height / 2);
            MiddleOrigin = new Vector2(0, Segment.Height / 2);
            MiddleScale = new Vector2(Tangent.Length(), ThicknessScale);
        }

        public void Update(GameTime gameTime)
        {
            Alpha -= FadeOutRate;            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Color = Color * (Alpha * AlphaMultiplier);     

            spriteBatch.Draw(Segment, Source, null, Color, Theta, MiddleOrigin, MiddleScale, SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(Cap, Source, null, Color, Theta, CapOrigin, ThicknessScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(Cap, Destination, null, Color, Theta + MathHelper.Pi, CapOrigin, ThicknessScale, SpriteEffects.None, 0f);
        }
    }
}
