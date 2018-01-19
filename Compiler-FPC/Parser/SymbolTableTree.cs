using System.Collections.Generic;

namespace Compiler_FPC.Parser
{
    class SymbolTable
    {
        public SymbolTable Parent { get; protected set; } = null;
        private List<SymbolTable> Childrens = new List<SymbolTable>();
        private int Offset = 0;

        private Dictionary<string, Symbol> Table = new Dictionary<string, Symbol>();

        public SymbolTable(SymbolTable parent = null)
        {
            Parent = parent;
        }

        public void AddChildTable(SymbolTable table)
        {
            Childrens.Add(table);
        }

        public void AddSymbol(Symbol symbol)
        {
            var symbolName = symbol.Node.Token.Value;
            
            if (Table.ContainsKey(symbolName))
            {
                var forward = Table[symbolName];

                if (symbol is SymTypeProc && forward is SymTypeProc && (forward as SymTypeProc).IsForward)
                {
                    // TODO type matching

                    Table[symbolName] = symbol;
                    return;
                }
                else
                    throw new DuplicateDeclarationException(Table[symbolName].Node.Token, symbol.Node.Token);
            }

            if (symbol is SymVar)
            {
                Offset += (symbol as SymVar).Type.Size;
                (symbol as SymVar).Offset = Offset;
            }

            Table.Add(symbolName, symbol);
        }

        public bool IsExist(string name)
        {
            return Table.ContainsKey(name);
        }

        public Symbol GetSymbol(Token token)
        {
            var name = token.Value;

            if (!IsExist(name))
                throw new NotFounIdException(token);

            return Table[name];
        }
    }

    class SymbolTableTree
    {
        public SymbolTable Root { get; } = new SymbolTable();
        public SymbolTable Current { get; protected set; }

        public SymbolTableTree()
        {
            Current = Root;
        }

        public void NewTable()
        {
            var newTable = new SymbolTable(Current);
            Current.AddChildTable(newTable);

            Current = newTable;
        }

        public void BackTable()
        {
            Current = Current.Parent;
        }

        public void AddSymbol(Symbol symbol)
        {
            Current.AddSymbol(symbol);
        }

        public Symbol GetSymbol(Token token)
        {
            var name = token.Value;
            SymbolTable ptr = Current;

            while (ptr != null)
            {
                if (ptr.IsExist(name))
                {
                    return ptr.GetSymbol(token);
                }

                ptr = ptr.Parent;
            }

            throw new NotFounIdException(token);
        }
    }
}
