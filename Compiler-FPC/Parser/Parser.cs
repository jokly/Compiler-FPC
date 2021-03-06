﻿using System.Collections.Generic;
using Compiler_FPC.Generator;

namespace Compiler_FPC.Parser
{
    class Parser
    {
        private readonly Tokenizer tokenizer;
        public SyntaxTree tree { get; private set; }
        private SymbolTableTree tables;

        private Parser()
        {
            tables = new SymbolTableTree();
            AddBuiltIn();
        }

        public Parser(string fileName) : this()
        {
            tokenizer = new Tokenizer(fileName);
        }

        public Parser(Tokenizer tokenizer) : this()
        {
            this.tokenizer = tokenizer;
        }

        public string Tree()
        {
            return BuildTree();
        }

        public string BuildTree()
        {
            if (tree == null)
            {
                tokenizer.Next();

                try
                {
                    tree = new SyntaxTree(parseProgram());
                }
                catch (CompilerException ex)
                {
                    return ex.Message;
                }
            }

            return tree.TreeString;
        }

        private void AddBuiltIn()
        {
            var scalarList = new List<SymType> { new SymTypeScalar() };

            tables.AddSymbol(new SymTypeProc(new Node(new Token(-1, -1, TokenType.WRITE, "write", "write")), scalarList));
            tables.AddSymbol(new SymTypeProc(new Node(new Token(-1, -1, TokenType.WRITELN, "writeln", "writeln")), scalarList));
            tables.AddSymbol(new SymTypeProc(new Node(new Token(-1, -1, TokenType.READ, "read", "read")), scalarList));
        }

        private Token matchNext(TokenType expectedTkType)
        {
            var next = tokenizer.Next();

            if (next != null && expectedTkType != tokenizer.Current.Type)
                throw new ParserException(next, expectedTkType);

            return next == null ? null : tokenizer.Current;
        }

        private Token matchNext(string expectedTkValue)
        {
            var next = tokenizer.Next();

            if (next != null && !expectedTkValue.Equals(tokenizer.Current.Value))
                throw new ParserException(next, expectedTkValue);

            return next == null ? null : tokenizer.Current;
        }

        private void require(TokenType expectedTkType)
        {
            if (tokenizer.Current != null && expectedTkType != tokenizer.Current.Type)
                throw new ParserException(tokenizer.Current, expectedTkType);
        }

        private void require(string expectedTkValue)
        {
            if (tokenizer.Current != null && !expectedTkValue.Equals(tokenizer.Current.Value))
                throw new ParserException(tokenizer.Current, expectedTkValue);
        }

        private Node parseProgram()
        {
            if (tokenizer.Current.Value.Equals("program"))
            {
                var programName = matchNext(TokenType.ID);
                matchNext(TokenType.SEMICOLON);
                tokenizer.Next();
                
                return new ProgramNode(programName, parseBlocks(true));
            }
            else
            {
                return parseExpr();
            }
        }

        private List<Node> parseBlocks(bool isMain = false)
        {
            List<Node> blocks = new List<Node>();
            Node block;
            while ((block = parseBlock(isMain)) != null)
            {
                blocks.Add(block);

                if (block is BlockNode || block is ForwardDecl)
                    return blocks;
            }

            return blocks;
        }

        private Node parseBlock(bool isMain = false)
        {
            switch(tokenizer.Current.Value)
            {
                case "const":
                    return new ConstDeclNode(tokenizer.Current, parseConstDecl());
                case "type":
                    return new TypeDeclNode(tokenizer.Current, parseTypeDecl());
                case "var":
                    return new DeclarationNode(tokenizer.Current, parseVar());
                case "begin":
                    return parseBeginBlock(isMain);
                case "forward":
                    var name = tokenizer.Current;
                    matchNext(TokenType.SEMICOLON);
                    tokenizer.Next();
                    return new ForwardDecl(name);
                case "procedure":
                    tables.NewTable();
                    var t = matchNext(TokenType.ID);
                    var p_args = parseArgs(tokenizer.Current);
                    var proc = new ProcedureNode(t, p_args, parseBlocks());
                    tables.BackTable();

                    var p_type_args = TypeBuilder.GetArgsType(p_args.Childrens, tables);
                    var typeProc = new SymTypeProc(proc, p_type_args);

                    if (proc.Childrens[0] is ForwardDecl)
                        typeProc.IsForward = true;

                    tables.AddSymbol(typeProc);
                    return proc;
                case "function":
                    tables.NewTable();
                    var funcName = matchNext(TokenType.ID);
                    var f_args = parseArgs(funcName, true);
                    var func = new FunctionNode(funcName, f_args, parseReturnValue(funcName), parseBlocks());
                    tables.BackTable();

                    var f_type_args = TypeBuilder.GetArgsType(f_args.Childrens, tables);
                    var typeFunc = new SymTypeFunc(func, f_type_args, TypeBuilder.Build(func.Right, tables));

                    if (func.Childrens[0] is ForwardDecl)
                        typeFunc.IsForward = true;

                    tables.AddSymbol(typeFunc);
                    return func;
                case "EOF":
                    return null;
                default:
                    throw new ParserException(tokenizer.Current);
            }
        }

        private BlockNode parseBeginBlock(bool isMain = false, bool isUntil = false)
        {
            var blockName = tokenizer.Current;
            var begNode = parseBegin(isUntil);

            if (!isUntil)
                tokenizer.Next();
            else
                require("until");
            
            if (!isUntil && (isMain && tokenizer.Current.Type != TokenType.DOT))
            {
                throw new ParserException(tokenizer.Current, ".");
            }
            else if ((!isUntil && (!isMain && tokenizer.Current.Type != TokenType.SEMICOLON)))
            {
                throw new ParserException(tokenizer.Current, ";");
            }

            tokenizer.Next();

            return new BlockNode(blockName, begNode);
        }

        private ArgsNode parseArgs(Token nameProc, bool isFunc = false)
        {
            matchNext(TokenType.LBRACKET);
            var args = parseVar(true);

            if (!isFunc)
            {
                matchNext(TokenType.SEMICOLON);
                tokenizer.Next();
            }

            return new ArgsNode(nameProc, args);
        }

        private ReturnValueNode parseReturnValue(Token nameFunc)
        {
            matchNext(TokenType.COLON);
            tokenizer.Next();

            var funcRetType = getVarType();
            matchNext(TokenType.SEMICOLON);
            tokenizer.Next();

            return new ReturnValueNode(nameFunc, funcRetType);
        }

        private List<Node> parseConstDecl()
        {
            List<Node> consts = new List<Node>();

            while (true)
            {
                var nameToken = tokenizer.Next();

                if (nameToken.Type == TokenType.KEY_WORD && consts.Count == 0)
                {
                    throw new ParserException(nameToken, TokenType.ID);
                }
                else if (nameToken.Type == TokenType.KEY_WORD)
                {
                    return consts;
                }

                require(TokenType.ID);

                matchNext(TokenType.RELOP_EQ);
                tokenizer.Next();

                var expr = parseExpr();
                var varNode = new ConstVarNode(nameToken, expr);
                var type = new SymVar(varNode, TypeBuilder.Build(expr, tables));
                tables.AddSymbol(type);
                varNode.NodeType = type;
                consts.Add(varNode);

                require(TokenType.SEMICOLON);
            }
        }

        private List<Node> parseTypeDecl()
        {
            List<Node> types = new List<Node>();
            tokenizer.Next();

            while (true)
            {
                var nameToken = tokenizer.Current;

                if (nameToken.Type == TokenType.KEY_WORD && types.Count == 0)
                {
                    throw new ParserException(nameToken, TokenType.ID);
                }
                else if (nameToken.Type == TokenType.KEY_WORD)
                {
                    return types;
                }

                require(TokenType.ID);

                matchNext(TokenType.RELOP_EQ);
                tokenizer.Next();

                if (tokenizer.Current.Value.Equals("record"))
                {
                    tables.NewTable();
                    var recordNode = new RecordNode(nameToken, parseVar());
                    tables.BackTable();

                    types.Add(recordNode);
                    tables.AddSymbol(TypeBuilder.Build(recordNode, tables));

                    require("end");
                    matchNext(TokenType.SEMICOLON);
                    tokenizer.Next();
                }
                else
                {
                    tables.NewTable();
                    var typeNode = new TypeNode(nameToken, getType());
                    tables.BackTable();

                    tables.AddSymbol(TypeBuilder.Build(typeNode, tables));
                    types.Add(typeNode);
                }
            }
        }

        private VarTypeNode getType()
        {
            if (tokenizer.Current.Value.Equals("function"))
            {
                var funcName = tokenizer.Current;
                return new FuncTypeNode(funcName, parseArgs(funcName, true), parseReturnValue(funcName));
            }
            else if (tokenizer.Current.Value.Equals("procedure"))
            {
                var procName = tokenizer.Current;
                return new ProcTypeNode(procName, parseArgs(tokenizer.Current));
            }

            var returnVal = getVarType();
            matchNext(TokenType.SEMICOLON);
            tokenizer.Next();

            return returnVal;
        }

        private List<Node> parseVar(bool isArgs = false)
        {
            List<Node> vars = new List<Node>();

            // Parse name of vars
            while (true)
            {
                List<Token> tokensVars = new List<Token>();

                while (true)
                {
                    var next = tokenizer.Next();

                    if (vars.Count == 0 && next.Type == TokenType.KEY_WORD)
                    {
                        throw new ParserException(next, TokenType.ID);
                    }
                    else if (next.Type == TokenType.KEY_WORD || (isArgs && next.Type == TokenType.RBRACKET))
                    {
                        return vars;
                    }
                    else if (next.Type != TokenType.ID)
                    {
                        throw new ParserException(next, TokenType.ID);
                    }

                    tokensVars.Add(tokenizer.Current);
                    tokenizer.Next();

                    if (tokenizer.Current.Type == TokenType.COMMA)
                    {
                        continue;
                    }
                    else if (tokenizer.Current.Type == TokenType.COLON)
                    {
                        break;
                    }
                    else
                    {
                        throw new ParserException(tokenizer.Current);
                    }
                }

                tokenizer.Next();

                // Parse type of vars
                var type = getVarType();

                foreach (var tokenVar in tokensVars)
                {
                    var varNode = new DeclVarNode(tokenVar, type);

                    var symType = TypeBuilder.Build(type, tables);
                    var symVar = new SymVar(varNode, symType);
                    tables.AddSymbol(symVar);
                    varNode.NodeType = symType;

                    vars.Add(varNode);
                }

                tokensVars.Clear();
                if (!isArgs)
                    matchNext(TokenType.SEMICOLON);
                else
                {
                    tokenizer.Next();

                    if (tokenizer.Current.Type == TokenType.RBRACKET)
                        return vars;
                    else if (tokenizer.Current.Type == TokenType.SEMICOLON)
                        continue;
                    else
                        throw new ParserException(tokenizer.Current, ")");
                }
            }
        }

        private VarTypeNode getVarType()
        {
            VarTypeNode type = null;

            if (tokenizer.Current.Value == "array")
            {
                type = getArrayTypeNode();
            }
            else if (tokenizer.Current.Type == TokenType.POINTER)
            {
                var ptr = tokenizer.Current;
                var typeToken = matchNext(TokenType.ID);

                type = new PtrTypeNode(ptr, new VarTypeNode(typeToken));
            }
            else if (tokenizer.Current.Type == TokenType.ID)
            {
                type = new VarTypeNode(tokenizer.Current);
            }
            else
            {
                throw new ParserException(tokenizer.Current, TokenType.ID);
            }

            return type;
        }

        private VarTypeNode getArrayTypeNode()
        {
            if (tokenizer.Current.Value == "array")
            {
                var of = tokenizer.Next();
                ExprNode leftRange = null;
                ExprNode rightRange = null;

                if (tokenizer.Current.Value.Equals("["))
                {
                    tokenizer.Next();
                    leftRange = parseExpr();
                    require(TokenType.DOUBLE_DOT);
                    tokenizer.Next();
                    rightRange = parseExpr();
                    require(TokenType.RSQUARE_BRACKET);
                    of = matchNext("of");
                }
                else if (!tokenizer.Current.Value.Equals("of"))
                    throw new ParserException(tokenizer.Current);

                tokenizer.Next();

                var type = new ArrayTypeNode(of, leftRange, rightRange, getArrayTypeNode());
                type.NodeType = type.Left.NodeType;
                return type;
            }
            else if (tokenizer.Current.Type == TokenType.POINTER)
            {
                var ptr = tokenizer.Current;
                var typeToken = matchNext(TokenType.ID);

                return new PtrTypeNode(ptr, new VarTypeNode(typeToken));
            }
            else if (tokenizer.Current.Type == TokenType.ID)
            {
                var type = new VarTypeNode((tokenizer.Current));
                type.NodeType = TypeBuilder.Build(type, tables);
                return type;
            }
            else
                throw new ParserException(tokenizer.Current, TokenType.ID);
        }

        private List<Node> parseBegin(bool isUntil = false)
        {
            List<Node> statements = new List<Node>();

            while (true)
            {
                var id = tokenizer.Next();

                Node block = null;
                while ((block = getBlock(id)) != null)
                {
                    statements.Add(block);
                    id = tokenizer.Current;
                }

                if (id.Value.Equals("end") || (id.Value.Equals("until") && isUntil))
                {
                    return statements;
                }

                var afterId = tokenizer.Next();
                Token ptrToken = null;

                if (afterId.Type == TokenType.POINTER)
                {
                    ptrToken = tokenizer.Current;
                    afterId = tokenizer.Next();
                }
                
                if (afterId.Type == TokenType.ASSIGNMENT || afterId.Type == TokenType.ADDITION_ASSIGNMENT ||
                    afterId.Type == TokenType.SUBSTRACTION_ASSIGNMENT || afterId.Type == TokenType.MULTIPLICATION_ASSIGNMENT ||
                    afterId.Type == TokenType.DIVISION_ASSIGNMENT)
                {
                    tokenizer.Next();

                    if (ptrToken != null)
                    {
                        statements.Add(new VarNode(id, new VarNode(ptrToken, new AssignmentNode(afterId, parseExpr()))));
                    }
                    else
                    {
                        var type = tables.GetSymbol(id);
                        var var_node = new AssignVarNode(id, new AssignmentNode(afterId, parseExpr()));
                        var_node.NodeType = (type as SymVar);
                        statements.Add(var_node);
                    }
                }
                else if ((id.Type == TokenType.ID || id.Type == TokenType.KEY_WORD) && afterId.Type == TokenType.LBRACKET)
                {
                    var symbol = tables.GetSymbol(id);
                    if (!(symbol is SymTypeFunc) && !(symbol is SymTypeProc))
                        throw new NotAFunctionException(id);

                    var func = genFuncCallNode(parseFuncCall(), id, symbol as SymType);

                    statements.Add(func);
                }
                else if (isUntil && !id.Value.Equals("until"))
                {
                    throw new ParserException(id, "until");
                }
                else if (afterId.Type == TokenType.LSQUARE_BRACKET)
                {
                    tokenizer.Next();
                    var index = parseExpr();
                    require(TokenType.RSQUARE_BRACKET);
                    tokenizer.Next();

                    afterId = tokenizer.Current;
                    if (afterId.Type == TokenType.ASSIGNMENT || afterId.Type == TokenType.ADDITION_ASSIGNMENT ||
                        afterId.Type == TokenType.SUBSTRACTION_ASSIGNMENT || afterId.Type == TokenType.MULTIPLICATION_ASSIGNMENT ||
                        afterId.Type == TokenType.DIVISION_ASSIGNMENT)
                    {
                        tokenizer.Next();
                        var type = tables.GetSymbol(id);
                        var var_node = new AssignVarNode(id, new AssignmentNode(afterId, parseExpr()), index);
                        var_node.NodeType = (type as SymVar);
                        statements.Add(var_node);
                    }
                    else
                        throw new ParserException(id, "assignment");

                }
                else if (afterId.Type != TokenType.ASSIGNMENT && afterId.Type != TokenType.ADDITION_ASSIGNMENT &&
                        afterId.Type != TokenType.SUBSTRACTION_ASSIGNMENT && afterId.Type != TokenType.MULTIPLICATION_ASSIGNMENT &&
                        afterId.Type != TokenType.DIVISION_ASSIGNMENT)
                {
                    if (id.Type == TokenType.ID)
                        throw new ParserException(id, "assignment");
                    else
                        throw new ParserException(id);
                }

                require(TokenType.SEMICOLON);
            }
        }

        private Node getBlock(Token id)
        {
            if (id.Type == TokenType.KEY_WORD)
            {
                switch (id.Value)
                {
                    case "begin":
                        return parseBeginBlock();
                    case "if":
                        return parseIf();
                    case "while":
                        return parseWhile();
                    case "repeat":
                        return parseRepeatUntil();
                    case "for":
                        return parseFor();
                    case "end": case "until":
                        return null;
                }
            }

            return null;
        }

        private Node parseIf()
        {
            var nameTok = tokenizer.Current;
            tokenizer.Next();

            var cond = parseExpr();

            require("then");

            matchNext("begin");

            var begBlock = parseBeginBlock();

            Node elseBlock = null;
            if (tokenizer.Current.Value.Equals("else"))
            {
                var elseToken = tokenizer.Current;
                tokenizer.Next();

                Node block = null;
                if (tokenizer.Current.Value.Equals("if"))
                    block = parseIf();
                else if (tokenizer.Current.Value.Equals("begin"))
                    block = parseBeginBlock();
                else
                    throw new ParserException(tokenizer.Current);

                elseBlock = new BlockNode(elseToken, new List<Node>() { block });
            }

            return new IfNode(nameTok, cond, begBlock, elseBlock);
        }

        private WhileNode parseWhile()
        {
            var nameTok = tokenizer.Current;
            tokenizer.Next();

            var cond = parseExpr();

            require("do");
            matchNext("begin");

            var begBlock = parseBeginBlock();

            return new WhileNode(nameTok, new ConditionNode(nameTok, cond), begBlock);
        }

        private RepeatNode parseRepeatUntil()
        {
            var nameTok = tokenizer.Current;

            var begBlock = parseBeginBlock(false, true);

            var cond = parseExpr();

            require(TokenType.SEMICOLON);

            tokenizer.Next();

            return new RepeatNode(nameTok, new ConditionNode(nameTok, cond), begBlock);
        }

        private ForNode parseFor()
        {
            var nameTok = tokenizer.Current;

            var nameCounter = matchNext(TokenType.ID);
            var assign = matchNext(TokenType.ASSIGNMENT);
            tokenizer.Next();
            var startVal = new VarNode(nameCounter, new AssignmentNode(assign, parseExpr()));

            Node dir;
            if (tokenizer.Current.Value.Equals("to"))
                dir = new ForDirTo(tokenizer.Current);
            else if (tokenizer.Current.Value.Equals("downto"))
                dir = new ForDirDownto(tokenizer.Current);
            else
                throw new ParserException(tokenizer.Current, "to / downto");

            tokenizer.Next();
            var endVal = parseExpr();

            require("do");
            matchNext("begin");

            var begBlock = parseBeginBlock();

            return new ForNode(nameTok, startVal, endVal, dir, begBlock);
        }

        private List<Node> parseFuncCall()
        {
            List<Node> args = new List<Node>();
            while (tokenizer.Current.Type != TokenType.RBRACKET)
            {
                tokenizer.Next();

                if (tokenizer.Current.Type == TokenType.RBRACKET)
                    continue;

                args.Add(parseExpr());

                if (tokenizer.Current.Type == TokenType.COMMA)
                    continue;
                else if (tokenizer.Current.Type != TokenType.RBRACKET)
                    throw new ParserException(tokenizer.Current, ")");
            }

            return args;
        }

        private void setNodeType(ExprNode e, SymType type)
        {
            if (e is IdNode)
            {
                e.NodeType = tables.GetSymbol(e.Token);
            }

            e.NodeType = type;
        }

        private ExprNode parseExpr()
        {
            var e = parseExpr1();
            var t = tokenizer.Current;

            while (t.Type == TokenType.RELOP_EQ || t.Type == TokenType.RELOP_NE ||
                   t.Type == TokenType.RELOP_LT || t.Type == TokenType.RELOP_GT ||
                   t.Type == TokenType.RELOP_LE || t.Type == TokenType.RELOP_GE ||
                   t.Text.Equals("in") || t.Text.Equals("is"))
            {
                tokenizer.Next();
                var r = parseExpr1();
                var type = ExprTypeBuilder.BinOpBuild(t, e, r);
                setNodeType(e, type);
                setNodeType(r, type);
                e = new BinOpNode(t, e, r, type);
                t = tokenizer.Current;
            }

            return e;
        }

        private ExprNode parseExpr1()
        {
            var e = parseTerm();
            var t = tokenizer.Current;

            while (t.Type == TokenType.PLUS || t.Type == TokenType.MINUS ||
                   t.Text.Equals("or") || t.Text.Equals("xor"))
            {
                tokenizer.Next();
                var r = parseTerm();
                var type = ExprTypeBuilder.BinOpBuild(t, e, r);
                setNodeType(e, type);
                setNodeType(r, type);
                e = new BinOpNode(t, e, r, type);
                t = tokenizer.Current;
            }

            return e;
        }

        private ExprNode parseTerm()
        {
            var e = parseTerm0();
            var t = tokenizer.Current;

            while (t.Type == TokenType.ASTERIX || t.Type == TokenType.FORWARD_SLASH ||
                   t.Text.Equals("div") || t.Text.Equals("mod") || t.Text.Equals("and") ||
                   t.Text.Equals("shl") || t.Text.Equals("shr") || t.Text.Equals("as") ||
                   t.Type == TokenType.BITWISE_SL || t.Type == TokenType.BITWISE_SR)
            {
                tokenizer.Next();
                var r = parseTerm0();
                var type = ExprTypeBuilder.BinOpBuild(t, e, r);
                setNodeType(e, type);
                setNodeType(r, type);
                e =  new BinOpNode(t, e, r, type);
                t = tokenizer.Current;
            }

            return e;
        }

        private ExprNode parseTerm0()
        {
            var t = tokenizer.Current;
            List<Token> binOps = new List<Token>();

            while (t.Type == TokenType.PLUS || t.Type == TokenType.MINUS ||
                   t.Type == TokenType.ADDRESS || t.Text.Equals("not"))
            {
                binOps.Add(t);
                t = tokenizer.Next();
            }

            if (binOps.Count != 0)
                return genUnOp(0, binOps, parseFactor());
            else
                return parseFactor();
        }

        private ExprNode genUnOp(int i, List<Token> binOps, ExprNode fac)
        {
            var type = ExprTypeBuilder.UnOpBuild(binOps[i], fac);

            if (i == binOps.Count - 1)
            {
                return new UnOpNode(binOps[i], fac, type);
            }

            return new UnOpNode(binOps[i], genUnOp(i + 1, binOps, fac), type);
        }

        private ExprNode parseFactor()
        {
            var t = tokenizer.Current;

            switch (t.Type)
            {
                case TokenType.ID:
                    return parseId();
                case TokenType.INTEGER:
                    tokenizer.Next();
                    return new IntConstNode(t, new SymTypeInteger());
                case TokenType.REAL:
                    tokenizer.Next();
                    return new RealConstNode(t, new SymTypeReal());
                case TokenType.STRING:
                    tokenizer.Next();

                    if (t.Value.Length == 1)
                        return new CharConstNode(t, new SymTypeChar());

                    return new StringConstNode(t);
                case TokenType.LBRACKET:
                    tokenizer.Next();
                    var e = parseExpr();

                    require(TokenType.RBRACKET);

                    tokenizer.Next();
                    return e;
            }

            throw new ParserException(t, "expression");
        }

        private ExprNode parseId(bool isNext = false, SymType type = null)
        {
            var t = tokenizer.Current;

            if (t.Type != TokenType.LBRACKET && t.Type != TokenType.LSQUARE_BRACKET &&
                t.Type != TokenType.DOT && t.Type != TokenType.ID && !isNext)
            {
                return null;
            }

            tokenizer.Next();

            if (tokenizer.Current.Type == TokenType.LBRACKET)
            {
                type = getType(type, t);

                if (!(type is SymTypeFunc) && !(type is SymTypeProc))
                    throw new NotAFunctionException(t);

                return genFuncCallNode(parseFuncCall(), t, type);
            }
            else if (tokenizer.Current.Type == TokenType.LSQUARE_BRACKET)
            {
                type = getType(type, t);

                if (!(type is SymTypeArray))
                    throw new NotAnArrayException(t);

                tokenizer.Next();
                var index = parseExpr();
                require(TokenType.RSQUARE_BRACKET);
                var sqrBr = new SquareBracketsNode(tokenizer.Current, index, parseId(true, (type as SymTypeArray).ElemType));
                var idNode = new IdNode(t, sqrBr);
                idNode.NodeType = type;

                return idNode;
            }
            else if (tokenizer.Current.Type == TokenType.DOT)
            {
                var dotTok = tokenizer.Current;
                matchNext(TokenType.ID);
                var rec = new IdNode(t, new DotNode(dotTok, parseId(true)));

                return rec;
            }
            else if (t.Type == TokenType.ID)
            {
                var id = new IdNode(t, parseId());

                var id_type = tables.GetSymbol(t);

                if (!(id_type is SymVar))
                    throw new NotFounIdException(t);

                id.NodeType = tables.GetSymbol(t);

                return id;
            }
            else
            {
                return null;
            }
        }

        private SymType getType(SymType type, Token token)
        {
            Symbol symbol = type;

            if (type == null)
            {
                symbol = tables.GetSymbol(token);

                if (symbol is SymVar)
                {
                    symbol = (symbol as SymVar).Type;
                }
            }

            if (symbol is SymTypeAlias)
                symbol = (symbol as SymTypeAlias).Type;

            return symbol as SymType;
        }

        private FuncCallNode genFuncCallNode(List<Node> args, Token t, SymType type = null)
        {
            if (type is SymTypeAlias)
                type = (type as SymTypeAlias).Type;

            if (!(type is SymTypeFunc) && !(type is SymTypeProc))
                throw new NotAFunctionException(t);

            var list_args = TypeBuilder.GetArgsType(args, tables);

            if (list_args.Count != (type as SymTypeProc).Args.Count)
                throw new ArgsException(t);
            else
            {
                for(int i = 0; i < list_args.Count; i++)
                {
                    Node arg = args[i];

                    if (args[i] is SquareBracketsNode)
                        arg = args[i].Left;

                    var input_type = TypeBuilder.GetTrueType(arg, arg.NodeType);
                    var def_type = TypeBuilder.GetTrueType((type as SymTypeProc).Args[i]);

                    TypeBuilder.GetType(def_type, input_type);
                }
            }

            if (type is SymTypeFunc)
            {
                var funcType = type as SymTypeFunc;
                var func = new FuncCallNode(t, args, parseId(true, funcType.ReturnType));
                func.NodeType = type;

                return func;
            }
            else
            {
                return new FuncCallNode(t, args, parseId(true));
            }
        }
    }
}
