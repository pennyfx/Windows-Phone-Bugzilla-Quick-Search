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
using System.Text.RegularExpressions;
using System.IO;

namespace BugzillaApp
{
  public static class ExtensionMethods
  {
    public static string Slugify(this string data){
      Regex re = new Regex(@"\w+");
      Match matchResults = re.Match(data);
      var items = new System.Collections.Generic.List<string>();
      while (matchResults.Success)
      {
        // matched text: matchResults.Value
        // match start: matchResults.Index
        // match length: matchResults.Length
        items.Add(matchResults.Value);
        matchResults = matchResults.NextMatch();
      }
      return string.Join("_", items.ToArray());
    }

    public static void CopyStream(Stream input, Stream output)
    {
      byte[] buffer = new byte[0x1000];
      int read;
      while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        output.Write(buffer, 0, read);
    }
  }
}
