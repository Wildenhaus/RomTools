using System;

namespace RomTools
{

  public class Result
  {

    #region Properties

    public bool Success { get; }
    public string Message { get; }
    public Exception Exception { get; }

    #endregion

    #region Constructor

    public Result( bool success, string message = null )
    {
      Success = success;
    }

    public Result( Exception exception, string message = null )
    {
      Success = false;
      Exception = exception;
      Message = message ?? exception.Message;
    }

    public static Result Successful( string message = null )
      => new Result( true, message );

    public static Result<T> Successful<T>( T value, string message = null )
      => new Result<T>( true, value, message );

    public static Result Failure( string message = null )
      => new Result( false, message );

    public static Result<T> Failure<T>( string message = null )
      => new Result<T>( false, default, message );

    public static Result Failure( Exception exception, string message = null )
      => new Result( exception, message );

    public static Result<T> Failure<T>( Exception exception, string message = null )
      => new Result<T>( exception, message );

    #endregion

  }

  public class Result<T> : Result
  {

    #region Properties

    public T Value { get; }

    #endregion

    #region Constructor

    public Result( bool success, T value, string message = null )
      : base( success, message )
    {
      Value = value;
    }

    public Result( Exception exception, string message = null )
      : base( exception, message )
    {
    }

    #endregion

  }

}
