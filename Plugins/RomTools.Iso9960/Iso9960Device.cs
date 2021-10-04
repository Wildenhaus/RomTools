using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DiscUtils;
using DiscUtils.Iso9660;
using RomTools.VFS;

namespace RomTools.Iso9960
{

  public class Iso9960Device : VfsDevice
  {

    #region Data Members

    private string _isoFilePath;
    private Stream _isoFileStream;
    private CDReader _isoReader;

    #endregion

    #region Constructor

    public Iso9960Device( string isoFilePath )
    {
      _isoFilePath = isoFilePath;
    }

    public Iso9960Device( Stream isoFileStream )
    {
      _isoFileStream = isoFileStream;
    }

    #endregion

    #region Overrides

    protected override void OnDisposing( bool disposing )
    {
      _isoFilePath = null;

      _isoReader.Dispose();
      _isoReader = null;

      _isoFileStream.Dispose();
      _isoFileStream = null;

      base.OnDisposing( disposing );
    }

    protected override async Task<Result> OnInitializing()
    {
      if( !string.IsNullOrWhiteSpace( _isoFilePath ) )
      {
        if ( !File.Exists( _isoFilePath ) )
          return Result.Failure( $"File does not exist: {_isoFilePath}" );

        _isoFileStream = File.OpenRead( _isoFilePath );
      }

      _isoReader = new CDReader( _isoFileStream, true );
      return Result.Successful();
    }

    protected override async Task<Result<VfsDirectory>> OnBuildFileTree()
    {
      var root = new Iso9960Directory( this, _isoReader.Root );
      AddChildEntriesRecursive( root );

      return Result<VfsDirectory>.Successful( root );
    }

    #endregion

    #region Private Methods

    private void AddChildEntriesRecursive( Iso9960Directory currentDirectory )
    {
      foreach ( var childDirectory in currentDirectory.NativeDirectory.GetDirectories() )
      {
        var childEntry = new Iso9960Directory( this, childDirectory, currentDirectory );
        AddChildEntriesRecursive( childEntry );
      }

      foreach ( var childFile in currentDirectory.NativeDirectory.GetFiles() )
        new Iso9960File( this, childFile, currentDirectory );
    }

    #endregion

    #region Child Classes

    internal class Iso9960Directory : VfsDirectory
    {

      #region Data Members

      private DiscDirectoryInfo _directory;

      #endregion

      #region Properties

      internal DiscDirectoryInfo NativeDirectory => _directory;

      public override FileAttributes Attributes => _directory.Attributes;
      public override DateTime? DateAccessed => _directory.LastAccessTime;
      public override DateTime? DateCreated => _directory.CreationTime;
      public override DateTime? DateModified => _directory.LastWriteTime;

      #endregion

      #region Constructor

      internal Iso9960Directory( Iso9960Device device, DiscDirectoryInfo directory, VfsEntry parent = null )
        : base( device, directory.FullName, parent )
      {
        _directory = directory;
      }

      #endregion

    }

    internal class Iso9960File : VfsFile
    {

      #region Data Members

      private DiscFileInfo _file;

      #endregion

      #region Properties

      public override FileAttributes Attributes => _file.Attributes;
      public override DateTime? DateAccessed => _file.LastAccessTime;
      public override DateTime? DateCreated => _file.CreationTime;
      public override DateTime? DateModified => _file.LastWriteTime;
      public override long SizeInBytes => _file.Length;

      #endregion

      #region Constructor

      internal Iso9960File( Iso9960Device device, DiscFileInfo file, VfsEntry parent = null )
        : base( device, SanitizeFileName( file.FullName ), parent )
      {
        _file = file;
      }

      #endregion

      #region Overrides

      public override Stream OpenRead()
        => _file.OpenRead();

      #endregion

      #region Private Methods

      private static string SanitizeFileName( string fileName )
      {
        // DiscUtils is appending ';1' to file names
        var smcIndex = fileName.IndexOf( ';' );
        if ( smcIndex != -1 )
          fileName = fileName.Substring( 0, smcIndex );

        return fileName;
      }

      #endregion

    }

    #endregion

  }

}
