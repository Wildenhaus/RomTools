using System;
using System.Collections.Generic;
using System.Text;

namespace RomTools.Data
{
  
  public readonly struct BytePatternMatch
  {

    #region Data Members

    public readonly BytePattern SearchPattern;
    public readonly long Offset;

    #endregion

    #region Constructor

    public BytePatternMatch( BytePattern searchPattern, long offset )
    {
      SearchPattern = searchPattern;
      Offset = offset;
    }

    #endregion

  }

}
