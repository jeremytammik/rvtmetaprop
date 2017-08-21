#region Namespaces
using System.Collections.Generic;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.IO;
#endregion

namespace rvtmetaprop
{
  class App : IExternalApplication
  {
    public const string Caption = "Meta Properties";

    /// <summary>
    /// Our one and only split button
    /// holding all the commands.
    /// </summary>
    SplitButton _split_button = null;

    /// <summary>
    /// This external application 
    /// singleton class instance.
    /// </summary>
    internal static App _app = null;

    /// <summary>
    /// Provide access to this class instance.
    /// </summary>
    public static App Instance
    {
      get { return _app; }
    }

    /// <summary>
    /// Return the full add-in assembly folder path.
    /// </summary>
    public static string Path
    {
      get
      {
        return System.IO.Path.GetDirectoryName(
          Assembly.GetExecutingAssembly().Location );
      }
    }

    #region Create Ribbon Tab
    /// <summary>
    /// Load a new icon bitmap from embedded resources.
    /// For the BitmapImage, make sure you reference 
    /// WindowsBase and PresentationCore, and import 
    /// the System.Windows.Media.Imaging namespace. 
    /// </summary>
    BitmapImage NewBitmapImage(
      System.Reflection.Assembly a,
      string imageName )
    {
      Stream s = a.GetManifestResourceStream( imageName );
      BitmapImage img = new BitmapImage();
      img.BeginInit();
      img.StreamSource = s;
      img.EndInit();
      return img;
    }

    void CreateRibbonTab(
      UIControlledApplication a )
    {
      Assembly assembly = Assembly.GetExecutingAssembly();

      string ass_path = assembly.Location;
      string ass_name = assembly.GetName().Name;

      // Create ribbon tab 

      string tab_name = Caption;

      try
      {
        a.CreateRibbonTab( tab_name );
      }
      catch( Autodesk.Revit.Exceptions.ArgumentException )
      {
        // Assume error is due to tab already existing
      }

      PushButtonData pbCommand = new PushButtonData(
        "Import", "Import", ass_path,
        ass_name + ".Command" );

      PushButtonData pbCommandOpt = new PushButtonData(
        "Settings", "Settings", ass_path,
        ass_name + ".CmdSettings" );

      pbCommand.LargeImage = NewBitmapImage( assembly,
        "rvtmetaprop.iCommand.png" );

      pbCommandOpt.LargeImage = NewBitmapImage( assembly,
        "rvtmetaprop.iCmdSettings.png" );

      // Add button tips (when data, must be 
      // defined prior to adding button.)

      pbCommand.ToolTip = "Import meta properties.";

      pbCommand.LongDescription = "Import meta properties"
        + " modified or added in the Forge configurator"
        + " sample.";

      //   Add new ribbon panel. 

      string panel_name = Caption;

      RibbonPanel new_panel = a.CreateRibbonPanel(
        tab_name, panel_name );

      // add button to ribbon panel

      SplitButtonData split_buttonData
        = new SplitButtonData(
          "splitButton", "metaprop" );

      _split_button = new_panel.AddItem(
        split_buttonData ) as SplitButton;

      _split_button.AddPushButton( pbCommand );
      _split_button.AddPushButton( pbCommandOpt );
    }

    /// <summary>
    /// Reset the top button to be the current one.
    /// Alternative solution: 
    /// set RibbonItem.IsSynchronizedWithCurrentItem 
    /// to false after creating the SplitButton.
    /// </summary>
    public void SetTopButtonCurrent()
    {
      IList<PushButton> sbList = _split_button.GetItems();
      _split_button.CurrentButton = sbList[0];
    }
    #endregion // Create Ribbon Tab

    public Result OnStartup( UIControlledApplication a )
    {
      _app = this;
      CreateRibbonTab( a );
      return Result.Succeeded;
    }

    public Result OnShutdown( UIControlledApplication a )
    {
      return Result.Succeeded;
    }
  }
}
