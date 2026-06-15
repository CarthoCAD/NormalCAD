using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace NormalCAD.View.Controls
{
    public partial class BottomBar : UserControl
    {
        private Controller.CadController? _controller;
        private TextBox? _txtPrompt;
        private TextBlock? _txtPromptPrefix;
        private Popup? _promptPopup;
        private Border? _promptPopupBorder;
        private TextBlock? _txtPromptPopup;
        private DispatcherTimer? _promptHideTimer;
        private DispatcherTimer? _promptCloseTimer;

        public Controller.CadController? Controller
        {
            get => _controller;
            set
            {
                if (_controller != null)
                {
                    _controller.Viewport.PointerMoved -= OnViewportPointerMoved;
                    _controller.InputManager.PromptMessageChanged -= OnPromptMessageChanged;
                    _controller.InputManager.CurrentPromptChanged -= OnCurrentPromptChanged;
                }
                _controller = value;
                if (_controller != null)
                {
                    _controller.Viewport.PointerMoved += OnViewportPointerMoved;
                    _controller.InputManager.PromptMessageChanged += OnPromptMessageChanged;
                    _controller.InputManager.CurrentPromptChanged += OnCurrentPromptChanged;

                    OnCurrentPromptChanged(_controller.InputManager.CurrentPrompt);
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

            _txtPrompt = this.FindControl<TextBox>("TxtPrompt");
            if (_txtPrompt != null)
            {
                _txtPrompt.KeyDown += OnTxtPromptKeyDown;
            }

            _txtPromptPrefix = this.FindControl<TextBlock>("TxtPromptPrefix");

            _promptPopup = this.FindControl<Popup>("PromptPopup");
            _promptPopupBorder = this.FindControl<Border>("PromptPopupBorder");
            _txtPromptPopup = this.FindControl<TextBlock>("TxtPromptPopup");
            if (_promptPopup != null)
            {
                _promptPopup.IsOpen = false;
            }

            var btnModel = this.FindControl<Button>("BtnModel");
            if (btnModel != null)
            {
                btnModel.Click += OnBtnModelClick;
            }
        }

        private async void OnTxtPromptKeyDown(object? sender, KeyEventArgs e)
        {
            if (_controller == null || _txtPrompt == null) return;

            if (e.Key == Key.Escape)
            {
                _txtPrompt.Text = "";
                _controller.CancelCurrentCommand();
                HideFloatingPrompt();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                string commandText = _txtPrompt.Text?.Trim() ?? "";
                if (!string.IsNullOrEmpty(commandText))
                {
                    await _controller.CmdManager.ExecuteCommand(commandText);
                    _txtPrompt.Text = "";
                }
                e.Handled = true;
            }
        }

        private void OnCurrentPromptChanged(string prompt)
        {
            if (_txtPromptPrefix != null)
            {
                _txtPromptPrefix.Text = prompt + ":";
            }
        }

        private void OnPromptMessageChanged(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ShowFloatingPrompt(message);
            }
            else
            {
                HideFloatingPrompt();
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
        }
        private void OnBtnModelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _controller?.InputManager.SetPromptMessage("Tool not implemented yet");
        }

        public void ShowFloatingPrompt(string message, int autoHideMs = 5000)
        {
            if (_promptPopup == null || _txtPromptPopup == null || _promptPopupBorder == null) return;

            _promptHideTimer?.Stop();
            _promptCloseTimer?.Stop();

            _txtPromptPopup.Text = message;

            if (!_promptPopup.IsOpen)
            {
                _promptPopup.IsOpen = true;
            }

            _promptPopupBorder.Opacity = 1;

            if (autoHideMs > 0)
            {
                var timer = new DispatcherTimer(
                    TimeSpan.FromMilliseconds(autoHideMs),
                    DispatcherPriority.Normal,
                    (s, e) =>
                    {
                        HideFloatingPrompt();
                        _promptHideTimer?.Stop();
                    });
                timer.Start();
                _promptHideTimer = timer;
            }
        }

        public void HideFloatingPrompt()
        {
            _promptHideTimer?.Stop();
            _promptCloseTimer?.Stop();

            if (_promptPopupBorder != null)
                _promptPopupBorder.Opacity = 0;

            if (_promptPopup != null)
            {
                var closeTimer = new DispatcherTimer(
                    TimeSpan.FromMilliseconds(250),
                    DispatcherPriority.Normal,
                    (s, e) =>
                    {
                        _promptPopup.IsOpen = false;
                        _promptCloseTimer?.Stop();
                    });
                closeTimer.Start();
                _promptCloseTimer = closeTimer;
            }
        }
    }
}
