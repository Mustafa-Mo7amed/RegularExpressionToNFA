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
                else if (c != '*' && c != '+')
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
            var st = new Stack<Node>();
            var par = new Stack<Node>();
            par.Push(new Node());
            for (int i = 0; i < s.Length; i++) { // maybe i + 2 < s.Length
                if (s[i] == '*' || s[i] == '+')
                    continue;
                if (s[i] == '(') {
                    if (Char.IsAsciiLetter(s[i + 1])) {
                        var node1 = new Node();
                        var node2 = new Node();
                        node1.Parent = par.Peek();
                        node2.Parent = node1;
                        par.Peek().child.Add(new Tuple<Node?, char>(node1, '#'));
                        node1.child.Add(new Tuple<Node?, char>(node2, s[i + 1]));
                        par.Push(node1);
                        par.Push(node2);
                        ++i; // maybe i += 2
                    }
                    else {
                        var node = new Node();
                        node.Parent = par.Peek();
                        par.Peek().child.Add(new Tuple<Node?, char>(node, '#'));
                        par.Push(node);
                    }
                }
                else if (s[i] == ')') {
                    if (backEdge[i] == '.') {
                        par.Pop();
                        par.Pop();
                    }
                }
            }
        }
    }
}