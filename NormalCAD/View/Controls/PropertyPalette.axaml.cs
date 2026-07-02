using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Resources;

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
        private ObjectId _selectedId = ObjectId.Null;

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

            if (_controller == null) return;

            var db = _controller.Database;
            var selectedIds = _controller.SelectedEntityIds;

            if (selectedIds.Count == 0)
            {
                _txtPropsTitle.Text = NoSelectionText;
                _selectedId = ObjectId.Null;
                return;
            }

            ObjectId id = selectedIds.First();
            _selectedId = id;

            Entity? entity = null;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var obj = trans.GetObject(id, OpenMode.ForRead);
                entity = obj as Entity;
            }

            if (entity == null)
            {
                _txtPropsTitle.Text = selectedIds.Count > 1
                    ? string.Format(SelectedFormat, selectedIds.Count)
                    : UnknownObjectText;
                return;
            }

            _txtPropsTitle.Text = selectedIds.Count > 1
                ? string.Format(SelectedFormat, selectedIds.Count)
                : entity.GetType().Name;

            BuildPropertyGrid(entity);
            _propsGrid.InvalidateVisual();
            InvalidateVisual();
        }

        private void BuildPropertyGrid(Entity entity)
        {
            if (_propsGrid == null) return;

            var props = TypeDescriptor.GetProperties(entity);
            var browsableProps = props.Cast<PropertyDescriptor>()
                .Where(p => p.Attributes.OfType<System.ComponentModel.CategoryAttribute>().Any()
                         || p.Attributes.OfType<System.ComponentModel.DisplayNameAttribute>().Any())
                .OrderBy(p => p.Category)
                .ThenBy(p => p.DisplayName);

            string? currentCategory = null;
            int rowIndex = 0;

            foreach (var prop in browsableProps)
            {
                if (prop.Category != currentCategory)
                {
                    currentCategory = prop.Category;
                    _propsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                    AddCategoryHeader(currentCategory ?? CategoryFallback, rowIndex);
                    rowIndex++;
                }

                _propsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                object value = prop.GetValue(entity) ?? "";
                bool isReadOnly = prop.IsReadOnly;

                var label = new TextBlock
                {
                    Text = prop.DisplayName,
                    FontSize = 11,
                    Foreground = _labelBrush,
                    Margin = new Avalonia.Thickness(0, 2, 4, 2),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(label, 0);
                Grid.SetRow(label, rowIndex);

                Control editor = prop.PropertyType.IsEnum || prop.PropertyType == typeof(bool)
                    ? CreateComboEditor(prop, value, isReadOnly)
                    : CreateTextEditor(prop, value, isReadOnly);

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

        private Control CreateTextEditor(PropertyDescriptor prop, object value, bool readOnly)
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
                Tag = prop
            };

            if (!readOnly)
                tb.KeyDown += OnTextEditorKeyDown;

            return tb;
        }

        private Control CreateComboEditor(PropertyDescriptor prop, object value, bool readOnly)
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
                Tag = prop
            };

            if (prop.PropertyType == typeof(bool))
            {
                cb.Items.Add(BooleanYes);
                cb.Items.Add(BooleanNo);
                cb.SelectedItem = (bool)value ? BooleanYes : BooleanNo;
            }
            else
            {
                foreach (var name in Enum.GetNames(prop.PropertyType))
                    cb.Items.Add(name);
                cb.SelectedItem = value.ToString();
            }

            if (!readOnly)
                cb.SelectionChanged += OnComboEditorChanged;

            return cb;
        }

        private void OnTextEditorKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key != Avalonia.Input.Key.Enter) return;
            if (sender is not TextBox tb || tb.Tag is not PropertyDescriptor prop) return;

            ApplyPropertyChange((entity, p) =>
            {
                if (p.PropertyType == typeof(double))
                {
                    if (double.TryParse(tb.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                        p.SetValue(entity, d);
                }
                else if (p.PropertyType == typeof(string))
                {
                    p.SetValue(entity, tb.Text);
                }
            }, prop);
            e.Handled = true;
        }

        private void OnComboEditorChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox cb || cb.Tag is not PropertyDescriptor prop) return;
            if (cb.SelectedItem is not string name) return;

            ApplyPropertyChange((entity, p) =>
            {
                if (p.PropertyType == typeof(bool))
                {
                    p.SetValue(entity, name == BooleanYes);
                }
                else
                {
                    var enumValue = Enum.Parse(p.PropertyType, name);
                    p.SetValue(entity, enumValue);
                }
            }, prop);
        }

        private void ApplyPropertyChange(Action<Entity, PropertyDescriptor> setter, PropertyDescriptor prop)
        {
            if (_controller == null || _selectedId.IsNull) return;

            var db = _controller.Database;
            try
            {
                using (var trans = db.TransactionManager.StartTransaction())
                {
                    var obj = trans.GetObject(_selectedId, OpenMode.ForWrite);
                    if (obj is Entity entity)
                    {
                        setter(entity, prop);
                        trans.Commit();
                        _controller.Viewport?.InvalidateVisual();
                    }
                }
            }
            catch
            {
                // Entity might have been deleted; refresh to clear stale state
                OnSelectionChanged();
            }
        }
    }
}
