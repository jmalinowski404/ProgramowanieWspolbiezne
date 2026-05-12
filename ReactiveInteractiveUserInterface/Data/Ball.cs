//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//____________________________________________________________________________________________________________________________________

using System;
using System.ComponentModel;

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor

    internal Ball(Vector initialPosition, Vector initialVelocity, double mass)
    {
      Position = initialPosition;
      _velocity = initialVelocity;
      Mass = mass;
    }

    // overload zgodny z istniejącymi testami (domyślna masa)
    internal Ball(Vector initialPosition, Vector initialVelocity) : this(initialPosition, initialVelocity, 5.0)
    {
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;

    private IVector _velocity = new Vector(0, 0);
    private const double MaxSpeed = 20.0;

    public IVector Velocity
    {
      get => _velocity;
      set => _velocity = ClampVelocity(value);
    }
    public IVector Position { get; set; }
    public double Mass { get; set; }

    #endregion IBall

    #region private

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    public void Move(Vector delta)
    {
      Position = new Vector(Position.x + delta.x, Position.y + delta.y);
      RaiseNewPositionChangeNotification();
    }
    private static IVector ClampVelocity(IVector v)
    {
      double vx = v.x, vy = v.y;
      double speed = Math.Sqrt(vx * vx + vy * vy);
      if (double.IsNaN(speed) || double.IsInfinity(speed)) return new Vector(0, 0);
      if (speed <= MaxSpeed || speed == 0.0) return new Vector(vx, vy);
      double scale = MaxSpeed / speed;
      return new Vector(vx * scale, vy * scale);
    }

    #endregion private
  }
}