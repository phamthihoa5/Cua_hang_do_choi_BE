namespace Application.Model.Finance
{
    public class FinanceSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }

        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }
        public int UnpaidOrders { get; set; }
        public int CancelledOrders { get; set; }
    }
}