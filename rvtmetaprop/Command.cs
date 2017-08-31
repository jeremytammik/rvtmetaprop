#region Namespaces
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
using Autodesk.Revit.ApplicationServices;
using System;
#endregion

namespace rvtmetaprop
{
  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    #region Create shared parameters
    /// <summary>
    /// Shared parameters filename; used only in case
    /// none is set.
    /// </summary>
    const string _shared_parameters_filename
      = "rvtmetaprop_shared_parameters.txt";

    /// <summary>
    /// Create the shared parameters.
    /// </summary>
    static void CreateSharedParameters(
      Document doc,
      Dictionary<string, ParamDef> paramdefs,
      List<string> log )
    {
      Application app = doc.Application;

      // Save original shared parameter file name

      string saveSharedParamsFileName
        = app.SharedParametersFilename;

      // Set up our own shared parameter file name

      string path = Path.GetTempPath();

      path = Path.Combine( path,
        _shared_parameters_filename );

      StreamWriter stream;
      stream = new StreamWriter( path );
      stream.Close();

      app.SharedParametersFilename = path;

      path = app.SharedParametersFilename;

      // Retrieve shared parameter file object

      DefinitionFile f
        = app.OpenSharedParameterFile();

      List<string> keys = new List<string>( paramdefs.Keys );
      keys.Sort();
      foreach( string pname in keys )
      {
        ParamDef def = paramdefs[pname];

        // Create the category set for binding

        Binding binding = app.Create.NewInstanceBinding(
          def.Categories );

        // Retrieve or create shared parameter group

        DefinitionGroup group
          = f.Groups.get_Item( def.GroupName )
          ?? f.Groups.Create( def.GroupName );

        // Retrieve or create the parameter;
        // we could check if they are already bound, 
        // but it looks like Insert will just ignore 
        // them in that case.

        Definition definition = group.Definitions.get_Item(
          pname );

        try
        {
          if( null == definition )
          {
            ExternalDefinitionCreationOptions opt
              = new ExternalDefinitionCreationOptions(
                pname, def.Type );

            definition = group.Definitions.Create( opt );
          }

          doc.ParameterBindings.Insert( definition, binding,
            BuiltInParameterGroup.PG_GENERAL );
        }
        catch( Exception ex )
        {
          log.Add( string.Format(
            "Error: creating shared parameter '{0}': {1}",
            pname, ex.Message ) );
        }
      }

      // Restore original shared parameter file name 

      app.SharedParametersFilename = saveSharedParamsFileName;
    }
    #endregion // Create shared parameters

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Document doc = uidoc.Document;

      List<string> log = new List<string>();

      log.Add( string.Format(
        "\r\n\r\n{0:yyyy-MM-dd HH:mm:ss}: rvtmetaprop begin",
        DateTime.Now ) );

      int n;

      #region Select meta property input file

      string filename = "";

      if( !FileSelector.Select( ref filename ) )
      {
        return Result.Cancelled;
      }
      log.Add( "Input file selected: " + filename );

      #endregion // Select meta property input file

      #region Deserialise meta properties from input file

      List<MetaProp> props = null;

      if( filename.ToLower().EndsWith( ".json" ) )
      {
        string s = File.ReadAllText( filename );

        props = JsonConvert
          .DeserializeObject<List<MetaProp>>( s );
      }
      else if( filename.ToLower().EndsWith( ".csv" ) )
      {
        IEnumerable<IList<string>> a
          = EasyCsv.FromFile( filename, true );

        n = a.Count();
        props = new List<MetaProp>( n );
        foreach( IList<string> rec in a )
        {
          props.Add( new MetaProp( rec ) );
        }
      }
      else
      {
        message = "Unhandled meta property file format: "
          + Path.GetExtension( filename );
        return Result.Failed;
      }
      log.Add( props.Count + " meta properties deserialised" );

      #endregion // Deserialise meta properties from input file

      #region Remove 'Model' properties

      // Special 'Model' properties have extenalId prefix 'doc_'

      IEnumerable<MetaProp> doc_props
        = props.Where<MetaProp>( m
          => m.IsModelProperty );

      int nModelProp = doc_props.Count<MetaProp>();

      props.RemoveAll( m => m.IsModelProperty );

      log.Add( nModelProp.ToString()
        + " 'Model' properties removed with"
        + " extenalId prefix 'doc_'" );

      #endregion // Remove 'Model' properties

      #region Determine missing elements

      // Test that original elements are present in model

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

        s = string.Join( "\r\n  ",
          missing.Select<MetaProp, string>(
            m => m.component ) );

        string s2 = "\r\n\r\n" + nModelProp
          + " model properties ignored";

        d.MainContent = s + s2;
        d.Show();

        props.RemoveAll(
          m => null == doc.GetElement(
            m.externalId ) );

        log.Add( n
          + " properties on missing element removed:\r\n" + s );
      }
      #endregion // Determine missing elements

      #region Determine parameters to use

      // Determine what existing properties can be used;
      // for new ones, create dictionary mapping parameter 
      // name to shared parameter definition input data

      Dictionary<string, ParamDef> paramdefs
      = new Dictionary<string, ParamDef>();

      foreach( MetaProp m in props )
      {
        m.CanSet = false;

        string s = m.displayName;

        // Check for existing parameters

        Element e = doc.GetElement( m.externalId );
        IList<Parameter> a = e.GetParameters( m.displayName );

        n = a.Count;

        if( 0 < n )
        {
          // Property already exists on element

          if( 1 < n )
          {
            log.Add( string.Format(
              "Error: element <{0}> already has {1} parameters named '{2}'",
              m.component, n, m.displayName ) );
          }
          else
          {
            Parameter p = a[0];
            Definition pdef = p.Definition;

            //ExternalDefinition extdef = pdef as ExternalDefinition;
            //InternalDefinition intdef = pdef as InternalDefinition;
            //log.Add( string.Format( "extdef {0}, intdef {1}",
            //  null == extdef ? "<null>" : "ok",
            //  null == intdef ? "<null>" : "ok" ) );

            ParameterType ptyp = pdef.ParameterType;
            if( m.ParameterType != ptyp )
            {
              log.Add( string.Format(
                "Error: element <{0}> parameter '{1}' has type {2} != meta property type {3}",
                m.component, m.displayName, ptyp.ToString(), m.metaType ) );
            }
            else if( p.IsReadOnly )
            {
              log.Add( string.Format(
                "Error: element <{0}> parameter '{1}' is read-only",
                m.component, m.displayName ) );
            }
            else
            {
              m.CanSet = true;
            }
          }
        }
        else
        {
          // Property needs to be added to element

          if( !paramdefs.ContainsKey( s ) )
          {
            paramdefs.Add( s, new ParamDef( m ) );
          }

          ParamDef def = paramdefs[s];

          Category cat = e.Category;

          if( !def.Categories.Contains( cat ) )
          {
            def.Categories.Insert( cat );
          }
          m.CanSet = true;
        }
      }

      props.RemoveAll( m => !m.CanSet );

      #endregion // Determine parameters to use

      using( TransactionGroup tg = new TransactionGroup( doc ) )
      {
        tg.Start( "Import Forge Meta Properties" );

        #region Create required shared parameter bindings

        // Create required shared parameter bindings

        n = paramdefs.Count;

        if( 0 < n )
        {
          using( Transaction tx = new Transaction( doc ) )
          {
            tx.Start( "Create Shared Parameters" );
            CreateSharedParameters( doc, paramdefs, log );
            tx.Commit();
            log.Add( string.Format( 
              "Created {0} new shared parameter{1}", 
              n, (1 == n ? "" : "s") ) );
          }
        }
        #endregion // Create required shared parameter bindings

        #region Import Forge meta properties to parameter values

        // Set meta properties on elements

        using( Transaction tx = new Transaction( doc ) )
        {
          tx.Start( "Import Forge Meta Properties" );

          n = props.Count;

          log.Add( string.Format(
            "Setting {0} propert{1}:",
            n, ( 1 == n ? "y" : "ies" ) ) );

          foreach( MetaProp m in props )
          {
            log.Add( string.Format(
              "Set {0} property {1} = '{2}'", m.component,
              m.displayName, m.DisplayString ) );

            Element e = doc.GetElement( m.externalId );

            IList<Parameter> a = e.GetParameters( m.displayName );

            n = a.Count; // may be zero if shared param creation failed

            Debug.Assert( 0 == n || 1 == n, 
              "expected one single parameter of this "
              + "name; all others are skipped" );

            if( 0 < n )
            {
              m.SetValue( a[0] );
            }
          }
          tx.Commit();
        }
        #endregion // Import Forge meta properties to parameter values

        tg.Assimilate();
        //tg.Commit();
      }

      log.Add( string.Format(
        "{0:yyyy-MM-dd HH:mm:ss}: rvtmetaprop completed.\r\n",
        DateTime.Now ) );

      filename = Path.Combine( Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location ),
        "rvtmetaprop.log" );

      File.AppendAllText( filename,
        string.Join( "\r\n", log ) );

      Process.Start( filename );

      return Result.Succeeded;
    }
  }
}
