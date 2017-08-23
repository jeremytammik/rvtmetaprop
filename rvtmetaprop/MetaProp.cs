using System;
using System.Collections.Generic;

namespace rvtmetaprop
{
  class MetaProp
  {
    public string displayCategory { get; set; }
    public string displayValue { get; set; }
    public string displayName { get; set; }
    public string externalId { get; set; }
    public string component { get; set; }
    public string metaType { get; set; }
    public MetaProp( IList<string> record )
    {
      if( 6 != record.Count )
      {
        throw new ArgumentException(
          "Expected six fileds in record." );
      }
      displayCategory = record[0];
      displayValue = record[0];
      displayName = record[0];
      externalId = record[0];
      component = record[0];
      metaType = record[0];
    }
  }
}
