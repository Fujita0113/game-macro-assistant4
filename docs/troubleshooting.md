# トラブルシューティングガイド

## 環境関連の問題

### DPIスケーリング制限事項 (R-022)

#### 既知の制限事項
- **100%以外のDPIスケーリング環境では座標精度に制限があります**
- Windows 11のディスプレイ設定で125%, 150%, 200%等に設定されている場合、マクロの座標が正確に再現されない場合があります

#### 対象環境
- Windows 11 (主要対象OS)
- マルチディスプレイ環境での異なるDPI設定
- 高DPI対応アプリケーション

#### 座標変換アルゴリズム（擬似コード）

```csharp
// 記録時の座標変換
Point RecordScreenCoordinate(Point physicalPoint)
{
    var dpiScale = GetSystemDpiScale();
    return new Point(
        x: (int)(physicalPoint.X / dpiScale.X),
        y: (int)(physicalPoint.Y / dpiScale.Y)
    );
}

// 再生時の座標変換
Point PlaybackScreenCoordinate(Point logicalPoint)
{
    var currentDpiScale = GetSystemDpiScale();
    return new Point(
        x: (int)(logicalPoint.X * currentDpiScale.X),
        y: (int)(logicalPoint.Y * currentDpiScale.Y)
    );
}

// DPIスケール取得
DpiScale GetSystemDpiScale()
{
    using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
    {
        return new DpiScale(
            X: graphics.DpiX / 96.0f,  // 96 DPI = 100%
            Y: graphics.DpiY / 96.0f
        );
    }
}
```

#### 回避策
1. **推奨**: 記録・再生時に同一のDPI設定を使用
2. マクロファイルにDPI情報を記録し、再生時に警告表示
3. 座標の相対位置での記録（将来のバージョンで対応予定）

---

## スクリーンキャプチャの問題

### Desktop Duplication API失敗 (R-006)

#### 症状
- `Err-CAP` エラーコードがログに記録される
- スクリーンショットが取得できない
- "CaptureLimited" ウォーターマークが表示される

#### 原因
1. **DirectX 11非対応環境**
2. **仮想マシン環境**
3. **リモートデスクトップ接続**
4. **専用GPU使用時のドライバー問題**

#### 対処法
```bash
# 1. DirectX診断ツールで確認
dxdiag

# 2. GPUドライバーの更新
# デバイスマネージャー > ディスプレイアダプター > ドライバーの更新

# 3. フォールバック動作の確認
# ログファイル確認: %APPDATA%\GameMacroAssistant\Logs\YYYY-MM-DD.log
```

### GDI BitBltフォールバック制限

#### 制限事項
- **最大15 FPS**での画面キャプチャ
- 半透明ウォーターマークの重畳
- パフォーマンス低下の可能性

#### パフォーマンス改善
1. 不要なウィンドウの最小化
2. 視覚効果の無効化
3. バックグラウンドアプリの終了

---

## パフォーマンス問題 (R-020)

### 高負荷状態の診断

#### 閾値超過時の症状
- **CPU使用率 > 15%**
- **メモリ使用量 > 300MB**
- "High Load" トースト通知
- 進捗バーが赤色表示

#### パフォーマンス監視コマンド

```powershell
# CPU使用率確認
Get-Counter "\Processor(_Total)\% Processor Time"

# メモリ使用量確認
Get-Process -Name "GameMacroAssistant*" | Select-Object ProcessName, WS, CPU

# パフォーマンスカウンター継続監視
typeperf "\Process(GameMacroAssistant*)\% Processor Time" "\Process(GameMacroAssistant*)\Working Set" -si 1
```

#### 対処法
1. **画面キャプチャ頻度の調整**
2. **複雑な画像マッチングの最適化**
3. **メモリリークの確認**
4. **バックグラウンドプロセスの確認**

---

## 画像マッチング精度の問題 (R-013)

### マッチング失敗の原因

#### 閾値設定の調整
```json
{
  "imageMatch": {
    "ssimThreshold": 0.95,      // より緩い: 0.90, より厳しい: 0.98
    "pixelDifferenceThreshold": 0.03  // より緩い: 0.05, より厳しい: 0.01
  }
}
```

#### 環境要因
1. **アニメーション効果**
2. **ウィンドウの透明効果**
3. **フォントレンダリングの違い**
4. **色深度の変更**

#### デバッグ方法
1. マッチング対象画像の保存確認
2. SSIM/ピクセル差分スコアのログ確認
3. 検索領域の調整

---

## ホットキー競合 (R-012)

### グローバルホットキー登録失敗

#### 症状
- ホットキーが反応しない
- 代替候補が提示される
- システムログにエラー記録

#### 競合チェック方法
```powershell
# アクティブなホットキー確認
Get-WmiObject -Class Win32_Process | Where-Object {$_.Name -like "*hotkey*"}

# レジストリでのホットキー確認
Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
```

#### 対処法
1. 提示された代替候補から選択
2. カスタムキーコンビネーションの設定
3. 他のアプリケーションとの競合確認

---

## ファイル暗号化の問題 (R-018)

### パスフレーズ認証失敗

#### エラーパターン
- **3回連続失敗**: 読み込みキャンセル
- **8桁未満**: パスフレーズ拒否
- **ファイル破損**: 復号化失敗

#### 復旧方法
```csharp
// パスフレーズ回復フロー（管理者向け）
try 
{
    var backup = LoadBackupMacro(macroId);
    if (backup != null) 
    {
        // バックアップからの復旧
        RestoreFromBackup(backup);
    }
}
catch (Exception ex)
{
    LogError($"Recovery failed: {ex.Message}");
    // 手動復旧が必要
}
```

---

## ログとエラーコード

### ログファイル場所
```
%APPDATA%\GameMacroAssistant\Logs\YYYY-MM-DD.log
```

### 主要エラーコード
- **Err-CAP**: スクリーンキャプチャ失敗
- **Err-TIM**: タイミング精度基準超過
- **Err-MATCH**: 画像マッチング失敗
- **Err-HOTKEY**: ホットキー登録失敗
- **Err-DECRYPT**: 復号化失敗

### サポート情報の収集
```bash
# システム情報収集
systeminfo > system_info.txt
dxdiag /t dxdiag_info.txt

# アプリケーションログ
copy "%APPDATA%\GameMacroAssistant\Logs\*.log" support_logs\

# 設定ファイル
copy "%APPDATA%\GameMacroAssistant\settings.json" support_logs\
```

---

## 技術サポート

### 報告時に必要な情報
1. **OS バージョン**: `winver` コマンド結果
2. **GPU 情報**: `dxdiag` 出力
3. **エラーログ**: 最新の日付のログファイル
4. **再現手順**: 具体的な操作手順
5. **マクロファイル**: 問題のある .gma.json（パスフレーズは除外）

### 連絡先
- **GitHub Issues**: https://github.com/GameMacroAssistant/GameMacroAssistant/issues
- **メール**: support@gamemacroassistant.dev
- **ドキュメント**: https://docs.gamemacroassistant.dev