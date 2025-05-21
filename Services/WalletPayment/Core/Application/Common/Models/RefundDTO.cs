using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Application.Transactions.Queries.GetRefundableTransaction;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Common.Models;
public record RefundabilityResult
{
    public bool IsRefundable { get; init; }
    public RefundSourceType SourceType { get; init; }
    public Guid SourceId { get; init; }
    public decimal OriginalAmount { get; init; }
    public decimal RefundableAmount { get; init; }
    public decimal AlreadyRefundedAmount { get; init; }
    public bool HasPartialRefunds { get; init; }
    public List<RefundHistoryItemDto> RefundHistory { get; init; } = new();
    public string? Currency { get; init; }
    public DateTime? TransactionDate { get; init; }
    public string? Description { get; init; }
}

public record RefundResult
{
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid? RefundId { get; init; }
    public RefundSourceType SourceType { get; init; }
    public Guid SourceId { get; init; }
    public decimal RefundedAmount { get; init; }
    public decimal RemainingBalance { get; init; }
    public DateTime RefundDate { get; init; }
    public bool IsPartial { get; init; }
}