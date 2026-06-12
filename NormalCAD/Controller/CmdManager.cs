namespace NormalCAD.Controller
{
    public class CmdManager(CadController cadController)
    {
        private readonly CadController _cadController = cadController;

        public void ExecuteCommand(string cmdName)
        {
            switch (cmdName)
            {
                case "file.exit":
                    ExitCmd();
                    break;
                default:
                    ShowUnimplementedMessage(cmdName);
                    break;
            }
        }

        private void ExitCmd()
        {
            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            window?.Close();
        }

        private async void ShowUnimplementedMessage(string cmdName)
        {
            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            var dialog = new Avalonia.Controls.Window
            {
                Title = "Atenção",
                Width = 300,
                Height = 150,
                Content = new Avalonia.Controls.TextBlock { Text = $"Command: {cmdName}" }
            };
            await dialog.ShowDialog(window!);
            return;
        }

    }
    
}
