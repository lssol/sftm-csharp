using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace tree_matching_csharp.Visualization.Models
{
    
    public class MatcherViewModel
    {
        public class Match
        {
            public string Id1 { get; set; }
            public string Id2 { get; set; }
        }
        public IFormFile Website1 { get; set; }
        public IFormFile Website2 { get; set; }
        public string Host { get; set; }
        
        public string SourceDoc { get; set; }
        public string TargetDoc { get; set; }
        
        public string Source { get; set; }
        public string Target { get; set; }
        public IEnumerable<Match> Matching { get; set; }
    }
}