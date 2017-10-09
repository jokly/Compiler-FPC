using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new CLOptions();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("working ...");
            }
        }
    }
}
