using System;
using RomTools.Plugins;

namespace RomTools.Iso9960
{

  public class Iso9960Plugin : Plugin
  {

    #region Properties

    public override string PluginName => "ISO-9960";
    public override string PluginVersion => "v0.1a";
    public override string PluginAuthor => "Haus";
    public override string PluginDescription => "Adds support for reading .iso files that follow the ISO-9960 standard.";

    #endregion

    #region Overrides

    protected override void OnInitializing()
    {
      RegisterBytePattern( BytePatternDefinitions.Iso9960Magic );
      RegisterVfsDevice<Iso9960Device>( BytePatternDefinitions.Iso9960Magic );
    }

    #endregion

  }

}
