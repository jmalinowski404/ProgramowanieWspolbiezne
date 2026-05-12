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
            ball.NewPositionNotification += (sender, newPosition) =>
            {
                Debug.WriteLine($"BusinessBall: Data->Business event for dataHash={System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(sender)} pos=({newPosition.x},{newPosition.y})");
        
                RaisePositionChangeEvent(this, newPosition);
            };
    }

    #region IBall

    public event EventHandler<IPosition>? NewPositionNotification;

    #endregion IBall

    #region private

    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
      Debug.WriteLine($"BusinessBall: Raising Business NewPositionNotification pos=({e.x},{e.y})");
      
      NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
    }

    #endregion private
  }
}