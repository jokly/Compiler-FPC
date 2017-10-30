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

                List<Node> blocks = new List<Node>();
                Node block;
                tokenizer.Next();
                while ((block = parseBlock()) != null)
                {
                    blocks.Add(block);
                }

                return new ProgramNode(programName, blocks);
            }
            else
            {
                return parseExpr();
            }
        }

        private Node parseBlock()
        {
            switch(tokenizer.Current.Value)
            {
                case "const":
                    return new ConstDeclNode(tokenizer.Current, parseConstDecl());
                case "var":
                    return new DeclarationNode(tokenizer.Current, parseVar());
                case "begin":
                    return new BlockNode(tokenizer.Current, parseBegin());
            }

            return null;
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

        private List<Node> parseVar()
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

                foreach (var tokenVar in tokensVars)
                {
                    vars.Add(new VarNode(tokenVar, type));
                }

                tokensVars.Clear();
                matchNext(TokenType.SEMICOLON);
            }
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
                    statements.Add(new FuncCallNode(id, parseFunc()));
                }
                else
                {
                    throw new Exception("Expect expression");
                }
              
                if (tokenizer.Current.Type != TokenType.SEMICOLON)
                    throw new Exception("Expected ';'");
            }
        }

        private List<Node> parseFunc()
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
            var e = parseTerm();
            var t = tokenizer.Current;

            while (t.Type == TokenType.PLUS || t.Type == TokenType.MINUS)
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
            var e = parseFactor();
            var t = tokenizer.Current;

            while (t.Type == TokenType.ASTERIX || t.Type == TokenType.FORWARD_SLASH)
            {
                tokenizer.Next();
                var r = parseFactor();
                e =  new BinOpNode(t, e, r);
                t = tokenizer.Current;
            }

            return e;
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
                        funcCall.Add(parseFunc());

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
                case TokenType.ADDRESS:
                    var id = matchNext(TokenType.ID);
                    tokenizer.Next();
                    return new PtrConstNode(t, new IdNode(id));
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
