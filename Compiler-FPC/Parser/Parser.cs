using System;
using System.Collections.Generic;

namespace Compiler_FPC.Parser
{
    class Parser
    {
        private readonly Tokenizer tokenizer;
        private SyntaxTree tree;

        public Parser(string fileName)
        {
            tokenizer = new Tokenizer(fileName); 
        }

        public Parser(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public string Tree()
        {
            if (tree == null)
            {
                tokenizer.Next();
                try
                {
                    tree = new SyntaxTree(parseProgram());
                }
                catch (ParserException ex)
                {
                    return ex.Message;
                }
            }

            return tree.TreeString;
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
                    return new ProcedureNode(matchNext(TokenType.ID), parseArgs(tokenizer.Current), parseBlocks());
                case "function":
                    var funcName = matchNext(TokenType.ID);
                    return new FunctionNode(funcName, parseArgs(funcName, true), parseReturnValue(funcName), parseBlocks());
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

                consts.Add(new VarNode(nameToken, parseExpr()));

                require(TokenType.SEMICOLON);
            }
        }

        private List<Node> parseTypeDecl()
        {
            List<Node> types = new List<Node>();

            while(true)
            {
                var nameToken = tokenizer.Next();

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
                    types.Add(new RecordNode(nameToken, parseVar()));
                    
                    require("end");
                }
                else
                {
                    types.Add(new TypeNode(nameToken, getVarType()));
                }

                matchNext(TokenType.SEMICOLON);
            }
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
                    vars.Add(new VarNode(tokenVar, type));
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
                matchNext(TokenType.LSQUARE_BRACKET);
                var leftRange = matchNext(TokenType.INTEGER);
                matchNext(TokenType.DOUBLE_DOT);
                var rightRange = matchNext(TokenType.INTEGER);
                matchNext(TokenType.RSQUARE_BRACKET);
                var of = matchNext("of");

                tokenizer.Next();

                return new ArrayTypeNode(of, leftRange.Value, rightRange.Value, getArrayTypeNode());
            }
            else if (tokenizer.Current.Type == TokenType.POINTER)
            {
                var ptr = tokenizer.Current;
                var typeToken = matchNext(TokenType.ID);

                return new PtrTypeNode(ptr, new VarTypeNode(typeToken));
            }
            else if (tokenizer.Current.Type == TokenType.ID)
            {
                return new VarTypeNode((tokenizer.Current));
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
                        statements.Add(new VarNode(id, new AssignmentNode(afterId, parseExpr())));
                    }
                }
                else if (id.Type == TokenType.ID && afterId.Type == TokenType.LBRACKET)
                {
                    statements.Add(new FuncCallNode(id, parseFuncCall()));
                }
                else if (isUntil && !id.Value.Equals("until"))
                {
                    throw new ParserException(id, "until");
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
                    default:
                        throw new ParserException(id);
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

            tokenizer.Next();

            return args;
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
                e = new BinOpNode(t, e, r);
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
                e = new BinOpNode(t, e, r);
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
                e =  new BinOpNode(t, e, r);
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
            if (i == binOps.Count - 1)
                return new UnOpNode(binOps[i], fac);

            return new UnOpNode(binOps[i], genUnOp(i + 1, binOps, fac));
        }

        private ExprNode parseFactor()
        {
            var t = tokenizer.Current;

            switch (t.Type)
            {
                case TokenType.ID:
                    tokenizer.Next();

                    List<List<Node>> funcCall = new List<List<Node>>();
                    while (tokenizer.Current.Type == TokenType.LBRACKET)
                        funcCall.Add(parseFuncCall());

                    SquareBracketsNode sqrBr = null;
                    if (tokenizer.Current.Type == TokenType.LSQUARE_BRACKET)
                    {
                        var indexes = parseSquareBrackets();
                        sqrBr = new SquareBracketsNode(tokenizer.Current, indexes);
                        tokenizer.Next();

                        if (funcCall.Count != 0) funcCall[funcCall.Count - 1].Add(sqrBr);
                    }

                    DotNode rec = null;
                    if (tokenizer.Current.Type == TokenType.DOT)
                    {
                        var dotTok = tokenizer.Current;
                        matchNext(TokenType.ID);
                        rec = new DotNode(dotTok, parseFactor());

                        if (funcCall.Count != 0) funcCall[funcCall.Count - 1].Add(rec);
                    }

                    if (funcCall.Count != 0)
                        return genFuncCallNode(0, funcCall, t);
                    else
                        return new IdNode(t, sqrBr, rec);
                case TokenType.INTEGER:
                    tokenizer.Next();
                    return new IntConstNode(t);
                case TokenType.REAL:
                    tokenizer.Next();
                    return new RealConstNode(t);
                case TokenType.STRING:
                    tokenizer.Next();

                    if (t.Value.Length == 1)
                        return new CharConstNode(t);

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

        private List<Node> parseSquareBrackets()
        {
            List<Node> indexes = new List<Node>();

            while (tokenizer.Current.Type != TokenType.RSQUARE_BRACKET && tokenizer.Next() != null)
            {
                if (tokenizer.Current.Type == TokenType.COMMA || tokenizer.Current.Type == TokenType.RSQUARE_BRACKET)
                    continue;

                indexes.Add(parseExpr());
            }

            if (tokenizer.Current == null)
                throw new ParserException(tokenizer.Current, ")");

            return indexes;
        }

        private FuncCallNode genFuncCallNode(int i, List<List<Node>> args, Token t)
        {
            if (i == args.Count)
                return null;

            return new FuncCallNode(t, args[i], genFuncCallNode(i + 1, args, t));
        }
    }
}
