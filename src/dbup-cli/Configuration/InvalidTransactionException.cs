namespace DbUp.Cli.Configuration;

public class InvalidTransactionException(Transaction transaction)
    : DbUpCliException($"Unsupported transaction value: {transaction}")
{
    public Transaction Transaction { get; } = transaction;
}