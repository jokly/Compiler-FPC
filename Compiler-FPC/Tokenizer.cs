using System;
using System.Collections.Generic;
using System.IO;

namespace Compiler_FPC
{
    class Tokenizer
    {
        public Token Current { get; private set; }

        private readonly int[,] states =
        {
            {
                -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,1,1,1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
            },
            {
                -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,1,1,1,1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
            },
        };

        private readonly Dictionary<int, TokenType> statesToToken = new Dictionary<int, TokenType>()
        {
            {1, TokenType.Integer},
        };

        private StreamReader input;
        private int currentState;
        private int currentRow, currentCol;
        private string currentText;

        public Tokenizer(string fileName)
        {
            Current = null;
            currentState = 0;
            currentRow = currentCol = 1;
            currentText = "";

            input = new StreamReader(fileName);
        }

        public Token Next()
        {
            int currentData;
            char ch;

            while ((currentData = input.Read()) != -1)
            {
                var newState = states[currentState, currentData];
                ch = (char) currentData;

                if (newState != 0 && newState != -1)
                    currentText += ch;

                currentCol++;
                if (ch == '\n')
                {
                    currentRow++;
                    currentCol = 1;
                }

                if (newState == -1)
                {
                    return GetToken();
                }

                currentState = newState;
            }

            return null;
        }

        private Token GetToken()
        {
            if (statesToToken.TryGetValue(currentState, out var type))
            {
                Current = new Token(currentRow, currentCol - currentText.Length,
                                    type, currentText, currentText);

                currentText = "";
                currentState = 0;

                return Current;
            }

            input.Close();
            throw new Exception(currentText);
        }
    }
}
