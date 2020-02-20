using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace tree_matching_csharp.Test
{
    public class DomTests
    {
        public static string SimpleWebpage = @"
            <html>
                <head></head>
                <body>
                    <h1></h1>
                    <div>
                        <p> Paragraph 1</p>
                        <p> Paragraph 2</p>
                    </div>
                </body>
            </html>
        ";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task WebpageToTree()
        {
            var nodes = await DOM.WebpageToTree(SimpleWebpage);
            Assert.AreEqual(nodes.Count(), 5);
        }
    }
}