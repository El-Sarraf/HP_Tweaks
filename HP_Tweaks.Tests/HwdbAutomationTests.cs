using Xunit;
using Xunit.Abstractions;
using HP_Tweaks.CLI;

namespace HP_Tweaks.Tests;

public class HwdbAutomationTests
{
    private readonly ITestOutputHelper _output;

    public HwdbAutomationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void BuildHwdbContent_ShouldGenerateCorrectFormat_WhenScancodesAreValid()
    {
        // Arrange
        string[] sampleScancodes = { "68", "f8" };

        // Act
        string generatedContent = Program.BuildHwdbContent(sampleScancodes);

        // Assert
        Assert.Contains("evdev:atkbd:dmi:*", generatedContent);
        Assert.Contains("KEYBOARD_KEY_68=playpause", generatedContent);
        Assert.Contains("KEYBOARD_KEY_f8=f14", generatedContent);

        // Print the file structure
        _output.WriteLine("=== [SUCCESS] Generated HWDB File Structure ===");
        _output.WriteLine(generatedContent);
        _output.WriteLine("===============================================");
    }

    [Fact]
    public void BuildAirplaneScriptContent_ShouldInjectCorrectSudoUser()
    {
        // Arrange
        string testUser = "elsarraf";

        // Act
        string scriptContent = Program.BuildAirplaneScriptContent(testUser);

        // Assert
        Assert.Contains("#!/bin/bash", scriptContent);
        Assert.Contains("id -u elsarraf", scriptContent);
        Assert.Contains("$NMCLI radio wifi", scriptContent);
        //print the script
        _output.WriteLine("=== [SUCCESS] Generated Bash Script Structure ===");
        _output.WriteLine(scriptContent);
        _output.WriteLine("=================================================");
    }
}