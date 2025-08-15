#!/bin/bash
# カバレッジ測定実行スクリプト
# 使用例: ./scripts/run_coverage.sh --target 80

set -e

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TARGET_COVERAGE=80
COVERAGE_DIR="$PROJECT_ROOT/coverage"
REPORTS_DIR="$COVERAGE_DIR/report"

# パラメータ解析
while [[ $# -gt 0 ]]; do
    case $1 in
        --target)
            TARGET_COVERAGE="$2"
            shift 2
            ;;
        --help)
            echo "使用方法: $0 [--target カバレッジ閾値]"
            echo "例: $0 --target 80"
            exit 0
            ;;
        *)
            echo "不明なパラメータ: $1"
            exit 1
            ;;
    esac
done

echo "🔍 Starting coverage analysis..."
echo "Target coverage: ${TARGET_COVERAGE}%"
echo "Project root: $PROJECT_ROOT"

# カバレッジディレクトリ準備
rm -rf "$COVERAGE_DIR"
mkdir -p "$COVERAGE_DIR" "$REPORTS_DIR"

cd "$PROJECT_ROOT"

# .NET テストプロジェクト検索
TEST_PROJECTS=$(find . -name "*.Tests.csproj" -o -name "*Test.csproj" | head -5)

if [[ -z "$TEST_PROJECTS" ]]; then
    echo "⚠️  テストプロジェクトが見つかりません"
    echo "以下のパターンで検索しました: *.Tests.csproj, *Test.csproj"
    exit 1
fi

echo "📋 Found test projects:"
echo "$TEST_PROJECTS"

# テスト実行とカバレッジ収集
echo "🧪 Running tests with coverage collection..."

for project in $TEST_PROJECTS; do
    echo "Testing: $project"
    dotnet test "$project" \
        --configuration Release \
        --collect:"XPlat Code Coverage" \
        --results-directory "$COVERAGE_DIR" \
        --logger "console;verbosity=minimal" || {
            echo "❌ Tests failed for $project"
            exit 1
        }
done

# カバレッジレポート存在確認
COVERAGE_FILES=$(find "$COVERAGE_DIR" -name "coverage.cobertura.xml" 2>/dev/null)

if [[ -z "$COVERAGE_FILES" ]]; then
    echo "❌ カバレッジファイルが生成されませんでした"
    echo "パッケージ確認: dotnet add package coverlet.collector"
    exit 1
fi

echo "📊 Found coverage files:"
echo "$COVERAGE_FILES"

# ReportGenerator でHTMLレポート生成
echo "📝 Generating HTML coverage report..."

# ReportGenerator のインストール確認
if ! command -v reportgenerator &> /dev/null; then
    echo "Installing ReportGenerator..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# レポート生成
reportgenerator \
    -reports:"$COVERAGE_DIR/**/coverage.cobertura.xml" \
    -targetdir:"$REPORTS_DIR" \
    -reporttypes:"Html;Cobertura" \
    -verbosity:Warning

# カバレッジ結果解析
COVERAGE_XML="$REPORTS_DIR/Cobertura.xml"
if [[ -f "$COVERAGE_XML" ]]; then
    # XMLからカバレッジ率抽出 (Python使用)
    COVERAGE_RESULT=$(python3 -c "
import xml.etree.ElementTree as ET
try:
    tree = ET.parse('$COVERAGE_XML')
    root = tree.getroot()
    line_rate = float(root.get('line-rate', 0)) * 100
    branch_rate = float(root.get('branch-rate', 0)) * 100
    print(f'{line_rate:.1f}|{branch_rate:.1f}')
except Exception as e:
    print('0.0|0.0')
")
    
    LINE_COVERAGE=$(echo "$COVERAGE_RESULT" | cut -d'|' -f1)
    BRANCH_COVERAGE=$(echo "$COVERAGE_RESULT" | cut -d'|' -f2)
    
    echo ""
    echo "📊 Coverage Results:"
    echo "  Line Coverage:   ${LINE_COVERAGE}%"
    echo "  Branch Coverage: ${BRANCH_COVERAGE}%"
    echo "  Target:          ${TARGET_COVERAGE}%"
    echo ""
    
    # 閾値チェック
    if (( $(echo "$LINE_COVERAGE >= $TARGET_COVERAGE" | bc -l) )); then
        echo "✅ Coverage target met! (${LINE_COVERAGE}% >= ${TARGET_COVERAGE}%)"
        EXIT_CODE=0
    else
        echo "❌ Coverage below target! (${LINE_COVERAGE}% < ${TARGET_COVERAGE}%)"
        EXIT_CODE=1
    fi
    
    echo "📂 HTML Report: file://$REPORTS_DIR/index.html"
    
    exit $EXIT_CODE
else
    echo "❌ カバレッジレポート生成に失敗しました"
    exit 1
fi