using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using NormalCAD.Core.DatabaseServices;
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
        private List<ObjectId> _selectedIds = [];

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

            if (_controller == null || _propertyManager == null) return;

            var db = _controller.Database;
            var selectedIds = _controller.SelectedEntityIds;

            if (selectedIds.Count == 0)
            {
                _txtPropsTitle.Text = NoSelectionText;
                _selectedIds = [];
                return;
            }

            _selectedIds = selectedIds.ToList();

            var entities = new List<Entity>();
            foreach (var id in selectedIds)
            {
                using (var trans = db.TransactionManager.StartTransaction())
                {
                    var obj = trans.GetObject(id, OpenMode.ForRead);
                    if (obj is Entity entity)
                        entities.Add(entity);
                }
            }

            if (entities.Count == 0)
            {
                _txtPropsTitle.Text = UnknownObjectText;
                return;
            }

            _txtPropsTitle.Text = entities.Count > 1
                ? string.Format(SelectedFormat, entities.Count)
                : entities[0].GetType().Name;

            var descriptors = entities.Count == 1
                ? _propertyManager.GetProperties(entities[0])
                : _propertyManager.GetMergedProperties(entities);

            BuildPropertyGrid(descriptors);
            _propsGrid.InvalidateVisual();
            InvalidateVisual();
        }

        private void BuildPropertyGrid(IReadOnlyList<CorePropDesc> descriptors)
        {
            if (_propsGrid == null) return;

            var ordered = descriptors.OrderBy(d => d.Category).ThenBy(d => d.Order);

            string? currentCategory = null;
            int rowIndex = 0;

            foreach (var desc in ordered)
            {
                if (desc.Category != currentCategory)
                {
                    currentCategory = desc.Category;
                    _propsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    AddCategoryHeader(currentCategory ?? CategoryFallback, rowIndex);
                    rowIndex++;
                }

                _propsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                object value = desc.GetValue() ?? "";
                bool isReadOnly = desc.IsReadOnly || desc.TrySetValue == null;

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

                Control editor = desc.PropertyType.IsEnum || desc.PropertyType == typeof(bool) || desc.ComboValues is not null
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

            if (desc.PropertyType == typeof(bool))
            {
                cb.Items.Add(BooleanYes);
                cb.Items.Add(BooleanNo);
                cb.SelectedItem = value is bool b ? (b ? BooleanYes : BooleanNo) : null;
            }
            else if (desc.ComboValues is not null)
            {
                foreach (var name in desc.ComboValues)
                    cb.Items.Add(name);
                cb.SelectedItem = value.ToString();
            }
            else
            {
                foreach (var name in Enum.GetNames(desc.PropertyType))
                    cb.Items.Add(name);
                cb.SelectedItem = value.ToString();
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
            if (cb.SelectedItem is not string name || desc.TrySetValue == null) return;

            object? value;
            if (desc.PropertyType == typeof(bool))
                value = name == BooleanYes;
            else if (desc.ComboValues is not null)
                value = name;
            else
                value = Enum.Parse(desc.PropertyType, name);

            ApplyAndCommit(desc, value);
        }

        private void ApplyAndCommit(CorePropDesc desc, object? value)
        {
            if (_controller == null) return;

            var db = _controller.Database;
            try
            {
                using (var trans = db.TransactionManager.StartTransaction())
                {
                    if (desc.TrySetValue!(value))
                    {
                        trans.Commit();
                        _controller.Viewport?.InvalidateVisual();
                        OnSelectionChanged();
                    }
                    else
                    {
                        _controller.InputManager.SetPromptMessage($"Invalid value for {desc.DisplayName}.");
                    }
                }
            }
            catch
            {
                OnSelectionChanged();
            }
        }
    }
}
