using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ExpenseIt
{
    /// <summary>
    /// Логика взаимодействия для ImgLoader.xaml
    /// </summary>
    public partial class ImgLoader : UserControl
    {
        const string URL_TEXT_DEF  = "Enter URL";

 
        private readonly BitmapImage defImg = new BitmapImage(new Uri(@"pack://application:,,,/Resources/1.jpg")); 
        private readonly int number;
        private MainWindow main;
        public ImgLoader(MainWindow main, int number)
        {
            this.main = main;
            InitializeComponent();
            tbURL.Text = URL_TEXT_DEF; 
            this.number = number;
            CanStart();
        }


        private WebClient webClient;
        public double byteReceived;
        public long totalByte = 0;
        private Object lockObj = new object(); 
        public volatile bool isLoading;
        private string fileName;

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void BntStartSetDisabled(bool val)
        {
            Dispatcher.Invoke(() => { btnStart.IsEnabled = val; });
        }

        private void BntStopSetDisabled(bool val)
        {
            Dispatcher.Invoke(() => { btnStop.IsEnabled = val; });
        }


        public void Start()
        {
            BntStartSetDisabled(false);          
            img.Source = defImg;
            new Task(() =>
            {
                BntStopSetDisabled(true);
                string uriStr = "";
                Dispatcher.Invoke(() =>
                {
                    uriStr = tbURL.Text;
                });


                Uri imgUrl = null;
                try
                {
                    imgUrl = new Uri(uriStr);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Форма № { number }. Неверный формат uri({ex.Message})");
                    CanStart();
                    return;
                }

 
                lock (lockObj)
                {
                    isLoading = true;
                    byteReceived = totalByte = 0;
                }
                webClient = new WebClient();
                webClient.DownloadFileCompleted += Completed;
                webClient.DownloadProgressChanged += ProgressChanged;
             
                fileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString() + Guid.NewGuid().ToString());
                try
                {
                    webClient.DownloadFileTaskAsync(imgUrl, fileName);
                }
                catch (Exception ex)
                {
                    webClient.Dispose();
                    CanStart();
                    MessageBox.Show($"Форма № { number }. Загрузка не удалась({ex.Message})");
                }

            }).Start();
        }


        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                totalByte = long.Parse(webClient.ResponseHeaders["Content-Length"]);
            }
            catch(Exception ex) {
                Stop();
                MessageBox.Show($"Ошибка: невозможно получить данные изображения({ ex.Message })");
            }
            byteReceived = e.BytesReceived;
            main.UddateProgress();
        }


        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            BitmapImage src;
            lock (lockObj)
            {
                isLoading = false;
                try
                {
                    src = new BitmapImage(new Uri(fileName));
                }
                catch (Exception)
                {
                    MessageBox.Show($"Форма № {number}. Неверный формат изображения.");
                    CanStart();
                    return;
                }
                src.Freeze();
                Thread.Sleep(50);
                Dispatcher.Invoke(() =>
                {
                    img.Source = src;
                    CanStart();
                });
            }
             
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void Stop()
        {
            webClient.CancelAsync();
            img.Source = defImg;
            isLoading = false;
            CanStart();
        }

        private void CanStart()
        {
            BntStartSetDisabled(true);
            BntStopSetDisabled(false);
        }
        private void tbURL_GotFocus(object sender, RoutedEventArgs e)
        {
            if(tbURL.Text == URL_TEXT_DEF)
            {
                tbURL.Text = "";
            }
        }
    }
}
