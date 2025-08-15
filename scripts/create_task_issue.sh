#!/bin/bash
# GitHub Issue自動作成スクリプト
# 使用例: ./scripts/create_task_issue.sh --requirement "R-001" --title "マクロ記録開始ボタンUI実装"

set -e

REQUIREMENT=""
TITLE=""
PRIORITY="Medium"
SPRINT="sprint-01"
MILESTONE="v1.0"
ASSIGNEE=""

# パラメータ解析
while [[ $# -gt 0 ]]; do
    case $1 in
        --requirement)
            REQUIREMENT="$2"
            shift 2
            ;;
        --title)
            TITLE="$2"
            shift 2
            ;;
        --priority)
            PRIORITY="$2"
            shift 2
            ;;
        --sprint)
            SPRINT="$2"
            shift 2
            ;;
        --milestone)
            MILESTONE="$2"
            shift 2
            ;;
        --assignee)
            ASSIGNEE="$2"
            shift 2
            ;;
        --help)
            echo "使用方法: $0 --requirement R-XXX --title 'タスク名'"
            echo "オプション:"
            echo "  --requirement R-XXX    要件ID (必須)"
            echo "  --title 'タスク名'     タスクタイトル (必須)"
            echo "  --priority [High|Medium|Low]  優先度 (デフォルト: Medium)"
            echo "  --sprint スプリント名  スプリント (デフォルト: sprint-01)"
            echo "  --milestone マイルストーン  マイルストーン (デフォルト: v1.0)"
            echo "  --assignee ユーザー名  アサイン先"
            exit 0
            ;;
        *)
            echo "不明なパラメータ: $1"
            exit 1
            ;;
    esac
done

# 必須パラメータチェック
if [[ -z "$REQUIREMENT" || -z "$TITLE" ]]; then
    echo "❌ --requirement と --title は必須です"
    echo "使用方法: $0 --requirement R-001 --title 'タスク名'"
    exit 1
fi

# GitHub CLI 確認
if ! command -v gh &> /dev/null; then
    echo "❌ GitHub CLI (gh) がインストールされていません"
    echo "インストール: https://cli.github.com/"
    exit 1
fi

# 認証確認
if ! gh auth status &> /dev/null; then
    echo "❌ GitHub CLI が認証されていません"
    echo "認証コマンド: gh auth login"
    exit 1
fi

# タスクID生成 (既存Issue数から自動採番)
EXISTING_ISSUES=$(gh issue list --limit 1000 --json title | jq -r '.[].title' | grep -c '^\[T-' || true)
TASK_ID=$(printf "T-%03d" $((EXISTING_ISSUES + 1)))

echo "🎯 Creating new task issue..."
echo "Task ID: $TASK_ID"
echo "Requirement: $REQUIREMENT"
echo "Title: $TITLE"
echo "Priority: $PRIORITY"

# 要件定義から該当要件を抽出
REQUIREMENT_CONTENT=""
if [[ -f "docs/requirement.md" ]]; then
    REQUIREMENT_CONTENT=$(grep -A 2 "$REQUIREMENT" docs/requirement.md | tail -n 1 || echo "要件詳細が見つかりません")
fi

# Issue本文生成
ISSUE_BODY=$(cat << EOF
## 📋 タスク概要
**要件ID:** $REQUIREMENT  
**機能グループ:** [要件定義から確認]  
**優先度:** $PRIORITY

## 🎯 実装内容
### 対象要件
> $REQUIREMENT_CONTENT

### 実装範囲
- [ ] コア機能実装
- [ ] 単体テスト作成 (カバレッジ80%+)
- [ ] 統合テスト作成
- [ ] エラーハンドリング
- [ ] パフォーマンス要件対応

## 🏗️ 技術詳細
**実装予定ファイル:**
- \`src/GameMacroAssistant.Core/[ファイル名]\`
- \`src/GameMacroAssistant.Tests/[テストファイル名]\`

**依存関係:**
- 前提タスク: なし
- ブロック対象: なし

## ✅ 完了基準
- [ ] ビルド成功
- [ ] 全テスト成功
- [ ] カバレッジ80%以上達成
- [ ] コードレビュー完了
- [ ] ユーザーテスト作成完了

## 📊 進捗記録
### 実装フェーズ
- [ ] 設計完了
- [ ] 実装完了  
- [ ] テスト完了
- [ ] レビュー完了
- [ ] 修正完了
- [ ] ユーザーテスト準備完了
- [ ] 承認完了

### カバレッジ結果
\`\`\`
Line Coverage: 未測定
Branch Coverage: 未測定
Method Coverage: 未測定
\`\`\`

### テスト実行結果
\`\`\`
Total tests: 未実行
Passed: 未実行
Failed: 未実行
Skipped: 未実行
\`\`\`

## 📝 レビューファイル
レビュー完了後、以下にレビューレポートを添付:
- \`docs/reviews/$TASK_ID-review.md\`

## 🔗 関連リンク
- 要件定義: [docs/requirement.md](../docs/requirement.md)
- 技術仕様: [CLAUDE.md](../CLAUDE.md)
EOF
)

# Issue作成
ISSUE_TITLE="[$TASK_ID] $REQUIREMENT: $TITLE"
LABELS="task,implementation,$SPRINT"

# アサイン先オプション
ASSIGNEE_OPTION=""
if [[ -n "$ASSIGNEE" ]]; then
    ASSIGNEE_OPTION="--assignee $ASSIGNEE"
fi

echo "📝 Creating GitHub Issue..."

ISSUE_URL=$(gh issue create \
    --title "$ISSUE_TITLE" \
    --body "$ISSUE_BODY" \
    --label "$LABELS" \
    --milestone "$MILESTONE" \
    $ASSIGNEE_OPTION)

echo "✅ Issue created successfully!"
echo "🔗 Issue URL: $ISSUE_URL"
echo ""
echo "📋 Next steps:"
echo "1. Review the issue details"
echo "2. Start implementation"
echo "3. Update progress in issue description"
echo "4. Run tests and coverage: ./scripts/run_coverage.sh"
echo "5. Call review agent: python .claude/review-agent.py --task $TASK_ID ..."