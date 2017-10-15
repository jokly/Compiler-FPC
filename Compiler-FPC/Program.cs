using System;
using System.Collections.Generic;
using System.IO;
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

            if (!CommandLine.Parser.Default.ParseArguments(args, options)) return;

            if (options.GetVersion)
            {
                Console.WriteLine(assembly.Name);
                Console.WriteLine(assembly.Version.ToString());
            }

            if (options.LaunchLexer && options.GetInputFileName != null)
            {
                if (options.GetOutputFileName != null)
                {
                    using (var outputFile = new StreamWriter(options.GetOutputFileName))
                    {
                        outputFile.Write(getLexerOutput(options.GetInputFileName));
                    }
                }
                else
                {
                    Console.WriteLine(getLexerOutput(options.GetInputFileName));
                }
            }
            else if (options.LaunchLexer && options.GetInputFileName == null)
            {
                Console.WriteLine("No input files.");
            }
        }

        static string getLexerOutput(string fileName)
        {
            var table = new ConsoleTable(new ConsoleTableOptions
            {
                Columns = new List<string>() { "Position", "Type", "Value", "Text" },
                EnableCount = false
            });

            var lexer = new Tokenizer(fileName);

            try
            {
                Token tok;
                while ((tok = lexer.Next()) != null)
                {
                    table.AddRow($"({tok.Row}, {tok.Col})", tok.Type.ToString(), tok.Value, tok.Text);
                }
            }
            catch (TokenizerException e)
            {
                table.AddRow($"({e.Row}, {e.Col})", TokenType.ERROR, e.Text, e.Text);
            }
            
            return table.ToString();
        }
    }
}
