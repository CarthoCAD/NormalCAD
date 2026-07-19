using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NormalCAD.Controller.Commands
{
    public interface ICadCommand
    {
        string Name { get; }
        string LocalName { get; }
        string Alias { get; }
        IReadOnlyList<string> Aliases => Alias
            .Split(',')
            .Select(a => a.Trim())
            .Where(a => a.Length > 0)
            .ToList();
        CommandType Type { get; }
        CommandFlags Flags { get; }
        Task ActivateAsync(CadController controller);
        void Deactivate();
    }
}
