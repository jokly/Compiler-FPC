
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
            this.Row = row;
            this.Col = col;
            this.Type = tokenType;
            this.Value = value;
            this.Text = text;
        }
    }
}
