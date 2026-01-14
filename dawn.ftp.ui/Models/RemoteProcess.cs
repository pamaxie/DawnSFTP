using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace dawn.ftp.ui.Models;

/// <summary>
/// Represents a remote process with various attributes such as name, owner,
/// CPU usage, memory usage, and runtime information. This class is used to
/// monitor and manage processes on a remote system.
/// </summary>
public class RemoteProcess : ObservableObject {
    private string _name = string.Empty;
    private string _owner = string.Empty;
    private double _cpuUsage = 0.0;
    private double _memoryUsage = 0.0;
    private TimeSpan? _runTime = null;

    /// <summary>
    /// Gets or sets the name of the remote process. This property specifies
    /// the identifier or descriptive label for the process being monitored
    /// on the remote system.
    /// </summary>
    public string Name {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Gets or sets the owner of the remote process. This property indicates
    /// the user or entity responsible for initiating or managing the process
    /// on the remote system.
    /// </summary>
    public string Owner {
        get => _owner;
        set => SetProperty(ref _owner, value);
    }

    /// <summary>
    /// Gets or sets the CPU usage of the remote process. This property represents
    /// the percentage of CPU resources currently utilized by the process, expressed
    /// as a floating-point number. It provides insight into the computational
    /// load imposed by the process.
    /// </summary>
    public double CpuUsage {
        get => _cpuUsage;
        set {
            if (SetProperty(ref _cpuUsage, value)) {
                OnPropertyChanged(nameof(CpuUsageDisplay));
            }
        }
    }

    /// <summary>
    /// Gets the CPU usage formatted as a percentage string.
    /// </summary>
    public string CpuUsageDisplay => $"{_cpuUsage:F1}%";

    /// <summary>
    /// Gets or sets the memory usage of the remote process. This property represents
    /// the amount of memory, in megabytes, that the process is currently utilizing.
    /// The value is a floating-point number, enabling precise measurement of memory
    /// consumption.
    /// </summary>
    public double MemoryUsage {
        get => _memoryUsage;
        set {
            if (SetProperty(ref _memoryUsage, value)) {
                OnPropertyChanged(nameof(MemoryUsageDisplay));
            }
        }
    }

    /// <summary>
    /// Gets the memory usage formatted as a percentage string.
    /// </summary>
    public string MemoryUsageDisplay => $"{_memoryUsage:F1}%";

    /// <summary>
    /// Gets or sets the runtime of the remote process. This property represents
    /// the duration for which the process has been running. The value is nullable
    /// and can be <c>null</c> if the runtime information is unavailable or if the
    /// process has not started yet.
    /// </summary>
    public TimeSpan? RunTime {
        get => _runTime;
        set => SetProperty(ref _runTime, value);
    }
}