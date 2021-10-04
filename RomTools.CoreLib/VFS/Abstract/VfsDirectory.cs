namespace RomTools.VFS
{

  public abstract class VfsDirectory : VfsEntry
  {

    #region Properties

    public override VfsEntryType EntryType => VfsEntryType.Directory;

    #endregion

    #region Constructor

    protected VfsDirectory( VfsDevice device, string path, VfsEntry parent = null ) 
      : base( device, path, parent )
    {
    }

    #endregion

  }

}
