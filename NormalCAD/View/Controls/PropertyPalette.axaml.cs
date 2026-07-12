using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using NormalCAD.Controller.Providers;
using NormalCAD.Resources;

namespace NormalCAD.View.Controls
{
    public partial class PropertyPalette : UserControl
    {
        private static string NoSelectionText => PanelResources.Get("PROPERTYPALETTE.MSG.NOSELECTION");
        private static string SelectedFormat => PanelResources.Get("PROPERTYPALETTE.MSG.SELECTED");
        private static string UnknownObjectText => PanelResources.Get("PROPERTYPALETTE.MSG.UNKNOWN");

        private Controller.CadController? _controller;
        private EntityPropertyManager? _propertyManager;

        private TextBlock? _txtPropsTitle;
        private ItemsControl? _propsItems;

        public bool IsDropDownOpen { get; private set; }

        public event Action? DropDownClosed;

        public Controller.CadController? Controller
        {
            get => _controller;
            set
            {
                if (_controller != null)
                    _controller.SelectionChanged -= OnSelectionChanged;
                if (_propertyManager != null)
                    _propertyManager.PropertiesInvalidated -= OnPropertiesInvalidated;

                _controller = value;

                if (_controller != null)
                {
                    _propertyManager = _controller.EntityPropertyManager;
                    _propertyManager.PropertiesInvalidated += OnPropertiesInvalidated;
                    _controller.SelectionChanged += OnSelectionChanged;
                    OnSelectionChanged();
                }
            }
        }

        public PropertyPalette()
        {
            AvaloniaXamlLoader.Load(this);

            _txtPropsTitle = this.FindControl<TextBlock>("TxtPropsTitle");
            _propsItems = this.FindControl<ItemsControl>("PropsItems");

            AddHandler(KeyDownEvent, OnEditorKeyDown, RoutingStrategies.Bubble);
            AddHandler(LostFocusEvent, OnEditorLostFocus, RoutingStrategies.Bubble);

            global::NormalCAD.Controller.Services.LanguageService.LanguageChanged += Refresh;
        }

        public void Refresh() => OnSelectionChanged();

        internal void SetDropDownOpen(bool open)
        {
            IsDropDownOpen = open;
            if (!open)
                DropDownClosed?.Invoke();
        }

        private void OnPropertiesInvalidated() => Dispatcher.UIThread.Post(OnSelectionChanged);

        private void OnSelectionChanged()
        {
            if (_txtPropsTitle == null || _propsItems == null || _propertyManager == null) return;

            var properties = _propertyManager.GetPropertiesForSelection();

            if (properties.SelectionCount == 0)
            {
                _txtPropsTitle.Text = NoSelectionText;
                _propsItems.ItemsSource = null;
                return;
            }

            if (properties.EntityCount == 0)
            {
                _txtPropsTitle.Text = UnknownObjectText;
                _propsItems.ItemsSource = null;
                return;
            }

            _txtPropsTitle.Text = properties.EntityCount > 1
                ? string.Format(SelectedFormat, properties.EntityCount)
                : properties.SingleTypeDisplayName;

            _propsItems.ItemsSource = properties.Groups;
        }

        private void OnEditorKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (e.Source is TextBox textBox && CommitFromTextBox(textBox))
                e.Handled = true;
        }

        private void OnEditorLostFocus(object? sender, RoutedEventArgs e)
        {
            if (e.Source is TextBox textBox)
                CommitFromTextBox(textBox);
        }

        // Commits the text box's value (parse/commit/revert), matching the Enter
        // behaviour. Skips when the text is unchanged so re-created boxes (after a
        // rebuild) losing focus don't trigger spurious commits. Returns true when
        // the box hosts an editable property row.
        private bool CommitFromTextBox(TextBox textBox)
        {
            if (textBox.DataContext is not PropertyRow row) return false;
            if (textBox.Text == row.DisplayText) return true;

            if (row.CommitText(textBox.Text))
                textBox.Text = row.DisplayText;

            return true;
        }
    }
}
