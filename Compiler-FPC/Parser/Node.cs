using System.Collections.Generic;

namespace Compiler_FPC.Parser
{
    class Node
    {
        public Node Left { get; protected set; } = null;
        public Node Right { get; protected set; } = null;
        public Token Token { get; } = null;
        public string BlockName { get; protected set; } = "";
        public List<Node> Childrens { get; protected set; } = new List<Node>();

        public Node(Token token, string blockName = "")
        {
            Token = token;
            BlockName = blockName;
        }
    }

    class ProgramNode : Node
    {
        public ProgramNode(Token token, List<Node> childrens) : base(token, "Program")
        {
            Childrens = childrens;
        }
    }

    class DeclarationNode : Node
    {
        public DeclarationNode(Token token, List<Node> childrens) : base(token, "Declaration")
        {
            Childrens = childrens;
        }
    }

    class Var : Node
    {
        public Var(Token token, Node left) : base(token)
        {
            Left = left;
        }
    }

    class VarType : Node
    {
        public  VarType(Token token) : base(token) { }
    }

    class ArrayType : Node
    {
        public ArrayType(Token token, string leftRange, string rightRange, Node type) :
            base(token, "Array [" + leftRange + ".." + rightRange + "]")
        {
            Left = type;
        }
    }

    class BlockNode : Node
    {
        public BlockNode(Token token, List<Node> childrens) : base(token, "Block")
        {
            Childrens = childrens;
        }
    }

    class ExprNode : Node
    {
        public ExprNode(Token token) : base(token) { }
    }

    class BinOpNode : ExprNode
    {
        public BinOpNode(Token token, ExprNode left, ExprNode right) : base(token)
        {
            Left = left;
            Right = right;
        }
    }

    class IdNode : ExprNode
    {
        public IdNode(Token token) : base(token) { }
    }

    class ConstNode : ExprNode
    {
        public ConstNode(Token token) : base(token) { }
    }

    class IntConstNode : ConstNode
    {
        public IntConstNode(Token token) : base(token) { }
    }

    class RealConstNode : ConstNode
    {
        public RealConstNode(Token token) : base(token) { }
    }
}
