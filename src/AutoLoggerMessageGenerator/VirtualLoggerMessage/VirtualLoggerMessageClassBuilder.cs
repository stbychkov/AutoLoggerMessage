using System.Collections.Immutable;
using System.Linq;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging.Generators;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoLoggerMessageGenerator.VirtualLoggerMessage;

internal class VirtualLoggerMessageClassBuilder(bool useTelemetryExtensions = false)
{
    private const string LoggerMessageAttributeName = LoggerMessageGenerator.Parser.LoggerMessageAttribute;
    private const string LogPropertiesAttribute = "Microsoft.Extensions.Logging.LogPropertiesAttribute";

    public MemberDeclarationSyntax Build(ImmutableArray<LogCall> logCalls)
    {
        var methods = logCalls.Select(GenerateMethod).OfType<MemberDeclarationSyntax>().ToArray();

        var classDeclaration = ClassDeclaration(Constants.LoggerClassName)
            .AddModifiers(Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword))
            .AddMembers(methods);

        var namespaceDeclaration = NamespaceDeclaration(IdentifierName(Constants.GeneratorNamespace))
            .AddMembers(classDeclaration);

        return namespaceDeclaration;
    }

    private MethodDeclarationSyntax GenerateMethod(LogCall logCall)
    {
        var attributeList = GenerateLoggerMessageAttribute(logCall);

        var loggerParameter = Parameter(Identifier("Logger"))
            .WithType(IdentifierName($"{Constants.DefaultLoggingNamespace}.ILogger"));

        var parameters = logCall.Parameters.Select(c =>
        {
            var parameter = Parameter(Identifier(c.Name)).WithType(IdentifierName(c.Type));

            if (useTelemetryExtensions && c.HasPropertiesToLog)
                parameter = parameter.AddAttributeLists(GenerateLogPropertiesAttribute());

            return parameter;
        }).ToArray();

        var methodName = IdentifierHelper.ToValidCSharpMethodName($"{Constants.LogMethodPrefix}{logCall.Namespace}{logCall.ClassName}_{logCall.Location.Line}_{logCall.Location.Character}");

        var logCallMappingLocation = ParseLeadingTrivia(LogCallLocationMap.CreateMapping(logCall));
        var methodDeclaration = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), methodName)
            .AddModifiers(
                Token(SyntaxKind.InternalKeyword),
                Token(SyntaxKind.StaticKeyword),
                Token(SyntaxKind.PartialKeyword)
            )
            .AddAttributeLists(attributeList)
            .AddParameterListParameters(loggerParameter)
            .AddParameterListParameters(parameters)
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            .WithLeadingTrivia(logCallMappingLocation);

        return methodDeclaration;
    }

    private static AttributeListSyntax GenerateLoggerMessageAttribute(LogCall logCall)
    {
        var attribute = Attribute(ParseName(LoggerMessageAttributeName))
            .WithArgumentList(
                AttributeArgumentList(
                    SeparatedList(new[]
                    {
                        AttributeArgument(
                            ParseExpression($"Level = {Constants.DefaultLoggingNamespace}.LogLevel.{logCall.LogLevel}")),
                        AttributeArgument(
                            ParseExpression($"Message = \"{logCall.Message}\""))
                    })
                )
            );

        var attributeList = AttributeList(SingletonSeparatedList(attribute));
        return attributeList;
    }

    private static AttributeListSyntax GenerateLogPropertiesAttribute()
    {
        // TODO: Add configuration
        var attribute = Attribute(ParseName(LogPropertiesAttribute))
            .WithArgumentList(
                AttributeArgumentList(
                    SeparatedList(new[]
                    {
                        AttributeArgument(
                            ParseExpression($"OmitReferenceName = true")),
                        AttributeArgument(
                            ParseExpression($"SkipNullProperties = true"))
                    })
                )
            );

        var attributeList = AttributeList(SingletonSeparatedList(attribute));
        return attributeList;
    }
}
