using NetLox.AstClasses.Generators;
namespace NetLox.Model
{
    [GenerateAstClasses]
    public abstract class ExpressionBase
    {
        public const string AstClassesDef =
            $$"""
            [
                {
                    "ClassName" : "Binary",
                    "Properties" : {
                        "ExpressionBase": ["Left", "Right"],
                        "Token": "Operator",
                    }
                },
                {
                    "ClassName" : "Logical",
                    "Properties" : {
                        "ExpressionBase": ["Left", "Right"],
                        "Token": "Operator",
                    }
                },
                {
                    "ClassName": "Grouping",
                    "Properties": {
                        "ExpressionBase": "ExpressionIn"
                    }
                },
                {
                    "ClassName": "Literal",
                    "Properties" : {
                        "Object": "Value"
                    }
                },
                {
                    "ClassName": "Unary",
                    "Properties": {
                        "Token": "Operator",
                        "ExpressionBase": "Right"
                    }
                },
                {
                    "ClassName": "Variable",
                    "Properties": {
                        "Token": "Name"
                    }
                },
                {
                    "ClassName": "Assign",
                    "Properties": {
                        "Token": "Name",
                        "ExpressionBase": "Value"
                    }
                }
            ]
            """;

        public abstract R Accept<R>(AstClassesVisitor<R> visitor);
    }
}
