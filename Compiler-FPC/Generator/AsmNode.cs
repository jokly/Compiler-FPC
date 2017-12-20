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

    class AsmAddNode : AsmBinOpNode
    {
        public AsmAddNode(string left, string right) : base(left, right) { }

        public override string ToString() => $"add {Left}, {Right}";
    }

    class AsmMulNode : AsmBinOpNode
    {
        public AsmMulNode(string right) : base("", right) { }

        public override string ToString() => $"mul {Right}";
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
}
