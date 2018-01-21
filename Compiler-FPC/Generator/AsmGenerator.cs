using Compiler_FPC.Parser;
using System.Collections.Generic;

namespace Compiler_FPC.Generator
{
    class AsmGenerator
    {
        private string SectionData =
        @"section .data
        readInt  : DD '%i', 0
        readStr  : DD '%s', 0
        readReal : DD '%f', 0

        writeInt  : DD '%i', 0
        writeStr  : DD '%c', 0
        writeReal : DD '%f', 0

        writelnInt  : DD '%i', 10, 0
        writelnStr  : DD '%c', 10, 0
        writelnReal : DD '%f', 10, 0

";

        private string SectionBss = 
        @"section .bss
";

        private string SectionText = 
        @"section .text
        global _main

        extern _printf
        extern _scanf

";

        private string _Main =
        @"_main:
        push ebp
        mov ebp, esp
";

        private SyntaxTree Tree;

        private List<AsmNode> Data = new List<AsmNode>();
        private List<AsmNode> Bss = new List<AsmNode>();
        private List<AsmNode> MainList = new List<AsmNode>();

        public AsmGenerator(SyntaxTree tree)
        {
            Tree = tree;
        }

        public string AsmText()
        {
            try
            {
                GenerateProgram();
            }
            catch (AsmGeneratorException ex)
            {
                return ex.Message;
            }
            

            return SectionData + SectionBss + SectionText + _Main;
        }

        private void GenerateProgram()
        {
            FillLists(Tree.Root);

            GenerateSectionData();
            GenerateSectionBss();

            foreach (var node in MainList)
            {
                _Main += "\t" + node.ToString() + "\n";
            }

            _Main += "        leave\n";
            _Main += "        ret";
        }

        private void GenerateSectionData()
        {
            foreach (var node in Data)
            {
                SectionData += "\t" + node.ToString() + "\n";
            }

            SectionData += "\n";
        }

        private void GenerateSectionBss()
        {
            foreach(var node in Bss)
            {
                SectionBss += "\t" + node.ToString() + "\n";
            }

            SectionBss += "\n";
        }

        private void FillLists(Node current)
        {
            if (current.Left != null)
            {
                FillLists(current.Left);
            }

            if (current.Right != null)
            {
                FillLists(current.Right);
            }

            if (current.Childrens.Count != 0)
            {
                foreach (var ch in current.Childrens)
                    FillLists(ch);
            }

            var commands = current.Generate();

            foreach (var com in commands)
            {
                if (com is AsmSectionDataNode)
                {
                    Data.Add(com);
                }
                else if (com is AsmSectionBssNode)
                {
                    Bss.Add(com);
                }
                else if (com is AsmSectionProgramNode)
                {
                    MainList.Add(com);
                }
            }
        }
    }
}
