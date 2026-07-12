using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NormalCAD.Controller.Providers
{
    public enum EditorKind
    {
        Text,
        Combo
    }

    public sealed class PropertyGroup
    {
        public string Header { get; }
        public IReadOnlyList<PropertyRow> Rows { get; }

        public PropertyGroup(string header, IReadOnlyList<PropertyRow> rows)
        {
            Header = header;
            Rows = rows;
        }
    }

    public sealed class PropertyRow : INotifyPropertyChanged
    {
        private readonly PropertyDescriptor _descriptor;
        private readonly Func<PropertyDescriptor, object?, PropertyEditResult> _commit;
        private bool _loading;

        public PropertyRow(
            PropertyDescriptor descriptor,
            EditorKind editor,
            IReadOnlyList<ComboOption>? options,
            Func<PropertyDescriptor, object?, PropertyEditResult> commit)
        {
            _descriptor = descriptor;
            Editor = editor;
            Options = options;
            _commit = commit;
            LoadSelection();
        }

        public string DisplayName => _descriptor.DisplayName;
        public bool IsReadOnly => _descriptor.IsReadOnly;
        public bool IsEditable => !_descriptor.IsReadOnly;
        public EditorKind Editor { get; }
        public IReadOnlyList<ComboOption>? Options { get; }

        public bool IsText => Editor == EditorKind.Text;
        public bool IsCombo => Editor == EditorKind.Combo;

        // Read-only display string, recomputed from the model (drives text editors).
        public string DisplayText => _descriptor.Format(_descriptor.GetValue());

        private object? _selectedOption;
        public object? SelectedOption
        {
            get => _selectedOption;
            set
            {
                if (Equals(_selectedOption, value)) return;
                _selectedOption = value;
                OnPropertyChanged();

                if (_loading || IsReadOnly) return;
                if (value is ComboOption option &&
                    _commit(_descriptor, option.Value) != PropertyEditResult.Committed)
                {
                    LoadSelection(); // rejected/failed -> revert; committed -> refresh handles it
                }
            }
        }

        // Called by the view when the user confirms a text edit (Enter).
        // Returns true when the field should revert to the current value.
        public bool CommitText(string? text)
        {
            if (IsReadOnly) return true;
            if (!_descriptor.TryParse(text, out var value)) return true;
            return _commit(_descriptor, value) != PropertyEditResult.Committed;
        }

        private void LoadSelection()
        {
            if (Options is null) return;

            _loading = true;

            var current = _descriptor.GetValue();
            object? match = null;
            foreach (var option in Options)
            {
                if (Equals(option.Value, current))
                {
                    match = option;
                    break;
                }
            }

            SelectedOption = match;
            _loading = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
