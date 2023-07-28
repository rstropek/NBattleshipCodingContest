namespace NBattleshipCodingContest.PlayersGenerator.Tests
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using Xunit;

    public class PlayersGeneratorTest
    {
        [Fact]
        public void InitializeImpl()
        {
            var generator = new PlayersGenerator();

            SyntaxReceiverCreator? receiver = null;
            PlayersGenerator.InitializeImpl((r) => receiver = r);

            // Make sure that syntax receiver is registered during initialization
            Assert.NotNull(receiver);

            // Note null forgiving operator. Read more at
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-forgiving

            Assert.IsAssignableFrom<PlayersGenerator.SyntaxReceiver>(receiver!());
        }

        private const string Code = @"
namespace NBattleshipCodingContest.Players
{
    using System;

    public abstract class PlayerBase { }

    public class IgnoreAttribute : Attribute { }

    public abstract class PlayerBase2 { }

    public class MyPlayer : PlayerBase { }

    [IgnoreAttribute]
    public class MyPlayer2 : PlayerBase { }

    public class MyNonPlayer : PlayerBase2 { }
}
";

        /// <summary>
        /// Compile given code and return syntax tree and compilation.
        /// </summary>
        private static (SyntaxTree, Compilation) Compile(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("TestPlayerGenerator")
                .AddReferences(new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) })
                .AddSyntaxTrees(tree);
            return (tree, compilation);
        }

        /// <summary>
        /// Helper class that wraps a <see cref="PlayersGenerator.SyntaxReceiver"/> into a <see cref="CSharpSyntaxWalker"/>
        /// </summary>
        private class ClassCollector : CSharpSyntaxWalker
        {
            private readonly PlayersGenerator.SyntaxReceiver receiver;

            public ClassCollector(PlayersGenerator.SyntaxReceiver receiver) => this.receiver = receiver;

            public override void VisitClassDeclaration(ClassDeclarationSyntax classDeclarationSyntax) => 
                receiver.OnVisitSyntaxNode(classDeclarationSyntax);
        }

        /// <summary>
        /// Helper method that runs the receiver over a given syntax tree.
        /// </summary>
        private static PlayersGenerator.SyntaxReceiver FillReceiver(SyntaxTree tree)
        {
            var receiver = new PlayersGenerator.SyntaxReceiver();

            // Use a syntax walker to fill receiver
            var collector = new ClassCollector(receiver);
            collector.Visit(tree.GetRoot());

            return receiver;
        }

        [Fact]
        public void SyntaxReceiver()
        {
            var (tree, _) = Compile(Code);
            var receiver = FillReceiver(tree);

            // Check that syntax receiver recognized candidate classes (i.e. classes
            // with at least one base class).
            Assert.Equal(4, receiver.CandidateClasses.Count);
        }

        [Fact]
        public void Generate()
        {
            var (tree, compilation) = Compile(Code);
            var receiver = FillReceiver(tree);

            var generator = new PlayersGenerator();
            SourceText? sourceText = null;
            PlayersGenerator.ExecuteImpl(receiver.CandidateClasses, compilation, st => sourceText = st);

            // Make sure that source text contains the test player
            Assert.NotNull(sourceText);
            Assert.Contains("MyPlayer", sourceText!.ToString());

            // Note that in practice, you would probably use Roslyn to compile the generated
            // code and check its syntax tree. We do not do that to keep the sample simple.
        }
    }
}
