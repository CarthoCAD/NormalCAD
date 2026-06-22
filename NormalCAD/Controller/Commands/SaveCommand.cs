using System;
using Avalonia.Input;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Commands
{
    public class SaveCommand : ICadCommand
    {
        public string Name => "_.SAVE";
        public string LocalName => "SAVE";
        public string Alias => "";
        public bool IsInternal => false;

        public async void Activate(CadController controller)
        {
            string filePath = controller.Document.FilePath;

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    controller.SaveViewportState();
                    Services.FileService.Save(controller.Database, filePath);
                    controller.InputManager.SetPromptMessage($"Desenho salvo: {System.IO.Path.GetFileName(filePath)}");
                }
                catch (Exception ex)
                {
                    controller.InputManager.SetPromptMessage($"Erro ao salvar: {ex.Message}");
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
            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null) return;

            var file = await window.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Salvar Desenho",
                DefaultExtension = ".dwg",
                FileTypeChoices = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("AutoCAD DWG") { Patterns = new[] { "*.dwg" } },
                    new Avalonia.Platform.Storage.FilePickerFileType("AutoCAD DXF") { Patterns = new[] { "*.dxf" } }
                }
            });

            if (file != null)
            {
                string path = file.Path.LocalPath;
                try
                {
                    controller.SaveViewportState();
                    Services.FileService.Save(controller.Database, path);
                    controller.Document.FilePath = path;
                    controller.Document.Name = System.IO.Path.GetFileName(path);
                    controller.InputManager.SetPromptMessage($"Desenho salvo: {System.IO.Path.GetFileName(path)}");
                }
                catch (Exception ex)
                {
                    controller.InputManager.SetPromptMessage($"Erro ao salvar: {ex.Message}");
                }
            }
        }

        public void Deactivate() { }
        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(KeyEventArgs e) { }
    }
}
