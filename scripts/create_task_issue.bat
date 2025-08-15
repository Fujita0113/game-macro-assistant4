@echo off
REM Windows用GitHub Issue自動作成スクリプト
REM 使用例: scripts\create_task_issue.bat R-001 "マクロ記録開始ボタンUI実装"

setlocal enabledelayedexpansion

set REQUIREMENT=%1
set TITLE=%2
set PRIORITY=Medium
set SPRINT=sprint-01
set MILESTONE=v1.0
set ASSIGNEE=

REM パラメータチェック
if "%REQUIREMENT%"=="" (
    echo ❌ 要件IDが指定されていません
    echo 使用方法: %0 R-XXX "タスク名"
    exit /b 1
)

if "%TITLE%"=="" (
    echo ❌ タイトルが指定されていません
    echo 使用方法: %0 R-XXX "タスク名"
    exit /b 1
)

REM GitHub CLI 確認
gh --version >nul 2>&1
if errorlevel 1 (
    echo ❌ GitHub CLI (gh) がインストールされていません
    echo インストール: https://cli.github.com/
    exit /b 1
)

REM 認証確認
gh auth status >nul 2>&1
if errorlevel 1 (
    echo ❌ GitHub CLI が認証されていません
    echo 認証コマンド: gh auth login
    exit /b 1
)

echo 🎯 Creating new task issue...
echo Requirement: %REQUIREMENT%
echo Title: %TITLE%
echo Priority: %PRIORITY%

REM タスクID生成（簡易版）
for /f %%i in ('gh issue list --limit 1000 --json title ^| jq -r "length"') do set ISSUE_COUNT=%%i
set /a TASK_NUM=%ISSUE_COUNT% + 1
set TASK_ID=T-%03d
call set TASK_ID=T-%%TASK_NUM:~-3%%

REM 要件内容取得
set REQUIREMENT_CONTENT=要件詳細を確認してください
if exist "docs\requirement.md" (
    for /f "delims=" %%a in ('findstr /C:"%REQUIREMENT%" "docs\requirement.md"') do set REQUIREMENT_CONTENT=%%a
)

REM Issue本文をテンプレートファイルに書き出し
echo ## 📋 タスク概要 > temp_issue_body.md
echo **要件ID:** %REQUIREMENT% >> temp_issue_body.md
echo **機能グループ:** [要件定義から確認] >> temp_issue_body.md
echo **優先度:** %PRIORITY% >> temp_issue_body.md
echo. >> temp_issue_body.md
echo ## 🎯 実装内容 >> temp_issue_body.md
echo ### 対象要件 >> temp_issue_body.md
echo ^> %REQUIREMENT_CONTENT% >> temp_issue_body.md
echo. >> temp_issue_body.md
echo ### 実装範囲 >> temp_issue_body.md
echo - [ ] コア機能実装 >> temp_issue_body.md
echo - [ ] 単体テスト作成 (カバレッジ80%+) >> temp_issue_body.md
echo - [ ] 統合テスト作成 >> temp_issue_body.md
echo - [ ] エラーハンドリング >> temp_issue_body.md
echo - [ ] パフォーマンス要件対応 >> temp_issue_body.md
echo. >> temp_issue_body.md
echo ## ✅ 完了基準 >> temp_issue_body.md
echo - [ ] ビルド成功 >> temp_issue_body.md
echo - [ ] 全テスト成功 >> temp_issue_body.md
echo - [ ] カバレッジ80%%以上達成 >> temp_issue_body.md
echo - [ ] コードレビュー完了 >> temp_issue_body.md
echo - [ ] ユーザーテスト作成完了 >> temp_issue_body.md

REM Issue作成
set ISSUE_TITLE=[%TASK_ID%] %REQUIREMENT%: %TITLE%
set LABELS=task,implementation,%SPRINT%

echo 📝 Creating GitHub Issue...
gh issue create --title "%ISSUE_TITLE%" --body-file temp_issue_body.md --label "%LABELS%" --milestone "%MILESTONE%"

if errorlevel 1 (
    echo ❌ Issue作成に失敗しました
    del temp_issue_body.md
    exit /b 1
)

del temp_issue_body.md

echo ✅ Issue created successfully!
echo 📋 Task ID: %TASK_ID%
echo.
echo 📋 Next steps:
echo 1. Review the issue details
echo 2. Start implementation
echo 3. Run tests: scripts\run_coverage.bat
echo 4. Call review agent: python .claude\review-agent.py --task %TASK_ID% ...

endlocal