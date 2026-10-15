using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

//
// sigScan C# Implementation - Written by atom0s [aka Wiccaan]
// Class Version: 2.0.0
//
// [ CHANGE LOG ] -------------------------------------------------------------------------
//
//      2.0.0
//          - Updated to no longer require unsafe or fixed code.
//          - Removed unneeded methods and code.
//
//      1.0.0
//          - First version written and release.
//
// [ CREDITS ] ----------------------------------------------------------------------------
//
// sigScan is based on the FindPattern code written by
// dom1n1k and Patrick at GameDeception.net
//
// Full credit to them for the purpose of this code. I, atom0s, simply
// take credit for converting it to C#.
//
// [ USAGE ] ------------------------------------------------------------------------------
//
// Examples:
//
//      SigScan _sigScan = new SigScan();
//      _sigScan.Process = someProc;
//      _sigScan.Address = new IntPtr(0x123456);
//      _sigScan.Size = 0x1000;
//      IntPtr pAddr = _sigScan.FindPattern(new byte[]{ 0xFF, 0xFF, 0xFF, 0xFF, 0x51, 0x55, 0xFC, 0x11 }, "xxxx?xx?", 12);
//
//      SigScan _sigScan = new SigScan(someProc, new IntPtr(0x123456), 0x1000);
//      IntPtr pAddr = _sigScan.FindPattern(new byte[]{ 0xFF, 0xFF, 0xFF, 0xFF, 0x51, 0x55, 0xFC, 0x11 }, "xxxx?xx?", 12);
//
// ----------------------------------------------------------------------------------------
namespace Farplane.Memory;

public class SigScan
{
    /// <summary>
    /// m_vDumpedRegion
    ///
    ///     The memory dumped from the external process.
    /// </summary>
    byte[] m_vDumpedRegion;

    #region "sigScan Class Construction"
    /// <summary>
    /// SigScan
    ///
    ///     Main class constructor that uses no params.
    ///     Simply initializes the class properties and
    ///     expects the user to set them later.
    /// </summary>
    public SigScan()
    {
        this.Process = null;
        this.Address = IntPtr.Zero;
        this.Size = 0;
        this.m_vDumpedRegion = null;
    }

    /// <summary>
    /// SigScan
    ///
    ///     Overloaded class constructor that sets the class
    ///     properties during construction.
    /// </summary>
    /// <param name="proc">The process to dump the memory from.</param>
    /// <param name="addr">The started address to begin the dump.</param>
    /// <param name="size">The size of the dump.</param>
    public SigScan(Process proc, IntPtr addr, int size)
    {
        this.Process = proc;
        this.Address = addr;
        this.Size = size;
    }
    #endregion

    #region "sigScan Class Private Methods"
    /// <summary>
    /// DumpMemory
    ///
    ///     Internal memory dump function that uses the set class
    ///     properties to dump a memory region.
    /// </summary>
    /// <returns>Boolean based on RPM results and valid properties.</returns>
    bool DumpMemory()
    {
        try
        {
            // Checks to ensure we have valid data.
            if (this.Process == null)
            {
                return false;
            }

            if (this.Process.HasExited)
            {
                return false;
            }

            if (this.Address == IntPtr.Zero)
            {
                return false;
            }

            if (this.Size == 0)
            {
                return false;
            }

            // Create the region space to dump into.
            this.m_vDumpedRegion = new byte[this.Size];

            var bReturn = false;

            // Dump the memory.
            bReturn = WinAPI.ReadProcessMemory(
                this.Process.Handle,
                this.Address,
                this.m_vDumpedRegion,
                (int)this.Size,
                out var nBytesRead
            );

            // Validation checks.
            if (!bReturn || nBytesRead != this.Size)
            {
                return false;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// MaskCheck
    ///
    ///     Compares the current pattern byte to the current memory dump
    ///     byte to check for a match. Uses wildcards to skip bytes that
    ///     are deemed unneeded in the compares.
    /// </summary>
    /// <param name="nOffset">Offset in the dump to start at.</param>
    /// <param name="btPattern">Pattern to scan for.</param>
    /// <param name="strMask">Mask to compare against.</param>
    /// <returns>Boolean depending on if the pattern was found.</returns>
    bool MaskCheck(int nOffset, byte[] btPattern, string strMask)
    {
        // Loop the pattern and compare to the mask and dump.
        for (var x = 0; x < btPattern.Length; x++)
        {
            // If the mask char is a wildcard, just continue.
            if (strMask[x] == '?')
            {
                continue;
            }

            // If the mask char is not a wildcard, ensure a match is made in the pattern.
            if ((strMask[x] == 'x') && (btPattern[x] != this.m_vDumpedRegion[nOffset + x]))
            {
                return false;
            }
        }

        // The loop was successful so we found the pattern.
        return true;
    }
    #endregion

    #region "sigScan Class Public Methods"
    /// <summary>
    /// FindPattern
    ///
    ///     Attempts to locate the given pattern inside the dumped memory region
    ///     compared against the given mask. If the pattern is found, the offset
    ///     is added to the located address and returned to the user.
    /// </summary>
    /// <param name="pattern">Byte pattern to look for in the dumped region.
    /// It should be in hex with ?? as a wildcard byte, spaces will be stripped
    /// before the pattern is searched.</param>
    /// <param name="nOffset">The offset added to the result address.</param>
    /// <returns>IntPtr - zero if not found, address if found.</returns>
    public IntPtr FindPattern(string pattern, int nOffset = 0)
    {
        // Generate a byte pattern and mask from the pattern string
        var patternString = pattern.Replace(" ", "");
        if (patternString.Length % 2 != 0)
        {
            throw new Exception("The pattern string must be divisible by 2.");
        }

        var patternBytes = new byte[patternString.Length / 2];
        var maskString = new StringBuilder();

        for (var i = 0; i < patternBytes.Length; i++)
        {
            var byteString = patternString.Substring(i * 2, 2);
            if (byteString == "??")
            {
                maskString.Append("?");
                patternBytes[i] = 0;
            }
            else
            {
                maskString.Append("x");
                patternBytes[i] = byte.Parse(byteString, NumberStyles.HexNumber);
            }
        }

        var strMask = maskString.ToString();
        var btPattern = (byte[])patternBytes.Clone();

        try
        {
            // Dump the memory region if we have not dumped it yet.
            if (this.m_vDumpedRegion == null || this.m_vDumpedRegion.Length == 0)
            {
                if (!this.DumpMemory())
                {
                    return IntPtr.Zero;
                }
            }

            // Ensure the mask and pattern lengths match.
            if (strMask.Length != btPattern.Length)
            {
                return IntPtr.Zero;
            }

            // Loop the region and look for the pattern.
            for (var x = 0; x < this.m_vDumpedRegion.Length; x++)
            {
                if (this.MaskCheck(x, btPattern, strMask))
                {
                    // The pattern was found, return it.
                    return this.Address + (x + nOffset);
                }
            }

            // Pattern was not found.
            return IntPtr.Zero;
        }
        catch (Exception)
        {
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// ResetRegion
    ///
    ///     Resets the memory dump array to nothing to allow
    ///     the class to redump the memory.
    /// </summary>
    public void ResetRegion() => this.m_vDumpedRegion = null;
    #endregion

    #region "sigScan Class Properties"
    public Process Process { get; set; }
    public IntPtr Address { get; set; }
    public long Size { get; set; }
    #endregion
}
