using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    class CheckBox : Button
    {
        public bool Checked;

        public CheckBox(Vector2 position):base("Buttons/SmallButton", position)
        {
            base.FontName = "Fonts/ButtonFont";
        }

        public override void Update()
        {
            base.Update();

            if (Checked == true)
                base.Text = "X";
            else
                base.Text = " ";

            if (JustClicked == true)               
            {
                if (Checked == true)
                    Checked = false;
                else
                    Checked = true;
            }
        }
    }
}
