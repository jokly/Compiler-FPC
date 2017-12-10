using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC.Parser
{
    class ExprTypeBuilder
    {
        public static SymType UnOpBuild(Token op, ExprNode fac)
        {
            SymType type = GetTrueType(fac);

            if (op.Type == TokenType.PLUS || op.Type == TokenType.MINUS)
            {
                if (type is SymTypeInteger || type is SymTypeReal)
                    return type;
                else
                    throw new IncompatibleTypes(op);
            }
            else if (op.Type == TokenType.ADDRESS)
            {
                if (fac.TypeNode is SymVar)
                    return new SymTypePointer(type);
                else
                    throw new IncompatibleTypes(op);
            }
            else if (op.Value.Equals("not"))
            {
                if (type is SymTypeInteger)
                    return new SymTypeInteger();
                else
                    throw new IncompatibleTypes(op);
            }
            else
            {
                throw new IncompatibleTypes(op);
            }
        }

        private static SymType GetTrueType(Node node)
        {
            SymType type = null;

            if (node.TypeNode is SymVar)
                type = (node.TypeNode as SymVar).Type;
            else
                type = node.TypeNode as SymType;

            if (type is SymTypeAlias)
                type = (type as SymTypeAlias).Type;

            return type;
        }
    }
}
