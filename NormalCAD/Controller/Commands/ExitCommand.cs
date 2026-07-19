using Avalonia;
using System.Threading.Tasks;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Commands
{
    public class ExitCommand : ICadCommand
    {
        public string Name => "_.QUIT";
        public string LocalName => CommandResources.Get("QUIT.LOCALNAME");
        public CommandType Type => CommandType.Immediate;
        public CommandFlags Flags => CommandFlags.None;
        public string Alias => CommandResources.Get("QUIT.ALIAS");

        public Task ActivateAsync(CadController controller)
        {
            var window = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            window?.Close();
            return Task.CompletedTask;
        }

        public void Deactivate() { }
    }
}
