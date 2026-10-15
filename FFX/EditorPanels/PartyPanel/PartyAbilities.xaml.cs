using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Farplane.Common;
using Farplane.Common.Controls;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace Farplane.FFX.EditorPanels.PartyPanel;

/// <summary>
/// Interaction logic for PartyAbilities.xaml
/// </summary>
public partial class PartyAbilities : UserControl
{
    readonly ButtonGrid _gridSkill = new(2, 22);
    readonly ButtonGrid _gridSpecial = new(2, 22);
    readonly ButtonGrid _gridWhiteMagic = new(2, 22);
    readonly ButtonGrid _gridBlackMagic = new(2, 19);

    readonly Ability[] _skills = Ability
        .Abilities.Where(a => a.Type == AbilityType.Skill)
        .ToArray();
    readonly Ability[] _specials = Ability
        .Abilities.Where(a => a.Type == AbilityType.Special)
        .ToArray();
    readonly Ability[] _wMagic = Ability
        .Abilities.Where(a => a.Type == AbilityType.WhiteMagic)
        .ToArray();
    readonly Ability[] _bMagic = Ability
        .Abilities.Where(a => a.Type == AbilityType.BlackMagic)
        .ToArray();
    int _characterIndex = -1;

    static readonly Tuple<AppTheme, Accent> currentStyle = ThemeManager.DetectAppStyle(
        Application.Current
    );

    readonly Brush _trueAbilityBrush = new SolidColorBrush(
        (Color)currentStyle.Item1.Resources["BlackColor"]
    );

    readonly Brush _falseAbilityBrush = new SolidColorBrush(
        (Color)currentStyle.Item1.Resources["Gray2"]
    );

    public PartyAbilities()
    {
        this.InitializeComponent();
        foreach (var tabItem in this.TabAbilities.Items)
        {
            ControlsHelper.SetHeaderFontSize((TabItem)tabItem, 14);
        }

        this.TabSkills.Content = this._gridSkill;
        this.TabSpecial.Content = this._gridSpecial;
        this.TabWhiteMagic.Content = this._gridWhiteMagic;
        this.TabBlackMagic.Content = this._gridBlackMagic;

        this._gridSkill.ButtonClicked += this.ButtonSkill_Click;
        this._gridSpecial.ButtonClicked += this.ButtonSpecial_Click;
        this._gridWhiteMagic.ButtonClicked += this.ButtonWhiteMagic_Click;
        this._gridBlackMagic.ButtonClicked += this.ButtonBlackMagic_Click;

        this._gridSkill.ShowScrollBar = false;
        this._gridSpecial.ShowScrollBar = false;
        this._gridWhiteMagic.ShowScrollBar = false;
        this._gridBlackMagic.ShowScrollBar = false;
    }

    void ButtonSkill_Click(int buttonIndex)
    {
        this.ToggleSkill(AbilityType.Skill, buttonIndex);
        this.Refresh(this._characterIndex);
    }

    void ButtonSpecial_Click(int buttonIndex)
    {
        this.ToggleSkill(AbilityType.Special, buttonIndex);
        this.Refresh(this._characterIndex);
    }

    void ButtonWhiteMagic_Click(int buttonIndex)
    {
        this.ToggleSkill(AbilityType.WhiteMagic, buttonIndex);
        this.Refresh(this._characterIndex);
    }

    void ButtonBlackMagic_Click(int buttonIndex)
    {
        this.ToggleSkill(AbilityType.BlackMagic, buttonIndex);
        this.Refresh(this._characterIndex);
    }

    void ToggleSkill(AbilityType type, int buttonIndex)
    {
        if (this._characterIndex == -1)
        {
            return;
        }

        Ability skill = null;
        switch (type)
        {
            case AbilityType.Skill:
                skill = this._skills[buttonIndex];
                break;
            case AbilityType.Special:
                skill = this._specials[buttonIndex];
                break;
            case AbilityType.WhiteMagic:
                skill = this._wMagic[buttonIndex];
                break;
            case AbilityType.BlackMagic:
                skill = this._bMagic[buttonIndex];
                break;
            default:
                return;
        }

        Party.ToggleSkillFlag(this._characterIndex, skill.BitOffset);
    }

    public void Refresh(int characterIndex)
    {
        this._characterIndex = characterIndex;

        if (this._characterIndex == -1)
        {
            return;
        }

        var party = Party.ReadPartyMember(this._characterIndex);

        var skillArray = BitHelper.GetBitArray(party.SkillFlags);

        for (var i = 0; i < this._skills.Length; i++)
        {
            var button = (Button)this._gridSkill.GridBase.Children[i];
            button.Foreground = skillArray[this._skills[i].BitOffset]
                ? this._trueAbilityBrush
                : this._falseAbilityBrush;
            button.Content = this._skills[i].Name;
        }

        for (var i = 0; i < this._specials.Length; i++)
        {
            var button = (Button)this._gridSpecial.GridBase.Children[i];
            button.Foreground = skillArray[this._specials[i].BitOffset]
                ? this._trueAbilityBrush
                : this._falseAbilityBrush;
            button.Content = this._specials[i].Name;
        }

        for (var i = 0; i < this._wMagic.Length; i++)
        {
            var button = (Button)this._gridWhiteMagic.GridBase.Children[i];
            button.Foreground = skillArray[this._wMagic[i].BitOffset]
                ? this._trueAbilityBrush
                : this._falseAbilityBrush;
            button.Content = this._wMagic[i].Name;
        }

        for (var i = 0; i < this._bMagic.Length; i++)
        {
            var button = (Button)this._gridBlackMagic.GridBase.Children[i];
            button.Foreground = skillArray[this._bMagic[i].BitOffset]
                ? this._trueAbilityBrush
                : this._falseAbilityBrush;
            button.Content = this._bMagic[i].Name;
        }
    }
}
