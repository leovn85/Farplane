using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Binarysharp.MemoryManagement;
using Farplane.Properties;

namespace Farplane.Memory;

public static partial class GameMemory
{
    internal delegate void ProcessExitDelegate();
    internal static event ProcessExitDelegate ProcessExited;
    internal static MemorySharp MemorySharp { get; private set; }
    static Process _attachedProcess;

    public static bool IsAttached => MemorySharp != null && MemorySharp.IsRunning;
    public static Process Process => MemorySharp?.Native;
    public static string[] FFX_Hashes =
    [
        "3D13E5ED53821DF8DB204CD3C27D470C5C220B18E16849FC4440996C15F277E1", // FFX (original)
        "7B00191231BB04903F5A59A18FEEACFF718C1D0AAB038FBF966586EDF75E4E27", // FFX with 4GB patch
        "D7F08CDA89DBCD6F429814E9523CE7A63A094417442A025573AFBBEF3251EF20", // FFX-2
        "78CE34397DA5E6F49B72C2AEBADEDAF4CD3F6720E1949D46A1B8ED67D3DB5CED", // FFX (2026 Steam)
    ];

    internal static bool Attach(Process process)
    {
        if (MemorySharp != null)
        {
            Detach();
        }

        if (process == null || process.HasExited)
        {
            return false;
        }

        MemorySharp = new MemorySharp(process);
        var sigCheck = CheckProcessSignature();
        if (!sigCheck)
        {
            MemorySharp.Dispose();
            MemorySharp = null;
            return false;
        }

        CheckForUnX();

        // Đăng ký sự kiện Exited chuẩn của Process thay vì luồng polling Sleep(1000)
        _attachedProcess = process;
        try
        {
            _attachedProcess.EnableRaisingEvents = true;
            _attachedProcess.Exited += OnProcessExited;
        }
        catch
        {
            // Trường hợp không đủ quyền bật EnableRaisingEvents
        }

        if (_attachedProcess.HasExited)
        {
            Detach();
            return false;
        }

        return true;
    }

    static void OnProcessExited(object sender, EventArgs e)
    {
        try
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(Detach);
            }
            else
            {
                Detach();
            }
        }
        catch
        {
            Detach();
        }
    }

    internal static void Detach()
    {
        if (_attachedProcess != null)
        {
            try
            {
                _attachedProcess.Exited -= OnProcessExited;
            }
            catch { }
            _attachedProcess = null;
        }

        MemorySharp?.Dispose();
        MemorySharp = null;
        ProcessExited?.Invoke();
        OffsetScanner.Reset();
    }

    internal static bool CheckProcessSignature()
    {
        if (!IsAttached)
        {
            return false;
        }

        string stringHash;
        try
        {
            using var sha = SHA256.Create();
            var exePath = MemorySharp.Modules.MainModule.Path;
            var exeBytes = File.ReadAllBytes(exePath);
            var moduleHash = sha.ComputeHash(exeBytes);

            stringHash = moduleHash.Aggregate(
                new StringBuilder(),
                (sb, hb) => sb.Append(hb.ToString("X2")),
                sb => sb.ToString()
            );
        }
        catch (Exception ex)
        {
            var proceed = MessageBox.Show(
                $"An error occurred while validating the selected process:\n{ex.Message}\n\nWould you like to continue anyway?",
                "Process verification error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation
            );
            return proceed == MessageBoxResult.Yes;
        }

        if (!FFX_Hashes.Contains(stringHash))
        {
            Console.WriteLine("Hash: " + stringHash);
            var proceed = MessageBox.Show(
                "The selected process does not appear to match any known compatible processes.\n\n"
                    + "Farplane will attempt to calculate offsets automatically, but proceeding "
                    + "may cause errors or cause the game to crash. Now is a good time to make a "
                    + "backup of your saved games!\n\n"
                    + "Are you sure you wish to continue?",
                "Unknown process error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation
            );
            return proceed == MessageBoxResult.Yes;
        }
        return true;
    }

    internal static void CheckForUnX()
    {
        if (MemorySharp == null)
        {
            return;
        }
        var modules = MemorySharp.Modules.RemoteModules.ToList();
        var unxModule = modules.FirstOrDefault(module =>
            string.Compare(module.Name, "unx.dll", StringComparison.CurrentCultureIgnoreCase) == 0
        );
        if (unxModule != null && !Settings.Default.NeverShowUnXWarning)
        {
            MessageBox.Show(
                "Farplane has detected that Untitled Project X (unx.dll) is currently loaded.\n\n"
                    + "Some features of UnX, even when disabled, will overwrite changes made by Farplane, "
                    + "leading to unpredictable results.\n\n"
                    + "This warning message can be disabled from the Settings menu.",
                "UnX Warning",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }
    }

    public static T Read<T>(int offset, bool isRelative = true)
    {
        if (!IsAttached)
        {
            return default;
        }
        return MemorySharp.Read<T>((IntPtr)offset, isRelative);
    }

    public static T[] Read<T>(int offset, int count, bool isRelative = true)
    {
        if (!IsAttached)
        {
            return default;
        }
        return MemorySharp.Read<T>((IntPtr)offset, count, isRelative);
    }

    public static void Write<T>(int offset, T value, bool isRelative = true)
    {
        if (!IsAttached)
        {
            return;
        }
        MemorySharp.Write((IntPtr)offset, value, isRelative);
    }

    public static void Write<T>(int offset, T[] value, bool isRelative = true)
    {
        if (!IsAttached)
        {
            return;
        }
        MemorySharp.Write((IntPtr)offset, value, isRelative);
    }
}
