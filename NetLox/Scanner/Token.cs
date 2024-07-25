namespace NetLox.Scanner
{
    public class Token
    {
        private readonly TokenType _type;
        private readonly string _lexeme;
        private readonly object _literal;
        private readonly int _line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            _type = type;
            _lexeme = lexeme;
            _literal = literal;
            _line = line;
        }

        public TokenType Type
        {
            get
            {
                return _type;
            }
        }

        public string Lexeme
        {
            get
            {
                return _lexeme;
            }
        }

        public object Literal
        {
            get
            {
                return _literal;
            }
        }

        public int Line => _line;

        public override string ToString()
        {
            return $"{_type} {_lexeme} {_literal}";
        }
    }
}
