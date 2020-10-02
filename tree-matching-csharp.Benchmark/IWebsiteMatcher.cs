using System.Threading.Tasks;

namespace tree_matching_csharp.Benchmark
{
    public interface IWebsiteMatcher
    {
        Task<WebsiteMatcher.Result?> MatchWebsites(string source, string target);
    }
}