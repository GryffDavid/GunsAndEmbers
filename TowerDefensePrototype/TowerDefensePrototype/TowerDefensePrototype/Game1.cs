using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TowerDefensePrototype
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Tower Tower;
        UserInterface UserInterface;
        StaticSprite Ground;
        TrapButtons TrapButtons;
        SpriteFont ResourceFont;
        int Resources;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Resources = 100;
            Tower = new Tower("Tower", "TowerButton", new Vector2(32, 720-160-256-30), 100, 1);
            UserInterface = new UserInterface("UI", new Vector2(0,720-160));
            Ground = new StaticSprite("Ground", new Vector2(0, 720-160-48));
            TrapButtons = new TrapButtons("TrapButton", new Vector2(0, 720 - 160-32),2);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Tower.LoadContent(Content);
            UserInterface.LoadContent(Content);
            Ground.LoadContent(Content);
            TrapButtons.LoadContent(Content);
            ResourceFont = Content.Load<SpriteFont>("ResourceFont");
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            Tower.Update(gameTime);
            UserInterface.Update(gameTime);
            TrapButtons.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
                spriteBatch.DrawString(ResourceFont, "Resources: " + Resources.ToString(), new Vector2(32, 32), Color.White);
                Tower.Draw(spriteBatch);
                Ground.Draw(spriteBatch);
                UserInterface.Draw(spriteBatch);
                TrapButtons.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
