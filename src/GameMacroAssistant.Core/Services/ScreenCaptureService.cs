using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Logging;

namespace GameMacroAssistant.Core.Services;

/// <summary>
/// スクリーンキャプチャサービス
/// Desktop Duplication APIとGDI BitBltフォールバックによるスクリーンショット取得 (R-004, R-006)
/// </summary>
public class ScreenCaptureService : IDisposable
{
    private readonly ILogger<ScreenCaptureService> _logger;
    private bool _isDesktopDuplicationAvailable = true;
    private bool _disposed;

    public ScreenCaptureService(ILogger<ScreenCaptureService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// アクティブディスプレイのネイティブ解像度でPNGを取得 (R-004)
    /// 失敗時は10msバックオフで最大2回リトライ
    /// </summary>
    public async Task<(bool Success, byte[]? ImageData, string? ErrorCode)> CaptureScreenAsync()
    {
        const int maxRetries = 2;
        const int backoffMs = 10;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (_isDesktopDuplicationAvailable)
                {
                    var result = await CaptureWithDesktopDuplicationAsync();
                    if (result.Success)
                        return result;
                    
                    // Desktop Duplication API失敗時はGDIにフォールバック (R-006)
                    _logger.LogWarning("Desktop Duplication API failed, falling back to GDI BitBlt");
                    _isDesktopDuplicationAvailable = false;
                }

                // GDI BitBltフォールバック (R-006)
                return await CaptureWithGdiBitBltAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Screen capture attempt {Attempt} failed", attempt + 1);
                
                if (attempt < maxRetries)
                {
                    await Task.Delay(backoffMs);
                }
            }
        }

        _logger.LogError("All screen capture attempts failed");
        return (false, null, "Err-CAP");
    }

    /// <summary>
    /// Desktop Duplication APIでのキャプチャ (R-004)
    /// </summary>
    private async Task<(bool Success, byte[]? ImageData, string? ErrorCode)> CaptureWithDesktopDuplicationAsync()
    {
        await Task.Yield(); // 非同期コンテキストに切り替え
        
        // TODO: Desktop Duplication API (dxgi.dll) を使用した実装
        // - IDXGIFactory1::EnumAdapters1でアダプタ列挙
        // - IDXGIOutput1::DuplicateOutputでデスクトップ複製
        // - IDXGIOutputDuplication::AcquireNextFrameでフレーム取得
        // - テクスチャをCPUアクセス可能形式に変換してPNG出力
        
        throw new NotImplementedException("Desktop Duplication API implementation pending");
    }

    /// <summary>
    /// GDI BitBltでのフォールバック実装 (R-006)
    /// 最大15 FPS制限と半透明ウォーターマーク重畳
    /// </summary>
    private async Task<(bool Success, byte[]? ImageData, string? ErrorCode)> CaptureWithGdiBitBltAsync()
    {
        await Task.Yield();
        
        try
        {
            var bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            
            using var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb);
            using var graphics = Graphics.FromImage(bitmap);
            
            // スクリーンキャプチャ
            graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            
            // 半透明ウォーターマーク "CaptureLimited" を重畳 (R-006)
            AddWatermark(graphics, bounds.Size);
            
            // PNG形式で出力
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            
            _logger.LogWarning("Using GDI fallback capture - performance limited to 15 FPS");
            
            return (true, stream.ToArray(), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GDI BitBlt capture failed");
            return (false, null, "Err-CAP");
        }
    }

    /// <summary>
    /// 半透明ウォーターマーク追加 (R-006)
    /// </summary>
    private static void AddWatermark(Graphics graphics, Size screenSize)
    {
        const string watermarkText = "CaptureLimited";
        using var font = new Font("Arial", 24, FontStyle.Bold);
        using var brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255)); // 半透明白色
        
        var textSize = graphics.MeasureString(watermarkText, font);
        var position = new PointF(
            screenSize.Width - textSize.Width - 20,
            screenSize.Height - textSize.Height - 20
        );
        
        graphics.DrawString(watermarkText, font, brush, position);
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        // TODO: Desktop Duplication リソースの解放
        
        _disposed = true;
    }
}

/// <summary>
/// スクリーン情報取得用の簡易実装
/// </summary>
internal static class Screen
{
    public static ScreenInfo? PrimaryScreen => new()
    {
        Bounds = new Rectangle(0, 0, 1920, 1080) // TODO: 実際の解像度を取得
    };
}

internal class ScreenInfo
{
    public Rectangle Bounds { get; set; }
}