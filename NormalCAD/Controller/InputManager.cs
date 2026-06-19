using System;
using System.Collections.Generic;
using Avalonia.Input;
using NormalCAD.Core.Geometry;
using NormalCAD.Controller.Commands;

namespace NormalCAD.Controller
{
    public class InputManager
    {
        private readonly CadController _controller;
        private readonly List<string> _promptHistory = new();
        private const int MaxPromptHistory = 100;

        public event Action<string>? PromptMessageChanged;
        public event Action<string>? CurrentPromptChanged;

        public string CurrentPrompt { get; private set; } = "CMD";

        public InputManager(CadController controller)
        {
            _controller = controller;
        }

        public void SetPromptMessage(string message)
        {
            _promptHistory.Add(message);
            if (_promptHistory.Count > MaxPromptHistory)
                _promptHistory.RemoveAt(0);
            PromptMessageChanged?.Invoke(message);
        }

        public void SetCurrentPrompt(string prompt)
        {
            if (CurrentPrompt == prompt) return;
            CurrentPrompt = prompt;
            CurrentPromptChanged?.Invoke(prompt);
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
