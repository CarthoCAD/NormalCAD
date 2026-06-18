using System;
using Avalonia.Input;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Commands
{
    public class OpenDwgCommand : ICadCommand
    {
        public string Name => "_.DWGIN";
        public string LocalName => "DWGIN";
        public string Alias => "DWG";
        public bool IsInternal => false;

        public async void Activate(CadController controller)
        {
            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window != null)
            {
                var files = await window.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                {
                    Title = "Abrir Desenho DWG",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { new Avalonia.Platform.Storage.FilePickerFileType("AutoCAD DWG") { Patterns = new[] { "*.dwg" } } }
                });

                if (files.Count > 0)
                {
                    string path = files[0].Path.LocalPath;
                    try
                    {
                        var db = Services.DwgService.LoadDwg(path);
                        controller.SetDatabase(db);
                        controller.InputManager.SetPromptMessage($"DWG carregado: {System.IO.Path.GetFileName(path)}");
                    }
                    catch (Exception ex)
                    {
                        controller.InputManager.SetPromptMessage($"Erro ao abrir DWG: {ex.Message}");
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
