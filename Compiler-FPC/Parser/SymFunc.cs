using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC.Parser
{
    class SymProc : Symbol
    {
        public SymProc(Node node) : base(node)
        {

        }
    }

    class SymFunc : SymProc
    {
        public SymFunc(Node node) : base(node)
        {

        }
    }
}
