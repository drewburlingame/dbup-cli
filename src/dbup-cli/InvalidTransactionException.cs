namespace DbUp.Cli;

public class InvalidTransactionException(Transaction transaction)
    : DbUpCliException($"Unsupported transaction value: {transaction}")
{
    public Transaction Transaction { get; } = transaction;
}