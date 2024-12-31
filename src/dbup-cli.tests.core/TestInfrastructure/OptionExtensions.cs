using Optional;

namespace DbUp.Cli.Tests.TestInfrastructure;

public static class OptionExtensions
{
    public static string GetErrorOrNull<T>(this Option<T, Error> option)
    {
        string error = null;
        option.MatchNone(err => error = err.Message);
        return error;
    }
    
    public static string GetErrorOrThrow<T>(this Option<T, Error> option) => 
        option.GetErrorOrNull() ?? throw new Exception("option is missing error");

    public static T GetValueOrNull<T>(this Option<T, Error> option)
    {
        T value = default;
        option.MatchSome(v => value = v);
        return value;
    }

    public static T GetValueOrThrow<T>(this Option<T, Error> option) => 
        option.GetValueOrNull() ?? throw new Exception("option is missing value. error: " + option.GetErrorOrNull());
}