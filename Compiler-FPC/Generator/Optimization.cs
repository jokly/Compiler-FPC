using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC.Generator
{
    class Optimization
    {
        public int Size { get; protected set; }
        public List<AsmNode> Old { get; protected set; }
        public List<AsmNode> New { get; protected set; }

        public Optimization(int size, List<AsmNode> old, List<AsmNode> _new)
        {
            Size = size;
            Old = old;
            New = _new;
        }
    }
}
