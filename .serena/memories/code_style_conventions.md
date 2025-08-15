# コードスタイル・規約

## C# コーディング規約

### 命名規則
- **クラス名**: PascalCase (例: `MacroRecorder`, `ScreenCaptureService`)
- **メソッド名**: PascalCase (例: `StartRecording`, `CaptureScreen`)
- **プロパティ名**: PascalCase (例: `IsRecording`, `CaptureInterval`)
- **フィールド名**: _camelCase (例: `_logger`, `_isInitialized`)
- **パラメータ名**: camelCase (例: `captureRegion`, `timeout`)
- **ローカル変数**: camelCase (例: `result`, `imageData`)

### ファイル・ディレクトリ命名
- **TaskID-機能名-詳細**: `T-001-core-models`
- **相対パス使用**: 絶対パス禁止（環境依存回避）
- **Windows パス区切り**: `\` または `/` （ツールに応じて統一）

### アーキテクチャ制約
- **疎結合設計**: レイヤー間依存性最小化
- **依存性注入**: Microsoft.Extensions.DependencyInjection使用
- **SOLID原則**: 単一責任、開放閉鎖、リスコフ置換、インターフェース分離、依存性逆転
- **インターフェース優先**: サービス層は必ずインターフェース定義

### .NET 8 / C# 12 機能
- **Nullable Reference Types**: 有効化必須
- **Global Usings**: 有効化
- **File Scoped Namespaces**: 推奨
- **Primary Constructors**: 適用可能な場合使用

### MVVM パターン
- **CommunityToolkit.Mvvm**: ObservableObject, RelayCommand使用
- **ViewModelLocator**: 依存性注入ベース
- **View-ViewModel分離**: コードビハインドは最小限

### エラーハンドリング
- **using文**: IDisposableパターン徹底
- **ConfigureAwait(false)**: ライブラリコードで使用
- **カスタム例外**: 適切な例外階層定義

### テスト規約
- **xUnit**: テストフレームワーク
- **Moq**: モッキングライブラリ
- **AAA パターン**: Arrange-Act-Assert
- **テストカバレッジ**: Core層 80%以上維持