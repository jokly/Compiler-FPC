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
                tree = new SyntaxTree(parseProgram());
            }

            return tree.TreeString;
        }

        private Token matchNext(TokenType expectedTkType)
        {
            var next = tokenizer.Next();

            if (next != null && expectedTkType != tokenizer.Current.Type)
            {
                throw new Exception("Invalid token");
            }

            return next == null ? null : tokenizer.Current;
        }

        private Token matchNext(string expectedTkValue)
        {
            var next = tokenizer.Next();

            if (next != null && !expectedTkValue.Equals(tokenizer.Current.Value))
            {
                throw new Exception("Invalid token");
            }

            return next == null ? null : tokenizer.Current;
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

                if (blocks[blocks.Count - 1] is BlockNode)
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
                    var blockName = tokenizer.Current;
                    var begNode = parseBegin();

                    if ((isMain && tokenizer.Current.Type != TokenType.DOT) ||
                        (!isMain && tokenizer.Current.Type != TokenType.SEMICOLON))
                        throw new Exception("Invalid token");

                    tokenizer.Next();
                    return new BlockNode(blockName, begNode);
                case "procedure":
                    return new ProcedureNode(matchNext(TokenType.ID), parseArgs(tokenizer.Current), parseBlocks());
                case "function":
                    var nameFunc = matchNext(TokenType.ID);
                    return new FunctionNode(nameFunc, parseArgs(nameFunc, true),
                        parseReturnValue(nameFunc), parseBlocks());
            }

            return null;
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

                if (nameToken.Type == TokenType.KEY_WORD && consts.Count == 0 || nameToken == null)
                {
                    throw new Exception("Identifier expected");
                }
                else if (nameToken.Type == TokenType.KEY_WORD)
                {
                    return consts;
                }
                else if (nameToken.Type != TokenType.ID)
                {
                    throw new Exception("Identifier expected");
                }

                matchNext(TokenType.RELOP_EQ);
                tokenizer.Next();

                consts.Add(new VarNode(nameToken, parseExpr()));

                if (tokenizer.Current.Type != TokenType.SEMICOLON)
                    throw new Exception("Expected ';'");
            }
        }

        private List<Node> parseTypeDecl()
        {
            List<Node> types = new List<Node>();

            while(true)
            {
                var nameToken = tokenizer.Next();

                if (nameToken.Type == TokenType.KEY_WORD && types.Count == 0 || nameToken == null)
                {
                    throw new Exception("Identifier expected");
                }
                else if (nameToken.Type == TokenType.KEY_WORD)
                {
                    return types;
                }
                else if (nameToken.Type != TokenType.ID)
                {
                    throw new Exception("Identifier expected");
                }

                matchNext(TokenType.RELOP_EQ);
                tokenizer.Next();

                if (tokenizer.Current.Value.Equals("record"))
                {
                    types.Add(new RecordNode(nameToken, parseVar()));

                    if (!tokenizer.Current.Value.Equals("end"))
                        throw new Exception("Expected 'end'");
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

                    if (vars.Count == 0 && next.Type == TokenType.KEY_WORD || next == null)
                    {
                        throw new Exception("Identifier expected");
                    }
                    else if (next.Type == TokenType.KEY_WORD)
                    {
                        return vars;
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
                        throw new Exception("Invalid token");
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
                    else if (tokenizer.Current.Type != TokenType.SEMICOLON)
                        throw new Exception("Excepted ';'");
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
                throw new Exception("Identifier expected");
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
            else
            {
                return new VarTypeNode((tokenizer.Current));
            }
        }

        private List<Node> parseBegin()
        {
            List<Node> statements = new List<Node>();

            while (true)
            {
                var id = tokenizer.Next();
                var afterId = tokenizer.Next();
                Token ptrToken = null;

                if (afterId.Type == TokenType.POINTER)
                {
                    ptrToken = tokenizer.Current;
                    afterId = tokenizer.Next();
                }

                if (id == null)
                {
                    throw new Exception("Expect 'end'");
                }
                else if (id.Value.Equals("end"))
                {
                    return statements;
                }
                else if (afterId.Type == TokenType.ASSIGNMENT || afterId.Type == TokenType.ADDITION_ASSIGNMENT ||
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
                else if (afterId.Type == TokenType.LBRACKET)
                {
                    statements.Add(new FuncCallNode(id, parseFuncCall()));
                }
                else
                {
                    throw new Exception("Expect expression");
                }
              
                if (tokenizer.Current.Type != TokenType.SEMICOLON)
                    throw new Exception("Expected ';'");
            }
        }

        private List<Node> parseFuncCall()
        {
            Token next = null;
            List<Node> args = new List<Node>();
            while (tokenizer.Current.Type != TokenType.RBRACKET && (next = tokenizer.Next()) != null)
            {
                if (next.Type == TokenType.COMMA || next.Type == TokenType.RBRACKET)
                    continue;

                args.Add(parseExpr());
            }

            if (next == null)
                throw new Exception("Expect ')'");

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

                    if (funcCall.Count != 0)
                        return genFuncCallNode(0, funcCall, t);
                    else
                        return new IdNode(t, sqrBr);
                case TokenType.INTEGER:
                    tokenizer.Next();
                    return new IntConstNode(t);
                case TokenType.REAL:
                    tokenizer.Next();
                    return new RealConstNode(t);
                case TokenType.STRING:
                    tokenizer.Next();
                    return new StringConstNode(t);
                case TokenType.LBRACKET:
                    tokenizer.Next();
                    var e = parseExpr();

                    if (tokenizer.Current.Type != TokenType.RBRACKET)
                        throw new Exception("Expected ')'");

                    tokenizer.Next();
                    return e;
            }

            throw new Exception("Invalid token");
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
                throw new Exception("Expect ')'");

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
