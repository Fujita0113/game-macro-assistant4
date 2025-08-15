using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Drawing;
using System.Drawing.Imaging;
using GameMacroAssistant.Core.Models;
using GameMacroAssistant.Core.Services;
using Xunit;

namespace GameMacroAssistant.Tests.Core.Services;

/// <summary>
/// ImageMatcher のユニットテスト
/// SSIM・ピクセル差分による画像マッチング機能のテスト (R-013)
/// </summary>
public class ImageMatcherTests
{
    private readonly Mock<ILogger<ImageMatcher>> _mockLogger;
    private readonly ImageMatcher _imageMatcher;
    private readonly ImageMatchSettings _defaultSettings;

    public ImageMatcherTests()
    {
        _mockLogger = new Mock<ILogger<ImageMatcher>>();
        _imageMatcher = new ImageMatcher(_mockLogger.Object);
        _defaultSettings = new ImageMatchSettings
        {
            SsimThreshold = 0.95,
            PixelDifferenceThreshold = 0.03
        };
    }

    [Fact]
    public void FindMatch_IdenticalImages_ReturnsExactMatch()
    {
        // Arrange
        var imageData = CreateTestImageBytes(100, 100, Color.Red);

        // Act
        var result = _imageMatcher.FindMatch(imageData, imageData, _defaultSettings);

        // Assert
        result.IsMatch.Should().BeTrue();
        result.SsimScore.Should().BeGreaterThan(0.99);
        result.PixelDifferenceRatio.Should().BeLessThan(0.01);
        result.MatchLocation.Should().Be(new Point(0, 0));
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void FindMatch_SimilarImages_ReturnsMatchWithinThreshold()
    {
        // Arrange - わずかに異なる画像
        var sourceImage = CreateTestImageBytes(100, 100, Color.Red);
        var templateImage = CreateTestImageBytes(100, 100, Color.FromArgb(250, 0, 0)); // 少し暗い赤

        // Act
        var result = _imageMatcher.FindMatch(sourceImage, templateImage, _defaultSettings);

        // Assert
        result.IsMatch.Should().BeTrue("Similar colors should match within threshold");
        result.SsimScore.Should().BeGreaterThan(_defaultSettings.SsimThreshold);
        result.PixelDifferenceRatio.Should().BeLessThan(_defaultSettings.PixelDifferenceThreshold);
    }

    [Fact]
    public void FindMatch_DifferentImages_ReturnsNoMatch()
    {
        // Arrange
        var sourceImage = CreateTestImageBytes(100, 100, Color.Red);
        var templateImage = CreateTestImageBytes(100, 100, Color.Blue);

        // Act
        var result = _imageMatcher.FindMatch(sourceImage, templateImage, _defaultSettings);

        // Assert
        result.IsMatch.Should().BeFalse("Completely different colors should not match");
        result.SsimScore.Should().BeLessThan(_defaultSettings.SsimThreshold);
        result.PixelDifferenceRatio.Should().BeGreaterThan(_defaultSettings.PixelDifferenceThreshold);
    }

    [Fact]
    public void FindMatch_WithSearchArea_LimitsSearchToSpecifiedRegion()
    {
        // Arrange
        var sourceImage = CreateComplexTestImage(200, 200);
        var templateImage = CreateTestImageBytes(50, 50, Color.Blue);
        var searchArea = new Rectangle(75, 75, 100, 100); // 中央部分のみ検索

        // Act
        var result = _imageMatcher.FindMatch(sourceImage, templateImage, _defaultSettings, searchArea);

        // Assert
        if (result.IsMatch)
        {
            result.MatchLocation!.Value.X.Should().BeGreaterOrEqualTo(searchArea.X);
            result.MatchLocation.Value.Y.Should().BeGreaterOrEqualTo(searchArea.Y);
            result.MatchLocation.Value.X.Should().BeLessOrEqualTo(searchArea.Right - templateImage.Length);
            result.MatchLocation.Value.Y.Should().BeLessOrEqualTo(searchArea.Bottom);
        }
    }

    [Fact]
    public void FindMatch_TemplateLargerThanSearchArea_ReturnsError()
    {
        // Arrange
        var sourceImage = CreateTestImageBytes(100, 100, Color.Red);
        var templateImage = CreateTestImageBytes(150, 150, Color.Red); // より大きなテンプレート
        var searchArea = new Rectangle(0, 0, 100, 100);

        // Act
        var result = _imageMatcher.FindMatch(sourceImage, templateImage, _defaultSettings, searchArea);

        // Assert
        result.IsMatch.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Template size exceeds search area");
    }

    [Fact]
    public void FindMatch_InvalidImageData_ReturnsError()
    {
        // Arrange
        var validImage = CreateTestImageBytes(50, 50, Color.Red);
        var invalidImage = new byte[] { 0x00, 0x01, 0x02 }; // 無効な画像データ

        // Act
        var result = _imageMatcher.FindMatch(validImage, invalidImage, _defaultSettings);

        // Assert
        result.IsMatch.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid image data");
    }

    [Fact]
    public void FindMatch_CustomThresholds_RespectsSettings()
    {
        // Arrange - より厳しい閾値設定
        var strictSettings = new ImageMatchSettings
        {
            SsimThreshold = 0.99,
            PixelDifferenceThreshold = 0.001
        };
        
        var sourceImage = CreateTestImageBytes(50, 50, Color.Red);
        var slightlyDifferentImage = CreateTestImageBytes(50, 50, Color.FromArgb(250, 0, 0));

        // Act
        var result = _imageMatcher.FindMatch(sourceImage, slightlyDifferentImage, strictSettings);

        // Assert - 厳しい閾値では一致しない
        result.IsMatch.Should().BeFalse("Strict thresholds should reject slight differences");
    }

    [Fact]
    public void FindMatch_PerformanceTest_CompletesWithinReasonableTime()
    {
        // Arrange - 大きな画像でのパフォーマンステスト
        var sourceImage = CreateTestImageBytes(500, 500, Color.White);
        var templateImage = CreateTestImageBytes(50, 50, Color.Black);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = _imageMatcher.FindMatch(sourceImage, templateImage, _defaultSettings);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, 
            "Image matching should complete within reasonable time");
    }

    [Theory]
    [InlineData(0.90, 0.05)] // 緩い設定
    [InlineData(0.95, 0.03)] // デフォルト設定 (R-013)
    [InlineData(0.99, 0.01)] // 厳しい設定
    public void FindMatch_VariousThresholds_BehavesConsistently(double ssimThreshold, double pixelThreshold)
    {
        // Arrange
        var settings = new ImageMatchSettings
        {
            SsimThreshold = ssimThreshold,
            PixelDifferenceThreshold = pixelThreshold
        };
        var sourceImage = CreateTestImageBytes(100, 100, Color.Green);
        var templateImage = CreateTestImageBytes(100, 100, Color.Green);

        // Act
        var result = _imageMatcher.FindMatch(sourceImage, templateImage, settings);

        // Assert
        result.IsMatch.Should().BeTrue("Identical images should always match regardless of thresholds");
        result.SsimScore.Should().BeGreaterThan(ssimThreshold);
        result.PixelDifferenceRatio.Should().BeLessThan(pixelThreshold);
    }

    /// <summary>
    /// テスト用の単色画像バイト配列作成
    /// </summary>
    private static byte[] CreateTestImageBytes(int width, int height, Color color)
    {
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        using var brush = new SolidBrush(color);
        
        graphics.FillRectangle(brush, 0, 0, width, height);
        
        using var stream = new System.IO.MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    /// <summary>
    /// テスト用の複雑な画像作成（複数色を含む）
    /// </summary>
    private static byte[] CreateComplexTestImage(int width, int height)
    {
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        
        // 背景を白で塗りつぶし
        graphics.Clear(Color.White);
        
        // 中央に青い矩形
        using var blueBrush = new SolidBrush(Color.Blue);
        graphics.FillRectangle(blueBrush, width / 4, height / 4, width / 2, height / 2);
        
        // 角に赤い円
        using var redBrush = new SolidBrush(Color.Red);
        graphics.FillEllipse(redBrush, 10, 10, 30, 30);
        
        using var stream = new System.IO.MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }
}