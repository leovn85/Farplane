using System;
using Farplane;
using Farplane.FarplaneMod;
using Farplane.Memory;

public class FFXBreakHPLimit : IFarplaneMod
{
    private bool _modActive = false;

    private static int OffsetHPIn => OffsetScanner.GetOffset(GameOffset.FFX_HPLimitInBattle);
    private static int OffsetMPIn => OffsetScanner.GetOffset(GameOffset.FFX_MPLimitInBattle);
    private static int OffsetHPOut => OffsetScanner.GetOffset(GameOffset.FFX_HPLimitOutOfBattle);
    private static int OffsetMPOut => OffsetScanner.GetOffset(GameOffset.FFX_MPLimitOutOfBattle);

    private byte[] _origHPIn;
    private byte[] _origMPIn;
    private byte[] _origHPOut;
    private byte[] _origMPOut;

    public void Configure(object parentWindow) { }
    public string ConfigButton => null;
    public bool AutoActivate => true;
    public string Name => "Break HP Limit";
    public string Author => "Topher";
    public string Description => "Remove the HP/MP cap allowing HP and MP to exceed 99999.";
    public GameType GameType => GameType.FFX;

    public ModState GetState() => _modActive ? ModState.Activated : ModState.Deactivated;

    // mov eax, 7FFFFFFFh (5 bytes) + 5 NOPs = 10 bytes overwrite and eax, ...; add eax, ...
    private static readonly byte[] Patch10Bytes =
    [
        0xB8, 0xFF, 0xFF, 0xFF, 0x7F, // mov eax, 7FFFFFFFh
        0x90, 0x90, 0x90, 0x90, 0x90  // 5x NOP
    ];

    public void Activate()
    {
        if (_modActive || !GameMemory.IsAttached)
            return;

        ModLogger.WriteLine("Activating Break HP/MP Limit");

        var hpIn = OffsetHPIn;
        var mpIn = OffsetMPIn;
        var hpOut = OffsetHPOut;
        var mpOut = OffsetMPOut;

        if (hpIn == 0 || mpIn == 0 || hpOut == 0 || mpOut == 0)
            return;

        if (_origHPIn == null)
            _origHPIn = GameMemory.Read<byte>(hpIn, 10, isRelative: false);
        if (_origMPIn == null)
            _origMPIn = GameMemory.Read<byte>(mpIn, 10, isRelative: false);
        if (_origHPOut == null)
            _origHPOut = GameMemory.Read<byte>(hpOut, 10, isRelative: false);
        if (_origMPOut == null)
            _origMPOut = GameMemory.Read<byte>(mpOut, 10, isRelative: false);

        GameMemory.Write(hpIn, Patch10Bytes, isRelative: false);
        GameMemory.Write(mpIn, Patch10Bytes, isRelative: false);
        GameMemory.Write(hpOut, Patch10Bytes, isRelative: false);
        GameMemory.Write(mpOut, Patch10Bytes, isRelative: false);

        _modActive = true;
    }

    public void Deactivate()
    {
        if (!_modActive || !GameMemory.IsAttached)
            return;

        ModLogger.WriteLine("Deactivating Break HP/MP Limit");

        var hpIn = OffsetHPIn;
        var mpIn = OffsetMPIn;
        var hpOut = OffsetHPOut;
        var mpOut = OffsetMPOut;

        if (_origHPIn != null && hpIn != 0)
            GameMemory.Write(hpIn, _origHPIn, isRelative: false);
        if (_origMPIn != null && mpIn != 0)
            GameMemory.Write(mpIn, _origMPIn, isRelative: false);
        if (_origHPOut != null && hpOut != 0)
            GameMemory.Write(hpOut, _origHPOut, isRelative: false);
        if (_origMPOut != null && mpOut != 0)
            GameMemory.Write(mpOut, _origMPOut, isRelative: false);

        _modActive = false;
    }

    public void Update() { }
}
