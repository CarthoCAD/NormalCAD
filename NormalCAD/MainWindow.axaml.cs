using Avalonia.Controls;
using Avalonia.Input;
using NormalCAD.Controller;
using NormalCAD.View.Controls;

namespace NormalCAD
{
    public partial class MainWindow : Window
    {
        private readonly CadController _controller;
        private readonly PropertyPalette _propertyPalette;
        private readonly LayerPalette _layerPalette;

        public MainWindow()
        {
            InitializeComponent();

            _controller = new CadController(Viewport);

            _propertyPalette = new PropertyPalette { Controller = _controller };
            _layerPalette = new LayerPalette { Controller = _controller };

            MenuBar.Controller = _controller;
            BottomBar.Controller = _controller;

            Viewport.PointerPressed += (s, e) =>
            {
                CollapseDrawer();
            };

            BtnTabProps.Click += (s, e) => ToggleDrawer(_propertyPalette);
            BtnTabLayers.Click += (s, e) => ToggleDrawer(_layerPalette);
        }

        private void ToggleDrawer(UserControl palette)
        {
            if (DrawerPanel.Width > 0 && DrawerContent.Content == palette)
            {
                CollapseDrawer();
            }
            else
            {
                DrawerContent.Content = palette;
                DrawerPanel.Width = 250;
            }
        }

        private void CollapseDrawer()
        {
            if (DrawerPanel.Width > 0)
            {
                DrawerPanel.Width = 0;
                DrawerContent.Content = null;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _controller?.OnKeyDown(e);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);

            if (string.IsNullOrEmpty(e.Text))
                return;

            var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
            if (focused is TextBox)
                return;

            var prompt = BottomBar.FindControl<TextBox>("TxtPrompt");
            if (prompt != null)
            {
                prompt.Focus();
                var caret = prompt.CaretIndex;
                prompt.Text = prompt.Text?.Insert(caret, e.Text) ?? e.Text;
                prompt.CaretIndex = caret + e.Text.Length;
                e.Handled = true;
            }
        }
    }
}
