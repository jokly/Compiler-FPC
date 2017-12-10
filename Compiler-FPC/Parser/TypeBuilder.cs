﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC.Parser
{
    class TypeBuilder
    {
        public static SymType Build(Node node, SymbolTableTree tables)
        {
            if (node is IntConstNode)
            {
                return new SymTypeInteger();
            }
            else if (node is RealConstNode)
            {
                return new SymTypeReal();
            }
            else if (node is CharConstNode)
            {
                return new SymTypeChar();
            }
            else if (node is IdNode)
            {
                return (tables.GetSymbol(node.Token) as SymVar).Type;
            }
            else if (node is ArrayTypeNode)
            {
                var start = node.Childrens[0];
                var end = node.Childrens[1];

                // TODO
                //if (!(Build(start, tables) is SymTypeInteger))
                //    throw new ShouldBeIntegerException(start.Token);

                //if (!(Build(end, tables) is SymTypeInteger))
                //    throw new ShouldBeIntegerException(end.Token);

                return new SymTypeArray(Build(node.Left, tables), start, end);
            }
            else if (node is RecordNode)
            {
                var fields = new Dictionary<string, SymType>();

                foreach(var rec in node.Childrens)
                {
                    var name = rec.Token.Value;
                    var type = Build(rec.Left, tables);

                    fields.Add(name, type);
                }

                return new SymTypeRecord(node, fields);
            }
            else if (node is PtrTypeNode)
            {
                return new SymTypePointer(Build(node.Left, tables));
            }
            else if (node is TypeNode)
            {
                return new SymTypeAlias(node, Build(node.Left, tables));
            }
            else if (node is ProcTypeNode)
            {
                var args = GetArgsType(node.Left.Childrens, tables);
                return new SymTypeProc(node, args);
            }
            else if (node is FuncTypeNode)
            {
                var args = GetArgsType(node.Left.Childrens, tables);
                var returnedType = Build(node.Right, tables);

                return new SymTypeFunc(node, args, returnedType);
            }
            else if (node is ReturnValueNode)
            {
                return GetType(node.Left.Token, tables);
            }
            else if (node is VarTypeNode)
            {
                return GetType(node.Token, tables);
            }
            else
            {
                throw new UnknowTypeException(node.Token);
            }
        }

        public static List<SymType> GetArgsType(List<Node> args, SymbolTableTree tables)
        {
            var types_list = new List<SymType>();

            foreach (var arg in args)
            {
                if (arg.TypeNode is SymVar)
                    types_list.Add((arg.TypeNode as SymVar).Type);
                else
                    types_list.Add(arg.TypeNode as SymType);
            }

            return types_list;
        }

        private static SymType GetType(Token token, SymbolTableTree tables)
        {
            switch (token.Value)
            {
                case "integer":
                    return new SymTypeInteger();
                case "real":
                    return new SymTypeReal();
                case "char":
                    return new SymTypeChar();
                default:
                    return tables.GetSymbol(token) as SymType;
            }
        }
    }
}
