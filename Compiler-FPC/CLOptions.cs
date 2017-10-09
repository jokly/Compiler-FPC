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

        [Option('f', HelpText = "Path to file")]
        public string GetFileName { get; set; }

        [Option('l', "lexer", HelpText = "Launch lexical analyzer")]
        public bool LaunchLexer { get; set; }
    }
}
