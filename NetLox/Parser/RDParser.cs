using NetLox.Errors;
using NetLox.Model;
using NetLox.Scanner;

namespace NetLox.Parser
{
    public class RDParser
    {
        private readonly IList<Token> _tokens;
        private int _current;

        public RDParser(IList<Token> tokens)
        {
            _tokens = tokens;
            _current = 0;
        }

        public IList<StatementBase> Parse()
        {
            var statements = new List<StatementBase>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }
            return statements;
        }

        private StatementBase Declaration()
        {
            try
            {
                if (Match(TokenType.VAR))
                {
                    return VarDeclaration();
                }
                return Statement();
            }
            catch (ParsingError)
            {
                Synchronize();
                return null;
            }
        }

        private StatementBase VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name");
            ExpressionBase initializer = null;
            if(Match(TokenType.EQUAL))
            {
                initializer = ExpressionRule();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new VarStmt
            {
                Name = name,
                Initializer = initializer
            };
        }

        private StatementBase Statement()
        {
            if (Match(TokenType.FOR))
            {
                return ForStatement();
            }
            if (Match(TokenType.IF))
            {
                return IfStatement();
            }
            if (Match(TokenType.PRINT))
            {
                return PrintStatement();
            }
            if (Match(TokenType.WHILE))
            {
                return WhileStatement();
            }
            if (Match(TokenType.LEFT_BRACE))
            {
                return new BlockStmt
                {
                    Statements = BlockRule()
                };
            }
            return ExpressionStatement();
        }

        private StatementBase ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
            StatementBase initializer;
            if(Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            ExpressionBase condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = ExpressionRule();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition");

            ExpressionBase increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = ExpressionRule();
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses");

            var body = Statement();

            if(increment is not null)
            {
                body = new BlockStmt
                {
                    Statements = new List<StatementBase> { body, new ExpressionStmt { Expr = increment } }
                };
            }

            if (condition is null) condition = new Literal
            {
                Value = true
            };
            body = new WhileStmt
            {
                Body = body,
                Condition = condition
            };

            if (initializer is not null)
            {
                body = new BlockStmt
                {
                    Statements = new List<StatementBase>() { initializer, body }
                };
            }
            return body;
        }

        private StatementBase WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            var condition = ExpressionRule();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'while' condition.");
            var body = Statement();
            return new WhileStmt
            {
                Body = body,
                Condition = condition
            };
        }

        private StatementBase IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            var condition = ExpressionRule();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'if' condition.");
            var thenBranch = Statement();
            StatementBase elseBranch = null;
            if(Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }
            return new IfStmt
            {
                Condition = condition,
                ThenBranch = thenBranch,
                ElseBranch = elseBranch
            };
        }

        private List<StatementBase> BlockRule()
        {
            var statements = new List<StatementBase>();
            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }
            Consume(TokenType.RIGHT_BRACE, "Expect '}' after a block.");
            return statements;
        }

        private StatementBase ExpressionStatement()
        {
            ExpressionBase value = ExpressionRule();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new ExpressionStmt
            {
                Expr = value
            };
        }

        private StatementBase PrintStatement()
        {
            ExpressionBase value = ExpressionRule();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new PrintStmt
            {
                Expr = value
            };
        }

        private ExpressionBase ExpressionRule()
        {
            return AssignmentRule();
        }

        private ExpressionBase AssignmentRule()
        {
            var expr = OrRule();
            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                var value = AssignmentRule();
                if (expr is Variable variable)
                {
                    Token name = variable.Name;
                    return new Assign
                    {
                        Name = name,
                        Value = value
                    };
                }
                else
                {
                    Error(equals, "Invalid assignment target.");
                }
            }
            return expr;
        }

        private ExpressionBase OrRule()
        {
            var expr = AndRule();
            while (Match(TokenType.OR))
            {
                Token oper = Previous();
                var right = AndRule();
                expr = new Logical
                {
                    Operator = oper,
                    Left = expr,
                    Right = right
                };
            }
            return expr;
        }

        private ExpressionBase AndRule()
        {
            var expr = EqualityRule();
            while (Match(TokenType.AND))
            {
                Token oper = Previous();
                var right = EqualityRule();
                expr = new Logical
                {
                    Operator = oper,
                    Left = expr,
                    Right = right
                };
            }
            return expr;
        }

        private ExpressionBase EqualityRule()
        {
            var expression = ComparisonRule();
            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token oper = Previous();
                var right = ComparisonRule();
                expression = new Binary
                {
                    Left = expression,
                    Right = right,
                    Operator = oper
                };
            }
            return expression;
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private bool Match(params TokenType[] tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                if (Check(tokenType))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType tokenType)
        {
            if (IsAtEnd())
            {
                return false;
            }
            else
            {
                return Peek().Type == tokenType;
            }
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Advance()
        {
            if (!IsAtEnd())
            {
                ++_current;
            }
            return Previous();
        }

        private ExpressionBase ComparisonRule()
        {
            var expression = TermRule();
            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL,
                TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token oper = Previous();
                var right = TermRule();
                expression = new Binary
                {
                    Left = expression,
                    Operator = oper,
                    Right = right
                };
            }
            return expression;
        }

        private ExpressionBase TermRule()
        {
            var expression = FactorRule();
            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token oper = Previous();
                var right = FactorRule();
                expression = new Binary
                {
                    Left = expression,
                    Operator = oper,
                    Right = right
                };
            }
            return expression;
        }

        private ExpressionBase FactorRule()
        {
            var expression = UnaryRule();
            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token oper = Previous();
                var right = UnaryRule();
                expression = new Binary
                {
                    Left = expression,
                    Operator = oper,
                    Right = right
                };
            }
            return expression;
        }

        private ExpressionBase UnaryRule()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token oper = Previous();
                var right = UnaryRule();
                return new Unary
                {
                    Operator = oper,
                    Right = right
                };
            }
            return PrimaryRule();
        }

        private ExpressionBase PrimaryRule()
        {
            if (Match(TokenType.TRUE)) return new Literal { Value = true };
            if (Match(TokenType.FALSE)) return new Literal { Value = false };
            if (Match(TokenType.NIL)) return new Literal { Value = null };
            if (Match(TokenType.NUMBER, TokenType.STRING))
                return new Literal { Value = Previous().Literal };
            if (Match(TokenType.IDENTIFIER))
            {
                return new Variable
                {
                    Name = Previous()
                };
            }
            if (Match(TokenType.LEFT_PAREN))
            {
                var expression = ExpressionRule();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping { ExpressionIn = expression };
            }
            throw Error(Peek(), "Expecting Expression");
        }

        private Token Consume(TokenType tokenType, string message)
        {
            if (Check(tokenType))
            {
                return Advance();
            }
            throw Error(Peek(), message);
        }

        private static ParsingError Error(Token token, string message)
        {
            ErrorsUtils.Error(token, message);
            return new ParsingError();
        }

        private void Synchronize()
        {
            Advance();
            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.SEMICOLON)
                {
                    return;
                }
                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }
                Advance();
            }
        }
    }

    public class ParsingError : Exception { }
}
