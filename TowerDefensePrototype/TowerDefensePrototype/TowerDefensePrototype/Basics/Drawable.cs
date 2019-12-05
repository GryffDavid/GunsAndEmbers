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
        public GraphicsType GraphicsType = GraphicsType.Diffuse;

        public static Random Random = new Random();

        public bool Active;
        public float DrawDepth, PreviousMaxY;

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

        public Texture2D Texture;
        public Rectangle DestinationRectangle, SourceRectangle;
        public Color Color = Color.White;
        public Vector2 Position;

        public ShadowCaster ShadowCaster;

        public VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
        public int[] indices = new int[6];

        public TexCoord DiffuseCoords, NormalCoords, EmissiveCoords;
        public Rectangle DiffuseSourceRectangle, NormalSourceRectangle, EmissiveSourceRectangle;


        public virtual void Initialize()
        {
            #region Vertex Declarations
            vertices[0] = new VertexPositionColorTexture()
               {
                   Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0),
                   TextureCoordinate = new Vector2(0, 0),
                   Color = Color
               };

            vertices[1] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0),
                TextureCoordinate = new Vector2(1, 0),
                Color = Color
            };

            vertices[2] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(1, 1),
                Color = Color
            };

            vertices[3] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(0, 1),
                Color = Color
            };

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;
            #endregion

            switch (GraphicsType)
            {
                case GraphicsType.Diffuse:
                    {
                        Emissive = false;
                        Normal = false;
                    }
                    break;

                case GraphicsType.Normal:
                    {
                        Emissive = false;
                        Normal = true;

                        #region Normal
                        
                        #endregion
                    }
                    break;

                case GraphicsType.Emissive:
                    {
                        Emissive = true;
                        Normal = false;

                        #region Emissive

                        #endregion
                    }
                    break;
                    
                case GraphicsType.NormalEmissive:
                    {
                        Emissive = true;
                        Normal = true;

                        #region Normal
                        
                        #endregion

                        #region Emissive
                        
                        #endregion
                    }
                    break;
            }
            
        }

        
        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }

        public virtual void Draw(GraphicsDevice graphics, Effect effect)
        {            
            effect.Parameters["Texture"].SetValue(Texture);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
            }
        }

        public virtual void Draw(GraphicsDevice graphics, BasicEffect effect)
        {
            effect.Texture = Texture;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
            }
        }

        

        /// <summary>
        /// For invaders, traps etc. that needs to cast shadows from lights
        /// </summary>
        /// <param name="graphics">Graphics Device</param>
        /// <param name="effect">Basic Effect for drawing the actual sprite</param>
        /// <param name="shadowEffect">The shadow effect for blurring the shadow</param>
        /// <param name="lightList">The list of lights from which to cast shadows</param>
        public virtual void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect)
        {
            effect.TextureEnabled = true;
            effect.VertexColorEnabled = true;
            effect.Texture = Texture;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
            }
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

        public virtual void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect, Effect particleEffect)
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

        public virtual void DrawSpriteNormal(SpriteBatch spriteBatch)
        {

        }

        public virtual void DrawSpriteOcclusion(GraphicsDevice graphics, BasicEffect effect)
        {
            effect.TextureEnabled = true;
            effect.VertexColorEnabled = true;
            effect.Texture = Texture;

            for (int i = 0; i < 4; i++)
            {
                vertices[i].Color = Color.Black;
            }

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
            }

            for (int i = 0; i < 4; i++)
            {
                vertices[i].Color = Color;
            }
        }

        public virtual void DrawSpriteEmissve(GraphicsDevice graphics, BasicEffect effect)
        {

        }
    }
}
