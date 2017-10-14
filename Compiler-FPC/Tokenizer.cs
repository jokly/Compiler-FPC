using System;
using System.IO;

namespace Compiler_FPC
{
    class Tokenizer
    {
        public Token Current { get; private set; }

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

            while ((currentData = input.Read()) != -1)
            {
                var newState = Config.StateTable[currentState, currentData];
                var ch = (char) currentData;

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

            if (currentState != -1 && currentState != 0)
                return GetToken();

            return null;
        }

        private Token GetToken()
        {
            if (Config.StatesToToken.TryGetValue(currentState, out var type))
            {
                var lowercaseText = currentText.ToLower();
                var value = currentText;

                if (type == TokenType.Id && Config.KeyWords.Contains(lowercaseText))
                {
                    type = TokenType.KeyWord;
                    value = lowercaseText;
                }

                Current = new Token(currentRow, currentCol - currentText.Length,
                                    type, value, currentText);

                currentText = "";
                currentState = 0;

                return Current;
            }

            input.Close();
            throw new Exception(currentText);
        }
    }
}
