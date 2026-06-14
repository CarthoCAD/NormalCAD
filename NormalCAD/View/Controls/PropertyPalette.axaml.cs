using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NormalCAD.Core;
using NormalCAD.Core.Entities;

namespace NormalCAD.View.Controls
{
    public partial class PropertyPalette : UserControl
    {
        private Controller.CadController? _controller;

        public Controller.CadController? Controller
        {
            get => _controller;
            set
            {
                if (_controller != null)
                {
                    _controller.SelectionChanged -= OnSelectionChanged;
                }
                _controller = value;
                if (_controller != null)
                {
                    _controller.SelectionChanged += OnSelectionChanged;
                    OnSelectionChanged(); // Initial update
                }
            }
        }

        public PropertyPalette()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnSelectionChanged()
        {
            if (_controller == null) return;
            var selectedIds = _controller.Viewport.SelectedEntityIds;

            var txtPropsTitle = this.FindControl<TextBlock>("TxtPropsTitle");
            var panelLineProps = this.FindControl<StackPanel>("PanelLineProps");
            var panelCircleProps = this.FindControl<StackPanel>("PanelCircleProps");

            if (txtPropsTitle == null || panelLineProps == null || panelCircleProps == null) return;

            if (selectedIds.Count != 1)
            {
                txtPropsTitle.Text = selectedIds.Count == 0 ? "Nenhum objeto selecionado" : $"{selectedIds.Count} objetos selecionados";
                panelLineProps.IsVisible = false;
                panelCircleProps.IsVisible = false;
                return;
            }

            ObjectId id = ObjectId.Null;
            foreach (var selectedId in selectedIds) { id = selectedId; break; }

            if (_controller.Database.TryGetObject(id, out var dbObj))
            {
                if (dbObj is Line line)
                {
                    txtPropsTitle.Text = "Linha";
                    panelLineProps.IsVisible = true;
                    panelCircleProps.IsVisible = false;

                    this.FindControl<TextBox>("TxtLineStartX")!.Text = line.StartPoint.X.ToString("F4");
                    this.FindControl<TextBox>("TxtLineStartY")!.Text = line.StartPoint.Y.ToString("F4");
                    this.FindControl<TextBox>("TxtLineEndX")!.Text = line.EndPoint.X.ToString("F4");
                    this.FindControl<TextBox>("TxtLineEndY")!.Text = line.EndPoint.Y.ToString("F4");
                    this.FindControl<TextBox>("TxtLineLayer")!.Text = line.Layer;
                }
                else if (dbObj is Circle circle)
                {
                    txtPropsTitle.Text = "Círculo";
                    panelLineProps.IsVisible = false;
                    panelCircleProps.IsVisible = true;

                    this.FindControl<TextBox>("TxtCircleCenterX")!.Text = circle.Center.X.ToString("F4");
                    this.FindControl<TextBox>("TxtCircleCenterY")!.Text = circle.Center.Y.ToString("F4");
                    this.FindControl<TextBox>("TxtCircleRadius")!.Text = circle.Radius.ToString("F4");
                    this.FindControl<TextBox>("TxtCircleLayer")!.Text = circle.Layer;
                }
                else
                {
                    txtPropsTitle.Text = "Objeto Desconhecido";
                    panelLineProps.IsVisible = false;
                    panelCircleProps.IsVisible = false;
                }
            }
        }
    }
}
