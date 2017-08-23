using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace rvtmetaprop
{
  class MetaProp
  {
    public string externalId { get; set; }
    public string component { get; set; }
    public string displayCategory { get; set; }
    public string displayName { get; set; }
    public string displayValue { get; set; }
    public string metaType { get; set; }
    public string filelink { get; set; }
    public string filename { get; set; }
    public string link { get; set; }

    /// <summary>
    /// Default constructor for JSON deserialisation
    /// </summary>
    public MetaProp()
    {
    }
    
    /// <summary>
    /// Read from a CSV file record containing the 
    /// following fields:
    /// "externalId","component","displayCategory","displayName","displayValue","metaType","filelink","filename","link"
    /// </summary>
    public MetaProp( IList<string> record )
    {
      int n = record.Count;
      //Debug.Print( n.ToString() );
      if( 8 != n && 9 != n )
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
      filename = record[7];
      if( 9 == n )
      {
        link = record[8];
      }
    }

    public bool IsModelProperty
    {
      get
      {
        return externalId.StartsWith( "doc_" );
      }
    }

    public string ParameterValue
    {
      get
      {
        // metaType has one of the following values: 
        // DeleteOverride, File, Link, Text

        if( metaType.Equals( "Text" ) )
        {
          return displayValue;
        }
        if( metaType.Equals("Link" ) )
        {
          return "link:" + displayValue + ":" + link;
        }
        if( metaType.Equals( "File" ) )
        {
          return "file:" + displayValue + ":" + filelink + ":" + filename;
        }
        if( metaType.Equals( "DeleteOverride" ) )
        {
          return string.Empty;
        }
        Debug.Assert( false, "unexpected metaType" );
        return string.Empty;
      }
    }
  }
}
