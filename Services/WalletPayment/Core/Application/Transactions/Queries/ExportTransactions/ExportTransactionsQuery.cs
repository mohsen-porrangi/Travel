using BuildingBlocks.CQRS;
using WalletPayment.Application.Transactions.Queries.Common;

namespace WalletPayment.Application.Transactions.Queries.ExportTransactions;

public record ExportTransactionsQuery : IQuery<ExportTransactionsResponse>
{
    public Guid UserId { get; init; }
    public ExportFormat Format { get; init; } = ExportFormat.Csv;
    public TransactionFilter Filter { get; init; } = new();
}

public enum ExportFormat
{
    Csv,
    Excel,
    Pdf
}

public record ExportTransactionsResponse
{
    public byte[] FileContents { get; init; }
    public string FileName { get; init; }
    public string ContentType { get; init; }
}