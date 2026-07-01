using System;
using Avalonia.Input;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Commands
{
    public class OpenCommand : ICadCommand
    {
        public string Name => "_.OPEN";
        public string LocalName => "OPEN";
        public string Alias => "";
        public bool IsInternal => false;

        public async void Activate(CadController controller)
        {
            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window != null)
            {
                var files = await window.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                {
                    Title = "Open Drawing",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new Avalonia.Platform.Storage.FilePickerFileType("Drawing Files") { Patterns = new[] { "*.dwg", "*.dxf" } },
                        new Avalonia.Platform.Storage.FilePickerFileType("AutoCAD DWG") { Patterns = new[] { "*.dwg" } },
                        new Avalonia.Platform.Storage.FilePickerFileType("AutoCAD DXF") { Patterns = new[] { "*.dxf" } }
                    }
                });

                if (files.Count > 0)
                {
                    string path = files[0].Path.LocalPath;
                    try
                    {
                        var db = Services.FileService.Load(path);
                        controller.SetDatabase(db, path);
                        controller.InputManager.SetPromptMessage($"Drawing loaded: {System.IO.Path.GetFileName(path)}");
                    }
                    catch (Exception ex)
                    {
                        controller.InputManager.SetPromptMessage($"Error opening: {ex.Message}");
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
