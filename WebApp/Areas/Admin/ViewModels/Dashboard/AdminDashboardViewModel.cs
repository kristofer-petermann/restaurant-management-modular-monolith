namespace WebApp.Areas.Admin.ViewModels.Dashboard;

public class AdminDashboardViewModel
{
    public required string PageTitle { get; init; }
    public required string Subtitle { get; init; }
    public required IReadOnlyList<AdminDashboardStatViewModel> Stats { get; init; }
}

public class AdminDashboardStatViewModel
{
    public required string Label { get; init; }
    public required string Value { get; init; }
    public required string Controller { get; init; }
}
