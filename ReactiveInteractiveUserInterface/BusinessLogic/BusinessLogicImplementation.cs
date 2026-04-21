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
using System.Numerics;
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
        throw new ArgumentNullException(nameof(upperLayerHandler));

      logicBalls.Clear();

      layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
      {
          logicBalls.Add(databall);

          databall.NewPositionNotification += (sender, args) =>
          {
               CheckCollisions(databall);
          };

          upperLayerHandler(new Position(startingPosition.x, startingPosition.x), new Ball(databall));
      }          
      
      );
    }

    #endregion BusinessLogicAbstractAPI

    #region private

    private bool Disposed = false;

    private List<Data.IBall> logicBalls = new();

    private readonly UnderneathLayerAPI layerBellow;

    private void CheckCollisions(Data.IBall ball)
        {
            double boardWidth = 400.0;
            double boardHeight = 400.0;
            double ballDiameter = 20.0;
            double ballRadius = ballDiameter / 2.0;

            foreach (var otherBall in logicBalls)
            {
                if (otherBall == ball) continue;

                lock (ball)
                {
                    lock (otherBall)
                    {
                        double dx = Math.Abs(otherBall.Position.x - ball.Position.x);
                        double dy = Math.Abs(otherBall.Position.y - ball.Position.y);
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        if (distance <= ballDiameter)
                        {
                            ball.Velocity = new Data.Vector(-ball.Velocity.x, -ball.Velocity.y);
                            otherBall.Velocity = new Data.Vector(-otherBall.Velocity.x, -otherBall.Velocity.y);
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