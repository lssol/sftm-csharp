using System.Collections;
using System.Collections.Generic;

namespace tree_matching_csharp
{
    public class Edge
    {
        public Node Node1;
        public Node Node2;
        public float? Cost;
    }

    public class Node
    {
        public string Value;
        public string Signature;
        public string XPath;
    }
    public class Tree
    {
        public Tree()
        {
            Edges = new List<Edge>();
            Nodes = new List<Node>();
        }

        public IList Edges;
        public IList Nodes;
    }
}