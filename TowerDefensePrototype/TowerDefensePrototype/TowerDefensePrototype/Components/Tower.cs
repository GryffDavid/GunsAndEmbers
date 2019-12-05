using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Tower : Drawable
    {
        public Texture2D Texture;
        string AssetName;
        public Vector2 Position;
        //public Rectangle DestinationRectangle;
        public BoundingBox BoundingBox;

        public float MaxHP, CurrentHP, Slots;
        public int MaxPowerUnits, CurrentPowerUnits;

        public Shield Shield;

        public Color Color;
        
        public Tower(string assetName, Vector2 position, int totalHitpoints, int maxShield, int slots, float shieldTime, int powerUnits)
        {
            AssetName = assetName;
            Position = position;
            MaxHP = totalHitpoints;
            CurrentHP = MaxHP;
            Slots = slots;

            Shield = new TowerDefensePrototype.Shield(300)
            { 
                CurrentShield = maxShield, 
                MaxShield = maxShield, 
                ShieldTime = shieldTime, 
                ShieldOn = true,
                CurrentRadius = 300,
                RechargeRate = 0.5f
            };

            Shield.Active = true;

            Color = Color.White;
            MaxPowerUnits = powerUnits;
            CurrentPowerUnits = powerUnits;


        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            BoundingBox = new BoundingBox(new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0),
                                          new Vector3(DestinationRectangle.Right, DestinationRectangle.Bottom, 0));

            //Shield.ShieldBoundingSphere = new BoundingSphere(new Vector3(DestinationRectangle.Center.X, DestinationRectangle.Center.Y, 0), 300);
            Shield.Position = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Center.Y);

            base.Initialize();
            vertices[0].TextureCoordinate = new Vector2(0, 0);
            vertices[1].TextureCoordinate = new Vector2(1, 0);
            vertices[2].TextureCoordinate = new Vector2(1, 1);
            vertices[3].TextureCoordinate = new Vector2(0, 1);

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;
        }

        public void Update(GameTime gameTime)
        {
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            Shield.Update(gameTime, new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Center.Y));
        }

        //public override void Draw(SpriteBatch spriteBatch)
        //{
        //    spriteBatch.Draw(Texture, DestinationRectangle, null, Color, 0, Vector2.Zero, SpriteEffects.None, 1);
        //    base.Draw(spriteBatch);
        //}

        //public override void DrawSpriteDepth(GraphicsDevice graphics, Effect effect)
        //{
        //    //if (Active == true)
        //    {
        //        effect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(0, 0, 0)));
        //        effect.Parameters["Texture"].SetValue(Texture);
        //        effect.Parameters["depth"].SetValue(DrawDepth);

        //        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        //        {
        //            pass.Apply();
        //            graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
        //        }
        //    }

        //    //base.DrawSpriteDepth(graphics, effect);
        //}

        public void Draw(SpriteBatch spriteBatch)
        {
            //DRAW SHADOW HERE
            spriteBatch.Draw(Texture, DestinationRectangle, null, Color, 0, Vector2.Zero, SpriteEffects.None, 1);
        }

        public void TakeDamage(float value)
        {
            CurrentHP += Shield.TakeDamage(value);
        }
    }
}
