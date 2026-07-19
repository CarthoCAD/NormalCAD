using System;
using System.Threading.Tasks;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Commands
{
    public class OpenCommand : ICadCommand
    {
        private static string DialogTitle => DialogResources.Get("FILEDIALOG.TITLE.OPEN");
        private static string FileTypeGroup => DialogResources.Get("FILEDIALOG.FILETYPE.GROUP");
        private static string FileTypeDwg => DialogResources.Get("FILEDIALOG.FILETYPE.DWG");
        private static string FileTypeDxf => DialogResources.Get("FILEDIALOG.FILETYPE.DXF");
        private static string MsgLoaded => CommandResources.Get("OPEN.MSG.LOADED");
        private static string MsgError => CommandResources.Get("OPEN.MSG.ERROR");

        public string Name => "_.OPEN";
        public string LocalName => CommandResources.Get("OPEN.LOCALNAME");
        public CommandType Type => CommandType.Immediate;
        public CommandFlags Flags => CommandFlags.None;
        public string Alias => "";

        public async Task ActivateAsync(CadController controller)
        {
            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window != null)
            {
                var files = await window.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                {
                    Title = DialogTitle,
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new Avalonia.Platform.Storage.FilePickerFileType(FileTypeGroup) { Patterns = new[] { "*.dwg", "*.dxf" } },
                        new Avalonia.Platform.Storage.FilePickerFileType(FileTypeDwg) { Patterns = new[] { "*.dwg" } },
                        new Avalonia.Platform.Storage.FilePickerFileType(FileTypeDxf) { Patterns = new[] { "*.dxf" } }
                    }
                });

                if (files.Count > 0)
                {
                    string path = files[0].Path.LocalPath;
                    try
                    {
                        var db = Services.FileService.Load(path);
                        controller.SetDatabase(db, path);
                        controller.InputManager.SetPromptMessage(string.Format(MsgLoaded, System.IO.Path.GetFileName(path)));
                    }
                    catch (Exception ex)
                    {
                        controller.InputManager.SetPromptMessage(string.Format(MsgError, ex.Message));
                    }
                }
            }
            controller.SetCommand(new BaseCommand());
        }

        public void Deactivate() { }
    }
}
