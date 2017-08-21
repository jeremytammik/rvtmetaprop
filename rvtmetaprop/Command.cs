#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Windows.Forms;
using System.IO;
#endregion

namespace rvtmetaprop
{
  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    static string _default_folder = "";
    static string _filename = "";

    /// <summary>
    /// Select a specified file in the given folder.
    /// </summary>
    /// <param name="folder">Initial folder.</param>
    /// <param name="filename">Selected filename on 
    /// success.</param>
    /// <returns>Return true if a file was successfully 
    /// selected.</returns>
    static bool FileSelect(
      string folder,
      string title,
      string filter,
      ref string filename )
    {
      bool rc = false;
      using( OpenFileDialog dlg = new OpenFileDialog() )
      {
        dlg.Title = title;
        dlg.CheckFileExists = true;
        dlg.CheckPathExists = true;
        dlg.InitialDirectory = folder;
        dlg.FileName = filename;
        dlg.Filter = filter;
        rc = ( DialogResult.OK == dlg.ShowDialog() );
        filename = dlg.FileName;
      }
      return rc;
    }

    /// <summary>
    /// Select a meta property file in the given folder.
    /// </summary>
    /// <param name="folder">Initial folder.</param>
    /// <param name="filename">Selected filename on 
    /// success.</param>
    /// <returns>Return true if a file was successfully 
    /// selected.</returns>
    static public bool FileSelectMetaProp(
      string folder,
      ref string filename )
    {
      return FileSelect( folder,
        "Select meta property file",
        "CSV Files (*.csv)|*.csv|JSON Files (*.json)|*.json|All Files|*.*",
        ref filename );
    }
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      if( !FileSelectMetaProp(
        _default_folder,
        ref _filename ) )
      {
        return Result.Cancelled;
      }

      _default_folder = Path.GetDirectoryName( _filename );


      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Document doc = uidoc.Document;

      // Access current selection

      Selection sel = uidoc.Selection;

      // Retrieve elements from database

      FilteredElementCollector col
        = new FilteredElementCollector( doc )
          .WhereElementIsNotElementType()
          .OfCategory( BuiltInCategory.INVALID )
          .OfClass( typeof( Wall ) );

      // Filtered element collector is iterable

      foreach( Element e in col )
      {
        Debug.Print( e.Name );
      }

      // Modify document within a transaction

      using( Transaction tx = new Transaction( doc ) )
      {
        tx.Start( "Transaction Name" );
        tx.Commit();
      }

      return Result.Succeeded;
    }
  }
}
