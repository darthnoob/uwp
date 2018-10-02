Universal Windows Platform MEGA app
===================================
A fully-featured client to access your Cloud Storage provided by MEGA.<br>
This repository contains all the development history of the official Universal Windows Platform MEGA app: https://mega.nz/#mobile

#### Target OS and supported devices
- Windows 10 Desktop (desktop, laptop and tactil PCs).
- Windows 10 Mobile (mobile and tablet).
- Windows 10 Team (surface hub).

#### Used 3rd party libraries and controls
You can see a detailed list at [CREDITS.md](CREDITS.md)

## Compilation
This document will guide you to build the application on a Windows machine with Microsoft Visual Studio.

#### Requirements
- Microsoft Windows 10 machine.
- Microsoft Visual Studio (at least Microsoft Visual Studio Express 2015 for Windows).

#### Preparation
1. Get the source code. Clone or donwload this repository.

2. Download the required third party libraries from this link: 
https://mega.nz/#!cwFTHQ7Q!Wz00uYeny6n3uYCQYoOITyK4UBhVgQ7O_3l1f47lc3Y

3. Uncompress that file into `uwp\MegaSDK\bindings\wp8`

4. Open Microsoft Visual Studio and open the solution file `uwp\MegaApp\MegaApp.sln`

5. Install the following NuGet packages from `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution` and add the needed references for the project:
   - `GoedWare.Controls.Breadcrumb`
   - `Microsoft.NETCore.UniversalWindowsPlatform`
   - `Microsoft.Toolkit.Uwp`
   - `Microsoft.Toolkit.Uwp.Notifications`
   - `Microsoft.Toolkit.Uwp.UI`
   - `Microsoft.Toolkit.Uwp.UI.Animations`
   - `Microsoft.Toolkit.Uwp.UI.Controls`
   - `Microsoft.Xaml.Behaviors.Uwp.Managed`
   - `SQLite.Net-PCL`
   - `WindowsStateTriggers`
   - `ZXing.Net`

6. Install the _"SQLite for Universal Windows Platform"_ from `Tools -> Extensions and Updates` and add the needed reference for the project.

7. Make sure the `MegaApp` project is selected as init or main project.

8. Build the project.

9. Enjoy!

If you want to build the third party dependencies by yourself, you can download the source code from this link:
https://mega.nz/#!EpNXUQZa!zeQ9_YIuDC1gXmHnQwONNv7Otm-hMMerkugeR2piFi0
