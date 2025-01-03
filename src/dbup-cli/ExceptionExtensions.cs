namespace DbUp.Cli;

public static class ExceptionExtensions
{
    public static string FlattenErrorMessages(this Exception ex)
    {
        var msg = ex.Message;
        while (ex.InnerException is not null)
        {
            msg += $" > {ex.InnerException.Message}";
            ex = ex.InnerException;
        }

        return msg;
    }
}