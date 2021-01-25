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

        public string Source { get; set; } = "https://www.goodreads.com/book/show/7144.Crime_and_Punishment?ac=1&from_search=true&qid=rlXLotJMLj&rank=1";

        public long MillisecondsToMatch { get; set; } = 0;
        public string Target { get; set; } = "https://www.goodreads.com/book/show/15823480-anna-karenina?ac=1&from_search=true&qid=hRSbRPjyYB&rank=1";
        public IEnumerable<Match> Matching { get; set; }
    }
}