using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class VictoryScreen
    {
        StaticSprite Box;
        public Button Retry, Complete, Return;
        float ShotsFired, ShotsHit, Accuracy, TotalDamage;
        public bool Victory;

        public VictoryScreen(bool victory)
        {
            Victory = victory;

            Box = new StaticSprite("VictoryBox", new Vector2(1280/2 - (750/2), 720/2 - (500/2)), new Vector2(1, 1));

            Retry = new Button("Buttons/ButtonLeft", new Vector2(-225+100, Box.Position.Y + 500 - 32 - 48), null, new Vector2(0.5f, 1), null, "Retry", "Fonts/ButtonFont");
            Retry.NextScale.X = 0.5f;
            Retry.NextPosition.X = 265;

            if (Victory == true)
            {
                Complete = new Button("Buttons/ButtonRight", new Vector2(1280 + 225 - 100, Box.Position.Y + 500 - 32 - 48), null, new Vector2(0.5f, 1), null, "Complete", "Fonts/ButtonFont", "Right");
                Complete.NextScale.X = 0.5f;
                Complete.NextPosition.X = 1280 - 265 - 225;
            }

            if (Victory == false)
            {
                Return = new Button("Buttons/ButtonRight", new Vector2(1280 + 225 - 100, Box.Position.Y + 500 - 32 - 48), null, new Vector2(0.5f, 1), null, "Return", "Fonts/ButtonFont", "Right");
                Return.NextScale.X = 0.5f;
                Return.NextPosition.X = 1280 - 265 - 225;
            }
        }

        public void LoadContent(ContentManager contentManager)
        {
            Box.LoadContent(contentManager);
            Retry.LoadContent(contentManager);  
   
            if (Victory == true)
            Complete.LoadContent(contentManager);
            else
            Return.LoadContent(contentManager);
        }

        public void Update()
        {
            Retry.Update();
            
            if (Victory == true)
            Complete.Update();
            else
            Return.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Box.Draw(spriteBatch);
            Retry.Draw(spriteBatch);

            if (Victory == true)
            Complete.Draw(spriteBatch);
            else
            Return.Draw(spriteBatch);
        }
    }
}
