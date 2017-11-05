using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC.Parser
{
    class ParserException : Exception
    {
        protected Token token;
        protected string expectedTk;

        protected Dictionary<TokenType, string> TokenTypeToStr = new Dictionary<TokenType, string>()
        {
            { TokenType.ID, "Identifier" },
            { TokenType.SEMICOLON, ";" },
            { TokenType.COLON, ":" },
            { TokenType.LBRACKET, "(" },
            { TokenType.RBRACKET, ")" },
            { TokenType.RELOP_EQ, "=" },
            { TokenType.LSQUARE_BRACKET, "[" },
            { TokenType.RSQUARE_BRACKET, "]" },
            { TokenType.DOUBLE_DOT, ".." },
            { TokenType.ASSIGNMENT, ":=" },
        };

        public ParserException(Token token, string expectedTk = "")
        {
            this.token = token;
            this.expectedTk = expectedTk;
        }

        public ParserException(Token token, TokenType expectedTk)
        {
            this.token = token;
            this.expectedTk = TokenTypeToStr.ContainsKey(expectedTk) ? TokenTypeToStr[expectedTk] : expectedTk.ToString();
        }

        public override string Message => $"({token.Row}, {token.Col}): Unexpected token '{token.Text}';" +
                                          (expectedTk.Equals("") ? "" : $" Expected: '{expectedTk}'");
    }

    class ExpectedAfterException : ParserException
    {
        public ExpectedAfterException(Token token, string expectedTk = "") : base(token, expectedTk) { }
        public ExpectedAfterException(Token token, TokenType expectedTk) : base(token, expectedTk) { }

        public override string Message => $"({token.Row}, {token.Col}): Expected after '{token.Text}' token '{expectedTk}'";
    }
}
