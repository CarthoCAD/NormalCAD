using NormalCAD.Core.ApplicationServices;

namespace NormalCAD.Core
{
    internal interface IApplicationHost
    {
        DocumentCollection DocumentManager { get; }

        Document CreateDocument();

        void ShowAlertDialog(string message);
    }
}
