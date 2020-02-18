using System;

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
    }
}