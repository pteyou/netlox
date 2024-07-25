using NetLox.Errors;
using NetLox.Scanner;

namespace NetLox.Environment
{
    public class EnvironmentData
    {
        private readonly Dictionary<string, object> Values;
        private readonly EnvironmentData Enclosing;

        public EnvironmentData()
        {
            Values = new Dictionary<string, object>();
            Enclosing = null;
        }

        public EnvironmentData(EnvironmentData enclosing) : this()
        {
            Enclosing = enclosing;
        }

        public void Define(string name, object value)
        {
            Values[name] = value;
        }

        public object Get(Token name)
        {
            if (!Values.TryGetValue(name.Lexeme, out object? value))
            {
                if (Enclosing is not null)
                {
                    return Enclosing.Get(name);
                }
                throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
            }
            else
            {
                return value;
            }
        }

        public void Assign(Token name, object value)
        {
            if(!Values.ContainsKey(name.Lexeme))
            {
                if (Enclosing is not null)
                {
                    Enclosing.Assign(name, value);
                    return;
                }
                throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
            }
            Values[name.Lexeme] = value;
        }
    }
}
