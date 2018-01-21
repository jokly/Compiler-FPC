using System.Reflection;
using CommandLine;
using CommandLine.Text;

namespace Compiler_FPC
{
    class CLOptions
    {
        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("Compiler FPC", "1.0.0"),
                Copyright = new CopyrightInfo("Tikhon Slasten ", 2017),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            help.AddOptions(this);

            return help;
        }

        [Option('v', "version", HelpText = "Show compiler version")]
        public bool GetVersion { get; set; }

        [Option('f', HelpText = "Path to input file")]
        public string GetInputFileName { get; set; }

        [Option('o', HelpText = "Path to output file")]
        public string GetOutputFileName { get; set; }

        [Option('l', "lexer", HelpText = "Launch lexical analyzer")]
        public bool LaunchLexer { get; set; }

        [Option('p', "parser", HelpText = "Launch parser")]
        public bool LaunchParser { get; set; }

        [Option('g', "generator", HelpText = "Launch asm generator")]
        public bool LaunchGenerator { get; set; }

        [Option('q', "optimize", HelpText = "Asm code optimizations")]
        public bool LauncOptimization { get; set; }
    }
}
