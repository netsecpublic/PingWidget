# Ping Monitor Widget

A lightweight, customizable, and floating desktop ping monitor for Windows. Built for system administrators, developers, and power users who need to keep a constant eye on server latency and uptime without cluttering their workspace.

## Features

* **Real-Time Monitoring:** Continuously pings multiple servers, hostnames, or IPv6 addresses and displays the latency (ms) directly on your desktop.
* **Visual Status Indicators:** 
  * 🟩 **Green:** Healthy / Below latency threshold.
  * 🟧 **Orange:** High Latency (exceeds your custom threshold for 3 consecutive pings).
  * 🟥 **Red:** Offline / Timeout (fails 3 consecutive pings).
* **Smart Alarms & Notifications:** Triggers audio alerts and system tray balloon popups when a server degrades or goes offline. 
* **Flashing System Tray:** The system tray icon aggressively flashes when an active, unmuted alarm is triggered, ensuring you never miss an outage.
* **Granular Muting:** Right-click any server to temporarily mute its alarms (1 min, 10 min, 1 hour, 24 hours, or forever). A bright `🔇` indicator displays while muted, completely silencing all popups and sounds for that specific server.
* **Dynamic, Unobtrusive UI:** 
  * Adjustable transparency with real-time preview.
  * "Always on Top" support.
  * Automatically resizes to fit long hostnames while keeping ping values perfectly aligned.
* **Quick Actions:** Right-click a server to instantly open a Command Prompt (`cmd.exe`) for quick troubleshooting.
* **Portable & Lightweight:** Compiles to a single, small `.exe` file that generates its `settings.json` in the same directory. 

## Installation

1. Download the latest `PingWidget.exe` from the Releases page.
2. Place the `.exe` in any folder (e.g., your Desktop).
3. Double-click to run. 
*(Note: If you do not have the .NET 8 Desktop Runtime installed, Windows will automatically prompt you to download it upon first launch).*

## Usage & Configuration

Once launched, PingWidget sits quietly on your desktop. 

* **Add/Manage Servers:** Right-click anywhere on the widget and select **Settings**. Here you can add new IPs/hostnames, set individual latency thresholds (ms), toggle alarms on/off per server, and use the **Up/Down** buttons to organize your list.
* **Adjust Appearance:** In Settings, drag the opacity slider to fade the widget into your background.
* **Mute an Alarm:** Right-click a specific server bar -> **Mute Alarm** -> Select your duration.
* **Minimize:** Right-click and select **Minimize to Tray** to hide the desktop widget while keeping background monitoring and tray alerts active.

## Built With

* C# / WPF
* .NET 8.0

## Building from Source

To compile a minimal, framework-dependent single-file executable, run the following from the Developer PowerShell in the project directory:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugSymbols=false -p:DebugType=None
