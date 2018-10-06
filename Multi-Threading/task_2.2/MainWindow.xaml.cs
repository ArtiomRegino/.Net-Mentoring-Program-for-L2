using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace task_2._2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private List<string> _resourses;
        private CancellationTokenSource _source;

        public MainWindow()
        {
            _resourses = new List<string>(); 
            InitializeComponent();
        }

        private void AddUrlToBtn_Click(object sender, RoutedEventArgs e)
        {
            _resourses.Add(UrlTextBox.Text);
            ResultTextBox.Text += $"{UrlTextBox.Text} --added to list for download.\n";
            UrlTextBox.Text = "";
        }
        
        private async void StartDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            _source = new CancellationTokenSource();

            try
            {
                await AccessWebAsync(_source.Token);
            }
            catch (OperationCanceledException)
            {
                ResultTextBox.Text += "Download canceled.\n";
            }
            catch (Exception)
            {
                ResultTextBox.Text += "Download faulted.\n";
            }
        }

        private void CancelDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            _source.Cancel();
        }

        private async Task AccessWebAsync(CancellationToken token)
        {
            HttpClient client = new HttpClient();

            foreach (var url in _resourses)
            {
                var response = await client.GetAsync(url, token);

                var bytePage = await response.Content.ReadAsByteArrayAsync();

                token.ThrowIfCancellationRequested();

                DisplayDownload(url, bytePage.Length);
            }
        }

        private void DisplayDownload(string url, int byteSize)
        {
                ResultTextBox.Text += $"{url} -- {byteSize} byte was downloaded.\n";
        }

    }
}
