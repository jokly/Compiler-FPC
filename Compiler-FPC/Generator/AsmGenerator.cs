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

        writelnInt  : DB '%i', 10, 0
        writelnStr  : DB '%c', 10, 0
        writelnReal : DB '%f', 10, 0

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

        public string AsmText(bool isOptimize = false)
        {
            try
            {
                GenerateProgram(isOptimize);
            }
            catch (AsmGeneratorException ex)
            {
                return ex.Message;
            }


            return SectionData + SectionBss + SectionText + _Main;
        }

        private void GenerateProgram(bool isOptimize = false)
        {
            FillLists(Tree.Root);

            if (isOptimize)
                Optimize();

            GenerateSectionData();
            GenerateSectionBss();

            foreach (var node in MainList)
            {
                _Main += "\t" + node.ToString() + "\n";
            }

            _Main += "        leave\n";
            _Main += "        ret";
        }

        private void Optimize()
        {
            // 2
            var i = 0;
            while (i < MainList.Count - 1)
            {
                // push eax; pop eax
                if (MainList[i] is AsmPushNode && MainList[i + 1] is AsmPopNode)
                {
                    var pushOp = (MainList[i] as AsmPushNode).Value;
                    var popOp = (MainList[i + 1] as AsmPopNode).Destination;
                    if (!pushOp.Equals(popOp))
                    {
                        i++;
                        continue;
                    }

                    for (var j = 0; j < 2; j++)
                        MainList.RemoveAt(i);
                }

                i++;
            }

            // 4
            i = 0;
            while (i < MainList.Count - 3)
            {
                // a + 1
                if (MainList[i] is AsmPushNode && MainList[i + 1] is AsmPopNode &&
                    MainList[i + 2] is AsmPopNode && MainList[i + 3] is AsmAddNode)
                {
                    if (!(MainList[i] as AsmPushNode).Value.Equals("0x1; 1"))
                    {
                        i++;
                        continue;
                    }

                    for (var j = 0; j < 4; j++)
                        MainList.RemoveAt(i);

                    MainList.InsertRange(i, new List<AsmNode>()
                    {
                        new AsmPopNode("eax"),
                        new AsmIncNode("eax")
                    });
                }

                i++;
            }
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

        public static List<AsmNode> GenAsm(Node node, List<AsmNode> asm_list)
        {
            if (!(node is IfNode || node is ForNode || node is WhileNode || node is RepeatNode || node is IdNode))
            {
                if (node.Left != null)
                    GenAsm(node.Left, asm_list);

                if (node.Right != null)
                    GenAsm(node.Right, asm_list);

                if (node.Childrens.Count != 0)
                    foreach (var ch in node.Childrens)
                        GenAsm(ch, asm_list);
            }

            var commands = node.Generate();
            asm_list.AddRange(commands);

            return asm_list;
        }

        private void FillLists(Node current)
        {
            var asm = new List<AsmNode>();
            var commands = GenAsm(current, asm);

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
