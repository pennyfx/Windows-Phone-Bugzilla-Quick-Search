using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Tasks;

namespace BugzillaApp
{
  public partial class SearchResults : PhoneApplicationPage
  {
    delegate void ProcessZillaResult(JObject item);

    public SearchResults()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Searching.....
    /// </summary>
    /// <param name="e"></param>
    protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
    {
      base.OnNavigatedTo(e);

      string query = "", fileName = "";

      var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
      EnsureDirectoryExists(store);
      
      // IF we have a new query, check to see if it's still cached
      if (NavigationContext.QueryString.TryGetValue("query", out query) && !string.IsNullOrEmpty(query))
      {
        string slug = query.Slugify();
        fileName = string.Format("{0}{1}",
          Settings.GetSettings().CachedQueriesFilePath, slug);

        JObject cached = CheckCache(fileName);
        if (cached != null)
        {
          ProcessBugZillaResults(cached);
        }
        else
        {
          PanoramaItem pItem = new PanoramaItem()
          {
            Name = slug,
            Header = query,
            Content = CreateProgressBar()
          };
          PanoView.Items.Add(pItem);
          PanoView.DefaultItem = pItem;
          QueryZilla(query, slug, fileName, ProcessBugZillaResults);
        }
      }

      //
      //Load cached searches
      //
      foreach (string file in store.GetFileNames(Settings.GetSettings().CachedQueriesFilePath + "*"))
      {
        var fullName = Settings.GetSettings().CachedQueriesFilePath + file;
        if (fullName == fileName) continue;
        using (var stream = store.OpenFile(fullName, FileMode.Open))
        {
          JObject jsonObj = ConvertJsonStreamToJObject(stream);
          ProcessBugZillaResults(jsonObj);
        }
      }
    }


    /// <summary>
    /// Process Json result from bugzilla API
    /// </summary>
    /// <param name="item"></param>
    private void ProcessBugZillaResults(JObject item)
    {
      var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
      PanoramaItem pItem = (PanoramaItem)PanoView.FindName(item["slug"].ToString());
      if (pItem == null)
      {
        pItem = new PanoramaItem();
        pItem.Name = item["slug"].ToString();
        pItem.Header = item["query"].ToString();
        PanoView.Items.Add(pItem);
      }

      pItem.Hold += new EventHandler<GestureEventArgs>((object sender, GestureEventArgs ge) =>
      {
        store.DeleteFile(item["filename"].ToString());
        PanoView.Items.Remove(this); // is this enough for it to be garbage collected?          
      });
      var lb = new ListBox();


      var items = ((JArray)((JObject)item)["bugs"]).Select(bug => new BugzillaItem(bug));
      foreach (var b in items)
      {
        ListBoxItem li = CreateBugzillaListItem(b);
        lb.Items.Add(li);
      }
      pItem.Content = lb;
    }


    /// <summary>
    /// Query Bugzilla, save data, and then call the process results callback
    /// </summary>
    /// <param name="query"></param>
    /// <param name="slug"></param>
    /// <param name="filename"></param>
    /// <param name="callback"></param>
    private void QueryZilla(string query, string slug, string filename, ProcessZillaResult callback)
    {
      var settings = Settings.GetSettings();
      var bugzillaQuery = string.Format("https://{0}/{1}/bug?quicksearch={2}&cache_ok=1",
        settings.BugzillaHost, settings.BugzillaApiVersion, query);

      WebRequest req = HttpWebRequest.Create(bugzillaQuery);

      req.BeginGetResponse(new AsyncCallback((IAsyncResult resResult) =>
      {

        HttpWebResponse response = (HttpWebResponse)((HttpWebRequest)resResult.AsyncState).EndGetResponse(resResult);
        JObject jsonObj = null;

        using (Stream streamResponse = response.GetResponseStream())
        {
          jsonObj = ConvertJsonStreamToJObject(streamResponse);
          jsonObj["query"] = query;
          jsonObj["slug"] = slug;
          jsonObj["filename"] = filename;

          JsonSerializer serializer = new JsonSerializer();
          var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
          var fileStream = store.CreateFile(filename);
          StreamWriter sr = new StreamWriter(fileStream);
          serializer.Serialize(sr, jsonObj);
          sr.Close();
        }

        response.Close();
        Dispatcher.BeginInvoke(callback, jsonObj);

      }), req);
    }


    /// <summary>
    /// Convert json stream to a JObject
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private JObject ConvertJsonStreamToJObject(Stream s)
    {
      JsonSerializer serializer = new JsonSerializer();
      StreamReader streamRead = new StreamReader(s);
      var r = new JsonTextReader(streamRead);
      var result = (JObject)serializer.Deserialize(r);
      return result;
    }


    /// <summary>
    /// Check to see if the results of this query already exist in local storage.
    /// If so return the deserialize object
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private JObject CheckCache(string fileName)
    {
      JObject result = null;
      var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
      if (store.FileExists(fileName))
      {
        if (store.GetLastWriteTime(fileName) > DateTime.Now.AddMinutes(10))
        {
          store.DeleteFile(fileName);
        }
        else
        {
          using (var stream = store.OpenFile(fileName, FileMode.Open))
          {
            result = ConvertJsonStreamToJObject(stream);
          }
        }
      }
      return result;
    }


    /// <summary>
    /// Create UI listitems for BugzillaItems
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    private static ListBoxItem CreateBugzillaListItem(BugzillaItem b)
    {
      ListBoxItem li = new ListBoxItem();
      li.DoubleTap += new EventHandler<GestureEventArgs>((object sender, GestureEventArgs ge) =>
      {
        WebBrowserTask task = new WebBrowserTask();
        task.Uri = new Uri("https://bugzilla.mozilla.org/show_bug.cgi?id=" + b.ID.ToString());
        task.Show();
      });
      StackPanel s = new StackPanel();
      s.Children.Add(new TextBlock()
      {
        Text = string.Format("[{1}] {0}", b.Summary, b.ID),
        TextWrapping = TextWrapping.Wrap
      });
      s.Children.Add(new TextBlock()
      {
        Text = string.Format("{0} - {1} - {2}", b.Product, b.Component, b.AssignedTo)
      });
      s.Children.Add(new TextBlock()
      {
        Text = string.Format("{0:MMM d yyyy} - {1} - {2}", b.CreatedDate, b.Status, b.Resolution),
        Margin = new Thickness(0, 0, 0, 10)
      });
      li.Content = s;
      return li;
    }


    /// <summary>
    /// Returns a progress bar
    /// </summary>
    /// <returns></returns>
    private StackPanel CreateProgressBar()
    {
      var stack = new StackPanel();
      var pb = new ProgressBar()
      {
        Margin = new Thickness(0, 120, 0, 50),
        Opacity = 1,
        Height = 10,
        VerticalAlignment = System.Windows.VerticalAlignment.Center,
        Name = "progressBar",
        Style = this.Resources["PerformanceProgressBar"] as Style,
        IsIndeterminate = true,
        Visibility = System.Windows.Visibility.Visible
      };
      stack.Children.Add(pb);
      return stack;
    }

    /// <summary>
    /// Make sure the cached queries directory exists
    /// </summary>
    /// <param name="store"></param>
    private static void EnsureDirectoryExists(System.IO.IsolatedStorage.IsolatedStorageFile store)
    {
      if (!store.DirectoryExists(Settings.GetSettings().CachedQueriesFilePath))
      {
        store.CreateDirectory(Settings.GetSettings().CachedQueriesFilePath.Replace("/", ""));
      }
    }
  }
}