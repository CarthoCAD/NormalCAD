using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NormalCAD.Controller.Commands;
using NormalCAD.Resources;

namespace NormalCAD.Controller
{
    public class CmdManager
    {
        private static string MsgCannotCallDirectly => CommandResources.Get("CMD.MSG.CANNOT_CALL_DIRECTLY");
        private static string MsgEcho => CommandResources.Get("CMD.MSG.ECHO");
        private static string MsgUnknownCommand => CommandResources.Get("CMD.MSG.UNKNOWN_COMMAND");

        private readonly CadController _controller;
        private readonly Dictionary<string, ICadCommand> _commands = [];
        private readonly List<string> _commandHistory = new();
        private const int MaxHistory = 50;

        public IReadOnlyList<string> CommandHistory => _commandHistory;

        public CmdManager(CadController cadController)
        {
            _controller = cadController;
            DiscoverCommands();
            Services.LanguageService.LanguageChanged += RebuildIndex;
        }

        private void DiscoverCommands()
        {
            var commandTypes = typeof(ICadCommand).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface
                    && typeof(ICadCommand).IsAssignableFrom(t)
                    && t != typeof(BaseCommand));

            foreach (var type in commandTypes)
            {
                var cmd = (ICadCommand)Activator.CreateInstance(type)!;
                RegisterCommand(cmd);
            }
        }

        private void RegisterCommand(ICadCommand cmd)
        {
            _commands[cmd.Name.ToLower()] = cmd;
            _commands[cmd.LocalName.ToLower()] = cmd;

            foreach (var alias in cmd.Aliases)
            {
                if (!string.IsNullOrEmpty(alias))
                {
                    _commands[alias.ToLower()] = cmd;
                }
            }
        }

        public async Task ExecuteCommand(string cmdName)
        {
            string input = cmdName.Trim();
            if (string.IsNullOrEmpty(input)) return;

            string key = input.ToLower();

            if (_commands.TryGetValue(key, out var cmd))
            {
                var localName = cmd.LocalName.ToUpperInvariant();
                if (_commandHistory.Count == 0 || _commandHistory[0] != localName)
                {
                    _commandHistory.Insert(0, localName);
                    if (_commandHistory.Count > MaxHistory)
                        _commandHistory.RemoveAt(_commandHistory.Count - 1);
                }

                _controller.InputManager.SetPromptMessage(string.Format(MsgEcho, cmd.LocalName));
                _controller.SetCommand(cmd);
            }
            else
            {
                _controller.InputManager.SetPromptMessage(string.Format(MsgUnknownCommand, input));
            }

            await Task.CompletedTask;
        }

        public void RebuildIndex()
        {
            _commands.Clear();
            DiscoverCommands();
        }
    }
}
