
using System.Collections.Generic;

namespace Compiler_FPC.Parser
{
    class SymType : Symbol
    {
        public SymType(Node node = null) : base(node)
        {

        }
    }

    class SymTypeScalar : SymType
    {
        public SymTypeScalar() : base()
        {

        }
    }

    class SymTypeInteger : SymTypeScalar
    {
        public SymTypeInteger() : base()
        {

        }
    }

    class SymTypeReal : SymTypeScalar
    {
        public SymTypeReal() : base()
        {

        }
    }

    class SymTypeChar : SymTypeScalar
    {
        public SymTypeChar() : base()
        {

        }
    }

    class SymTypeArray : SymType
    {
        public SymType ElemType { get; protected set; }
        public Node Start { get; protected set; }
        public Node End { get; protected set; }

        public SymTypeArray(SymType elemType, Node start, Node end) : base()
        {
            ElemType = elemType;
            Start = start;
            End = end;
        }
    }

    class SymTypeRecord : SymType
    {
        public Dictionary<string, SymType> Fields { get; protected set; }

        public SymTypeRecord(Node node, Dictionary<string, SymType> fields) : base(node)
        {
            Fields = fields;
        }
    }

    class SymTypePointer : SymType
    {
        public SymType Type { get; protected set; }

        public SymTypePointer(SymType symType)
        {
            Type = symType;
        }
    }

    class SymTypeAlias : SymType
    {
        public SymType Type { get; protected set; }

        public SymTypeAlias(Node node, SymType symType) : base(node)
        {
            Type = symType;
        }
    }

    class SymTypeProc : SymType
    {
        public bool IsForward { get; set; } = false;
        public List<SymType> Args;

        public SymTypeProc(Node node, List<SymType> args) : base(node)
        {
            Args = args;
        }
    }

    class SymTypeFunc : SymTypeProc
    {
        public SymType ReturnType { get; protected set; }

        public SymTypeFunc(Node node, List<SymType> args, SymType returnedType) : base(node, args)
        {
            ReturnType = returnedType;
        }
    }
}
