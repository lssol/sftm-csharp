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

        public string Source { get; set; } = "https://www.amazon.fr/Anna-Kar%C3%A9nine-L%C3%A9on-Tolsto%C3%AF/dp/2253098388/ref=sr_1_1?__mk_fr_FR=%C3%85M%C3%85%C5%BD%C3%95%C3%91&crid=M2D2OM4R7TWX&dchild=1&keywords=tolstoi+anna+karenine&qid=1611569521&sprefix=tolstoi%2Caps%2C157&sr=8-1";

        public string Target { get; set; } =
            "https://www.amazon.fr/Crime-ch%C3%A2timent-Fedor-M-Dostoievski/dp/2253082503/ref=sr_1_1?__mk_fr_FR=%C3%85M%C3%85%C5%BD%C3%95%C3%91&crid=1ZANFRQGKJN86&dchild=1&keywords=dostoievski&qid=1611569532&sprefix=dostoi%2Caps%2C155&sr=8-1";
        public IEnumerable<Match> Matching { get; set; }
    }
}