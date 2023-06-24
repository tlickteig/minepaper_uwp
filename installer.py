from urllib.request import urlopen
import os
import subprocess, sys


def main():

    VERSION_NUMBER = "1.0.11.0"
    BASE_URL = f"https://raw.githubusercontent.com/tlickteig/minepaper_uwp/main/Packages/MinePaper_{VERSION_NUMBER}/"
    BASE_PATH = os.getenv("temp") + f"\MinePaper_{VERSION_NUMBER}"

    FOLDER_NAMES = ["Add-AppDevPackage.resources",
                    "Dependencies",
                    "TelemetryDependencies",
                    "Add-AppDevPackage.resources\\cs-CZ",
                    "Add-AppDevPackage.resources\\de-DE",
                    "Add-AppDevPackage.resources\\en-US",
                    "Add-AppDevPackage.resources\\es-ES",
                    "Add-AppDevPackage.resources\\fr-FR",
                    "Add-AppDevPackage.resources\\it-IT",
                    "Add-AppDevPackage.resources\\ja-JP",
                    "Add-AppDevPackage.resources\\ko-KR",
                    "Add-AppDevPackage.resources\\pl-PL",
                    "Add-AppDevPackage.resources\\pt-BR",
                    "Add-AppDevPackage.resources\\ru-RU",
                    "Add-AppDevPackage.resources\\tr-TR",
                    "Add-AppDevPackage.resources\\zh-CN",
                    "Add-AppDevPackage.resources\\zh-TW",
                    "Dependencies\\arm",
                    "Dependencies\\arm64",
                    "Dependencies\\Win32",
                    "Dependencies\\x64",
                    "Dependencies\\x86"]
    FILE_NAMES = ["Add-AppDevPackage.resources\\cs-CZ\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\de-DE\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\en-US\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\es-ES\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\fr-FR\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\it-IT\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\ja-JP\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\ko-KR\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\pl-PL\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\pt-BR\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\ru-RU\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\tr-TR\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\zh-CN\\Add-AppDevPackage.psd1",
                  "Add-AppDevPackage.resources\\zh-TW\\Add-AppDevPackage.psd1",
                  "Dependencies\\arm\\Microsoft.NET.Native.Framework.2.2.appx",
                  "Dependencies\\arm\\Microsoft.NET.Native.Runtime.2.2.appx",
                  "Dependencies\\arm\\Microsoft.UI.Xaml.2.8.appx",
                  "Dependencies\\arm\\Microsoft.VCLibs.ARM.14.00.appx",
                  "Dependencies\\arm\\Microsoft.VCLibs.ARM.14.00.Desktop.appx",
                  "Dependencies\\arm64\\Microsoft.NET.Native.Framework.2.2.appx",
                  "Dependencies\\arm64\\Microsoft.NET.Native.Runtime.2.2.appx",
                  "Dependencies\\arm64\\Microsoft.UI.Xaml.2.8.appx",
                  "Dependencies\\arm64\\Microsoft.VCLibs.ARM64.14.00.appx",
                  "Dependencies\\Win32\\Microsoft.UI.Xaml.2.8.appx",
                  "Dependencies\\x64\\Microsoft.NET.Native.Framework.2.2.appx",
                  "Dependencies\\x64\\Microsoft.NET.Native.Runtime.2.2.appx",
                  "Dependencies\\x64\\Microsoft.UI.Xaml.2.8.appx",
                  "Dependencies\\x64\\Microsoft.VCLibs.x64.14.00.appx",
                  "Dependencies\\x64\\Microsoft.VCLibs.x64.14.00.Desktop.appx",
                  "Dependencies\\x86\\Microsoft.NET.Native.Framework.2.2.appx",
                  "Dependencies\\x86\\Microsoft.NET.Native.Runtime.2.2.appx",
                  "Dependencies\\x86\\Microsoft.UI.Xaml.2.8.appx",
                  "Dependencies\\x86\\Microsoft.VCLibs.x86.14.00.appx",
                  "Dependencies\\x86\\Microsoft.VCLibs.x86.14.00.Desktop.appx",
                  "TelemetryDependencies\\LogSideloadingTelemetry.ps1",
                  "TelemetryDependencies\\Microsoft.Diagnostics.Tracing.EventSource.dll",
                  "TelemetryDependencies\\Microsoft.VisualStudio.RemoteControl.dll",
                  "TelemetryDependencies\\Microsoft.VisualStudio.Telemetry.dll",
                  "TelemetryDependencies\\Microsoft.VisualStudio.Utilities.Internal.dll",
                  "TelemetryDependencies\\Newtonsoft.Json.dll",
                  "TelemetryDependencies\\System.Runtime.CompilerServices.Unsafe.dll",
                  "Add-AppDevPackage.ps1",
                  "Install.ps1",
                  "MinePaper_1.0.11.0_ARM.appxsym",
                  "MinePaper_1.0.11.0_x64.appxsym",
                  "MinePaper_1.0.11.0_x86.appxsym",
                  "MinePaper_1.0.11.0_x86_x64_arm.cer",
                  "MinePaper_1.0.11.0_x86_x64_arm.msixbundle"]
    create_folder_if_not_exists(BASE_PATH)

    print("Downloading files...")
    for folder in FOLDER_NAMES:
        create_folder_if_not_exists(BASE_PATH + "\\" + folder)

    for file in FILE_NAMES:
        download_file(BASE_URL + file.replace("\\", "/"), BASE_PATH + "\\" + file)

    p = subprocess.Popen(["powershell.exe", f"{BASE_PATH}\\Install.ps1"], stdout=sys.stdout)
    p.communicate()


def create_folder_if_not_exists(folder_name):

    if not os.path.exists(folder_name):
        os.makedirs(folder_name)


def download_file(remote_path, local_path):

    with urlopen(remote_path) as file:
        content = file.read()

    with open(local_path, 'wb') as download:
        download.write(content)


main()