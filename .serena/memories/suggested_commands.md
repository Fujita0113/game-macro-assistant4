# 推奨コマンド一覧

## ビルド・テスト関連
```bash
# ソリューション全体ビルド
dotnet build GameMacroAssistant.sln

# リリースビルド
dotnet build GameMacroAssistant.sln --configuration Release

# テスト実行
dotnet test

# テストカバレッジ測定
dotnet test --collect:"XPlat Code Coverage"

# カバレッジレポート生成
reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html

# カバレッジレポート表示 (Windows)
start TestResults/CoverageReport/index.html
```

## ワークフロー管理
```bash
# ワークフロー状態確認
python src/WorkflowStateMachine.py

# 進捗ダッシュボード表示
python src/ProgressVisualizer.py

# 成果物検証
python src/ArtifactValidator.py <task_id>
```

## Git操作
```bash
# 現在の状態確認
git status
git log --oneline -10

# ブランチ操作
git branch -a
git worktree list

# worktree作成 (Dev-Agent用)
git worktree add worktrees/<task-id> -b task-<task-id>-<description>
```

## Windows特有のコマンド
```bash
# ディレクトリ一覧
dir /b
ls  # PowerShell or Git Bash

# ファイル検索
where <filename>
Get-ChildItem -Recurse -Name "*.cs"

# プロセス確認
tasklist | findstr dotnet
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"}
```

## パッケージ管理
```bash
# NuGetパッケージ復元
dotnet restore

# パッケージ追加
dotnet add package <PackageName>

# パッケージ一覧
dotnet list package
```