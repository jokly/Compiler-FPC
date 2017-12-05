using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC.Parser
{
    class Symbol
    {
        public Node Node { get; protected set; }

        public Symbol(Node node)
        {
            Node = node;
        }
    }
}
