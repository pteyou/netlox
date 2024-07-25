using System;
using System.Collections.Generic;
using System.Linq;

namespace NetLox.Expression.Generators.Model
{
    internal class ExpressionsToGenerate : IEquatable<ExpressionsToGenerate>
    {
        public IList<ClassToGenerate> ExpressionClasses { get; set; }

        public bool Equals(ExpressionsToGenerate? other)
        {
            return other is not null &&
                ExpressionClasses.SequenceEqual(other.ExpressionClasses);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ExpressionsToGenerate);
        }

        public override int GetHashCode()
        {
            return -954604087 + EqualityComparer<IList<ClassToGenerate>>.Default.GetHashCode(ExpressionClasses);
        }
    }
}
