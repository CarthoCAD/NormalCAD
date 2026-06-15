using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NormalCAD.Controller.Commands;

namespace NormalCAD.Controller
{
    public class CmdManager
    {
        private readonly CadController _controller;
        private readonly Dictionary<string, ICadCommand> _commands = [];

        public CmdManager(CadController cadController)
        {
            _controller = cadController;
            DiscoverCommands();
        }

        private void DiscoverCommands()
        {
            var commandTypes = typeof(ICadCommand).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && typeof(ICadCommand).IsAssignableFrom(t));

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

            if (!string.IsNullOrWhiteSpace(cmd.Alias))
            {
                foreach (var alias in cmd.Alias.Split(','))
                {
                    var trimmed = alias.Trim().ToLower();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        _commands[trimmed] = cmd;
                    }
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
                if (cmd.IsInternal)
                {
                    _controller.InputManager.SetPromptMessage($"Command '{input}' cannot be called directly.");
                    return;
                }

                _controller.InputManager.SetPromptMessage($"command: {cmd.LocalName}");
                _controller.SetCommand(cmd);
            }
            else
            {
                _controller.InputManager.SetPromptMessage($"Unknown command '{input}'. Press F1 for help.");
            }

            await Task.CompletedTask;
        }
    }
}
