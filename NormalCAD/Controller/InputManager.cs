using System;
using System.Collections.Generic;
using Avalonia.Input;
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

        private readonly CadController _controller;
        private readonly List<string> _promptHistory = new();
        private const int MaxPromptHistory = 100;

        private string[] _keywords = Array.Empty<string>();
        private Action<string>? _keywordHandler;

        public bool HasKeywords => _keywords.Length > 0;

        public event Action<string>? PromptMessageChanged;
        public event Action<string>? CurrentPromptChanged;

        public string CurrentPrompt { get; private set; } = DefaultPrompt;

        public InputManager(CadController controller)
        {
            _controller = controller;
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
            if (!HasKeywords)
                return false;

            var handler = _keywordHandler;
            if (handler == null)
                return false;

            foreach (var kw in _keywords)
            {
                if (string.Equals(kw, text, StringComparison.OrdinalIgnoreCase))
                {
                    handler(kw);
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
                handler(matches[0]);
                return true;
            }

            if (matches.Count > 1)
                SetPromptMessage(MsgAmbiguousKeyword);
            else
                SetPromptMessage(MsgKeywordRequired);

            return true;
        }

        public void SetPromptMessage(string message)
        {
            _promptHistory.Add(message);
            if (_promptHistory.Count > MaxPromptHistory)
                _promptHistory.RemoveAt(0);
            PromptMessageChanged?.Invoke(message);
        }

        public IReadOnlyList<string> GetRecentPrompts() => _promptHistory.AsReadOnly();

        public void OnPointerPressed(Point3d worldPt, PointerPressedEventArgs e)
        {
            _controller.ActiveCommand?.OnPointerPressed(worldPt, e);
        }

        public void OnPointerMoved(Point3d worldPt)
        {
            _controller.ActiveCommand?.OnPointerMoved(worldPt);
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
                _controller.CancelCurrentCommand();
                e.Handled = true;
                return;
            }

            _controller.ActiveCommand?.OnKeyDown(e);
        }
    }
}
