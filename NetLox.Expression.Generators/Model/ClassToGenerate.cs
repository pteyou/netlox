using System;
using System.Collections.Generic;
using System.Linq;

namespace NetLox.Expression.Generators.Model
{
    internal class ClassToGenerate : IEquatable<ClassToGenerate?>
    {
        public ClassToGenerate(string nameSpace, string className, string baseClassName,
            IList<string> propertyNames, IList<string> propertyTypeNames)
        {
            NameSpace = nameSpace;
            ClassName = className;
            BaseClassName = baseClassName;
            PropertyNames = propertyNames;
            PropertyTypeNames = propertyTypeNames;
        }

        public string NameSpace { get; }
        public string ClassName { get; }
        public string BaseClassName { get; }
        public IList<string> PropertyNames { get; }
        public IList<string> PropertyTypeNames { get; }

        public bool Equals(ClassToGenerate? other)
        {
            return other is not null &&
                NameSpace == other.NameSpace &&
                ClassName == other.ClassName &&
                BaseClassName == other.BaseClassName &&
                PropertyNames.SequenceEqual(other.PropertyNames) &&
                PropertyTypeNames.SequenceEqual(other.PropertyTypeNames);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ClassToGenerate);
        }

        public override int GetHashCode()
        {
            int hashCode = -1873490020;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NameSpace);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ClassName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BaseClassName);
            return hashCode;
        }
    }
}
