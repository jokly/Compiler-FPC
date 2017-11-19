using System.Collections.Generic;

namespace Compiler_FPC.Parser
{
    class Symbol
    {
        string Name;

        public Symbol(string name)
        {
            Name = name;
        }
    }

    class SymbolTable
    {
        public SymbolTable Parent { get; protected set; } = null;
        private List<SymbolTable> Childrens = new List<SymbolTable>();

        private Dictionary<string, Node> Table = new Dictionary<string, Node>();

        public SymbolTable(SymbolTable parent = null)
        {
            Parent = parent;
        }

        public void AddChildTable(SymbolTable table)
        {
            Childrens.Add(table);
        }

        public void AddSymbol(Node symbol)
        {
            var symbolName = symbol.Token.Value;

            if (Table.ContainsKey(symbolName))
                throw new DuplicateDeclarationException(Table[symbolName].Token, symbol.Token);

            Table.Add(symbolName, symbol);
        }

        public bool IsExist(string name)
        {
            return Table.ContainsKey(name);
        }

        public void GetType(string name)
        {
            return; // Table[name]
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

        public void AddSymbol(Node symbol)
        {
            Current.AddSymbol(symbol);
        }

        public void GetType(Token token)
        {
            var name = token.Value;
            SymbolTable ptr = Current;

            while (ptr != null)
            {
                if (ptr.IsExist(name))
                {
                    return; //ptr.GetType(name);
                }

                ptr = ptr.Parent;
            }

            throw new NotFounIdException(token);
        }
    }
}
