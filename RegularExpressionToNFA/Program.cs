using System.Text;

namespace RegularExpressionToNFA {
    class Node {
        static int global_id = 0;
        public int id;
        public Node? Parent, Begin;
        public List<Tuple<Node?, char>> child;
        public Node() {
            this.id = ++global_id;
            this.child = new List<Tuple<Node?, char>>();
            this.Parent = null;
            this.Begin = null;
        }
    }

    internal class Program {
        static bool regular(string s) {
            var st = new Stack<char>();
            foreach (char c in s) {
                if (c == '(')
                    st.Push(c);
                else {
                    if (st.Count == 0 || st.Peek() != '(')
                        return false;
                    st.Pop();
                }
            }
            return st.Count == 0;
        }
        static void Main(string[] args) {
            // input
            string s = Console.ReadLine();

            // check if the regular expression is valid
            if (s[0] == '*' || s[0] == '+' || s.Contains('|')) {
                Console.WriteLine("Invalid Regular Expression");
                return;
            }
            for (int i = 0; i < s.Length - 1; i++) {
                if (s[i] == s[i + 1]) {
                    if (s[i] == '*' || s[i] == '+') {
                        Console.WriteLine("Invalid Regular Expression");
                        return;
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in s) {
                if (c == '(' || c == ')')
                    sb.Append(c);
            }
            if (!regular(sb.ToString())) {
                Console.WriteLine("Invalid Regular Expression");
                return;
            }
            sb.Clear();
            // formmating input
            foreach (char c in s) {
                if (Char.IsAsciiLetter(c)) {
                    sb.Append('(');
                    sb.Append(c);
                    sb.Append(')');
                }
                else
                    sb.Append(c);
            }
            s = sb.ToString();
            // convert regular expression to NFA
            var root = new Node();
            var parentStack = new Stack<Node>();
            var beginStack = new Stack<Node>();
            parentStack.Push(root);
            beginStack.Push(root);
            for (int i = 1; i < s.Length; ++i) {
                if (s[i] == '*' || s[i] == '+')
                    continue;
                var par = parentStack.Peek();
                var begin = beginStack.Count != 0 ? beginStack.Peek() : null;
                if (s[i] == '(') {
                    var node = new Node();
                    par.child.Add(new Tuple<Node?, char>(node, '#'));
                    parentStack.Push(node);
                    beginStack.Push(node);
                }
                else if (Char.IsAsciiLetter(s[i])) {
                    var node = new Node();
                    par.child.Add(new Tuple<Node?, char>(node, s[i]));
                    parentStack.Push(node);
                }
                else if (s[i] == ')') {
                    if (i + 1 < s.Length && s[i + 1] == '*') {
                        var node = new Node();
                        par.child.Add(new Tuple<Node?, char>(node, '#'));
                        node.child.Add(new Tuple<Node?, char>(begin, '#'));
                        begin?.child.Add(new Tuple<Node?, char>(node, '#'));
                        parentStack.Push(node);
                        beginStack.Pop();
                    }
                    else if (i + 1 < s.Length && s[i + 1] == '+') {
                        var node = new Node();
                        par.child.Add(new Tuple<Node?, char>(node, '#'));
                        node.child.Add(new Tuple<Node?, char>(begin, '#'));
                        parentStack.Push(node);
                        beginStack.Pop();
                    }
                    else
                        beginStack.Pop();
                }
            }
            Console.WriteLine("NFA is created successfully\n");
            var visited = new HashSet<Node>();
            dfs(root, visited);
        }
        static void dfs(Node node, HashSet<Node> visited) {
            visited.Add(node);
            foreach (var child in node.child) {
                if (child.Item1 == null)
                    continue;
                Console.WriteLine(node.id + " " + child.Item1.id + " " + child.Item2 + "\n");
                if (child.Item1 != null && !visited.Contains(child.Item1))
                    dfs(child.Item1, visited);
            }
        }
    }
}