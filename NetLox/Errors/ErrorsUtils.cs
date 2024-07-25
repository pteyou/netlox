using NetLox.Interpretor;
using NetLox.Scanner;

namespace NetLox.Errors
{
    public static class ErrorsUtils
    {
        public static bool _hadErrors = false;
        public static bool _hadRuntimeError = false;

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if(token.Type == TokenType.EOF)
            {
                Report(token.Line, "at end", message);
            }
            else
            {
                Report(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        public static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error {where} : {message}");
            _hadErrors = true;
        }

        internal static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine(error.Message + $"{System.Environment.NewLine} [line " + error.Tok.Line + "]");
            _hadRuntimeError = true;
        }
    }

    public class RuntimeError : ApplicationException
    {
        private Token oper;

        public RuntimeError(Token oper, string message) : base(message)
        {
            this.oper = oper;
        }

        public Token Tok => oper;
    }
}
