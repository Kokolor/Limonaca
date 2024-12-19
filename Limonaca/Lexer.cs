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
    KeywordDevprint,
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
            "devprint" => new Token(TokenType.KeywordDevprint, value),
            "int32" => new Token(TokenType.TypeInt32, value),
            "int16" => new Token(TokenType.TypeInt16, value),
            _ => isIdentifier ? new Token(TokenType.Identifier, value) : new Token(TokenType.Number, value)
        };
    }
}