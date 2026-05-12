//____________________________________________________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using UnderneathLayerAPI = TP.ConcurrentProgramming.BusinessLogic.BusinessLogicAbstractAPI;

namespace TP.ConcurrentProgramming.Presentation.Model
{
  /// <summary>
  /// Class Model - implements the <see cref="ModelAbstractApi" />
  /// </summary>
  internal class ModelImplementation : ModelAbstractApi
  {
    internal ModelImplementation() : this(null)
    { }

    internal ModelImplementation(UnderneathLayerAPI underneathLayer)
    {
      layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetBusinessLogicLayer() : underneathLayer;
      eventObservable = Observable.FromEventPattern<BallChaneEventArgs>(this, "BallChanged");
      syncContext = SynchronizationContext.Current;
    }

    #region ModelAbstractApi

    public override void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(Model));
      layerBellow.Dispose();
      Disposed = true;
    }

    public override IDisposable Subscribe(IObserver<IBall> observer)
    {
        IScheduler scheduler = syncContext != null ? new SynchronizationContextScheduler(syncContext) : Scheduler.Default;

        return eventObservable
                    .ObserveOn(scheduler)
                    .Subscribe(x => observer.OnNext(x.EventArgs.Ball),
                                ex => observer.OnError(ex),
                                () => observer.OnCompleted());
    }

    public override void Start(int numberOfBalls)
    {
      layerBellow.Start(numberOfBalls, StartHandler);
    }

        public override void Pause()
        {
            layerBellow.Pause();
        }

        public override void Resume()
        {
            layerBellow.Resume();
        }

    #endregion ModelAbstractApi

    #region API

    public event EventHandler<BallChaneEventArgs> BallChanged;

    #endregion API

    #region private

    private bool Disposed = false;
    private readonly IObservable<EventPattern<BallChaneEventArgs>> eventObservable = null;
    private readonly UnderneathLayerAPI layerBellow = null;
    private readonly SynchronizationContext? syncContext;

    private void StartHandler(BusinessLogic.IPosition position, BusinessLogic.IBall ball)
    {
      var handler = BallChanged;
      if (handler == null) return;

      if (syncContext != null)
      {
        syncContext.Post(_ =>
        {
          try
          {
            ModelBall newBall = new ModelBall(position.x, position.y, ball) { Diameter = 20.0 };
            var args = new BallChaneEventArgs() { Ball = newBall };
            handler(this, args);
          }
          catch (Exception e)
          {
            Debug.WriteLine($"StartHandler UI-post exception: {e}");
          }
        }, null);
      }
      else
      {
        try
        {
          ModelBall newBall = new ModelBall(position.x, position.y, ball) { Diameter = 20.0 };
          var args = new BallChaneEventArgs() { Ball = newBall };
          handler(this, args);
        }
        catch (Exception e)
        {
          Debug.WriteLine($"StartHandler direct exception: {e}");
        }
      }
    }

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    [Conditional("DEBUG")]
    internal void CheckUnderneathLayerAPI(Action<UnderneathLayerAPI> returnNumberOfBalls)
    {
      returnNumberOfBalls(layerBellow);
    }

    [Conditional("DEBUG")]
    internal void CheckBallChangedEvent(Action<bool> returnBallChangedIsNull)
    {
      returnBallChangedIsNull(BallChanged == null);
    }

    #endregion TestingInfrastructure
  }

  public class BallChaneEventArgs : EventArgs
  {
    public IBall Ball { get; init; }
  }
}