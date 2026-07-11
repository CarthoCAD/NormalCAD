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

        public CmdManager(CadController cadController)
        {
            _controller = cadController;
            DiscoverCommands();
            Services.LanguageService.LanguageChanged += RebuildIndex;
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
                    _controller.InputManager.SetPromptMessage(string.Format(MsgCannotCallDirectly, input));
                    return;
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
