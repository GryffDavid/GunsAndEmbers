using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Level2 : Level
    {
        public Level2()
        {
            Number = 2;
            WaveList = new List<Wave>();
            WorldType = WorldType.Snowy;
            StartWeather = Weather.Snow;
            Resources = 1300;

            Wave Wave1 = new Wave(false, 800, 5000,
                new Soldier(new Vector2(2050, 400)),
                new Soldier(new Vector2(2050, 400)),
                //new Spider(new Vector2(2050, 400)),
                //new Soldier(new Vector2(2050, 400)),
                //new Soldier(new Vector2(2050, 400)),
                //new Spider(new Vector2(2050, 400)),
                //new Soldier(new Vector2(2050, 400)),
                //new Soldier(new Vector2(2050, 400)),
                //new Spider(new Vector2(2050, 400)),
                //new Soldier(new Vector2(2050, 400)),
                //new Soldier(new Vector2(2050, 400)),
                //new Soldier(new Vector2(2050, 400)),
                //new Soldier(new Vector2(2050, 400)),
                //new Soldier(new Vector2(2050, 400)),
                //new Spider(new Vector2(2050, 400)),
                //new Spider(new Vector2(2050, 400)),
                //new Spider(new Vector2(2050, 400)),
                //new Spider(new Vector2(2050, 400)),
                //new Spider(new Vector2(2050, 400)),
                //new Spider(new Vector2(2050, 400))
                new Soldier(new Vector2(2050, 400)));

            WaveList.Add(Wave1);
        }

        public override void LoadContent(ContentManager contentManager)
        {
            GroundTexture = contentManager.Load<Texture2D>("Backgrounds/Ground");
            ForegroundTexture = contentManager.Load<Texture2D>("Backgrounds/Foreground");
            SkyBackgroundTexture = contentManager.Load<Texture2D>("Backgrounds/Sky");
        }
    }
}
