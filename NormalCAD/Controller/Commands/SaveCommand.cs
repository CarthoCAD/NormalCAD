using System;
using System.Threading.Tasks;
using NormalCAD.Core.ApplicationServices;
using NormalCAD.Core.Geometry;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Commands
{
    public class SaveCommand : ICadCommand
    {
        private static string MsgSaved => CommandResources.Get("SAVE.MSG.SAVED");
        private static string MsgError => CommandResources.Get("SAVE.MSG.ERROR");
        private static string DialogTitle => DialogResources.Get("FILEDIALOG.TITLE.SAVE");
        private static string FileTypeDwg => DialogResources.Get("FILEDIALOG.FILETYPE.DWG");
        private static string FileTypeDxf => DialogResources.Get("FILEDIALOG.FILETYPE.DXF");

        public string Name => "_.SAVE";
        public string LocalName => CommandResources.Get("SAVE.LOCALNAME");
        public CommandType Type => CommandType.Immediate;
        public CommandFlags Flags => CommandFlags.None;
        public string Alias => "";

        public async Task ActivateAsync()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                CadController.Current.FinishCommand();
                return;
            }

            var db = doc.Database;
            string filePath = db.Filename;

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    CadController.Current.SaveViewportState();
                    Services.FileService.Save(db, filePath);
                    CadController.Current.InputManager.SetPromptMessage(string.Format(MsgSaved, System.IO.Path.GetFileName(filePath)));
                }
                catch (Exception ex)
                {
                    CadController.Current.InputManager.SetPromptMessage(string.Format(MsgError, ex.Message));
                }
            }
            else
            {
                await ShowSaveDialog();
            }

            CadController.Current.FinishCommand();
        }

        public static async Task ShowSaveDialog()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            var db = doc.Database;

            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null) return;

            var file = await window.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = DialogTitle,
                DefaultExtension = ".dwg",
                FileTypeChoices =
                [
                    new Avalonia.Platform.Storage.FilePickerFileType(FileTypeDwg) { Patterns = new[] { "*.dwg" } },
                    new Avalonia.Platform.Storage.FilePickerFileType(FileTypeDxf) { Patterns = new[] { "*.dxf" } }
                ]
            });

            if (file != null)
            {
                string path = file.Path.LocalPath;
                try
                {
                    CadController.Current.SaveViewportState();
                    Services.FileService.Save(db, path);
                    db.Filename = path;
                    CadController.Current.InputManager.SetPromptMessage(string.Format(MsgSaved, System.IO.Path.GetFileName(path)));
                }
                catch (Exception ex)
                {
                    CadController.Current.InputManager.SetPromptMessage(string.Format(MsgError, ex.Message));
                }
            }
        }

        public void Deactivate() { }
    }
}
