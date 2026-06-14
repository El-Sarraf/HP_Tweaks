using HP_Tweaks.CLI;
using Xunit;
namespace HP_Tweaks.Tests;

public class KeyCodeObtainerTests
{
    [Fact]
    public async Task KeyCodeObtainer_ShouldExtractCorrectCode_WhenValidKernerStreamIsPassed()
    {
        // Arrange
        string streamfile_path = @"/home/elsarraf/Development/HP-Tweaks/HP-Tweaks.Tests/fakestream.txt";
        string streamexample;
        if (File.Exists(streamfile_path))
        {

            streamexample = await File.ReadAllTextAsync(streamfile_path);

        }
        else
        {
            streamexample = "Event: time 1781180192.299130, type 4 (EV_MSC), code 4 (MSC_SCAN), value 68\n" + "Event: time 1781180192.299130, -------------- SYN_REPORT ------------\n" + "Event: time 1781180192.437232, type 4 (EV_MSC), code 4 (MSC_SCAN), value 68\n";

        }
        using var testStream = new StringReader(streamexample);
        string expectedScanCode = "68";
        // Act
        string actualScanCode = await Program.KeyCodeObtainer("Diamond Key(F12)", testStream);
        //Assert
        Assert.Equal(expectedScanCode, actualScanCode);

    }
    [Fact]
    public async Task KeyCodeObtainer_ShouldIgnoreEnterKey_WhenValueIs1c()
    {
        // Given
        string fakeOutputWithEnter = "Event: type 4 (EV_MSC), code 4 (MSC_SCAN), value 1c\n" + "Event: type 4 (EV_MSC), code 4 (MSC_SCAN), value f8\n";

        // When
        using var testStream = new StringReader(fakeOutputWithEnter);
        string expectedScanCode = "f8";
        // Act
        string actualScanCode = await Program.KeyCodeObtainer("Airplane Key(F11)", testStream);
        //Assert
        Assert.Equal(actualScanCode, expectedScanCode);
        // Then
    }
}
