using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC
{
    class TokenizerException : Exception
    {
        public int Row { get; }
        public int Col { get; }
        public string Text { get; }

        public TokenizerException(int row, int col, string text)
        {
            this.Row = row;
            this.Col = col;
            this.Text = text;
        }

        public override string Message => $"({Row}, {Col}) | {Text}";
    }
}
