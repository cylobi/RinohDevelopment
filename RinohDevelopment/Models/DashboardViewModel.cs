namespace RinohDevelopment.ViewModels;

public class DashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public decimal Credit { get; set; }
    public List<RecyclableRequestListItemViewModel> RecentRequests { get; set; } = new List<RecyclableRequestListItemViewModel>();
}