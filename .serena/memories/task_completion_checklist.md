# タスク完了時チェックリスト

## 実装完了時の必須手順

### 1. ビルド・テスト検証
```bash
# 1. ソリューションビルド成功確認
dotnet build GameMacroAssistant.sln

# 2. 全テスト実行・成功確認
dotnet test

# 3. テストカバレッジ測定・80%以上確認
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html
```

### 2. 静的解析・品質チェック
- **コンパイラ警告**: ゼロ件維持
- **Null参照警告**: 解決済み
- **SOLID原則**: 準拠確認
- **命名規約**: 適合確認

### 3. 成果物検証
```bash
# 成果物自動検証
python src/ArtifactValidator.py <task_id>
```

### 4. 完了シグナル送信（Dev-Agent用）
```json
##DEV_DONE##|evidence:{
  "task_id": "T-XXX",
  "files": ["src/path/to/modified/file.cs"],
  "build_status": "success",
  "test_coverage": "XX%",
  "tests_passing": XX,
  "tests_total": XX,
  "warnings_count": 0
}
```

### 5. Git操作（手動実行時）
```bash
# 変更ファイル確認
git status
git diff

# コミット（メインエージェントが実行）
# - Dev-Agentは直接コミット禁止
# - シグナル経由でメインエージェントが処理
```

## Review-Agent完了時
```json
##REVIEW_PASS##|evidence:{
  "task_id": "T-XXX",
  "reviewed_files": ["src/path/file.cs"],
  "coverage_percent": "XX",
  "issues_found": "0",
  "static_analysis_result": "clean",
  "worktree_path": "worktrees/T-XXX"
}
```

## TestDoc-Agent完了時
```json
##TESTDOC_COMPLETE##|evidence:{
  "task_id": "T-XXX",
  "test_file_path": "docs/user-tests/T-XXX.md",
  "test_count": X,
  "estimated_minutes": XX,
  "worktree_path": "worktrees/T-XXX"
}
```

## 禁止事項
- **旧形式シグナル**: `##DEV_DONE##` (証跡なし) → 自動拒否
- **progress.json直接更新**: サブエージェント禁止
- **絶対パス使用**: 環境依存回避のため禁止