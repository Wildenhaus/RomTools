using System;
using System.IO;
using System.Threading.Tasks;

namespace RomTools.VFS
{

  public class HostFileSystemDevice : VfsDevice
  {

    #region Data Members

    protected DirectoryInfo _rootDirectoryInfo;

    #endregion

    #region Constructor

    public HostFileSystemDevice( string path )
    {
      _rootDirectoryInfo = new DirectoryInfo( path );
    }

    #endregion

    #region Overrides

    protected override void OnDisposing( bool disposing )
    {
      _rootDirectoryInfo = null;
      base.OnDisposing( disposing );
    }

    protected override async Task<Result<VfsDirectory>> OnBuildFileTree()
    {
      var root = new HostFileSystemDirectory( this, _rootDirectoryInfo );
      AddChildEntriesRecursive( root );

      return Result.Successful<VfsDirectory>( root );
    }

    #endregion

    #region Private Methods

    private void AddChildEntriesRecursive( VfsDirectory currentDirectory )
    {
      foreach( var childDirectory in Directory.EnumerateDirectories( currentDirectory.FullName ) )
      {
        var childEntry = new HostFileSystemDirectory( this, new DirectoryInfo( childDirectory ), currentDirectory );
        AddChildEntriesRecursive( childEntry );
      }

      foreach( var childFile in Directory.EnumerateFiles( currentDirectory.FullName ) )
        new HostFileSystemFile( this, new FileInfo( childFile ), currentDirectory );
    }

    #endregion

    #region Child Classes

    internal class HostFileSystemDirectory : VfsDirectory
    {

      #region Data Members

      private DirectoryInfo _directory;

      #endregion

      #region Properties

      public override FileAttributes Attributes => _directory.Attributes;
      public override DateTime? DateAccessed => _directory.LastAccessTime;
      public override DateTime? DateCreated => _directory.CreationTime;
      public override DateTime? DateModified => _directory.LastWriteTime;

      #endregion

      #region Constructor

      internal HostFileSystemDirectory( HostFileSystemDevice device, DirectoryInfo directory, VfsEntry parent = null )
        : base( device, directory.FullName, parent )
      {
        _directory = directory;
      }

      #endregion

    }

    internal class HostFileSystemFile : VfsFile
    {

      #region Data Members

      private FileInfo _file;

      #endregion

      #region Properties

      public override FileAttributes Attributes => _file.Attributes;
      public override DateTime? DateAccessed => _file.LastAccessTime;
      public override DateTime? DateCreated => _file.CreationTime;
      public override DateTime? DateModified => _file.LastWriteTime;

      #endregion

      #region Constructor

      internal HostFileSystemFile( HostFileSystemDevice device, FileInfo file, VfsEntry parent )
        : base( device, file.FullName, parent )
      {
        _file = file;
      }

      #endregion

      #region Overrides

      public override Stream OpenRead()
        => _file.OpenRead();

      #endregion

    }

    #endregion

  }

}
