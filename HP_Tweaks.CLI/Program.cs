using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace HP_Tweaks.CLI;

public class Program
{
    private const string HwdbFilePath = "/etc/udev/hwdb.d/90-hp-programmable-key.hwdb";

    static async Task Main(string[] args)
    {
        DisplayHeader();

        // Root Access Checkup
        if (!CheckForRootAccess())
        {
            Error("Please Run the app as root");
            Info("Testing root access request.");
            RequestingRootAccess();
            return;
        }
        else
        {
            Success("Congrats you are root.");
        }

        // Installing required dependency (evtest).
        if (!CheckForDependencies())
        {
            Warn("evtest not found in /usr/sbin or /usr/bin. Installing now...");
            InstallDependencies();
        }
        else
        {
            Success("Evtest is found and ready.");
        }

        // Starting evtest
        Info("Please press your [Diamond / F12] key now to capture its scancode...");
        var kbdpath = DiscoverKeyboardEventPath();
        if (string.IsNullOrEmpty(kbdpath))
        {
            Error("Couldn't find a valid keyboard device path.");
            return;
        }
        var evtestInfo = new ProcessStartInfo
        {
            FileName = "evtest",

            Arguments = kbdpath,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        string[] capturedCodes = new string[2];
        string[] keysToCapture = { "Diamond Key (F12)", "Airplan Key(F11)" };
        for (int i = 0; i < keysToCapture.Length; i++)
        {
            Info($"Please press your [{keysToCapture[i]}] now to capture its scancode..");

            try
            {
                using var evtestProcess = Process.Start(evtestInfo);

                if (evtestProcess != null)
                {
                    string scancode = await KeyCodeObtainer(keysToCapture[i], evtestProcess.StandardOutput);
                    if (!evtestProcess.HasExited)
                    {
                        evtestProcess.Kill();
                    }

                    if (!string.IsNullOrEmpty(scancode))
                    {
                        Success($"Successfully captured Scancode for {keysToCapture[i]}: {scancode}");

                        capturedCodes[i] = scancode;


                        // if (RegisterKeysOnSystem(HwdbFilePath, codes) == 0)
                        // {
                        //     RestartTheHardware();
                        // }
                    }
                    else
                    {
                        Error($"Failed to capture a valid scancode for {keysToCapture[i]}.");
                        return;
                    }
                }
                else
                {
                    Error("Failed to start evtest process.");
                }

            }
            catch (Exception ex)
            {
                Error($"An error occurred during hardware monitoring: {ex.Message}");
            }

        } // end of for loop
        if (RegisterKeysOnSystem(HwdbFilePath, capturedCodes) == 0)
        {
            RestartTheHardware();
            DeployAirplanScript();
        }

    }

    public static void DeployAirplanScript()
    {
        string? realUser = Environment.GetEnvironmentVariable("SUDO_USER");
        if (string.IsNullOrEmpty(realUser) || realUser == "root")
        {
            realUser = Environment.UserName;
        }

        string userHome = $"/home/{realUser}";
        string binFolder = Path.Combine(userHome, ".local", "bin");
        string scriptPath = Path.Combine(binFolder, "toggle-airplane.sh");

        Info($"Deploying Airplane mode toggle script for user [{realUser}]...");

        try
        {
            if (!Directory.Exists(binFolder))
            {
                Directory.CreateDirectory(binFolder);
            }

            string content = BuildAirplaneScriptContent(realUser);
            File.WriteAllText(scriptPath, content);

            // إعطاء الصلاحيات للسكريبت ونقل ملكيته للمستخدم
            var chmodInfo = new ProcessStartInfo { FileName = "chmod", Arguments = $"+x {scriptPath}", UseShellExecute = false, CreateNoWindow = true };
            Process.Start(chmodInfo)?.WaitForExit();

            var chownInfo = new ProcessStartInfo { FileName = "chown", Arguments = $"{realUser}:{realUser} {scriptPath}", UseShellExecute = false, CreateNoWindow = true };
            Process.Start(chownInfo)?.WaitForExit();

            Success($"Script deployed successfully at: {scriptPath}");
            Info($"Shortcut setup note: Bind your 'Launch5' (F11/Airplane) key via GNOME to trigger this path.");
        }
        catch (Exception ex)
        {
            Error($"Failed to deploy bash script: {ex.Message}");
        }
    }

    public static string BuildAirplaneScriptContent(string realUser)
    {
        var scriptContent = new System.Text.StringBuilder();
        scriptContent.AppendLine("#!/bin/bash");
        scriptContent.AppendLine("export DISPLAY=:0");
        scriptContent.AppendLine($"export DBUS_SESSION_BUS_ADDRESS=\"unix:path=/run/user/$(id -u {realUser})/bus\"");
        scriptContent.AppendLine();
        scriptContent.AppendLine("NMCLI=\"/usr/bin/nmcli\"");
        scriptContent.AppendLine("NOTIFY=\"/usr/bin/notify-send\"");
        scriptContent.AppendLine();
        scriptContent.AppendLine("if $NMCLI radio wifi | grep -q \"enabled\"; then");
        scriptContent.AppendLine("    $NMCLI radio all off");
        scriptContent.AppendLine("    $NOTIFY -i network-wireless-disconnected \"Airplane Mode\" \"All radios disabled\"");
        scriptContent.AppendLine("else");
        scriptContent.AppendLine("    $NMCLI radio all on");
        scriptContent.AppendLine("    $NOTIFY -i network-wireless \"Airplane Mode\" \"All radios enabled\"");
        scriptContent.AppendLine("fi");

        return scriptContent.ToString();
    }

    // Print and UI methods
    public static void DisplayHeader()
    {
        //System.Console.Clear();
        //Console.ForegroundColor = ConsoleColor.Blue;
        System.Console.WriteLine("=============================================");
        System.Console.WriteLine("|-------------------------------------------|");
        System.Console.WriteLine("|         Author : Ahmed El Sarraf          |");
        System.Console.WriteLine("|-------------------------------------------|");
        System.Console.WriteLine("|---HP EliteBook 845 G8 Functions Tweaker---|");
        System.Console.WriteLine("|--------Airplane and Diamond keys--v1.0----|");
        System.Console.WriteLine("=============================================");
        Console.ResetColor();

    }
    public static void Info(string msg)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        System.Console.WriteLine($"[i]-{msg}");
        Console.ResetColor();
    }
    public static void Warn(string msg)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        System.Console.WriteLine($"[!]-{msg}");
        Console.ResetColor();


    }
    public static void Error(string msg)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        System.Console.WriteLine($"[x]-{msg}");
        Console.ResetColor();

    }
    public static void Success(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"[+]-{msg}");
        Console.ResetColor();

    }

    public static bool CheckForDependencies()
    {
        // evtest is the fundamental service for detecting the key code.
        return File.Exists("/usr/bin/evtest") || File.Exists("/usr/sbin/evtest");
    }
    public static bool CheckForRootAccess()
    {
        //Check is the user running the app using sudo or not.
        // The Easy way
        // return Environment.UserName == "root";

        // The Advanced way
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            [DllImport("libc")]
            static extern uint getuid();
            return getuid() == 0;
        }
        return false;
        //return false
    }
    public static bool RequestingRootAccess()
    {
        Info("Trying to request root access from the user.");
        try
        {
            string currentApp = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.GetCommandLineArgs()[0];
            var startInfo = new ProcessStartInfo
            {
                FileName = "pkexec",
                Arguments = currentApp,
                UseShellExecute = false

            };
            using var elevatedProcess = Process.Start(startInfo);
            elevatedProcess?.WaitForExit(TimeSpan.FromSeconds(5));
            Environment.Exit(elevatedProcess?.ExitCode ?? 0);
            return true;
        }
        catch (System.Exception ex)
        {

            Error($"Unable to elevate to root access :{ex.Message}");
            return false;
        }



    }
    public static void InstallDependencies()
    {
        // Installing the required dependency "evtest" here.
        Info("Trying to update apt via apt update");
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "apt",
                Arguments = "update",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            process?.WaitForExit(TimeSpan.FromSeconds(5));
            if (process != null && process.ExitCode == 0)
            {
                Success("apt successfully updated");
            }
            else
            {
                Error($"apt update failed with exit code: {process?.ExitCode}. (Are you root?)");
                return;
            }


        }
        catch (System.Exception ex)
        {

            Error($"Failed to update apt automatically: {ex.Message}");
        }
        // Installing the required dependency "evtest" here.
        Info("Trying to install evtest via apt install evtest -y");
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "apt",
                Arguments = "install evtest -y",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            process?.WaitForExit(TimeSpan.FromSeconds(5));
            if (process != null && process.ExitCode == 0)
            {
                Success("evtest successfully installed");

            }
            else
            {
                Error($"evtest installation failed with exit code: {process?.ExitCode}.");
            }

        }
        catch (System.Exception ex)
        {

            Error($"Failed to install evtest automatically: {ex.Message}");
        }


    }
    public static string DiscoverKeyboardEventPath()
    {
        Info("Scanning system profiles for the primary hardware keyboard controller...");
        // the /bin/bash -c "echo '' | evtest" # a hack to open evtest and get the results and close it immediatly via pressing enter ('' = empty stream followed by \n in bash)
        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = "-c \"echo '' | evtest\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process == null) return string.Empty;
            string stdOutput = process.StandardOutput.ReadToEnd();
            string stdError = process.StandardError.ReadToEnd();

            string menuOutput = stdOutput + stdError;
            process.WaitForExit(TimeSpan.FromSeconds(5));

            string detectedPath = ParseEvtestOutput(menuOutput);
            if (!string.IsNullOrEmpty(detectedPath))
            {
                Success($"Successfully Detected Keyboard Controller:{detectedPath}");
                return detectedPath;
            }
        }
        catch (System.Exception ex)
        {
            Error($"Error during keyboard detection :{ex.Message}");

        }
        if (File.Exists("/dev/input/by-path/platform-i8042-serio-0-event-kbd"))
        {
            return "/dev/input/by-path/platform-i8042-serio-0-event-kbd";
        }

        return string.Empty;
    }
    public static string ParseEvtestOutput(string menuOutput)
    {
        if (string.IsNullOrEmpty(menuOutput)) return string.Empty;
        var match = Regex.Match(menuOutput, @"(/dev/input/event[0-9]+):.*AT Translated Set 2 keyboard", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
    public static string BuildHwdbContent(string[] keyCodes)
    {
        if (keyCodes.Length < 2 || string.IsNullOrEmpty(keyCodes[0]) || string.IsNullOrEmpty(keyCodes[1]))
        {
            throw new ArgumentException("Both Diamond and Airplane scancodes must be provided.");
        }

        var hwdbContent = new System.Text.StringBuilder();
        hwdbContent.AppendLine("evdev:atkbd:dmi:*");
        hwdbContent.AppendLine($" KEYBOARD_KEY_{keyCodes[0].ToLower().Trim()}=playpause");
        hwdbContent.AppendLine($" KEYBOARD_KEY_{keyCodes[1].ToLower().Trim()}=f14");

        return hwdbContent.ToString();
    }
    public static int RegisterKeysOnSystem(string dir, string[] keyCodes)
    {
        Info($"Writing hardware configuration to {dir}...");
        try
        {
            if (keyCodes.Length == 0 || string.IsNullOrEmpty(keyCodes[0]))
            {
                Error("No scancodes provided to register.");
                return -1;
            }
            else if (keyCodes.Length > 2)
            {
                Error("only 2 key codes allowed (nice try)");
                return -1;
            }

            // Building the right config for the hp elitebook 845 g8
            var hwdbContent = BuildHwdbContent(keyCodes);
            File.WriteAllText(dir, hwdbContent.ToString());
            Success("Hardware configuration file written successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Error($"Failed to write system config file: {ex.Message}");
            return -1;
        }
    }

    public static void RestartTheHardware()
    {
        //Restart the hardware using the following command
        //sudo systemd-hwdb update && sudo udevadm control --reload && sudo udevadmReload trigger --subsystem-match=input 
        Info("Reloading Linux hardware configuration database (udev)...");
        Info("Executing : sudo systemd-hwdb update && sudo udevadm control --reload && sudo udevadm trigger --subsystem-match=input ");
        try
        {
            //sudo systemd-hwdb update which rebuild the hardware database.
            var hwdbUpdate = new ProcessStartInfo { FileName = "systemd-hwdb", Arguments = "update", UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
            using var p1 = Process.Start(hwdbUpdate) ?? throw new Exception("Failed to start systemd-hwdb update");
            string error_p1 = p1?.StandardError.ReadToEnd() ?? "";
            p1?.WaitForExit(TimeSpan.FromSeconds(5));

            if (p1?.ExitCode != 0)
            {
                throw new Exception(string.IsNullOrWhiteSpace(error_p1) ? $"systemd-hwdb update failed with exit code {p1?.ExitCode}" : $"systemd-hwdb update failed:{error_p1}");

            }

            //sudo udevadm control --reload which reads the new modifications
            var udevadmReload = new ProcessStartInfo { FileName = "udevadm", Arguments = "control --reload", UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
            using var p2 = Process.Start(udevadmReload) ?? throw new Exception("Failed to start udevadm control --reload");
            string error_p2 = p2?.StandardError.ReadToEnd() ?? "";
            p2?.WaitForExit(TimeSpan.FromSeconds(5));
            if (p2?.ExitCode != 0)
            {
                throw new Exception(string.IsNullOrWhiteSpace(error_p2) ? $"udevadm control --reload failed with exit code:{p2?.ExitCode}" : $"udevadm control --reload failed:{error_p2}");
            }

            //sudo udevadm trigger --subsystem-match=input which apply the new modifications instantly for input devices
            var udevadmTriggerReload = new ProcessStartInfo { FileName = "udevadm", Arguments = "trigger --subsystem-match=input", UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
            using var p3 = Process.Start(udevadmTriggerReload) ?? throw new Exception("Failed to start udevadm trigger --subsystem-match=input");
            string error_p3 = p3?.StandardError.ReadToEnd() ?? "";
            p3?.WaitForExit(TimeSpan.FromSeconds(5));
            if (p3?.ExitCode != 0)
            {
                throw new Exception(string.IsNullOrWhiteSpace(error_p3) ? $"udevadm trigger --subsystem-match=input failed with exit code:{p3?.ExitCode}" : $"udevadm trigger --subsystem-match=input failed:{error_p3}");
            }

            Success("Hardware configuration reloaded successfully.");
        }
        catch (System.Exception ex)
        {
            Error($"Failed reloading hardware:{ex.Message}");
        }


    }
    // The Core function that fetches the key from evtest and detects its code
    /// <summary>
    /// A function that opens evtest app and allow the user to press the key to detect it's code value
    /// </summary>
    /// <param name="keyName">The key name that the user will be warned about.</param>
    /// <param name="inputStream"> Evtest compatible stream output </param>
    /// <returns>Key code ex:68 as a string</returns>
    public static async Task<string> KeyCodeObtainer(string keyName, TextReader inputStream)
    {
        // A sample data stream of evtest
        //Event: time 1781180195.304419, -------------- SYN_REPORT ------------
        //Event: time 1781180195.383773, type 4 (EV_MSC), code 4 (MSC_SCAN), value 68

        // Scancode holder
        string capturedScancode = string.Empty;
        // read the stream and pull the key code - using modern pattern matching
        // if the input stream content is not null define it as line , can be written like :
        // string line; while ((line = await inputStream.ReadLineAsync()) != null)
        while (await inputStream.ReadLineAsync() is { } line)
        {
            if (line.Contains("MSC_SCAN") && line.Contains("value"))
            {
                // splitting the line before the word "value " and selecting the 2nd half then cleaning the spaces.
                string parsedValue = line.Split("value ")[1].Trim();
                //if it contains a value and that value is not Enter button code (1c)
                if (!string.IsNullOrEmpty(parsedValue) && parsedValue != "1c")
                {
                    capturedScancode = parsedValue;
                    break;

                }
            }
        }

        return capturedScancode;
    }



}
