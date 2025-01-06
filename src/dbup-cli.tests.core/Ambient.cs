namespace DbUp.Cli.Tests;

public static class Ambient
{
    private static readonly AsyncLocal<ITestOutputHelper> TestOutputHelper = new AsyncLocal<ITestOutputHelper>();

    public static ITestOutputHelper Output
    {
        get => TestOutputHelper.Value;
        set => TestOutputHelper.Value = value;
    }

    public static Action<string> WriteLine
    {
        get
        {
            var output = Output;
            if (output == null)
            {
                throw new InvalidOperationException($"{nameof(Ambient)}.{nameof(Output)} has not been set for the current test");
            }

            return output.WriteLine;
        }
    }
}