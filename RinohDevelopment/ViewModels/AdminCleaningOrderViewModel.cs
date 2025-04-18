using RinohDevelopment.Models;

namespace RinohDevelopment.ViewModels;

public class AdminCleaningOrderViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal ServicePrice { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ServiceDate { get; set; }
    public string ServiceAddress { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    
    public string StatusDisplay => GetStatusDisplay(Status);
    
    private string GetStatusDisplay(OrderStatus status)
    {
        switch (status)
        {
            case OrderStatus.Pending:
                return "در انتظار تایید";
            case OrderStatus.Confirmed:
                return "تایید شده";
            case OrderStatus.Completed:
                return "انجام شده";
            case OrderStatus.Cancelled:
                return "لغو شده";
            default:
                return "";
        }
    }
}