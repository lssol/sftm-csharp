using System.Collections.Generic;

namespace tree_matching_csharp.Visualization.Models
{
    public class TreeVizResult
    {
        public IEnumerable<CytoElementDef> Tree1     { get; set; }
        public IEnumerable<CytoElementDef> Tree2     { get; set; }
        public Matching Matching { get; set; }
    }

    public class CytoElementDef
    {
        public class DataClass
        {
            public string              Id     { get; set; }
            public IEnumerable<string> Value  { get; set; }
            public string              Label  { get; set; }
            public string              Parent { get; set; }
        }

        public string    Group { get; set; }
        public DataClass Data  { get; set; }
    }

    public class Matching
    {
        public class Match
        {
            public string Id1 { get; set; }
            public string Id2 { get; set; }
            public FtmCost.Cost Cost { get; set; }
        }

        public double Time     { get; set; }
        public double Cost  { get; set; }
        public IEnumerable<Match>  Matches { get; set; }
    }
}