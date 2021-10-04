using System.IO;

namespace RomTools.VFS
{

  public abstract class VfsFile : VfsEntry
  {

    #region Properties

    public override VfsEntryType EntryType => VfsEntryType.File;

    public virtual long SizeInBytes { get; }

    #endregion

    #region Constructor

    protected VfsFile( VfsDevice device, string path, VfsEntry parent = null ) 
      : base( device, path, parent )
    {
    }

    #endregion

    #region Public Methods

    public abstract Stream OpenRead();

    #endregion

  }

}
