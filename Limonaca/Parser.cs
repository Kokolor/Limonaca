namespace Limonaca;

public enum NodeType
{
    Plus,
    Minus,
    Multiply,
    Divide,
    Number,
    Identifier,
    Devprint,
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
        return ParseStatement(tokens);
    }

    private static Node ParseStatement(List<Token> tokens)
    {
        var token = tokens[_currentTokenIndex];

        if (token.GetType() == TokenType.KeywordDevprint)
        {
            _currentTokenIndex++;
            var expr = ParseExpression(tokens);
            if (_currentTokenIndex < tokens.Count && tokens[_currentTokenIndex].GetType() == TokenType.SemiColon)
            {
                _currentTokenIndex++;
            }
            else
            {
                throw new InvalidOperationException("Expected ';' at the end of the statement.");
            }

            return new Node(NodeType.Devprint, expr);
        }

        var exprNode = ParseExpression(tokens);
        if (_currentTokenIndex < tokens.Count && tokens[_currentTokenIndex].GetType() == TokenType.SemiColon)
        {
            _currentTokenIndex++;
        }
        else
        {
            throw new InvalidOperationException("Expected ';' at the end of the statement.");
        }

        return exprNode;
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
        var token = tokens[_currentTokenIndex];

        if (token.GetType() == TokenType.ParenthesisOpen)
        {
            _currentTokenIndex++;
            var expr = ParseExpression(tokens);
            _currentTokenIndex++;
            return expr;
        }

        _currentTokenIndex++;
        return token.GetType() switch
        {
            TokenType.Number => new Node(NodeType.Number, value: token.GetValue()),
            TokenType.Identifier => new Node(NodeType.Identifier, value: token.GetValue()),
            TokenType.KeywordDevprint => new Node(NodeType.Devprint, value: token.GetValue()),
            _ => throw new ArgumentException($"Unexpected token: {token}")
        };
    }
}