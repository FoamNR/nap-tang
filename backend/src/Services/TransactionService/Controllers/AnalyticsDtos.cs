using System;
using System.Collections.Generic;

namespace EasyTrack.TransactionService.Controllers;

public record SummaryResponse(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetBalance,
    DateTime StartDate,
    DateTime EndDate
);

public record CategoryBreakdownDto(
    Guid CategoryId,
    string CategoryName,
    string IconName,
    string ColorHex,
    decimal TotalAmount,
    decimal Percentage
);

public record TrendDataPoint(
    string Label,
    decimal Income,
    decimal Expense
);

public record TrendResponse(
    string Interval,
    List<TrendDataPoint> DataPoints
);
