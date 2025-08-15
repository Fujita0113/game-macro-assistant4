using GameMacroAssistant.Core.Models;

namespace GameMacroAssistant.Core.Services;

/// <summary>
/// マクロ記録サービスのインターフェース
/// マウス・キーボード入力をフックし、イベントを記録する (R-001, R-002, R-003)
/// </summary>
public interface IMacroRecorder : IDisposable
{
    /// <summary>
    /// 記録が実行中かどうか
    /// </summary>
    bool IsRecording { get; }
    
    /// <summary>
    /// 記録中のマクロステップ
    /// </summary>
    IReadOnlyList<MacroStep> RecordedSteps { get; }
    
    /// <summary>
    /// 記録開始 (R-001)
    /// </summary>
    Task StartRecordingAsync();
    
    /// <summary>
    /// 記録停止 (R-005: デフォルトESCキー)
    /// </summary>
    Task StopRecordingAsync();
    
    /// <summary>
    /// 記録停止キーの変更 (R-005)
    /// </summary>
    void SetStopRecordingKey(int virtualKeyCode);
    
    /// <summary>
    /// 新しいステップが記録された時のイベント
    /// </summary>
    event EventHandler<MacroStep>? StepRecorded;
    
    /// <summary>
    /// 記録が停止された時のイベント
    /// </summary>
    event EventHandler? RecordingStopped;
    
    /// <summary>
    /// エラーが発生した時のイベント
    /// </summary>
    event EventHandler<string>? ErrorOccurred;
}