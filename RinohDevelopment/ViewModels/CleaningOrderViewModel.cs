using RinohDevelopment.Models;

namespace RinohDevelopment.ViewModels;

public class CleaningOrderViewModel
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; }
    public decimal ServicePrice { get; set; }
    public DateTime ServiceDate { get; set; }
    public string ServiceAddress { get; set; }
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