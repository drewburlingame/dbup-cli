using System.ComponentModel.DataAnnotations;
using DbUp.Cli.DbProviders;
using JetBrains.Annotations;

namespace DbUp.Cli.Configuration;

[UsedImplicitly]
public class Migration : IValidatableObject
{
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    [Required]
    public string Version { get; private set; } = null!;
    public string Provider { get; private set; } = null!;
    // ReSharper enable UnusedAutoPropertyAccessor.Local
    [Required]
    public string ConnectionString { get; private set; } = null!;
    public int ConnectionTimeoutSec { get; private set; } = 30;
    public bool DisableVars { get; private set; } = false;
    public Transaction Transaction { get; private set; } = Transaction.None;
    public Journal JournalTo { get; private set; } = Journal.Default;
    public NamingOptions Naming { get; private set; } = NamingOptions.Default;
    public List<ScriptBatch> Scripts { get; set; } = [];

    public Dictionary<string, string>? Vars { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Providers.IsSupportedProvider(Provider))
            yield return new ValidationResult($"Unsupported provider: {Provider}");
    }
}