using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using NormalCAD.Core;
using NormalCAD.Core.Geometry;
using NormalCAD.Core.Entities;
using NormalCAD.Controller;
using NormalCAD.Controller.Commands;
using NormalCAD.Controller.Services;
using Avalonia;
using Avalonia.Styling;

namespace NormalCAD
{
    public partial class MainWindow : Window
    {
        private readonly CadController? _controller;

        public MainWindow()
        {
            InitializeComponent();

            // Configura o banco de dados inicial e o controlador
            _controller = new CadController(new Database(), Viewport);

            // Assina os eventos do controlador
            _controller.SelectionChanged += OnSelectionChanged;
            _controller.DatabaseChanged += UpdateLayersList;
            _controller.ActiveCommandChanged += OnActiveCommandChanged;

            MenuBar.Controller = _controller;

            // Fios de controle de eventos de mouse no viewport para atualizar coordenadas e snaps
            Viewport.PointerMoved += (s, e) =>
            {
                var screenPos = e.GetPosition(Viewport);
                var worldPos = Viewport.ScreenToWorld(screenPos);
                TxtCoordinates.Text = $"X: {worldPos.X:F4}, Y: {worldPos.Y:F4}";

                if (Viewport.ActiveSnapType != SnapType.None && Viewport.ActiveSnapPoint.HasValue)
                {
                    TxtSnapStatus.Text = $"Snap: {Viewport.ActiveSnapType} ({Viewport.ActiveSnapPoint.Value.X:F2}, {Viewport.ActiveSnapPoint.Value.Y:F2})";
                }
                else
                {
                    TxtSnapStatus.Text = "Snap: Nenhum";
                }
            };

            // Assina cliques de botões da barra de ferramentas
            BtnSelect.Click += (s, e) => { SetActiveToolButton(BtnSelect); _controller.SetCommand(new SelectCommand()); };
            BtnDrawLine.Click += (s, e) => { SetActiveToolButton(BtnDrawLine); _controller.SetCommand(new DrawLineCommand()); };
            BtnDrawCircle.Click += (s, e) => { SetActiveToolButton(BtnDrawCircle); _controller.SetCommand(new DrawCircleCommand()); };
            BtnDelete.Click += (s, e) => { DeleteSelected(); };
            BtnClearAll.Click += (s, e) => { ClearAll(); };

            // Ações do menu superior
            BtnOpen.Click += BtnOpen_Click;
            BtnSave.Click += BtnSave_Click;
            BtnTheme.Click += BtnTheme_Click;

            // Gerenciador de camadas
            BtnAddLayer.Click += BtnAddLayer_Click;
            LstLayers.SelectionChanged += LstLayers_SelectionChanged;

            // Inicializa a lista de camadas
            UpdateLayersList();
            SetActiveToolButton(BtnSelect);
        }

        private void SetActiveToolButton(Button activeBtn)
        {
            BtnSelect.Background = new SolidColorBrush(Color.Parse("#3E3E3E"));
            BtnDrawLine.Background = new SolidColorBrush(Color.Parse("#3E3E3E"));
            BtnDrawCircle.Background = new SolidColorBrush(Color.Parse("#3E3E3E"));

            activeBtn.Background = new SolidColorBrush(Color.Parse("#007ACC"));
        }

        private void OnActiveCommandChanged(string cmdName)
        {
            TxtActiveTool.Text = $"Ferramenta Ativa: {cmdName}";
            if (cmdName == "Seleção") SetActiveToolButton(BtnSelect);
            else if (cmdName == "Desenhar Linha") SetActiveToolButton(BtnDrawLine);
            else if (cmdName == "Desenhar Círculo") SetActiveToolButton(BtnDrawCircle);
        }

        private void DeleteSelected()
        {
            if (_controller == null) return;
            var selected = Viewport.SelectedEntityIds;
            if (selected.Count > 0)
            {
                using (var trans = _controller.Database.TransactionManager.StartTransaction())
                {
                    if (_controller.Database.TryGetObject(_controller.Database.BlockTableId, out var btObj) && btObj is BlockTable bt)
                    {
                        var modelSpaceId = bt[BlockTableRecord.ModelSpace];
                        if (_controller.Database.TryGetObject(modelSpaceId, out var btrObj) && btrObj is BlockTableRecord btr)
                        {
                            foreach (var entId in selected)
                            {
                                btr.RemoveEntity(entId);
                            }
                        }
                    }
                    trans.Commit();
                }
                selected.Clear();
                _controller.NotifySelectionChanged();
                _controller.NotifyDatabaseChanged();
            }
        }

        private void ClearAll()
        {
            if (_controller == null) return;
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
            Viewport.SelectedEntityIds.Clear();
            _controller.NotifySelectionChanged();
            _controller.NotifyDatabaseChanged();
        }

        private async void BtnOpen_Click(object? sender, RoutedEventArgs e)
        {
            if (_controller == null) return;

            var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
                    TxtActiveTool.Text = $"DXF carregado: {System.IO.Path.GetFileName(path)}";
                }
                catch (Exception ex)
                {
                    TxtActiveTool.Text = $"Erro ao abrir DXF: {ex.Message}";
                }
            }
        }

        private async void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            if (_controller == null) return;

            var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
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
                    TxtActiveTool.Text = $"DXF salvo: {System.IO.Path.GetFileName(path)}";
                }
                catch (Exception ex)
                {
                    TxtActiveTool.Text = $"Erro ao salvar DXF: {ex.Message}";
                }
            }
        }

        private void BtnTheme_Click(object? sender, RoutedEventArgs e)
        {
            _controller!.IsLightTheme = !_controller.IsLightTheme;
            ApplyTheme(_controller.IsLightTheme);
        }

        private void BtnAddLayer_Click(object? sender, RoutedEventArgs e)
        {
            if (_controller == null) return;
            string name = TxtNewLayerName.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(name))
            {
                try
                {
                    using (var trans = _controller.Database.TransactionManager.StartTransaction())
                    {
                        var lt = (LayerTable)trans.GetObject(_controller.Database.LayerTableId, OpenMode.ForWrite);
                        if (!lt.Has(name))
                        {
                            var rand = new Random();
                            var color = new EntityColor((byte)rand.Next(50, 255), (byte)rand.Next(50, 255), (byte)rand.Next(50, 255));
                            var layer = new LayerTableRecord(name, color);
                            lt.Add(layer);
                        }
                        trans.Commit();
                    }
                    TxtNewLayerName.Text = "";
                    _controller.NotifyDatabaseChanged();
                }
                catch (Exception ex)
                {
                    TxtActiveTool.Text = $"Erro ao criar camada: {ex.Message}";
                }
            }
        }

        private void LstLayers_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_controller == null) return;
            if (LstLayers.SelectedItem is LayerItem item)
            {
                _controller.ActiveLayer = item.Name;
            }
        }

        private void OnSelectionChanged()
        {
            if (_controller == null) return;
            var selectedIds = Viewport.SelectedEntityIds;
            if (selectedIds.Count != 1)
            {
                TxtPropsTitle.Text = selectedIds.Count == 0 ? "Nenhum objeto selecionado" : $"{selectedIds.Count} objetos selecionados";
                PanelLineProps.IsVisible = false;
                PanelCircleProps.IsVisible = false;
                return;
            }

            ObjectId id = ObjectId.Null;
            foreach (var selectedId in selectedIds) { id = selectedId; break; }

            if (_controller.Database.TryGetObject(id, out var dbObj))
            {
                if (dbObj is Line line)
                {
                    TxtPropsTitle.Text = "Linha";
                    PanelLineProps.IsVisible = true;
                    PanelCircleProps.IsVisible = false;

                    TxtLineStartX.Text = line.StartPoint.X.ToString("F4");
                    TxtLineStartY.Text = line.StartPoint.Y.ToString("F4");
                    TxtLineEndX.Text = line.EndPoint.X.ToString("F4");
                    TxtLineEndY.Text = line.EndPoint.Y.ToString("F4");
                    TxtLineLayer.Text = line.Layer;
                }
                else if (dbObj is Circle circle)
                {
                    TxtPropsTitle.Text = "Círculo";
                    PanelLineProps.IsVisible = false;
                    PanelCircleProps.IsVisible = true;

                    TxtCircleCenterX.Text = circle.Center.X.ToString("F4");
                    TxtCircleCenterY.Text = circle.Center.Y.ToString("F4");
                    TxtCircleRadius.Text = circle.Radius.ToString("F4");
                    TxtCircleLayer.Text = circle.Layer;
                }
                else
                {
                    TxtPropsTitle.Text = "Objeto Desconhecido";
                    PanelLineProps.IsVisible = false;
                    PanelCircleProps.IsVisible = false;
                }
            }
        }

        private void UpdateLayersList()
        {
            var list = new List<LayerItem>();
            if (_controller?.Database != null)
            {
                if (_controller.Database.TryGetObject(_controller.Database.LayerTableId, out var ltObj) && ltObj is LayerTable lt)
                {
                    foreach (var record in lt)
                    {
                        var brush = new SolidColorBrush(Color.FromArgb(record.Color.A, record.Color.R, record.Color.G, record.Color.B));
                        list.Add(new LayerItem { Name = record.Name, ColorBrush = brush });
                    }
                }
            }
            LstLayers.ItemsSource = list;

            if (_controller != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Name == _controller.ActiveLayer)
                    {
                        LstLayers.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void ApplyTheme(bool isLight)
        {
            Application.Current!.RequestedThemeVariant = isLight ? ThemeVariant.Light : ThemeVariant.Dark;

            Viewport.IsLightTheme = isLight;
            
            var panelBg = isLight ? "#EAEAEA" : "#252526";
            var topBarBg = isLight ? "#DCDCDC" : "#2D2D2D";
            var bottomBarBg = isLight ? "#DCDCDC" : "#2D2D2D";
            var textBrush = isLight ? Brushes.Black : Brushes.White;
            var secTextBrush = isLight ? Brushes.DimGray : Brushes.LightGray;
            var textboxBg = isLight ? (IBrush)Brushes.White : new SolidColorBrush(Color.Parse("#333333"));
            var borderBrush = isLight ? "#CCCCCC" : "#3D3D3D";

            BorderTopBar.Background = new SolidColorBrush(Color.Parse(topBarBg));
            BorderTopBar.BorderBrush = new SolidColorBrush(Color.Parse(borderBrush));
            
            BorderLeftToolbar.Background = new SolidColorBrush(Color.Parse(panelBg));
            BorderLeftToolbar.BorderBrush = new SolidColorBrush(Color.Parse(borderBrush));
            
            BorderRightPanel.Background = new SolidColorBrush(Color.Parse(panelBg));
            BorderRightPanel.BorderBrush = new SolidColorBrush(Color.Parse(borderBrush));

            TxtLogo.Foreground = isLight ? (IBrush)Brushes.DarkBlue : new SolidColorBrush(Color.Parse("#007ACC"));
            TxtToolsHeader.Foreground = secTextBrush;

            TabProps.Foreground = textBrush;
            TabLayers.Foreground = textBrush;
            TxtPropsTitle.Foreground = secTextBrush;
            
            LblStartX.Foreground = secTextBrush;
            LblStartY.Foreground = secTextBrush;
            LblEndX.Foreground = secTextBrush;
            LblEndY.Foreground = secTextBrush;
            LblLineLayer.Foreground = secTextBrush;
            
            LblCenterX.Foreground = secTextBrush;
            LblCenterY.Foreground = secTextBrush;
            LblRadius.Foreground = secTextBrush;
            LblCircleLayer.Foreground = secTextBrush;

            TxtLineStartX.Background = textboxBg;
            TxtLineStartX.Foreground = textBrush;
            TxtLineStartY.Background = textboxBg;
            TxtLineStartY.Foreground = textBrush;
            TxtLineEndX.Background = textboxBg;
            TxtLineEndX.Foreground = textBrush;
            TxtLineEndY.Background = textboxBg;
            TxtLineEndY.Foreground = textBrush;
            TxtLineLayer.Background = textboxBg;
            TxtLineLayer.Foreground = textBrush;

            TxtCircleCenterX.Background = textboxBg;
            TxtCircleCenterX.Foreground = textBrush;
            TxtCircleCenterY.Background = textboxBg;
            TxtCircleCenterY.Foreground = textBrush;
            TxtCircleRadius.Background = textboxBg;
            TxtCircleRadius.Foreground = textBrush;
            TxtCircleLayer.Background = textboxBg;
            TxtCircleLayer.Foreground = textBrush;
            
            TxtNewLayerName.Background = textboxBg;
            TxtNewLayerName.Foreground = textBrush;

            LstLayers.Background = isLight ? Brushes.White : new SolidColorBrush(Color.Parse("#1E1E1E"));
            LstLayers.Foreground = textBrush;

            Viewport.InvalidateVisual();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _controller?.OnKeyDown(e);
        }
    }

    public class LayerItem
    {
        public string Name { get; set; } = string.Empty;
        public IBrush ColorBrush { get; set; } = Brushes.White;
    }
}