using System;
using System.Collections.Generic;
using cuahanggiay.Models;

namespace cuahanggiay.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        
        // Data cho biểu đồ: 7 ngày gần nhất gồm nhãn ngày và giá trị
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<decimal> ChartData { get; set; } = new List<decimal>();
    }
}
