using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string getTree()
        {
            if (tree == null)
            {
                tokenizer.Next();
                tree = new SyntaxTree(parseExpr());
            }

            return tree.TreeString;
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
