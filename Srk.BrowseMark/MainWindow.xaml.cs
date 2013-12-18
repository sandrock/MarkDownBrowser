
namespace Srk.BrowseMark
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using Markdown.Xaml;
    using Microsoft.Win32;
    using SrkToolkit.Mvvm.Commands;
    using uhttpsharp;
    using System.Net;
    using Srk.BrowseMark.LocalHttpServer;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string address;
        private string status;
        private TextToFlowDocumentConverter converter;
        private FlowDocument document;
        private Markdown md;
        private string xaml;
        private ICommand goToPageCommand;
        private HttpServer server;

        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.md = this.Resources["Markdown"] as Markdown;
            this.converter = new TextToFlowDocumentConverter()
            {
                Markdown = this.md,
            };
            this.Status = "Ready.";

            try
            {
                this.server = new HttpServer(IPAddress.Loopback, 8001);
                this.server.Start();
            }
            catch (Exception ex)
            {
                this.Status = "Failed to start internal HTTP server: " + ex.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Address
        {
            get { return this.address; }
            set { this.SetValue(ref address, value, "Address"); }
        }

        public string Status
        {
            get { return this.status; }
            set { this.SetValue(ref status, value, "Status"); }
        }

        public FlowDocument Document
        {
            get { return this.document; }
            set { this.SetValue(ref document, value, "Document"); }
        }

        public string Xaml
        {
            get { return this.xaml; }
            set { this.SetValue(ref xaml, value, "Xaml"); }
        }

        public ICommand GoToPage
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return this.goToPageCommand ?? (this.goToPageCommand = new RelayCommand<string>(this.OnGoToPage)); }
        }

        private void OnOpenButtonClicked(object sender, RoutedEventArgs e)
        {
            var diag = new OpenFileDialog()
            {
                DefaultExt = "Markdown file|*.md,*.markdown",
            };
            if (diag.ShowDialog(this) == true)
            {
                this.Address = diag.FileName;
                this.Load(this.address);
            }
        }

        private void OnGoButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Address))
            {
                this.Load(this.address);
            }
        }

        private void Load(string address)
        {
            var url = MdHandler.GetDocumentsUrl(address, this.server.Port);
            System.Diagnostics.Process.Start(url);
            
            ////this.browser.Navigate(url);
            return;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                string content = null;
                try
                {
                    if (address.StartsWith("http"))
                    {
                        this.Dispatcher.BeginInvoke(() => 
                        {
                            this.Status = "HTTP is not yet supported.";
                        });
                    }
                    else if (File.Exists(address))
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            this.Status = "Loading file...";
                        });
                        ////content = File.ReadAllText(address);

                        


                        ////HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                        ////var response = request.GetResponse();
                        ////using (var responseStream = response.GetResponseStream())
                        ////using   (var responseStreamReader = new StreamReader(responseStream))
                        ////{
                        ////    content = responseStreamReader.ReadToEnd();
                        ////}

                    }
                }
                catch (Exception ex)
                {
                    this.status = ex.Message;
                }

                if (content != null)
                {

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        this.Document = this.converter.Convert(content, typeof(FlowDocument), null, CultureInfo.CurrentCulture) as FlowDocument;

                        this.Xaml =  XamlWriter.Save(this.Document);
                        //this.Xaml = this.md.Transform(content);

                        var pages = this.Document.FindChildren<ContainerVisual>(true, false);
                        foreach (var page in pages)
                        {
                            page.SetValue(FrameworkElement.DataContextProperty, this);
                        }

                        this.Status = "Loaded.";
                    });
                }
            });
        }





        private void OnGoToPage(string param)
        {
            if (param == null)
                return;

            if (!param.StartsWith("http"))
            {
                
            }
        }










        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetValue<T>(ref T property, T value, string propertyName)
        {
            if (Object.Equals(property, value))
            {
                return false;
            }

            property = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.server != null)
            {
                var server = this.server;
                this.server = null;
                this.Status = "Stopping internal HTTP server...";
                e.Cancel = true;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    server.Dispose();

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        this.Close();
                    });
                });
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    this.Load(file);
                }
            }
            else
            {
            }
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
        }
    }
}
