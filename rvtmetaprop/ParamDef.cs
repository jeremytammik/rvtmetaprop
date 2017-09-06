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

    public ParamDef( MetaProp m )
    {
      Type = m.ParameterType;
      Categories = new CategorySet(); // List<ElementId>();
      GroupName = m.displayCategory;
    }


    /// <summary>
    /// Return the built-in parameter group enum value 
    /// corresponding to the property group name.
    /// </summary>
    public BuiltInParameterGroup BipGroup
    {
      get
      {
        return (BuiltInParameterGroup) Enum.Parse( 
          typeof( BuiltInParameterGroup ), GroupName );
      }
    }

  }
}
