using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using NormalCAD.Core.DatabaseServices;

namespace NormalCAD.View.Controls
{
    public partial class LayerPalette : UserControl
    {
        private Controller.CadController? _controller;

        public Controller.CadController? Controller
        {
            get => _controller;
            set
            {
                if (_controller != null)
                {
                    _controller.DatabaseChanged -= OnDatabaseChanged;
                }
                _controller = value;
                if (_controller != null)
                {
                    _controller.DatabaseChanged += OnDatabaseChanged;
                    OnDatabaseChanged(); // Initial update
                }
            }
        }

        public LayerPalette()
        {
            InitializeComponent();

            var btnAddLayer = this.FindControl<Button>("BtnAddLayer");
            if (btnAddLayer != null) btnAddLayer.Click += BtnAddLayer_Click;

            var lstLayers = this.FindControl<ListBox>("LstLayers");
            if (lstLayers != null) lstLayers.SelectionChanged += LstLayers_SelectionChanged;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnDatabaseChanged()
        {
            if (_controller == null) return;
            var list = new List<LayerItem>();
            if (_controller.Database != null)
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

            var lstLayers = this.FindControl<ListBox>("LstLayers");
            if (lstLayers != null)
            {
                lstLayers.ItemsSource = list;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Name == _controller.ActiveLayer)
                    {
                        lstLayers.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void BtnAddLayer_Click(object? sender, RoutedEventArgs e)
        {
            if (_controller == null) return;
            var txtNewLayerName = this.FindControl<TextBox>("TxtNewLayerName");
            if (txtNewLayerName == null) return;

            string name = txtNewLayerName.Text?.Trim() ?? "";
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
                    txtNewLayerName.Text = "";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating layer: {ex.Message}");
                }
            }
        }

        private void LstLayers_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_controller == null) return;
            var lstLayers = sender as ListBox;
            if (lstLayers?.SelectedItem is LayerItem item)
            {
                _controller.ActiveLayer = item.Name;
            }
        }
    }

    public class LayerItem
    {
        public string Name { get; set; } = string.Empty;
        public IBrush ColorBrush { get; set; } = Brushes.White;
    }
}
