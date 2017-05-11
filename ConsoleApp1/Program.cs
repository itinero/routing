using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            // download and extract test-data if not already there.
            Download.DownloadLuxembourgAll();

            // test building a routerdb.
            var routerDb = RouterDbBuildingTests.Run();
        }
    }
}
