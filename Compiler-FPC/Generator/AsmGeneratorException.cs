using System;

namespace Compiler_FPC.Generator
{
    class AsmGeneratorException : Exception
    {
        public Token Token { get; protected set; }
    }

    class AsmGeneratorInvalidType : AsmGeneratorException
    {
        public AsmGeneratorInvalidType(Token token)
        {
            Token = token;
        }

        public override string Message => $"({Token.Row}, {Token.Col}): Invalid type";
    }
}
