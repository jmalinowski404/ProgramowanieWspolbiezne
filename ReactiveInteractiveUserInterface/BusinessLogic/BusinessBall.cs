//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class Ball : IBall
  {
    public Ball(Data.IBall ball)
    {
            _dataBall = ball;
            ball.NewPositionNotification += (sender, newPosition) =>
            {
                RaisePositionChangeEvent(this, newPosition);
            };
    }

    #region IBall

    public string Color => _dataBall.Color;
    public event EventHandler<IPosition>? NewPositionNotification;

    #endregion IBall

    #region private
    private readonly Data.IBall _dataBall;

    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
      NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
    }

    #endregion private
  }
}