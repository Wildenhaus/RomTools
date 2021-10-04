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

    private static readonly Dictionary<ByteSignature, HashSet<Type>> _vfsDeviceTypeRegistry;

    #endregion

    #region Constructor

    static VfsDeviceFactory()
    {
      _vfsDeviceTypeRegistry = new Dictionary<ByteSignature, HashSet<Type>>();
    }

    #endregion

    #region Public Methods

    public static async Task<Result<VfsDevice>> CreateVfsDevice( Stream fileStream, ByteSignature byteSignature )
    {
      if ( !_vfsDeviceTypeRegistry.TryGetValue( byteSignature, out var deviceTypes ) )
        return Result.Failure<VfsDevice>( $"Could not find a VfsDevice for byte signature [{byteSignature.PatternText}]." );

      foreach( var deviceType in deviceTypes )
      {
        fileStream.Seek( 0, SeekOrigin.Begin );
        var deviceResult = await TryCreateVfsDeviceInstance( fileStream, deviceType );
        if ( deviceResult.Success )
          return deviceResult;
      }

      return Result.Failure<VfsDevice>( "Failed to createa a device." );
    }

    public static Result RegisterDevice<TDevice>( params ByteSignature[] byteSignatures )
      where TDevice : VfsDevice
      => RegisterDevice( typeof( TDevice ), byteSignatures );

    public static Result RegisterDevice( Type deviceType, params ByteSignature[] byteSignatures )
    {
      if( !typeof( VfsDevice ).IsAssignableFrom( deviceType ) )
        return Result.Failure( $"Type `{deviceType.FullName}` is not derived from VfsDevice and cannot be registered." );
      if ( deviceType.IsAbstract )
        return Result.Failure( $"VFS Device Type `{deviceType.FullName}` is abstract and cannot be registered." );

      foreach( var byteSignature in byteSignatures )
      {
        if(!_vfsDeviceTypeRegistry.TryGetValue( byteSignature, out var registeredDevices ) )
        {
          registeredDevices = new HashSet<Type>();
          _vfsDeviceTypeRegistry.Add( byteSignature, registeredDevices );
        }

        registeredDevices.Add( deviceType );
      }

      return Result.Successful( $"Registered VFS Device `{deviceType.Name}` to {byteSignatures.Length} byte signatures." );
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
