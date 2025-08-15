using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using GameMacroAssistant.Core.Models;
using GameMacroAssistant.Core.Services;

namespace GameMacroAssistant.Wpf.ViewModels;

/// <summary>
/// メインウィンドウのViewModel
/// MVVM パターンでの画面制御とマクロ管理
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly ScreenCaptureService _screenCapture;
    private readonly ImageMatcher _imageMatcher;

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private string _recordingButtonText = "記録開始";

    [ObservableProperty]
    private string _statusMessage = "準備完了";

    [ObservableProperty]
    private double _cpuUsage;

    [ObservableProperty]
    private double _memoryUsage;

    [ObservableProperty]
    private int _stopRecordingKey = 27; // ESC key (R-005)

    [ObservableProperty]
    private double _ssimThreshold = 0.95; // R-013

    [ObservableProperty]
    private double _pixelDifferenceThreshold = 0.03; // R-013

    [ObservableProperty]
    private MacroStep? _selectedStep;

    public ObservableCollection<MacroStep> MacroSteps { get; } = new();

    public MainViewModel(
        ILogger<MainViewModel> logger,
        ScreenCaptureService screenCapture,
        ImageMatcher imageMatcher)
    {
        _logger = logger;
        _screenCapture = screenCapture;
        _imageMatcher = imageMatcher;

        // パフォーマンス監視タイマー (R-020)
        StartPerformanceMonitoring();
    }

    /// <summary>
    /// マクロ記録開始/停止コマンド (R-001)
    /// </summary>
    [RelayCommand]
    private async Task StartRecording()
    {
        try
        {
            if (!IsRecording)
            {
                await StartMacroRecording();
            }
            else
            {
                await StopMacroRecording();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Recording operation failed");
            StatusMessage = $"エラー: {ex.Message}";
        }
    }

    /// <summary>
    /// マクロファイルを開く
    /// </summary>
    [RelayCommand]
    private async Task OpenMacro()
    {
        try
        {
            // TODO: ファイルダイアログでマクロファイル選択 (.gma.json)
            // TODO: パスフレーズ認証 (R-018)
            // TODO: スキーマバージョン確認 (R-027)
            
            StatusMessage = "マクロファイルを開きました";
            _logger.LogInformation("Macro file opened successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open macro file");
            StatusMessage = $"ファイル読み込みエラー: {ex.Message}";
        }
    }

    /// <summary>
    /// マクロファイルを保存
    /// </summary>
    [RelayCommand]
    private async Task SaveMacro()
    {
        try
        {
            // TODO: .gma.json 形式での保存
            // TODO: 暗号化オプション (R-018)
            // TODO: スキーマバージョン付与 (R-027)
            
            StatusMessage = "マクロファイルを保存しました";
            _logger.LogInformation("Macro file saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save macro file");
            StatusMessage = $"ファイル保存エラー: {ex.Message}";
        }
    }

    /// <summary>
    /// マクロ実行
    /// </summary>
    [RelayCommand]
    private async Task PlayMacro()
    {
        try
        {
            // TODO: マクロステップの順次実行
            // TODO: タイミング精度確保 (R-014: 平均≤5ms、最大≤15ms)
            // TODO: 画像マッチング待機 (R-013)
            // TODO: エラーハンドリングとトースト通知 (R-015)
            
            StatusMessage = "マクロを実行中...";
            _logger.LogInformation("Macro execution started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Macro execution failed");
            StatusMessage = $"実行エラー: {ex.Message}";
        }
    }

    /// <summary>
    /// マクロ実行停止
    /// </summary>
    [RelayCommand]
    private async Task StopMacro()
    {
        try
        {
            // TODO: 実行中マクロの停止
            StatusMessage = "マクロ実行を停止しました";
            _logger.LogInformation("Macro execution stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop macro execution");
            StatusMessage = $"停止エラー: {ex.Message}";
        }
    }

    /// <summary>
    /// マクロ記録開始処理
    /// </summary>
    private async Task StartMacroRecording()
    {
        // TODO: IMacroRecorder の実装と統合
        IsRecording = true;
        RecordingButtonText = "記録停止";
        StatusMessage = "マクロを記録中... (ESCキーで停止)";
        
        _logger.LogInformation("Macro recording started");
    }

    /// <summary>
    /// マクロ記録停止処理
    /// </summary>
    private async Task StopMacroRecording()
    {
        IsRecording = false;
        RecordingButtonText = "記録開始";
        StatusMessage = "記録を停止しました";
        
        // TODO: ビジュアルエディタの自動表示 (R-007)
        
        _logger.LogInformation("Macro recording stopped");
    }

    /// <summary>
    /// パフォーマンス監視開始 (R-020)
    /// </summary>
    private void StartPerformanceMonitoring()
    {
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        
        timer.Tick += (_, _) =>
        {
            // TODO: 実際のCPU・メモリ使用率取得
            CpuUsage = GetCpuUsage();
            MemoryUsage = GetMemoryUsage();
            
            // パフォーマンス閾値チェック (R-020: CPU≤15%, RAM≤300MB)
            if (CpuUsage > 15 || MemoryUsage > 300)
            {
                // TODO: トースト通知 "High Load"
                _logger.LogWarning("Performance threshold exceeded - CPU: {CpuUsage:F1}%, RAM: {MemoryUsage:F0}MB", 
                    CpuUsage, MemoryUsage);
            }
        };
        
        timer.Start();
    }

    /// <summary>
    /// CPU使用率取得 (モック実装)
    /// </summary>
    private double GetCpuUsage()
    {
        // TODO: 実際のCPU使用率取得実装
        return Random.Shared.NextDouble() * 20;
    }

    /// <summary>
    /// メモリ使用量取得 (モック実装)
    /// </summary>
    private double GetMemoryUsage()
    {
        // TODO: 実際のメモリ使用量取得実装
        return Random.Shared.NextDouble() * 200 + 50;
    }
}