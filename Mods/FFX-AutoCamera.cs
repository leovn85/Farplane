using System;
using Farplane;
using Farplane.FarplaneMod;
using Farplane.FFX;
using Farplane.FFX.Values;
using Farplane.Memory;

public class AutoCameraMod : IFarplaneMod
{
    private bool _modActive = false;
    private static int OffsetBattlePointer => Offsets.GetOffset(OffsetType.BattlePlayerPointer);
    private static int OffsetCurrentRoom => Offsets.GetOffset(OffsetType.CurrentRoom);
    private static int OffsetCameraFlag =>
        Offsets.GetOffset(OffsetType.DebugFlags) + (int)DebugFlags.GamepadCamera;

    private static int updateTicks = 0;
    private static int lastRoom = 0;

    public void Configure(object parentWindow) { }
    public string ConfigButton => null;
    public bool AutoActivate => true;

    private static readonly ushort[] BannedRooms =
    [
        0x00B0, // Macalania/Bevelle
        0x00B1, // Macalania/Bevelle
        0x0133, // Monster Arena
        0x0149, // Bevelle to Calm Lands
    ];

    public string Name => "Auto Free Camera";
    public string Author => "Topher";
    public string Description =>
        "Automatically enables and disables the free camera debug option when entering combat and changing rooms.\n\nUse the right stick to move the camera. Can be glitchy, might need to change rooms to deactivate properly.";
    public GameType GameType => GameType.FFX;

    public ModState GetState() => _modActive ? ModState.Activated : ModState.Deactivated;

    public void Activate()
    {
        if (_modActive)
            return;
        lastRoom = -1;
        ModLogger.WriteLine("Activating free camera mod");
        _modActive = true;
    }

    public void Deactivate()
    {
        if (!_modActive || !GameMemory.IsAttached)
            return;
        ModLogger.WriteLine("Deactivating free camera mod");
        GameMemory.Write(OffsetCameraFlag, (byte)0);
        _modActive = false;
    }

    public void Update()
    {
        if (!_modActive || !GameMemory.IsAttached)
            return;

        updateTicks++;
        if (updateTicks < 50)
            return;
        updateTicks = 0;

        var battlePointer = GameMemory.Read<int>(OffsetBattlePointer);
        if (battlePointer == 0)
        {
            var currentRoom = GameMemory.Read<ushort>(OffsetCurrentRoom);
            if (Array.IndexOf(BannedRooms, currentRoom) != -1)
            {
                GameMemory.Write(OffsetCameraFlag, (byte)0);
                lastRoom = currentRoom;
                return;
            }

            if (currentRoom != lastRoom)
            {
                ModLogger.WriteLine("Room change detected: {0}", currentRoom.ToString("X2"));
                GameMemory.Write(OffsetCameraFlag, (byte)0);
                lastRoom = currentRoom;
                return;
            }

            GameMemory.Write(OffsetCameraFlag, (byte)1);
        }
        else
        {
            GameMemory.Write(OffsetCameraFlag, (byte)0);
        }
    }
}
