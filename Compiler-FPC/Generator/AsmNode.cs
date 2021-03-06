﻿
using System.Collections.Generic;

namespace Compiler_FPC.Generator
{
    enum AsmDataType
    {
        DB, DW, DD, DQ
    }

    enum AsmBssType
    {
        RESB, RESW, RESD, RESQ
    }

    class AsmNode
    {
        public AsmNode() { }

        public override string ToString() => base.ToString();
    }

    class AsmSectionNode : AsmNode
    {
        public string Name { get; private set; }

        public AsmSectionNode(string name)
        {
            Name = name;
        }

        public override string ToString() => $"section {Name}";
    }

    class AsmSectionDataNode : AsmNode
    {

    }

    class AsmDataNode : AsmSectionDataNode
    {
        public string ID { get; private set; }
        public AsmDataType Type { get; private set; }
        public string Value { get; private set; }

        public AsmDataNode(string id, AsmDataType type, string value)
        {
            ID = id;
            Type = type;
            Value = value;
        }

        public override string ToString() => $"{ID}: {Type} {Value}";
    }

    class AsmSectionBssNode : AsmNode
    {

    }

    class AsmBssNode : AsmSectionBssNode
    {
        public string ID { get; private set; }
        public AsmBssType Type { get; private set; }
        public int Bytes { get; private set; }

        public AsmBssNode(string id, AsmBssType type, int bytes = 4)
        {
            ID = id;
            Type = type;
            Bytes = bytes;
        }

        public override string ToString() => $"{ID}: {Type} {Bytes}";
    }

    class AsmSectionProgramNode : AsmNode
    {

    }

    class AsmPushNode : AsmSectionProgramNode
    {
        public string Value { get; private set; }

        public AsmPushNode(string value)
        {
            Value = value;
        }

        public override string ToString() => $"push {Value}";
    }

    class AsmPopNode : AsmSectionProgramNode
    {
        public string Destination { get; private set; }

        public AsmPopNode(string destination)
        {
            Destination = destination;
        }

        public AsmPopNode(AsmBssNode destination)
        {
            Destination = $"DWORD [{destination.ID}]";
        }

        public override string ToString() => $"pop {Destination}";
    }

    class AsmMoveNode : AsmSectionProgramNode
    {
        public string To { get; private set; }
        public string From { get; private set; }

        public AsmMoveNode(string to, string from)
        {
            To = to;
            From = from;
        }

        public override string ToString() => $"mov {To}, {From}";
    }

    class AsmFldNode : AsmSectionProgramNode
    {
        public string Operand { get; private set; }

        public AsmFldNode(string operand)
        {
            Operand = operand;
        }

        public override string ToString() => $"fld {Operand}";
    }

    class AsmFildNode : AsmSectionProgramNode
    {
        public string Operand { get; private set; }

        public AsmFildNode(string operand)
        {
            Operand = operand;
        }

        public override string ToString() => $"fild {Operand}";
    }

    class AsmFstpNode : AsmSectionProgramNode
    {
        public string Operand { get; private set; }

        public AsmFstpNode(string operand)
        {
            Operand = operand;   
        }

        public override string ToString() => $"fstp {Operand}";
    }

    class AsmUnOpNode : AsmSectionProgramNode
    {
        public string Operand { get; private set; }

        public AsmUnOpNode(string operand)
        {
            Operand = operand;
        }

        public override string ToString() => base.ToString();
    }

    class AsmNegNode : AsmUnOpNode
    {
        public AsmNegNode(string operand) : base(operand) { }

        public override string ToString() => $"neg {Operand}";
    }

    class AsmFchsNode : AsmUnOpNode
    {
        public AsmFchsNode() : base("") { }

        public override string ToString() => $"fchs";
    }

    class AsmNotNode : AsmUnOpNode
    {
        public AsmNotNode(string operand) : base(operand) { }

        public override string ToString() => $"not {Operand}";
    }

    class AsmIncNode : AsmUnOpNode
    {
        public AsmIncNode(string operand) : base(operand) { }

        public override string ToString() => $"inc {Operand}";
    }

    class AsmBinOpNode : AsmSectionProgramNode
    {
        public string Left { get; private set; }
        public string Right { get; private set; }

        public AsmBinOpNode(string left, string right)
        {
            Left = left;
            Right = right;
        }

        public override string ToString() => base.ToString();
    }

    class AsmAndNode : AsmBinOpNode
    {
        public AsmAndNode(string left, string right) : base(left, right) { }

        public override string ToString() => $"and {Left}, {Right}";

    }

    class AsmOrNode : AsmBinOpNode
    {
        public AsmOrNode(string left, string right) : base(left, right) { }

        public override string ToString() => $"or {Left}, {Right}";
    }

    class AsmXorNode : AsmBinOpNode
    {
        public AsmXorNode(string left, string right) : base(left, right) { }

        public override string ToString() => $"xor {Left}, {Right}";
    }

    class AsmCmpNode : AsmBinOpNode
    {
        public AsmCmpNode(string left, string right) : base(left, right) { }

        public override string ToString() => $"cmp {Left}, {Right}";
    }

    class AsmAddNode : AsmBinOpNode
    {
        public AsmAddNode(string left, string right) : base(left, right) { }

        public override string ToString() => $"add {Left}, {Right}";
    }

    class AsmSubNode : AsmBinOpNode
    {
        public AsmSubNode(string left, string right) : base(left, right) { }

        public override string ToString() => $"sub {Left}, {Right}";
    }

    class AsmMulNode : AsmBinOpNode
    {
        public AsmMulNode(string right) : base("", right) { }

        public override string ToString() => $"mul {Right}";
    }

    class AsmDivNode : AsmBinOpNode
    {
        public AsmDivNode(string right) : base("", right) { }

        public override string ToString() => $"div {Right}";
    }

    class AsmShlNode : AsmBinOpNode
    {
        public AsmShlNode(string left, string right) : base(left, right) { }

        public override string ToString() => $"shl {Left}, {Right}";
    }

    class AsmShrNode : AsmBinOpNode
    {
        public AsmShrNode(string left, string right) : base(left, right) { }

        public override string ToString() => $"shr {Left}, {Right}";
    }

    class AsmFaddpNode : AsmBinOpNode
    {
        public AsmFaddpNode() : base("", "") { }

        public override string ToString() => $"faddp";
    }

    class AsmFsubpNode : AsmBinOpNode
    {
        public AsmFsubpNode() : base("", "") { }

        public override string ToString() => $"fsubp";
    }

    class AsmFmulpNode : AsmBinOpNode
    {
        public AsmFmulpNode() : base ("", "") { }

        public override string ToString() => $"fmulp";
    }

    class AsmFdivpNode : AsmBinOpNode
    {
        public AsmFdivpNode() : base("", "") { }

        public override string ToString() => $"fdivp";
    }

    class AsmCallNode : AsmSectionProgramNode
    {
        public string Name { get; private set; }

        public AsmCallNode(string name)
        {
            Name = name;
        }

        public override string ToString() => $"call {Name}";
    }

    class AsmLabelNode : AsmSectionProgramNode
    {
        public string Name { get; protected set; }
        private static int Current = 0;

        public AsmLabelNode()
        {
            Name = GenLabel(Current);
            Current++;
        }

        public AsmLabelNode(string name) { Name = name; }

        public static int GetCurrent()
        {
            return Current;
        }

        public static string GenLabel(int num)
        {
            return "L" + num.ToString();
        }

        public override string ToString() => $"{Name}:";
    }

    class AsmJmpNode : AsmSectionProgramNode
    {
        public string Label { get; protected set; }

        public AsmJmpNode(string label) { Label = label; }

        public override string ToString() => $"jmp {Label}";
    }

    class AsmJeNode : AsmJmpNode
    {
        public AsmJeNode(string label) : base(label) { }

        public override string ToString() => $"je {Label}";
    }

    class AsmJneNode : AsmJmpNode
    {
        public AsmJneNode(string label) : base(label) { }

        public override string ToString() => $"jne {Label}";
    }

    class AsmJgNode : AsmJmpNode
    {
        public AsmJgNode(string label) : base(label) { }

        public override string ToString() => $"jg {Label}";
    }

    class AsmJgeNode : AsmJmpNode
    {
        public AsmJgeNode(string label) : base(label) { }

        public override string ToString() => $"jge {Label}";
    }

    class AsmJlNode : AsmJmpNode
    {
        public AsmJlNode(string label) : base(label) { }

        public override string ToString() => $"jl {Label}";
    }

    class AsmJleNode : AsmJmpNode
    {
        public AsmJleNode(string label) : base(label) { }

        public override string ToString() => $"jle {Label}";
    }
}
