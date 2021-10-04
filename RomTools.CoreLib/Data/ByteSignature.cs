using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace RomTools.Data
{

  public readonly struct ByteSignature : IEquatable<ByteSignature>
  {

    #region Data Members

    public readonly ByteSignatureType Type;
    public readonly ByteSignature.Byte[] Pattern;
    public readonly string PatternText;
    public readonly string Description;

    private readonly Guid _md5Hash; // The MD5 hash of the pattern, stored as a Guid

    #endregion

    #region Properties

    public int Length
    {
      [MethodImpl( MethodImplOptions.AggressiveInlining )]
      get => Pattern.Length;
    }

    #endregion

    #region Constructor

    private ByteSignature( ByteSignatureType type, string pattern, string description = null )
    {
      pattern = SanitizePattern( pattern );

      Type = type;
      PatternText = pattern;
      Pattern = ParsePattern( pattern );
      Description = description;

      _md5Hash = ComputeHash( pattern );
    }

    public static ByteSignature Define( ByteSignatureType type, string pattern, string description = null )
      => new ByteSignature( type, pattern, description );

    #endregion

    #region Private Methods

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Guid ComputeHash( string pattern )
    {
      using ( var md5 = MD5.Create() )
      {
        var textBytes = Encoding.ASCII.GetBytes( pattern );
        var md5Bytes = md5.ComputeHash( textBytes );
        return new Guid( md5Bytes );
      }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int HexCharToInt( char c )
    {
      if ( char.IsDigit( c ) )
        return c - '0';
      else if ( c >= 'A' && c <= 'F' )
        return c - 'A' + 10;

      ThrowInvalidPatternTokenException( c );
      return -1;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ByteSignature.Byte[] ParsePattern( string pattern )
    {
      // Sanitize
      var span = pattern.AsSpan();

      var len = pattern.Length;
      var bytes = new List<ByteSignature.Byte>( len / 2 );

      for ( var i = 0; i < len; i += 2 )
      {
        var nibbleA = ParseNibble( span[ i + 0 ] );
        var nibbleB = ParseNibble( span[ i + 1 ] );

        bytes.Add( new Byte( nibbleA, nibbleB ) );
      }

      return bytes.ToArray();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ByteSignature.Nibble ParseNibble( char c )
    {
      if ( c == '?' )
        return Nibble.Masked;
      else
        return new Nibble( ( byte ) ( HexCharToInt( c ) & 0xF ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string SanitizePattern( string pattern )
    {
      var builder = new StringBuilder( pattern.Length );
      foreach ( var c in pattern )
      {
        if ( char.IsDigit( c ) || ( c >= 'A' && c <= 'F' ) )
          builder.Append( c );
        else if ( c >= 'a' && c <= 'f' )
          builder.Append( char.ToUpper( c ) );
        else if ( c == '?' )
          builder.Append( '?' );
        else if ( c == ' ' )
          continue;
        else
          ThrowInvalidPatternTokenException( c );
      }

      // If we end on an incomplete byte, add a wildcard
      if ( builder.Length % 2 != 0 )
        builder.Append( '?' );

      return builder.ToString();
    }

    private static void ThrowInvalidPatternTokenException( char token )
      => throw new Exception( $"Invalid token encountered when parsing signature: '{token}'" ); // TODO: Make this an actual exception type

    #endregion

    #region IEquatable Methods

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals( ByteSignature other )
      =>  _md5Hash == other._md5Hash 
      && PatternText.Equals( other.PatternText, StringComparison.InvariantCultureIgnoreCase );

    #endregion

    #region Overrides

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override bool Equals( object obj )
      => obj is ByteSignature other && Equals( other );

    public override int GetHashCode()
      => _md5Hash.GetHashCode();

    #endregion

    #region Child Structures

    [StructLayout( LayoutKind.Explicit, Size = 4 )]
    public readonly struct Byte
    {

      #region Data Members

      [FieldOffset( 0x0 )] public readonly Nibble A;
      [FieldOffset( 0x2 )] public readonly Nibble B;

      #endregion

      #region Constructor

      internal Byte( Nibble a, Nibble b )
      {
        A = a;
        B = b;
      }

      #endregion

    }

    [StructLayout( LayoutKind.Explicit, Size = 2 )]
    public readonly struct Nibble
    {

      #region Constants

      public static readonly Nibble Masked = new Nibble( 0, true );

      #endregion

      #region Data Members

      [FieldOffset( 0 )] public readonly bool IsMasked;
      [FieldOffset( 1 )] public readonly byte Value;

      #endregion

      #region Constructor

      internal Nibble( byte value, bool isMasked = false )
      {
        Value = value;
        IsMasked = isMasked;
      }

      #endregion

    }

    #endregion

  }

}
