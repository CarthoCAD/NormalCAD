using System;
using Avalonia.Input;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Commands
{
    public class SaveDwgCommand : ICadCommand
    {
        public string Name => "_.DWGOUT";
        public string LocalName => "DWGOUT";
        public string Alias => "DWGS";
        public bool IsInternal => false;

        public async void Activate(CadController controller)
        {
            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window != null)
            {
                var file = await window.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
                {
                    Title = "Salvar Desenho DWG",
                    DefaultExtension = ".dwg",
                    FileTypeChoices = new[] { new Avalonia.Platform.Storage.FilePickerFileType("AutoCAD DWG") { Patterns = new[] { "*.dwg" } } }
                });

                if (file != null)
                {
                    string path = file.Path.LocalPath;
                    try
                    {
                        controller.SaveViewportState();
                        Services.DwgService.SaveDwg(controller.Database, path);
                        controller.InputManager.SetPromptMessage($"DWG salvo: {System.IO.Path.GetFileName(path)}");
                    }
                    catch (Exception ex)
                    {
                        controller.InputManager.SetPromptMessage($"Erro ao salvar DWG: {ex.Message}");
                    }
                }
            }
            controller.SetCommand(new BaseCommand());
        }

        public void Deactivate() { }
        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e) { }
        public void OnPointerMoved(Point3d worldPt) { }
        public void OnKeyDown(KeyEventArgs e) { }
    }
}
