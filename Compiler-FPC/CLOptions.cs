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
    }
}
