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
            var str1 = new[] {"sacha", "tommy", "tommy"};
            var str2 = new[] {"Drink", "tommy", "tommy", "leave"};
            var inter = str1.Intersect(str2);
            Console.WriteLine("hello");
        }
    }
}