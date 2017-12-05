using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC.Parser
{
    class SymVar : Symbol
    {
        public SymType Type { get; protected set; }

        public SymVar(Node node, SymType type) : base(node)
        {
            Type = type;
            Node = node;
        }
    }
}
