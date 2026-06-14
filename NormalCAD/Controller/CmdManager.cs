using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using NormalCAD.Core;
using NormalCAD.Core.Entities;
using NormalCAD.Controller.Commands;
using NormalCAD.Controller.Services;

namespace NormalCAD.Controller
{
    public class CmdManager(CadController cadController)
    {
        private readonly CadController _controller = cadController;

        public async Task ExecuteCommand(string cmdName)
        {
            switch (cmdName)
            {
                case "file.open":
                    await OpenDxfCmd();
                    break;
                case "file.save":
                    await SaveDxfCmd();
                    break;
                case "file.exit":
                    ExitCmd();
                    break;
                case "change_theme":
                    ToggleTheme();
                    break;
                case "edit.select":
                    _controller.SetCommand(new BaseCommand());
                    break;
                case "edit.erase":
                    _controller.SetCommand(new EraseCommand());
                    break;
                case "edit.clean_all":
                    CleanAllCmd();
                    break;
                case "draw.line":
                    _controller.SetCommand(new DrawLineCommand());
                    break;
                case "draw.circle":
                    _controller.SetCommand(new DrawCircleCommand());
                    break;
                default:
                    await ShowUnimplementedMessage(cmdName);
                    break;
            }
        }

        private void ExitCmd()
        {
            var window = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            window?.Close();
        }

        private void ToggleTheme()
        {
            var isLight = !_controller.IsLightTheme;
            _controller!.IsLightTheme = isLight;

            Application.Current!.RequestedThemeVariant = isLight ? Avalonia.Styling.ThemeVariant.Light : Avalonia.Styling.ThemeVariant.Dark;

            _controller.Viewport.IsLightTheme = isLight;
            _controller.Viewport.InvalidateVisual();
        }

        private async Task OpenDxfCmd()
        {
            var window = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null) return;

            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Abrir Desenho DXF",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("AutoCAD DXF") { Patterns = new[] { "*.dxf" } } }
            });

            if (files.Count > 0)
            {
                string path = files[0].Path.LocalPath;
                try
                {
                    var db = DxfService.LoadDxf(path);
                    _controller.SetDatabase(db);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao abrir DXF: {ex.Message}");
                }
            }
        }

        private async Task SaveDxfCmd()
        {
            var window = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null) return;

            var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Salvar Desenho DXF",
                DefaultExtension = ".dxf",
                FileTypeChoices = new[] { new FilePickerFileType("AutoCAD DXF") { Patterns = new[] { "*.dxf" } } }
            });

            if (file != null)
            {
                string path = file.Path.LocalPath;
                try
                {
                    DxfService.SaveDxf(_controller.Database, path);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao salvar DXF: {ex.Message}");
                }
            }
        }

        private void CleanAllCmd()
        {
            using (var trans = _controller.Database.TransactionManager.StartTransaction())
            {
                if (_controller.Database.TryGetObject(_controller.Database.BlockTableId, out var btObj) && btObj is BlockTable bt)
                {
                    var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                    if (_controller.Database.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                    {
                        var ids = new List<ObjectId>(btr.GetEntityIds());
                        foreach (var id in ids)
                        {
                            btr.RemoveEntity(id);
                        }
                    }
                }
                trans.Commit();
            }
            _controller.Viewport.SelectedEntityIds.Clear();
            _controller.NotifySelectionChanged();
            _controller.NotifyDatabaseChanged();
        }

        private async Task ShowUnimplementedMessage(string cmdName)
        {
            var window = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            var dialog = new Window
            {
                Title = "Atenção",
                Width = 300,
                Height = 150,
                Content = new TextBlock { Text = $"Command: {cmdName}" }
            };
            await dialog.ShowDialog(window!);
        }
    }
}
