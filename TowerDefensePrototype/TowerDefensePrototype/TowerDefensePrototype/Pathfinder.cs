using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Pathfinder
    {
        Vector2 GridSize = new Vector2(32, 32);
        static Random Random = new Random();

        //As the pathfinder is created it gets a list of all the possible obstacles and then
        //returns a path
        //If another obstacle is placed, the pathfinder is reset with new start position data
        //and new obstacle data
        //The "map" is regenerated for the pathfinder everytime a pathfinder object is created

        //public Map Map;
        private List<SearchNode> OpenList;
        private List<SearchNode> ClosedList;

        private List<Vector2> WayPoints = new List<Vector2>();

        private Dictionary<Vector2, Vector2> paths;

        public Tile[,] TileArray = new Tile[37, 7];

        private ObservableCollection<Trap> _TrapList;
        public ObservableCollection<Trap> TrapList
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

        ContentManager contentManager;

        private struct SearchNode
        {
            public Vector2 Position;
            public int DistanceToGoal;
            public int DistanceTraveled;

            public SearchNode(Vector2 mapPosition, int distanceToGoal, int distanceTraveled)
            {
                Position = mapPosition;
                DistanceToGoal = distanceToGoal;
                DistanceTraveled = distanceTraveled;
            }
        }

        bool FindPath = true;

        public Pathfinder(ObservableCollection<Trap> trapList, List<Invader> invaderList, Vector2 startPos, Vector2 endPos)
        {
            TileArray = new Tile[(int)(1648 / GridSize.X), (int)(288 / GridSize.Y)];

            //Map = map;
            TrapList = trapList;
            TrapList.CollectionChanged += TrapsChanged;

            InvaderList = invaderList;

            StartIndex = new Vector2((int)((startPos.X - 272) / (GridSize.X)), (int)((startPos.Y - 672) / (GridSize.Y)));
            EndIndex = new Vector2((int)((endPos.X - 272) / (GridSize.X)), (int)((endPos.Y - 672) / (GridSize.Y)));

            for (int x = 0; x < TileArray.GetLength(0); x++)
            {
                for (int y = 0; y < TileArray.GetLength(1); y++)
                {
                    TileArray[x, y] = new Tile(new Vector2(x, y));
                }
            }

            OpenList = new List<SearchNode>();
            ClosedList = new List<SearchNode>();
            paths = new Dictionary<Vector2, Vector2>();
            OpenList.Add(new SearchNode(StartIndex, StepDistance(StartIndex, EndIndex), 0));
        }

        public List<Vector2> GetWaypoints()
        {
            for (int x = 0; x < TileArray.GetLength(0); x++)
            {
                for (int y = 0; y < TileArray.GetLength(1); y++)
                {
                    TileArray[x, y].Update();

                    if (TrapList.Any(Trap => Trap.DestinationRectangle.Intersects(TileArray[x, y].DestinationRectangle) && Trap.Solid == true))
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

            while (FindPath == true)
            {
                DoSearchStep();
            }

            if (WayPoints.Count == 0)
            {
                foreach (Vector2 point in FinalPath())
                {
                    WayPoints.Add(new Vector2(272 + Random.Next(-8, 8), 672 + Random.Next(-8, 8)) + (point * GridSize.X) + new Vector2(GridSize.X / 2, GridSize.Y));
                }
            }

            //WayPoints = FinalPath().ToList();

            return WayPoints;
        }
        
        public void LoadContent(ContentManager content)
        {
            contentManager = content;

            for (int x = 0; x < TileArray.GetLength(0); x++)
            {
                for (int y = 0; y < TileArray.GetLength(1); y++)
                {
                    TileArray[x, y].LoadContent(content);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //for (int x = 0; x < TileArray.GetLength(0); x++)
            //{
            //    for (int y = 0; y < TileArray.GetLength(1); y++)
            //    {
            //        TileArray[x, y].Draw(spriteBatch);
            //    }
            //}
        }



        private void DoSearchStep()
        {
            SearchNode newOpenListNode;

            bool foundNewNode = SelectNodeToVisit(out newOpenListNode);

            if (foundNewNode == true)
            {
                Vector2 currentPos = newOpenListNode.Position;

                foreach (Vector2 point in OpenMapTiles(currentPos))
                {
                    SearchNode tile = new SearchNode(point, StepDistanceToEnd(point), newOpenListNode.DistanceTraveled + 1);

                    if (!InList(OpenList, point) && !InList(ClosedList, point))
                    {
                        OpenList.Add(tile);
                        paths[point] = newOpenListNode.Position;
                    }
                }

                if (currentPos == EndIndex)
                {
                    FindPath = false;

                    foreach (Vector2 point in FinalPath())
                    {
                        for (int x = 0; x < TileArray.GetLength(0); x++)
                        {
                            for (int y = 0; y < TileArray.GetLength(1); y++)
                            {
                                if (point == new Vector2(x,y))
                                    TileArray[x,y].TileState = TileState.FinalPath;
                            }
                        }
                    }
                    //FinalPath();
                }

                OpenList.Remove(newOpenListNode);
                ClosedList.Add(newOpenListNode);

                //if (Map != null)
                {
                    int COL = (int)MathHelper.Clamp((int)newOpenListNode.Position.X, 0, TileArray.GetLength(0) - 1);
                    int ROW = (int)MathHelper.Clamp((int)newOpenListNode.Position.Y, 0, TileArray.GetLength(1) - 1);

                    TileArray[COL, ROW].TileState = TileState.Path;
                }
            }
            else
            {
                //No open positions
                //There is no available path to destination
                FindPath = false;
                
            }
        }

        private bool SelectNodeToVisit(out SearchNode result)
        {
            result = new SearchNode();
            bool success = false;
            float smallestDistance = float.PositiveInfinity;
            float currentDistance = 0f;

            if (OpenList.Count > 0)
            {
                foreach (SearchNode node in OpenList)
                {
                    currentDistance = node.DistanceToGoal;

                    if (currentDistance < smallestDistance)
                    {
                        success = true;
                        result = node;
                        smallestDistance = currentDistance;
                    }
                }
            }

            return success;
        }

        private static bool InList(List<SearchNode> list, Vector2 point)
        {
            bool inList = false;

            foreach (SearchNode node in list)
            {
                if (node.Position == point)
                {
                    inList = true;
                }
            }

            return inList;
        }

        public LinkedList<Vector2> FinalPath()
        {
            LinkedList<Vector2> path = new LinkedList<Vector2>();
            if (FindPath == false && ClosedList.Count > 0)
            {
                Vector2 curPrev = EndIndex;
                path.AddFirst(curPrev);
                while (paths.ContainsKey(curPrev))
                {
                    curPrev = paths[curPrev];
                    path.AddFirst(curPrev);
                }
            }
            return path;
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


        private void TrapsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int p = 0;
            FindPath = true;
            WayPoints.Clear();

            for (int x = 0; x < TileArray.GetLength(0); x++)
            {
                for (int y = 0; y < TileArray.GetLength(1); y++)
                {
                    TileArray[x, y] = new Tile(new Vector2(x, y));
                    TileArray[x, y].LoadContent(contentManager);
                }
            }

            OpenList = new List<SearchNode>();
            ClosedList = new List<SearchNode>();
            paths = new Dictionary<Vector2, Vector2>();
            OpenList.Add(new SearchNode(StartIndex, StepDistance(StartIndex, EndIndex), 0));

            //GetWaypoints();
            //Update();

            //while (FindPath == true)
            //{
            //    DoSearchStep();
            //}

            //UPDATE THE PATHFINDING HERE
            //
            //Should merge this and the pathfinder class together
            //Or make one Map object that is shared between all invaders
            //Just need to take into account that the size of the invaders fluctuates and therefore the size
            //of the tiles should fluctuate too.
        }
    }
}
