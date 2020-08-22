using LambdaCore.Helper;
using LambdaCore.Services;
using System;

namespace LambdaTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Updating Solid Joys Entries...");
            AsyncHelper.RunSync(() => new SolidJoysUpdater().UpdateSolidJoysEntries());
            Console.WriteLine();

            Console.WriteLine("Updating Article Entries...");
            AsyncHelper.RunSync(() => new ArticlesUpdater().UpdateArticleEntries());
            Console.WriteLine();
        }
    }
}
