using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Common;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
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

        public async Task<(string, string)> GetWebsites(MatcherViewModel model)
        {
            if (model.Website1 != null && model.Website2 != null)
            {
                var website1 = await FileToString(model.Website1);
                var website2 = await FileToString(model.Website2);
                return (website1, website2);
            }
            
            var client = new HttpClient();
            using var response1 = await client.GetAsync(model.Source);
            using var response2 = await client.GetAsync(model.Target);
            using var content1 = response1.Content;
            using var content2 = response2.Content;
            var r1 = await content1.ReadAsStringAsync();
            var r2 = await content2.ReadAsStringAsync();
            return (r1, r2);
        }

        private string? GetHost(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;
            var uri = new Uri(url);
            return uri.GetLeftPart(UriPartial.Authority);
        }
        
        [Route("/match")]
        [HttpPost]
        public async Task<ActionResult> MatchTwoWebsites(MatcherViewModel matcherViewModel)
        {
            var (website1, website2) = await GetWebsites(matcherViewModel);
            var webDOM1 = await DOM.WebpageToDocument(website1);
            var webDOM2 = await DOM.WebpageToDocument(website2);
            AddBase(webDOM1, matcherViewModel.Host ?? GetHost(matcherViewModel.Source) ?? "");
            AddBase(webDOM2, matcherViewModel.Host ?? GetHost(matcherViewModel.Target) ?? "");
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
                SourceDoc = webDOM1.DocumentElement.OuterHtml,
                TargetDoc = webDOM2.DocumentElement.OuterHtml,
            };
            return View("Matcher", viewModel);
        }

        private void AddSignatures(IDocument doc)
        {
            doc.All.ForEach(el => el.SetAttribute(DOM.AttributeName, Guid.NewGuid().ToString()));
        }

        private void AddBase(IDocument doc, string host)
        {
            var baseElement = doc.CreateElement("base");
            baseElement.SetAttribute("href", host);
            doc.Head.Prepend(baseElement);
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