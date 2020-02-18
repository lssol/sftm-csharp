using System;
using System.Collections;
using System.Collections.Generic;

namespace tree_matching_csharp
{
    public class Scored<T>
    {
        public Scored(T value, double? score)
        {
            Value = value;
            Score = score;
        }

        public T Value { get; set; }
        public double? Score { get; set; }
    }
    
    public class Neighbors : Dictionary<Node, HashSet<Scored<Node>>> {}
    
    public class Edge
    {
        public Node   Node1;
        public Node   Node2;
        public float? Cost;
    }

    public class Node
    {
        public Node()
        {
            Id = Guid.NewGuid();
        }

        public Guid   Id { get; set; }
        public string Value;
        public string Signature;
        public string XPath;
    }
    public class Tree
    {
        public readonly IList       Edges;
        public readonly IList<Node> Nodes;
        public Tree()
        {
            Edges = new List<Edge>();
            Nodes = new List<Node>();
        }

    }
}