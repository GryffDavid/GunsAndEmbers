using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Pathfinder
    {
        Vector2 GridSize = new Vector2(16, 16);

        //As the pathfinder is created it gets a list of all the possible obstacles and then
        //returns a path
        //If another obstacle is placed, the pathfinder is reset with new start position data
        //and new obstacle data
        //The "map" is regenerated for the pathfinder everytime a pathfinder object is created

        public Map Map;
        private List<SearchNode> OpenList;
        private List<SearchNode> ClosedList;

        private List<Vector2> WayPoints = new List<Vector2>();

        private Dictionary<Vector2, Vector2> paths;

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

        public Pathfinder(Map map)
        {
            Map = map;
            OpenList = new List<SearchNode>();
            ClosedList = new List<SearchNode>();
            paths = new Dictionary<Vector2, Vector2>();
            OpenList.Add(new SearchNode(map.StartIndex, Map.StepDistance(map.StartIndex, map.EndIndex), 0));
        }

        public List<Vector2> GetWaypoints()
        {
            while (FindPath == true)
            {
                DoSearchStep();
            }
                        
            foreach (Vector2 point in FinalPath())
            {
                WayPoints.Add(new Vector2(272, 672) + (point * 16));
            }

            return WayPoints;
        }


        private void DoSearchStep()
        {
            SearchNode newOpenListNode;

            bool foundNewNode = SelectNodeToVisit(out newOpenListNode);

            if (foundNewNode == true)
            {
                Vector2 currentPos = newOpenListNode.Position;

                foreach (Vector2 point in Map.OpenMapTiles(currentPos))
                {
                    SearchNode tile = new SearchNode(point, Map.StepDistanceToEnd(point), newOpenListNode.DistanceTraveled + 1);

                    if (!InList(OpenList, point) && !InList(ClosedList, point))
                    {
                        OpenList.Add(tile);
                        paths[point] = newOpenListNode.Position;
                    }
                }

                if (currentPos == Map.EndIndex)
                {
                    FindPath = false;
                    FinalPath();
                }

                OpenList.Remove(newOpenListNode);
                ClosedList.Add(newOpenListNode);

                if (Map != null)
                {
                    int COL = (int)MathHelper.Clamp((int)newOpenListNode.Position.X, 0, Map.TileArray.GetLength(0) - 1);
                    int ROW = (int)MathHelper.Clamp((int)newOpenListNode.Position.Y, 0, Map.TileArray.GetLength(1) - 1);

                    Map.TileArray[COL, ROW].TileState = TileState.Path;
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
                Vector2 curPrev = Map.EndIndex;
                path.AddFirst(curPrev);
                while (paths.ContainsKey(curPrev))
                {
                    curPrev = paths[curPrev];
                    path.AddFirst(curPrev);
                }
            }
            return path;
        }
    }
}
