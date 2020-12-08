using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Common;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoreLinq;
using tree_matching_csharp.Benchmark;
using tree_matching_csharp.Visualization.Models;
using tree_matching_csharp.Visualization.Utils;

namespace tree_matching_csharp.Visualization.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        [Route("/match")]
        [HttpGet]
        public IActionResult MatchTwoWebsites()
        {
            return View("Matcher", new MatcherViewModel());
        }
        
        [Route("/match")]
        [HttpPost]
        public async Task<ActionResult> MatchTwoWebsites(MatcherViewModel matcherViewModel)
        {
            var website1 = await FileToString(matcherViewModel.Website1);
            var website2 = await FileToString(matcherViewModel.Website2);
            var webDOM1 = await DOM.WebpageToDocument(website1);
            var webDOM2 = await DOM.WebpageToDocument(website2);
            AddSignatures(webDOM1);
            AddSignatures(webDOM2);
            var source = DOM.DomToTree(webDOM1);
            var target = DOM.DomToTree(webDOM2);
            
            var matcher = new SftmTreeMatcher(Settings.SFTMParameters());
            var resultMatching = await matcher.MatchTrees(source, target);
            var matches = resultMatching.Edges
                .Where(edge => edge?.Source?.Signature != null && edge.Target?.Signature != null )
                .Select(edge => (edge.Source.Signature, edge.Target.Signature)).ToDictionary();
            webDOM1.All.ForEach(el =>
            {
                var signature = el.GetAttribute(DOM.AttributeName);
                var matchingValue = matches.ContainsKey(signature) ? matches[signature] : signature;
                el.SetAttribute(DOM.AttributeName,  matchingValue);
            });
            var viewModel = new MatcherViewModel
            {
                Source = webDOM1.DocumentElement.OuterHtml,
                Target = webDOM2.DocumentElement.OuterHtml,
            };
            return View("Matcher", viewModel);
        }

        private void AddSignatures(IDocument doc)
        {
            doc.All.ForEach(el => el.SetAttribute(DOM.AttributeName, Guid.NewGuid().ToString()));
        }

        private async Task<string> FileToString(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return await Task.FromResult((string)null);

            using var reader = new StreamReader(file.OpenReadStream());
            return await reader.ReadToEndAsync();
        }
    }
}