using RinohDevelopment.Models;

namespace RinohDevelopment.ViewModels;

public class RecyclableRequestDetailViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime PickupDate { get; set; }
    public TimeSpan PickupTimeStart { get; set; }
    public TimeSpan PickupTimeEnd { get; set; }
    public string Notes { get; set; }
    public RequestStatus Status { get; set; }
    
    public bool HasCollection { get; set; }
    public DateTime? CollectionDate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? CreditAssigned { get; set; }
    
    public string StatusDisplay => GetStatusDisplay(Status);
    public string TimeDisplay => $"{PickupDate.ToString("yyyy/MM/dd")} - {PickupTimeStart.ToString(@"hh\:mm")} تا {PickupTimeEnd.ToString(@"hh\:mm")}";
    
    private string GetStatusDisplay(RequestStatus status)
    {
        switch (status)
        {
            case RequestStatus.Pending:
                return "در انتظار تایید";
            case RequestStatus.Confirmed:
                return "تایید شده";
            case RequestStatus.Completed:
                return "انجام شده";
            case RequestStatus.Cancelled:
                return "لغو شده";
            default:
                return "";
        }
    }
}