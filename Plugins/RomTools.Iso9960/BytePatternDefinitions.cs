using RomTools.Data;

namespace RomTools.Iso9960
{

  public static class BytePatternDefinitions
  {

    public static BytePattern Iso9960Magic = BytePattern.Define( 
      type: BytePatternType.Magic, 
      pattern: "43 44 30 30 31", // CD001 
      description: "The file header magic number for ISO-9960 images." );

  }

}
