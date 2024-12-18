namespace Limonaca;

public enum TokenType
{
    Plus,
    Minus,
    Star,
    Slash,
    Identifier,
    Number,
    KeywordIn,
    KeywordFn,
    KeywordReturn,
    KeywordAs,
    KeywordBegin,
    KeywordEnd,
    KeywordNone,
    KeywordVar,
    TypeInt32,
    TypeInt16,
    Colon,
    SemiColon,
    Comma,
    Arrow,
    Assignment,
    ParenthesisOpen,
    ParenthesisClose,
    Eof,
}

public class Token(TokenType type, string value)
{
    private TokenType Type { get; } = type;
    private string Value { get; } = value;

    public new TokenType GetType() => Type;
    public string GetValue() => Value;

    public override string ToString() => $"{Type} {Value}";
}

public static class Lexer
{
    public static List<Token> Tokenize(string input)
    {
        var tokens = new List<Token>();
        var currentValue = "";
        var isIdentifier = false;

        foreach (var c in input)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!string.IsNullOrEmpty(currentValue))
                {
                    tokens.Add(CreateToken(currentValue, isIdentifier));
                    currentValue = "";
                    isIdentifier = false;
                }

                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                currentValue += c;
                isIdentifier = true;
            }
            else if (char.IsDigit(c))
            {
                currentValue += c;
            }
            else
            {
                if (!string.IsNullOrEmpty(currentValue))
                {
                    tokens.Add(CreateToken(currentValue, isIdentifier));
                    currentValue = "";
                    isIdentifier = false;
                }

                tokens.Add(c switch
                {
                    '+' => new Token(TokenType.Plus, ""),
                    '-' => new Token(TokenType.Minus, ""),
                    '*' => new Token(TokenType.Star, ""),
                    '/' => new Token(TokenType.Slash, ""),
                    ':' => new Token(TokenType.Colon, ""),
                    ';' => new Token(TokenType.SemiColon, ""),
                    ',' => new Token(TokenType.Comma, ""),
                    '=' => new Token(TokenType.Assignment, ""),
                    '(' => new Token(TokenType.ParenthesisOpen, ""),
                    ')' => new Token(TokenType.ParenthesisClose, ""),
                    '>' => new Token(TokenType.Arrow, ""),
                    _ => throw new ArgumentException($"Unrecognized character: {c}")
                });
            }
        }

        if (!string.IsNullOrEmpty(currentValue))
        {
            tokens.Add(CreateToken(currentValue, isIdentifier));
        }

        tokens.Add(new Token(TokenType.Eof, ""));
        return tokens;
    }

    private static Token CreateToken(string value, bool isIdentifier)
    {
        return value switch
        {
            "in" => new Token(TokenType.KeywordIn, value),
            "fn" => new Token(TokenType.KeywordFn, value),
            "return" => new Token(TokenType.KeywordReturn, value),
            "begin" => new Token(TokenType.KeywordBegin, value),
            "end" => new Token(TokenType.KeywordEnd, value),
            "as" => new Token(TokenType.KeywordAs, value),
            "none" => new Token(TokenType.KeywordNone, value),
            "var" => new Token(TokenType.KeywordVar, value),
            "int32" => new Token(TokenType.TypeInt32, value),
            "int16" => new Token(TokenType.TypeInt16, value),
            _ => isIdentifier ? new Token(TokenType.Identifier, value) : new Token(TokenType.Number, value)
        };
    }
}

public enum NodeType
{
    Plus,
    Minus,
    Multiply,
    Divide,
    Number,
    Identifier,
}

public class Node(NodeType type, Node? left = null, Node? right = null, string value = "")
{
    private NodeType Type { get; } = type;
    private Node? Left { get; } = left;
    private Node? Right { get; } = right;
    private string Value { get; } = value;

    public new NodeType GetType() => Type;
    public string GetValue() => Value;
    public Node? GetLeft() => Left;
    public Node? GetRight() => Right;

    public override string ToString() => $"{Type} {Value}";
}

public static class Parser
{
    private static int _currentTokenIndex;

    public static Node Parse(List<Token> tokens)
    {
        _currentTokenIndex = 0;
        return ParseExpression(tokens);
    }

    private static Node ParseExpression(List<Token> tokens)
    {
        var left = ParseTerm(tokens);

        while (_currentTokenIndex < tokens.Count &&
               (tokens[_currentTokenIndex].GetType() == TokenType.Plus ||
                tokens[_currentTokenIndex].GetType() == TokenType.Minus))
        {
            var operatorToken = tokens[_currentTokenIndex];
            _currentTokenIndex++;

            var right = ParseTerm(tokens);
            left = new Node(
                operatorToken.GetType() switch
                {
                    TokenType.Plus => NodeType.Plus,
                    TokenType.Minus => NodeType.Minus,
                    _ => throw new InvalidOperationException("Unexpected operator")
                },
                left,
                right
            );
        }

        return left;
    }

    private static Node ParseTerm(List<Token> tokens)
    {
        var left = ParseFactor(tokens);

        while (_currentTokenIndex < tokens.Count &&
               (tokens[_currentTokenIndex].GetType() == TokenType.Star ||
                tokens[_currentTokenIndex].GetType() == TokenType.Slash))
        {
            var operatorToken = tokens[_currentTokenIndex];
            _currentTokenIndex++;

            var right = ParseFactor(tokens);
            left = new Node(
                operatorToken.GetType() switch
                {
                    TokenType.Star => NodeType.Multiply,
                    TokenType.Slash => NodeType.Divide,
                    _ => throw new InvalidOperationException("Unexpected operator")
                },
                left,
                right
            );
        }

        return left;
    }

    private static Node ParseFactor(List<Token> tokens)
    {
        var token = tokens[_currentTokenIndex++];

        return token.GetType() switch
        {
            TokenType.Number => new Node(NodeType.Number, value: token.GetValue()),
            TokenType.Identifier => new Node(NodeType.Identifier, value: token.GetValue()),
            _ => throw new ArgumentException($"Unexpected token: {token}")
        };
    }
}

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