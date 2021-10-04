using System;
using System.Threading.Tasks;

namespace RomTools.VFS
{

  public abstract class VfsDevice : IDisposable
  {

    #region Data Members

    protected VfsDirectory _root;

    private bool _isDisposed;
    private bool _isInitialized;

    #endregion

    #region Properties

    public bool IsDisposed => _isDisposed;
    public bool IsInitialized => _isInitialized;

    #endregion

    #region Public Methods

    public async Task<Result> Initialize()
    {
      if ( _isInitialized )
        return Result.Successful();

      try
      {
        var initResult = await OnInitializing();
        if ( !initResult.Success )
          return initResult;

        var buildFileTreeResult = await OnBuildFileTree();
        if ( !buildFileTreeResult.Success )
          return buildFileTreeResult;
        else
          _root = buildFileTreeResult.Value;

        _isInitialized = true;
        return Result.Successful();
      }
      catch( Exception ex )
      {
        return Result.Failure( ex, "Failed to initialize the VFS device." );
      }
    }

    #endregion

    #region Virtual Methods

    protected virtual void OnDisposing( bool disposing )
    {
      _root = null;
    }

    protected virtual Task<Result> OnInitializing()
      => Task.FromResult( Result.Successful() );

    protected abstract Task<Result<VfsDirectory>> OnBuildFileTree();

    #endregion

    #region IDisposable Methods

    public void Dispose()
    {
      Dispose( true );
      GC.SuppressFinalize( this );
    }

    private void Dispose( bool disposing )
    {
      if ( _isDisposed )
        return;

      OnDisposing( disposing );
      _isDisposed = true;
    }

    #endregion

  }

}
