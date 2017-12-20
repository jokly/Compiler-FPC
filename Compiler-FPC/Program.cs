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

            // Version
            if (options.GetVersion)
            {
                Console.WriteLine(assembly.Name);
                Console.WriteLine(assembly.Version.ToString());
            }

            // Lexer
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

            // Parser
            if (options.LaunchParser && options.GetInputFileName != null)
            {
                if (options.GetOutputFileName != null)
                {
                    using (var outputFile = new StreamWriter(options.GetOutputFileName))
                    {
                        outputFile.Write(getParserOutput(options.GetInputFileName));
                    }
                }
                else
                {
                    Console.WriteLine(getParserOutput(options.GetInputFileName));
                }
            }
            else if (options.LaunchParser && options.GetInputFileName == null)
            {
                Console.WriteLine("No input files.");
            }

            // AsmGenerator
            if (options.LaunchGenerator && options.GetInputFileName != null)
            {
                if (options.GetOutputFileName != null)
                {
                    using (var outputFile = new StreamWriter(options.GetOutputFileName))
                    {
                        outputFile.Write(getGeneratorOutput(options.GetInputFileName));
                    }
                }
                else
                {
                    Console.WriteLine(getGeneratorOutput(options.GetInputFileName));
                }
            }
            else if (options.LaunchGenerator && options.GetInputFileName == null)
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
                while ((tok = lexer.Next()).Type != TokenType.EOF)
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

        static string getParserOutput(string fileName)
        {
            var tokenizer = new Tokenizer(fileName);
            var text = new Parser.Parser(tokenizer).Tree();

            return text;
        }

        static string getGeneratorOutput(string fileName)
        {
            var tokenizer = new Tokenizer(fileName);
            var parser = new Parser.Parser(tokenizer);
            parser.BuildTree();

            return new Generator.AsmGenerator(parser.tree).AsmText();
        }
    }
}
