using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Map
    {
        public Tile[,] TileArray = new Tile[103, 18];

        private List<Trap> _TrapList;
        public List<Trap> TrapList
        {
            get { return _TrapList; }
            set { _TrapList = value; }
        }
        
        private List<Invader> _InvaderList;
        public List<Invader> InvaderList 
        {
            get { return _InvaderList; }
            set { _InvaderList = value; }
        }
        
        public Vector2 StartIndex = new Vector2(52, 16);
        public Vector2 EndIndex = new Vector2(94, 2);

        Vector2 Size = new Vector2(16, 16);

        public Map(List<Trap> trapList, List<Invader> invaderList, Vector2 startPos, Vector2 endPos)
        {
            TrapList = trapList;
            InvaderList = invaderList;

            StartIndex = new Vector2((int)((startPos.X - 272) / (Size.X)), (int)((startPos.Y - 672) / (Size.Y)));
            EndIndex = new Vector2((int)((endPos.X - 272) / (Size.X)), (int)((endPos.Y - 672) / (Size.Y)));

            for (int x = 0; x < TileArray.GetLength(0); x++)
            {
                for (int y = 0; y < TileArray.GetLength(1); y++)
                {
                    TileArray[x, y] = new Tile(new Vector2(x, y));
                }
            }
        }

        public void LoadContent(ContentManager content)
        {
            for (int x = 0; x < TileArray.GetLength(0); x++)
            {
                for (int y = 0; y < TileArray.GetLength(1); y++)
                {
                    TileArray[x, y].LoadContent(content);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int x = 0; x < TileArray.GetLength(0); x++)
            {
                for (int y = 0; y < TileArray.GetLength(1); y++)
                {
                    TileArray[x, y].Update(gameTime);
                    
                    if (TrapList.Any(Trap => Trap.DestinationRectangle.Intersects(TileArray[x, y].DestinationRectangle)))
                    {
                        TileArray[x, y].TileState = TileState.Solid;
                    }

                    if (StartIndex == new Vector2(x, y))
                    {
                        TileArray[x, y].TileState = TileState.StartPos;
                    }

                    if (EndIndex == new Vector2(x, y))
                    {
                        TileArray[x, y].TileState = TileState.EndPos;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < TileArray.GetLength(0); x++)
            {
                for (int y = 0; y < TileArray.GetLength(1); y++)
                {
                    TileArray[x, y].Draw(spriteBatch);
                }
            }
        }

        public static int StepDistance(Vector2 pointA, Vector2 pointB)
        {
            int distanceX = (int)Math.Abs(pointA.X - pointB.X);
            int distanceY = (int)Math.Abs(pointA.Y - pointB.Y);

            return distanceX + distanceY;
        }

        public int StepDistanceToEnd(Vector2 point)
        {
            return StepDistance(point, EndIndex);
        }

        public IEnumerable<Vector2> OpenMapTiles(Vector2 mapLoc)
        {
            //Down
            if (IsOpen((int)mapLoc.X, (int)mapLoc.Y + 1))
                yield return new Vector2(mapLoc.X, mapLoc.Y + 1);

            //Up
            if (IsOpen((int)mapLoc.X, (int)mapLoc.Y - 1))
                yield return new Vector2(mapLoc.X, mapLoc.Y - 1);

            //Right
            if (IsOpen((int)mapLoc.X + 1, (int)mapLoc.Y))
                yield return new Vector2(mapLoc.X + 1, mapLoc.Y);

            //Left
            if (IsOpen((int)mapLoc.X - 1, (int)mapLoc.Y))
                yield return new Vector2(mapLoc.X - 1, mapLoc.Y);

            //Down Left Diagonal
            if (IsOpen((int)mapLoc.X - 1, (int)mapLoc.Y + 1) &&
                IsOpen((int)mapLoc.X - 1, (int)mapLoc.Y) &&
                IsOpen((int)mapLoc.X, (int)mapLoc.Y + 1))
                yield return new Vector2(mapLoc.X - 1, mapLoc.Y + 1);

            //Up left Diagonal
            if (IsOpen((int)mapLoc.X - 1, (int)mapLoc.Y - 1) &&
                IsOpen((int)mapLoc.X - 1, (int)mapLoc.Y) &&
                IsOpen((int)mapLoc.X, (int)mapLoc.Y - 1))
                yield return new Vector2(mapLoc.X - 1, mapLoc.Y - 1);

            //Down Right Diagonal
            if (IsOpen((int)mapLoc.X + 1, (int)mapLoc.Y + 1) &&
                IsOpen((int)mapLoc.X + 1, (int)mapLoc.Y) &&
                IsOpen((int)mapLoc.X, (int)mapLoc.Y + 1))
                yield return new Vector2(mapLoc.X + 1, mapLoc.Y + 1);

            //Up Right Diagonal
            if (IsOpen((int)mapLoc.X + 1, (int)mapLoc.Y - 1) &&
                IsOpen((int)mapLoc.X + 1, (int)mapLoc.Y) &&
                IsOpen((int)mapLoc.X, (int)mapLoc.Y - 1))
                yield return new Vector2(mapLoc.X + 1, mapLoc.Y - 1);
        }

        private bool IsOpen(int column, int row)
        {
            int COL = (int)MathHelper.Clamp(column, 0, TileArray.GetLength(0) - 1);
            int ROW = (int)MathHelper.Clamp(row, 0, TileArray.GetLength(1) - 1);

            if (TileArray[COL, ROW].TileState == TileState.Solid)
                return false;
            else
                return true;
        }
    }
}
