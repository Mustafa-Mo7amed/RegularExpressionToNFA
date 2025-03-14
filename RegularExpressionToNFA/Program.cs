using System.Text;

namespace RegularExpressionToNFA {
    class Node {
        static int global_id = 0; // giving each node a unique id
        public int id;
        public List<Tuple<Node?, char>> child; // stores the child nodes and the letter on the edge between them
        public Node() {
            this.id = ++global_id;
            this.child = new List<Tuple<Node?, char>>();
        }
    }

    internal class Program {
        // takes a regular expression and returns true if it is valid (i.e. it has balanced parentheses)
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

            // check if the regular expression is valid (this version doesn't handle | yet)
            if (s[0] == '*' || s[0] == '+' || s.Contains('|')) {
                Console.WriteLine("Invalid Regular Expression");
                return;
            }

            // if there's ** or ++ in the regular expression then it's invalid
            for (int i = 0; i < s.Length - 1; i++) {
                if (s[i] == s[i + 1]) {
                    if (s[i] == '*' || s[i] == '+') {
                        Console.WriteLine("Invalid Regular Expression");
                        return;
                    }
                }
            }

            // takes all parentheses to check if there are balanced
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
            // formmating input as if AB --> (A)(B) (that format is easier to handle)
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
            var parentStack = new Stack<Node>(); // stores the last visited node
            var beginStack = new Stack<Node>(); // the node created when meeting '(' to handle * and +
            var root = new Node();
            parentStack.Push(root);
            beginStack.Push(root);
            for (int i = 1; i < s.Length; ++i) {
                if (s[i] == '*' || s[i] == '+')
                    continue;
                var par = parentStack.Peek(); // the last visited node
                var begin = beginStack.Count != 0 ? beginStack.Peek() : null; // the last open parentheses '('
                if (s[i] == '(') { // if we meet '(' then we create a new node and push it to the stacks
                    var node = new Node();
                    par.child.Add(new Tuple<Node?, char>(node, '#'));
                    parentStack.Push(node);
                    beginStack.Push(node);
                }
                else if (Char.IsAsciiLetter(s[i])) { // if we meet a letter then we create a new node and push it to the parent stack
                    var node = new Node();
                    par.child.Add(new Tuple<Node?, char>(node, s[i]));
                    parentStack.Push(node);
                }
                else if (s[i] == ')') {
                    // if we meet ')' and the next character is '*' then we create a new node and push it to the parent stack
                    // then connect this node to the begin node and vice vera to allow not taking the letter and taking multiple times
                    if (i + 1 < s.Length && s[i + 1] == '*') {
                        var node = new Node();
                        par.child.Add(new Tuple<Node?, char>(node, '#'));
                        node.child.Add(new Tuple<Node?, char>(begin, '#'));
                        begin?.child.Add(new Tuple<Node?, char>(node, '#'));
                        parentStack.Push(node);
                        beginStack.Pop(); // pop the begin node as we already connected it to the new node
                    }
                    // same as the '*' but without adding an edge from the beginning to the end to only allow taking the letter multiple time (at least once)
                    else if (i + 1 < s.Length && s[i + 1] == '+') {
                        var node = new Node();
                        par.child.Add(new Tuple<Node?, char>(node, '#'));
                        node.child.Add(new Tuple<Node?, char>(begin, '#'));
                        parentStack.Push(node);
                        beginStack.Pop(); // pop the begin node as we already connected it to the new node
                    }
                    else
                        beginStack.Pop(); // pop the begin node as we already connected it to the new node
                }
            }
            Console.WriteLine("NFA is created successfully\n");
            // print the NFA
            var visited = new HashSet<Node>(); // using a HashSet to avoid cycles
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