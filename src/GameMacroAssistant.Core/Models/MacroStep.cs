using System.Drawing;

namespace GameMacroAssistant.Core.Models;

/// <summary>
/// マクロの各ステップを表現するモデル
/// </summary>
public abstract class MacroStep
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public abstract MacroStepType Type { get; }
    public string? ScreenshotPath { get; set; }
}

/// <summary>
/// マウス操作ステップ (R-002)
/// </summary>
public class MouseStep : MacroStep
{
    public override MacroStepType Type => MacroStepType.Mouse;
    
    /// <summary>
    /// スクリーン絶対座標 (px)
    /// </summary>
    public Point Position { get; set; }
    
    /// <summary>
    /// マウスボタン種別
    /// </summary>
    public MouseButton Button { get; set; }
    
    /// <summary>
    /// 押下時間 (ms)
    /// </summary>
    public int PressedDurationMs { get; set; }
}

/// <summary>
/// キーボード操作ステップ (R-003)
/// </summary>
public class KeyboardStep : MacroStep
{
    public override MacroStepType Type => MacroStepType.Keyboard;
    
    /// <summary>
    /// 仮想キーコード
    /// </summary>
    public int VirtualKeyCode { get; set; }
    
    /// <summary>
    /// キーの押下・離上種別
    /// </summary>
    public KeyAction Action { get; set; }
}

/// <summary>
/// 待機ステップ
/// </summary>
public class DelayStep : MacroStep
{
    public override MacroStepType Type => MacroStepType.Delay;
    
    /// <summary>
    /// 待機時間 (ms)
    /// </summary>
    public int DelayMs { get; set; }
}

/// <summary>
/// 画像検索・条件待機ステップ
/// </summary>
public class ImageWaitStep : MacroStep
{
    public override MacroStepType Type => MacroStepType.ImageWait;
    
    /// <summary>
    /// 検索対象画像のパス
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;
    
    /// <summary>
    /// タイムアウト時間 (ms)
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;
    
    /// <summary>
    /// 検索領域 (null の場合は全画面)
    /// </summary>
    public Rectangle? SearchArea { get; set; }
}

public enum MacroStepType
{
    Mouse,
    Keyboard,
    Delay,
    ImageWait
}

public enum MouseButton
{
    Left,
    Right,
    Middle,
    X1,
    X2
}

public enum KeyAction
{
    KeyDown,
    KeyUp
}