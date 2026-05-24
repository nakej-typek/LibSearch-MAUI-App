using LibSearch.App.Views;

namespace LibSearch.App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("reader", typeof(ReaderPage));
		Routing.RegisterRoute("saved", typeof(SavedListPage));
		Routing.RegisterRoute("history", typeof(HistoryPage));
		Routing.RegisterRoute("stats", typeof(StatsPage));
		Routing.RegisterRoute("library/reader", typeof(ReaderPage));
	}
}
