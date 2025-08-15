using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GameMacroAssistant.Core.Services;
using GameMacroAssistant.Wpf.ViewModels;
using GameMacroAssistant.Wpf.Views;

namespace GameMacroAssistant.Wpf;

/// <summary>
/// WPFアプリケーションのエントリポイント
/// 依存性注入とMVVMパターンの初期化
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // DI コンテナの設定
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(ConfigureLogging)
            .Build();

        // メインウィンドウの表示
        var mainWindow = _host.Services.GetRequiredService<MainView>();
        mainWindow.Show();

        _host.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    /// <summary>
    /// 依存性注入サービスの設定
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // Core Services
        services.AddTransient<ScreenCaptureService>();
        services.AddTransient<ImageMatcher>();
        // TODO: services.AddTransient<IMacroRecorder, MacroRecorderService>();
        
        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<EditorViewModel>();
        
        // Views
        services.AddTransient<MainView>();
        services.AddTransient<EditorView>();
    }

    /// <summary>
    /// ログ設定
    /// </summary>
    private static void ConfigureLogging(ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
        
        // TODO: ファイルログ設定 (R-015: %APPDATA%\GameMacroAssistant\Logs\YYYY-MM-DD.log)
        logging.SetMinimumLevel(LogLevel.Information);
    }
}