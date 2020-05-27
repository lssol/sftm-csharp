namespace tree_matching_csharp
{
    public static class SimilarityPropagation
    {
        public class Parameters
        {
            public double   Sibling    { get; set; }
            public double   SiblingInv { get; set; }
            public double   Parent     { get; set; }
            public double   ParentInv  { get; set; }
            public double[] Envelop    { get; set; }
        }

        public static Neighbors PropagateSimilarity(Neighbors neighbors, Parameters parameters)
        {
            var newSimilarity = new Neighbors();
            foreach (var (targetNode, hits) in neighbors.Value)
            foreach (var (sourceNode, score) in hits)
            {

            }

            return neighbors;
        }
    }
}