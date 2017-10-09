using System;
using System.Reflection;

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
                    Tokenizer lexer = new Tokenizer(options.GetFileName);
                    Token tok;
                    while ((tok = lexer.Next()) != null)
                    {
                        Console.WriteLine(tok.Row + " : " + tok.Col);
                        Console.WriteLine(tok.Type.ToString());
                        Console.WriteLine(tok.Text);
                        Console.WriteLine("-----------");
                    }
                }
                else if (options.LaunchLexer && options.GetFileName == null)
                {
                    Console.WriteLine("No input files.");
                }
            }
        }
    }
}
