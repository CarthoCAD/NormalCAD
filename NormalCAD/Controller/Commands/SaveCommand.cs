using System;
using Avalonia.Input;
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
        public string Alias => "";
        public bool IsInternal => false;

        public async void Activate(CadController controller)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                controller.SetCommand(new BaseCommand());
                return;
            }

            var db = doc.Database;
            string filePath = db.Filename;

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    controller.SaveViewportState();
                    Services.FileService.Save(db, filePath);
                    controller.InputManager.SetPromptMessage(string.Format(MsgSaved, System.IO.Path.GetFileName(filePath)));
                }
                catch (Exception ex)
                {
                    controller.InputManager.SetPromptMessage(string.Format(MsgError, ex.Message));
                }
            }
            else
            {
                await ShowSaveDialog(controller);
            }

            controller.SetCommand(new BaseCommand());
        }

        public static async System.Threading.Tasks.Task ShowSaveDialog(CadController controller)
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
                    controller.SaveViewportState();
                    Services.FileService.Save(db, path);
                    db.Filename = path;
                    controller.InputManager.SetPromptMessage(string.Format(MsgSaved, System.IO.Path.GetFileName(path)));
                }
                catch (Exception ex)
                {
                    controller.InputManager.SetPromptMessage(string.Format(MsgError, ex.Message));
                }
            }
        }

        public void Deactivate() { }
        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(KeyEventArgs e) { }
    }
}
