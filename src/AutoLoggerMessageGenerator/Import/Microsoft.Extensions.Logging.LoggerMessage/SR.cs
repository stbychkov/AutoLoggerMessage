// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Resources;

namespace System
{
    internal static partial class SR
    {
        private static readonly bool s_usingResourceKeys = GetUsingResourceKeysSwitchValue();

        // This method is a target of ILLink substitution.
        private static bool GetUsingResourceKeysSwitchValue() => AppContext.TryGetSwitch("System.Resources.UseSystemResourceKeys", out bool usingResourceKeys) ? usingResourceKeys : false;

        // This method is used to decide if we need to append the exception message parameters to the message when calling SR.Format.
        // by default it returns the value of System.Resources.UseSystemResourceKeys AppContext switch or false if not specified.
        // Native code generators can replace the value this returns based on user input at the time of native code generation.
        // The trimming tools are also capable of replacing the value of this method when the application is being trimmed.
        internal static bool UsingResourceKeys() => s_usingResourceKeys;

        // We can optimize out the resource string blob if we can see all accesses to it happening
        // through the generated SR.XXX properties.
        // If a call to GetResourceString is left, the optimization gets defeated and we need to keep
        // the whole resource blob. It's important to keep this private. CoreCLR's CoreLib gets a free
        // pass because the VM needs to be able to call into this, but that's a known set of constants.
#if CORECLR || LEGACY_GETRESOURCESTRING_USER
        internal
#else
        private
#endif
        static string GetResourceString(string resourceKey)
        {
            if (UsingResourceKeys())
            {
                return resourceKey;
            }

            string? resourceString = null;
            try
            {
                resourceString =
#if SYSTEM_PRIVATE_CORELIB || NATIVEAOT
                    InternalGetResourceString(resourceKey);
#else
                    ResourceManager.GetString(resourceKey);
#endif
            }
            catch (MissingManifestResourceException) { }

            return resourceString!; // only null if missing resources
        }

#if LEGACY_GETRESOURCESTRING_USER
        internal
#else
        private
#endif
        static string GetResourceString(string resourceKey, string defaultString)
        {
            string resourceString = GetResourceString(resourceKey);

            return resourceKey == resourceString || resourceString == null ? defaultString : resourceString;
        }

        internal static string Format(string resourceFormat, object? p1)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1);
            }

            return string.Format(resourceFormat, p1);
        }

        internal static string Format(string resourceFormat, object? p1, object? p2)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2);
            }

            return string.Format(resourceFormat, p1, p2);
        }

        internal static string Format(string resourceFormat, object? p1, object? p2, object? p3)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2, p3);
            }

            return string.Format(resourceFormat, p1, p2, p3);
        }

        internal static string Format(string resourceFormat, params object?[]? args)
        {
            if (args != null)
            {
                if (UsingResourceKeys())
                {
                    return resourceFormat + ", " + string.Join(", ", args);
                }

                return string.Format(resourceFormat, args);
            }

            return resourceFormat;
        }

        internal static string Format(IFormatProvider? provider, string resourceFormat, object? p1)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1);
            }

            return string.Format(provider, resourceFormat, p1);
        }

        internal static string Format(IFormatProvider? provider, string resourceFormat, object? p1, object? p2)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2);
            }

            return string.Format(provider, resourceFormat, p1, p2);
        }

        internal static string Format(IFormatProvider? provider, string resourceFormat, object? p1, object? p2, object? p3)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2, p3);
            }

            return string.Format(provider, resourceFormat, p1, p2, p3);
        }

        internal static string Format(IFormatProvider? provider, string resourceFormat, params object?[]? args)
        {
            if (args != null)
            {
                if (UsingResourceKeys())
                {
                    return resourceFormat + ", " + string.Join(", ", args);
                }

                return string.Format(provider, resourceFormat, args);
            }

            return resourceFormat;
        }
    }
}

namespace FxResources.Microsoft.Extensions.Logging.Generators
{
    internal static class SR { }
}
namespace System
{
    internal static partial class SR
    {
        private static global::System.Resources.ResourceManager s_resourceManager;
        internal static global::System.Resources.ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new global::System.Resources.ResourceManager(typeof(FxResources.Microsoft.Extensions.Logging.Generators.SR)));

        /// <summary>Logging method names cannot start with _</summary>
        internal static string @InvalidLoggingMethodNameMessage => GetResourceString("InvalidLoggingMethodNameMessage", @"Logging method names cannot start with _");
        /// <summary>Logging method parameter names cannot start with _</summary>
        internal static string @InvalidLoggingMethodParameterNameMessage => GetResourceString("InvalidLoggingMethodParameterNameMessage", @"Logging method parameter names cannot start with _");
        /// <summary>Could not find a required type definition</summary>
        internal static string @MissingRequiredTypeTitle => GetResourceString("MissingRequiredTypeTitle", @"Could not find a required type definition");
        /// <summary>Could not find definition for type {0}</summary>
        internal static string @MissingRequiredTypeMessage => GetResourceString("MissingRequiredTypeMessage", @"Could not find definition for type {0}");
        /// <summary>Multiple logging methods cannot use the same event id within a class</summary>
        internal static string @ShouldntReuseEventIdsTitle => GetResourceString("ShouldntReuseEventIdsTitle", @"Multiple logging methods cannot use the same event id within a class");
        /// <summary>Multiple logging methods are using event id {0} in class {1}</summary>
        internal static string @ShouldntReuseEventIdsMessage => GetResourceString("ShouldntReuseEventIdsMessage", @"Multiple logging methods are using event id {0} in class {1}");
        /// <summary>Logging methods must return void</summary>
        internal static string @LoggingMethodMustReturnVoidMessage => GetResourceString("LoggingMethodMustReturnVoidMessage", @"Logging methods must return void");
        /// <summary>One of the arguments to a static logging method must implement the Microsoft.Extensions.Logging.ILogger interface</summary>
        internal static string @MissingLoggerArgumentTitle => GetResourceString("MissingLoggerArgumentTitle", @"One of the arguments to a static logging method must implement the Microsoft.Extensions.Logging.ILogger interface");
        /// <summary>One of the arguments to the static logging method '{0}' must implement the Microsoft.Extensions.Logging.ILogger interface</summary>
        internal static string @MissingLoggerArgumentMessage => GetResourceString("MissingLoggerArgumentMessage", @"One of the arguments to the static logging method '{0}' must implement the Microsoft.Extensions.Logging.ILogger interface");
        /// <summary>Logging methods must be static</summary>
        internal static string @LoggingMethodShouldBeStaticMessage => GetResourceString("LoggingMethodShouldBeStaticMessage", @"Logging methods must be static");
        /// <summary>Logging methods must be partial</summary>
        internal static string @LoggingMethodMustBePartialMessage => GetResourceString("LoggingMethodMustBePartialMessage", @"Logging methods must be partial");
        /// <summary>Logging methods cannot be generic</summary>
        internal static string @LoggingMethodIsGenericMessage => GetResourceString("LoggingMethodIsGenericMessage", @"Logging methods cannot be generic");
        /// <summary>Don't include a template for {0} in the logging message since it is implicitly taken care of</summary>
        internal static string @ShouldntMentionInTemplateMessage => GetResourceString("ShouldntMentionInTemplateMessage", @"Don't include a template for {0} in the logging message since it is implicitly taken care of");
        /// <summary>Don't include exception parameters as templates in the logging message</summary>
        internal static string @ShouldntMentionExceptionInMessageTitle => GetResourceString("ShouldntMentionExceptionInMessageTitle", @"Don't include exception parameters as templates in the logging message");
        /// <summary>Remove redundant qualifier (Info:, Warning:, Error:, etc) from the logging message since it is implicit in the specified log level.</summary>
        internal static string @RedundantQualifierInMessageMessage => GetResourceString("RedundantQualifierInMessageMessage", @"Remove redundant qualifier (Info:, Warning:, Error:, etc) from the logging message since it is implicit in the specified log level.");
        /// <summary>Redundant qualifier in logging message</summary>
        internal static string @RedundantQualifierInMessageTitle => GetResourceString("RedundantQualifierInMessageTitle", @"Redundant qualifier in logging message");
        /// <summary>Argument '{0}' is not referenced from the logging message</summary>
        internal static string @ArgumentHasNoCorrespondingTemplateMessage => GetResourceString("ArgumentHasNoCorrespondingTemplateMessage", @"Argument '{0}' is not referenced from the logging message");
        /// <summary>Argument is not referenced from the logging message</summary>
        internal static string @ArgumentHasNoCorrespondingTemplateTitle => GetResourceString("ArgumentHasNoCorrespondingTemplateTitle", @"Argument is not referenced from the logging message");
        /// <summary>Template '{0}' is not provided as argument to the logging method</summary>
        internal static string @TemplateHasNoCorrespondingArgumentMessage => GetResourceString("TemplateHasNoCorrespondingArgumentMessage", @"Template '{0}' is not provided as argument to the logging method");
        /// <summary>Logging template has no corresponding method argument</summary>
        internal static string @TemplateHasNoCorrespondingArgumentTitle => GetResourceString("TemplateHasNoCorrespondingArgumentTitle", @"Logging template has no corresponding method argument");
        /// <summary>Logging methods cannot have a body</summary>
        internal static string @LoggingMethodHasBodyMessage => GetResourceString("LoggingMethodHasBodyMessage", @"Logging methods cannot have a body");
        /// <summary>A LogLevel value must be supplied in the LoggerMessage attribute or as a parameter to the logging method</summary>
        internal static string @MissingLogLevelMessage => GetResourceString("MissingLogLevelMessage", @"A LogLevel value must be supplied in the LoggerMessage attribute or as a parameter to the logging method");
        /// <summary>Don't include log level parameters as templates in the logging message</summary>
        internal static string @ShouldntMentionLogLevelInMessageTitle => GetResourceString("ShouldntMentionLogLevelInMessageTitle", @"Don't include log level parameters as templates in the logging message");
        /// <summary>Don't include logger parameters as templates in the logging message</summary>
        internal static string @ShouldntMentionLoggerInMessageTitle => GetResourceString("ShouldntMentionLoggerInMessageTitle", @"Don't include logger parameters as templates in the logging message");
        /// <summary>Couldn't find a field of type Microsoft.Extensions.Logging.ILogger in class {0}</summary>
        internal static string @MissingLoggerFieldMessage => GetResourceString("MissingLoggerFieldMessage", @"Couldn't find a field of type Microsoft.Extensions.Logging.ILogger in class {0}");
        /// <summary>Couldn't find a field of type Microsoft.Extensions.Logging.ILogger</summary>
        internal static string @MissingLoggerFieldTitle => GetResourceString("MissingLoggerFieldTitle", @"Couldn't find a field of type Microsoft.Extensions.Logging.ILogger");
        /// <summary>Found multiple fields of type Microsoft.Extensions.Logging.ILogger in class {0}</summary>
        internal static string @MultipleLoggerFieldsMessage => GetResourceString("MultipleLoggerFieldsMessage", @"Found multiple fields of type Microsoft.Extensions.Logging.ILogger in class {0}");
        /// <summary>Found multiple fields of type Microsoft.Extensions.Logging.ILogger</summary>
        internal static string @MultipleLoggerFieldsTitle => GetResourceString("MultipleLoggerFieldsTitle", @"Found multiple fields of type Microsoft.Extensions.Logging.ILogger");
        /// <summary>Can't have the same template with different casing</summary>
        internal static string @InconsistentTemplateCasingMessage => GetResourceString("InconsistentTemplateCasingMessage", @"Can't have the same template with different casing");
        /// <summary>Logging method '{0}' contains malformed format strings</summary>
        internal static string @MalformedFormatStringsMessage => GetResourceString("MalformedFormatStringsMessage", @"Logging method '{0}' contains malformed format strings");
        /// <summary>Generating more than 6 arguments is not supported</summary>
        internal static string @GeneratingForMax6ArgumentsMessage => GetResourceString("GeneratingForMax6ArgumentsMessage", @"Generating more than 6 arguments is not supported");
        /// <summary>Argument '{0}' is using the unsupported out parameter modifier</summary>
        internal static string @InvalidLoggingMethodParameterOutMessage => GetResourceString("InvalidLoggingMethodParameterOutMessage", @"Argument '{0}' is using the unsupported out parameter modifier");
        /// <summary>Argument is using the unsupported out parameter modifier</summary>
        internal static string @InvalidLoggingMethodParameterOutTitle => GetResourceString("InvalidLoggingMethodParameterOutTitle", @"Argument is using the unsupported out parameter modifier");
        /// <summary>Multiple logging methods are using event name {0} in class {1}</summary>
        internal static string @ShouldntReuseEventNamesMessage => GetResourceString("ShouldntReuseEventNamesMessage", @"Multiple logging methods are using event name {0} in class {1}");
        /// <summary>Multiple logging methods should not use the same event name within a class</summary>
        internal static string @ShouldntReuseEventNamesTitle => GetResourceString("ShouldntReuseEventNamesTitle", @"Multiple logging methods should not use the same event name within a class");
        /// <summary>Logging method contains malformed format strings</summary>
        internal static string @MalformedFormatStringsTitle => GetResourceString("MalformedFormatStringsTitle", @"Logging method contains malformed format strings");
        /// <summary>C# language version not supported by the source generator.</summary>
        internal static string @LoggingUnsupportedLanguageVersionTitle => GetResourceString("LoggingUnsupportedLanguageVersionTitle", @"C# language version not supported by the source generator.");
        /// <summary>The Logging source generator is not available in C# {0}. Please use language version {1} or greater.</summary>
        internal static string @LoggingUnsupportedLanguageVersionMessageFormat => GetResourceString("LoggingUnsupportedLanguageVersionMessageFormat", @"The Logging source generator is not available in C# {0}. Please use language version {1} or greater.");
        /// <summary>Class '{0}' has a primary constructor parameter of type Microsoft.Extensions.Logging.ILogger that is hidden by a field in the class or a base class, preventing its use</summary>
        internal static string @PrimaryConstructorParameterLoggerHiddenMessage => GetResourceString("PrimaryConstructorParameterLoggerHiddenMessage", @"Class '{0}' has a primary constructor parameter of type Microsoft.Extensions.Logging.ILogger that is hidden by a field in the class or a base class, preventing its use");
        /// <summary>Primary constructor parameter of type Microsoft.Extensions.Logging.ILogger is hidden by a field</summary>
        internal static string @PrimaryConstructorParameterLoggerHiddenTitle => GetResourceString("PrimaryConstructorParameterLoggerHiddenTitle", @"Primary constructor parameter of type Microsoft.Extensions.Logging.ILogger is hidden by a field");

    }
}
