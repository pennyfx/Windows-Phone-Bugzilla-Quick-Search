using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace BugzillaApp
{
  public class Settings
  {
    public string BugzillaHost { get; set; }
    public string BugzillaApiVersion { get; set; }
    public string BugzillaUsername { get; set; }
    public string BugzillaPassword { get; set; }
    public string CachedQueriesFilePath { get; set; }

    public static Settings GetSettings()
    {
      return new Settings() { 
        BugzillaApiVersion = "1.1",
        BugzillaHost = "api-dev.bugzilla.mozilla.org",
        CachedQueriesFilePath = "/query_cache/"
      };
    }
  }
}
