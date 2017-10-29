namespace Compiler_FPC.Parser
{
    class SyntaxTree
    {
        public Node Root { get; }
        public string TreeString { get; private set; } = "";

        public SyntaxTree(Node root)
        {
            Root = root;

            setTreeString(root, "", true);
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

            if (!root.BlockName.Equals(""))
            {
                TreeString += root.BlockName + " ";
            }

            TreeString += root.Token.Value + '\n';

            for (var i = 0; i < root.Childrens.Count; i++)
            {
                setTreeString(root.Childrens[i], indent, i == root.Childrens.Count - 1 && root.Left == null && root.Right == null);
            }

            setTreeString(root.Left, indent, root.Right == null);
            setTreeString(root.Right, indent, true);
        }
    }
}
