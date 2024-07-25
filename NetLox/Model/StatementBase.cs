using NetLox.AstClasses.Generators;

namespace NetLox.Model
{
    [GenerateAstClasses]
    public abstract class StatementBase
    {
        public const string AstClassesDef =
            $$"""
            [
                {
                    "ClassName" : "BlockStmt",
                    "Properties" : {
                        "IList<StatementBase>": "Statements"
                    }
                },
                {
                    "ClassName" : "ExpressionStmt",
                    "Properties" : {
                        "ExpressionBase": "Expr"
                    }
                },
                {
                    "ClassName" : "IfStmt",
                    "Properties" : {
                        "ExpressionBase": "Condition",
                        "StatementBase": ["ThenBranch", "ElseBranch"]
                    }
                },
                {
                    "ClassName": "PrintStmt",
                    "Properties": {
                        "ExpressionBase": "Expr"
                    }
                },
                {
                    "ClassName": "VarStmt",
                    "Properties": {
                        "Token": "Name",
                        "ExpressionBase": "Initializer"
                    }
                },
                {
                    "ClassName": "WhileStmt",
                    "Properties": {
                        "StatementBase": "Body",
                        "ExpressionBase": "Condition"
                    }
                }
            ]
            """;

        public abstract R Accept<R>(AstClassesVisitor<R> visitor);
    }
}
