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
    public class Level1 : Level
    {        
        public Level1(Game1 game)
        {
            //DialogueItems = new StoryDialogueItems();
            LevelDialogue = new Level1Dialogue(game);
            Number = 1;
            WaveList = new List<Wave>();
            WorldType = WorldType.Snowy;
            StartWeather = Weather.Snow;
            Resources = 200;
                        
            //A float in the middle of the list changes the delay between invaders
            //An int in the middle of the list creates a pause without changing the timing
            #region Wave1
            Wave Wave1 = new Wave(false, 350, 100,

                    //new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                //new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                //new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                //new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                //new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                ////new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                ////3500,
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850))
                //new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                //new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                //new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                //new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                //new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                //new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                //new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                //new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),

                    //new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),

                    //2000,

                    //new BatteringRam(new Vector2(2050, 600), new Vector2(700, 850)),
                //new BatteringRam(new Vector2(2050, 600), new Vector2(700, 850)),

                    //5000,

                    //new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HealDrone(new Vector2(2050, 600), new Vector2(700, 850))//,

                //new GunShip(new Vector2(2050, 600), new Vector2(700, 850))
                //new DropShip(new Vector2(2050, 600), new Vector2(700, 850))
                 );            
            #endregion

            #region Wave2
            Wave Wave2 = new Wave(false, 350, 100,
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)));

            #endregion

            WaveList.Add(Wave1);
            WaveList.Add(Wave2);
        }

        public override void LoadContent(ContentManager contentManager)
        {
            StoryDialogueItems DialogueItems = contentManager.Load<StoryDialogueItems>("StoryDialogue/Level1Dialogue");
            LevelDialogue.ItemsList = DialogueItems.DialogueItems;

            //DialogueItems = contentManager.Load<StoryDialogueItems>("StoryDialogue/Level1Dialogue");
            //LevelDialogue = contentManager.Load<LevelDialogue>("StoryDialogue/StoryDialogue1");            
            GroundTexture = contentManager.Load<Texture2D>("Backgrounds/Ground");
            ForegroundTexture = contentManager.Load<Texture2D>("Backgrounds/Foreground");
            SkyBackgroundTexture = contentManager.Load<Texture2D>("Backgrounds/Sky");

            //AmbienceList = new List<SoundEffect>()
            //{
            //    contentManager.Load<SoundEffect>("Sounds/Ambience/PolarWindAmbience")
            //};
        }        
    }
}
