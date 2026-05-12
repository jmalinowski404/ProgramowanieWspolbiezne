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

    public double Diameter { get; init; } = 0;

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion INotifyPropertyChanged

    #endregion IBall

    #region private

    private double TopBackingField;
    private double LeftBackingField;
    private readonly SynchronizationContext? syncContext;

    private void NewPositionNotification(object sender, IPosition e)
    {
      Debug.WriteLine($"ModelBall: received NewPositionNotification pos=({e.x},{e.y}) from senderHash={System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(sender)}");

      if (syncContext != null)
      {
        try
        {
          syncContext.Send(_ =>
          {
            Top = e.y;
            Left = e.x;
          }, null);
        }
        catch (System.Exception ex)
        {
          Debug.WriteLine($"ModelBall: syncContext.Send exception: {ex}");
          Top = e.y;
          Left = e.x;
        }
      }
      else
      {
        Top = e.y;
        Left = e.x;
      }
    }

    private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
      Debug.WriteLine($"ModelBall: PropertyChanged {propertyName} Top={TopBackingField} Left={LeftBackingField}");
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