using System.Collections.Generic;

namespace Compiler_FPC.Parser
{
    class Node
    {
        public Node Left { get; protected set; } = null;
        public Node Right { get; protected set; } = null;
        public Token Token { get; } = null;

        public Node(Token token)
        {
            this.Token = token;
        }
    }

    class ProgramNode : Node
    {
        public ProgramNode(Token token) : base(token) { }
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
