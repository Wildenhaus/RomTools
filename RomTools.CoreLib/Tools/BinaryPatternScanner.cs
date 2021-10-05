using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RomTools.Data;

namespace RomTools.Tools
{

  public static class BinaryPatternScanner
  {

    #region Constants

    public const int NOT_FOUND = -1;
    private const int BUFFER_SIZE = 1024 * 1024;

    #endregion

    #region Public Methods

    public static BytePatternMatch[] FindAllMatches( Stream stream, params BytePattern[] patterns )
    {
      // Save the current location. If the stream is seekable, we're going to scan from the 
      // beginning and restore the state afterwards.
      var initialStreamPosition = stream.Position;
      if ( stream.CanSeek )
        stream.Seek( 0, SeekOrigin.Begin );

      var matches = new ConcurrentBag<BytePatternMatch>();
      var buffer = ArrayPool<byte>.Shared.Rent( BUFFER_SIZE );

      while(stream.Position < stream.Length )
      {
        var bytesRead = stream.Read( buffer );
        if ( bytesRead <= 0 )
          break;

        Parallel.ForEach( patterns, patterns =>
        {
          // Would be better if we could allocate this outside of the Parallel block, but we can't :(
          var readSpan = buffer.AsSpan( 0, bytesRead ); 

          var matchOffset = Match( patterns, readSpan );
          if ( matchOffset != NOT_FOUND )
            matches.Add( new BytePatternMatch( patterns, matchOffset ) );
        } );
      }

      ArrayPool<byte>.Shared.Return( buffer );
      return matches.ToArray();
    }

    #endregion

    #region Private Methods

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int Match( BytePattern bytePattern, Span<byte> buffer, int offset = 0 )
    {
      var pattern = bytePattern.Pattern.AsSpan();
      var patternSize = pattern.Length;

      for ( int i = offset, pos = 0; i < buffer.Length; i++ )
      {
        if ( MatchByte( ref buffer[ i ], ref pattern[ pos ] ) )
        {
          pos++;
          if ( pos == patternSize )
            return i - patternSize + 1;
        }
        else
        {
          i -= pos;
          pos = 0;
        }
      }

      return NOT_FOUND;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool MatchByte( ref byte bufferByte, ref BytePattern.Byte patternByte )
    {
      var nibbleA = patternByte.A;
      if ( !nibbleA.IsMasked )
      {
        var a = bufferByte >> 4;
        if ( a != nibbleA.Value )
          return false;
      }

      var nibbleB = patternByte.B;
      if ( !nibbleB.IsMasked )
      {
        var b = bufferByte & 0xF;
        if ( b != nibbleB.Value )
          return false;
      }

      return true;
    }

    #endregion

  }

}
