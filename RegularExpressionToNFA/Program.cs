using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        //public Node(Node? Parent, Node? Begin, List<Tuple<Node?, char>> child) {
        //    this.id = ++global_id;
        //    this.child = child;
        //    this.Parent = Parent;
        //    this.Begin = Begin;
        //}
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
        static void dfs(int i, Node par, Node begin, ref string s, char[] backEdge) {
            if (i == s.Length)
                return;
            if (s[i] == '*' || s[i] == '+') {
                dfs(i + 1, par, begin, ref s, backEdge);
                return;
            }
            if (s[i] == '(') {
                var node = new Node();
                par.child.Add(new Tuple<Node?, char>(node, '#'));
                dfs(i + 1, node, node, ref s, backEdge);
            }
            else if (Char.IsAsciiLetter(s[i])) {
                var node1 = new Node();
                var node2 = new Node();
                node1.child.Add(new Tuple<Node?, char>(node2, s[i]));
                par.child.Add(new Tuple<Node?, char>(node1, '#'));
                dfs(i + 1, node2, begin, ref s, backEdge);
            }
            else if (s[i] == ')') {
                if (backEdge[i] == '*') {
                    var node = new Node();
                    par.child.Add(new Tuple<Node?, char>(node, '#'));
                    node.child.Add(new Tuple<Node?, char>(begin, '#'));
                    begin.child.Add(new Tuple<Node?, char>(node, '#'));
                    dfs(i + 1, node, begin, ref s, backEdge);
                }
                else if (backEdge[i] == '+') {
                    var node = new Node();
                    par.child.Add(new Tuple<Node?, char>(node, '#'));
                    node.child.Add(new Tuple<Node?, char>(begin, '#'));
                    dfs(i + 1, node, begin, ref s, backEdge);
                }
                else {
                    var node = new Node();
                    par.child.Add(new Tuple<Node?, char>(node, '#'));
                    dfs(i + 1, node, begin, ref s, backEdge);
                }
            }
        }
        static void Main(string[] args) {
            // input
            string s = Console.ReadLine();

            // check if the regular expression is valid
            if (s[0] == '*' || s[0] == '+' || s[0] == '|' || s[s.Length - 1] == '|') {
                Console.WriteLine("Invalid Regular Expression");
                return;
            }
            StringBuilder sb = new StringBuilder();
            foreach (char c in s) {
                if (c == '|') {
                    if (!regular(sb.ToString())) {
                        Console.WriteLine("Invalid Regular Expression");
                        return;
                    }
                    sb.Clear();
                }
                else if (c == '(' || c == ')')
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
            char[] backEdge = new char[s.Length];
            for (int i = 0; i < s.Length - 1; i++) {
                if (s[i] == ')') {
                    if (s[i + 1] == '*')
                        backEdge[i] = '*';
                    else if (s[i + 1] == '+')
                        backEdge[i] = '+';
                }
                else
                    backEdge[i] = '.';
            }
            // convert regular expression to NFA
            var root = new Node();
            //dfs(1, node, node, ref s, backEdge);
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
                    if (backEdge[i] == '*') {
                        var node = new Node();
                        par.child.Add(new Tuple<Node?, char>(node, '#'));
                        node.child.Add(new Tuple<Node?, char>(begin, '#'));
                        begin.child.Add(new Tuple<Node?, char>(node, '#'));
                        parentStack.Push(node);
                        beginStack.Pop();
                    }
                    else if (backEdge[i] == '+') {
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
            Console.WriteLine("NFA is created successfully");
            var visited = new HashSet<Node>();
            dfs(root, visited);
        }
        static void dfs(Node node, HashSet<Node> visited) {
            visited.Add(node);
            //Console.WriteLine("Node " + node.id + " :");
            foreach (var child in node.child) {
                if (child.Item1 == null)
                    continue;
                //Console.WriteLine("Edge " + child.Item2 + " to Node " + child.Item1.id);
                Console.WriteLine(node.id + " " + child.Item1.id + " " + child.Item2);
                Console.WriteLine();
                if (child.Item1 != null && !visited.Contains(child.Item1)) {
                    dfs(child.Item1, visited);

                }
            }
        }
    }
}