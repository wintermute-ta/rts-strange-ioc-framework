using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public float G;
    public float H;
    public float F { get { return H + G; } }
    public Node parent;
    public IAStarCell cell;
    public Node(float G, float H, Node parent, IAStarCell c)
    {
        this.G = G;
        this.H = H;
        this.parent = parent;
        this.cell = c;
    }
}
