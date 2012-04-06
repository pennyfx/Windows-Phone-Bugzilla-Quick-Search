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
using System.IO;
using System.Text;
using System.Resources;


namespace BugzillaApp
{
  public partial class MainPage : PhoneApplicationPage
  {
    // Constructor
    public MainPage()
    {
      InitializeComponent();
    }


    private void btnSearch_Click(object sender, RoutedEventArgs e)
    {
      NavigationService.Navigate(new Uri("/SearchResults.xaml?query=" + txtSearch.Text, UriKind.Relative));      
    }

  }
}