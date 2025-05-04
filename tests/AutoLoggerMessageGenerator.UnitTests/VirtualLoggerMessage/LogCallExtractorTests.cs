using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.VirtualLoggerMessage;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.UnitTests.VirtualLoggerMessage;

internal class LogMessageCallLocationMapTests : BaseSourceGeneratorTest
{
     [Test]
     public async Task MapBack_GivenMethodName_ShouldReturnLastLocationBeforeTheCurrentNode()
     {
         var logCall1 = new LogMessageCall { Id = Guid.NewGuid() };
         var logCall2 = new LogMessageCall { Id = Guid.NewGuid() };
         var logCall3 = new LogMessageCall { Id = Guid.NewGuid() };

         var additionalDeclarations = $$"""
                           public void Method0(){}

                           {{LogMessageCallLocationMap.CreateMapping(logCall1)}}
                           public void Method1(){}

                           {{LogMessageCallLocationMap.CreateMapping(logCall2)}}
                           public void Method2(){}

                           {{LogMessageCallLocationMap.CreateMapping(logCall3)}}
                           public void Method3(){}
                           """;
         var (_, syntaxTree) = await CompileSourceCode(string.Empty, additionalDeclarations);

         var methodDeclarations = syntaxTree.GetRoot().DescendantNodes()
             .OfType<MethodDeclarationSyntax>()
             .ToArray();

         var method0Declaration = methodDeclarations.Single(m => m.Identifier.Text == "Method0");
         var method1Declaration = methodDeclarations.Single(m => m.Identifier.Text == "Method1");
         var method2Declaration = methodDeclarations.Single(m => m.Identifier.Text == "Method2");
         var method3Declaration = methodDeclarations.Single(m => m.Identifier.Text == "Method3");

         await Assert.That(LogMessageCallLocationMap.TryMapBack(syntaxTree.GetText(), method0Declaration.GetLocation(), out var logCallId0))
             .IsFalse();

         await Assert.That(LogMessageCallLocationMap.TryMapBack(syntaxTree.GetText(), method1Declaration.GetLocation(), out var logCallId1))
             .IsTrue();
         await Assert.That(logCallId1).IsEqualTo(logCall1.Id);

         await Assert.That(LogMessageCallLocationMap.TryMapBack(syntaxTree.GetText(), method2Declaration.GetLocation(), out var logCallId2))
             .IsTrue();
         await Assert.That(logCallId2).IsEqualTo(logCall2.Id);

         await Assert.That(LogMessageCallLocationMap.TryMapBack(syntaxTree.GetText(), method3Declaration.GetLocation(), out var logCallId3))
             .IsTrue();
         await Assert.That(logCallId3).IsEqualTo(logCall3.Id);
     }
}
