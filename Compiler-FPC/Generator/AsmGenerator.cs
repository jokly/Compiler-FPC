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
        writeStr  : DD '%s', 0
        writeReal : DD '%f', 0

        writelnInt  : DD '%i', 10, 0
        writelnStr  : DD '%s', 10, 0
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
            GenerateProgram();

            return SectionData + SectionBss + SectionText + _Main;
        }

        public void GenPrint(TokenType print_type, Node value)
        {
            string type = TypeToStr(value.NodeType); ;

            if (value is IdNode)
            {
                MainList.Add(new AsmPushNode($"DWORD [{value.Token.Value}]"));
            }
            else if (value is IntConstNode)
            {
                MainList.Add(new AsmPushNode($"{value.Token.Value}"));
            }

            if (print_type == TokenType.WRITE)
            {
                MainList.Add(new AsmPushNode("Write" + type));
            }
            else if (print_type == TokenType.WRITELN)
            {
                MainList.Add(new AsmPushNode("WriteLn" + type));
            }
            else
                throw new System.Exception(); //TODO

            MainList.Add(new AsmCallNode("_printf"));
            MainList.Add(new AsmAddNode("esp", "8"));
        }

        private string TypeToStr(Symbol type)
        {
            if (type is SymVar)
            {
                return TypeToStr((type as SymVar).Type);
            }
            else if (type is SymTypeInteger)
            {
                return "Int";
            }
            else if (type is SymTypeReal)
            {
                return "Real";
            }
            else if (type is SymTypeChar)
            {
                return "Str";
            }
            else
            {
                throw new System.Exception(); // TODO
            }
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

            _Main += "\n";
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
                else
                    throw new System.Exception(); // TODO
            }
        }
    }
}
