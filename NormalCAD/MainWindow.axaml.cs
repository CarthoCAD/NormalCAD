using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using NormalCAD.Controller;
using NormalCAD.View.Controls;

namespace NormalCAD
{
    public partial class MainWindow : Window
    {
        private readonly CadController _controller;
        private readonly PropertyPalette _propertyPalette;
        private readonly LayerPalette _layerPalette;

        private DispatcherTimer? _autohideTimer;
        private const int AutohideDelayMs = 500;

        private bool _isResizing;
        private double _resizeStartX;
        private double _resizeStartWidth;
        private double _drawerWidth = 250;
        private const double MinDrawerWidth = 150;
        private const double MaxDrawerWidth = 500;

        public MainWindow()
        {
            InitializeComponent();

            _controller = new CadController(Viewport);

            _propertyPalette = new PropertyPalette { Controller = _controller };
            _layerPalette = new LayerPalette { Controller = _controller };

            MenuBar.Controller = _controller;
            BottomBar.Controller = _controller;

            BtnTabProps.PointerEntered += (s, e) => ShowDrawer(_propertyPalette);
            BtnTabLayers.PointerEntered += (s, e) => ShowDrawer(_layerPalette);

            BtnTabProps.Click += (s, e) => ToggleDrawer(_propertyPalette);
            BtnTabLayers.Click += (s, e) => ToggleDrawer(_layerPalette);

            SidebarGrid.PointerExited += OnSidebarPointerExited;
            SidebarGrid.PointerEntered += OnSidebarPointerEntered;

            DrawerGrip.PointerPressed += OnGripPointerPressed;
            DrawerGrip.PointerMoved += OnGripPointerMoved;
            DrawerGrip.PointerReleased += OnGripPointerReleased;
        }

        private void ShowDrawer(UserControl palette)
        {
            _autohideTimer?.Stop();

            DrawerContent.Content = palette;
            DrawerPanel.Width = _drawerWidth;

            if (palette is PropertyPalette pp)
                pp.Refresh();
        }

        private void ToggleDrawer(UserControl palette)
        {
            if (DrawerPanel.Width > 0 && DrawerContent.Content == palette)
            {
                CollapseDrawer();
            }
            else
            {
                ShowDrawer(palette);
            }
        }

        private void CollapseDrawer()
        {
            _autohideTimer?.Stop();
            DrawerPanel.Width = 0;
        }

        private void OnGripPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DrawerPanel.Width <= 0) return;

            _autohideTimer?.Stop();
            _isResizing = true;
            _resizeStartX = e.GetPosition(this).X;
            _resizeStartWidth = DrawerPanel.Width;
            e.Pointer.Capture(DrawerGrip);
            e.Handled = true;
        }

        private void OnGripPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_isResizing) return;

            double currentX = e.GetPosition(this).X;
            double delta = _resizeStartX - currentX;
            _drawerWidth = Math.Max(MinDrawerWidth, Math.Min(MaxDrawerWidth, _resizeStartWidth + delta));
            DrawerPanel.Width = _drawerWidth;
        }

        private void OnGripPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!_isResizing) return;

            _isResizing = false;
            e.Pointer.Capture(null);

            // Restart autohide timer if pointer is outside sidebar
            var pos = e.GetPosition(SidebarGrid);
            if (pos.X < 0 || pos.Y < 0 ||
                pos.X > SidebarGrid.Bounds.Width || pos.Y > SidebarGrid.Bounds.Height)
            {
                OnSidebarPointerExited(sender, e);
            }
        }

        private void OnSidebarPointerExited(object? sender, PointerEventArgs e)
        {
            _autohideTimer?.Stop();
            _autohideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(AutohideDelayMs) };
            _autohideTimer.Tick += (s, args) =>
            {
                CollapseDrawer();
                _autohideTimer?.Stop();
            };
            _autohideTimer.Start();
        }

        private void OnSidebarPointerEntered(object? sender, PointerEventArgs e)
        {
            _autohideTimer?.Stop();
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
