using System.Collections.Generic;
using Farplane.Memory;

namespace Farplane.FFX.Data;

public static class Skills
{
    static int _offsetSkillTable =>
        GameMemory.Read<int>(OffsetScanner.GetOffset(GameOffset.FFX_SkillTablePointer), false);

    static KernelTable _skillTable;

    static void LoadTable()
    {
        var tableOffset = _offsetSkillTable;
        if (tableOffset != 0)
        {
            _skillTable = new KernelTable(tableOffset);
        }
    }

    public static List<string> GetSkillNames()
    {
        LoadTable();
        var names = new List<string>();
        if (_skillTable == null)
        {
            return names;
        }

        for (var i = 0; i < _skillTable.BlockCount; i++)
        {
            var skillName = _skillTable.GetString1(i);
            names.Add(skillName);
        }
        return names;
    }

    public static string GetSkillName(int skillIndex)
    {
        if (_skillTable == null)
        {
            LoadTable();
        }
        return _skillTable?.GetString1(skillIndex) ?? string.Empty;
    }

    public static string GetSkillDescription(int skillIndex)
    {
        if (_skillTable == null)
        {
            LoadTable();
        }
        return _skillTable?.GetString2(skillIndex) ?? string.Empty;
    }
}

public struct SkillData
{
    public ushort unknown_1;
    public ushort unknown_2;
    public ushort unknown_3;
    public ushort Animation1;
    public ushort Animation2;
}
