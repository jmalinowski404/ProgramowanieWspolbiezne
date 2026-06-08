//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2023, Mariusz Postol LODZ POLAND.
//
//____________________________________________________________________________________________________________________________________

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using TP.ConcurrentProgramming.BusinessLogic;
using LogicIBall = TP.ConcurrentProgramming.BusinessLogic.IBall;

namespace TP.ConcurrentProgramming.Presentation.Model
{
  internal class ModelBall : IBall
  {
    public ModelBall(double top, double left, LogicIBall underneathBall)
    {
      TopBackingField = top;
      LeftBackingField = left;
      _underneathBall = underneathBall;
      syncContext = SynchronizationContext.Current;
      underneathBall.NewPositionNotification += NewPositionNotification;
    }

    #region IBall

    public double Top
    {
      get { return TopBackingField; }
      private set
      {
        if (TopBackingField == value)
          return;
        TopBackingField = value;
        RaisePropertyChanged();
      }
    }

    public double Left
    {
      get { return LeftBackingField; }
      private set
      {
        if (LeftBackingField == value)
          return;
        LeftBackingField = value;
        RaisePropertyChanged();
      }
    }

    public string Color
    {
      get { return ColorBackingField; }
      private set
      {
        if (ColorBackingField == value)
          return;
        ColorBackingField = value;
        RaisePropertyChanged();
      }
    }

    public double Diameter { get; init; } = 20.0;

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion INotifyPropertyChanged

    #endregion IBall

    #region private

    private double TopBackingField;
    private double LeftBackingField;
    private string ColorBackingField = "Blue";
    private readonly LogicIBall _underneathBall;
    private readonly SynchronizationContext? syncContext;

    private void NewPositionNotification(object sender, IPosition e)
    {
      if (syncContext != null)
      {
        try
        {
          syncContext.Post(_ =>
          {
            Top = e.y;
            Left = e.x;
            Color = _underneathBall.Color;
          }, null);
        }
        catch (System.Exception ex)
        {
          Top = e.y;
          Left = e.x;
          Color = _underneathBall.Color;
        }
      }
      else
      {
        Top = e.y;
        Left = e.x;
        Color = _underneathBall.Color;
      }
    }

    private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion private

    #region testing instrumentation

    [Conditional("DEBUG")]
    internal void SetLeft(double x)
    { Left = x; }

    [Conditional("DEBUG")]
    internal void SettTop(double x)
    { Top = x; }

    #endregion testing instrumentation
  }
}