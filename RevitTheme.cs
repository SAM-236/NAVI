using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
#if REVIT2024_OR_GREATER
using Autodesk.Revit.UI;
#endif

namespace NAVI
{
    /// <summary>Brushes that match Revit's light and dark UI themes.</summary>
    public sealed class RevitTheme
    {
        public bool IsDark { get; private set; }
        public Brush WindowBackground { get; private set; }
        public Brush Surface { get; private set; }
        public Brush Text { get; private set; }
        public Brush SubtleText { get; private set; }
        public Brush Border { get; private set; }
        public Brush ButtonBackground { get; private set; }
        public Brush ButtonHover { get; private set; }
        public Brush Accent { get; private set; }
        public Brush AccentHover { get; private set; }
        public Brush Warning { get; private set; }

        public static RevitTheme Resolve(ThemeMode mode) =>
            (mode == ThemeMode.Dark || (mode == ThemeMode.Auto && RevitIsDark())) ? Dark() : Light();

        static bool RevitIsDark()
        {
#if REVIT2024_OR_GREATER
            try { return UIThemeManager.CurrentTheme == UITheme.Dark; }
            catch { }
#endif
            return false;
        }

        static RevitTheme Dark() => new RevitTheme
        {
            IsDark = true,
            WindowBackground = Hex("#3B4453"),
            Surface = Hex("#2E3440"),
            Text = Hex("#F2F4F7"),
            SubtleText = Hex("#B9C1CE"),
            Border = Hex("#566072"),
            ButtonBackground = Hex("#4A5466"),
            ButtonHover = Hex("#576276"),
            Accent = Hex("#0696D7"),
            AccentHover = Hex("#28A9E6"),
            Warning = Hex("#F5A623"),
        };

        static RevitTheme Light() => new RevitTheme
        {
            IsDark = false,
            WindowBackground = Hex("#F5F5F5"),
            Surface = Hex("#FFFFFF"),
            Text = Hex("#1E1E1E"),
            SubtleText = Hex("#555B66"),
            Border = Hex("#C8CCD2"),
            ButtonBackground = Hex("#E6E8EB"),
            ButtonHover = Hex("#D7DADF"),
            Accent = Hex("#0696D7"),
            AccentHover = Hex("#0582BC"),
            Warning = Hex("#E08A00"),
        };

        static Brush Hex(string hex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        /// <summary>Dark Windows title bar (Win10 1809+). Silently does nothing elsewhere.</summary>
        public static void ApplyDarkTitleBar(IntPtr hwnd)
        {
            try
            {
                int on = 1;
                // 20 = DWMWA_USE_IMMERSIVE_DARK_MODE (Win10 20H1+), 19 on earlier builds.
                if (DwmSetWindowAttribute(hwnd, 20, ref on, sizeof(int)) != 0)
                    DwmSetWindowAttribute(hwnd, 19, ref on, sizeof(int));
            }
            catch { }
        }
    }
}
