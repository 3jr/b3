using System.Windows;
using System.Windows.Input;

namespace BallOnTiltablePlate.JanRapp.MainApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void BackupB3SettingsSaverSavesCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            GlobalSettings.BackupSettingsSaves();
        }
    }
}