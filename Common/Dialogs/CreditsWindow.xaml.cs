using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MahApps.Metro.Controls;

namespace Farplane.Common.Dialogs;

/// <summary>
/// Interaction logic for CreditsWindow.xaml
/// </summary>
public partial class CreditsWindow : MetroWindow
{
    public CreditsWindow()
    {
        this.InitializeComponent();

        this.TextMarquee.Text = string.Empty;

        var creditsText = Properties.Resources.Credits;
        var creditsStream = new StringReader(creditsText);
        var creditsLine = creditsStream.ReadLine();

        while (creditsLine != null)
        {
            this.TextMarquee.Text += creditsLine + '\n';
            creditsLine = creditsStream.ReadLine();
        }
        this.TextMarquee.InvalidateVisual();
    }

    void CreditsWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var doubleAnimation = new DoubleAnimation
        {
            From = -this.TextMarquee.ActualHeight,
            To = this.GridMain.ActualHeight,
            RepeatBehavior = RepeatBehavior.Forever,
            Duration = new Duration(TimeSpan.Parse("0:0:30")),
        };
        this.TextMarquee.BeginAnimation(Canvas.BottomProperty, doubleAnimation);
    }

    void Canvas_MouseDown(object sender, MouseButtonEventArgs e) => this.Close();
}
