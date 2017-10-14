
namespace Compiler_FPC
{
    class Token
    {
        public int Row { get; private set; }
        public int Col { get; private set; }
        public TokenType Type { get; private set; }
        public string Value { get; private set; } 
        public string Text { get; private set; }

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
