using System;
using System.IO;
using System.Runtime.InteropServices;
using Farplane.Common;
using Farplane.Memory;

namespace Farplane.FFX.Data;

public static class Blitzball
{
    public const int TotalPlayers = 60;
    static int _dataPointer => OffsetScanner.GetOffset(GameOffset.FFX_BlitzballData);
    static int _modePointer => OffsetScanner.GetOffset(GameOffset.FFX_BlitzballMode);
    static int _gamePointer => OffsetScanner.GetOffset(GameOffset.FFX_BlitzballPointer);
    static readonly int _blitzDataSize = Marshal.SizeOf<BlitzballData>();

    public static BlitzballData ReadBlitzballData(bool dumpBytes = false)
    {
        var blitzBytes = GameMemory.Read<byte>(_dataPointer, _blitzDataSize, false);
        if (dumpBytes && blitzBytes != null)
        {
            File.WriteAllBytes($"blitzdata_{DateTime.Now.Millisecond}.bin", blitzBytes);
        }

        return StructHelper.BytesToStruct<BlitzballData>(blitzBytes);
    }

    public static void WriteBlitzballData(BlitzballData data)
    {
        var dataBytes = StructHelper.StructToBytes(data);
        GameMemory.Write(_dataPointer, dataBytes, false);
    }

    public static BlitzballPlayer[] GetPlayers()
    {
        var blitzData = ReadBlitzballData();
        var players = new BlitzballPlayer[TotalPlayers];
        for (var i = 0; i < TotalPlayers; i++)
        {
            players[i] = GetPlayerInfo(i, blitzData);
        }
        return players;
    }

    public static BlitzballPlayer GetPlayerInfo(int playerIndex, BlitzballData blitzData)
    {
        var techs = new byte[5];
        if (blitzData.TechsEquipped != null && blitzData.TechsEquipped.Length >= (playerIndex * 5) + 5)
        {
            for (var i = 0; i < 5; i++)
            {
                techs[i] = blitzData.TechsEquipped[(playerIndex * 5) + i];
            }
        }

        var player = new BlitzballPlayer()
        {
            Level = blitzData.PlayerLevels != null && blitzData.PlayerLevels.Length > playerIndex ? blitzData.PlayerLevels[playerIndex] : (byte)0,
            Contract = blitzData.PlayerContracts != null && blitzData.PlayerContracts.Length > playerIndex ? blitzData.PlayerContracts[playerIndex] : (byte)0,
            Experience = blitzData.PlayerExperience != null && blitzData.PlayerExperience.Length > playerIndex ? blitzData.PlayerExperience[playerIndex] : (ushort)0,
            LeagueGoals = 0,
            Salary = blitzData.PlayerSalary != null && blitzData.PlayerSalary.Length > playerIndex ? blitzData.PlayerSalary[playerIndex] : (ushort)0,
            SkillFlags1 = blitzData.AbilityFlags1 != null && blitzData.AbilityFlags1.Length > playerIndex ? blitzData.AbilityFlags1[playerIndex] : 0,
            SkillFlags2 = blitzData.AbilityFlags2 != null && blitzData.AbilityFlags2.Length > playerIndex ? blitzData.AbilityFlags2[playerIndex] : 0,
            TechCapacity = blitzData.TechsAvailable != null && blitzData.TechsAvailable.Length > playerIndex ? blitzData.TechsAvailable[playerIndex] : (byte)0,
            Techs = techs,
            TournamentGoals = 0,
        };
        return player;
    }

    public static byte GetTeamSize(int teamIndex)
    {
        var sizes = GetTeamSizes();
        return sizes != null && sizes.Length > teamIndex ? sizes[teamIndex] : (byte)0;
    }

    public static void SetTeamSize(int teamIndex, byte teamSize)
    {
        var bdPointer1 = OffsetScanner.GetOffset(GameOffset.FFX_BlitzballTeamData);
        if (bdPointer1 == 0)
            return;
        var bdPointer2 = GameMemory.Read<int>(bdPointer1, false);
        if (bdPointer2 == 0)
            return;
        var bdPointer = GameMemory.Read<int>(bdPointer2 + 0x2C, false);
        if (bdPointer == 0)
            return;
        GameMemory.Write(bdPointer + 0xA88 + teamIndex, teamSize, false);
    }
    public static byte[] GetTeamSizes()
    {
        var bdPointer1 = OffsetScanner.GetOffset(GameOffset.FFX_BlitzballTeamData);
        if (bdPointer1 == 0)
            return new byte[6];
        var bdPointer2 = GameMemory.Read<int>(bdPointer1, false);
        if (bdPointer2 == 0)
            return new byte[6];
        var bdPointer = GameMemory.Read<int>(bdPointer2 + 0x2C, false);
        if (bdPointer == 0)
            return new byte[6];
        return GameMemory.Read<byte>(bdPointer + 0xA88, 6, false) ?? new byte[6];
    }

    public static BlitzballPlayer GetPlayerInfo(int playerIndex)
    {
        var blitzData = ReadBlitzballData();
        return GetPlayerInfo(playerIndex, blitzData);
    }

    public static void SetPlayerInfo(int playerIndex, BlitzballPlayer playerInfo)
    {
        var blitzData = ReadBlitzballData();
        if (blitzData.PlayerLevels == null)
            return;
        blitzData.PlayerLevels[playerIndex] = playerInfo.Level;
        blitzData.PlayerContracts[playerIndex] = playerInfo.Contract;
        blitzData.PlayerExperience[playerIndex] = playerInfo.Experience;
        blitzData.PlayerSalary[playerIndex] = playerInfo.Salary;
        blitzData.AbilityFlags1[playerIndex] = playerInfo.SkillFlags1;
        blitzData.AbilityFlags2[playerIndex] = playerInfo.SkillFlags2;
        blitzData.TechsAvailable[playerIndex] = playerInfo.TechCapacity;
        Array.Copy(playerInfo.Techs, 0, blitzData.TechsEquipped, playerIndex * 5, 5);
        WriteBlitzballData(blitzData);
    }
}

public class BlitzballStat
{
    public ushort HP { get; set; }
    public ushort SH { get; set; }
    public ushort PA { get; set; }
    public ushort EN { get; set; }
}

public class BlitzballPlayer
{
    public byte Level { get; set; }
    public ushort Experience { get; set; }
    public byte Contract { get; set; }
    public ushort Salary { get; set; }
    public ushort LeagueGoals { get; set; }
    public ushort TournamentGoals { get; set; }
    public byte TechCapacity { get; set; }
    public byte[] Techs { get; set; }
    public int SkillFlags1 { get; set; }
    public int SkillFlags2 { get; set; }
}

[StructLayout(LayoutKind.Sequential, Pack = 0, Size = 2080)]
public struct BlitzballData
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public int[] AbilityFlags1;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public int[] AbilityFlags2;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 134)]
    public byte[] unknown_0;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 300)]
    public byte[] TechsEquipped;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public byte[] TechsAvailable;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public byte[] PlayerLevels;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] unknown_2;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
    public byte[] TeamData;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 27)]
    public byte[] unknown_3;
    [MarshalAs(UnmanagedType.U1)]
    public byte LeagueStatus;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] LeagueMatchups;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] TournamentMatchups;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] TournamentWinners;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] TeamLeagueWins;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] TeamLeagueLosses;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 45)]
    public byte[] unknown_7;
    public byte TournamentStatus;
    public byte unknown_8;
    public byte GoalCount;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public byte[] GoalScorers;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] unknown_4;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public byte[] PlayerContracts;
    [MarshalAs(UnmanagedType.U2)]
    public ushort TeamWins;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public ushort[] PlayerExperience;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 192)]
    public byte[] unknown_5;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 248)]
    public byte[] unknown_6;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public ushort[] PlayerSalary;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public ushort[] BlitzballPrizes;
}
