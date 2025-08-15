using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using GameMacroAssistant.Core.Models;

namespace GameMacroAssistant.Wpf.ViewModels;

/// <summary>
/// ビジュアルエディタのViewModel
/// Undo/Redo機能とブロック編集機能 (R-009, R-010)
/// </summary>
public partial class EditorViewModel : ObservableObject
{
    private readonly ILogger<EditorViewModel> _logger;
    private readonly Stack<EditorAction> _undoStack = new();
    private readonly Stack<EditorAction> _redoStack = new();
    
    [ObservableProperty]
    private MacroStep? _selectedStep;

    [ObservableProperty]
    private bool _canUndo;

    [ObservableProperty]
    private bool _canRedo;

    [ObservableProperty]
    private double _complexOperationTimeoutSeconds = 2.0; // R-009: デフォルト2秒

    public ObservableCollection<MacroStep> EditableSteps { get; } = new();

    public EditorViewModel(ILogger<EditorViewModel> logger)
    {
        _logger = logger;
        
        // 複合操作タイマーの設定 (R-009)
        SetupComplexOperationTimer();
    }

    /// <summary>
    /// Undo コマンド (R-009)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (_undoStack.Count == 0) return;

        var action = _undoStack.Pop();
        action.Undo();
        _redoStack.Push(action);
        
        UpdateUndoRedoState();
        _logger.LogDebug("Undo operation executed: {ActionType}", action.GetType().Name);
    }

    /// <summary>
    /// Redo コマンド (R-009)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        if (_redoStack.Count == 0) return;

        var action = _redoStack.Pop();
        action.Execute();
        _undoStack.Push(action);
        
        UpdateUndoRedoState();
        _logger.LogDebug("Redo operation executed: {ActionType}", action.GetType().Name);
    }

    /// <summary>
    /// 画像選択コマンド (R-008)
    /// </summary>
    [RelayCommand]
    private async Task SelectImage()
    {
        try
        {
            // TODO: ドラッグによる矩形選択で条件画像を編集
            _logger.LogInformation("Image selection started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image selection failed");
        }
    }

    /// <summary>
    /// 画像切り抜きコマンド (R-008)
    /// </summary>
    [RelayCommand]
    private async Task CropImage()
    {
        try
        {
            // TODO: 選択領域の画像切り抜き実装
            _logger.LogInformation("Image cropping started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image cropping failed");
        }
    }

    /// <summary>
    /// ステップの移動 (R-010: ドラッグ&ドロップによる順序変更)
    /// </summary>
    public void MoveStep(MacroStep step, int newIndex)
    {
        try
        {
            var oldIndex = EditableSteps.IndexOf(step);
            if (oldIndex == -1 || oldIndex == newIndex) return;

            // 垂直リストにスナップ (R-010)
            var clampedIndex = Math.Max(0, Math.Min(newIndex, EditableSteps.Count - 1));
            
            var moveAction = new MoveStepAction(EditableSteps, step, oldIndex, clampedIndex);
            ExecuteAction(moveAction);
            
            _logger.LogDebug("Step moved from index {OldIndex} to {NewIndex}", oldIndex, clampedIndex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move step");
            // 失敗した場合は操作をキャンセルして元の位置に戻す (R-010)
        }
    }

    /// <summary>
    /// エディタアクション実行
    /// </summary>
    private void ExecuteAction(EditorAction action)
    {
        action.Execute();
        _undoStack.Push(action);
        _redoStack.Clear(); // 新しい操作実行時はRedoスタックをクリア
        
        UpdateUndoRedoState();
    }

    /// <summary>
    /// Undo/Redo状態更新
    /// </summary>
    private void UpdateUndoRedoState()
    {
        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
    }

    /// <summary>
    /// 複合操作タイマー設定 (R-009)
    /// 2秒以内に完了した複合操作は1操作として扱う
    /// </summary>
    private void SetupComplexOperationTimer()
    {
        // TODO: 複合操作の検出とグループ化ロジック
        // 設定画面で0.5s～5.0sの範囲で変更可能
    }
}

/// <summary>
/// エディタアクションの基底クラス
/// </summary>
public abstract class EditorAction
{
    public abstract void Execute();
    public abstract void Undo();
}

/// <summary>
/// ステップ移動アクション
/// </summary>
public class MoveStepAction : EditorAction
{
    private readonly ObservableCollection<MacroStep> _steps;
    private readonly MacroStep _step;
    private readonly int _oldIndex;
    private readonly int _newIndex;

    public MoveStepAction(ObservableCollection<MacroStep> steps, MacroStep step, int oldIndex, int newIndex)
    {
        _steps = steps;
        _step = step;
        _oldIndex = oldIndex;
        _newIndex = newIndex;
    }

    public override void Execute()
    {
        _steps.RemoveAt(_oldIndex);
        _steps.Insert(_newIndex, _step);
    }

    public override void Undo()
    {
        _steps.RemoveAt(_newIndex);
        _steps.Insert(_oldIndex, _step);
    }
}