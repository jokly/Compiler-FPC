using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_FPC.Parser
{
    class SyntaxTree
    {
        public Node Root { get; }
        public string TreeString { get; private set; } = "";

        public SyntaxTree(Node root)
        {
            this.Root = root;

            setTreeString(root, "");
        }

        private void setTreeString(Node root, string indent, bool last = false)
        {
            if (root == null) return;

            TreeString += indent;
            if (last)
            {
                TreeString += "└─";
                indent += "  ";
            }
            else
            {
                TreeString += "├─";
                indent += "│ ";
            }

            TreeString += root.Token.Value + '\n';

            setTreeString(root.Left, indent);
            setTreeString(root.Right, indent, true);
        }
    }
}
