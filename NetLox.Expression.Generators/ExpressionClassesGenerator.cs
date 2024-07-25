using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetLox.Expression.Generators.Model;
using Newtonsoft.Json;
using System.Text;

namespace NetLox.Expression.Generators
{
    [Generator]
    public class ExpressionClassesGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classes = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => IsSyntaxTarget(node),
            transform: static (ctx, _) => GetSemanticTarget(ctx))
            .Where(static target => target is not null);

            context.RegisterSourceOutput(classes, Execute);
            context.RegisterPostInitializationOutput(static (ctx) => PostInitializationOutput(ctx));
        }

        private void Execute(SourceProductionContext context, ExpressionsToGenerate? source)
        {
            if (source is null) return;
            var namespaceName = source.ExpressionClasses.First().NameSpace;
            var baseClassName = source.ExpressionClasses.First().BaseClassName; ;
            var classFileName = $"{namespaceName}.{baseClassName}.g.cs";
            var visitorFileName = $"{namespaceName}.{baseClassName}.visitor.g.cs";
            var expressionClassesStringBuilder = new StringBuilder();
            var visitorStringBuilder = new StringBuilder();
            expressionClassesStringBuilder.Append($@"using NetLox.Scanner;
namespace {namespaceName};
");
            visitorStringBuilder.Append($@"namespace {namespaceName};

public partial interface AstClassesVisitor<R> {{
");
            foreach (var expression in source.ExpressionClasses)
            {
                expressionClassesStringBuilder.Append(DefineClass(expression));
                visitorStringBuilder.AppendLine($@"    R Visit({expression.ClassName} {expression.ClassName.ToLower()});");
            }
            visitorStringBuilder.Append("}");
            context.AddSource(classFileName, expressionClassesStringBuilder.ToString());
            context.AddSource(visitorFileName, visitorStringBuilder.ToString());
        }

        private string DefineClass(ClassToGenerate expression)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($@"
public class {expression.ClassName} : {expression.BaseClassName}
{{");
            for (int i = 0; i < expression.PropertyNames.Count; ++i)
            {
                stringBuilder.Append($@"
    public {expression.PropertyTypeNames[i]} {expression.PropertyNames[i]} {{ get; set; }}");
            }
            stringBuilder.Append($@"

    public override R Accept<R>(AstClassesVisitor<R> visitor)
    {{
        return visitor.Visit(this);
    }}
}}");
            return stringBuilder.ToString();
        }

        private static ExpressionsToGenerate? GetSemanticTarget(GeneratorSyntaxContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            var attibuteSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("NetLox.AstClasses.Generators.GenerateAstClassesAttribute");
            var result = new ExpressionsToGenerate
            {
                ExpressionClasses = new List<ClassToGenerate>()
            };

            if (classSymbol is not null && attibuteSymbol is not null)
            {
                foreach (var attributeData in classSymbol.GetAttributes())
                {
                    if (attibuteSymbol.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
                    {
                        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                        var baseClassName = classSymbol.Name;
                        IFieldSymbol memberSymbol = classSymbol.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(f => f.Name == "AstClassesDef");
                        if (memberSymbol is not null)
                        {
                            var jsonText = memberSymbol.ConstantValue!.ToString();
                            var expressionShapes = JsonConvert.DeserializeObject<IList<ExpressionShape>>(jsonText);
                            if (expressionShapes is null) return null;
                            foreach (var expressionShape in expressionShapes)
                            {
                                var propertyNames = new List<string>();
                                var propertyTypeNames = new List<string>();
                                foreach (var property in expressionShape.Properties)
                                {
                                    if(property.Value is string propertyValueString)
                                    {
                                        propertyNames.Add(propertyValueString);
                                        propertyTypeNames.Add(property.Key);
                                    }
                                    else
                                    {
                                        foreach (var propertyValueStringEntry in 
                                            ((Newtonsoft.Json.Linq.JArray)property.Value).ToObject<List<string>>())
                                        {
                                            propertyNames.Add(propertyValueStringEntry);
                                            propertyTypeNames.Add(property.Key);
                                        }
                                    }
                                }
                                result.ExpressionClasses.Add(new ClassToGenerate(namespaceName, expressionShape.ClassName,
                                    baseClassName, propertyNames, propertyTypeNames));
                            }
                        }
                    }
                }
            }
            return result.ExpressionClasses.Count > 0 ? result : null;
        }

        private static bool IsSyntaxTarget(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDeclarationSyntax
                && classDeclarationSyntax.AttributeLists.Count > 0;
        }

        private static void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource("NetLox.AstClasses.Generators.GenerateAstClassesAttribute.g.cs",
                @"namespace NetLox.AstClasses.Generators
{
    internal class GenerateAstClassesAttribute : System.Attribute {}
}");
        }
    }
}
