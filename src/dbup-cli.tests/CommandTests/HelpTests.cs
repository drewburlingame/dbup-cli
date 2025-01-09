using CommandDotNet.Execution;
using CommandDotNet.TestTools;

namespace DbUp.Cli.Tests.CommandTests;

public class HelpTests
{
    public HelpTests(ITestOutputHelper output) => Ambient.Output = output;

    private readonly TestHost host = new();

    public static TheoryData<string> GetCommands => new TheoryData<string>(
        new TestHost().AppRunner
            .GetFromContext(["-h"], ctx => ctx.RootCommand, middlewareStage: MiddlewareStages.ParseInput)!
            .Subcommands
            .Select(c => c.Name));

    [Fact]
    public async Task VerifyAppHelp() =>
        await Verify(host.Run("-h").Console.AllText());
    
    [Theory]
    [MemberData(nameof(GetCommands))]
    public async Task VerifyCommandHelp(string commandName) =>
        await Verify(host.Run(commandName, "-h").Console.AllText());
    
    [Fact]
    public async Task VerifyVersion() => 
        await Verify(host.Run("--version").Console.AllText());
}