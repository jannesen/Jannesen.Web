using System;
using System.Text;

namespace Jannesen.Web.Core;

internal sealed class DataSourceReader(string str)
{
    internal enum TokenType
    {
        Unknown,
        EOF,
        Name,
        QuoteName,              // []
        Colon,                  // :
        Comma,                  // ,
        LParen,                 // (
        RParen,                 // )
        Pipe                    // |
    }

    internal readonly struct Token(DataSourceReader reader, TokenType type, int start, int end)
    {
        public          DataSourceReader    Reader      => reader;
        public          TokenType           Type        => type;
        public          int                 Start       => start;
        public          int                 End         => end;
        public          int                 Length      => (end-start);

        public          Token               Test(TokenType type)
        {
            if (Type != type) {
                throw new FormatException($"Unexpected token {Type} expect {type}.");
            }

            return this;
        }
        public          Token               TestName()
        {
            if (!(Type is TokenType.Name or TokenType.QuoteName)) {
                throw new FormatException($"Unexpected token {Type} expect Name,QuoteName.");
            }

            return this;
        }

        public          string              GetString()
        {
            switch(Type) {
            case TokenType.Name:
                return reader.Str[start..end];

            case TokenType.QuoteName: {
                    var r = new StringBuilder(end-start-2);
                    var s = reader.Str.AsSpan()[(start + 1)..(end - 1)];

                    for(var i = 0 ; i < s.Length ; ++i) {
                        var c = s[i];
                        if (c == '\\' || c == '[') {
                            c = s[++i];
                        }
                        r.Append(c);
                    }
                    return r.ToString();
                }

            default:
                throw new InvalidOperationException($"GetString of {Type} is not posible.");
            }
        }
    }

    public          string      Str         => str;
    public          int         Posistion         { get; set; }
    private         Token?      _next;

    public          Token       Peek
    {
        get {
            if (!_next.HasValue) {
                _next = Next();
            }

            return _next.Value;
        }
    }
    public          Token       Next()
    {
        if (_next.HasValue) {
            var r = _next.Value;
            _next = null;
            return r;
        }

        if (Posistion >= Str.Length) {
            return new Token(this, TokenType.EOF, Posistion, Posistion);
        }

        var start     = Posistion++;
        var firstChar = str[start];

        switch(firstChar) {
        case ':':   return new Token(this, TokenType.Colon,  start, Posistion);
        case '(':   return new Token(this, TokenType.LParen, start, Posistion);
        case ')':   return new Token(this, TokenType.RParen, start, Posistion);
        case ',':   return new Token(this, TokenType.Comma,  start, Posistion);
        case '|':   return new Token(this, TokenType.Pipe,   start, Posistion);

        case '[':
            for (;;) {
                if (Posistion >= str.Length) {
                    throw new FormatException("Missing ']'.");
                }

                var c = str[Posistion++];
                switch(c) {
                case ']':
                    return new Token(this, TokenType.QuoteName, start, Posistion);

                case '\\':
                case '[':
                    ++Posistion;
                    break;
                }
            }

        default:
            if (_isNameFirst(firstChar)) {
                while (Posistion < str.Length && _isName(str[Posistion])) {
                    ++Posistion;
                }
            }

            return new Token(this, TokenType.Name, start, Posistion);
        }
    }
    public          bool        NextIf(TokenType type)
    {
        if (Peek.Type == type) {
            _next = null;
            return true;
        }

        return false;
    }

    private static  bool        _isNameFirst(char c)
    {
        return ('a' <= c && c <= 'z') ||
               ('A' <= c && c <= 'Z') ||
               c == '_';
    }
    private static  bool        _isName(char c)
    {
        return _isNameFirst(c) ||
               ('0' <= c && c <= '9') ||
               c == '-';
    }
}
