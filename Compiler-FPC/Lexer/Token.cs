
namespace Compiler_FPC
{
    class Token
    {
        public int Row { get; }
        public int Col { get; }
        public TokenType Type { get; }
        public string Value { get; } 
        public string Text { get; }

        public Token(int row, int col, TokenType tokenType, string value, string text)
        {
            Row = row;
            Col = col;
            Type = tokenType;
            Value = value;
            Text = text;
        }
    }
}
