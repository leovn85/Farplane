using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Farplane;
using Farplane.Common;
using Farplane.FarplaneMod;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using Farplane.Memory;
using MahApps.Metro.Controls;

public class SeymourMod : IFarplaneMod
{
    private bool _modActive = false;

    private static int OffsetModBytes1 => OffsetScanner.GetOffset(GameOffset.FFX_SeymourMenuCheck1);
    private static int OffsetModBytes2 => OffsetScanner.GetOffset(GameOffset.FFX_SeymourMenuCheck2);

    private static int OffsetSeymourData =>
        OffsetScanner.GetOffset(GameOffset.FFX_PartyStatBase) + (7 * StructHelper.GetSize<PartyMember>());

    private static int OffsetSeymourInParty =>
        OffsetSeymourData + StructHelper.GetFieldOffset<PartyMember>(nameof(PartyMember.InParty));

    private byte[] _origBytes1;
    private byte[] _origBytes2;

    private static readonly byte[] NopBytes4 = [0x90, 0x90, 0x90, 0x90];

    public void Configure(object parentWindow)
    {
        var win = new MetroWindow
        {
            Title = "Configuration",
            Width = 300,
            Height = 100,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Owner = (Window)parentWindow,
            BorderThickness = new Thickness(0),
            GlowBrush = Brushes.Black,
            ResizeMode = ResizeMode.NoResize,
        };
        var check = new CheckBox
        {
            Content = "Add/remove Seymour automatically",
            Margin = new Thickness(5),
            IsChecked = ModSettings.ReadSetting<bool>("AutoAddRemove"),
        };
        var butt = new Button { Content = "Save", Margin = new Thickness(5) };
        var stack = new StackPanel { Children = { check, butt }, Margin = new Thickness(5) };
        butt.Click += (sender, args) =>
        {
            ModSettings.WriteSetting("AutoAddRemove", check.IsChecked);
            win.Close();
        };
        win.Content = stack;
        win.ShowDialog();
    }

    public string ConfigButton => "Configure";
    public bool AutoActivate => true;
    public string Name => "Seymour in Party";
    public string Author => "Topher";
    public string Description => "Adds Seymour to your party and enables him in the menu.";
    public GameType GameType => GameType.FFX;

    public ModState GetState() => _modActive ? ModState.Activated : ModState.Deactivated;

    public void Activate()
    {
        if (_modActive || !GameMemory.IsAttached)
            return;

        var addr1 = OffsetModBytes1;
        var addr2 = OffsetModBytes2;

        if (addr1 == 0 || addr2 == 0)
            return;

        if (_origBytes1 == null)
            _origBytes1 = GameMemory.Read<byte>(addr1, 4, isRelative: false);
        if (_origBytes2 == null)
            _origBytes2 = GameMemory.Read<byte>(addr2, 4, isRelative: false);

        // NOP 4 bytes of "cmp al, 7; jz ..." (3C 07 74 24)
        GameMemory.Write(addr1, NopBytes4, isRelative: false);
        GameMemory.Write(addr2, NopBytes4, isRelative: false);

        if (ModSettings.ReadSetting<bool>("AutoAddRemove"))
        {
            Party.AddCharacter(Character.Seymour);
            GameMemory.Write(OffsetSeymourInParty, (byte)17, isRelative: false);
        }

        _modActive = true;
    }

    public void Deactivate()
    {
        if (!_modActive || !GameMemory.IsAttached)
            return;

        var addr1 = OffsetModBytes1;
        var addr2 = OffsetModBytes2;

        if (_origBytes1 != null && addr1 != 0)
            GameMemory.Write(addr1, _origBytes1, isRelative: false);
        if (_origBytes2 != null && addr2 != 0)
            GameMemory.Write(addr2, _origBytes2, isRelative: false);

        if (ModSettings.ReadSetting<bool>("AutoAddRemove"))
        {
            Party.RemoveCharacter(Character.Seymour);
            GameMemory.Write(OffsetSeymourInParty, (byte)16, isRelative: false);
        }

        _modActive = false;
    }

    public void Update() { }
}
