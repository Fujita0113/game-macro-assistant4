using System.Collections.ObjectModel;

namespace GameMacroAssistant.Core.Models;

/// <summary>
/// マクロ実行ファイルを表現するモデル。R-027で定義されたスキーマに準拠
/// </summary>
public class Macro
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public ObservableCollection<MacroStep> Steps { get; set; } = new();
    public MacroSettings Settings { get; set; } = new();
    
    /// <summary>
    /// パスフレーズによる暗号化が有効かどうか (R-018)
    /// </summary>
    public bool IsEncrypted { get; set; }
}

/// <summary>
/// マクロの実行設定
/// </summary>
public class MacroSettings
{
    /// <summary>
    /// 画像一致判定の閾値設定 (R-013)
    /// </summary>
    public ImageMatchSettings ImageMatch { get; set; } = new();
    
    /// <summary>
    /// 記録停止キー設定 (R-005)
    /// </summary>
    public int StopRecordingKey { get; set; } = 27; // ESC key default
    
    /// <summary>
    /// グローバルホットキー設定 (R-012)
    /// </summary>
    public string? GlobalHotkey { get; set; }
}

/// <summary>
/// 画像マッチング設定 (R-013)
/// </summary>
public class ImageMatchSettings
{
    /// <summary>
    /// SSIM閾値 (デフォルト: 0.95)
    /// </summary>
    public double SsimThreshold { get; set; } = 0.95;
    
    /// <summary>
    /// ピクセル差閾値 (デフォルト: 3%)
    /// </summary>
    public double PixelDifferenceThreshold { get; set; } = 0.03;
}