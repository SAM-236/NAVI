NAVI - CAD Import Guard for Revit 2020-2026
===========================================

Warns (or blocks) when someone uses Insert > Import CAD and offers
Insert > Link CAD instead. One config file controls every Revit version.


WHAT'S IN THE ZIP
-----------------
The zip mirrors the machine-wide Revit add-in folder:

  2020\NAVI.addin            2020\NAVI\NAVI.dll
  2021\NAVI.addin            2021\NAVI\NAVI.dll
  2022\NAVI.addin            2022\NAVI\NAVI.dll
  2023\NAVI.addin            2023\NAVI\NAVI.dll
  2024\NAVI.addin            2024\NAVI\NAVI.dll
  2025\NAVI.addin            2025\NAVI\NAVI.dll
  2026\NAVI.addin            2026\NAVI\NAVI.dll
  NAVI\CadImportGuard.ini    (shared config for all versions)

Each NAVI.dll is built for its own Revit year. Don't mix them between folders.


INSTALL
-------
1. Close Revit.
2. Extract the zip into:

       C:\ProgramData\Autodesk\Revit\Addins\

   so you end up with, for example:

       C:\ProgramData\Autodesk\Revit\Addins\2024\NAVI.addin
       C:\ProgramData\Autodesk\Revit\Addins\2024\NAVI\NAVI.dll
       C:\ProgramData\Autodesk\Revit\Addins\NAVI\CadImportGuard.ini

   Writing to ProgramData usually needs admin rights.
   Folders for Revit years that aren't installed can be skipped or left; Revit ignores them.

3. Only copy NAVI\CadImportGuard.ini if the machine doesn't already have one,
   so you don't overwrite settings the ADT Team has changed.
   (If the file is missing, NAVI creates one with the defaults the first time Revit starts.)

4. Start Revit. The first time, Revit may ask whether to load NAVI. Choose "Always Load".


CONFIG
------
File:  C:\ProgramData\Autodesk\Revit\Addins\NAVI\CadImportGuard.ini
Read once when Revit starts. Restart Revit after editing.

  Enabled          = True    False = NAVI doesn't touch Import CAD at all
  AllowImport      = True    True = user may click "Import anyway"
                             False = import is always cancelled (offers Link CAD instead)
  ApplyInProjects  = True    Run the guard in project files (.rvt)
  ApplyInFamilies  = False   Run the guard in family files (.rfa)
  ExemptAdtMembers = True    ADT members skip the guard entirely...
  ADT Member       = False   ...when this machine is marked as an ADT member
  Theme            = Auto    Auto follows Revit's theme (2024+). Light or Dark forces it.


UNINSTALL
---------
Close Revit and delete, for each year:

  C:\ProgramData\Autodesk\Revit\Addins\<year>\NAVI.addin
  C:\ProgramData\Autodesk\Revit\Addins\<year>\NAVI\

Optionally delete C:\ProgramData\Autodesk\Revit\Addins\NAVI\ (the config).


REBUILDING THE ZIP
------------------
From the NAVI folder, in PowerShell:

    .\Deploy\Make-DeployZip.ps1

Builds the R2020-R2026 configurations and writes NAVI_Revit2020-2026.zip next to this file.
Revit 2027 needs the .NET 10 SDK (Visual Studio 2026); add it with  -Versions 2020..2027
