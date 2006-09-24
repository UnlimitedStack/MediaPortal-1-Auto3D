#region usings

using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using MediaPortal.Util;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.GUI.Library;
//using MediaPortal.Webepg.TV.Database;

#endregion

namespace MediaPortal.ProcessPlugins.WebEPG
{
  public class WebEPGGrabber : IPlugin, IWakeable, ISetupForm
  {

    #region vars

    bool _grabberRunning;
    bool _run;
    int _runHours = 0;
    int _runMinutes = 0;
    bool _runMondays = false;
    bool _runTuesdays = false;
    bool _runWednesdays = false;
    bool _runThursdays = false;
    bool _runFridays = false;
    bool _runSaturdays = false;
    bool _runSundays = false;
    Thread _thread;
    ILog _epgLog;

    #endregion

    #region Ctor

    public WebEPGGrabber()
    {
      // setup logging
      ServiceProvider services = GlobalServiceProvider.Instance;
      _epgLog = services.Get<ILog>();

      // load settings
      using (Settings reader = new Settings(Config.GetFile(Config.Dir.Config, "mediaportal.xml")))
      {
        _runMondays = reader.GetValueAsBool("webepggrabber", "monday", true);
        _runTuesdays = reader.GetValueAsBool("webepggrabber", "tuesday", true);
        _runWednesdays = reader.GetValueAsBool("webepggrabber", "wednesday", true);
        _runThursdays = reader.GetValueAsBool("webepggrabber", "thursday", true);
        _runFridays = reader.GetValueAsBool("webepggrabber", "friday", true);
        _runSaturdays = reader.GetValueAsBool("webepggrabber", "saturday", true);
        _runSundays = reader.GetValueAsBool("webepggrabber", "sunday", true);
        _runHours = reader.GetValueAsInt("webepggrabber", "hours", 0);
        _runMinutes = reader.GetValueAsInt("webepggrabber", "minutes", 0);
      }
      Log.Info("WebEPGGrabber: schedule: {0}:{1}", _runHours, _runMinutes);
      Log.Info("WebEPGGrabber: run on: monday:{0}, tuesday:{1}, wednesday:{2}, thursday:{3}, friday:{4}, saturday:{5}, sunday:{6}", _runMondays, _runTuesdays, _runWednesdays, _runThursdays, _runFridays, _runSaturdays, _runSundays);
    }
    #endregion

    #region IPlugin members

    /// <summary>
    /// Starts the WebEPGGrabber
    /// </summary>
    public void Start()
    {
      _run = true;
      _thread = new Thread(new ThreadStart(Run));
      _thread.IsBackground = true;
      _thread.Name = "WebEPGGrabber thread";
      _thread.Priority = ThreadPriority.Lowest;
      _thread.Start();
      Log.Info("WebEPGGrabber: started");
    }

    /// <summary>
    /// Stops the WebEPGGrabber
    /// </summary>
    public void Stop()
    {
      _run = false;
      Log.Info("WebEPGGrabber: stopped");
    }

    #endregion

    /// <summary>
    /// Main entrypoint for the scheduler thread. Ideally should be replaced by a general scheduler/threadpool
    /// which can run timed events.
    /// </summary>
    private void Run()
    {
      MediaPortal.GUI.Library.Log.Debug("WebEPGGrabber.Run: thread started");
      while (_run)
      {
        if (ShouldRunSchedule())
        {
          // Start the WebEPG grabber
          _grabberRunning = true;
          Log.Info("WebEPGGrabber.Run: schedule is due:{0}", DateTime.Now.ToString());

          MediaPortal.EPG.WebEPG grabber = new MediaPortal.EPG.WebEPG();
          try
          {
            Log.Info("WebEPGGrabber.Run: run grabber");
            grabber.Import();
          }
          catch (Exception ex)
          {
            Log.Error("WebEPGGrabber.Run: grabber exception:{0} {1}", ex.Message, ex.StackTrace);
            _epgLog.Error(LogType.WebEPG, "WebEPG: Fatal Error");
            _epgLog.Error(LogType.WebEPG, "WebEPG: {0}", ex.Message);
          }
          _epgLog.Info(LogType.WebEPG, "WebEPG: Finished");

          // store last run
          using (Settings writer = new Settings(Config.GetFile(Config.Dir.Config, "mediaportal.xml")))
          {
            writer.SetValue("webepggrabber", "lastrun", DateTime.Now.Day);
          }
          Log.Info("WebEPGGrabber.Run: grabber finished:{0}", DateTime.Now.ToString());
          _grabberRunning = false;
        }
        else
        {
          // stay Idle for a minute checking if we have to stop every second
          int timeout = 60000;
          while (_run && timeout > 0)
          {
            Thread.Sleep(1000);
            timeout -= 1000;
          }
        }
      }
      Log.Debug("WebEPGGrabber.Run: thread stopped");
    }

    /// <summary>
    /// Determines if the configured scheduled is due
    /// </summary>
    /// <returns>bool indicating whether or not to start the grabber</returns>
    private bool ShouldRunSchedule()
    {
      // if we've already run today then don't run
      if (HasRunToday())
      {
        return false;
      }

      // check if we have to run this day
      if (!ShouldRun(DateTime.Now.DayOfWeek))
      {
        return false;
      }

      // check if the schedule is due
      if (DateTime.Now.Hour >= _runHours)
      {
        if (DateTime.Now.Minute >= _runMinutes)
        {
          Log.Info("WebEPGGrabber.ShouldRunSchedule: schedule {0}:{1} is due: {2}:{3}", _runHours, _runMinutes, DateTime.Now.Hour, DateTime.Now.Minute);
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Determines if the grabber has already run today
    /// </summary>
    /// <returns></returns>
    private bool HasRunToday()
    {
      int lastRunDay;
      using (Settings reader = new Settings(Config.GetFile(Config.Dir.Config, "mediaportal.xml")))
      {
        lastRunDay = reader.GetValueAsInt("webepggrabber", "lastrun", 0);
      }
      if (lastRunDay == DateTime.Now.Day)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Determines if the schedule should run on the specified DateTime.DayOfWeek
    /// </summary>
    /// <param name="dow">DayOfWeek to check for</param>
    /// <returns>bool indicating whether or not the schedule should run on the specified day</returns>
    private bool ShouldRun(DayOfWeek dow)
    {
      switch (dow)
      {
        case DayOfWeek.Monday:
          if (!_runMondays) return false;
          break;
        case DayOfWeek.Tuesday:
          if (!_runTuesdays) return false;
          break;
        case DayOfWeek.Wednesday:
          if (!_runWednesdays) return false;
          break;
        case DayOfWeek.Thursday:
          if (!_runThursdays) return false;
          break;
        case DayOfWeek.Friday:
          if (!_runFridays) return false;
          break;
        case DayOfWeek.Saturday:
          if (!_runSaturdays) return false;
          break;
        case DayOfWeek.Sunday:
          if (!_runSundays) return false;
          break;
        default:
          return false;
      }
      return true;
    }

    #region IWakeable members

    /// <summary>
    /// Determines on what DateTime the next schedule will run
    /// </summary>
    /// <param name="time">earliestWakeupDateTime</param>
    /// <returns>DateTime indicating the next schedule</returns>
    public DateTime GetNextEvent(DateTime time)
    {
      DateTime nextRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, _runHours, _runMinutes, 0);
      if (HasRunToday())
      {
        // find out next scheduled run datetime
        int i = 0;
        while (++i < 8)
        {
          if (ShouldRun(nextRun.AddDays(i).DayOfWeek))
          {
            nextRun = nextRun.AddDays(i);
            break;
          }
        }
        if (DateTime.Now.DayOfWeek == nextRun.DayOfWeek)
        {
          MediaPortal.GUI.Library.Log.Error("WebEPGGrabber.GetNextEvent: no valid next run day found!");
          nextRun = nextRun.AddYears(1);
        }
      }
      return nextRun;
    }

    /// <summary>
    /// Is PowerScheduler allowed to put the system into standby?
    /// </summary>
    /// <returns>bool</returns>
    public bool DisallowShutdown()
    {
      return _grabberRunning;
    }

    #endregion
    

    #region ISetupForm members

    public string PluginName()
    {
      return "WebEPG grabber";
    }

    public string Author()
    {
      return "micheloe";
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public string Description()
    {
      return "Run WebEPG inside MediaPortal, preventing standby when active and resuming from standby when schedule is due";
    }

    public bool GetHome(out string buttonText, out string buttonImage, out string imageFocus, out string pictureImage)
    {
      buttonText = String.Empty;
      buttonImage = String.Empty;
      imageFocus = String.Empty;
      pictureImage = String.Empty;
      return false;
    }

    public int GetWindowId()
    {
      return -1;
    }

    public bool HasSetup()
    {
      return true;
    }

    public void ShowPlugin()
    {
      System.Windows.Forms.Form f = new WebEPGGrabberSettings();
      System.Windows.Forms.DialogResult result = f.ShowDialog();
    }

    #endregion

  }
}
