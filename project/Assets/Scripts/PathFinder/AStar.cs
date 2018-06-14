using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Original: https://blog.playmedusa.com/verbose-astar-pathfinding-algorithm-in-c-for-unity3d/
public static class AStar
{
    //TODO change function search to linq's search
    public static void FindPath(List<IAStarCell> result, IAStarCell startCell, IAStarCell goalCell, bool targetCellMustBeFree)
    {
        //for test
        int iterationsCount = 0;
        //
        List<Node> openList = new List<Node>();
        List<Node> closeList = new List<Node>();
        List<IAStarCell> neighbours = new List<IAStarCell>();

        Node start = new Node(0, 0, null, startCell);
        Node end = new Node(0, 0, null, goalCell);
        Node currentNode = null;
        openList.Add(start);
        bool keepSearching = true;
        bool pathExists = true;

        while ((keepSearching) && (pathExists))
        {
            //test
            iterationsCount++;

            currentNode = GetBestNode(openList);
            if (currentNode == null)
            {
                pathExists = false;
                break;
            }
            closeList.Add(currentNode);
            if (NodeIsGoal(currentNode, end))
                keepSearching = false;
            else {
                if (targetCellMustBeFree)
                    neighbours = GetValidNeighbours(currentNode);
                else
                    neighbours = GetValidNeighboursIgnoreTargetCell(currentNode, end);

                foreach (IAStarCell neighbour in neighbours)
                {
                    if (FindInList(neighbour,closeList) != null)
                        continue;
                    Node inOpenList = FindInList(neighbour,openList);
                    if (inOpenList == null)
                    { 
                        openList.Add(PrepareNewNodeFrom(currentNode, neighbour, end));
                    }
                    else {
                        float pathDist = currentNode.G + MovementCost(currentNode.cell, neighbour);
                        if (currentNode.G + MovementCost(currentNode.cell, neighbour) < inOpenList.G)
                        {
                            inOpenList.G = pathDist;
                            inOpenList.parent = currentNode;
                        }
                    }
                }
            }
        }

        if (pathExists)
        {
            PathFromNode(result, currentNode);
        }
        else
        {
            //just to dont return Null
            result.Clear();
        }
    }

    public static void PathFromNode(List<IAStarCell> result, Node node)
    {
        if (result != null)
        {
            result.Clear();
            if (node != null)
            {
                while (node != null)
                {
                    result.Add(node.cell);
                    node = node.parent;
                }
                if (result.Count != 0)
                {
                    result.Reverse();
                    result.RemoveAt(0);
                }
            }
        }
    }

    public static Node GetBestNode(List<Node> nodesList)
    {
        float minF = float.MaxValue;
        Node bestOne = null;

        foreach (Node n in nodesList)
        {
            if (n.F < minF)
            {
                minF = n.F;
                bestOne = n;
            }
        }

        if (bestOne != null)
            nodesList.Remove(bestOne);
        return bestOne;
    }

    public static bool NodeIsGoal(Node node, Node end)
    {
        return ((node.cell.AStarX == end.cell.AStarX) && (node.cell.AStarZ == end.cell.AStarZ));
    }

    public static List<IAStarCell> GetValidNeighbours(Node n)
    {
        List<IAStarCell> neighbours = new List<IAStarCell>();
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            IAStarCell neighbour = n.cell.GetAStarNeighbor(d);
            if (neighbour != null)
            {
                if (neighbour.IsWalkable())
                {
                    neighbours.Add(neighbour);
                }
            }
        }
        return neighbours;
    }

    /* Last cell does not need to be walkable in the farm game */
    public static List<IAStarCell> GetValidNeighboursIgnoreTargetCell(Node n, Node end)
    {
        List<IAStarCell> neighbours = new List<IAStarCell>();
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            IAStarCell neighbour = n.cell.GetAStarNeighbor(d);
            if (neighbour != null)
            {
                if (neighbour.IsWalkable() || (neighbour.AStarCoordinates == end.cell.AStarCoordinates))
                {
                    neighbours.Add(neighbour);
                }
            }
        }
        return neighbours;
    }

    public static Node PrepareNewNodeFrom(Node n, IAStarCell cell, Node end)
    {
        Node newNode = new Node(0, 0, n, cell);
        newNode.G = n.G + MovementCost(n.cell, newNode.cell);
        newNode.H = Heuristic(newNode, end);
        newNode.parent = n;
        return newNode;
    }

    public static float Heuristic(Node n, Node end)
    {
        return Mathf.Sqrt((n.cell.AStarX - end.cell.AStarX) * (n.cell.AStarX - end.cell.AStarX) + (n.cell.AStarZ - end.cell.AStarZ) * (n.cell.AStarZ - end.cell.AStarZ));
    }

    public static float MovementCost(IAStarCell a, IAStarCell b)
    {
        return b.MovementCost();
    }

    public static Node FindInList(Node n, List<Node> list)
    {

        foreach (Node nn in list)
        {
            if ((nn.cell.AStarX == n.cell.AStarX) && (nn.cell.AStarZ == n.cell.AStarZ))
                return nn;
        }

        return null;
    }

    public static Node FindInList(IAStarCell c, List<Node> list)
    {
        foreach (Node nn in list)
        {
            if ((nn.cell.AStarX == c.AStarX) && (nn.cell.AStarZ == c.AStarZ))
                return nn;
        }

        return null;
    }


}
