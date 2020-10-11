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
            //LevelDialogue = new Level1Dialogue(game);
            Number = 1;
            WaveList = new List<Wave>();
            WorldType = WorldType.Snowy;
            StartWeather = Weather.Snow;
            Resources = 20000;
                        
            //A float in the middle of the list changes the delay between invaders
            //An int in the middle of the list creates a pause without changing the timing
            //THE OVERFLOW BOOL VARIABLE NEEDS TO GO IN THE CURRENT WAVE TO CREATE OVERFLOW, NOT THE NEXT WAVE.
            //WAVE 2 WILL NOT START IF WAVE 1 HAS A FALSE OVERFLOW BUT WAVE 2 HAS A TRUE OVERFLOW.
            //KEEPS CATCHING ME OUT AND I END UP DEBUGGING IT OVER AND OVER AGAIN
            #region TestWave
            Wave TestWave = new Wave(false, 350, 100,

                 //new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                 //new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                 //new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                 //new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                 //new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),

                //new GunShip(new Vector2(2050, 300), new Vector2(300, 300)), 

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
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),

                new StationaryCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                new StationaryCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                new StationaryCannon(new Vector2(2050, 600), new Vector2(856, 856)),

                new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850))

                //2000,

                //new BatteringRam(new Vector2(2050, 600), new Vector2(700, 850)),
                //new BatteringRam(new Vector2(2050, 600), new Vector2(700, 850)),

                5000,

                //new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                //new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),

                new DropShip(new Vector2(2050, 300), new Vector2(300, 300),
                        new BatteringRam(new Vector2(1800, 0), new Vector2(700, 850)),
                        new BatteringRam(new Vector2(1800, 0), new Vector2(700, 850))),

                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),

                 new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                 new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                 new HealDrone(new Vector2(2050, 600), new Vector2(700, 850))
                );

            #endregion


            #region Wave1
            Wave Wave1 = new Wave(false, 350, 100,

                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new RifleMan(new Vector2(2050, 600), new Vector2(700, 850)),
                
                new FlameJetTrooper(new Vector2(2050, 600), new Vector2(700, 850)),
                new FlameJetTrooper(new Vector2(2050, 600), new Vector2(700, 850)),
                new FlameJetTrooper(new Vector2(2050, 600), new Vector2(700, 850)),

                new JumpMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new JumpMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new JumpMan(new Vector2(2050, 600), new Vector2(700, 850)),
                new JumpMan(new Vector2(2050, 600), new Vector2(700, 850)),

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
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),

                new StationaryCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                new StationaryCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                new StationaryCannon(new Vector2(2050, 600), new Vector2(700, 850)),

                new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),
                new HarpoonCannon(new Vector2(2050, 600), new Vector2(700, 850)),

                2000,

                new BatteringRam(new Vector2(2050, 600), new Vector2(700, 850)),
                new BatteringRam(new Vector2(2050, 600), new Vector2(700, 850)),

                5000,

                new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),
                new HealDrone(new Vector2(2050, 600), new Vector2(700, 850)),

                new DropShip(new Vector2(2050, 300), new Vector2(300, 300),
                        new BatteringRam(new Vector2(1800, 0), new Vector2(700, 850)),
                        new BatteringRam(new Vector2(1800, 0), new Vector2(700, 850)))
                 );            
            #endregion


            WaveList.Add(TestWave);
            //WaveList.Add(Wave1);


            TrapUnlocks.Add(TrapType.Barrel);
            TurretUnlocks.Add(TurretType.Freeze);

            //TrapUnlocks[0] = TrapType.Barrel;
            //TurretUnlocks[0] = TurretType.Freeze;
        }

        public override void LoadContent(ContentManager contentManager)
        {
            //StoryDialogueItems DialogueItems = contentManager.Load<StoryDialogueItems>("StoryDialogue/Level1Dialogue");
            //LevelDialogue.ItemsList = DialogueItems.DialogueItems;

            //LevelDialogue.TutorialMarker = new ButtonMarker(new Vector2(100, 100), contentManager.Load<Texture2D>("WhiteBlock"));
            ////LevelDialogue.TutorialMarker.Texture = contentManager.Load<Texture2D>("WhiteBlock");

            //LevelDialogue.DialogueBox = new StoryDialogueBox();
            ////LevelDialogue.DialogueBox.Position = new Vector2(100, 100);
            //LevelDialogue.DialogueBox.BoxTexture = contentManager.Load<Texture2D>("WhiteBlock");
            //LevelDialogue.DialogueBox.DialogueFont = contentManager.Load<SpriteFont>("Fonts/RobotoBold20_0_Outline");
            //LevelDialogue.DialogueBox.TipFont = contentManager.Load<SpriteFont>("Fonts/RobotoLight20_2");

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
