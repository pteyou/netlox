using NetLox.Errors;
using System.Globalization;

namespace NetLox.Scanner
{
    public class Lexer
    {
        private readonly string _source;
        private int _start, _current, _line;
        private readonly IList<Token> _tokens = new List<Token>();

        public Lexer(string source)
        {
            _source = source;
            _start = 0;
            _current = 0;
            _line = 1;
        }
        public IList<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }
            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return _tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
                    if(Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    ++_line;
                    break;
                case '"':
                    GetString();
                    break;
                default:
                    if(char.IsDigit(c))
                    {
                        GetNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        GetIdentifier();
                    }
                    else
                    {
                        ErrorsUtils.Error(_line, "Unexpected character");
                    }
                    break;
            }
        }

        private static bool IsAlpha(char c)
        {
            return char.IsLetter(c) || c == '_';
        }

        private static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || char.IsDigit(c);
        }

        private void GetIdentifier()
        {
            while(IsAlphaNumeric(Peek()) && !IsAtEnd())
            {
                Advance();
            }
            var isKeyword = Keywords.KeyW.TryGetValue(_source[_start.._current],
                out var tokenType);
            if(isKeyword)
                AddToken(tokenType);
            else
                AddToken(TokenType.IDENTIFIER);
        }

        private void GetNumber()
        {
            while(char.IsDigit(Peek()))
            {
                Advance();
            }
            if(Peek() == '.' && char.IsDigit(PeekNext()))
            {
                Advance();
                while (char.IsDigit(Peek()))
                {
                    Advance();
                }
            }
            AddToken(TokenType.NUMBER, double.Parse(_source[_start.._current], 
                NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture));
        }

        private char PeekNext()
        {
            if(_current + 1 >= _source.Length)
            {
                return '\0';
            }
            return _source[_current + 1];
        }

        private void GetString()
        {
            while(Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') ++_line;
                Advance();
            }
            if(IsAtEnd())
            {
                ErrorsUtils.Error(_line, "Unterminated string");
                return;
            }
            Advance();
            var value = _source[(_start + 1)..(_current - 1)];
            AddToken(TokenType.STRING, value);
        }

        private char Peek()
        {
            if(IsAtEnd())
            {
                return '\0';
            }
            return _source[_current];
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected)
            {
                return false;
            }
            ++_current;
            return true;
        }

        private char Advance()
        {
            ++_current;
            return _source[_current - 1];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            string lexeme = _source[_start.._current];
            _tokens.Add(new Token(type, lexeme, literal, _line));
        }
        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }
    }
}
