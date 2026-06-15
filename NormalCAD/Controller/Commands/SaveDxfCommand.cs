using System;
using Avalonia.Input;
using NormalCAD.Core.Geometry;

namespace NormalCAD.Controller.Commands
{
    public class SaveDxfCommand : ICadCommand
    {
        public string Name => "_.DXFOUT";
        public string LocalName => "DXFOUT";
        public string Alias => "DXFO";
        public bool IsInternal => false;

        public async void Activate(CadController controller)
        {
            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window != null)
            {
                var file = await window.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
                {
                    Title = "Salvar Desenho DXF",
                    DefaultExtension = ".dxf",
                    FileTypeChoices = new[] { new Avalonia.Platform.Storage.FilePickerFileType("AutoCAD DXF") { Patterns = new[] { "*.dxf" } } }
                });

                if (file != null)
                {
                    string path = file.Path.LocalPath;
                    try
                    {
                        Services.DxfService.SaveDxf(controller.Database, path);
                        controller.InputManager.SetPromptMessage($"DXF salvo: {System.IO.Path.GetFileName(path)}");
                    }
                    catch (Exception ex)
                    {
                        controller.InputManager.SetPromptMessage($"Erro ao salvar DXF: {ex.Message}");
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
