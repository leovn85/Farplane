using System;
using Farplane;
using Farplane.FarplaneMod;
using Farplane.Memory;

public class FFXBreakDamageLimit : IFarplaneMod
{
    private bool _modActive = false;
    private static int OffsetDamageLimit => OffsetScanner.GetOffset(GameOffset.FFX_DamageLimitPatch);
    private byte[] _originalBytes;

    public void Configure(object parentWindow) { }
    public string ConfigButton => null;
    public bool AutoActivate => true;
    public string Name => "Break Damage Limit";
    public string Author => "Topher";
    public string Description => "Remove the damage cap allowing damage to exceed 99999.";
    public GameType GameType => GameType.FFX;

    public ModState GetState() => _modActive ? ModState.Activated : ModState.Deactivated;

    public void Activate()
    {
        if (_modActive || !GameMemory.IsAttached)
            return;

        var targetOffset = OffsetDamageLimit;
        if (targetOffset == 0)
            return;

        ModLogger.WriteLine("Activating Break Damage Limit");

        // 32 bytes at 0x38ECE1:
        // mov ebx, 7FFFFFFFh (5 bytes)
        // jmp short +0x1B (2 bytes: jump to 0x38ED01 skip any limit check 9999)
        // 25 byte NOPs remaining
        const int patchLength = 32;
        if (_originalBytes == null)
        {
            _originalBytes = GameMemory.Read<byte>(targetOffset, patchLength, isRelative: false);
        }

        var patchBytes = new byte[patchLength];
        // mov ebx, 7FFFFFFFh
        patchBytes[0] = 0xBB;
        patchBytes[1] = 0xFF;
        patchBytes[2] = 0xFF;
        patchBytes[3] = 0xFF;
        patchBytes[4] = 0x7F;
        // jmp short +0x1B
        patchBytes[5] = 0xEB;
        patchBytes[6] = 0x1B;

        for (var i = 7; i < patchLength; i++)
        {
            patchBytes[i] = 0x90; // NOP
        }

        GameMemory.Write(targetOffset, patchBytes, isRelative: false);
        _modActive = true;
    }

    public void Deactivate()
    {
        if (!_modActive || !GameMemory.IsAttached)
            return;

        var targetOffset = OffsetDamageLimit;
        if (targetOffset != 0 && _originalBytes != null)
        {
            ModLogger.WriteLine("Deactivating Break Damage Limit");
            GameMemory.Write(targetOffset, _originalBytes, isRelative: false);
        }

        _modActive = false;
    }

    public void Update() { }
}
