using NetLox.Model;
using System.Text;

namespace NetLox.Printer
{
    internal class AstPrinter : AstClassesVisitor<string>
    {
        public string Print(ExpressionBase expr)
        {
            return expr.Accept(this);
        }

        public string Visit(Binary binary)
        {
            return Paranthese(binary.Operator.Lexeme, binary.Left, binary.Right);
        }

        public string Visit(Grouping grouping)
        {
            return Paranthese("group", grouping.ExpressionIn);
        }

        public string Visit(Literal literal)
        {
            if (literal.Value is null) return "nil";
            return literal.Value.ToString()!;
        }

        public string Visit(Unary unary)
        {
            return Paranthese(unary.Operator.Lexeme, unary.Right);
        }

        public string Visit(ExpressionStmt expressionstmt)
        {
            throw new NotImplementedException();
        }

        public string Visit(PrintStmt printstmt)
        {
            throw new NotImplementedException();
        }

        public string Visit(VarStmt varstmt)
        {
            throw new NotImplementedException();
        }

        public string Visit(Variable variable)
        {
            throw new NotImplementedException();
        }

        public string Visit(Assign assign)
        {
            throw new NotImplementedException();
        }

        public string Visit(BlockStmt blockstmt)
        {
            throw new NotImplementedException();
        }

        public string Visit(IfStmt ifstmt)
        {
            throw new NotImplementedException();
        }

        public string Visit(WhileStmt whilestmt)
        {
            throw new NotImplementedException();
        }

        public string Visit(Logical logical)
        {
            throw new NotImplementedException();
        }

        private string Paranthese(string name, params ExpressionBase[] expressionArray)
        {
            var builder = new StringBuilder();
            builder.Append('(').Append(name);
            foreach (var expression in expressionArray)
            {
                builder.Append(' ');
                builder.Append(expression.Accept(this));
            }
            builder.Append(')');
            return builder.ToString();
        }
    }
}
