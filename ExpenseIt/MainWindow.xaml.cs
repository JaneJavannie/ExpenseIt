using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Net;
using System.Net.Http;
using System.IO;
using System.Security.Cryptography.X509Certificates;


namespace ExpenseIt
{
    
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private object lockObj = new object();
        List<ImgLoader> imagesLoaders = new List<ImgLoader>();
        private double barMax;
        public MainWindow()
        {
            InitializeComponent();
            AddimgLoader(0);
            AddimgLoader(1);
            AddimgLoader(2);
            barMax = progressBar.Maximum;
        }

        private void AddimgLoader(int column)
        {
            var imgLoader = new ImgLoader(this, column);
            imgLoader.Name = "imgLoader" + column;
            Grid.SetRow(imgLoader, 0);
            Grid.SetColumn(imgLoader, column);
            grid.Children.Add(imgLoader);
            imagesLoaders.Add(imgLoader);
        }

        private void btnLoadAll_Click(object sender, RoutedEventArgs e)
        {
            if(imagesLoaders.FirstOrDefault(x=>x.isLoading) == null)
            {
                foreach (var imagesLoader in imagesLoaders)
                {
                    imagesLoader.Start();
                }
            }
            else
            {
                MessageBox.Show("Дождитесь окончания загрузки других изображений.");
            }
           
        }

        public void UddateProgress()
        {
            lock (lockObj)
            {
                double totalByte = 0;
                double cur = 0;
                foreach (var imagesLoader in imagesLoaders)
                {
                    totalByte += imagesLoader.totalByte;
                    cur += imagesLoader.byteReceived;   
                }  
                if (totalByte > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        progressBar.Value = cur / totalByte * barMax;
                    });
                }
            }
        }

    }
}
