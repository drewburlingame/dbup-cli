using CommandDotNet;
using CommandDotNet.NameCasing;
using DbUp.Cli.Cmd;
using DbUp.Cli.DbUpCustomization;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace DbUp.Cli;

public static class Program
{
    private static int Main(string[] args) =>
        NewAppRunner(new CliEnvironment(), c => new ConsoleLogger(c)).Run(args);

    public static AppRunner NewAppRunner(IEnvironment environment,
        Func<IConsole, IUpgradeLog> getLogger,
        IConnectionFactory? connectionFactory = null) =>
        new AppRunner<Commands>()
            .UseDefaultMiddleware(
                excludeDebugDirective: true // security risk
            )
            .UseNameCasing(Case.KebabCase)
            .Configure(b =>
            {
                b.UseParameterResolver(_ => environment);
                b.UseParameterResolver(c => c.Services.GetOrAdd(() => getLogger(c.Console)));
                b.UseParameterResolver(_ => connectionFactory);
            })
            .UseErrorHandler((ctx, ex) =>
            {
                // TODO: log via structured logging.
                ctx!.Console.WriteLine(ex.FlattenErrorMessages());
                return ExitCodes.Error;
            });
}