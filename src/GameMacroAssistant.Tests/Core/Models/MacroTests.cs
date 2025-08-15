using FluentAssertions;
using GameMacroAssistant.Core.Models;
using Xunit;

namespace GameMacroAssistant.Tests.Core.Models;

/// <summary>
/// Macro モデルのユニットテスト
/// マクロファイル構造とスキーマ準拠の検証 (R-027)
/// </summary>
public class MacroTests
{
    [Fact]
    public void Macro_DefaultConstructor_InitializesCorrectly()
    {
        // Act
        var macro = new Macro();

        // Assert
        macro.Id.Should().NotBeNullOrEmpty();
        macro.Name.Should().Be(string.Empty);
        macro.Description.Should().Be(string.Empty);
        macro.Version.Should().Be("1.0");
        macro.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        macro.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        macro.Steps.Should().NotBeNull().And.BeEmpty();
        macro.Settings.Should().NotBeNull();
        macro.IsEncrypted.Should().BeFalse();
    }

    [Fact]
    public void Macro_WithSteps_MaintainsStepOrder()
    {
        // Arrange
        var macro = new Macro();
        var step1 = new MouseStep { Position = new System.Drawing.Point(10, 10) };
        var step2 = new KeyboardStep { VirtualKeyCode = 65, Action = KeyAction.KeyDown }; // 'A'
        var step3 = new DelayStep { DelayMs = 1000 };

        // Act
        macro.Steps.Add(step1);
        macro.Steps.Add(step2);
        macro.Steps.Add(step3);

        // Assert
        macro.Steps.Should().HaveCount(3);
        macro.Steps[0].Should().BeOfType<MouseStep>();
        macro.Steps[1].Should().BeOfType<KeyboardStep>();
        macro.Steps[2].Should().BeOfType<DelayStep>();
    }

    [Fact]
    public void MacroSettings_DefaultValues_MatchRequirements()
    {
        // Act
        var settings = new MacroSettings();

        // Assert - R-013: デフォルト閾値確認
        settings.ImageMatch.SsimThreshold.Should().Be(0.95);
        settings.ImageMatch.PixelDifferenceThreshold.Should().Be(0.03);
        
        // R-005: デフォルト停止キー (ESC = 27)
        settings.StopRecordingKey.Should().Be(27);
        
        settings.GlobalHotkey.Should().BeNull();
    }

    [Fact]
    public void ImageMatchSettings_ThresholdValidation_AcceptsValidValues()
    {
        // Arrange
        var settings = new ImageMatchSettings();

        // Act & Assert - 有効な範囲のテスト
        settings.SsimThreshold = 0.8;
        settings.SsimThreshold.Should().Be(0.8);

        settings.SsimThreshold = 1.0;
        settings.SsimThreshold.Should().Be(1.0);

        settings.PixelDifferenceThreshold = 0.0;
        settings.PixelDifferenceThreshold.Should().Be(0.0);

        settings.PixelDifferenceThreshold = 0.1;
        settings.PixelDifferenceThreshold.Should().Be(0.1);
    }
}

/// <summary>
/// MacroStep 継承クラスのテスト
/// </summary>
public class MacroStepTests
{
    [Fact]
    public void MouseStep_Properties_SetCorrectly()
    {
        // Arrange & Act
        var step = new MouseStep
        {
            Position = new System.Drawing.Point(100, 200),
            Button = MouseButton.Left,
            PressedDurationMs = 150
        };

        // Assert
        step.Type.Should().Be(MacroStepType.Mouse);
        step.Position.X.Should().Be(100);
        step.Position.Y.Should().Be(200);
        step.Button.Should().Be(MouseButton.Left);
        step.PressedDurationMs.Should().Be(150);
        step.Id.Should().NotBeNullOrEmpty();
        step.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void KeyboardStep_Properties_SetCorrectly()
    {
        // Arrange & Act
        var step = new KeyboardStep
        {
            VirtualKeyCode = 13, // Enter key
            Action = KeyAction.KeyDown
        };

        // Assert
        step.Type.Should().Be(MacroStepType.Keyboard);
        step.VirtualKeyCode.Should().Be(13);
        step.Action.Should().Be(KeyAction.KeyDown);
    }

    [Fact]
    public void DelayStep_Properties_SetCorrectly()
    {
        // Arrange & Act
        var step = new DelayStep { DelayMs = 2000 };

        // Assert
        step.Type.Should().Be(MacroStepType.Delay);
        step.DelayMs.Should().Be(2000);
    }

    [Fact]
    public void ImageWaitStep_Properties_SetCorrectly()
    {
        // Arrange & Act
        var searchArea = new System.Drawing.Rectangle(50, 50, 200, 200);
        var step = new ImageWaitStep
        {
            ImagePath = "test_image.png",
            TimeoutMs = 10000,
            SearchArea = searchArea
        };

        // Assert
        step.Type.Should().Be(MacroStepType.ImageWait);
        step.ImagePath.Should().Be("test_image.png");
        step.TimeoutMs.Should().Be(10000);
        step.SearchArea.Should().Be(searchArea);
    }

    [Fact]
    public void ImageWaitStep_DefaultTimeout_IsReasonable()
    {
        // Act
        var step = new ImageWaitStep();

        // Assert
        step.TimeoutMs.Should().Be(5000, "Default timeout should be 5 seconds");
    }

    [Theory]
    [InlineData(MouseButton.Left)]
    [InlineData(MouseButton.Right)]
    [InlineData(MouseButton.Middle)]
    [InlineData(MouseButton.X1)]
    [InlineData(MouseButton.X2)]
    public void MouseButton_AllValues_AreSupported(MouseButton button)
    {
        // Arrange & Act
        var step = new MouseStep { Button = button };

        // Assert
        step.Button.Should().Be(button);
    }

    [Theory]
    [InlineData(KeyAction.KeyDown)]
    [InlineData(KeyAction.KeyUp)]
    public void KeyAction_AllValues_AreSupported(KeyAction action)
    {
        // Arrange & Act
        var step = new KeyboardStep { Action = action };

        // Assert
        step.Action.Should().Be(action);
    }

    [Fact]
    public void MacroStep_ScreenshotPath_CanBeSet()
    {
        // Arrange
        var step = new MouseStep();

        // Act
        step.ScreenshotPath = "screenshot_123.png";

        // Assert
        step.ScreenshotPath.Should().Be("screenshot_123.png");
    }

    [Fact]
    public void MacroStep_Id_IsUniquePerInstance()
    {
        // Act
        var step1 = new MouseStep();
        var step2 = new MouseStep();

        // Assert
        step1.Id.Should().NotBe(step2.Id);
    }
}