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
        public List<SymbolTable> Childrens { get; protected set; } = new List<SymbolTable>();

        public Dictionary<string, Node> Table { get; } = new Dictionary<string, Node>();

        public SymbolTable(SymbolTable parent = null)
        {
            Parent = parent;
        }

        public void AddSymbol(Node symbol)
        {
            var symbolName = symbol.Token.Value;

            if (Table.ContainsKey(symbolName))
                throw new DuplicateDeclarationException(Table[symbolName].Token, symbol.Token);

            Table.Add(symbolName, symbol);
        }

        public void AddChildTable(SymbolTable table)
        {
            Childrens.Add(table);
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

        public void GetType(string name)
        {

        }
    }
}
