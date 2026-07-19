using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Input;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Core.EditorInput;
using NormalCAD.Core.Geometry;
using NormalCAD.Controller.Commands;
using NormalCAD.Resources;

namespace NormalCAD.Controller
{
    public class InputManager
    {
        private static string DefaultPrompt => CommandResources.Get("CMD.PROMPT.DEFAULT");
        private static string MsgAmbiguousKeyword => CommandResources.Get("CMD.MSG.AMBIGUOUS_KEYWORD");
        private static string MsgKeywordRequired => CommandResources.Get("CMD.MSG.KEYWORD_REQUIRED");
        private static string MsgPointOrKeyword => CommandResources.Get("CMD.MSG.POINT_OR_KEYWORD_REQUIRED");
        private static string MsgPointRequired => CommandResources.Get("CMD.MSG.POINT_REQUIRED");
        private static string MsgDistanceOrKeyword => CommandResources.Get("CMD.MSG.DISTANCE_OR_KEYWORD_REQUIRED");
        private static string MsgDistanceRequired => CommandResources.Get("CMD.MSG.DISTANCE_REQUIRED");
        private static string MsgStringRequired => CommandResources.Get("CMD.MSG.STRING_REQUIRED");
        private static string MsgInvalidPoint => CommandResources.Get("CMD.MSG.INVALID_POINT");
        private static string MsgInvalidDistance => CommandResources.Get("CMD.MSG.INVALID_DISTANCE");

        internal bool HasAnyCallback =>
            _pointCallback != null || _distanceCallback != null ||
            _stringCallback != null || _keywordCallback != null ||
            _selectionManager.IsActive;

        internal bool IsShiftPressed => _selectionManager.IsShiftPressed;

        internal bool HasEditingPrompt =>
            _pointCallback != null || _distanceCallback != null ||
            _stringCallback != null || _keywordCallback != null;

        public event Action<string?>? NavigateToPromptRequested;

        private readonly CadController _controller;
        private readonly SelectionManager _selectionManager;
        private readonly List<string> _promptHistory = new();
        private const int MaxPromptHistory = 100;

        private string[] _keywords = Array.Empty<string>();
        private Action<string>? _keywordHandler;

        private Action<PromptPointResult>? _pointCallback;
        private Action<PromptDoubleResult>? _distanceCallback;
        private Action<PromptStringResult>? _stringCallback;
        private Action<PromptKeywordResult>? _keywordCallback;
        private Action<Point3d>? _mouseMoveCallback;
        private PromptPointOptions? _activePointOptions;
        private int _historyIndex = -1;

        private readonly Dictionary<string, Entity?> _previews = new();

        public bool HasKeywords => _keywords.Length > 0;
        public IReadOnlyDictionary<string, Entity?> ActivePreviews => _previews;

        public event Action<string>? PromptMessageChanged;
        public event Action<string>? CurrentPromptChanged;

        public string CurrentPrompt { get; private set; } = DefaultPrompt;

        public InputManager(CadController controller)
        {
            _controller = controller;
            _selectionManager = new SelectionManager(controller, this);
        }

        public void SetCurrentPrompt(string cmdName)
        {
            SetCurrentPrompt(cmdName, "", null, null);
        }

        public void SetCurrentPrompt(string cmdName, string message,
            string[]? keywords = null, Action<string>? callback = null)
        {
            _keywords = keywords ?? Array.Empty<string>();
            _keywordHandler = callback;

            var prompt = keywords is { Length: > 0 }
                ? $"{cmdName} {message} or [{string.Join("/", keywords)}]:"
                : $"{cmdName} {message}:";

            if (CurrentPrompt == prompt) return;
            CurrentPrompt = prompt;
            CurrentPromptChanged?.Invoke(prompt);
        }

        public void ClearKeywords()
        {
            _keywords = Array.Empty<string>();
            _keywordHandler = null;
        }

        public bool TryHandleKeyword(string text)
        {
            if (!HasKeywords) return false;

            foreach (var kw in _keywords)
            {
                if (string.Equals(kw, text, StringComparison.OrdinalIgnoreCase))
                {
                    DispatchKeyword(kw);
                    return true;
                }
            }

            var matches = new List<string>();
            foreach (var kw in _keywords)
            {
                if (kw.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                    matches.Add(kw);
            }

            if (matches.Count == 1)
            {
                DispatchKeyword(matches[0]);
                return true;
            }

            if (matches.Count > 1)
                SetPromptMessage(MsgAmbiguousKeyword);
            else
                SetPromptMessage(MsgKeywordRequired);

            return false;
        }

        private void DispatchKeyword(string keyword)
        {
            if (_pointCallback != null)
                _pointCallback(new PromptPointResult(PromptStatus.Keyword, default, keyword));
            else if (_distanceCallback != null)
                _distanceCallback(new PromptDoubleResult(PromptStatus.Keyword, 0, keyword));
            else if (_stringCallback != null)
                _stringCallback(new PromptStringResult(PromptStatus.Keyword, keyword));
            else if (_keywordCallback != null)
                _keywordCallback(new PromptKeywordResult(PromptStatus.OK, keyword));
        }

        public void SetPromptMessage(string message)
        {
            _promptHistory.Add(message);
            if (_promptHistory.Count > MaxPromptHistory)
                _promptHistory.RemoveAt(0);
            PromptMessageChanged?.Invoke(message);
        }

        public IReadOnlyList<string> GetRecentPrompts() => _promptHistory.AsReadOnly();

        #region Preview

        public void SetPreview(string key, Entity entity)
        {
            _previews[key] = entity;
        }

        public void RemovePreview(string key)
        {
            _previews.Remove(key);
        }

        public void ClearAllPreviews()
        {
            _previews.Clear();
        }

        private void UpdateRubberband(Point3d worldPt)
        {
            if (_activePointOptions?.BasePoint is { } basePt)
            {
                var line = new Line(basePt, worldPt);
                _previews["$rubberband"] = line;
            }
        }

        #endregion

        #region Callback Registration

        public void RegisterGetPoint(PromptPointOptions options, Action<PromptPointResult> callback)
        {
            _pointCallback = callback;
            _activePointOptions = options;
            _keywords = options.Keywords ?? Array.Empty<string>();
            _keywordCallback = null;
            _distanceCallback = null;
            _stringCallback = null;

            var prompt = BuildPromptText(options.Message, _keywords);
            if (CurrentPrompt != prompt)
            {
                CurrentPrompt = prompt;
                CurrentPromptChanged?.Invoke(prompt);
            }
        }

        public void RegisterGetDistance(PromptDistanceOptions options, Action<PromptDoubleResult> callback)
        {
            _distanceCallback = callback;
            _pointCallback = null;
            _activePointOptions = options;
            _keywords = options.Keywords ?? Array.Empty<string>();
            _keywordCallback = null;
            _stringCallback = null;

            var prompt = BuildPromptText(options.Message, _keywords);
            if (CurrentPrompt != prompt)
            {
                CurrentPrompt = prompt;
                CurrentPromptChanged?.Invoke(prompt);
            }
        }

        public void RegisterGetString(PromptStringOptions options, Action<PromptStringResult> callback)
        {
            _stringCallback = callback;
            _pointCallback = null;
            _distanceCallback = null;
            _keywordCallback = null;
            _activePointOptions = null;
            _keywords = Array.Empty<string>();

            CurrentPrompt = options.Message + ":";
            CurrentPromptChanged?.Invoke(CurrentPrompt);
        }

        public void RegisterGetKeywords(PromptKeywordOptions options, Action<PromptKeywordResult> callback)
        {
            _keywordCallback = callback;
            _pointCallback = null;
            _distanceCallback = null;
            _stringCallback = null;
            _activePointOptions = null;
            _keywords = options.Keywords ?? Array.Empty<string>();

            var prompt = BuildPromptText(options.Message, _keywords);
            if (CurrentPrompt != prompt)
            {
                CurrentPrompt = prompt;
                CurrentPromptChanged?.Invoke(prompt);
            }
        }

        public void RegisterGetEntity(PromptEntityOptions options, Action<PromptEntityResult> callback)
        {
            _pointCallback = null;
            _distanceCallback = null;
            _stringCallback = null;
            _keywordCallback = null;
            _selectionManager.BeginGetEntity(options, callback);
        }

        public void RegisterGetSelection(PromptSelectionOptions options, Action<PromptSelectionResult> callback)
        {
            _pointCallback = null;
            _distanceCallback = null;
            _stringCallback = null;
            _keywordCallback = null;
            _selectionManager.BeginGetSelection(options, callback);
        }

        public void ResetPromptToCommand()
        {
            if (_controller.ActiveCommand != null)
                SetCurrentPrompt(_controller.ActiveCommand.LocalName);
        }

        public void AcceptPrompt()
        {
            if (HasEditingPrompt)
                FinishActivePrompt(PromptStatus.None);
        }

        public void RegisterMouseMove(Action<Point3d> callback)
        {
            _mouseMoveCallback = callback;
        }

        public void ClearAllRegistrations()
        {
            _pointCallback = null;
            _distanceCallback = null;
            _stringCallback = null;
            _keywordCallback = null;
            _mouseMoveCallback = null;
            _activePointOptions = null;
            _keywordHandler = null;
            _keywords = Array.Empty<string>();
            _previews.Clear();
            _selectionManager.Cancel();
        }

        private string ActiveLocalName =>
            _controller.ActiveCommand?.GetType() == typeof(BaseCommand)
                ? ""
                : _controller.ActiveCommand?.LocalName ?? "";

        private string BuildPromptText(string message, string[] keywords)
        {
            var prefix = string.IsNullOrEmpty(ActiveLocalName)
                ? ""
                : ActiveLocalName + " ";
            return keywords is { Length: > 0 }
                ? $"{prefix}{message} or [{string.Join("/", keywords)}]:"
                : $"{prefix}{message}:";
        }

        #endregion

        #region Input Dispatch

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            if (_selectionManager.IsActive)
            {
                _selectionManager.OnPointerPressed(worldPt,
                    e.GetPosition(_controller.Viewport),
                    (e.KeyModifiers & KeyModifiers.Shift) != 0);
                return;
            }

            if (_pointCallback != null)
            {
                _pointCallback(new PromptPointResult(PromptStatus.OK, worldPt, ""));
                return;
            }

            if (_distanceCallback != null)
            {
                double distance = _activePointOptions?.BasePoint is { } bp
                    ? bp.DistanceTo(worldPt)
                    : worldPt.DistanceTo(Point3d.Origin);
                _distanceCallback(new PromptDoubleResult(PromptStatus.OK, distance, ""));
                return;
            }
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            if (_selectionManager.IsActive)
            {
                _selectionManager.OnPointerMoved(worldPt);
                return;
            }

            UpdateRubberband(worldPt);
            _mouseMoveCallback?.Invoke(worldPt);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                _controller.SetCommand(new EraseCommand());
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                CancelActivePrompt(PromptStatus.Cancel);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                if (HasEditingPrompt)
                    FinishActivePrompt(PromptStatus.None);
                else
                    TryRepeatLastCommand();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up)
            {
                if (!HasEditingPrompt)
                {
                    var text = NavigateHistory(1);
                    NavigateToPromptRequested?.Invoke(text);
                }
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Down)
            {
                if (!HasEditingPrompt)
                {
                    var text = NavigateHistory(-1);
                    NavigateToPromptRequested?.Invoke(text);
                }
                e.Handled = true;
                return;
            }
        }

        private void CancelActivePrompt(PromptStatus status)
        {
            if (_pointCallback != null)
                _pointCallback(new PromptPointResult(status));
            else if (_distanceCallback != null)
                _distanceCallback(new PromptDoubleResult(status));
            else if (_stringCallback != null)
                _stringCallback(new PromptStringResult(status));
            else if (_keywordCallback != null)
                _keywordCallback(new PromptKeywordResult(status));
            else
                _controller.CancelCurrentCommand();
        }

        private void FinishActivePrompt(PromptStatus status)
        {
            if (_pointCallback != null)
                _pointCallback(new PromptPointResult(status));
            else if (_distanceCallback != null)
                _distanceCallback(new PromptDoubleResult(status));
            else if (_stringCallback != null)
                _stringCallback(new PromptStringResult(status));
            else if (_keywordCallback != null)
                _keywordCallback(new PromptKeywordResult(status));
        }

        public bool TryProcessTextInput(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            if (TryHandleKeyword(text))
                return true;

            if (_pointCallback != null)
            {
                if (Point3d.TryParse(text, out var point))
                {
                    _pointCallback(new PromptPointResult(PromptStatus.OK, point, ""));
                    return true;
                }
                SetPromptMessage(HasKeywords ? MsgPointOrKeyword : MsgInvalidPoint);
                return true;
            }

            if (_distanceCallback != null)
            {
                if (double.TryParse(text, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var numericValue))
                {
                    _distanceCallback(new PromptDoubleResult(PromptStatus.OK, numericValue, ""));
                    return true;
                }
                SetPromptMessage(HasKeywords ? MsgDistanceOrKeyword : MsgInvalidDistance);
                return true;
            }

            if (_stringCallback != null)
            {
                _stringCallback(new PromptStringResult(PromptStatus.OK, text));
                return true;
            }

            if (_keywordCallback != null)
            {
                SetPromptMessage(MsgKeywordRequired);
                return true;
            }

            return false;
        }

        #endregion

        #region Command History

        public string? NavigateHistory(int delta)
        {
            var history = _controller.CmdManager.CommandHistory;
            if (history.Count == 0) return null;

            _historyIndex += delta;
            if (_historyIndex < -1)
                _historyIndex = -1;
            if (_historyIndex >= history.Count)
                _historyIndex = history.Count - 1;

            return _historyIndex >= 0 ? history[_historyIndex] : null;
        }

        public void ResetHistoryIndex()
        {
            _historyIndex = -1;
        }

        public bool TryRepeatLastCommand()
        {
            var history = _controller.CmdManager.CommandHistory;
            if (history.Count == 0) return false;

            _ = _controller.CmdManager.ExecuteCommand(history[0]);
            return true;
        }

        #endregion
    }
}
