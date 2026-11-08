using System;
using System.Windows;
using System.Windows.Interop;

namespace NAVI
{
    public enum CadImportChoice { Cancel, LinkInstead, ImportAnyway }

    public partial class CadImportDialog : Window
    {
        readonly RevitTheme _theme;
        readonly GuardMode _mode;

        public CadImportChoice Choice { get; private set; } = CadImportChoice.Cancel;

        CadImportDialog(GuardMode mode, bool isFamilyDocument, RevitTheme theme)
        {
            _mode = mode;
            _theme = theme;
            InitializeComponent();
            ApplyTheme();

            string where = isFamilyDocument ? "families" : "project files";
            BodyText.Text =
                $"CAD imports can slow {where} down drastically.\n" +
                "Best practice is to use Insert > Link CAD instead.";

            DetailsText.Text =
                "Imported CAD brings every layer, line pattern and block into the model, and it stays " +
                "there even after the import is deleted. Exploding an import multiplies that into " +
                "thousands of elements.\n\n" +
                "Talk to the ADT Team about the risks before importing or exploding CAD.";

            if (mode == GuardMode.Block)
            {
                PolicyNote.Visibility = Visibility.Visible;
                SecondaryButton.Content = "Cancel";
                SecondaryButton.IsCancel = true;
            }
            else
            {
                SecondaryButton.Content = "Import anyway";
            }

            Loaded += (s, e) => PrimaryButton.Focus();
            // Esc or the title-bar X leaves Choice = Cancel, which cancels the import.
            PreviewKeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Escape) Close(); };
        }

        /// <summary>Shows the warning parented to Revit and returns what the user picked.</summary>
        public static CadImportChoice Prompt(IntPtr revitWindow, GuardConfig config, bool isFamilyDocument)
        {
            var dialog = new CadImportDialog(config.Mode, isFamilyDocument, RevitTheme.Resolve(config.Theme));
            if (revitWindow != IntPtr.Zero)
                new WindowInteropHelper(dialog).Owner = revitWindow;

            dialog.ShowDialog();
            return dialog.Choice;
        }

        void ApplyTheme()
        {
            Resources["WindowBackground"] = _theme.WindowBackground;
            Resources["Surface"] = _theme.Surface;
            Resources["Text"] = _theme.Text;
            Resources["SubtleText"] = _theme.SubtleText;
            Resources["Border"] = _theme.Border;
            Resources["ButtonBackground"] = _theme.ButtonBackground;
            Resources["ButtonHover"] = _theme.ButtonHover;
            Resources["Accent"] = _theme.Accent;
            Resources["AccentHover"] = _theme.AccentHover;
            Resources["Warning"] = _theme.Warning;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (_theme.IsDark)
                RevitTheme.ApplyDarkTitleBar(new WindowInteropHelper(this).Handle);
        }

        void OnLinkInstead(object sender, RoutedEventArgs e)
        {
            Choice = CadImportChoice.LinkInstead;
            Close();
        }

        void OnSecondary(object sender, RoutedEventArgs e)
        {
            Choice = _mode == GuardMode.Allow ? CadImportChoice.ImportAnyway : CadImportChoice.Cancel;
            Close();
        }

        void OnToggleDetails(object sender, RoutedEventArgs e)
        {
            bool show = DetailsText.Visibility != Visibility.Visible;
            DetailsText.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            DetailsToggle.Content = show ? "Hide details" : "Show details";
        }
    }
}
