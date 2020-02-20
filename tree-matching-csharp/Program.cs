using System;
using System.Collections.Generic;
using System.Linq;

namespace tree_matching_csharp
{
    class MyClass
    {
        
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var a = new LinkedList<MyClass>();
            a.AddLast(new MyClass());
            a.AddLast(new MyClass());
            var b = a.First;
            a.Remove(b);
        }
    }
}
