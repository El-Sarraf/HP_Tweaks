using Xunit;
using HP_Tweaks.CLI; 
namespace HP_Tweaks.Tests;

public class KeyboardDetectionTests
{
    [Fact]
    public void ParseEvtestOutput_WithStandardOutput_ShouldReturnCorrectEventPath()
    {
        // Arrange 
        string mockOutput = @"
Available devices:
/dev/input/event0:  Lid Switch
/dev/input/event4:  AT Translated Set 2 keyboard
/dev/input/event5:  HP Wireless Hotkeys
Select the device event number [0-5]: ";

        // Act 
        string result = Program.ParseEvtestOutput(mockOutput);

        // Assert 
        Assert.Equal("/dev/input/event4", result);
    }

    [Fact]
    public void ParseEvtestOutput_WithTabsAndExtraSpaces_ShouldStillMatchFlexibleRegex()
    {
        // Arrange random tabs or spaces test
        string mockOutput = "No Device\n/dev/input/event2:\t\tAT Translated Set 2 keyboard\nSelect number:";

        // Act
        string result = Program.ParseEvtestOutput(mockOutput);

        // Assert 
        Assert.Equal("/dev/input/event2", result);
    }

    [Fact]
    public void ParseEvtestOutput_WithLowerCaseName_ShouldMatchCaseInsensitive()
    {
        // Arrange lowercase situation
        string mockOutput = "/dev/input/event1:  at translated set 2 keyboard";

        // Act
        string result = Program.ParseEvtestOutput(mockOutput);

        // Assert 
        Assert.Equal("/dev/input/event1", result);
    }

    [Fact]
    public void ParseEvtestOutput_WithNoMatchingKeyboard_ShouldReturnEmptyString()
    {
        // Arrange No keyboard case
        string mockOutput = @"
/dev/input/event0:  HP HD Camera
/dev/input/event1:  PixArt USB Optical Mouse";

        // Act
        string result = Program.ParseEvtestOutput(mockOutput);

        // Assert
        Assert.Empty(result);
    }
}