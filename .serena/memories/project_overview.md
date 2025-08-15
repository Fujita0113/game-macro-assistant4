# GameMacroAssistant - プロジェクト概要

## プロジェクトの目的
GameMacroAssistantは、Windows 11環境でのPCゲーム操作を自動化するマクロツールです。プログラミング知識がないライトユーザーでも直感的にマクロを作成・編集できることを目標としています。

## 技術スタック
- **言語**: C# (.NET 8)
- **UI Framework**: WPF with MVVM pattern
- **MVVM Library**: CommunityToolkit.Mvvm
- **テストフレームワーク**: xUnit, Moq
- **カバレッジツール**: Coverlet, ReportGenerator
- **Windows API**: Desktop Duplication API, GDI BitBlt, user32.dll

## アーキテクチャ
- MVVM パターンによる疎結合設計
- 依存性注入 (Microsoft.Extensions.DependencyInjection)
- SOLID原則準拠
- テストカバレッジ 80%以上維持
- Core層とWPF層の分離設計

## 主要機能
1. **マクロ記録**: マウス・キーボード操作の自動記録
2. **ビジュアルエディタ**: ドラッグ&ドロップによるマクロ編集
3. **マクロ実行**: 高精度な操作再現
4. **画像マッチング**: SSIM・ピクセル差分による画像比較
5. **スクリーンキャプチャ**: Desktop Duplication API + GDI フォールバック