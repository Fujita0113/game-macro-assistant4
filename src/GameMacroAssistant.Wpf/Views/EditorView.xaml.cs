using System.Windows.Controls;
using GameMacroAssistant.Wpf.ViewModels;

namespace GameMacroAssistant.Wpf.Views;

/// <summary>
/// ビジュアルエディタのコードビハインド
/// </summary>
public partial class EditorView : UserControl
{
    public EditorView(EditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}