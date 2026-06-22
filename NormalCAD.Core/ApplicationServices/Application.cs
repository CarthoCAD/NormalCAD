namespace NormalCAD.Core.ApplicationServices
{
    public static class Application
    {
        internal static IApplicationHost? Host { get; set; }

        public static DocumentCollection DocumentManager
            => Host?.DocumentManager
               ?? throw new System.InvalidOperationException(
                   "Application is not initialized. The host must set Application.Host before use.");

        public static void ShowAlertDialog(string message)
            => Host?.ShowAlertDialog(message);
    }
}
