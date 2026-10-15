//css_reference "..\bin\Debug\farplane.exe";
using System;
using System.Linq;
using System.Windows;
using Farplane;
using Farplane.Common;
using Farplane.FarplaneMod;
using Farplane.FFX;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using Farplane.Memory;

public class FFXBreakHPLimit : IFarplaneMod
{
    private bool _modActive = false;
    private static int _offsetHPLimit = 0x386897;
    private static int _offsetMPLimit = 0x3868BF;
    private static int _offsetHPCheck = 0x38D389;
    private static int _offsetMPCheck = 0x38E5FE;

    public void Configure(object parentWindow) { }

    public string ConfigButton
    {
        get { return null; }
    }
    public bool AutoActivate
    {
        get { return true; }
    }
    public string Name
    {
        get { return "Break HP Limit"; }
    }

    public string Author
    {
        get { return "Topher"; }
    }

    public string Description
    {
        get { return "Remove the HP/MP cap allowing HP and MP to exceed 99999."; }
    }

    public GameType GameType
    {
        get { return GameType.FFX; }
    }

    public ModState GetState()
    {
        if (_modActive)
            return ModState.Activated;
        return ModState.Deactivated;
    }

    private byte[] _originalHPLimit;
    private byte[] _originalMPLimit;
    private byte[] _originalHPCheck;
    private byte[] _originalMPCheck;

    public void Activate()
    {
        if (_modActive)
            return;
        ModLogger.WriteLine("Activating Break HP/MP Limit");

        var bytesHPLimit = GameMemory.Assembly.Generate("mov eax,7FFFFFFF", _offsetHPLimit);
        var bytesMPLimit = GameMemory.Assembly.Generate("mov eax,7FFFFFFF", _offsetMPLimit);
        var bytesHPCheck = GameMemory.Assembly.Generate(
            new[] { "and eax, 7FFFFFFF", "nop", "nop", "nop", "nop", "nop" },
            _offsetHPCheck
        );
        var bytesMPCheck = GameMemory.Assembly.Generate("push 7fffffff", _offsetMPCheck);

        // Lưu lại byte gốc trước khi ghi đè
        if (_originalHPLimit == null)
            _originalHPLimit = GameMemory.Read<byte>(_offsetHPLimit, bytesHPLimit.Length);
        if (_originalMPLimit == null)
            _originalMPLimit = GameMemory.Read<byte>(_offsetMPLimit, bytesMPLimit.Length);
        if (_originalHPCheck == null)
            _originalHPCheck = GameMemory.Read<byte>(_offsetHPCheck, bytesHPCheck.Length);
        if (_originalMPCheck == null)
            _originalMPCheck = GameMemory.Read<byte>(_offsetMPCheck, bytesMPCheck.Length);

        // Tiêm đúng vào cả 4 địa chỉ tương ứng
        GameMemory.Write(_offsetHPLimit, bytesHPLimit);
        GameMemory.Write(_offsetMPLimit, bytesMPLimit);
        GameMemory.Write(_offsetHPCheck, bytesHPCheck);
        GameMemory.Write(_offsetMPCheck, bytesMPCheck);

        _modActive = true;
    }
    public void Deactivate()
    {
        if (!_modActive)
            return;
        ModLogger.WriteLine("Deactivating Break HP/MP Limit");

        // Khôi phục lại toàn bộ mã máy gốc
        if (_originalHPLimit != null)
            GameMemory.Write(_offsetHPLimit, _originalHPLimit);
        if (_originalMPLimit != null)
            GameMemory.Write(_offsetMPLimit, _originalMPLimit);
        if (_originalHPCheck != null)
            GameMemory.Write(_offsetHPCheck, _originalHPCheck);
        if (_originalMPCheck != null)
            GameMemory.Write(_offsetMPCheck, _originalMPCheck);

        _modActive = false;
    }

    public static void ValidateBytes(byte[] bytes)
    {
        // Verify bytes against original bytes here
    }

    public void Update()
    {
        return;
    }
}
