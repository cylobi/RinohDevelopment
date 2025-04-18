using RinohDevelopment.Models;

namespace RinohDevelopment.ViewModels;

public class RecyclableRequestListViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime PickupDate { get; set; }
    public TimeSpan PickupTimeStart { get; set; }
    public TimeSpan PickupTimeEnd { get; set; }
    public string Notes { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
}