using System;
using System.Globalization;

namespace NormalCAD.Controller.Services
{
    public static class LanguageService
    {
        public static event Action? LanguageChanged;

        public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.InvariantCulture;

        public static void Initialize()
        {
            ApplyCulture(ConfigService.Current.Language);
        }

        public static void SetCulture(string cultureName)
        {
            ApplyCulture(cultureName);
            ConfigService.Update(c => c.Language = cultureName);
            LanguageChanged?.Invoke();
        }

        private static void ApplyCulture(string cultureName)
        {
            var culture = string.IsNullOrEmpty(cultureName)
                ? CultureInfo.InvariantCulture
                : new CultureInfo(cultureName);

            CurrentCulture = culture;

            // Use the global DefaultThreadCurrentUICulture rather than the thread's
            // explicit CurrentUICulture. Avalonia's dispatcher invokes event handlers
            // under a captured ExecutionContext that restores the thread culture after
            // each message, which would revert a per-thread change: strings read during
            // the toggle handler (command names, menu) would localize, but strings read
            // in later handlers (interaction prompts/messages) would fall back to the
            // previous language. A global default is not part of the ExecutionContext,
            // so it persists across messages.
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        public static string GetDisplayLabel()
        {
            if (CurrentCulture.Name == "pt-BR")
                return Resources.CommandResources.Get("LANGUAGE.MSG.PTBR");
            return Resources.CommandResources.Get("LANGUAGE.MSG.EN");
        }
    }
}
