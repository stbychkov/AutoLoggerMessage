using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging.Generators;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoLoggerMessageGenerator.VirtualLoggerMessage;

internal class VirtualLoggerMessageClassBuilder(
    SourceGeneratorConfiguration configuration,
    bool useTelemetryExtensions = false)
{
    private const string LoggerMessageAttributeName = LoggerMessageGenerator.Parser.LoggerMessageAttribute;
    private const string LogPropertiesAttribute = "Microsoft.Extensions.Logging.LogPropertiesAttribute";

    public MemberDeclarationSyntax Build(ImmutableArray<LogMessageCall> logCalls)
    {
        var methods = logCalls.Select(GenerateMethod).OfType<MemberDeclarationSyntax>().ToArray();

        var classDeclaration = ClassDeclaration(Constants.LoggerClassName)
            .AddModifiers(Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword))
            .AddMembers(methods);

        var namespaceDeclaration = NamespaceDeclaration(IdentifierName(Constants.GeneratorNamespace))
            .AddMembers(classDeclaration);

        return namespaceDeclaration;
    }

    private MethodDeclarationSyntax GenerateMethod(LogMessageCall logMessageCall)
    {
        var attributeList = GenerateLoggerMessageAttribute(logMessageCall);

        var loggerParameter = Parameter(Identifier("Logger"))
            .WithType(IdentifierName($"{Constants.DefaultLoggingNamespace}.ILogger"));

        var parameters = logMessageCall.Parameters
            .Where(c => !Constants.LoggerMessageAttributeParameterTypes.Contains(c.Type))
            .Select(c =>
        {
            var parameter = Parameter(Identifier(c.Name)).WithType(IdentifierName(c.NativeType));

            if (useTelemetryExtensions && c.HasPropertiesToLog)
                parameter = parameter.AddAttributeLists(GenerateLogPropertiesAttribute());

            return parameter;
        }).ToArray();

        var logCallMappingLocation = ParseLeadingTrivia(LogMessageCallLocationMap.CreateMapping(logMessageCall));
        var methodDeclaration = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), logMessageCall.GeneratedMethodName)
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

    private AttributeListSyntax GenerateLoggerMessageAttribute(LogMessageCall logMessageCall)
    {
        var attribute = Attribute(ParseName(LoggerMessageAttributeName))
            .WithArgumentList(
                AttributeArgumentList(
                    SeparatedList(new[]
                    {
                        AttributeArgument(
                            ParseExpression(
                                $"Level = {Constants.DefaultLoggingNamespace}.LogLevel.{logMessageCall.LogLevel}")),
                        AttributeArgument(
                            AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("Message"),
                                LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(logMessageCall.Message)))),
                        AttributeArgument(
                            ParseExpression(
                                $"SkipEnabledCheck = {configuration.GenerateSkipEnabledCheck.ToLowerBooleanString()}"))
                    })
                )
            );

        var attributeList = AttributeList(SingletonSeparatedList(attribute));
        return attributeList;
    }

    private AttributeListSyntax GenerateLogPropertiesAttribute()
    {
        var attributeArguments = SeparatedList(new[]
        {
            AttributeArgument(
                ParseExpression(
                    $"OmitReferenceName = {configuration.GenerateOmitReferenceName.ToLowerBooleanString()}")),
            AttributeArgument(
                ParseExpression(
                    $"SkipNullProperties = {configuration.GenerateSkipNullProperties.ToLowerBooleanString()}"))
        });

        if (configuration.GenerateTransitive)
        {
            attributeArguments = attributeArguments.Add(
                AttributeArgument(
                    ParseExpression(
                        $"Transitive = {configuration.GenerateTransitive.ToLowerBooleanString()}"))
            );
        }

        var attribute = Attribute(ParseName(LogPropertiesAttribute))
            .WithArgumentList(
                AttributeArgumentList(
                    SeparatedList(attributeArguments)
                )
            );

        var attributeList = AttributeList(SingletonSeparatedList(attribute));
        return attributeList;
    }
}
