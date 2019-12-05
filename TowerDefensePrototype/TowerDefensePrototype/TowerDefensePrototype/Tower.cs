using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Tower
    {        
        string AssetName;
        public Vector2 TowerPosition, TowerScale, ButtonPosition, ButtonScale;
        int TowerHP, AvailableSlots;
        bool Active;
        Texture2D Texture;
        Color TowerColor, ButtonColor;
        Rectangle DestinationRectangle;        
        List<Button> TowerButtonList;

        public Tower(string assetName, string buttonAssetName, Vector2 towerPosition, int towerHP, int availableSlots, Vector2? towerScale = null, Color? towerColor = null, Vector2? buttonScale = null, Color? buttonColor = null)
        {
            Active = true;

            AssetName = assetName;
            TowerPosition = towerPosition;
            TowerHP = towerHP;
            AvailableSlots = availableSlots;

            #region Optional tower parameters
                if (towerScale == null)
                    TowerScale = new Vector2(1, 1);
                else
                    TowerScale = towerScale.Value;

                if (towerColor == null)
                    TowerColor = Color.White;
                else
                    TowerColor = towerColor.Value;
            #endregion

            #region Optional button parameters
                if (buttonScale == null)
                    ButtonScale = new Vector2(1, 1);
                else
                    ButtonScale = buttonScale.Value;

                if (buttonColor == null)
                    ButtonColor = Color.White;
                else
                    ButtonColor = buttonColor.Value;
            #endregion

            ButtonPosition.X = TowerPosition.X + 16;
            ButtonPosition.Y = TowerPosition.Y + 16;

            TowerButtonList = new List<Button>();

            for (int i = 0; i < availableSlots; i++)
            {
                TowerButtonList.Add(new Button(buttonAssetName, new Vector2(ButtonPosition.X, ButtonPosition.Y + (i * 38))));
            }
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);

            foreach (Button towerButton in TowerButtonList)
            {
                towerButton.LoadContent(contentManager);
            }
        }

        public void Update(GameTime gameTime)
        {            
            if (TowerHP <= 0)
                Active = false;

            if (Active == true)
            {
                DestinationRectangle = new Rectangle((int)TowerPosition.X, (int)TowerPosition.Y, Texture.Width, Texture.Height);

                foreach (Button towerButton in TowerButtonList)
                {
                    towerButton.Update(gameTime);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                spriteBatch.Draw(Texture, DestinationRectangle, TowerColor);

                foreach (Button towerButton in TowerButtonList)
                {
                    towerButton.Draw(spriteBatch);
                }
            }
        }
    }
}
