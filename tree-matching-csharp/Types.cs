using System;
using System.Collections;
using System.Collections.Generic;

namespace tree_matching_csharp
{
    public class Edge
    {
        public Node   Source;
        public Node   Target;
        public FtmCost.Cost FtmCost;
        public double NormalizedScore;
        public double Score { get; set; }
    }

    public class Node
    {
        public Node()
        {
            Id = Guid.NewGuid();
            Children = new List<Node>();
        }

        public Guid Id;
        public IList<string> Value;
        public string Signature;
        public Node Parent;
        public Node LeftSibling;
        public IList<Node> Children;
    }

    public class TreeMatchingOutput
    {
        public Dictionary<string, string> Matching { get; set; }
    }
}