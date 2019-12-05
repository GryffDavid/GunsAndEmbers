using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class UserInterface
    {
        Texture2D Texture;
        string AssetName;
        Vector2 Position, Scale;
        Color Color;
        Rectangle DestinationRectangle;
        public List<Button> ButtonList;

        public UserInterface(string assetName, Vector2 position, Vector2? scale = null, Color? color = null)
        {
            AssetName = assetName;
            Position = position;

            if (scale == null)
                Scale = new Vector2(1, 1);
            else
                Scale = scale.Value;

            if (color == null)
                Color = Color.White;
            else
                Color = color.Value;

            ButtonList = new List<Button>();

            for (int i = 0; i < 8; i++)
            {
                ButtonList.Add(new Button("Button", new Vector2(16 + i * (128 + 32), 720 - 16 - 128)));
            }
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
            foreach (Button button in ButtonList)
            {
                button.LoadContent(contentManager);
            }
        }

        public void Update(GameTime gameTime)
        {
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            foreach (Button button in ButtonList)
            {
                button.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color);

            foreach (Button button in ButtonList)
            {
                button.Draw(spriteBatch);
            }
        }
    }
}
