using NetLox.Environment;
using NetLox.Errors;
using NetLox.Model;
using NetLox.Scanner;

namespace NetLox.Interpretor
{
    public class Interpreter : AstClassesVisitor<object>
    {
        private EnvironmentData _environment = new();
        public void Interpret(IList<StatementBase> statements)
        {
            try
            {
                foreach(StatementBase statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                ErrorsUtils.RuntimeError(error);
            }
        }

        private void Execute(StatementBase statement)
        {
            statement.Accept(this);
        }

        private static string Stringify(object value)
        {
            if (value is null) return "nil";
            if (value is double d)
            {
                var svalue = d.ToString();
                if (svalue.EndsWith(".0"))
                    return svalue.Substring(0, svalue.Length - 2);
                return svalue;
            }
            return value.ToString();
        }

        private object Evaluate(ExpressionBase expression)
        {
            return expression.Accept(this);
        }
        public object Visit(Binary binary)
        {
            var left = Evaluate(binary.Left);
            var right = Evaluate(binary.Right);
            switch (binary.Operator.Type) {
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.GREATER:
                    CheckNumberOperand(binary.Operator, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperand(binary.Operator, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperand(binary.Operator, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperand(binary.Operator, left, right);
                    return (double)left <= (double)right;
                case TokenType.MINUS:
                    CheckNumberOperand(binary.Operator, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperand(binary.Operator, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperand(binary.Operator, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left is double dleft && right is double dright)
                        return dleft + dright;
                    else if (left is string sleft && right is string sright)
                        return sleft + sright;
                    throw new RuntimeError(binary.Operator, "Operands must be two numbers or two strings.");
                default:
                    return null;
                }
            }

        public object Visit(Grouping grouping)
        {
            return Evaluate(grouping.ExpressionIn);
        }

        public object Visit(Literal literal)
        {
            return literal.Value;
        }

        public object Visit(Unary unary)
        {
            var right = Evaluate(unary.Right);
            switch (unary.Operator.Type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(unary.Operator, right);
                    return -(double)right;
                default:
                    return null;
            }
        }

        private static bool IsTruthy(object val)
        {
            if(val is null) return false;
            if(val is bool b) return b;
            return true;
        }

        private static bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }

        private static void CheckNumberOperand(Token oper, params object[] operands)
        {
            foreach(var operandItem in operands)
            {
                if (operandItem is not double)
                    throw new RuntimeError(oper, "Operand must be a number.");
            }
        }

        public object Visit(ExpressionStmt expressionstmt)
        {
            Evaluate(expressionstmt.Expr);
            return null;
        }

        public object Visit(PrintStmt printstmt)
        {
            var val = Evaluate(printstmt.Expr);
            Console.WriteLine(Stringify(val));
            return null;
        }

        public object Visit(VarStmt varstmt)
        {
            object value = null;
            if(varstmt.Initializer is not null)
            {
                value = Evaluate(varstmt.Initializer);
            }
            _environment.Define(varstmt.Name.Lexeme, value);
            return null;
        }

        public object Visit(Variable variable)
        {
            return _environment.Get(variable.Name);
        }

        public object Visit(Assign assign)
        {
            var value = Evaluate(assign.Value);
            _environment.Assign(assign.Name, value);
            return value;
        }

        public object Visit(BlockStmt blockstmt)
        {
            ExecuteBlock(blockstmt.Statements, new EnvironmentData(_environment));
            return null;
        }

        private void ExecuteBlock(IList<StatementBase> statements, EnvironmentData environmentData)
        {
            EnvironmentData previous = _environment;
            try
            {
                _environment = environmentData;
                foreach(var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _environment = previous;
            }
        }

        public object Visit(IfStmt ifstmt)
        {
            if (IsTruthy(Evaluate(ifstmt.Condition)))
            {
                Execute(ifstmt.ThenBranch);
            }
            else if (ifstmt.ElseBranch is not null)
            {
                Execute(ifstmt.ElseBranch);
            }
            return null;
        }

        public object Visit(Logical logical)
        {
            var left = Evaluate(logical.Left);
            if (logical.Operator.Type == TokenType.OR)
            {
                if (IsTruthy(left))
                    return left;
            }
            else
            {
                if (!IsTruthy(left))
                    return left;
            }
            return Evaluate(logical.Right);
        }

        public object Visit(WhileStmt whilestmt)
        {
            while(IsTruthy(Evaluate(whilestmt.Condition)))
            {
                Execute(whilestmt.Body);
            }
            return null;
        }
    }
}
