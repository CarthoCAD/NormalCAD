using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using NormalCAD.Resources;

namespace NormalCAD.View.Controls
{
    public partial class BottomBar : UserControl
    {
        private static string ModelButtonText => PanelResources.Get("BOTTOMBAR.BUTTON.MODEL");
        private static string CmdLabelText => PanelResources.Get("BOTTOMBAR.LABEL.CMD");
        private static string CommandPlaceholder => PanelResources.Get("BOTTOMBAR.PLACEHOLDER.COMMAND");
        private static string NotImplementedMsg => PanelResources.Get("BOTTOMBAR.MSG.NOTIMPLEMENTED");
        private TextBox? _txtPrompt;
        private TextBlock? _txtPromptPrefix;
        private Popup? _promptPopup;
        private Border? _promptPopupBorder;
        private TextBlock? _txtPromptPopup;
        private DispatcherTimer? _promptHideTimer;
        private DispatcherTimer? _promptCloseTimer;

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
                btnModel.Content = ModelButtonText;
                btnModel.Click += OnBtnModelClick;
            }

            if (_txtPrompt != null)
                _txtPrompt.PlaceholderText = CommandPlaceholder;

            if (_txtPromptPrefix != null)
                _txtPromptPrefix.Text = CmdLabelText;

            global::NormalCAD.Controller.Services.LanguageService.LanguageChanged += RelocalizeUi;
        }

        public void AttachCadController()
        {
            Controller.CadController.Current.Viewport.PointerMoved += OnViewportPointerMoved;
            Controller.CadController.Current.InputManager.PromptMessageChanged += OnPromptMessageChanged;
            Controller.CadController.Current.InputManager.CurrentPromptChanged += OnCurrentPromptChanged;
            Controller.CadController.Current.InputManager.NavigateToPromptRequested += OnNavigateToPromptRequested;

            OnCurrentPromptChanged(Controller.CadController.Current.InputManager.CurrentPrompt);
        }

        private void RelocalizeUi()
        {
            var btnModel = this.FindControl<Button>("BtnModel");
            if (btnModel != null)
                btnModel.Content = ModelButtonText;

            if (_txtPrompt != null)
                _txtPrompt.PlaceholderText = CommandPlaceholder;

            if (_txtPromptPrefix != null)
                _txtPromptPrefix.Text = Controller.CadController.Current.InputManager.CurrentPrompt ?? CmdLabelText;
        }

        private async void OnTxtPromptKeyDown(object? sender, KeyEventArgs e)
        {
            if (_txtPrompt == null) return;

            if (e.Key == Key.Up)
            {
                var text = Controller.CadController.Current.InputManager.NavigateHistory(1);
                if (text != null) _txtPrompt.Text = text;
                _txtPrompt.CaretIndex = _txtPrompt.Text.Length;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Down)
            {
                var text = Controller.CadController.Current.InputManager.NavigateHistory(-1);
                _txtPrompt.Text = text ?? "";
                _txtPrompt.CaretIndex = _txtPrompt.Text.Length;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                _txtPrompt.Text = "";
                Controller.CadController.Current.InputManager.ResetHistoryIndex();
                Controller.CadController.Current.CancelCurrentCommand();
                HideFloatingPrompt();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                string commandText = _txtPrompt.Text?.Trim() ?? "";
                _txtPrompt.Text = "";
                Controller.CadController.Current.InputManager.ResetHistoryIndex();

                if (!string.IsNullOrEmpty(commandText))
                {
                    if (!Controller.CadController.Current.InputManager.TryProcessTextInput(commandText))
                    {
                        await Controller.CadController.Current.CmdManager.ExecuteCommand(commandText);
                    }
                }
                else if (Controller.CadController.Current.InputManager.HasEditingPrompt)
                {
                    Controller.CadController.Current.InputManager.AcceptPrompt();
                }
                else
                {
                    Controller.CadController.Current.InputManager.TryRepeatLastCommand();
                }

                e.Handled = true;
            }
        }

        private void OnCurrentPromptChanged(string prompt)
        {
            if (_txtPromptPrefix != null)
            {
                _txtPromptPrefix.Text = prompt;
            }
        }

        private void OnNavigateToPromptRequested(string? text)
        {
            if (_txtPrompt == null) return;
            _txtPrompt.Text = text ?? "";
            _txtPrompt.CaretIndex = _txtPrompt.Text.Length;
            _txtPrompt.Focus();
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
            var viewport = Controller.CadController.Current.Viewport;
            var screenPos = e.GetPosition(viewport);
            var worldPos = viewport.ScreenToWorld(screenPos);

            var txtCoordinates = this.FindControl<TextBlock>("TxtCoordinates");
            if (txtCoordinates != null)
            {
                txtCoordinates.Text = worldPos.ToString2D();
            }
        }
        private void OnBtnModelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Controller.CadController.Current.InputManager.SetPromptMessage(NotImplementedMsg);
        }

        public void ShowFloatingPrompt(string message, int autoHideMs = 3000)
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
