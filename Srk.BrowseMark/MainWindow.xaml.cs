// MarkDownBrowser
// Copyright 2013 SandRock
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

namespace Srk.BrowseMark
{
    using Microsoft.Win32;
    using Srk.BrowseMark.LocalHttpServer;
    using SrkToolkit.Mvvm.Commands;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using uhttpsharp;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string address;
        private string status;
        private ICommand goToPageCommand;
        private HttpServer server;

        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;

            this.Status = null;

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

        private void Load(string address)
        {
            var url = MdHandler.GetDocumentsUrl(address, this.server.Port);
            System.Diagnostics.Process.Start(url);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://github.com/sandrock/MarkDownBrowser"));
        }

        private void OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
            e.Handled = true;
        }
    }
}
