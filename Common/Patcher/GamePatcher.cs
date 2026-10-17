using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Farplane.Common.Patcher;

public class GamePatcherStatus
{
    public string ExePath { get; set; }
    public string ExeName { get; set; }
    public bool Exists { get; set; }
    public long FileSize { get; set; }
    public bool Is4GBEnabled { get; set; }
    public bool IsAudioUndubbed { get; set; }
    public bool? IsFarplaneFixed { get; set; } // null if not applicable or unrecognized build
    public bool HasOriginalBackup { get; set; }
}

public class FarplanePatch
{
    public int Version { get; }
    public int Offset { get; }
    public byte[] Old { get; }
    public byte[] New { get; }

    public FarplanePatch(int version, int offset, byte[] oldBytes, byte[] newBytes)
    {
        this.Version = version;
        this.Offset = offset;
        this.Old = oldBytes;
        this.New = newBytes;
    }
}

public class FarplaneBuild
{
    public long Size { get; }
    public FarplanePatch[] Patches { get; }

    public FarplaneBuild(long size, FarplanePatch[] patches)
    {
        this.Size = size;
        this.Patches = patches;
    }
}

public static class GamePatcher
{
    public const string FfxExe = "FFX.exe";
    public const string Ffx2Exe = "FFX-2.exe";
    public const string FfxWillExe = "FFX&X-2_Will.exe";

    // Known Steam FFX.exe build sizes
    public const long FFX_OLD_SIZE = 0xA2E600; // 10,675,712 bytes
    public const long FFX_NEW_SIZE = 0xA31500; // 10,687,744 bytes (Update 2026)

    private static readonly FarplaneBuild[] FarplaneBuilds =
    [
        // BUILD: 0xA2E600
        new FarplaneBuild(FFX_OLD_SIZE,
        [
            new FarplanePatch(10, 0x36E0C4, new byte[] { 0x74 }, new byte[] { 0x75 }),
            new FarplanePatch(10, 0x36DFE0, new byte[] { 0x68, 0x68, 0x29, 0xB5, 0x00, 0xE8, 0xC6, 0x0A, 0xEC, 0xFF, 0x83, 0xC4, 0x04, 0x33, 0xC0, 0xC3, 0x68, 0xD8, 0x29, 0xB5, 0x00, 0xE8, 0xB6, 0x0A, 0xEC, 0xFF, 0x83, 0xC4, 0x04, 0x33, 0xC0, 0xC3, 0x68, 0x48, 0x2A, 0xB5, 0x00, 0xE8, 0xA6, 0x0A, 0xEC, 0xFF, 0x83, 0xC4, 0x04, 0x33, 0xC0, 0xC3, 0x68, 0xB8, 0x2A, 0xB5, 0x00, 0xE8, 0x96, 0x0A, 0xEC, 0xFF, 0x59, 0xC3, 0xCC, 0xCC, 0xCC, 0xCC, 0x68, 0x30, 0x2B, 0xB5, 0x00, 0xE8, 0x86, 0x0A, 0xEC, 0xFF, 0x83, 0xC4, 0x04, 0x33, 0xC0, 0xC3, 0x68, 0xA8, 0x2B, 0xB5, 0x00, 0xE8, 0x76, 0x0A, 0xEC, 0xFF, 0x83, 0xC4, 0x04, 0x33, 0xC0, 0xC3 }, new byte[] { 0xE8, 0x00, 0x00, 0x00, 0x00, 0x58, 0x83, 0xB8, 0x27, 0xB4, 0x9B, 0x00, 0x00, 0x74, 0x3D, 0x56, 0xE8, 0xFB, 0x01, 0x00, 0x00, 0xBE, 0x90, 0x01, 0x00, 0x00, 0xE8, 0xA1, 0x32, 0xED, 0xFF, 0x85, 0xC0, 0x75, 0x1B, 0xE8, 0x88, 0x32, 0xED, 0xFF, 0x85, 0xC0, 0x75, 0x12, 0xE8, 0xBF, 0x88, 0xF6, 0xFF, 0x85, 0xC0, 0x74, 0x16, 0x83, 0xB8, 0xF0, 0x06, 0x00, 0x00, 0x00, 0x74, 0x0D, 0x6A, 0x05, 0xE8, 0xFB, 0x14, 0xCC, 0xFF, 0x83, 0xC4, 0x04, 0x4E, 0x75, 0xCF, 0x5E, 0xC3, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC }),
            new FarplanePatch(10, 0x48D4BD, new byte[] { 0xE8, 0x2E, 0x0D, 0xEE, 0xFF }, new byte[] { 0xE8, 0x1E, 0x0B, 0xEE, 0xFF }),
            new FarplanePatch(10, 0x508E71, new byte[] { 0xE8, 0x7A, 0x53, 0xE6, 0xFF }, new byte[] { 0xE8, 0x6A, 0x51, 0xE6, 0xFF }),
            new FarplanePatch(10, 0x9158FC, new byte[] { 0xE1, 0x3B, 0xF1, 0x3B, 0x01, 0x3C, 0x11, 0x3C, 0x21, 0x3C, 0x31, 0x3C }, new byte[] { 0xE1, 0x0B, 0xF1, 0x0B, 0x01, 0x0C, 0x11, 0x0C, 0x21, 0x0C, 0x31, 0x0C }),
            new FarplanePatch(10, 0x70AE80, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, new byte[] { 0x60, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x5B, 0x8B, 0x45, 0x04, 0x8D, 0x8B, 0x38, 0xC5, 0xE3, 0xFF, 0x39, 0xC8, 0x74, 0x0E, 0x8D, 0x8B, 0x72, 0xBF, 0xE3, 0xFF, 0x39, 0xC8, 0x0F, 0x85, 0xB2, 0x00, 0x00, 0x00, 0x8B, 0xB3, 0x2A, 0x59, 0x15, 0x00, 0x85, 0xF6, 0x0F, 0x84, 0xA4, 0x00, 0x00, 0x00, 0x8B, 0x7D, 0x08, 0x8B, 0x6D, 0x0C, 0x01, 0xFD, 0x8D, 0x86, 0x0C, 0x1C, 0x00, 0x00, 0x50, 0xFF, 0x93, 0xCE, 0x06, 0x00, 0x00, 0x6A, 0x00, 0x8B, 0x86, 0x64, 0x1C, 0x00, 0x00, 0x8D, 0x8E, 0x58, 0x1C, 0x00, 0x00, 0x39, 0xC8, 0x74, 0x69, 0x85, 0xC0, 0x74, 0x65, 0x8B, 0x08, 0x8B, 0x50, 0x04, 0x01, 0xCA, 0x39, 0xF9, 0x72, 0x2A, 0x39, 0xEA, 0x77, 0x26, 0xFF, 0x70, 0x0C, 0x29, 0xCA, 0x01, 0x54, 0x24, 0x04, 0x8B, 0x48, 0x08, 0x8B, 0x50, 0x0C, 0x89, 0x51, 0x0C, 0x89, 0x4A, 0x08, 0x8B, 0x8E, 0x4C, 0x1C, 0x00, 0x00, 0x89, 0x08, 0x89, 0x86, 0x4C, 0x1C, 0x00, 0x00, 0x58, 0xEB, 0xBD, 0x39, 0xF9, 0x73, 0x12, 0x39, 0xFA, 0x76, 0x23, 0x29, 0xFA, 0x01, 0x14, 0x24, 0x89, 0xFA, 0x29, 0xCA, 0x89, 0x50, 0x04, 0xEB, 0x15, 0x39, 0xE9, 0x73, 0x11, 0x52, 0x89, 0xEA, 0x29, 0xCA, 0x01, 0x54, 0x24, 0x04, 0x5A, 0x29, 0xEA, 0x89, 0x50, 0x04, 0x89, 0x28, 0x8B, 0x40, 0x0C, 0xEB, 0x8D, 0x58, 0x29, 0x86, 0x00, 0x1C, 0x00, 0x00, 0x8D, 0x86, 0x0C, 0x1C, 0x00, 0x00, 0x50, 0xFF, 0x93, 0xD2, 0x06, 0x00, 0x00, 0x61, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x58, 0xFF, 0xA0, 0x3E, 0x05, 0x00, 0x00 }),
            new FarplanePatch(10, 0x542D50, new byte[] { 0xFF, 0x15, 0x98, 0xC0, 0xB0, 0x00 }, new byte[] { 0xE8, 0x2B, 0x81, 0x1C, 0x00, 0x90 }),
            new FarplanePatch(10, 0x9345AE, new byte[] { 0x52, 0x39 }, new byte[] { 0x52, 0x09 }),
            new FarplanePatch(10, 0x258, new byte[] { 0x7B, 0xAA, 0x70, 0x00 }, new byte[] { 0x00, 0xAC, 0x70, 0x00 }),
            new FarplanePatch(11, 0x70AF70, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, new byte[] { 0x51, 0xE8, 0x8A, 0x8C, 0xB4, 0xFF, 0x59, 0xE9, 0xA4, 0x4C, 0xB4, 0xFF }),
            new FarplanePatch(11, 0x25336A, new byte[] { 0xE8, 0xB1, 0xC8, 0xFF, 0xFF }, new byte[] { 0xE8, 0x01, 0x7C, 0x4B, 0x00 })
        ]),

        // BUILD: 0xA31500
        new FarplaneBuild(FFX_NEW_SIZE,
        [
            new FarplanePatch(12, 0x36DFB4, new byte[] { 0x74 }, new byte[] { 0x75 }),
            new FarplanePatch(12, 0x36DED0, new byte[] { 0x68, 0x68, 0x29, 0xB5, 0x00, 0xE8, 0x26, 0x0A, 0xEC, 0xFF, 0x83, 0xC4, 0x04, 0x33, 0xC0, 0xC3, 0x68, 0xD8, 0x29, 0xB5, 0x00, 0xE8, 0x16, 0x0A, 0xEC, 0xFF, 0x83, 0xC4, 0x04, 0x33, 0xC0, 0xC3, 0x68, 0x48, 0x2A, 0xB5, 0x00, 0xE8, 0x06, 0x0A, 0xEC, 0xFF, 0x83, 0xC4, 0x04, 0x33, 0xC0, 0xC3, 0x68, 0xB8, 0x2A, 0xB5, 0x00, 0xE8, 0xF6, 0x09, 0xEC, 0xFF, 0x59, 0xC3, 0xCC, 0xCC, 0xCC, 0xCC, 0x68, 0x30, 0x2B, 0xB5, 0x00, 0xE8, 0xE6, 0x09, 0xEC, 0xFF, 0x83, 0xC4, 0x04, 0x33, 0xC0, 0xC3, 0x68, 0xA8, 0x2B, 0xB5, 0x00, 0xE8, 0xD6, 0x09, 0xEC, 0xFF, 0x83, 0xC4, 0x04, 0x33, 0xC0, 0xC3 }, new byte[] { 0xE8, 0x00, 0x00, 0x00, 0x00, 0x58, 0x83, 0xB8, 0x37, 0xB5, 0x9B, 0x00, 0x00, 0x74, 0x3D, 0x56, 0xE8, 0xFB, 0x01, 0x00, 0x00, 0xBE, 0x90, 0x01, 0x00, 0x00, 0xE8, 0x01, 0x32, 0xED, 0xFF, 0x85, 0xC0, 0x75, 0x1B, 0xE8, 0xE8, 0x31, 0xED, 0xFF, 0x85, 0xC0, 0x75, 0x12, 0xE8, 0x9F, 0x88, 0xF6, 0xFF, 0x85, 0xC0, 0x74, 0x16, 0x83, 0xB8, 0xF0, 0x06, 0x00, 0x00, 0x00, 0x74, 0x0D, 0x6A, 0x05, 0xE8, 0x1B, 0x15, 0xCC, 0xFF, 0x83, 0xC4, 0x04, 0x4E, 0x75, 0xCF, 0x5E, 0xC3, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC }),
            new FarplanePatch(12, 0x48D54D, new byte[] { 0xE8, 0x8E, 0x0B, 0xEE, 0xFF }, new byte[] { 0xE8, 0x7E, 0x09, 0xEE, 0xFF }),
            new FarplanePatch(12, 0x508E71, new byte[] { 0xE8, 0x6A, 0x52, 0xE6, 0xFF }, new byte[] { 0xE8, 0x5A, 0x50, 0xE6, 0xFF }),
            new FarplanePatch(12, 0x9158D0, new byte[] { 0xD1, 0x3A, 0xE1, 0x3A, 0xF1, 0x3A, 0x01, 0x3B, 0x11, 0x3B, 0x21, 0x3B }, new byte[] { 0xD1, 0x0A, 0xE1, 0x0A, 0xF1, 0x0A, 0x01, 0x0B, 0x11, 0x0B, 0x21, 0x0B }),
            new FarplanePatch(12, 0x70AE80, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, new byte[] { 0x60, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x5B, 0x8B, 0x45, 0x04, 0x8D, 0x8B, 0x38, 0xC5, 0xE3, 0xFF, 0x39, 0xC8, 0x74, 0x0E, 0x8D, 0x8B, 0x72, 0xBF, 0xE3, 0xFF, 0x39, 0xC8, 0x0F, 0x85, 0xB2, 0x00, 0x00, 0x00, 0x8B, 0xB3, 0x2A, 0x59, 0x15, 0x00, 0x85, 0xF6, 0x0F, 0x84, 0xA4, 0x00, 0x00, 0x00, 0x8B, 0x7D, 0x08, 0x8B, 0x6D, 0x0C, 0x01, 0xFD, 0x8D, 0x86, 0x0C, 0x1C, 0x00, 0x00, 0x50, 0xFF, 0x93, 0xCE, 0x06, 0x00, 0x00, 0x6A, 0x00, 0x8B, 0x86, 0x64, 0x1C, 0x00, 0x00, 0x8D, 0x8E, 0x58, 0x1C, 0x00, 0x00, 0x39, 0xC8, 0x74, 0x69, 0x85, 0xC0, 0x74, 0x65, 0x8B, 0x08, 0x8B, 0x50, 0x04, 0x01, 0xCA, 0x39, 0xF9, 0x72, 0x2A, 0x39, 0xEA, 0x77, 0x26, 0xFF, 0x70, 0x0C, 0x29, 0xCA, 0x01, 0x54, 0x24, 0x04, 0x8B, 0x48, 0x08, 0x8B, 0x50, 0x0C, 0x89, 0x51, 0x0C, 0x89, 0x4A, 0x08, 0x8B, 0x8E, 0x4C, 0x1C, 0x00, 0x00, 0x89, 0x08, 0x89, 0x86, 0x4C, 0x1C, 0x00, 0x00, 0x58, 0xEB, 0xBD, 0x39, 0xF9, 0x73, 0x12, 0x39, 0xFA, 0x76, 0x23, 0x29, 0xFA, 0x01, 0x14, 0x24, 0x89, 0xFA, 0x29, 0xCA, 0x89, 0x50, 0x04, 0xEB, 0x15, 0x39, 0xE9, 0x73, 0x11, 0x52, 0x89, 0xEA, 0x29, 0xCA, 0x01, 0x54, 0x24, 0x04, 0x5A, 0x29, 0xEA, 0x89, 0x50, 0x04, 0x89, 0x28, 0x8B, 0x40, 0x0C, 0xEB, 0x8D, 0x58, 0x29, 0x86, 0x00, 0x1C, 0x00, 0x00, 0x8D, 0x86, 0x0C, 0x1C, 0x00, 0x00, 0x50, 0xFF, 0x93, 0xD2, 0x06, 0x00, 0x00, 0x61, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x58, 0xFF, 0xA0, 0x3E, 0x05, 0x00, 0x00 }),
            new FarplanePatch(12, 0x542D50, new byte[] { 0xFF, 0x15, 0x98, 0xC0, 0xB0, 0x00 }, new byte[] { 0xE8, 0x2B, 0x81, 0x1C, 0x00, 0x90 }),
            new FarplanePatch(12, 0x9345D6, new byte[] { 0x52, 0x39 }, new byte[] { 0x52, 0x09 }),
            new FarplanePatch(12, 0x260, new byte[] { 0x7B, 0xAA, 0x70, 0x00 }, new byte[] { 0x00, 0xAC, 0x70, 0x00 }),
            new FarplanePatch(12, 0x70AF70, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, new byte[] { 0x51, 0xE8, 0xDA, 0x8A, 0xB4, 0xFF, 0x59, 0xE9, 0xF4, 0x4A, 0xB4, 0xFF }),
            new FarplanePatch(12, 0x2531BA, new byte[] { 0xE8, 0xB1, 0xC8, 0xFF, 0xFF }, new byte[] { 0xE8, 0xB1, 0x7D, 0x4B, 0x00 })
        ])
    ];

    #region Auto Detection

    public static string DetectGameDirectory()
    {
        // 1. Check if the game process is running
        var runningProc = Process.GetProcessesByName("FFX")
            .Concat(Process.GetProcessesByName("FFX-2"))
            .FirstOrDefault();

        if (runningProc != null)
        {
            try
            {
                var dir = Path.GetDirectoryName(runningProc.MainModule?.FileName);
                if (!string.IsNullOrEmpty(dir) && File.Exists(Path.Combine(dir, FfxExe)))
                    return dir;
            }
            catch { }
        }

        // 2. Check Steam Registry
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 359870")
                         ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 359870");
            if (key != null)
            {
                var installDir = key.GetValue("InstallLocation") as string;
                if (!string.IsNullOrEmpty(installDir) && Directory.Exists(installDir) && File.Exists(Path.Combine(installDir, FfxExe)))
                    return installDir;
            }
        }
        catch { }

        // 3. Check common Steam library paths
        string[] commonPaths =
        [
            @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY FFX&FFX-2 HD Remaster",
            @"D:\Steam\steamapps\common\FINAL FANTASY FFX&FFX-2 HD Remaster",
            @"E:\Steam\steamapps\common\FINAL FANTASY FFX&FFX-2 HD Remaster",
            @"D:\SteamLibrary\steamapps\common\FINAL FANTASY FFX&FFX-2 HD Remaster",
            @"E:\SteamLibrary\steamapps\common\FINAL FANTASY FFX&FFX-2 HD Remaster",
            AppDomain.CurrentDomain.BaseDirectory
        ];

        foreach (var path in commonPaths)
        {
            if (Directory.Exists(path) && File.Exists(Path.Combine(path, FfxExe)))
                return path;
        }

        return string.Empty;
    }

    #endregion

    #region Backup & Restore

    public static string GetOriginalBackupPath(string exePath) => exePath + ".original";

    public static string FindAnyBackupPath(string exePath)
    {
        // Support Farplane's .original, Fuxake's .bak, and XyFreak's .en.exe
        string[] candidates =
        [
            exePath + ".original",
            exePath + ".bak",
            Path.ChangeExtension(exePath, ".en.exe")
        ];

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
                return candidate;
        }

        return string.Empty;
    }

    public static void EnsureOriginalBackup(string exePath)
    {
        var backupPath = GetOriginalBackupPath(exePath);
        if (!File.Exists(backupPath) && File.Exists(exePath))
        {
            var existingOtherBackup = FindAnyBackupPath(exePath);
            if (!string.IsNullOrEmpty(existingOtherBackup))
            {
                File.Copy(existingOtherBackup, backupPath, overwrite: false);
            }
            else
            {
                File.Copy(exePath, backupPath, overwrite: false);
            }
        }
    }

    public static bool RestoreFromOriginal(string exePath, out string error)
    {
        error = string.Empty;
        var backupPath = FindAnyBackupPath(exePath);
        if (string.IsNullOrEmpty(backupPath) || !File.Exists(backupPath))
        {
            error = $"No backup file found for {Path.GetFileName(exePath)} (.original / .bak / .en.exe)";
            return false;
        }

        try
        {
            File.Copy(backupPath, exePath, overwrite: true);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    #endregion

    #region 4GB Patch (Large Address Aware)

    public static bool IsLargeAddressAware(string exePath)
    {
        if (!File.Exists(exePath))
            return false;

        try
        {
            using var fs = new FileStream(exePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new BinaryReader(fs);
            if (fs.Length < 0x40)
                return false;

            if (reader.ReadUInt16() != 0x5A4D) // 'MZ'
                return false;

            fs.Seek(0x3C, SeekOrigin.Begin);
            int e_lfanew = reader.ReadInt32();
            if (e_lfanew <= 0 || e_lfanew + 0x18 > fs.Length)
                return false;

            fs.Seek(e_lfanew, SeekOrigin.Begin);
            if (reader.ReadUInt32() != 0x00004550) // 'PE\0\0'
                return false;

            fs.Seek(e_lfanew + 0x16, SeekOrigin.Begin);
            ushort characteristics = reader.ReadUInt16();
            return (characteristics & 0x0020) != 0;
        }
        catch
        {
            return false;
        }
    }

    public static void SetLargeAddressAware(string exePath, bool enable)
    {
        EnsureOriginalBackup(exePath);
        using var fs = new FileStream(exePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        using var reader = new BinaryReader(fs);
        using var writer = new BinaryWriter(fs);

        fs.Seek(0x3C, SeekOrigin.Begin);
        int e_lfanew = reader.ReadInt32();
        fs.Seek(e_lfanew + 0x16, SeekOrigin.Begin);
        ushort characteristics = reader.ReadUInt16();

        if (enable)
            characteristics |= 0x0020;
        else
            characteristics = (ushort)(characteristics & ~0x0020);

        fs.Seek(e_lfanew + 0x16, SeekOrigin.Begin);
        writer.Write(characteristics);
    }

    #endregion

    #region Audio Undub Patch (Japanese Audio)

    public class StringPair
    {
        public int JpOffset { get; set; }
        public int EnOffset { get; set; }
        public int Length { get; set; }
        public string JpString { get; set; }
        public string EnString { get; set; }
    }

    public static bool GetRDataSection(byte[] fileBytes, out int rdataOffset, out int rdataSize)
    {
        rdataOffset = 0;
        rdataSize = 0;
        if (fileBytes == null || fileBytes.Length < 0x40)
            return false;

        if (BitConverter.ToUInt16(fileBytes, 0) != 0x5A4D)
            return false;

        int e_lfanew = BitConverter.ToInt32(fileBytes, 0x3C);
        if (e_lfanew <= 0 || e_lfanew + 0xF8 > fileBytes.Length)
            return false;

        if (BitConverter.ToUInt32(fileBytes, e_lfanew) != 0x00004550)
            return false;

        ushort numSections = BitConverter.ToUInt16(fileBytes, e_lfanew + 6);
        ushort sizeOfOptHeader = BitConverter.ToUInt16(fileBytes, e_lfanew + 20);
        int sectionHeadersOffset = e_lfanew + 24 + sizeOfOptHeader;

        for (int i = 0; i < numSections; i++)
        {
            int secOffset = sectionHeadersOffset + (i * 40);
            if (secOffset + 40 > fileBytes.Length)
                break;

            string secName = Encoding.ASCII.GetString(fileBytes, secOffset, 8).TrimEnd('\0');
            if (secName.Equals(".rdata", StringComparison.OrdinalIgnoreCase))
            {
                rdataSize = BitConverter.ToInt32(fileBytes, secOffset + 16);
                rdataOffset = BitConverter.ToInt32(fileBytes, secOffset + 20);
                return rdataOffset > 0 && rdataSize > 0 && rdataOffset + rdataSize <= fileBytes.Length;
            }
        }
        return false;
    }

    public static List<StringPair> CollectAudioStrings(byte[] fileBytes)
    {
        var pairs = new List<StringPair>();
        if (!GetRDataSection(fileBytes, out int rdataRaw, out int rdataSize))
            return pairs;

        string[] tokens = ["/JP/", "/jp/"];
        int prevOffset = -1;
        int prevLen = 0;
        int baseOffset = rdataRaw;
        int endOffset = rdataRaw + rdataSize;

        for (int pos = baseOffset; pos < endOffset; pos++)
        {
            if (fileBytes[pos] != 0)
            {
                int currentPos = pos;
                int jpLen = 0;
                while (currentPos + jpLen < endOffset && fileBytes[currentPos + jpLen] != 0)
                    jpLen++;

                if (currentPos + jpLen > endOffset)
                    break;

                string currentStr = Encoding.ASCII.GetString(fileBytes, currentPos, jpLen);
                bool matchedPair = false;
                bool hasToken = false;

                foreach (var token in tokens)
                {
                    int tokenIndex = currentStr.IndexOf(token, StringComparison.Ordinal);
                    if (tokenIndex >= 0)
                    {
                        hasToken = true;

                        // 1. Check string immediately following currentStr
                        int enPos = currentPos + jpLen + 1;
                        while (enPos < endOffset && fileBytes[enPos] == 0)
                            enPos++;

                        int enLen = 0;
                        while (enPos + enLen < endOffset && fileBytes[enPos + enLen] != 0)
                            enLen++;

                        if (enPos + enLen <= endOffset && jpLen == enLen)
                        {
                            string nextStr = Encoding.ASCII.GetString(fileBytes, enPos, enLen);
                            if (currentStr.Substring(0, tokenIndex).Equals(nextStr.Substring(0, tokenIndex), StringComparison.Ordinal))
                            {
                                pairs.Add(new StringPair
                                {
                                    JpOffset = currentPos,
                                    EnOffset = enPos,
                                    Length = jpLen,
                                    JpString = currentStr,
                                    EnString = nextStr
                                });
                                pos = enPos + enLen;
                                prevOffset = -1;
                                prevLen = 0;
                                matchedPair = true;
                                break;
                            }
                        }

                        // 2. Check string immediately preceding currentStr
                        if (prevOffset != -1 && jpLen == prevLen)
                        {
                            string prevStr = Encoding.ASCII.GetString(fileBytes, prevOffset, prevLen);
                            if (currentStr.Substring(0, tokenIndex).Equals(prevStr.Substring(0, tokenIndex), StringComparison.Ordinal))
                            {
                                pairs.Add(new StringPair
                                {
                                    JpOffset = currentPos,
                                    EnOffset = prevOffset,
                                    Length = jpLen,
                                    JpString = currentStr,
                                    EnString = prevStr
                                });
                                pos = currentPos + jpLen;
                                prevOffset = -1;
                                prevLen = 0;
                                matchedPair = true;
                                break;
                            }
                        }

                        break;
                    }
                }

                if (!matchedPair)
                {
                    pos = currentPos + jpLen;
                    if (!hasToken)
                    {
                        prevOffset = currentPos;
                        prevLen = jpLen;
                    }
                    else
                    {
                        prevOffset = -1;
                        prevLen = 0;
                    }
                }
            }
        }

        return pairs;
    }

    public static bool IsAudioUndubbed(string exePath)
    {
        if (!File.Exists(exePath))
            return false;

        try
        {
            var bytes = File.ReadAllBytes(exePath);
            var pairs = CollectAudioStrings(bytes);
            if (pairs.Count == 0)
                return false;

            int matched = pairs.Count(p => p.EnString.Contains("/JP/") || p.EnString.Contains("/jp/"));
            return matched > 0 && matched == pairs.Count;
        }
        catch
        {
            return false;
        }
    }

    public static int ApplyUndubPatch(string exePath, bool undo = false)
    {
        EnsureOriginalBackup(exePath);
        var bytes = File.ReadAllBytes(exePath);

        if (!undo)
        {
            var pairs = CollectAudioStrings(bytes);
            if (pairs.Count == 0)
                return 0;

            foreach (var pair in pairs)
            {
                Buffer.BlockCopy(bytes, pair.JpOffset, bytes, pair.EnOffset, pair.Length);
            }

            File.WriteAllBytes(exePath, bytes);
            return pairs.Count;
        }
        else
        {
            // Restore original English strings from clean backup if available
            var backupPath = FindAnyBackupPath(exePath);
            if (!string.IsNullOrEmpty(backupPath) && File.Exists(backupPath))
            {
                var backupBytes = File.ReadAllBytes(backupPath);
                var cleanPairs = CollectAudioStrings(backupBytes);
                if (cleanPairs.Count > 0 && backupBytes.Length == bytes.Length)
                {
                    foreach (var pair in cleanPairs)
                    {
                        Buffer.BlockCopy(backupBytes, pair.EnOffset, bytes, pair.EnOffset, pair.Length);
                    }
                    File.WriteAllBytes(exePath, bytes);
                    return cleanPairs.Count;
                }
            }

            // Fallback if no clean backup exists: convert /JP/ -> /US/ and /jp/ -> /us/ in target slot
            var currentPairs = CollectAudioStrings(bytes);
            if (currentPairs.Count == 0)
                return 0;

            foreach (var pair in currentPairs)
            {
                string restoredEn = pair.EnString
                    .Replace("/JP/", "/US/")
                    .Replace("/jp/", "/us/");
                byte[] enBytes = Encoding.ASCII.GetBytes(restoredEn);
                if (enBytes.Length == pair.Length)
                {
                    Buffer.BlockCopy(enBytes, 0, bytes, pair.EnOffset, pair.Length);
                }
            }

            File.WriteAllBytes(exePath, bytes);
            return currentPairs.Count;
        }
    }

    #endregion

    #region Farplane Background Video Fix (Embedded FarplaneFix v1.2)

    private static bool Matches(byte[] data, int offset, byte[] expected)
    {
        if (data == null || expected == null || offset < 0 || offset + expected.Length > data.Length)
            return false;

        for (int i = 0; i < expected.Length; i++)
        {
            if (data[offset + i] != expected[i])
                return false;
        }
        return true;
    }

    public static bool? IsFarplaneFixed(string ffxExePath)
    {
        if (!File.Exists(ffxExePath))
            return null;

        try
        {
            var data = File.ReadAllBytes(ffxExePath);
            var build = FarplaneBuilds.FirstOrDefault(b => b.Size == data.LongLength);
            if (build == null)
                return null;

            bool allNew = true;
            foreach (var patch in build.Patches)
            {
                if (Matches(data, patch.Offset, patch.New))
                {
                    continue;
                }
                if (Matches(data, patch.Offset, patch.Old))
                {
                    allNew = false;
                }
                else
                {
                    // Unknown/corrupted bytes at patch offset
                    return null;
                }
            }

            return allNew;
        }
        catch
        {
            return null;
        }
    }

    public static bool ApplyFarplaneFix(string ffxExePath, bool undo, out string message)
    {
        message = string.Empty;
        if (!File.Exists(ffxExePath))
        {
            message = "FFX.exe not found.";
            return false;
        }

        EnsureOriginalBackup(ffxExePath);

        try
        {
            var data = File.ReadAllBytes(ffxExePath);
            var build = FarplaneBuilds.FirstOrDefault(b => b.Size == data.LongLength);
            if (build == null)
            {
                message = "Unsupported FFX.exe build size.";
                return false;
            }

            // Validate all 11 patch regions match either Old or New before writing
            foreach (var patch in build.Patches)
            {
                if (!Matches(data, patch.Offset, patch.New) && !Matches(data, patch.Offset, patch.Old))
                {
                    message = $"Unrecognized byte sequence at offset 0x{patch.Offset:X}.";
                    return false;
                }
            }

            // Apply all 11 patches (or revert all 11 patches if undo == true)
            foreach (var patch in build.Patches)
            {
                var sourceBytes = undo ? patch.Old : patch.New;
                Buffer.BlockCopy(sourceBytes, 0, data, patch.Offset, sourceBytes.Length);
            }

            File.WriteAllBytes(ffxExePath, data);
            message = undo
                ? "Farplane Fix v1.2 removed (all 11 patches reverted to original)."
                : "Farplane Fix v1.2 applied (all 11 patches & code caves written).";
            return true;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }

    #endregion

    #region Status Retrieval

    public static GamePatcherStatus GetStatus(string gameDir, string exeFileName)
    {
        var fullPath = Path.Combine(gameDir, exeFileName);
        var status = new GamePatcherStatus
        {
            ExePath = fullPath,
            ExeName = exeFileName,
            Exists = File.Exists(fullPath)
        };

        if (!status.Exists)
            return status;

        var fi = new FileInfo(fullPath);
        status.FileSize = fi.Length;
        status.HasOriginalBackup = !string.IsNullOrEmpty(FindAnyBackupPath(fullPath));
        status.Is4GBEnabled = IsLargeAddressAware(fullPath);
        status.IsAudioUndubbed = IsAudioUndubbed(fullPath);

        if (string.Equals(exeFileName, FfxExe, StringComparison.OrdinalIgnoreCase))
        {
            status.IsFarplaneFixed = IsFarplaneFixed(fullPath);
        }

        return status;
    }

    #endregion
}
