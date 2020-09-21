namespace NBattleshipCodingContest.PlayersGenerator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Generates a static list of battleship players.
    /// </summary>
    /// <remarks>
    /// This generator makes runtime reflection for identifying battleship player no
    /// longer necessary. Leads to faster startup.
    /// </remarks>
    [Generator]
    public class PlayersGenerator : ISourceGenerator
    {
        /// <summary>
        /// Visitor class that finds possible players.
        /// </summary>
        internal class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // Players must derive from PlayerBase -> candidate classes must
                // have at least one base type.
                if (syntaxNode is ClassDeclarationSyntax cds && cds.BaseList != null && cds.BaseList.Types.Any())
                {
                    CandidateClasses.Add(cds);
                }
            }
        }

        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context) =>
            // We do the work in a helper functions because we want to be able to verify that
            // a syntax receiver is registeres. However, our mocking framework Moq does not support
            // creating mock objects for structs like this (at least I don't know how to do it).
            InitializeImpl(context.RegisterForSyntaxNotifications);

        internal static void InitializeImpl(Action<SyntaxReceiverCreator> registerForSyntaxNotifications) =>
            // Register a syntax receiver that will be created for each generation pass
            registerForSyntaxNotifications(() => new SyntaxReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver || receiver.CandidateClasses == null)
            {
                throw new InvalidOperationException("Wrong syntax receiver or receiver never ran. Should never happen!");
            }

            ExecuteImpl(receiver.CandidateClasses, context.Compilation, st => context.AddSource("PlayersGenerated", st));
        }

        internal static void ExecuteImpl(List<ClassDeclarationSyntax> candidateClasses, Compilation compilation, Action<SourceText> addSource)
        {
            // Begin creating the source we'll inject into the users compilation

            StringBuilder sourceBuilder = new(@"
namespace NBattleshipCodingContest.Players
{
    using System;

    public static class PlayerList
    {
        public static readonly PlayerInfo[] Players = new PlayerInfo[] 
        {
");

            // Mandator base class for players
            var playerBaseSymbol = compilation.GetTypeByMetadataName("NBattleshipCodingContest.Players.PlayerBase");
            if (playerBaseSymbol == null)
            {
                return;
            }

            // Attribute used to ignore specific players (e.g. players that crash or are not finished)
            var playerIgnoreSymbol = compilation.GetTypeByMetadataName("NBattleshipCodingContest.Players.IgnoreAttribute");
            if (playerIgnoreSymbol == null)
            {
                return;
            }

            var playerClasses = new List<INamedTypeSymbol>();
            foreach (var f in candidateClasses)
            {
                var model = compilation.GetSemanticModel(f.SyntaxTree);
                if (model.GetDeclaredSymbol(f) is not INamedTypeSymbol ti)
                {
                    continue;
                }

                // Check whether the class has the correct base class
                if (!ti.BaseType?.Equals(playerBaseSymbol, SymbolEqualityComparer.Default) ?? false)
                {
                    continue;
                }

                // Check whether the class has no ignore attribute
                if (!ti.GetAttributes().Any(a => a.AttributeClass?.Equals(playerIgnoreSymbol, SymbolEqualityComparer.Default) ?? false))
                {
                    // Found a player
                    playerClasses.Add(ti);
                }
            }

            // Add all players to generated code
            sourceBuilder.Append(string.Join(",", playerClasses.Select(c =>
            {
                var displayString = c.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return @$"new PlayerInfo(""{displayString}"", () => new {displayString}())";
            })));

            // finish creating the source to inject
            sourceBuilder.Append(@"
        };
    }
}");

            // inject the created source into the users compilation
            addSource(SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}
