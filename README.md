# HP EliteBook Linux Function Keys Tweaker

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Linux-Only-green?style=for-the-badge" />
  <img src="https://img.shields.io/badge/HP-EliteBook-blue?style=for-the-badge" />
  <img src="https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge" />
</p>

A Linux CLI utility that automatically detects and remaps the special **HP EliteBook programmable keys** (Diamond/F12 and Airplane/F11) using the Linux **udev hardware database (hwdb)**.

This project was created because many HP EliteBook laptops expose special hardware buttons that either do nothing under Linux or require manual configuration.

The tool automates the entire process:

* Detects keyboard scancodes automatically
* Installs required dependencies
* Creates the hwdb configuration
* Reloads udev rules
* Deploys an Airplane Mode toggle script
* Provides user-friendly terminal output

---

# Supported Devices

Currently tested on:

* HP EliteBook 845 G8

Other HP EliteBook models may work as well, but have not been fully tested.

---

# Features

✅ Automatic root privilege detection

✅ Automatic privilege elevation using pkexec

✅ Automatic evtest installation

✅ Automatic keyboard controller detection

✅ Captures special key scancodes

✅ Creates hwdb mappings automatically

✅ Reloads Linux hardware configuration

✅ Deploys Airplane Mode toggle script

✅ Desktop notifications for Airplane Mode changes

---

# What Gets Configured?

The tool maps:

| Key            | Action     |
| -------------- | ---------- |
| Diamond (F12)  | Play/Pause |
| Airplane (F11) | F14        |

The generated hwdb file is:

```text
/etc/udev/hwdb.d/90-hp-programmable-key.hwdb
```

Example:

```text
evdev:atkbd:dmi:*
 KEYBOARD_KEY_68=playpause
 KEYBOARD_KEY_69=f14
```

---

# Airplane Mode Script

The installer also deploys:

```text
~/.local/bin/toggle-airplane.sh
```

This script:

* Toggles Wi-Fi
* Toggles Bluetooth
* Sends desktop notifications
* Uses NetworkManager (nmcli)

Example notification:

```text
Airplane Mode
All radios disabled
```

---

# Requirements

* Linux
* .NET 8 Runtime
* systemd
* udev
* NetworkManager
* pkexec
* root privileges

---

# Installation

Clone the repository:

```bash
git clone https://github.com/YOUR_USERNAME/hp-elitebook-linux-function-tweaker.git
cd hp-elitebook-linux-function-tweaker
```

Build:

```bash
dotnet build -c Release
```

Run:

```bash
sudo dotnet run
```

or

```bash
sudo ./HP_Tweaks.CLI
```

---

# Usage

Launch the application:

```bash
sudo ./HP_Tweaks.CLI
```

The program will:

1. Verify root access
2. Install evtest if missing
3. Detect the keyboard device
4. Ask you to press Diamond (F12)
5. Ask you to press Airplane (F11)
6. Create the hwdb configuration
7. Reload udev rules
8. Deploy the Airplane Mode helper script

---

# Example Output

```text
=============================================
|         Author : Ahmed El Sarraf          |
|---HP EliteBook 845 G8 Functions Tweaker---|
|--------Airplane and Diamond keys--v1.0----|
=============================================

[+] Congrats you are root.
[+] Evtest is found and ready.

[i]-Please press your [Diamond Key (F12)] now...
[+]-Successfully captured Scancode for Diamond Key (F12): 68

[i]-Please press your [Airplane Key(F11)] now...
[+]-Successfully captured Scancode for Airplane Key(F11): 69

[+]-Hardware configuration file written successfully.
[+]-Hardware configuration reloaded successfully.
```

---

# Project Structure

```text
HP_Tweaks.CLI/
│
├── Program.cs
├── README.md
├── LICENSE
└── .gitignore
```

---

# Security Notice

This utility performs system-level changes:

* Writes to `/etc/udev/hwdb.d`
* Reloads udev rules
* Executes system commands
* Installs dependencies through apt
* Deploys user scripts

Review the source code before running if desired.

---

# Future Plans

* Support additional HP EliteBook models
* Support Fedora and Arch package managers
* GUI version
* Automatic GNOME shortcut creation
* Fingerprint button support
* User-selectable key mappings

---

# Contributing

Issues, suggestions, and pull requests are welcome.

If you own a different HP EliteBook model, feel free to open an issue and share your key scancodes.

---

# License

MIT License

---

Made with ❤️ by Ahmed El Sarraf
