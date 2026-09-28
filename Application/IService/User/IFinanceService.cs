using Application.Model.Finance;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface IFinanceService
    {
        // Lấy số liệu tổng quan tài chính
        Task<FinanceSummaryDto> GetFinanceSummaryAsync(
            DateTime? fromDate,
            DateTime? toDate
        );

        // Lấy danh sách giao dịch
        Task<List<FinanceTransactionDto>> GetTransactionsAsync(
            DateTime? fromDate,
            DateTime? toDate
        );
    }
}