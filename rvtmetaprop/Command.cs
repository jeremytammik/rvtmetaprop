#region Namespaces
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace rvtmetaprop
{
  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    #region Input File Handling

#if DEBUG
    static string _default_folder = "C:/a/vs/rvtmetaprop/test";
#else
    static string _default_folder = "";
#endif // _DEBUG


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
        //"CSV Files (*.csv)|*.csv|JSON Files (*.json)|*.json|All Files|*.*",
        "Meta Property Files (*.csv;*.json)|*.csv;*.json|All Files|*.*",
        ref filename );
    }
    #endregion // Input File Handling

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      int n;

      #region Select meta property input file

      if( !FileSelectMetaProp(
        _default_folder,
        ref _filename ) )
      {
        return Result.Cancelled;
      }

      _default_folder = Path.GetDirectoryName( _filename );

      #endregion // Select meta property input file

      #region Deserialise meta properties from input file

      List<MetaProp> props = null;

      if( _filename.ToLower().EndsWith( ".json" ) )
      {
        string s = File.ReadAllText( _filename );

        props = JsonConvert
          .DeserializeObject<List<MetaProp>>( s );

        Debug.Print( props.Count + " props deserialised" );
      }
      else if( _filename.ToLower().EndsWith( ".csv" ) )
      {
        IEnumerable<IList<string>> a
          = EasyCsv.FromFile( _filename, true );

        n = a.Count();
        Debug.Print( n + " props deserialised" );
        props = new List<MetaProp>( n );
        foreach( IList<string> rec in a )
        {
          props.Add( new MetaProp( rec ) );
        }
      }
      else
      {
        message = "Unhandled meta property file format: "
          + Path.GetExtension( _filename );
        return Result.Failed;
      }

      #endregion // Deserialise meta properties from input file

      // Special 'Model' properties have extenalId prefix 'doc_'

      IEnumerable<MetaProp> doc_props
        = props.Where<MetaProp>( m
          => m.IsModelProperty );

      int nModelProp = doc_props.Count<MetaProp>();

      Debug.Print( nModelProp.ToString()
        + " 'Model' properties have extenalId prefix 'doc_'" );

      props.RemoveAll( m => m.IsModelProperty );

      // Test that original elements are present in model

      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Document doc = uidoc.Document;

      IEnumerable<MetaProp> missing
        = props.Where<MetaProp>( m
          => null == doc.GetElement( m.externalId ) );

      n = missing.Count<MetaProp>();

      if( 0 < n )
      {
        string s = string.Format(
          "{0} invalid unique id{1}: ",
          n, ( 1 == n ) ? "" : "s" );

        TaskDialog d = new TaskDialog( s );

        s = string.Join( "\r\n",
          missing.Select<MetaProp, string>(
            m => m.component ) );

        s += "\r\n\r\n" + nModelProp
          + " model properties ignored";

        d.MainContent = s;
        d.Show();
      }

      props.RemoveAll(
        m => null == doc.GetElement(
          m.externalId ) );

      // Create dictionary mapping parameter name to 
      // shared parameter definition input data

      Dictionary<string, ParamDef> paramdefs
        = new Dictionary<string, ParamDef>();

      foreach( MetaProp m in props )
      {
        string s = m.displayName;

        if( !paramdefs.ContainsKey( s ) )
        {
          paramdefs.Add( s, new ParamDef( m ) );
        }

        ParamDef def = paramdefs[s];

        Element e = doc.GetElement( m.externalId );
        Category cat = e.Category;
        ElementId id = cat.Id;

        if( !def.Categories.Contains( id ) )
        {
          def.Categories.Add( id );
        }

        // Check for existing parameters

        IList<Parameter> a = e.GetParameters( m.displayName );

        n = a.Count;

        if( 0 < n )
        {
          // Property already exists on element

          if( 1 < n )
          {
            Debug.Print( string.Format(
              "{0} already has {1} parameters named {2}",
              m.component, n, m.displayName ) );
          }

          foreach( Parameter p in a )
          {
            Definition pdef = p.Definition;
            ExternalDefinition extdef = pdef as ExternalDefinition;
            InternalDefinition intdef = pdef as InternalDefinition;
            Debug.Print( string.Format( "extdef {0}, intdef {1}",
              null == extdef ? "<null>" : "ok",
              null == intdef ? "<null>" : "ok" ) );

            ParameterType ptyp = pdef.ParameterType;
            if( def.Type != ptyp )
            {
              Debug.Print( string.Format(
                "{0} parameter {1} has type {2} != meta property type {3}",
                m.component, m.displayName, ptyp.ToString(), m.metaType ) );
            }
            else
            {
              //p.Set( m.displayValue );
            }
          }
        }
        else
        {
          // Property needs to be added to element
        }
      }

      // Create required shared parameters

      // Apply meta properties to model

      using( Transaction tx = new Transaction( doc ) )
      {
        tx.Start( "Import Forge Meta Properties" );
        foreach( MetaProp m in props )
        {
          Debug.Print( string.Format(
            "{0} property {1} = '{2}'", m.component,
            m.displayName, m.ParameterValue ) );

          Element e = doc.GetElement( m.externalId );

          IList<Parameter> a = e.GetParameters( m.displayName );

          n = a.Count;

          if( 0 < n )
          {
            // Property already exists on element

            if( 1 < n )
            {
              Debug.Print( string.Format(
                "{0} has {1} parameters named {2}",
                m.component, n, m.displayName ) );
            }

            foreach( Parameter p in a )
            {
              Definition pdef = p.Definition;
              ParameterType ptyp = pdef.ParameterType;
              if( ParameterType.Text != ptyp )
              {
                Debug.Print( string.Format(
                  "{0} parameter {1} has type {2}",
                  m.component, m.displayName, ptyp.ToString() ) );
              }
              else
              {
                // add support for int and double
                p.Set( m.displayValue );
              }
            }
          }
          else
          {
            // Property needs to be added to element

          }
        }
        tx.Commit();
      }

      return Result.Succeeded;
    }
  }
}
