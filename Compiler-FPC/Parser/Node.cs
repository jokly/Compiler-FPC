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

    class VarNode : Node
    {
        public VarNode(Token token, Node left) : base(token)
        {
            Left = left;
        }
    }

    class ConstDeclNode : Node
    {
        public ConstDeclNode(Token token, List<Node> childrens) : base(token, "Declaration")
        {
            Childrens = childrens;
        }
    }

    class TypeDeclNode : Node
    {
        public TypeDeclNode(Token token, List<Node> childrens) : base(token, "Declaration")
        {
            Childrens = childrens;
        }
    }

    class RecordNode: Node
    {
        public RecordNode(Token token, List<Node> vars) : base(token, "Record")
        {
            Childrens = vars;
        }
    }

    class TypeNode : Node
    {
        public TypeNode(Token token, Node type) : base(token)
        {
            Left = type;
        }
    }

    class VarTypeNode : Node
    {
        public  VarTypeNode(Token token, string blockName = "") : base(token, blockName) { }
    }

    class ArrayTypeNode : VarTypeNode
    {
        public ArrayTypeNode(Token token, string leftRange, string rightRange, Node type) :
            base(token, "Array [" + leftRange + ".." + rightRange + "]")
        {
            Left = type;
        }
    }

    class PtrTypeNode : VarTypeNode
    {
        public PtrTypeNode(Token token, VarTypeNode type) : base(token)
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

    class WhileNode : Node
    {
        public WhileNode(Token token, ConditionNode cond, BlockNode beginBlock) : base(token)
        {
            Left = cond;
            Right = beginBlock;
        }
    }

    class ConditionNode : Node
    {
        public ConditionNode(Token token, Node expr) : base(token, "Condition of")
        {
            Left = expr;
        }
    }

    class AssignmentNode : Node
    {
        public AssignmentNode(Token token, Node left) : base(token)
        {
            Left = left;
        }
    }

    class ProcedureNode : Node
    {
        public ProcedureNode(Token token, ArgsNode args, List<Node> childrens) : base(token, "Procedure")
        {
            Childrens = childrens;
            Left = args;
        }
    }

    class FunctionNode : Node
    {
        public FunctionNode(Token token, ArgsNode args, ReturnValueNode returnValue, List<Node> childrens) : base(token, "Function")
        {
            Childrens = childrens;
            Left = args;
            Right = returnValue;
        }
    }

    class ArgsNode : Node
    {
        public ArgsNode(Token token, List<Node> args) : base(token, "Args of")
        {
            Childrens = args;
        }
    }

    class ReturnValueNode : Node
    {
        public ReturnValueNode(Token token, Node type) : base(token, "Return type")
        {
            Left = type;
        }
    }

    class FuncCallNode : ExprNode
    {
        public FuncCallNode(Token token, List<Node> args, Node call = null) : base(token, "Call function")
        {
            Childrens = args;
            Left = call;
        }
    }

    class ExprNode : Node
    {
        public ExprNode(Token token, string name = "") : base(token, name) { }
    }

    class UnOpNode : ExprNode
    {
        public UnOpNode(Token token, ExprNode right) : base(token)
        {
            Right = right;
        }
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
        public IdNode(Token token, SquareBracketsNode index = null) : base(token)
        {
            Left = index;
        }
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

    class StringConstNode : ConstNode
    {
        public StringConstNode(Token token) : base(token) { }
    }

    class PtrConstNode : ConstNode
    {
        public PtrConstNode(Token token, IdNode var) : base(token)
        {
            Left = var;
        }
    }

    class SquareBracketsNode : ExprNode
    {
        public SquareBracketsNode(Token token, List<Node> indexes) : base(token, "[ Index")
        {
            Childrens = indexes;
        }
    }
}
