namespace RomTools.Data
{

  public enum BytePatternType : byte
  {

    /// <summary>
    ///   Pattern is found at the beginning of the file as magic number.
    /// </summary>
    Magic = 0x00,

    /// <summary>
    ///   Pattern is found within the file's body content.
    /// </summary>
    Content = 0xFF

  }

}
