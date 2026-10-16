using System;
using System.Runtime.InteropServices;
using Farplane.Common;
using Farplane.FFX.Values;
using Farplane.Memory;

namespace Farplane.FFX.Data;

public static class Party
{
    static int _offsetParty => OffsetScanner.GetOffset(GameOffset.FFX_PartyStatBase);
    static int _offsetPartyList => OffsetScanner.GetOffset(GameOffset.FFX_PartyList);
    static readonly int _blockLength = Marshal.SizeOf<PartyMember>();

    public static Character[] GetActiveParty()
    {
        var party = GameMemory.Read<byte>(_offsetPartyList, 8, false);
        if (party == null)
        {
            return new Character[8];
        }
        var outArray = new Character[8];
        for (var i = 0; i < 8; i++)
        {
            outArray[i] = (Character)party[i];
        }
        return outArray;
    }

    internal static void SetActiveParty(Character[] characters)
    {
        var writeArray = new byte[8];
        for (var i = 0; i < 8; i++)
        {
            if (i >= characters.Length)
            {
                writeArray[i] = 0xFF;
            }
            else
            {
                writeArray[i] = (byte)characters[i];
            }
        }
        GameMemory.Write(_offsetPartyList, writeArray, false);
    }

    public static void AddCharacter(Character character)
    {
        var partyList = GetActiveParty();
        var charaPos = Array.IndexOf(partyList, character);
        var writePos = Array.IndexOf(partyList, Character.None);
        if (charaPos == -1 && writePos != -1)
        {
            GameMemory.Write(_offsetPartyList + writePos, (byte)character, false);
        }
    }

    public static void RemoveCharacter(Character character)
    {
        var partyList = GetActiveParty();
        var partySize = partyList.Length;
        var charaPos = Array.IndexOf(partyList, character);
        if (charaPos != -1)
        {
            for (var i = charaPos; i < partySize - 1; i++)
            {
                partyList[i] = partyList[i + 1];
            }
            partyList[partySize - 1] = Character.None;
        }
        else
        {
            return;
        }

        var partyBytes = new byte[partySize];
        for (var i = 0; i < partySize; i++)
        {
            partyBytes[i] = (byte)partyList[i];
        }
        GameMemory.Write(_offsetPartyList, partyBytes, false);
    }

    public static PartyMember ReadPartyMember(int partyIndex)
    {
        var offset = _offsetParty + (partyIndex * _blockLength);
        var bytes = GameMemory.Read<byte>(offset, _blockLength, false);
        return StructHelper.BytesToStruct<PartyMember>(bytes);
    }

    public static void SetPartyMemberAttribute<T>(int partyIndex, string attributeName, T value)
    {
        var offset = _offsetParty + (partyIndex * _blockLength);
        var attributeOffset = Marshal.OffsetOf<PartyMember>(attributeName);
        GameMemory.Write((int)attributeOffset + offset, value, false);
    }

    public static void ToggleSkillFlag(int partyIndex, int skillFlag)
    {
        var offset =
            _offsetParty
            + (partyIndex * _blockLength)
            + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.SkillFlags));
        var byteIndex = skillFlag / 8;
        var bitIndex = skillFlag % 8;
        var partyMember = ReadPartyMember(partyIndex);
        var skillBytes = partyMember.SkillFlags;
        if (skillBytes == null || byteIndex >= skillBytes.Length)
            return;
        var newByte = BitHelper.ToggleBit(skillBytes[byteIndex], bitIndex);
        skillBytes[byteIndex] = newByte;
        GameMemory.Write(offset, skillBytes, false);
    }

    public static int GetMemoryOffset(int partyIndex) => _offsetParty + (partyIndex * _blockLength);

    public static void WritePartyMember(int partyIndex, PartyMember item)
    {
        var itemBytes = StructHelper.StructToBytes(item);
        var offset = _offsetParty + (partyIndex * _blockLength);
        GameMemory.Write(offset, itemBytes, false);
    }

    public static void RecalculateStats(int characterIndex = -1)
    {
        if (!GameMemory.IsAttached)
            return;
        var funcAddr = OffsetScanner.GetOffset(GameOffset.FFX_FuncCalcStats);
        if (funcAddr == 0)
            return;
        var processHandle = GameMemory.Process.Handle;

        byte[] shellcode;
        if (characterIndex >= 0)
        {
            // Shellcode call sub_3860F0(characterIndex)
            shellcode =
            [
                0x68, (byte)characterIndex, 0x00, 0x00, 0x00,       // push characterIndex
                0xB8, (byte)funcAddr, (byte)(funcAddr >> 8), (byte)(funcAddr >> 16), (byte)(funcAddr >> 24), // mov eax, funcAddr
                0xFF, 0xD0,                                         // call eax
                0x83, 0xC4, 0x04,                                   // add esp, 4
                0xC3                                                // ret
            ];
        }
        else
        {
            // Shellcode loops from 0 to 7 to recalculate for the entire party (Tidus through Seymour)
            shellcode =
            [
                0x60,                                               // pushad
                0xB9, 0x08, 0x00, 0x00, 0x00,                       // mov ecx, 8
                0x31, 0xF6,                                         // xor esi, esi
                0x56,                                               // loop_start: push esi
                0xB8, (byte)funcAddr, (byte)(funcAddr >> 8), (byte)(funcAddr >> 16), (byte)(funcAddr >> 24), // mov eax, funcAddr
                0xFF, 0xD0,                                         // call eax
                0x83, 0xC4, 0x04,                                   // add esp, 4
                0x46,                                               // inc esi
                0x49,                                               // dec ecx
                0x75, 0xF0,                                         // jnz loop_start (-16 bytes)
                0x61,                                               // popad
                0xC3                                                // ret
            ];
        }

        var allocMem = WinAPI.VirtualAllocEx(processHandle, IntPtr.Zero, (uint)shellcode.Length, 0x1000 | 0x2000, 0x40); // MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE
        if (allocMem == IntPtr.Zero)
            return;
        try
        {
            if (WinAPI.WriteProcessMemory(processHandle, allocMem, shellcode, shellcode.Length, out _))
            {
                var hThread = WinAPI.CreateRemoteThread(processHandle, IntPtr.Zero, 0, allocMem, IntPtr.Zero, 0, out _);
                if (hThread != IntPtr.Zero)
                {
                    WinAPI.WaitForSingleObject(hThread, 1000);
                    WinAPI.CloseHandle(hThread);
                }
            }
        }
        finally
        {
            WinAPI.VirtualFreeEx(processHandle, allocMem, 0, 0x8000); // MEM_RELEASE
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 0, CharSet = CharSet.Ansi, Size = 0x94)]
public struct PartyMember
{
    [MarshalAs(UnmanagedType.U2)]
    public ushort unknown_ushort;
    [MarshalAs(UnmanagedType.U4)]
    public int BaseHp;
    [MarshalAs(UnmanagedType.U4)]
    public int BaseMp;
    [MarshalAs(UnmanagedType.U1)]
    public byte BaseStrength;
    [MarshalAs(UnmanagedType.U1)]
    public byte BaseDefense;
    [MarshalAs(UnmanagedType.U1)]
    public byte BaseMagic;
    [MarshalAs(UnmanagedType.U1)]
    public byte BaseMagicDefense;
    [MarshalAs(UnmanagedType.U1)]
    public byte BaseAgility;
    [MarshalAs(UnmanagedType.U1)]
    public byte BaseLuck;
    [MarshalAs(UnmanagedType.U1)]
    public byte BaseEvasion;
    [MarshalAs(UnmanagedType.U1)]
    public byte BaseAccuracy;
    [MarshalAs(UnmanagedType.U4)]
    public int ApTotal;
    [MarshalAs(UnmanagedType.U4)]
    public int ApCurrent;
    [MarshalAs(UnmanagedType.U4)]
    public int CurrentHp;
    [MarshalAs(UnmanagedType.U4)]
    public int CurrentMp;
    [MarshalAs(UnmanagedType.U4)]
    public int CurrentHpMax;
    [MarshalAs(UnmanagedType.U4)]
    public int CurrentMpMax;
    [MarshalAs(UnmanagedType.U1)]
    public byte InParty;
    [MarshalAs(UnmanagedType.U1)]
    public byte EquippedWeapon;
    [MarshalAs(UnmanagedType.U1)]
    public byte EquippedArmor;
    [MarshalAs(UnmanagedType.U1)]
    public byte CurrentStrength;
    [MarshalAs(UnmanagedType.U1)]
    public byte CurrentDefense;
    [MarshalAs(UnmanagedType.U1)]
    public byte CurrentMagic;
    [MarshalAs(UnmanagedType.U1)]
    public byte CurrentMagicDefense;
    [MarshalAs(UnmanagedType.U1)]
    public byte CurrentAgility;
    [MarshalAs(UnmanagedType.U1)]
    public byte CurrentLuck;
    [MarshalAs(UnmanagedType.U1)]
    public byte CurrentEvasion;
    [MarshalAs(UnmanagedType.U1)]
    public byte CurrentAccuracy;
    [MarshalAs(UnmanagedType.U1)]
    public byte PoisonDamage;
    [MarshalAs(UnmanagedType.U1)]
    public byte OverdriveMode;
    [MarshalAs(UnmanagedType.U1)]
    public byte OverdriveLevel;
    [MarshalAs(UnmanagedType.U1)]
    public byte OverdriveMax;
    [MarshalAs(UnmanagedType.U1)]
    public byte SphereLevelCurrent;
    [MarshalAs(UnmanagedType.U1)]
    public byte SphereLevelTotal;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
    public byte[] SkillFlags;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] unknown_1;
    [MarshalAs(UnmanagedType.U4)]
    public int TotalBattles;
    [MarshalAs(UnmanagedType.U4)]
    public int TotalKills;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] unknown_2;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveWarrior;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveComrade;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveStoic;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveHealer;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveTactician;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveVictim;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveDancer;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveAvenger;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveSlayer;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveHero;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveRook;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveVictor;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveCoward;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveAlly;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveSufferer;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveDaredevil;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveLoner;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveBlank1;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveBlank2;
    [MarshalAs(UnmanagedType.U2)]
    public ushort OverdriveAeonsOnly;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] OverdriveModes;
}
