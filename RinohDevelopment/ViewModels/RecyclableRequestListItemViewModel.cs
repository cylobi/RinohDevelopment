using RinohDevelopment.Models;

namespace RinohDevelopment.ViewModels;

public class RecyclableRequestListItemViewModel
{
    public int Id { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime PickupDate { get; set; }
    public TimeSpan PickupTimeStart { get; set; }
    public TimeSpan PickupTimeEnd { get; set; }
    public RequestStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    
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