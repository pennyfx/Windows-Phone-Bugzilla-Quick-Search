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
using Newtonsoft.Json.Linq;

namespace BugzillaApp
{
  public class BugzillaItem
  {
    public BugzillaItem(JToken bugItem)
    {
      this.ID = long.Parse(bugItem["id"].ToString());
      this.Summary = bugItem["summary"].ToString();
      this.Status = bugItem["status"].ToString();
      this.Component = bugItem["component"].ToString();
      this.Product = bugItem["product"].ToString();
      this.Severity = bugItem["severity"].ToString();
      this.AssignedTo = bugItem["assigned_to"]["name"].ToString();
      this.Resolution = bugItem["resolution"].ToString();
      this.CreatedDate = DateTime.Parse(bugItem["creation_time"].ToString());
      
    }
    public long ID { get; set; }
    public string Status { get; set; }
    public string Component { get; set; }
    public string Product { get; set; }    
    public string Severity { get; set; }
    public string AssignedTo { get; set; }
    public string Resolution { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Summary { get; set; }
     
  }
}
