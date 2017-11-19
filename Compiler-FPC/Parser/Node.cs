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
        public ArrayTypeNode(Token token, Node leftRange, Node rightRange, Node type) :
            base(token, "Array")
        {
            Left = type;
            Childrens = new List<Node> { leftRange, rightRange };
        }
    }

    class PtrTypeNode : VarTypeNode
    {
        public PtrTypeNode(Token token, VarTypeNode type) : base(token)
        {
            Left = type;
        }
    }

    class ProcTypeNode : VarTypeNode
    {
        public ProcTypeNode(Token token, ArgsNode args) : base(token)
        {
            Left = args;
        }
    }

    class FuncTypeNode : VarTypeNode
    {
        public FuncTypeNode(Token token, ArgsNode args, ReturnValueNode returnValue) : base(token)
        {
            Left = args;
            Right = returnValue;
        }
    }

    class BlockNode : Node
    {
        public BlockNode(Token token, List<Node> childrens = null) : base(token, "Block")
        {
            Childrens = childrens;
        }
    }

    class IfNode : BlockNode
    {
        public IfNode(Token token, Node cond, Node beginBlock, Node elseNode) : base(token)
        {
            Left = cond;
            Childrens = new List<Node>() { beginBlock, elseNode };
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

    class RepeatNode : Node
    {
        public RepeatNode(Token token, ConditionNode cond, BlockNode beginBlock) : base(token)
        {
            Left = cond;
            Right = beginBlock;
        }
    }

    class ForNode : Node
    {
        public ForNode(Token token, Node startVal, Node endVal, Node direction, Node beginBlock) : base(token)
        {
            Left = startVal;
            Right = endVal;
            Childrens = new List<Node> { beginBlock, direction };
        }
    }

    class ForDirTo : Node
    {
        public ForDirTo(Token token) : base(token, "Direction") { }
    }

    class ForDirDownto : Node
    {
        public ForDirDownto(Token token) : base(token, "Direction") { }
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

    class ForwardDecl : Node
    {
        public ForwardDecl(Token token) : base(token) { }
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
        public IdNode(Token token, SquareBracketsNode index = null, DotNode record = null) : base(token)
        {
            Left = index;
            Right = record;
        }
    }

    class DotNode : ExprNode
    {
        public DotNode(Token token, ExprNode child) : base(token)
        {
            Left = child;
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

    class CharConstNode : ConstNode
    {
        public CharConstNode(Token token) : base(token) { }
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
