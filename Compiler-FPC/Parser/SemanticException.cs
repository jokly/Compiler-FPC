using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC.Parser
{
    class SemanticException : CompilerException
    {
        protected Token token;

        public SemanticException(Token token)
        {
            this.token = token;
        }

        public override string Message => $"({token.Row}, {token.Col}): Semantic exception";
    }

    class DuplicateDeclarationException : SemanticException
    {
        protected Token FirstDecl;

        public DuplicateDeclarationException(Token firstDecl, Token duplDecl) : base(duplDecl)
        {
            FirstDecl = firstDecl;
        }

        public override string Message => $"({token.Row}, {token.Col}): Duplicate identifier '{token.Value}';" +
                                          $" First found at ({FirstDecl.Row}, {FirstDecl.Col})";
    }

    class NotFounIdException : SemanticException
    {
        public NotFounIdException(Token token) : base(token) { }

        public override string Message => $"({token.Row}, {token.Col}): Identifier not found '{token.Value}'";
    }

    class UnknowTypeException : SemanticException
    {
        public UnknowTypeException(Token token) : base(token) { }

        public override string Message => $"({token.Row}, {token.Col}): Unknown type '{token.Value}'";
    }

    class ShouldBeIntegerException : SemanticException
    {
        public ShouldBeIntegerException(Token token) : base(token) { }

        public override string Message => $"({token.Row}, {token.Col}): Not integer";
    }

    class NotAFunction : SemanticException
    {
        public NotAFunction(Token token) : base(token) { }

        public override string Message => $"({token.Row}, {token.Col}): Not a function '{token.Value}'";
    }

    class NotAnArray : SemanticException
    {
        public NotAnArray(Token token) : base(token) { }

        public override string Message => $"({token.Row}, {token.Col}): Not an array '{token.Value}'"; 
    }
}
