using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "rvtmetaprop" )]
[assembly: AssemblyDescription( "Revit C# .NET add-in to import and store meta properties created in Forge" )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "Autodesk Inc." )]
[assembly: AssemblyProduct( "rvtmetaprop Revit C# .NET Add-In" )]
[assembly: AssemblyCopyright( "Copyright 2017 (C) Jeremy Tammik, Autodesk Inc." )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "321044f7-b0b2-4b1c-af18-e71a19252be0" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
//
// History:
//
// 2018.0.0.0 still working on skeleton
// 2018.0.0.1 bim element access, csv and json deserialisation works
// 2018.0.0.2 added int and double property types, parameter definition data and simple logging class
// 2018.0.0.3 implemented my own message logging
// 2018.0.0.4 implemented transaction group and separate transaction to set existing properties
// 2018.0.0.5 implemented FileSelector and CreateSharedParameters
// 2018.0.0.6 implemented setting of properties
// 2018.0.0.7 successful first test completed
// 2018.0.0.8 added check for read-only parameter
// 2018.0.0.9 cleaned up logging
// 2018.0.0.10 ignore file and link properties
// 2018.0.0.11 skip shared param creation for delete request
// 2018.0.0.12 added urban house sample model and first test on http://meta-editor.autodesk.link/
// 2017-09-04 2018.0.0.13 added support for shared param group name
// 2017-09-06 2018.0.0.14 implemented MetaProp.categoryId and ParamDef.BipGroup
// 2017-09-11 2018.0.0.15 added MetaProp.categoryId support parsing CSV
// 2017-09-11 2018.0.0.16 implemented MetaProp.BipGroup
//
[assembly: AssemblyVersion( "2018.0.0.16" )]
[assembly: AssemblyFileVersion( "2018.0.0.16" )]
