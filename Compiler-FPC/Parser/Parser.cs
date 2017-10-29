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
                case "var":
                    return new DeclarationNode(tokenizer.Current, parseVar());
                case "begin":
                    return new BlockNode(tokenizer.Current, parseBegin());
            }

            return null;
        }

        private List<Node> parseVar()
        {
            List<Node> vars = new List<Node>();

            while (true)
            {
                List<Token> tokensVars = new List<Token>();

                while (true)
                {
                    var next = tokenizer.Next();

                    if (vars.Count == 0 && next.Type == TokenType.KEY_WORD)
                    {
                        throw new Exception("Identifier expected");
                    }
                    else if (next.Type == TokenType.KEY_WORD)
                    {
                        return vars;
                    }
                    else if (next == null)
                    {
                        throw new Exception("Identifier expected");
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

                if (tokenizer.Current.Value == "array")
                {
                    matchNext(TokenType.LSQUARE_BRACKET);
                    var leftRange = matchNext(TokenType.INTEGER);
                    matchNext(TokenType.DOUBLE_DOT);
                    var rightRange = matchNext(TokenType.INTEGER);
                    matchNext(TokenType.RSQUARE_BRACKER);
                    var of = matchNext("of");
                    matchNext(TokenType.ID);

                    foreach (var tokenVar in tokensVars)
                    {
                        vars.Add(new VarNode(tokenVar, new ArrayTypeNode(of, leftRange.Value, rightRange.Value, new VarTypeNode(tokenizer.Current))));
                    }
                }
                else
                {
                    foreach (var tokenVar in tokensVars)
                    {
                        vars.Add(new VarNode(tokenVar, new VarTypeNode(tokenizer.Current)));
                    }
                }
                
                matchNext(TokenType.SEMICOLON);
            }
        }

        private List<Node> parseBegin()
        {
            List<Node> statements = new List<Node>();

            while (true)
            {
                var id = tokenizer.Next();
                var afterId = tokenizer.Next();

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
                    statements.Add(new VarNode(id, new AssignmentNode(afterId, parseExpr())));
                }
                else if (afterId.Type == TokenType.LBRACKET)
                {
                    statements.Add(parseFunc(id));
                }

                if (tokenizer.Current.Type != TokenType.SEMICOLON)
                    throw new Exception("Expected ';'");
            }
        }

        private FuncCallNode parseFunc(Token id)
        {
            Token next = null;
            List<Node> args = new List<Node>();
            while (tokenizer.Current.Type != TokenType.RBRACKET && (next = tokenizer.Next()) != null)
            {
                if (next.Type == TokenType.COMMA)
                    continue;

                args.Add(parseExpr());
            }

            if (next == null)
            {
                throw new Exception("Expect ')'");
            }

            tokenizer.Next();

            return new FuncCallNode(id, args);
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
                    if (tokenizer.Current.Type == TokenType.LBRACKET)
                        return parseFunc(t);
                    return new IdNode(t);
                case TokenType.INTEGER:
                    tokenizer.Next();
                    return new IntConstNode(t);
                case TokenType.REAL:
                    tokenizer.Next();
                    return new RealConstNode(t);
                case TokenType.LBRACKET:
                    tokenizer.Next();
                    var e = parseExpr();

                    if (tokenizer.Current.Type != TokenType.RBRACKET)
                        throw new Exception();

                    tokenizer.Next();
                    return e;
            }

            throw new Exception();
        }
    }
}
