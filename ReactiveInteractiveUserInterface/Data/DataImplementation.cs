//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//____________________________________________________________________________________________________________________________________

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.IO;
using System.Timers;

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor

    public DataImplementation()
    {
      pauseEvent = new ManualResetEventSlim(true);
      workers = new Dictionary<Ball, (Thread thread, CancellationTokenSource cts)>();
      BallsList = new List<Ball>();

      timer = new System.Timers.Timer(25);
      SetupMasterTimer();
    }

    private void SetupMasterTimer() {
        Random random = new Random();

        timer.Elapsed += (sender, e) =>
        {
            if (!pauseEvent.IsSet) return;

            tickCounter++;

            if (tickCounter >= 40)
            {
                tickCounter = 0;

                string randomColor = availableColors[random.Next(availableColors.Length)];

                lock (BallsLock)
                {
                    foreach (var ball in BallsList)
                    {
                        ball.ChangeColorAndNotify(randomColor);
                    }
                }

                logger.LogGlobalEvent($"Color: {randomColor}");
            }
        };
    }

    #endregion ctor

    #region DataAbstractAPI

    public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(DataImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));

      Random random = new Random();

      List<Thread> threadsToStart = new List<Thread>();

      for (int i = 0; i < numberOfBalls; i++)
      {
        Vector startingPosition = new(random.Next(100, 400 - 100), random.Next(100, 400 - 100));
        Vector startingVelocity = new Vector(random.Next(-5, 6), random.Next(-5, 6));
        Ball newBall = new(startingPosition, startingVelocity, 5.0);

        upperLayerHandler(startingPosition, newBall);

        lock (BallsLock)
        {
          BallsList.Add(newBall);
        }

        var cts = new CancellationTokenSource();
        Thread thread = new Thread(() => WorkerLoop(newBall, cts.Token))
        {
          IsBackground = true,
          Name = $"BallWorker-{i}"
        };
        workers.Add(newBall, (thread, cts));
        threadsToStart.Add(thread);
      }

      foreach (var t in threadsToStart)
      {
         t.Start();
      }

      timer.Start();
    }

    public override void Pause()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(DataImplementation));

        
      timer.Stop();
      pauseEvent.Reset();
    }

    public override void Resume()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(DataImplementation));

      timer.Start();
      pauseEvent.Set();
    }

    #endregion DataAbstractAPI

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          foreach (var entry in workers.Values)
          {
            try { entry.cts.Cancel(); }
            catch { }
          }

          foreach (var entry in workers.Values)
          {
            try
            {
              if (entry.thread.IsAlive)
                entry.thread.Join(200);
            }
            catch { }
          }

          foreach (var entry in workers.Values)
          {
            try { entry.cts.Dispose(); } catch { }
          }

          workers.Clear();

          lock (BallsLock)
          {
            foreach (var b in BallsList)
            {
              try { /* nothing extra to do for Ball */ } catch { }
            }
            BallsList.Clear();
          }

          pauseEvent.Dispose();

          timer.Stop();
          timer.Dispose();

          logger.Dispose();
        }
        Disposed = true;
      }
      else
        throw new ObjectDisposedException(nameof(DataImplementation));
    }

    public override void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    private bool Disposed = false;

    private List<Ball> BallsList;
    private readonly object BallsLock = new();
    private readonly ManualResetEventSlim pauseEvent;
    private readonly Dictionary<Ball, (Thread thread, CancellationTokenSource cts)> workers;
    private readonly Logger logger = new Logger();
    private System.Timers.Timer timer;
    private int tickCounter = 0;
    private readonly string[] availableColors = { "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Magenta", "Cyan" };

    private void WorkerLoop(Ball ball, CancellationToken ct)
    {
      try
      {
        while (!ct.IsCancellationRequested)
        {
          pauseEvent.Wait(ct);

          var v = ball.Velocity;
          if (double.IsNaN(v.x) || double.IsNaN(v.y) || double.IsInfinity(v.x) || double.IsInfinity(v.y))
          {
            ball.Velocity = new Vector(0, 0);
          }

          ball.Move(new Vector(ball.Velocity.x, ball.Velocity.y));

          Thread.Sleep(25);
        }
      }
      catch (OperationCanceledException)
      {
        Debug.WriteLine($"Worker cancelled: thread={Thread.CurrentThread.ManagedThreadId}, ballHash={RuntimeHelpers.GetHashCode(ball)}");
      }
      catch (Exception e)
      {
        Debug.WriteLine($"Ball worker exception: {e}");
      }
    }

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
    {
      lock (BallsLock)
      {
        returnBallsList(BallsList.ToArray());
      }
    }

    [Conditional("DEBUG")]
    internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
    {
      lock (BallsLock)
      {
        returnNumberOfBalls(BallsList.Count);
      }
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}