using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static ReactiveBinderGenerators.Consts;

namespace ReactiveBinderGenerators
{
    [Generator]
    public class ReactiveBinderGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("BindFrom", SourceText.From(MarkerAttributeCode, Encoding.UTF8));

            if (context.SyntaxReceiver is not SyntaxReceiver r)
            {
                return;
            }

            var options = (context.Compilation as CSharpCompilation)?.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions;
            var compilation = context
                .Compilation
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(MarkerAttributeCode, Encoding.UTF8), options));

            var bindFromAttribute = compilation.GetTypeByMetadataName(MarkerAttributeFullName) ?? throw new InvalidOperationException();
            var classes = r.CandidateClasses.Select(x =>
                {
                    var model = compilation.GetSemanticModel(x.SyntaxTree);
                    if (model.GetDeclaredSymbol(x) is INamedTypeSymbol typeSymbol)
                    {
                        return (syntax: x, symbol: typeSymbol);
                    }

                    throw new InvalidOperationException(@$"{model.GetDeclaredSymbol(x)}");
                })
                .Where(x => x.symbol.GetAttributes().Any(x => x.AttributeClass?.Equals(bindFromAttribute, SymbolEqualityComparer.Default) ?? false));

            var s = new StringBuilder();
            foreach (var c in classes)
            {
                processClass(s, c, bindFromAttribute, compilation);
            }

            context.AddSource("Binder", SourceText.From(s.ToString(), Encoding.UTF8));


            static void processClass(StringBuilder s,
                (ClassDeclarationSyntax syntax, ITypeSymbol symbol) clazz,
                INamedTypeSymbol bindFromAttribute,
                Compilation compilation)
            {
                var (syntax, symbol) = clazz;
                var generateFrom = symbol
                    .GetAttributes()
                    .Single(x => x.AttributeClass?.Equals(bindFromAttribute, SymbolEqualityComparer.Default) ?? false);
                var typeofExpression = generateFrom.ConstructorArguments.First();
                if (typeofExpression.Value is null)
                {
                    throw new InvalidOperationException();
                }

                var fromType = compilation.GetTypeByMetadataName(typeofExpression.Value.ToString()) ?? throw new InvalidOperationException();
                var properties = fromType.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(x => !x.IsStatic && !x.IsIndexer && !x.IsReadOnly);

                s.AppendLine($@"
using Reactive.Bindings.Extensions;
namespace {symbol.ContainingNamespace.ToDisplayString()}
{{
    public partial class {symbol.Name} : System.IDisposable
    {{
        private System.Reactive.Disposables.CompositeDisposable Disposables {{ get; }} = new System.Reactive.Disposables.CompositeDisposable();");

                foreach (var prop in properties)
                {
                    s.AppendLine($@"public {MakeReactivePropertyOf(prop.Type.ToDisplayString())} {prop.Name} {{ get; }}");
                }

                s.AppendLine($@"
        public {symbol.Name}({fromType.ToDisplayString()} model)
        {{");
                foreach (var prop in properties)
                {
                    s.AppendLine($@"this.{prop.Name} = Reactive.Bindings.ReactiveProperty.FromObject(model, x => x.{prop.Name})
                        .AddTo(Disposables);");
                }

                s.AppendLine($@"
        }}
        public void Dispose() => Disposables.Dispose();
    }}
}}");
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax c && c.AttributeLists.SelectMany(x => x.Attributes).Any())
                {
                    CandidateClasses.Add(c);
                }
            }
        }

    }

}
