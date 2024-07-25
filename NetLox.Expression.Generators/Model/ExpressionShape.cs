using System;
using System.Collections.Generic;

namespace NetLox.Expression.Generators.Model
{
    internal class ExpressionShape
    {
        public string ClassName { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
