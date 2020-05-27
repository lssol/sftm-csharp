using System;
using System.Collections;
using System.Collections.Generic;

namespace tree_matching_csharp
{
    public class Edge
    {
        public Node   Source;
        public Node   Target;
        public double Cost;
    }

    public class Node
    {
        public Node()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id;
        public IList<string> Value;
        public string Signature;
        public Node Parent;
        public Node LeftSibling;
        public bool IsNoMatch;
    }

    public class TreeMatchingOutput
    {
        public Dictionary<string, string> Matching { get; set; }
    }
}