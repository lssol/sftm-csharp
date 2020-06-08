using System;
using System.Linq;
using NUnit.Framework;

namespace tree_matching_csharp.Test
{
    public class TestTest
    {
        [Test]
        public void Test()
        {
            var tuples = new[] {("sacha", 4), ("ilan", 3), ("sacha", 9)};
            var lookup = tuples.ToLookup(t => t.Item1, t => t.Item2);
            var dic = lookup.ToDictionary(m => m.Key, m => m.Count());
            Console.WriteLine(lookup["sacha"].ToString());
            Console.WriteLine("hello world");
        }
    }
}