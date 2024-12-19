namespace Limonaca;

public static class Program
{
    private static void Main()
    {
        try
        {
            var input = File.ReadAllText("code.liml");
            if (input == null) throw new FileLoadException("Unable to load file.");

            var tokens = Lexer.Tokenize(input);

            Console.WriteLine("Tokens:");
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }

            var ast = Parser.Parse(tokens);
            Console.WriteLine("\nParsed AST:");
            PrintAst(ast, 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void PrintAst(Node? node, int indent)
    {
        while (true)
        {
            if (node == null) return;

            var indentStr = new string(' ', indent);
            Console.WriteLine($"{indentStr}{node.GetType()} {node.GetValue()}");

            PrintAst(node.GetLeft(), indent + 2);
            node = node.GetRight();
            indent = indent + 2;
        }
    }
}