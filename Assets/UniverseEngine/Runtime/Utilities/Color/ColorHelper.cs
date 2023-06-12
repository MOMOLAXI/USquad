using System;

namespace UniverseEngine
{
    public static class ColorHelper
    {
        const string TAG_FORMAT = "[<color=#{0}>{1}</color>]";
        
        public static string GetStringRichColorText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            Guid guid = GuidHelper.NewStatic(text);
            string color = GetColorWithAlpha(guid);
            return StringUtilities.Format(TAG_FORMAT, color, text);
        }

        public static string GetColorWithAlpha(Guid guid)
        {
            return guid.ToString().Substring(0, 6);
        }
    }
}

