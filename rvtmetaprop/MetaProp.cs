using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB;

namespace rvtmetaprop
{
  class MetaProp
  {
    /// <summary>
    /// Predicate indicating we can set this property
    /// </summary>
    public bool CanSet { get; set; }

    public string externalId { get; set; }

    public string component { get; set; }

    public string displayCategory { get; set; }

    public string displayName { get; set; }

    public string displayValue { get; set; }

    /// <summary>
    /// metaType has one of the following values: 
    /// DeleteOverride, File, Link, Text, Int, Double
    /// </summary>
    public string metaType { get; set; }
    public string filelink { get; set; }

    public string filename { get; set; }

    public string link { get; set; }

    /// <summary>
    /// Set to true if doc.GetElement returns null
    /// </summary>
    //public bool missingInBim { get; set; }

    /// <summary>
    /// Default constructor for JSON deserialisation
    /// </summary>
    public MetaProp()
    {
      CanSet = false;
    }

    /// <summary>
    /// Read from a CSV file record containing the 
    /// following fields:
    /// "externalId","component","displayCategory","displayName","displayValue","metaType","filelink","filename","link"
    /// </summary>
    public MetaProp( IList<string> record )
    {
      CanSet = false;

      int n = record.Count;
      //Debug.Print( n.ToString() );
      if( 7 != n && 8 != n && 9 != n )
      {
        throw new ArgumentException(
          "Expected nine fields in CSV file record." );
      }
      externalId = record[0];
      component = record[1];
      displayCategory = record[2];
      displayName = record[3];
      displayValue = record[4];
      metaType = record[5];
      filelink = record[6];
      if( 8 == n ) { filename = record[7]; }
      if( 9 == n ) { link = record[8]; }
    }

    /// <summary>
    /// Predicate indicating this is a Forge model
    /// property with no corresponding BIM element.
    /// </summary>
    public bool IsModelProperty
    {
      get
      {
        return externalId.StartsWith( "doc_" );
      }
    }

    /// <summary>
    /// Predicate indicating this is a File or Link
    /// property not supported in Revit.
    /// </summary>
    public bool IsFileOrLinkProperty
    {
      get
      {
        return metaType.Equals( "File" )
          || metaType.Equals( "Link" );
      }
    }

    /// <summary>
    /// Return the appropriate Revit parameter type to
    /// create a shared parameter.
    /// </summary>
    public ParameterType ParameterType
    {
      get
      {
        // metaType has one of the following values: 
        // DeleteOverride, File, Link, Text

        if( metaType.Equals( "Text" ) )
        {
          return ParameterType.Text;
        }
        if( metaType.Equals( "Int" ) )
        {
          return ParameterType.Integer;
        }
        if( metaType.Equals( "Double" ) )
        {
          return ParameterType.Number;
        }
        if( metaType.Equals( "Link" ) )
        {
          return ParameterType.Text;
        }
        if( metaType.Equals( "File" ) )
        {
          return ParameterType.Text;
        }
        if( metaType.Equals( "DeleteOverride" ) )
        {
          return ParameterType.Invalid;
        }
        Debug.Assert( false, "unexpected metaType" );
        return ParameterType.Invalid;
      }
    }

    /// <summary>
    /// Return the appropriate value to populate
    /// a Revit shared parameter.
    /// </summary>
    public string DisplayString
    {
      get
      {
        if( metaType.Equals( "Text" )
          || metaType.Equals( "Int" )
          || metaType.Equals( "Double" ) )
        {
          return displayValue;
        }
        if( metaType.Equals( "Link" ) )
        {
          return "link:" + displayValue + ":" + link;
        }
        if( metaType.Equals( "File" ) )
        {
          return "file:" + displayValue + ":" + filelink + ":" + filename;
        }
        if( metaType.Equals( "DeleteOverride" ) )
        {
          return "<delete>";
        }
        Debug.Assert( false, "unexpected metaType" );
        return string.Empty;
      }
    }

    /// <summary>
    /// Populate a Revit shared parameter value.
    /// </summary>
    public void SetValue( Parameter p )
    {
      StorageType st = p.StorageType;

      if( metaType.Equals( "DeleteOverride" ) )
      {
        switch( st )
        {
          case StorageType.Double: p.Set( 0.0 ); break;
          case StorageType.ElementId: p.Set( ElementId.InvalidElementId ); break;
          case StorageType.Integer: p.Set( 0 ); break;
          case StorageType.String: p.Set( string.Empty ); break;
          default: Debug.Assert( false, "cannot set this storage type:" + st.ToString() ); break;
        }
      }
      else if( metaType.Equals( "Text" )
        || metaType.Equals( "Link" )
        || metaType.Equals( "File" ) )
      {
        p.Set( DisplayString );
      }
      else if( metaType.Equals( "Int" ) )
      {
        int i;
        if( int.TryParse( displayValue, out i ) )
        {
          p.Set( i );
        }
        else
        {
          Debug.Assert( false,
            "invalid int property value "
            + displayValue );
        }
      }
      else if( metaType.Equals( "Double" ) )
      {
        double d;
        if( double.TryParse( displayValue, out d ) )
        {
          p.Set( d );
        }
        else
        {
          Debug.Assert( false,
            "invalid double property value "
            + displayValue );
        }
      }
      else
      {
        Debug.Assert( false, "unexpected metaType " + metaType );
      }
    }
  }
}
