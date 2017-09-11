using Autodesk.Revit.DB;
using System;

namespace rvtmetaprop
{
  class ParamDef
  {
    public ParameterType Type { get; set; }
    //public List<ElementId> Categories { get; set; }
    public CategorySet Categories { get; set; }
    public string GroupName { get; set; }
    public BuiltInParameterGroup BipGroup { get; set; }

    public ParamDef( MetaProp m )
    {
      Type = m.ParameterType;
      Categories = new CategorySet(); // List<ElementId>();
      GroupName = m.displayCategory;
      BipGroup = m.BipGroup;
    }
  }
}
