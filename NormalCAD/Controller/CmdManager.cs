using System.Threading.Tasks;

namespace NormalCAD.Controller
{
    public class CmdManager(CadController cadController)
    {
        private readonly CadController _controller = cadController;

        public async Task ExecuteCommand(string cmdName)
        {
            switch (cmdName)
            {
                case "file.exit":
                    ExitCmd();
                    break;
                case "change_theme":
                    ToggleTheme();
                    break;
                default:
                    await ShowUnimplementedMessage(cmdName);
                    break;
            }
        }

        private void ExitCmd()
        {
            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            window?.Close();
        }

        private void ToggleTheme()
        {
            var isLight = !_controller.IsLightTheme;
            _controller!.IsLightTheme = isLight;

            Avalonia.Application.Current!.RequestedThemeVariant = isLight ? Avalonia.Styling.ThemeVariant.Light : Avalonia.Styling.ThemeVariant.Dark;

            _controller.Viewport.IsLightTheme = isLight;

            _controller.Viewport.InvalidateVisual();
        }

        private async Task ShowUnimplementedMessage(string cmdName)
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
        }

    }
    
}
