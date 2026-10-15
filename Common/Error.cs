using System.Windows;

namespace Farplane.Common;

public class Error
{
    public static void Show(string errorText) =>
        MessageBox.Show(errorText, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
}
