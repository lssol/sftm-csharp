using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neighbors = System.Collections.Generic.Dictionary<tree_matching_csharp.Node, System.Collections.Generic.HashSet<tree_matching_csharp.Scored<tree_matching_csharp.Node>>>;

namespace tree_matching_csharp
{
    public static class Utils
    {
        public static Node GetNthParent(Node node, int n)
        {
            if (n < 0)
                throw new Exception("n cannot be negative");
            while (true)
            {
                if (node == null) return null;
                if (n    == 0) return node;
                node = node.Parent;
                n -= 1;
            }
        }
        
        public static IEnumerable<Edge> NeighborsToEdges(Neighbors neighbors)
        {
            return neighbors.SelectMany(pair => pair
                .Value
                .Select(source => new Edge
                {
                    Cost   = source.Score,
                    Source = source.Value,
                    Target = pair.Key
                }));
        }

        public static Dictionary<T, HashSet<G>> PushAt<T, G>(this Dictionary<T, HashSet<G>> self, T key, G value)
        {
            if (self.ContainsKey(key))
                self[key].Add(value);
            else
                self.Add(key, new HashSet<G>{value});

            return self;
        }

        public static LinkedList<T> RemoveIfExist<T>(this LinkedList<T> self, LinkedListNode<T> node)
        {
            if (node.List != null)
                self.Remove(node);
            
            return self;
        }
    }
}