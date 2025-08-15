# コードベース構造

## ディレクトリ構成
```
/
├── src/                          # アプリケーション本体
│   ├── GameMacroAssistant.Core/  # ビジネスロジック層
│   │   ├── Models/               # ドメインモデル
│   │   │   ├── Macro.cs
│   │   │   ├── Step.cs
│   │   │   ├── LogEntry.cs
│   │   │   └── PerformanceMetrics.cs
│   │   └── Services/             # サービス層
│   │       ├── ILogger.cs
│   │       ├── IMacroRecorder.cs
│   │       ├── ImageMatcher.cs
│   │       ├── MacroExecutor.cs
│   │       ├── MacroRecorder.cs
│   │       └── ScreenCaptureService.cs
│   ├── GameMacroAssistant.Wpf/   # プレゼンテーション層 (WPF UI)
│   ├── GameMacroAssistant.Tests/ # 単体・統合テスト
│   ├── WorkflowStateMachine.py   # ワークフロー状態管理
│   ├── ProgressVisualizer.py     # 進捗可視化
│   ├── ProgressManager.py        # 進捗データ管理
│   ├── ArtifactValidator.py      # 成果物検証
│   └── SignalParser.py          # エージェント間シグナル解析
├── docs/                        # プロジェクトドキュメント
│   ├── pm/                      # プロジェクト管理
│   ├── user-tests/              # ユーザーテスト手順書
│   ├── requirement.md           # 確定要件定義 v3.3
│   └── schema/                  # JSONスキーマ定義
├── worktrees/                   # Dev-Agent作業ディレクトリ
├── .claude/                     # Claude Code設定
│   ├── agents/                  # サブエージェント定義
│   ├── progress.json           # 全体進捗管理
│   └── settings.local.json     # Claude設定
└── GameMacroAssistant.sln      # Visual Studio ソリューション
```

## プロジェクト参照関係
- GameMacroAssistant.Wpf → GameMacroAssistant.Core (依存)
- GameMacroAssistant.Tests → Core, Wpf (依存)