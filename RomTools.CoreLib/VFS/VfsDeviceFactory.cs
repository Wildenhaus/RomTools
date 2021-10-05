using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RomTools.Data;

namespace RomTools.VFS
{

  public static class VfsDeviceFactory
  {

    #region Data Members

    private static readonly Dictionary<BytePattern, HashSet<Type>> _vfsDeviceTypeRegistry;

    #endregion

    #region Constructor

    static VfsDeviceFactory()
    {
      _vfsDeviceTypeRegistry = new Dictionary<BytePattern, HashSet<Type>>();
    }

    #endregion

    #region Public Methods

    public static async Task<Result<VfsDevice>> CreateVfsDevice( Stream fileStream, BytePattern bytePattern )
    {
      if ( !_vfsDeviceTypeRegistry.TryGetValue( bytePattern, out var deviceTypes ) )
        return Result.Failure<VfsDevice>( $"Could not find a VfsDevice for byte pattern [{bytePattern.PatternText}]." );

      foreach( var deviceType in deviceTypes )
      {
        fileStream.Seek( 0, SeekOrigin.Begin );
        var deviceResult = await TryCreateVfsDeviceInstance( fileStream, deviceType );
        if ( deviceResult.Success )
          return deviceResult;
      }

      return Result.Failure<VfsDevice>( "Failed to createa a device." );
    }

    public static Result RegisterDevice<TDevice>( params BytePattern[] bytePatterns )
      where TDevice : VfsDevice
      => RegisterDevice( typeof( TDevice ), bytePatterns );

    public static Result RegisterDevice( Type deviceType, params BytePattern[] bytePatterns )
    {
      if( !typeof( VfsDevice ).IsAssignableFrom( deviceType ) )
        return Result.Failure( $"Type `{deviceType.FullName}` is not derived from VfsDevice and cannot be registered." );
      if ( deviceType.IsAbstract )
        return Result.Failure( $"VFS Device Type `{deviceType.FullName}` is abstract and cannot be registered." );

      foreach( var bytePattern in bytePatterns )
      {
        if(!_vfsDeviceTypeRegistry.TryGetValue( bytePattern, out var registeredDevices ) )
        {
          registeredDevices = new HashSet<Type>();
          _vfsDeviceTypeRegistry.Add( bytePattern, registeredDevices );
        }

        registeredDevices.Add( deviceType );
      }

      return Result.Successful( $"Registered VFS Device `{deviceType.Name}` to {bytePatterns.Length} byte patterns." );
    }

    #endregion

    #region Private Methods

    private static async Task<Result<VfsDevice>> TryCreateVfsDeviceInstance( Stream fileStream, Type deviceType )
    {
      try
      {
        var deviceInstance = ( VfsDevice ) Activator.CreateInstance( deviceType, new[] { fileStream } );
        await deviceInstance.Initialize();

        return Result.Successful( deviceInstance );
      }
      catch (Exception ex )
      {
        return Result.Failure<VfsDevice>( ex, "Failed to create device." );
      }
    }

    #endregion

  }

}
