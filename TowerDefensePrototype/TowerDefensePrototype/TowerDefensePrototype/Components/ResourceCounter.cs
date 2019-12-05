using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class ResourceChange
    {
        public Vector2 Position, Velocity, NextPosition, StringSize;
        public string Text;
        public float CurrentTime, MaxTime;
    }

    class ResourceCounter
    {
        Vector2 StartPosition = new Vector2(489, 1000);
        public SpriteFont Font;
        public List<ResourceChange> Changes = new List<ResourceChange>();
        public Texture2D ResourceIcon;        

        public ResourceCounter()
        {

        }

        public void Update(GameTime gameTime)
        {
            foreach (ResourceChange change in Changes)
            {
                if (change.Position != change.NextPosition)
                {
                    change.Position = Vector2.Lerp(change.Position, change.NextPosition, 0.2f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                }

                change.CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            Changes.RemoveAll(Change => Change.CurrentTime >= Change.MaxTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = Changes.Count-1; i >= 0; i--)
            {
                //float ColorFade = ((100f / Changes[i].MaxTime) * Changes[i].CurrentTime)/100f;
                //spriteBatch.DrawString(Font, Changes[i].Text, Changes[i].Position, Color.White);

                spriteBatch.DrawString(Font, Changes[i].Text, new Vector2(Changes[i].Position.X - Changes[i].StringSize.X - 4, Changes[i].Position.Y - 4), Color.White);
                spriteBatch.Draw(ResourceIcon, new Rectangle((int)Changes[i].Position.X, (int)Changes[i].Position.Y, 16, 16), Color.White);
            }
        }

        public void AddChange(int diff)
        {
            string Diff = diff.ToString();

            if (diff > 0)
            {
                Diff = Diff.Insert(0, "+");   
            }

            for (int i = 0; i < Changes.Count; i++)
            {
                if (i > 0)
                {
                    Changes[i].NextPosition = Changes[i - 1].NextPosition - new Vector2(0, 24);
                    Changes[i].MaxTime = (5000 - (i * 1000));
                    Changes[i].CurrentTime = 0;
                }
                else
                {
                    Changes[i].NextPosition = Changes[i].Position - new Vector2(0, 24);
                    Changes[i].MaxTime = 5000;
                    Changes[i].CurrentTime = 0;
                }
            }

            Changes.Insert(0, 
                new ResourceChange()
                {
                    MaxTime = 5000f,
                    CurrentTime = 0f,
                    Position = StartPosition,
                    NextPosition = StartPosition, 
                    Text = Diff,
                    Velocity = new Vector2(0, -0.2f),
                    StringSize = Font.MeasureString(Diff)
                });
        }
    }
}
