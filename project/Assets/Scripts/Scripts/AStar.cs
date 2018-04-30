using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Original: https://blog.playmedusa.com/verbose-astar-pathfinding-algorithm-in-c-for-unity3d/
public class AStar
{
    class Node
    {
        public float G;
        public float H;
        public float F;
        public Node parent;
        public IAStarCell cell;
        public Node(float G, float F, float H, Node parent, IAStarCell c)
        {
            this.G = G;
            this.H = H;
            this.F = F;
            this.parent = parent;
            this.cell = c;
        }
    }

    List<Node> openList;
    List<Node> closeList;
    List<Node> neighbours;
    List<Node> finalPath;
    Node start;
    Node end;

    public AStar()
    {
        openList = new List<Node>();
        closeList = new List<Node>();
        neighbours = new List<Node>();
        finalPath = new List<Node>();
    }

    public void FindPath(IAStarCell startCell, IAStarCell goalCell, bool targetCellMustBeFree)
    {
        start = new Node(0, 0, 0, null, startCell);
        end = new Node(0, 0, 0, null, goalCell);
        openList.Add(start);
        bool keepSearching = true;
        bool pathExists = true;

        while ((keepSearching) && (pathExists))
        {
            Node currentNode = ExtractBestNodeFromOpenList();
            if (currentNode == null)
            {
                pathExists = false;
                break;
            }
            closeList.Add(currentNode);
            if (NodeIsGoal(currentNode))
                keepSearching = false;
            else {
                if (targetCellMustBeFree)
                    FindValidSixNeighbours(currentNode);
                else
                    FindValidSixNeighboursIgnoreTargetCell(currentNode);

                foreach (Node neighbour in neighbours)
                {
                    if (FindInCloseList(neighbour) != null)
                        continue;
                    Node inOpenList = FindInOpenList(neighbour);
                    if (inOpenList == null)
                    {
                        openList.Add(neighbour);
                    }
                    else {
                        if (neighbour.G < inOpenList.G)
                        {
                            inOpenList.G = neighbour.G;
                            inOpenList.F = inOpenList.G + inOpenList.H;
                            inOpenList.parent = currentNode;
                        }
                    }
                }
            }
        }

        if (pathExists)
        {
            Node n = FindInCloseList(end);
            while (n != null)
            {
                finalPath.Add(n);
                n = n.parent;
            }
        }
    }

    public List<int> PointsFromPath()
    {
        List<int> points = new List<int>();
        foreach (Node n in finalPath)
        {
            points.Add(n.cell.AStarX);
            points.Add(n.cell.AStarZ);
        }
        return points;
    }

    public List<IAStarCell> CellsFromPath()
    {
        List<IAStarCell> path = new List<IAStarCell>();
        foreach (Node n in finalPath)
        {
            path.Add(n.cell);
        }

        if (path.Count != 0)
        {
            path.Reverse();
            path.RemoveAt(0);
        }
        return path;
    }

    Node ExtractBestNodeFromOpenList()
    {
        float minF = float.MaxValue;
        Node bestOne = null;
        foreach (Node n in openList)
        {
            if (n.F < minF)
            {
                minF = n.F;
                bestOne = n;
            }
        }
        if (bestOne != null)
            openList.Remove(bestOne);
        return bestOne;
    }

    bool NodeIsGoal(Node node)
    {
        return ((node.cell.AStarX == end.cell.AStarX) && (node.cell.AStarZ == end.cell.AStarZ));
    }

    void FindValidSixNeighbours(Node n)
    {
        neighbours.Clear();
        IAStarCell neighbour = n.cell.GetAStarNeighbor(HexDirection.E);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable())
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
        neighbour = n.cell.GetAStarNeighbor(HexDirection.NE);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable())
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
        neighbour = n.cell.GetAStarNeighbor(HexDirection.NW);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable())
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
        neighbour = n.cell.GetAStarNeighbor(HexDirection.SE);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable())
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
        neighbour = n.cell.GetAStarNeighbor(HexDirection.SW);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable())
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
        neighbour = n.cell.GetAStarNeighbor(HexDirection.W);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable())
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
    }

    /* Last cell does not need to be walkable in the farm game */
    void FindValidSixNeighboursIgnoreTargetCell(Node n)
    {
        neighbours.Clear();
        IAStarCell neighbour = n.cell.GetAStarNeighbor(HexDirection.E);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable() || (neighbour.AStarCoordinates == end.cell.AStarCoordinates))
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
        neighbour = n.cell.GetAStarNeighbor(HexDirection.NE);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable() || (neighbour.AStarCoordinates == end.cell.AStarCoordinates))
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
        neighbour = n.cell.GetAStarNeighbor(HexDirection.NW);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable() || (neighbour.AStarCoordinates == end.cell.AStarCoordinates))
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
        neighbour = n.cell.GetAStarNeighbor(HexDirection.SE);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable() || (neighbour.AStarCoordinates == end.cell.AStarCoordinates))
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
        neighbour = n.cell.GetAStarNeighbor(HexDirection.SW);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable() || (neighbour.AStarCoordinates == end.cell.AStarCoordinates))
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
        neighbour = n.cell.GetAStarNeighbor(HexDirection.W);
        if (neighbour != null)
        {
            if (neighbour.IsWalkable() || (neighbour.AStarCoordinates == end.cell.AStarCoordinates))
            {
                Node vn = PrepareNewNodeFrom(n, neighbour);
                neighbours.Add(vn);
            }
        }
    }

    Node PrepareNewNodeFrom(Node n, IAStarCell cell)
    {
        Node newNode = new Node(0, 0, 0, n, cell);
        newNode.G = n.G + MovementCost(n, newNode);
        newNode.H = Heuristic(newNode);
        newNode.F = newNode.G + newNode.H;
        newNode.parent = n;
        return newNode;
    }

    float Heuristic(Node n)
    {
        return Mathf.Sqrt((n.cell.AStarX - end.cell.AStarX) * (n.cell.AStarX - end.cell.AStarX) + (n.cell.AStarZ - end.cell.AStarZ) * (n.cell.AStarZ - end.cell.AStarZ));
    }

    float MovementCost(Node a, Node b)
    {
        return b.cell.MovementCost();
    }

    Node FindInCloseList(Node n)
    {
        foreach (Node nn in closeList)
        {
            if ((nn.cell.AStarX == n.cell.AStarX) && (nn.cell.AStarZ == n.cell.AStarZ))
                return nn;
        }
        return null;
    }

    Node FindInOpenList(Node n)
    {
        foreach (Node nn in openList)
        {
            if ((nn.cell.AStarX == n.cell.AStarX) && (nn.cell.AStarZ == n.cell.AStarZ))
                return nn;
        }
        return null;
    }
}
