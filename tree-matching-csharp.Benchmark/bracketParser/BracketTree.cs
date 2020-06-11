using System.Collections.Generic;

namespace tree_matching_csharp.Benchmark.BracketParser
{
    public class BracketTree
    {
        public string Label { get; set; }
        public IList<BracketTree> Children { get; set; }
    }
}