using RomTools.Data;
using RomTools.VFS;

namespace RomTools.Plugins
{

  public abstract class Plugin
  {

    #region Properties

    public abstract string PluginName { get; }
    public abstract string PluginVersion { get; }
    public abstract string PluginAuthor { get; }

    public virtual string PluginDescription { get; }

    #endregion

    #region Public Methods

    public void Initialize()
    {
      OnInitializing();
    }

    #endregion

    #region Abstract Methods

    protected abstract void OnInitializing();

    #endregion

    #region Private Methods

    protected void RegisterBytePattern( BytePattern bytePattern )
    {
    }

    protected void RegisterVfsDevice<T>( params BytePattern[] applicableBytePatterns )
      where T : VfsDevice
    {
    }

    #endregion

  }

}