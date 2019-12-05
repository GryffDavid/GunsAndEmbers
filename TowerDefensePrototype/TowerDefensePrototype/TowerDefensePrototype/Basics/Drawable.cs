using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public abstract class Drawable
    {
        public bool Active;
        public float DrawDepth;
        public float PreviousMaxY;

        private float _MaxY;

        public float MaxY
        {
            get { return _MaxY; }
            set 
            {
                if (_MaxY != PreviousMaxY)
                {
                    PreviousMaxY = _MaxY;
                }

                _MaxY = value;
                DrawDepth = MaxY / 1080.0f;
            }
        }
        
        public BlendState BlendState = BlendState.AlphaBlend;
        public BoundingBox CollisionBox;        
        public BoundingBox BoundingBox;        
        public BoundingSphere BoundingSphere;
        public float ZDepth = 16f; //The theoretical physical depth of the object in the non-existant Z dimension. Not DrawDepth though.
        public bool Emissive = false;
        public bool Normal = false;
        public bool Shadows = false;
        
        

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }

        public virtual void Draw(GraphicsDevice graphics, Effect effect)
        {

        }

        public virtual void Draw(GraphicsDevice graphics, BasicEffect effect)
        {

        }
        

        /// <summary>
        /// For invaders, traps etc. that needs to cast shadows from lights
        /// </summary>
        /// <param name="graphics">Graphics Device</param>
        /// <param name="effect">Basic Effect for drawing the actual sprite</param>
        /// <param name="shadowEffect">The shadow effect for blurring the shadow</param>
        /// <param name="lightList">The list of lights from which to cast shadows</param>
        public virtual void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect, List<Light> lightList)
        { 

        }

        /// <summary>
        /// For drawing Heavy Projectiles - don't need to cast shadows from lights.
        /// </summary>
        /// <param name="graphics">Graphics Device</param>
        /// <param name="effect">Basic effect for drawing the actual sprite</param>
        /// <param name="shadowEffect">The shadow effect for blurring the shadow</param>
        public virtual void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect, SpriteBatch spriteBatch)
        {

        }


        /// <summary>
        /// Draw the depth of the sprite - reduce to single byte value to draw grey
        /// </summary>
        /// <param name="graphics">Graphics Device</param>
        /// <param name="effect">The effect that draws the sprite as a grey value</param>
        public virtual void DrawSpriteDepth(GraphicsDevice graphics, Effect effect)
        {

        }
        
        public virtual void DrawSpriteNormal(GraphicsDevice graphics, BasicEffect basicEffect)
        {

        }
    }
}
