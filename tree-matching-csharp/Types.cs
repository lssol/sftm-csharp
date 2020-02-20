using System;
using System.Collections;
using System.Collections.Generic;

namespace tree_matching_csharp
{
    public class Scored<T>
    {
        public Scored(T value, float? score)
        {
            Value = value;
            Score = score;
        }

        public T Value { get; set; }
        public float? Score { get; set; }
    }
    
    public class Edge
    {
        public Node   Source;
        public Node   Target;
        public float? Cost;
    }

    public class Node
    {
        public Node()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id;
        public string Value;
        public string Signature;
        public string XPath;
        public int SizeValue => Value.Length;
        public int SizeXPath => XPath.Length;
        public Node Parent;
        public bool IsNoMatch;
    }

    public class TreeMatchingOutput
    {
        public Dictionary<string, string> Matching { get; set; }
    }
}