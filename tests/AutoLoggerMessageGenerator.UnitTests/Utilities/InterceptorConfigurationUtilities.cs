namespace AutoLoggerMessageGenerator.UnitTests.Utilities;

public static class InterceptorConfigurationUtilities
{
    public static string GetInterceptorConfiguration()
    {
        #if PATH_BASED_INTERCEPTORS
            return "PathBasedInterceptor";
        #elif HASH_BASED_INTERCEPTORS
            return "HashBasedInterceptor";
        #else
            throw new NotSupportedException("Unknown interceptors configuration");
        #endif
    }
}
