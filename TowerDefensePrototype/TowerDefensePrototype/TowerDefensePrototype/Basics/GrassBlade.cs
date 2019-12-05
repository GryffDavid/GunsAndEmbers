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
        public Texture2D Base, Center, Tip;
        public Rectangle BaseRectangle, CenterRectangle, TipRectangle;

        public List<Vector2> Curve = new List<Vector2>();
        public List<Vector2> ThreePoints = new List<Vector2>();

        float BaseLength, CenterLength, TipLength, DrawDepth;

        public GrassBlade(Vector2 basePosition, Vector2 tipPosition, Vector2 controlPoint, float baseWidth)
        {
            #region Set up the 3 points of the Bezier curve
            //The base of the blade
            ThreePoints.Add(new Vector2(basePosition.X, basePosition.Y));
            //Control point
            ThreePoints.Add(new Vector2(basePosition.X + controlPoint.X, basePosition.Y + controlPoint.Y));
            //The tip of the blade
            ThreePoints.Add(new Vector2(basePosition.X + tipPosition.X, basePosition.Y + tipPosition.Y));
            #endregion

            #region Set up the actual curve
            Curve.Add(ThreePoints[0]);

            for (float t = 0.33f; t < 0.9; t += 0.33f)
            {
                Vector2 newPoint = new Vector2(
                    (float)Math.Pow(1 - t, 2) * ThreePoints[0].X + (2 * t * (1 - t)) * (ThreePoints[1].X) + (float)Math.Pow(t, 2) * ThreePoints[2].X,
                    (float)Math.Pow(1 - t, 2) * ThreePoints[0].Y + (2 * t * (1 - t)) * (ThreePoints[1].Y) + (float)Math.Pow(t, 2) * ThreePoints[2].Y);

                Curve.Add(newPoint);
            }

            Curve.Add(ThreePoints[2]);
            #endregion

            DrawDepth = ThreePoints[0].Y / 1080;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Base = contentManager.Load<Texture2D>("GrassBase2");
            Center = contentManager.Load<Texture2D>("GrassCenter2");
            Tip = contentManager.Load<Texture2D>("GrassTip2");
        }

        public void Update(GameTime gameTime)
        {
            BaseLength = Vector2.Distance(Curve[0], Curve[1]);
            BaseRectangle = new Rectangle((int)Curve[0].X, (int)Curve[0].Y, (int)BaseLength, Math.Min((int)(Base.Height / (Base.Width / BaseLength)), Base.Height));


            CenterLength = Vector2.Distance(Curve[1], Curve[2]);
            CenterRectangle = new Rectangle((int)Curve[1].X, (int)(Curve[1].Y + (CenterLength / 10)), (int)CenterLength, Math.Min((int)(Center.Height / (Center.Width / CenterLength)), Center.Height));

            TipLength = Vector2.Distance(Curve[2], Curve[3]);
            TipRectangle = new Rectangle((int)Curve[2].X, (int)(Curve[2].Y + (TipLength / 10)), (int)TipLength, Math.Min((int)(Tip.Height / (Tip.Width / TipLength)), Tip.Height));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 BaseDirection = Curve[0] - Curve[1];
            BaseDirection.Normalize();
            float BaseRotation = (float)(Math.Atan2(BaseDirection.Y, BaseDirection.X));


            spriteBatch.Draw(Base, BaseRectangle, null, Color.White, BaseRotation,
                             new Vector2(Base.Width, Base.Height / 2), SpriteEffects.None, DrawDepth);

            Vector2 CenterDirection = Curve[1] - Curve[2];
            CenterDirection.Normalize();
            float CenterRotation = (float)(Math.Atan2(CenterDirection.Y, CenterDirection.X));

            spriteBatch.Draw(Center, CenterRectangle, null, Color.White, CenterRotation,
                             new Vector2(Center.Width, Center.Height / 2), SpriteEffects.None, DrawDepth);

            Vector2 TipDirection = Curve[2] - Curve[3];
            TipDirection.Normalize();
            float TipRotation = (float)(Math.Atan2(TipDirection.Y, TipDirection.X));

            spriteBatch.Draw(Tip, TipRectangle, null, Color.White, TipRotation,
                             new Vector2(Tip.Width, Tip.Height / 2), SpriteEffects.None, DrawDepth);
        }
    }
}
