using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Functions
{
    public static class Themes
    {
        public static Theme DefaultTheme { get; set; } = new Theme("Default", "The default theme.", RTColor.FromHex("211F1D"));

        public static Theme CurrentTheme { get; set; } = DefaultTheme;

        public static Dictionary<string, Theme> ThemesDictionary { get; set; } = new Dictionary<string, Theme>
        {
            { "Default", DefaultTheme }
        };

        public class Theme
        {
            public Theme(string name, string description, RTColor backgroundColor)
            {
                Name = name;
                Description = description;
                BackgroundColor = backgroundColor;
            }

            public string Name { get; set; }
            public string Description { get; set; }

            public RTColor BackgroundColor { get; set; }
        }
    }
}
