using System.Collections.Generic;

namespace Farplane.FFX.Data;

public static class Skills
{
    static KernelTable _commandTable;
    static KernelTable _itemTable;

    public static KernelTable GetTable(KernelType type)
    {
        if (type == KernelType.Command)
        {
            _commandTable ??= new KernelTable(KernelType.Command);
            return _commandTable;
        }

        _itemTable ??= new KernelTable(KernelType.Item);
        return _itemTable;
    }

    public static void Reset()
    {
        _commandTable = null;
        _itemTable = null;
    }

    public static List<string> GetNames(KernelType type = KernelType.Command)
    {
        var table = GetTable(type);
        var names = new List<string>();

        if (table == null || table.BufferAddress == 0)
        {
            return names;
        }

        for (var i = 0; i < table.EntryCount; i++)
        {
            names.Add(table.GetName(i));
        }

        return names;
    }

    public static string GetSkillName(int index, KernelType type = KernelType.Command)
    {
        return GetTable(type)?.GetName(index) ?? string.Empty;
    }

    public static string GetSkillDescription(int index, KernelType type = KernelType.Command)
    {
        return GetTable(type)?.GetDescription(index) ?? string.Empty;
    }

    public static KernelCommandData ReadData(int index, KernelType type = KernelType.Command)
    {
        return GetTable(type)?.ReadCommandData(index) ?? default;
    }

    public static void WriteData(int index, KernelCommandData data, KernelType type = KernelType.Command)
    {
        GetTable(type)?.WriteCommandData(index, data);
    }
}
