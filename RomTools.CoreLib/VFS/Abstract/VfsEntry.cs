using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RomTools.VFS
{

  public abstract class VfsEntry
  {

    #region Data Members

    protected string _path;

    protected VfsDevice _device;
    protected VfsEntry _parent;
    protected Dictionary<string, VfsEntry> _children;

    #endregion

    #region Properties

    public string FullName => _path;
    public string Name
    {
      get => Path.GetFileName( _path );
    }

    public VfsDevice Device => _device;
    public VfsEntry Parent => _parent;
    public IEnumerable<VfsEntry> Children => _children.Values;

    public virtual FileAttributes Attributes { get; protected set; }
    public virtual DateTime? DateAccessed { get; protected set; }
    public virtual DateTime? DateCreated { get; protected set; }
    public virtual DateTime? DateModified { get; protected set; }

    public abstract VfsEntryType EntryType { get; }

    #endregion

    #region Constructor

    protected VfsEntry( VfsDevice device, string path, VfsEntry parent = null )
    {
      _device = device;
      _parent = parent;
      _children = new Dictionary<string, VfsEntry>();

      _path = path;
    }

    #endregion

    #region Public Methods

    public bool AddChild( VfsEntry childEntry )
      => _children.TryAdd( childEntry.Name, childEntry );

    public IEnumerable<VfsEntry> EnumerateChildren( bool recursive = false )
    {
      var results = Children;

      if ( recursive )
        results = results.Concat( Children.SelectManyRecursive( x => x.Children ) );

      return results;
    }

    public IEnumerable<VfsDirectory> EnumerateDirectories( bool recursive = false )
      => EnumerateChildren( recursive ).Where( x => x is VfsDirectory ).Cast<VfsDirectory>();

    public IEnumerable<VfsFile> EnumerateFiles( bool recursive = false )
      => EnumerateChildren( recursive ).Where( x => x is VfsFile ).Cast<VfsFile>();

    #endregion

  }

}
