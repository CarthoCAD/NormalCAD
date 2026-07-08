namespace NormalCAD.Core.ApplicationServices
{
    internal interface IApplicationHost
    {
        DocumentCollection DocumentManager { get; }

        Document CreateDocument();

        void ShowAlertDialog(string message);
    }
}
