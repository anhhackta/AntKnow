using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Server Build Automation
/// Menu: Build → Build Dedicated Server
/// </summary>
public class ServerBuilder
{
    private const string SERVER_SCENE = "Assets/Scenes/GameScene.unity";

    [MenuItem("Build/Build Dedicated Server (Windows) 🪟")]
    public static void BuildWindowsServer()
    {
        BuildServer(BuildTarget.StandaloneWindows64, "Windows", ".exe");
    }

    [MenuItem("Build/Build Dedicated Server (Linux) 🐧")]
    public static void BuildLinuxServer()
    {
        BuildServer(BuildTarget.StandaloneLinux64, "Linux", "");
    }

    [MenuItem("Build/Build Linux Server for Multiplay 🚀")]
    public static void BuildLinuxServerForMultiplay()
    {
        Debug.Log("========== BUILDING LINUX SERVER FOR MULTIPLAY ==========");

        // Validate scene exists
        if (!File.Exists(SERVER_SCENE))
        {
            Debug.LogError($"❌ Scene not found: {SERVER_SCENE}");
            return;
        }

        // Build path for Multiplay
        string buildFolder = "Builds/LinuxServer";
        string buildPath = $"{buildFolder}/AntKnowServer.x86_64";

        // Ensure build folder exists
        Directory.CreateDirectory(buildFolder);

        // Build options for Unity 6 + Multiplay
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { SERVER_SCENE },
            locationPathName = buildPath,
            target = BuildTarget.StandaloneLinux64,
            subtarget = (int)StandaloneBuildSubtarget.Server, // Unity 6: Auto headless
            options = BuildOptions.Development // For debugging
        };

        Debug.Log($"Building to: {buildPath}");
        Debug.Log($"Target: Linux x86_64 Dedicated Server");
        Debug.Log($"Unity 6: Headless mode automatic");

        // Perform build
        var report = BuildPipeline.BuildPlayer(buildOptions);

        // Check result
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            long sizeInMB = (long)(report.summary.totalSize / (1024 * 1024));
            Debug.Log($"✅ LINUX SERVER BUILD SUCCEEDED!");
            Debug.Log($"📦 Size: {sizeInMB} MB");
            Debug.Log($"📁 Location: {Path.GetFullPath(buildPath)}");
            Debug.Log($"⏱️ Build time: {report.summary.totalTime.TotalSeconds:F1}s");

            // Create run script
            CreateLinuxRunScript(buildFolder);

            // Create Multiplay config
            CreateMultiplayConfig(buildFolder);

            // Create zip instructions
            CreateZipInstructions(buildFolder);

            // Open build folder
            EditorUtility.RevealInFinder(buildPath);

            Debug.Log($"🚀 READY FOR MULTIPLAY UPLOAD!");
        }
        else
        {
            Debug.LogError($"❌ LINUX SERVER BUILD FAILED!");
            Debug.LogError($"Errors: {report.summary.totalErrors}");
            Debug.LogError($"Warnings: {report.summary.totalWarnings}");
        }

        Debug.Log($"========== BUILD COMPLETE ==========\n");
    }

    [MenuItem("Build/Build Dedicated Server (Mac) 🍎")]
    public static void BuildMacServer()
    {
        BuildServer(BuildTarget.StandaloneOSX, "Mac", ".app");
    }

    [MenuItem("Build/Build All Servers (Win + Linux) 🌐")]
    public static void BuildAllServers()
    {
        Debug.Log("========== BUILDING ALL SERVERS ==========");
        BuildWindowsServer();
        BuildLinuxServer();
        Debug.Log("========== ALL SERVERS BUILT ==========");
    }

    private static void BuildServer(BuildTarget target, string platformName, string extension)
    {
        Debug.Log($"========== BUILDING {platformName.ToUpper()} SERVER ==========");

        // Validate scene exists
        if (!File.Exists(SERVER_SCENE))
        {
            Debug.LogError($"❌ Scene not found: {SERVER_SCENE}");
            return;
        }

        // Build path
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string buildFolder = $"Builds/Server_{platformName}_{timestamp}";
        string buildPath = $"{buildFolder}/AntKnowServer{extension}";

        // Ensure build folder exists
        Directory.CreateDirectory(buildFolder);

        // Build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { SERVER_SCENE },
            locationPathName = buildPath,
            target = target,
            subtarget = (int)StandaloneBuildSubtarget.Server, // Dedicated Server
            // Unity 6: EnableHeadlessMode removed, auto-enabled for Dedicated Server
            options = BuildOptions.Development
        };

        Debug.Log($"Building to: {buildPath}");
        Debug.Log($"Target: {target}");
        Debug.Log($"Options: Headless + Development");

        // Perform build
        var report = BuildPipeline.BuildPlayer(buildOptions);

        // Check result
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            long sizeInMB = (long)(report.summary.totalSize / (1024 * 1024));
            Debug.Log($"✅ {platformName} SERVER BUILD SUCCEEDED!");
            Debug.Log($"📦 Size: {sizeInMB} MB");
            Debug.Log($"📁 Location: {Path.GetFullPath(buildPath)}");
            Debug.Log($"⏱️ Build time: {report.summary.totalTime.TotalSeconds:F1}s");

            // Create run script
            CreateRunScript(buildFolder, platformName, extension);

            // Open build folder
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            Debug.LogError($"❌ {platformName} SERVER BUILD FAILED!");
            Debug.LogError($"Errors: {report.summary.totalErrors}");
            Debug.LogError($"Warnings: {report.summary.totalWarnings}");
        }

        Debug.Log($"========== BUILD COMPLETE ==========\n");
    }

    private static void CreateRunScript(string buildFolder, string platformName, string extension)
    {
        if (platformName == "Windows")
        {
            // Create .bat file
            string batPath = Path.Combine(buildFolder, "RunServer.bat");
            string batContent = @"@echo off
echo ========================================
echo   AntKnow Dedicated Server
echo ========================================
echo.
echo Starting server...
echo Press Ctrl+C to stop
echo.

AntKnowServer.exe -batchmode -nographics -logFile server.log

pause
";
            File.WriteAllText(batPath, batContent);
            Debug.Log($"✅ Created run script: {batPath}");
        }
        else if (platformName == "Linux")
        {
            // Create .sh file
            string shPath = Path.Combine(buildFolder, "run_server.sh");
            string shContent = @"#!/bin/bash
echo ""=======================================""
echo ""  AntKnow Dedicated Server""
echo ""=======================================""
echo """"
echo ""Starting server...""
echo ""Press Ctrl+C to stop""
echo """"

chmod +x AntKnowServer
./AntKnowServer -batchmode -nographics -logFile server.log
";
            File.WriteAllText(shPath, shContent);
            
            // Make executable (on Unix systems)
            if (Application.platform == RuntimePlatform.OSXEditor || 
                Application.platform == RuntimePlatform.LinuxEditor)
            {
                System.Diagnostics.Process.Start("chmod", $"+x {shPath}");
            }
            
            Debug.Log($"✅ Created run script: {shPath}");
        }

        // Create README
        string readmePath = Path.Combine(buildFolder, "README.txt");
        string readmeContent = $@"AntKnow Dedicated Server - {platformName}
========================================

HOW TO RUN:
-----------
{(platformName == "Windows" ? 
    "Double-click RunServer.bat" : 
    "Run: ./run_server.sh")}

Or manually:
{(platformName == "Windows" ? 
    "AntKnowServer.exe -batchmode -nographics -logFile server.log" : 
    "./AntKnowServer -batchmode -nographics -logFile server.log")}

SERVER SETTINGS:
----------------
Port: 7777
Max Players: 4
Target FPS: 30

LOGS:
-----
Server logs are written to: server.log
Use 'tail -f server.log' to monitor in real-time

FIREWALL:
---------
Make sure port 7777 is open:
{(platformName == "Windows" ? 
    "Windows Firewall → Allow app → Add AntKnowServer.exe" : 
    "sudo ufw allow 7777")}

CLOUD DEPLOYMENT:
-----------------
1. Upload this folder to your server
2. Open port 7777 in cloud firewall
3. Run the server
4. Clients connect to: <SERVER_IP>:7777

Built: {System.DateTime.Now}
";
        File.WriteAllText(readmePath, readmeContent);
        Debug.Log($"✅ Created README: {readmePath}");
    }

    [MenuItem("Build/Clean Old Builds 🧹")]
    public static void CleanOldBuilds()
    {
        string buildsFolder = "Builds";
        if (Directory.Exists(buildsFolder))
        {
            var dirs = Directory.GetDirectories(buildsFolder);
            int count = 0;
            
            foreach (var dir in dirs)
            {
                if (dir.Contains("Server_"))
                {
                    Directory.Delete(dir, true);
                    count++;
                }
            }
            
            Debug.Log($"✅ Cleaned {count} old server builds");
        }
    }

    [MenuItem("Build/Open Builds Folder 📁")]
    public static void OpenBuildsFolder()
    {
        string buildsFolder = "Builds";
        if (!Directory.Exists(buildsFolder))
        {
            Directory.CreateDirectory(buildsFolder);
        }
        EditorUtility.RevealInFinder(buildsFolder);
    }

    // Helper methods for Multiplay build
    private static void CreateLinuxRunScript(string buildFolder)
    {
        string shPath = Path.Combine(buildFolder, "run_server.sh");
        string shContent = @"#!/bin/bash
echo ""=========================================""
echo ""  AntKnow Dedicated Server - Linux""
echo ""=========================================""
echo """"

# Make executable
chmod +x AntKnowServer.x86_64

# Run server
./AntKnowServer.x86_64 \
  -batchmode \
  -nographics \
  -logFile server.log \
  -port 7777

echo """"
echo ""Server stopped""
";
        File.WriteAllText(shPath, shContent);
        Debug.Log($"✅ Created run script: {shPath}");
    }

    private static void CreateMultiplayConfig(string buildFolder)
    {
        string configPath = Path.Combine(buildFolder, "build_config.json");
        string configContent = @"{
  ""buildName"": ""AntKnow Server"",
  ""buildVersion"": ""1.0.0"",
  ""executable"": ""AntKnowServer.x86_64"",
  ""queryType"": ""none"",
  ""binaryPath"": ""AntKnowServer.x86_64"",
  ""commandLine"": ""-batchmode -nographics -logFile server.log -port 7777""
}";
        File.WriteAllText(configPath, configContent);
        Debug.Log($"✅ Created Multiplay config: {configPath}");
    }

    private static void CreateZipInstructions(string buildFolder)
    {
        string readmePath = Path.Combine(buildFolder, "UPLOAD_TO_MULTIPLAY.txt");
        string readmeContent = @"🚀 UPLOAD TO UNITY MULTIPLAY
========================================

BƯỚC 1: ZIP BUILD FOLDER
-------------------------
Windows:
  Right-click folder → Send to → Compressed (zipped) folder
  Name: AntKnowServer_Linux_v1.0.0.zip

Linux/Mac:
  cd Builds
  zip -r AntKnowServer_Linux_v1.0.0.zip LinuxServer/

BƯỚC 2: UPLOAD TO MULTIPLAY
----------------------------
1. Mở: https://dashboard.unity3d.com/
2. Select Project
3. Multiplay → Builds → Upload Build
4. Upload: AntKnowServer_Linux_v1.0.0.zip
5. Wait for upload (~5-10 minutes)

BƯỚC 3: CONFIGURE BUILD
------------------------
Executable Path: AntKnowServer.x86_64
Command Line: -batchmode -nographics -logFile server.log -port 7777
Query Type: None
Server Type: Linux

BƯỚC 4: CREATE FLEET
---------------------
Fleet Name: AntKnow Production
Build: AntKnow Server v1.0.0
Regions: Asia Southeast (Singapore)
Min Servers: 1
Max Servers: 10
Players per Server: 4
Machine Type: 1 vCPU, 2GB RAM

BƯỚC 5: DEPLOY
--------------
Fleet → Deploy
Wait for deployment (~5-10 minutes)
Status: Active ✅

DONE! 🎉
Server IP: Check Multiplay Dashboard
Port: 7777
";
        File.WriteAllText(readmePath, readmeContent);
        Debug.Log($"✅ Created upload instructions: {readmePath}");
    }
}

