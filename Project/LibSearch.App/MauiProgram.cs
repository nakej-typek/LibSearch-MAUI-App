using LibSearch.App.Data;
using LibSearch.App.Services;
using LibSearch.App.ViewModels;
using LibSearch.App.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibSearch.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "libsearch.db");
		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite($"Data Source={dbPath}"));

		var options = new LibSearchOptions();
		builder.Services.AddSingleton(options);

		builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
		builder.Services.AddSingleton<ISessionService, SessionService>();
		builder.Services.AddScoped<IAuthService, AuthService>();
		builder.Services.AddScoped<ILibraryService, LibraryService>();
		builder.Services.AddScoped<ISearchHistoryService, SearchHistoryService>();
		builder.Services.AddScoped<ISavedPassageService, SavedPassageService>();
		builder.Services.AddSingleton<IExportService, ExportService>();
		builder.Services.AddScoped<IStatsService, StatsService>();

		builder.Services.AddHttpClient<ILibSearchClient, LibSearchHttpClient>(client =>
		{
			client.BaseAddress = new Uri(options.BaseUrl);
			client.Timeout = TimeSpan.FromMinutes(15);
		});

		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<LibraryViewModel>();
		builder.Services.AddTransient<ReaderViewModel>();
		builder.Services.AddTransient<SavedListViewModel>();
		builder.Services.AddTransient<HistoryViewModel>();
		builder.Services.AddTransient<StatsViewModel>();

		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<LibraryPage>();
		builder.Services.AddTransient<ReaderPage>();
		builder.Services.AddTransient<SavedListPage>();
		builder.Services.AddTransient<HistoryPage>();
		builder.Services.AddTransient<StatsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			db.Database.EnsureCreated();
		}

		return app;
	}
}
