﻿using Compiler_FPC.Generator;
using System;
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

        protected Symbol TrueNodeType = null;

        protected Symbol node_type;
        public Symbol NodeType
        {
            get { return node_type; }
         
            set
            {
                if (TrueNodeType == null)
                    TrueNodeType = node_type;

                node_type = value;
            }
        }

        public Node(Token token, string blockName = "")
        {
            Token = token;
            BlockName = blockName;
        }

        public Node(Token token, SymType type)
        {
            Token = token;
            NodeType = type;
        }

        public virtual List<AsmNode> Generate()
        {
            return new List<AsmNode>();
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

    class ConstVarNode : VarNode
    {
        public ConstVarNode(Token token, Node left) : base(token, left) { }

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();

            var trueType = TypeBuilder.GetTrueType(NodeType) as SymType;
            list.Add(new AsmSubNode("esp", trueType.Size.ToString()));
            list.Add(new AsmPopNode($"DWORD [ebp - {(NodeType as SymVar).Offset}]"));

            return list;
        }
    }

    class DeclVarNode : VarNode
    {
        public DeclVarNode(Token token, Node left) : base(token, left) { }

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();

            var trueType = TypeBuilder.GetTrueType(NodeType) as SymType;
            list.Add(new AsmSubNode("esp", trueType.Size.ToString()));
            
            return list;
        }
    }

    class AssignVarNode : VarNode
    {
        public AssignVarNode(Token token, Node left, Node index = null) : base(token, left) { Right = index;  }

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();

            if (Right == null)
                list.Add(new AsmPushNode($"{(NodeType as SymVar).Offset}"));
            else
            {
                list.Add(new AsmPopNode("eax"));
                list.Add(new AsmPushNode($"{(NodeType as SymVar).Offset}"));
                list.Add(new AsmPopNode("ebx"));
                list.Add(new AsmMulNode("ebx"));
                list.Add(new AsmPushNode("eax"));
            }

            list.Add(new AsmPushNode("ebp"));
            list.Add(new AsmPopNode("eax"));
            list.Add(new AsmPopNode("ebx"));
            list.Add(new AsmSubNode("eax", "ebx"));

            var destination = $"DWORD [eax]";
            var trueType = TypeBuilder.GetTrueType(NodeType);
            var exprType = TypeBuilder.GetTrueType(Left.Left.NodeType);

            if (!trueType.GetType().Equals(exprType.GetType()))
                throw new AsmGeneratorInvalidType(Token);

            if (trueType is SymTypeReal)
            {
                list.Add(new AsmFstpNode(destination));
            }
            else if (trueType is SymTypeInteger || trueType is SymTypeChar)
            {
                list.Add(new AsmPopNode(destination));
            }
            else
            {
                throw new AsmGeneratorInvalidType(Token);
            }

            return list;
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

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();

            var trueType = TypeBuilder.GetTrueType(NodeType) as SymType;

            var left = AsmGenerator.GenAsm(Childrens[0], new List<AsmNode>());
            var right = AsmGenerator.GenAsm(Childrens[1], new List<AsmNode>());

            list.Add(new AsmPopNode("eax"));
            list.Add(new AsmPopNode("ebx"));
            list.Add(new AsmSubNode("eax", "ebx"));
            list.Add(new AsmPushNode(trueType.Size.ToString()));
            list.Add(new AsmPopNode("ebx"));
            list.Add(new AsmMulNode("eax"));
            list.Add(new AsmSubNode("esp", "eax"));

            return list;
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

        public override List<AsmNode> Generate()
        {
            var empty = new List<AsmNode>();
            var cond = AsmGenerator.GenAsm(Left, empty);

            var beginBlock = new List<AsmNode>();
            foreach (var ch in Childrens[0].Childrens)
            {
                empty = new List<AsmNode>();
                beginBlock.AddRange(AsmGenerator.GenAsm(ch, empty));
            }

            var elseBlock = new List<AsmNode>();
            if (Childrens[1] != null)
            {
                empty = new List<AsmNode>();

                if (Childrens[1].Childrens[0] is IfNode)
                {
                    elseBlock = AsmGenerator.GenAsm(Childrens[1].Childrens[0], empty);
                }
                else
                {
                    foreach (var ch in Childrens[1].Childrens[0].Childrens)
                    {
                        empty = new List<AsmNode>();
                        elseBlock.AddRange(AsmGenerator.GenAsm(ch, empty));
                    }
                }
            }

            var list = new List<AsmNode>();
            int curr = AsmLabelNode.GetCurrent();

            list.AddRange(cond);
            list.Add(new AsmPopNode("eax"));
            list.Add(new AsmCmpNode("eax", "1"));
            list.Add(new AsmJeNode(AsmLabelNode.GenLabel(curr)));
            list.Add(new AsmJmpNode(AsmLabelNode.GenLabel(curr + 1)));
            list.Add(new AsmLabelNode());
            list.AddRange(beginBlock);
            list.Add(new AsmJmpNode(AsmLabelNode.GenLabel(curr + 2)));
            list.Add(new AsmLabelNode());
            list.AddRange(elseBlock);
            list.Add(new AsmLabelNode());

            return list;
        }
    }

    class WhileNode : Node
    {
        public WhileNode(Token token, ConditionNode cond, BlockNode beginBlock) : base(token)
        {
            Left = cond;
            Right = beginBlock;
        }

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();

            var empty = new List<AsmNode>();
            var cond = AsmGenerator.GenAsm(Left, empty);

            empty = new List<AsmNode>();
            var beginBlock = AsmGenerator.GenAsm(Right, empty);

            var curr = AsmLabelNode.GetCurrent();

            list.Add(new AsmLabelNode());
            list.AddRange(cond);
            list.Add(new AsmPopNode("eax"));
            list.Add(new AsmCmpNode("eax", "1"));
            list.Add(new AsmJneNode(AsmLabelNode.GenLabel(curr + 1)));
            list.AddRange(beginBlock);
            list.Add(new AsmJmpNode(AsmLabelNode.GenLabel(curr)));
            list.Add(new AsmLabelNode());

            return list;
        }
    }

    class RepeatNode : Node
    {
        public RepeatNode(Token token, ConditionNode cond, BlockNode beginBlock) : base(token)
        {
            Left = cond;
            Right = beginBlock;
        }

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();

            var empty = new List<AsmNode>();
            var cond = AsmGenerator.GenAsm(Left, empty);

            empty = new List<AsmNode>();
            var beginBlock = AsmGenerator.GenAsm(Right, empty);


            var curr = AsmLabelNode.GetCurrent();

            list.Add(new AsmLabelNode());
            list.AddRange(beginBlock);
            list.AddRange(cond);
            list.Add(new AsmPopNode("eax"));
            list.Add(new AsmCmpNode("eax", "0"));
            list.Add(new AsmJeNode(AsmLabelNode.GenLabel(curr)));


            return list;
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

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();

            var empty = new List<AsmNode>();
            var left = AsmGenerator.GenAsm(Left, empty);

            empty = new List<AsmNode>();
            var right = AsmGenerator.GenAsm(Right, empty);

            empty = new List<AsmNode>();
            var beginBlock = AsmGenerator.GenAsm(Childrens[0], empty);

            int curr = AsmLabelNode.GetCurrent();

            list.AddRange(right);
            list.AddRange(left);
            list.Add(new AsmLabelNode());
            list.Add(new AsmPopNode("eax"));
            list.Add(new AsmPopNode("ebx"));
            list.Add(new AsmCmpNode("eax", "ebx"));

            if (Childrens[1] is ForDirTo)
                list.Add(new AsmJgNode(AsmLabelNode.GenLabel(curr + 1)));
            else if (Childrens[1] is ForDirDownto)
                list.Add(new AsmJlNode(AsmLabelNode.GenLabel(curr + 1)));

            list.Add(new AsmPushNode("ebx"));
            list.Add(new AsmPushNode("eax"));
            list.AddRange(beginBlock);

            list.Add(new AsmPopNode("eax"));
            if (Childrens[1] is ForDirTo)
                list.Add(new AsmAddNode("eax", "1"));
            else if (Childrens[1] is ForDirDownto)
                list.Add(new AsmSubNode("eax", "1"));

            list.Add(new AsmPushNode("eax"));
            list.Add(new AsmJmpNode(AsmLabelNode.GenLabel(curr)));
            list.Add(new AsmLabelNode());

            return list;
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

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();
            var child = Childrens[0] is SquareBracketsNode ? Childrens[0].Left : Childrens[0];
            var child_type = TypeBuilder.GetTrueType(child, child.NodeType);

            if (Token.Value.Equals("write") || Token.Value.Equals("writeln"))
            {
                var format_node = new AsmPushNode(Token.Value + TypeToSuff(child_type));
                var offset = 8;

                if (child_type is SymTypeReal)
                {
                    list.Add(new AsmSubNode("esp", "8"));
                    list.Add(new AsmFstpNode("QWORD [esp]"));
                    offset = 12;
                }

                list.Add(format_node);
                list.Add(new AsmCallNode("_printf"));
                list.Add(new AsmAddNode("esp", offset.ToString()));
            }
            else if (Token.Value.Equals("read"))
            {
                var destination = child.Token.Value;

                if (!(child_type is SymTypeReal))
                    list.Add(new AsmPopNode($"DWORD [{destination}]"));

                list.Add(new AsmPushNode($"{destination}"));
                list.Add(new AsmPushNode(Token.Value + TypeToSuff(child_type)));
                list.Add(new AsmCallNode("_scanf"));
                list.Add(new AsmAddNode("esp", "8"));
            }

            return list;
        }

        private string TypeToSuff(Symbol type)
        {
            if (type is SymVar)
                return TypeToSuff((type as SymVar).Type);
            else if (type is SymTypeInteger)
                return "Int";
            else if (type is SymTypeReal)
                return "Real";
            else if (type is SymTypeChar)
                return "Str";
            else
                throw new AsmGeneratorInvalidType(Token);
        }
    }

    class ExprNode : Node
    {
        public ExprNode(Token token, string name = "") : base(token, name) { }
        public ExprNode(Token token, SymType type) : base(token, type) { }
    }

    class UnOpNode : ExprNode
    {
        public UnOpNode(Token token, ExprNode right, SymType type) : base(token, type)
        {
            Right = right;
        }

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();

            if (NodeType is SymTypeReal)
            {
                if (Token.Type == TokenType.MINUS)
                {
                    list.Add(new AsmFchsNode());
                }
            }
            else if (NodeType is SymTypeInteger)
            {
                list.Add(new AsmPopNode("eax"));

                if (Token.Type == TokenType.MINUS)
                {
                    list.Add(new AsmNegNode("eax"));
                    
                }
                else if (Token.Text.Equals("not"))
                {
                    int curr = AsmLabelNode.GetCurrent();
                    
                    list.Add(new AsmCmpNode("eax", "0"));
                    list.Add(new AsmJeNode(AsmLabelNode.GenLabel(curr)));
                    list.Add(new AsmPushNode("0"));
                    list.Add(new AsmJmpNode(AsmLabelNode.GenLabel(curr + 1)));
                    list.Add(new AsmLabelNode());
                    list.Add(new AsmPushNode("1"));
                    list.Add(new AsmLabelNode());
                    list.Add(new AsmPopNode("eax"));
                }

                list.Add(new AsmPushNode("eax"));
            }
            else
                throw new AsmGeneratorInvalidType(Token);

            return list;
        }
    }

    class BinOpNode : ExprNode
    {
        public BinOpNode(Token token, ExprNode left, ExprNode right, SymType type) : base(token, type)
        {
            Left = left;
            Right = right;
        }

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();

            if (NodeType is SymTypeReal)
            {
                if (Token.Type == TokenType.ASTERIX)
                {
                    list.Add(new AsmFmulpNode());
                }
                else if (Token.Type == TokenType.FORWARD_SLASH)
                {
                    list.Add(new AsmFdivpNode());
                }
                else if (Token.Type == TokenType.PLUS)
                {
                    list.Add(new AsmFaddpNode());
                }
                else if (Token.Type == TokenType.MINUS)
                {
                    list.Add(new AsmFsubpNode());
                }
            }
            else if (NodeType is SymTypeInteger)
            {
                list.Add(new AsmPopNode("ebx"));
                list.Add(new AsmPopNode("eax"));

                if (Token.Type == TokenType.ASTERIX)
                {
                    list.Add(new AsmMulNode("ebx"));
                }
                else if (Token.Type == TokenType.PLUS)
                {
                    list.Add(new AsmAddNode("eax", "ebx"));
                }
                else if (Token.Type == TokenType.MINUS)
                {
                    list.Add(new AsmSubNode("eax", "ebx"));
                }
                else if (Token.Value.Equals("div"))
                {
                    list.Add(new AsmDivNode("ebx"));
                }
                else if (Token.Value.Equals("mod"))
                {
                    list.Add(new AsmDivNode("ebx"));
                    list.Add(new AsmMoveNode("eax", "edx"));
                }
                else if (Token.Value.Equals("shl") || Token.Type == TokenType.BITWISE_SL)
                {
                    list.Add(new AsmPushNode("ebx"));
                    list.Add(new AsmMoveNode("cl", "[esp]"));
                    list.Add(new AsmPopNode("ebx"));
                    list.Add(new AsmShlNode("eax", "cl"));
                }
                else if (Token.Value.Equals("shr") || Token.Type == TokenType.BITWISE_SR)
                {
                    list.Add(new AsmPushNode("ebx"));
                    list.Add(new AsmMoveNode("cl", "[esp]"));
                    list.Add(new AsmPopNode("ebx"));
                    list.Add(new AsmShrNode("eax", "cl"));
                }
                else if (Token.Text.Equals("and"))
                {
                    list.Add(new AsmAndNode("eax", "ebx"));
                }
                else if (Token.Text.Equals("or"))
                {
                    list.Add(new AsmOrNode("eax", "ebx"));
                }
                else if (Token.Text.Equals("xor"))
                {
                    list.Add(new AsmXorNode("eax", "ebx"));
                }
                else if (Token.Type == TokenType.RELOP_EQ || Token.Type == TokenType.RELOP_NE ||
                          Token.Type == TokenType.RELOP_LT || Token.Type == TokenType.RELOP_GT ||
                          Token.Type == TokenType.RELOP_LE || Token.Type == TokenType.RELOP_GE)
                {
                    list.Add(new AsmCmpNode("eax", "ebx"));

                    int curr = AsmLabelNode.GetCurrent();
                    string true_label = AsmLabelNode.GenLabel(curr);
                    string false_label = AsmLabelNode.GenLabel(curr + 1);

                    if (Token.Type == TokenType.RELOP_EQ)
                        list.Add(new AsmJeNode(true_label));
                    else if (Token.Type == TokenType.RELOP_NE)
                        list.Add(new AsmJneNode(true_label));
                    else if (Token.Type == TokenType.RELOP_LT)
                        list.Add(new AsmJlNode(true_label));
                    else if (Token.Type == TokenType.RELOP_GT)
                        list.Add(new AsmJgNode(true_label));
                    else if (Token.Type == TokenType.RELOP_LE)
                        list.Add(new AsmJleNode(true_label));
                    else if (Token.Type == TokenType.RELOP_GE)
                        list.Add(new AsmJgeNode(true_label));

                    list.Add(new AsmPushNode("0"));
                    list.Add(new AsmJmpNode(false_label));
                    list.Add(new AsmLabelNode());
                    list.Add(new AsmPushNode("1"));
                    list.Add(new AsmLabelNode());
                    list.Add(new AsmPopNode("eax"));
                }

                list.Add(new AsmPushNode("eax"));
            }
            else
                throw new AsmGeneratorInvalidType(Token);

            return list;
        }
    }

    class IdNode : ExprNode
    {
        public IdNode(Token token, ExprNode callings) : base(token)
        {
            Left = callings;
        }

        public IdNode(Token token, SquareBracketsNode index = null, DotNode record = null) : base(token)
        {
            Left = index;
            Right = record;
        }

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();
            
            if (Left != null)
            {
                list.AddRange(AsmGenerator.GenAsm(Left.Left, new List<AsmNode>()));
                list.Add(new AsmPushNode((NodeType as SymType).Size.ToString()));
                list.Add(new AsmPopNode("eax"));
                list.Add(new AsmPopNode("ebx"));
                list.Add(new AsmMulNode("ebx"));
                list.Add(new AsmPushNode("eax"));
            }
            else
                list.Add(new AsmPushNode(((TrueNodeType == null ? NodeType : TrueNodeType) as SymVar).Offset.ToString()));


            list.Add(new AsmPopNode("ebx"));
            list.Add(new AsmPushNode("ebp"));
            list.Add(new AsmPopNode("eax"));
            list.Add(new AsmSubNode("eax", "ebx"));

            list.Add(new AsmPushNode($"DWORD [eax]"));

            var type = TypeBuilder.GetTrueType(this, NodeType);
            var true_type = TypeBuilder.GetTrueType(this, TrueNodeType);

            if (type is SymTypeReal)
            {
                if (true_type is SymTypeInteger)
                    list.Add(new AsmFildNode("DWORD [esp]"));
                else
                    list.Add(new AsmFldNode("DWORD [esp]"));

                list.Add(new AsmPopNode("eax"));
            }

            return list;
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
        public ConstNode(Token token, SymType type) : base(token, type) { }

        public override List<AsmNode> Generate()
        {
            var list = new List<AsmNode>();

            if (NodeType is SymTypeReal)
            {
                float num = (float)Convert.ToDouble(Token.Value);
                var i = BitConverter.ToInt32(BitConverter.GetBytes(num), 0);
                var hex_val = "0x" + i.ToString("X") + $"; {Token.Value}";

                list.Add(new AsmPushNode(hex_val));
                list.Add(new AsmFldNode("DWORD [esp]"));
                list.Add(new AsmPopNode("eax"));
            }
            else if (NodeType is SymTypeInteger)
            {
                int num = Convert.ToInt32(Token.Value);
                var i = BitConverter.ToInt32(BitConverter.GetBytes(num), 0);
                var hex_val = "0x" + i.ToString("X") + $"; {Token.Value}";

                list.Add(new AsmPushNode(hex_val));
            }
            else if (NodeType is SymTypeChar)
            {
                int chr = Token.Value[0];
                var hex_val = "0x" + chr.ToString("X") + $"; {Token.Value}";

                list.Add(new AsmPushNode(hex_val));
            }

            return list;
        }
    }

    class IntConstNode : ConstNode
    {
        public IntConstNode(Token token) : base(token) { }
        public IntConstNode(Token token, SymType type) : base(token, type) { }
    }

    class RealConstNode : ConstNode
    {
        public RealConstNode(Token token) : base(token) { }
        public RealConstNode(Token token, SymType type) : base(token, type) { }
    }

    class CharConstNode : ConstNode
    {
        public CharConstNode(Token token) : base(token) { }
        public CharConstNode(Token token, SymType type) : base(token, type) { }
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
        public SquareBracketsNode(Token token, Node index, Node child) : base(token, "[ Index")
        {
            Left = index;
            Right = child;
        }
    }
}
