using System;
using System.Collections.Generic;
using System.Reflection;
using ConsoleTables;

namespace Compiler_FPC
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new CLOptions();
            var assembly = Assembly.GetExecutingAssembly().GetName();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.GetVersion)
                {
                    Console.WriteLine(assembly.Name);
                    Console.WriteLine(assembly.Version.ToString());
                }

                if (options.LaunchLexer && options.GetFileName != null)
                {
                    var table = new ConsoleTable(new ConsoleTableOptions
                    {
                        Columns = new List<string>(){"Position", "Type", "Value", "Text"},
                        EnableCount = false
                    });

                    var lexer = new Tokenizer(options.GetFileName);

                    Token tok;
                    while ((tok = lexer.Next()) != null)
                    {
                        table.AddRow($"({tok.Row}, {tok.Col})", tok.Type.ToString(), tok.Value, tok.Text);
                    }

                    Console.WriteLine(table.ToString());
                }
                else if (options.LaunchLexer && options.GetFileName == null)
                {
                    Console.WriteLine("No input files.");
                }
            }
        }
    }
}
