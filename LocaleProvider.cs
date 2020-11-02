using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace Rampastring.XNAUI
{
    /// <summary>
    /// A static class that provides methods to allow
    /// for easy localization of graphic components in a
    /// centralized fashion.
    /// </summary>
    public static class LocaleProvider
    {
        private static IniFile localeIni;

        private static string STRINGS = "Strings";
        private static string GENERIC_CONTROLS = "Generic Controls";
        private static string PARENTED_CONTROLS = "Parented Controls";

        public static bool Initialized { get; set; }
        
        public static void Initialize(IniFile locale)
        {
            if (Initialized)
                throw new InvalidOperationException($"{nameof(LocaleProvider)} is already initialized.");

            localeIni = locale;
            Initialized = true;
        }

        /// <summary>
        /// Checks if the localized string exists.
        /// </summary>
        /// <param name="key">The key to be looked up.</param>
        /// <returns></returns>
        public static bool StringLocalized(string key) =>
            !string.IsNullOrWhiteSpace(key) && localeIni.KeyExists(STRINGS, key);

        /// <summary>
        /// Checks if the localized attribute value exists.
        /// </summary>
        /// <param name="parentName">The optional control parent name to be looked up.
        /// Pass <c>null</c> if looking up a generic one.</param>
        /// <param name="controlName">The control name to be looked up.</param>
        /// <param name="attribute">The control attribute name to be looked up.</param>
        /// <returns></returns>
        public static bool AttributeLocalized(string parentName, string controlName, string attribute) =>
            !string.IsNullOrWhiteSpace(controlName) && !string.IsNullOrWhiteSpace(attribute) &&
                (localeIni.KeyExists(GENERIC_CONTROLS, $"{controlName}_{attribute}") ||
                (string.IsNullOrWhiteSpace(parentName) && localeIni.KeyExists(PARENTED_CONTROLS,
                    $"{parentName}_{controlName}_{attribute}")));

        /// <summary>
        /// Looks up localized string value that isn't attached
        /// to any control (listbox entries, messages etc.)
        /// </summary>
        /// <param name="key">The key to be looked up.</param>
        /// <param name="defaultValue">Value to return if it's not localized.</param>
        /// <param name="memo">Whether to memorize the default value for stub locale file generation.</param>
        /// <returns>A localized string value or <paramref name="defaultValue"/>.</returns>
        public static string GetLocalizedStringValue(string key, string defaultValue, bool memo = false)
        {
            if (localeIni == null || string.IsNullOrWhiteSpace(defaultValue))
                return defaultValue;

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"Argument can't be null, empty or consist of whitespaces", nameof(key));

            if (memo && !localeIni.KeyExists(STRINGS, key))
                localeIni.SetStringValue(STRINGS, key, defaultValue);
            return localeIni.GetStringValue(STRINGS, key, defaultValue);
        }

        /// <summary>
        /// Looks up localized attribute value that is attached
        /// to a control, with or without accounting for control parent.
        /// </summary>
        /// <param name="parentName">The optional control parent name to be looked up.
        /// Pass <c>null</c> if looking up a generic one.</param>
        /// <param name="controlName">The control name to be looked up.</param>
        /// <param name="attribute">The control attribute name to be looked up.</param>
        /// <param name="defaultValue">Value to return if it's not localized.</param>
        /// <param name="memo">Whether to memorize the default value for stub locale file generation.</param>
        /// <returns>A localized string value or <paramref name="defaultValue"/>.</returns>
        public static string GetLocalizedAttributeValue(string parentName, string controlName, string attribute, string defaultValue, bool memo = false)
        {
            if (localeIni == null || string.IsNullOrWhiteSpace(defaultValue))
                return defaultValue;

            if (string.IsNullOrWhiteSpace(controlName))
                throw new ArgumentException($"Argument can't be null, empty or consist of whitespaces", nameof(controlName));

            if (string.IsNullOrWhiteSpace(attribute))
                throw new ArgumentException($"Argument can't be null, empty or consist of whitespaces", nameof(attribute));

            string genericKey = $"{controlName}_{attribute}";

            // Case when we need to look up only generic localized attiribute
            if (string.IsNullOrWhiteSpace(parentName))
            {
                if (memo && !localeIni.KeyExists(GENERIC_CONTROLS, genericKey))
                    localeIni.SetStringValue(GENERIC_CONTROLS, genericKey, defaultValue);
                return localeIni.GetStringValue(GENERIC_CONTROLS, genericKey, defaultValue);
            }

            string key = $"{parentName}_{controlName}_{attribute}";

            // Case when we don't have the specifically localized attribute
            // and look up generic localized one if it exists
            if (!localeIni.KeyExists(PARENTED_CONTROLS, key) &&
                localeIni.KeyExists(GENERIC_CONTROLS, genericKey))
            {
                return localeIni.GetStringValue(GENERIC_CONTROLS, genericKey, defaultValue);
            }

            if (memo && !localeIni.KeyExists(PARENTED_CONTROLS, key))
                localeIni.SetStringValue(PARENTED_CONTROLS, key, defaultValue);

            return localeIni.GetStringValue(PARENTED_CONTROLS, key, defaultValue);
        }

        /// <summary>
        /// Writes stub locale INI file with hardcoded values for translators.
        /// </summary>
        public static void GenerateLocaleIni() => localeIni.WriteIniFile();
    }
}
