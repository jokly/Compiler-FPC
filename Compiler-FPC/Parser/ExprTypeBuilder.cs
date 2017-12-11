using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC.Parser
{
    class ExprTypeBuilder
    {
        public static void AssignmentBuild(Token assign,  SymType id, SymType expr)
        {

        }

        public static SymType UnOpBuild(Token op, ExprNode fac)
        {
            SymType type = TypeBuilder.GetTrueType(fac, fac.TypeNode);

            if (op.Type == TokenType.PLUS || op.Type == TokenType.MINUS)
            {
                if (type is SymTypeInteger || type is SymTypeReal)
                    return type;
                else
                    throw new IncompatibleTypesException(op);
            }
            else if (op.Type == TokenType.ADDRESS)
            {
                if (fac.TypeNode is SymVar)
                    return new SymTypePointer(type);
                else
                    throw new IncompatibleTypesException(op);
            }
            else if (op.Value.Equals("not"))
            {
                if (type is SymTypeInteger)
                    return new SymTypeInteger();
                else
                    throw new IncompatibleTypesException(op);
            }
            else
            {
                throw new IncompatibleTypesException(op);
            }
        }

        public static SymType BinOpBuild(Token op, Node left, Node right)
        {
            if (op.Type == TokenType.ASTERIX)
            {
                return GetArithmeticType(left, right);
            }
            else if (op.Type == TokenType.FORWARD_SLASH)
            {
                if (GetArithmeticType(left, right) == null)
                    throw new IncompatibleTypesException(op);

                return new SymTypeReal();
            }
            else if (op.Type == TokenType.MINUS || op.Type == TokenType.PLUS)
            {
                var t = GetArithmeticType(left, right);

                if (t == null)
                    throw new IncompatibleTypesException(op);

                return t;
            }
            else if (op.Type == TokenType.RELOP_EQ || op.Type == TokenType.RELOP_NE ||
                     op.Type == TokenType.RELOP_LT || op.Type == TokenType.RELOP_GT ||
                     op.Type == TokenType.RELOP_LE || op.Type == TokenType.RELOP_GE)
            {
                var t = GetEqualType(left, right);

                if (t != null)
                    return t;

                t = GetArithmeticType(left, right);

                if (t != null)
                    return t;

                t = GetArithmeticType(right, left);

                if (t != null)
                    return t;

                throw new IncompatibleTypesException(op);
            }
            else if (op.Text.Equals("or") || op.Text.Equals("and") ||
                     op.Text.Equals("div") || op.Text.Equals("mod") ||
                     op.Text.Equals("shl") || op.Text.Equals("shr") ||
                     op.Type == TokenType.BITWISE_SL || op.Type == TokenType.BITWISE_SR)
            {
                var lt = TypeBuilder.GetTrueType(left, left.TypeNode);
                var rt = TypeBuilder.GetTrueType(right, right.TypeNode);

                if (lt is SymTypeInteger && rt is SymTypeInteger)
                    return lt;

                throw new IncompatibleTypesException(op);
            }
            else
            {
                throw new IncompatibleTypesException(op);
            }
        }

        private static SymType GetEqualType(Node left, Node right)
        {
            var lt = TypeBuilder.GetTrueType(left, right.TypeNode).GetType();
            var rt = TypeBuilder.GetTrueType(right, right.TypeNode).GetType();

            if (lt.Equals(rt))
                return TypeBuilder.GetTrueType(left, right.TypeNode);
            else
                return null;
        }

        private static SymType GetArithmeticType(Node left, Node right)
        {
            var lt = GetArithmeticType(left);
            var rt = GetArithmeticType(right);

            if (lt == null || rt == null)
                return null;

            if (lt is SymTypeReal || rt is SymTypeReal)
                return new SymTypeReal();
            else
                return new SymTypeInteger();
        }

        private static SymType GetArithmeticType(Node node)
        {
            var type = TypeBuilder.GetTrueType(node, node.TypeNode);

            if (type is SymTypeInteger || type is SymTypeReal)
                return type;
            else
                return null;
        }
    }
}
