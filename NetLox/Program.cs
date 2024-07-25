using NetLox.Errors;
using NetLox.Interpretor;
using NetLox.Model;
using NetLox.Parser;
using NetLox.Printer;
using NetLox.Scanner;

namespace NetLox
{
    public class Program
    {
        private  static readonly Interpreter _interpreter = new Interpreter();
        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage : NetLox [script]");
                System.Environment.Exit(-1);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                var cmd = Console.ReadLine();
                if (cmd == null)
                {
                    break;
                }
                Run(cmd);
                ErrorsUtils._hadErrors = false;
            }
        }

        private static void Run(string cmd)
        {
            var scanner = new Lexer(cmd);
            var tokenList = scanner.ScanTokens();
            var parser = new RDParser(tokenList);
            var statements = parser.Parse();
            if(ErrorsUtils._hadErrors)
            {
                return;
            }
            _interpreter.Interpret(statements);
        }

        private static void RunFile(string inputScript)
        {
            var script = new StreamReader(inputScript).ReadToEnd();
            Run(script);
            if (ErrorsUtils._hadErrors) System.Environment.Exit(60);
            if (ErrorsUtils._hadRuntimeError) System.Environment.Exit(70);
        }
    }
}