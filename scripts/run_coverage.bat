@echo off
REM Windows用カバレッジ測定スクリプト
REM 使用例: scripts\run_coverage.bat 80

setlocal enabledelayedexpansion

set TARGET_COVERAGE=80
set PROJECT_ROOT=%~dp0..
set COVERAGE_DIR=%PROJECT_ROOT%\coverage
set REPORTS_DIR=%COVERAGE_DIR%\report

REM パラメータ処理
if not "%1"=="" set TARGET_COVERAGE=%1

echo 🔍 Starting coverage analysis...
echo Target coverage: %TARGET_COVERAGE%%%
echo Project root: %PROJECT_ROOT%

REM カバレッジディレクトリ準備
if exist "%COVERAGE_DIR%" rmdir /s /q "%COVERAGE_DIR%"
mkdir "%COVERAGE_DIR%" 2>nul
mkdir "%REPORTS_DIR%" 2>nul

cd /d "%PROJECT_ROOT%"

REM テストプロジェクト検索
echo 📋 Searching for test projects...
set TEST_PROJECTS=
for /r %%i in (*.Tests.csproj *Test.csproj) do (
    echo Found: %%i
    set TEST_PROJECTS=!TEST_PROJECTS! "%%i"
)

if "%TEST_PROJECTS%"=="" (
    echo ⚠️  テストプロジェクトが見つかりません
    exit /b 1
)

REM テスト実行
echo 🧪 Running tests with coverage collection...
for %%p in (%TEST_PROJECTS%) do (
    echo Testing: %%p
    dotnet test %%p --configuration Release --collect:"XPlat Code Coverage" --results-directory "%COVERAGE_DIR%" --logger "console;verbosity=minimal"
    if errorlevel 1 (
        echo ❌ Tests failed for %%p
        exit /b 1
    )
)

REM カバレッジファイル確認
dir /s /b "%COVERAGE_DIR%\coverage.cobertura.xml" >nul 2>&1
if errorlevel 1 (
    echo ❌ カバレッジファイルが生成されませんでした
    echo パッケージ確認: dotnet add package coverlet.collector
    exit /b 1
)

REM ReportGenerator確認・インストール
reportgenerator --help >nul 2>&1
if errorlevel 1 (
    echo Installing ReportGenerator...
    dotnet tool install -g dotnet-reportgenerator-globaltool
)

REM HTMLレポート生成
echo 📝 Generating HTML coverage report...
reportgenerator -reports:"%COVERAGE_DIR%\**\coverage.cobertura.xml" -targetdir:"%REPORTS_DIR%" -reporttypes:"Html;Cobertura" -verbosity:Warning

REM カバレッジ結果確認
if exist "%REPORTS_DIR%\Cobertura.xml" (
    echo ✅ Coverage report generated successfully
    echo 📂 HTML Report: file:///%REPORTS_DIR%\index.html
    echo 📊 Check coverage results in the HTML report
) else (
    echo ❌ カバレッジレポート生成に失敗しました
    exit /b 1
)

endlocal