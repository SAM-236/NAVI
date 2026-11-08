#region Namespaces
using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

#endregion

namespace NAVI
{
    public class App : IExternalApplication
    {
        GuardConfig _config;
        AddInCommandBinding _importBinding;

        public Result OnStartup(UIControlledApplication a)
        {
            _config = GuardConfig.Load();
            if (!_config.ShouldHook) { return Result.Succeeded; }
            try
            {
                RevitCommandId CadImportcommandId = RevitCommandId.LookupCommandId("ID_FILE_IMPORT");

                //Check if Command Already Bound. If so skip
                if (CadImportcommandId != null && CadImportcommandId.CanHaveBinding && !CadImportcommandId.HasBinding)
                {
                    _importBinding = a.CreateAddInCommandBinding(CadImportcommandId);
                    _importBinding.BeforeExecuted += PreCadImport;
                }
            }
            catch
            {
                _importBinding = null;
            }
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            if (_importBinding != null)
            {
                _importBinding.BeforeExecuted -= PreCadImport;
                try { a.RemoveAddInCommandBinding(_importBinding.RevitCommandId); }
                catch { /*Revit closing either way*/}
                _importBinding = null;
            }
            return Result.Succeeded;
        }

        void PreCadImport(object sender, BeforeExecutedEventArgs e)
        {
            try
            {
                Document doc = e.ActiveDocument;
                if (doc == null || !_config.AppliesTo(doc.IsFamilyDocument)) { return; }

                var uiapp = sender as UIApplication;
                IntPtr owner = uiapp?.MainWindowHandle ?? IntPtr.Zero;

                CadImportChoice choice = CadImportDialog.Prompt(owner, _config, doc.IsFamilyDocument);
                if (choice == CadImportChoice.ImportAnyway && _config.Mode == GuardMode.Allow) { return; }
                e.Cancel = true;

                if (choice == CadImportChoice.LinkInstead && uiapp != null)
                {
                    RevitCommandId linkCad = RevitCommandId.LookupPostableCommandId(PostableCommand.LinkCAD);
                    if (linkCad != null && uiapp.CanPostCommand(linkCad))
                    {
                        uiapp.PostCommand(linkCad);
                    }
                }
            }
            catch
            {
                // Import is not cancelled, so the user can keep working.
                TaskDialog.Show("Congrats Critical Error", "This should never happen. Go buy a lotto ticket!");
            }
        }
    }
}
