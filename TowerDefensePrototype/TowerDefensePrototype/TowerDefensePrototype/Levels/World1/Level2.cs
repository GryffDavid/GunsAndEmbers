using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using GameDataTypes;

namespace TowerDefensePrototype
{
    public class Level2 : Level
    {        
        public Level2(Game1 game)
        {
            //DialogueItems = new StoryDialogueItems();
            LevelDialogue = new Level2Dialogue(game);
            Number = 2;
            WaveList = new List<Wave>();
            WorldType = WorldType.Snowy;
            StartWeather = Weather.Snow;
            Resources = 4000;
                        
            //A float in the middle of the list changes the delay between invaders
            //An int in the middle of the list creates a pause without changing the timing
            #region Wave1
            Wave Wave1 = new Wave(true, 350, 100,
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850))
                 );            
            #endregion

            #region Wave2
            Wave Wave2 = new Wave(true, 350, 100,
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                4000,
                new DropShip(new Vector2(2050, 300), new Vector2(300, 300),
                        new BatteringRam(new Vector2(1800, 0), new Vector2(700, 850)),
                        new BatteringRam(new Vector2(1800, 0), new Vector2(700, 850))
                        ),
                3000,
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850))
                 );
            #endregion

            #region Wave2
            Wave Wave3 = new Wave(false, 350, 100,
                new DropShip(new Vector2(2050, 300), new Vector2(300, 300),
                        new BatteringRam(new Vector2(1800, 0), new Vector2(700, 850)),
                        new BatteringRam(new Vector2(1800, 0), new Vector2(700, 850)),
                        new BatteringRam(new Vector2(1800, 0), new Vector2(700, 850)),
                        new BatteringRam(new Vector2(1800, 0), new Vector2(700, 850)),
                        new BatteringRam(new Vector2(1800, 0), new Vector2(700, 850))
                        ),
                7000,
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),

                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),

                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),

                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),

                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),

                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)), 
                8000,
                150f,
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850))
                 );
            #endregion

            WaveList.Add(Wave1);
            WaveList.Add(Wave2);
            WaveList.Add(Wave3);
        }

        public override void LoadContent(ContentManager contentManager)
        {
            StoryDialogueItems DialogueItems = contentManager.Load<StoryDialogueItems>("StoryDialogue/Level2Dialogue");
            LevelDialogue.ItemsList = DialogueItems.DialogueItems;

            LevelDialogue.TutorialMarker = new ButtonMarker(new Vector2(100, 100), contentManager.Load<Texture2D>("WhiteBlock"));

            LevelDialogue.DialogueBox = new StoryDialogueBox();
            LevelDialogue.DialogueBox.BoxTexture = contentManager.Load<Texture2D>("WhiteBlock");
            LevelDialogue.DialogueBox.DialogueFont = contentManager.Load<SpriteFont>("Fonts/RobotoBold20_0_Outline");
            LevelDialogue.DialogueBox.TipFont = contentManager.Load<SpriteFont>("Fonts/RobotoLight20_2");
       
            GroundTexture = contentManager.Load<Texture2D>("Backgrounds/Ground");
            ForegroundTexture = contentManager.Load<Texture2D>("Backgrounds/Foreground");
            SkyBackgroundTexture = contentManager.Load<Texture2D>("Backgrounds/Sky");
        }        
    }
}
