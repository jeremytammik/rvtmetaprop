using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace rvtmetaprop
{
  class ParamDef
  {
    public ParameterType Type { get; set; }
    public List<ElementId> Categories { get; set; }
    public string GroupName { get; set; }

    public ParamDef( MetaProp m )
    {
      Type = m.ParameterType;
      Categories = new List<ElementId>();
      GroupName = m.displayCategory;
    }
  }
}
