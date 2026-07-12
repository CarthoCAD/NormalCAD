using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using NormalCAD.Controller.Providers;
using NormalCAD.Resources;
using CorePropDesc = NormalCAD.Controller.Providers.PropertyDescriptor;

namespace NormalCAD.View.Controls
{
    public partial class PropertyPalette : UserControl
    {
        private static string NoSelectionText => PanelResources.Get("PROPERTYPALETTE.MSG.NOSELECTION");
        private static string SelectedFormat => PanelResources.Get("PROPERTYPALETTE.MSG.SELECTED");
        private static string UnknownObjectText => PanelResources.Get("PROPERTYPALETTE.MSG.UNKNOWN");
        private static string CategoryFallback => PanelResources.Get("PROPERTYPALETTE.CATEGORY.FALLBACK");
        private static string BooleanYes => PanelResources.Get("PROPERTYPALETTE.BOOLEAN.YES");
        private static string BooleanNo => PanelResources.Get("PROPERTYPALETTE.BOOLEAN.NO");

        private Controller.CadController? _controller;
        private EntityPropertyManager? _propertyManager;

        public bool IsDropDownOpen { get; private set; }

        public event Action? DropDownClosed;

        private TextBlock? _txtPropsTitle;
        private Grid? _propsGrid;

        private IBrush _labelBrush = Brushes.Gray;
        private IBrush _textBrush = Brushes.White;
        private IBrush _borderBrush = Brushes.Gray;
        private IBrush _accentBrush = new SolidColorBrush(Color.FromRgb(0, 0x7A, 0xCC));
        private bool _brushesResolved;

        public Controller.CadController? Controller
        {
            get => _controller;
            set
            {
                if (_controller != null)
                    _controller.SelectionChanged -= OnSelectionChanged;

                _controller = value;

                if (_controller != null)
                {
                    _propertyManager = _controller.EntityPropertyManager;
                    _controller.SelectionChanged += OnSelectionChanged;
                    OnSelectionChanged();
                }
            }
        }

        public PropertyPalette()
        {
            AvaloniaXamlLoader.Load(this);

            _txtPropsTitle = this.FindControl<TextBlock>("TxtPropsTitle");
            _propsGrid = this.FindControl<Grid>("PropsGrid");

            global::NormalCAD.Controller.Services.LanguageService.LanguageChanged += Refresh;
        }

        private void EnsureBrushes()
        {
            if (_brushesResolved) return;
            _brushesResolved = true;

            var app = Application.Current;
            if (app == null) return;

            var theme = app.ActualThemeVariant;

            if (app.Resources.TryGetResource("Theme.TextSec", theme, out var label))
                _labelBrush = label as IBrush ?? Brushes.Gray;
            if (app.Resources.TryGetResource("Theme.Text", theme, out var text))
                _textBrush = text as IBrush ?? Brushes.White;
            if (app.Resources.TryGetResource("Theme.Border", theme, out var border))
                _borderBrush = border as IBrush ?? Brushes.Gray;
        }

        public void Refresh()
        {
            OnSelectionChanged();
        }

        private void OnSelectionChanged()
        {
            if (_propsGrid == null || _txtPropsTitle == null) return;

            EnsureBrushes();

            _propsGrid.Children.Clear();
            _propsGrid.RowDefinitions.Clear();

            if (_propertyManager == null) return;

            var properties = _propertyManager.GetPropertiesForSelection();

            if (properties.SelectionCount == 0)
            {
                _txtPropsTitle.Text = NoSelectionText;
                return;
            }

            if (properties.EntityCount == 0)
            {
                _txtPropsTitle.Text = UnknownObjectText;
                return;
            }

            _txtPropsTitle.Text = properties.EntityCount > 1
                ? string.Format(SelectedFormat, properties.EntityCount)
                : properties.SingleTypeDisplayName;

            BuildPropertyGrid(properties.Descriptors);
            _propsGrid.InvalidateVisual();
            InvalidateVisual();
        }

        private void BuildPropertyGrid(IReadOnlyList<CorePropDesc> descriptors)
        {
            if (_propsGrid == null) return;

            var ordered = descriptors.OrderBy(d => d.Category).ThenBy(d => d.Order);

            PropertyCategory? currentCategory = null;
            int rowIndex = 0;

            foreach (var desc in ordered)
            {
                if (desc.Category != currentCategory)
                {
                    currentCategory = desc.Category;
                    _propsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    var categoryLabel = LocalizedEnum.Resolve(desc.Category);
                    if (string.IsNullOrEmpty(categoryLabel))
                        categoryLabel = CategoryFallback;
                    AddCategoryHeader(categoryLabel, rowIndex);
                    rowIndex++;
                }

                _propsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                object value = desc.GetValue() ?? "";
                bool isReadOnly = desc.IsReadOnly;

                var label = new TextBlock
                {
                    Text = desc.DisplayName,
                    FontSize = 11,
                    Foreground = _labelBrush,
                    Margin = new Avalonia.Thickness(0, 2, 4, 2),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(label, 0);
                Grid.SetRow(label, rowIndex);

                Control editor = desc.ComboOptions is not null || desc.PropertyType == typeof(bool)
                    ? CreateComboEditor(desc, value, isReadOnly)
                    : CreateTextEditor(desc, value, isReadOnly);

                Grid.SetColumn(editor, 1);
                Grid.SetRow(editor, rowIndex);

                _propsGrid.Children.Add(label);
                _propsGrid.Children.Add(editor);
                rowIndex++;
            }

            InvalidateVisual();
        }

        private void AddCategoryHeader(string text, int rowIndex)
        {
            if (_propsGrid == null) return;

            var border = new Border
            {
                BorderBrush = _borderBrush,
                BorderThickness = new Avalonia.Thickness(0, 0, 0, 1),
                Margin = new Avalonia.Thickness(0, 4, 0, 2),
                Padding = new Avalonia.Thickness(0, 0, 0, 2),
                Child = new TextBlock
                {
                    Text = text,
                    FontWeight = FontWeight.Bold,
                    FontSize = 11,
                    Foreground = _accentBrush
                }
            };

            Grid.SetColumn(border, 0);
            Grid.SetColumnSpan(border, 2);
            Grid.SetRow(border, rowIndex);
            _propsGrid.Children.Add(border);
        }

        private Control CreateTextEditor(CorePropDesc desc, object value, bool readOnly)
        {
            var tb = new TextBox
            {
                Text = value is double d
                    ? d.ToString("F4", CultureInfo.InvariantCulture)
                    : value.ToString(),
                IsReadOnly = readOnly,
                Background = Brushes.Transparent,
                Foreground = _textBrush,
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = _borderBrush,
                CornerRadius = new Avalonia.CornerRadius(3),
                Padding = new Avalonia.Thickness(4, 2),
                FontSize = 11,
                Height = 26,
                VerticalContentAlignment = VerticalAlignment.Center,
                Tag = desc
            };

            if (!readOnly)
                tb.KeyDown += OnTextEditorKeyDown;

            return tb;
        }

        private Control CreateComboEditor(CorePropDesc desc, object value, bool readOnly)
        {
            var cb = new ComboBox
            {
                IsEnabled = !readOnly,
                FontSize = 11,
                Height = 26,
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = _borderBrush,
                CornerRadius = new Avalonia.CornerRadius(3),
                Padding = new Avalonia.Thickness(4, 2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
                Tag = desc
            };

            if (desc.ComboOptions is not null)
            {
                foreach (var opt in desc.ComboOptions)
                    cb.Items.Add(opt);
                cb.SelectedItem = desc.ComboOptions.FirstOrDefault(o => Equals(o.Value, desc.GetValue()));
            }
            else if (desc.PropertyType == typeof(bool))
            {
                var trueOpt = new ComboOption(true, BooleanYes);
                var falseOpt = new ComboOption(false, BooleanNo);
                cb.Items.Add(trueOpt);
                cb.Items.Add(falseOpt);
                cb.SelectedItem = value is bool b ? (b ? trueOpt : falseOpt) : null;
            }

            if (!readOnly)
            {
                cb.SelectionChanged += OnComboEditorChanged;
                cb.DropDownOpened += (_, _) => IsDropDownOpen = true;
                cb.DropDownClosed += (_, _) =>
                {
                    IsDropDownOpen = false;
                    DropDownClosed?.Invoke();
                };
            }

            return cb;
        }

        private void OnTextEditorKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key != Avalonia.Input.Key.Enter) return;
            if (sender is not TextBox tb || tb.Tag is not CorePropDesc desc) return;
            if (desc.TrySetValue == null) return;

            if (desc.PropertyType == typeof(double))
            {
                if (double.TryParse(tb.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                    ApplyAndCommit(desc, d);
            }
            else if (desc.PropertyType == typeof(int))
            {
                if (int.TryParse(tb.Text, out int i))
                    ApplyAndCommit(desc, i);
            }
            else
            {
                ApplyAndCommit(desc, tb.Text);
            }
            e.Handled = true;
        }

        private void OnComboEditorChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox cb || cb.Tag is not CorePropDesc desc) return;
            if (desc.TrySetValue == null) return;

            object? value;

            if (cb.SelectedItem is ComboOption option)
            {
                value = option.Value;
            }
            else
            {
                return;
            }

            ApplyAndCommit(desc, value);
        }

        private void ApplyAndCommit(CorePropDesc desc, object? value)
        {
            if (_propertyManager == null) return;

            switch (_propertyManager.SetValue(desc, value))
            {
                case PropertyEditResult.Committed:
                case PropertyEditResult.Failed:
                    OnSelectionChanged();
                    break;
            }
        }
    }
}
