using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using NormalCAD.Core;

namespace NormalCAD.View.Controls
{
    public partial class BottomBar : UserControl
    {
        private Controller.CadController? _controller;

        public Controller.CadController? Controller
        {
            get => _controller;
            set
            {
                if (_controller != null)
                {
                    _controller.ActiveCommandChanged -= OnActiveCommandChanged;
                    _controller.Viewport.PointerMoved -= OnViewportPointerMoved;
                }
                _controller = value;
                if (_controller != null)
                {
                    _controller.ActiveCommandChanged += OnActiveCommandChanged;
                    _controller.Viewport.PointerMoved += OnViewportPointerMoved;

                    // Defina a ferramenta ativa inicial
                    var txtActiveTool = this.FindControl<TextBlock>("TxtActiveTool");
                    if (txtActiveTool != null)
                    {
                        txtActiveTool.Text = "Ferramenta Ativa: Seleção";
                    }
                }
            }
        }

        public BottomBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnActiveCommandChanged(string cmdName)
        {
            var txtActiveTool = this.FindControl<TextBlock>("TxtActiveTool");
            if (txtActiveTool != null)
            {
                txtActiveTool.Text = $"Ferramenta Ativa: {cmdName}";
            }
        }

        private void OnViewportPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_controller == null) return;
            var viewport = _controller.Viewport;
            var screenPos = e.GetPosition(viewport);
            var worldPos = viewport.ScreenToWorld(screenPos);

            var txtCoordinates = this.FindControl<TextBlock>("TxtCoordinates");
            if (txtCoordinates != null)
            {
                txtCoordinates.Text = $"X: {worldPos.X:F4}, Y: {worldPos.Y:F4}";
            }

            var txtSnapStatus = this.FindControl<TextBlock>("TxtSnapStatus");
            if (txtSnapStatus != null)
            {
                if (viewport.ActiveSnapType != SnapType.None && viewport.ActiveSnapPoint.HasValue)
                {
                    txtSnapStatus.Text = $"Snap: {viewport.ActiveSnapType} ({viewport.ActiveSnapPoint.Value.X:F2}, {viewport.ActiveSnapPoint.Value.Y:F2})";
                }
                else
                {
                    txtSnapStatus.Text = "Snap: Nenhum";
                }
            }
        }
    }
}
