//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024 Mariusz Postol LODZ POLAND.
//
//____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using TP.ConcurrentProgramming.Data;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
  {
    #region ctor

    public BusinessLogicImplementation() : this(null)
    { }

    internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
    {
      layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
    }

    #endregion ctor

    #region BusinessLogicAbstractAPI

    public override void Dispose()
    {
      Debug.WriteLine("BusinessLogicImplementation.Dispose called");
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      layerBellow.Dispose();
      Disposed = true;
    }

    public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(BusinessLogicImplementation));

      // clear under lock
      lock (logicBallsLock)
      {
        logicBalls.Clear();
      }

      layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
      {
        lock (logicBallsLock)
        {
          logicBalls.Add(databall);
        }

        databall.NewPositionNotification += (sender, args) =>
        {
          CheckCollisions(databall);
        };

        upperLayerHandler(new Position(startingPosition.x, startingPosition.y), new Ball(databall));
      });
    }

    public override void Pause()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

      layerBellow.Pause();
    }

    public override void Resume()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

      layerBellow.Resume();
    }

    #endregion BusinessLogicAbstractAPI

    #region private

    private bool Disposed = false;

    private List<Data.IBall> logicBalls = new();
    private readonly object logicBallsLock = new();

    private readonly UnderneathLayerAPI layerBellow;

    private static readonly object tieLock = new object();

    private void CheckCollisions(Data.IBall ball)
    {
      double boardWidth = 400.0;
      double boardHeight = 400.0;
      double ballDiameter = 20.0;
      double ballRadius = ballDiameter / 2.0;

      // snapshot under lock to avoid collection-modified exceptions
      Data.IBall[] snapshot;
      lock (logicBallsLock)
      {
        snapshot = logicBalls.ToArray();
      }

      foreach (var otherBall in snapshot)
      {
        if (otherBall == ball) continue;

        Data.IBall first = ball;
        Data.IBall second = otherBall;

        int h1 = RuntimeHelpers.GetHashCode(first);
        int h2 = RuntimeHelpers.GetHashCode(second);

        if (h1 > h2)
        {
          var temp = first;
          first = second;
          second = temp;
        }

        if (h1 == h2)
        {
          lock (tieLock)
          {
            lock (first)
            {
              lock (second)
              {
                ProcessCollision(ball, otherBall, ballRadius, ballDiameter, boardWidth, boardHeight);
              }
            }
          }
        }
        else
        {
          lock (first)
          {
            lock (second)
            {
              ProcessCollision(ball, otherBall, ballRadius, ballDiameter, boardWidth, boardHeight);
            }
          }
        }
      }

      if (ball.Position.x <= 0 || ball.Position.x >= boardWidth - ballDiameter)
      {
        ball.Velocity = new Data.Vector(-ball.Velocity.x, ball.Velocity.y);
      }

      if (ball.Position.y <= 0 || ball.Position.y >= boardHeight - ballDiameter)
      {
        ball.Velocity = new Data.Vector(ball.Velocity.x, -ball.Velocity.y);
      }
    }

    private void ProcessCollision(Data.IBall ball, Data.IBall otherBall, double ballRadius, double ballDiameter, double boardWidth, double boardHeight)
    {
      double dx = otherBall.Position.x - ball.Position.x;
      double dy = otherBall.Position.y - ball.Position.y;
      double distance = Math.Sqrt(dx * dx + dy * dy);

      if (distance == 0 || distance >= 2 * ballRadius) return;

      double nx = dx / distance;
      double ny = dy / distance;

      double dvx = ball.Velocity.x - otherBall.Velocity.x;
      double dvy = ball.Velocity.y - otherBall.Velocity.y;
      double dvDN = dvx * nx + dvy * ny;

      if (dvDN <= 0) return;

      double m1 = ball.Mass;
      double m2 = otherBall.Mass;

      double impulse = 2.0 * dvDN / (m1 + m2);

      ball.Velocity = new Data.Vector(
          ball.Velocity.x - impulse * m2 * nx,
          ball.Velocity.y - impulse * m2 * ny
      );
      otherBall.Velocity = new Data.Vector(
          otherBall.Velocity.x + impulse * m1 * nx,
          otherBall.Velocity.y + impulse * m1 * ny
      );

      // prevent overlap
      double overlap = 2 * ballRadius - distance;
      ball.Position = new Data.Vector(
          ball.Position.x - overlap / 2 * nx,
          ball.Position.y - overlap / 2 * ny
      );
      otherBall.Position = new Data.Vector(
          otherBall.Position.x + overlap / 2 * nx,
          otherBall.Position.y + overlap / 2 * ny
      );
    }

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}