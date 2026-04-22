//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable, INotifyPropertyChanged
  {
    #region ctor

    public MainWindowViewModel() : this(null)
    { }

    internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
    {
      ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
      Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));

      StartCommand = new RelayCommand(StartSimulation);
       PauseCommand = new RelayCommand(PauseSimulation);
     }

    #endregion ctor

    #region public API

    public void Start(int numberOfBalls)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      ModelLayer.Start(numberOfBalls);
      Observer.Dispose();
    }

    public void Pause()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            ModelLayer.Pause();
        }

    public void Resume() 
    { 
        ModelLayer.Resume();
    }

    public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

    #endregion public API

    #region Properties & Commands (DODANE DLA XAML)

    private int _numberOfBalls = 5;

    public int NumberOfBalls
    {
      get => _numberOfBalls;
      set
      {
        if (_numberOfBalls != value)
        {
          _numberOfBalls = value;
          RaisePropertyChanged(nameof(NumberOfBalls));
        }
      }
    }

    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }

    private void StartSimulation()
    {
       Start(NumberOfBalls);

       IsRunning = true;
    }

    private void PauseSimulation()
    {
        if (IsRunning) {
            Pause();
            IsRunning = false;
        } else {
            ResumeSimulation();
        }
    }

    private void ResumeSimulation()
    {
        Resume();

        IsRunning = true;
    }

    #endregion Properties & Commands

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          Balls.Clear();
          Observer.Dispose();
          ModelLayer.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        Disposed = true;
      }
    }

    public void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    public bool IsRunning
    {
        get => isRunning;
        set
        {
            isRunning = value;

            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(ActionButtonText));
        }
    }

    public string ActionButtonText => IsRunning ? "Pause" : "Resume";

    #endregion IDisposable

    #region private

    private IDisposable Observer = null;
    private ModelAbstractApi ModelLayer;
    private bool Disposed = false;
    private bool isRunning = false;

    #endregion private

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion
  }
}