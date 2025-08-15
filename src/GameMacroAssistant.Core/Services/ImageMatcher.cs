using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Logging;
using GameMacroAssistant.Core.Models;

namespace GameMacroAssistant.Core.Services;

/// <summary>
/// 画像マッチングサービス
/// SSIMとピクセル差分による画像比較機能 (R-013)
/// </summary>
public class ImageMatcher
{
    private readonly ILogger<ImageMatcher> _logger;

    public ImageMatcher(ILogger<ImageMatcher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 画像マッチング実行 (R-013)
    /// </summary>
    /// <param name="sourceImage">検索対象画像</param>
    /// <param name="templateImage">テンプレート画像</param>
    /// <param name="settings">マッチング設定</param>
    /// <param name="searchArea">検索領域 (nullの場合は全体)</param>
    /// <returns>マッチング結果</returns>
    public ImageMatchResult FindMatch(
        byte[] sourceImage, 
        byte[] templateImage, 
        ImageMatchSettings settings,
        Rectangle? searchArea = null)
    {
        try
        {
            using var sourceBitmap = LoadBitmap(sourceImage);
            using var templateBitmap = LoadBitmap(templateImage);
            
            if (sourceBitmap == null || templateBitmap == null)
            {
                return new ImageMatchResult { IsMatch = false, ErrorMessage = "Invalid image data" };
            }

            // 検索領域の設定
            var actualSearchArea = searchArea ?? new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
            
            // テンプレートサイズ検証
            if (templateBitmap.Width > actualSearchArea.Width || 
                templateBitmap.Height > actualSearchArea.Height)
            {
                return new ImageMatchResult 
                { 
                    IsMatch = false, 
                    ErrorMessage = "Template size exceeds search area" 
                };
            }

            var bestMatch = FindBestMatch(sourceBitmap, templateBitmap, actualSearchArea, settings);
            
            _logger.LogDebug("Image matching completed. SSIM: {Ssim:F3}, PixelDiff: {PixelDiff:F3}, " +
                           "Threshold SSIM: {SsimThreshold:F3}, Threshold PixelDiff: {PixelThreshold:F3}",
                bestMatch.SsimScore, bestMatch.PixelDifferenceRatio, 
                settings.SsimThreshold, settings.PixelDifferenceThreshold);

            return bestMatch;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image matching failed");
            return new ImageMatchResult { IsMatch = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// 最適マッチ位置を検索
    /// </summary>
    private ImageMatchResult FindBestMatch(
        Bitmap source, 
        Bitmap template, 
        Rectangle searchArea, 
        ImageMatchSettings settings)
    {
        var bestResult = new ImageMatchResult { IsMatch = false, SsimScore = 0, PixelDifferenceRatio = 1.0 };

        // テンプレートマッチング: スライディングウィンドウで全位置を検索
        for (int y = searchArea.Y; y <= searchArea.Bottom - template.Height; y++)
        {
            for (int x = searchArea.X; x <= searchArea.Right - template.Width; x++)
            {
                var matchArea = new Rectangle(x, y, template.Width, template.Height);
                var result = CalculateMatch(source, template, matchArea, settings);
                
                // より良いマッチが見つかった場合
                if (result.SsimScore > bestResult.SsimScore)
                {
                    bestResult = result;
                    bestResult.MatchLocation = new Point(x, y);
                }
                
                // 完全マッチが見つかった場合は早期終了
                if (result.IsMatch && result.SsimScore >= 0.99)
                {
                    return result;
                }
            }
        }

        return bestResult;
    }

    /// <summary>
    /// 指定位置でのマッチング計算
    /// </summary>
    private ImageMatchResult CalculateMatch(
        Bitmap source, 
        Bitmap template, 
        Rectangle matchArea, 
        ImageMatchSettings settings)
    {
        // SSIM計算
        var ssimScore = CalculateSSIM(source, template, matchArea);
        
        // ピクセル差分計算
        var pixelDiffRatio = CalculatePixelDifference(source, template, matchArea);
        
        // 閾値判定 (R-013: デフォルト SSIM 0.95, ピクセル差 3%)
        var isMatch = ssimScore >= settings.SsimThreshold && 
                     pixelDiffRatio <= settings.PixelDifferenceThreshold;

        return new ImageMatchResult
        {
            IsMatch = isMatch,
            SsimScore = ssimScore,
            PixelDifferenceRatio = pixelDiffRatio,
            MatchLocation = new Point(matchArea.X, matchArea.Y),
            Confidence = ssimScore * (1.0 - pixelDiffRatio)
        };
    }

    /// <summary>
    /// SSIM (Structural Similarity Index) 計算
    /// </summary>
    private double CalculateSSIM(Bitmap source, Bitmap template, Rectangle area)
    {
        // TODO: 完全なSSIM実装
        // 現在は簡易版として正規化相関係数を使用
        return CalculateNormalizedCrossCorrelation(source, template, area);
    }

    /// <summary>
    /// 正規化相関係数による類似度計算 (SSIM簡易版)
    /// </summary>
    private double CalculateNormalizedCrossCorrelation(Bitmap source, Bitmap template, Rectangle area)
    {
        double sumSource = 0, sumTemplate = 0, sumProduct = 0;
        double sumSquareSource = 0, sumSquareTemplate = 0;
        int pixelCount = template.Width * template.Height;

        for (int y = 0; y < template.Height; y++)
        {
            for (int x = 0; x < template.Width; x++)
            {
                var sourcePixel = source.GetPixel(area.X + x, area.Y + y);
                var templatePixel = template.GetPixel(x, y);
                
                var sourceGray = GetGrayscaleValue(sourcePixel);
                var templateGray = GetGrayscaleValue(templatePixel);
                
                sumSource += sourceGray;
                sumTemplate += templateGray;
                sumProduct += sourceGray * templateGray;
                sumSquareSource += sourceGray * sourceGray;
                sumSquareTemplate += templateGray * templateGray;
            }
        }

        var meanSource = sumSource / pixelCount;
        var meanTemplate = sumTemplate / pixelCount;
        
        var numerator = sumProduct - pixelCount * meanSource * meanTemplate;
        var denominator = Math.Sqrt(
            (sumSquareSource - pixelCount * meanSource * meanSource) *
            (sumSquareTemplate - pixelCount * meanTemplate * meanTemplate)
        );

        return denominator > 0 ? numerator / denominator : 0;
    }

    /// <summary>
    /// ピクセル差分比率計算
    /// </summary>
    private double CalculatePixelDifference(Bitmap source, Bitmap template, Rectangle area)
    {
        int differentPixels = 0;
        int totalPixels = template.Width * template.Height;
        const int tolerance = 10; // RGB値の許容差

        for (int y = 0; y < template.Height; y++)
        {
            for (int x = 0; x < template.Width; x++)
            {
                var sourcePixel = source.GetPixel(area.X + x, area.Y + y);
                var templatePixel = template.GetPixel(x, y);
                
                if (GetColorDistance(sourcePixel, templatePixel) > tolerance)
                {
                    differentPixels++;
                }
            }
        }

        return (double)differentPixels / totalPixels;
    }

    /// <summary>
    /// グレースケール値取得
    /// </summary>
    private static double GetGrayscaleValue(Color color)
    {
        return 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
    }

    /// <summary>
    /// 色距離計算
    /// </summary>
    private static double GetColorDistance(Color c1, Color c2)
    {
        return Math.Sqrt(
            Math.Pow(c1.R - c2.R, 2) +
            Math.Pow(c1.G - c2.G, 2) +
            Math.Pow(c1.B - c2.B, 2)
        );
    }

    /// <summary>
    /// バイト配列からBitmap読み込み
    /// </summary>
    private static Bitmap? LoadBitmap(byte[] imageData)
    {
        try
        {
            using var stream = new MemoryStream(imageData);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// 画像マッチング結果
/// </summary>
public class ImageMatchResult
{
    /// <summary>
    /// マッチングが成功したかどうか
    /// </summary>
    public bool IsMatch { get; set; }
    
    /// <summary>
    /// SSIM スコア (0.0 - 1.0)
    /// </summary>
    public double SsimScore { get; set; }
    
    /// <summary>
    /// ピクセル差分比率 (0.0 - 1.0)
    /// </summary>
    public double PixelDifferenceRatio { get; set; }
    
    /// <summary>
    /// マッチした位置
    /// </summary>
    public Point? MatchLocation { get; set; }
    
    /// <summary>
    /// 総合信頼度 (0.0 - 1.0)
    /// </summary>
    public double Confidence { get; set; }
    
    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string? ErrorMessage { get; set; }
}