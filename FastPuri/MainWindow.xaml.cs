using Microsoft.Win32;
using System;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

namespace FastPuri
{
    public partial class MainWindow : Window
    {
        string Defaultfilepath = null;
        string[] Filepaths = null;

        int FileOrder = 0;
        int FileOrderCache = 0;

        bool isPainting = false;

        //DispatcherTimer updatetimer = null;
        public MainWindow()
        {
            InitializeComponent();
            MainCanvas.ResizeEnabled = false;
            //AddUpdate();
        }
        /*
        public void AddUpdate()
        {
            updatetimer = new DispatcherTimer(DispatcherPriority.Background);
            updatetimer.Interval = new TimeSpan(0, 0, 1);
            updatetimer.Tick += (e, s) => { Debug_Update(); };

            updatetimer.Start();
        }
        public void Debug_Update()
        {
            LabelInfomation_1.Content = MainCanvas.Height;
        }
        */
        private void ClicktoOpen(object sender, RoutedEventArgs e)
        {
            OpenFileDialog filedialog = new OpenFileDialog();
            filedialog.Filter = "ImageFIle (*.png/*.jpg/*.jpeg)|*.png;*.jpg;*.jpeg";
            filedialog.InitialDirectory = Defaultfilepath;

            if (filedialog.ShowDialog() == true)
            {
                SelectImage(filedialog.FileName);

                // Save directory path.
                FileInfo fi = new FileInfo(filedialog.FileName);
                Defaultfilepath = fi.DirectoryName;

                //Get all files path.Find the index for the opening file.
                string extension = "*" + System.IO.Path.GetExtension(filedialog.FileName);
                Filepaths = System.IO.Directory.GetFiles(Defaultfilepath,extension , SearchOption.TopDirectoryOnly) ;

                FileOrder = Array.IndexOf(Filepaths, filedialog.FileName);

                FileOrderCache = FileOrder;
            }
        }
        private void Button_Prev_Click(object sender, RoutedEventArgs e)
        {
            if (FileOrder > 0)
            {
                FileOrderCache = FileOrder;
                FileOrder--;
                SelectImage(Filepaths[FileOrder]);
            }
            else
            {
                FileOrderCache = FileOrder;
                FileOrder = Filepaths.Length-1;
                SelectImage(Filepaths[FileOrder]);
            }
        }
        private void Button_Next_Click(object sender, RoutedEventArgs e)
        {
            if (FileOrder < Filepaths.Length-1)
            {
                FileOrderCache = FileOrder;
                FileOrder++;
                SelectImage(Filepaths[FileOrder]);
            }
            else
            {
                FileOrderCache = FileOrder;
                FileOrder = 0;
                SelectImage(Filepaths[FileOrder]);
            }
        }
        private void SelectImage(string FileName)
        {
            //Ask if want save.
            if (isPainting)
            {
                MessageBoxResult result = MessageBox.Show("プリクラを保存しますか？","SaveDialog",MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if(result == MessageBoxResult.Yes)
                {
                    LoadImage(FileName);
                    //image saveing...under maintenance...
                }
                else if(result == MessageBoxResult.No)
                {
                    LoadImage(FileName);
                    MainCanvas.Strokes.Clear();
                }
                else if(result == MessageBoxResult.Cancel)
                {
                    //do not
                    FileOrder = FileOrderCache;
                }
            }
            else
            {
                LoadImage(FileName);
            }
        }
        private void LoadImage(string FileName)
        {
            //Null check.
            if (System.IO.File.Exists(FileName))
            {
                BitmapImage btm = new BitmapImage();
                LabelInfomation.Content = FileName;

                using (FileStream str = File.OpenRead(FileName))
                {
                    btm.BeginInit();
                    btm.StreamSource = str;
                    btm.CacheOption = BitmapCacheOption.OnLoad;
                    btm.CreateOptions = BitmapCreateOptions.None;
                    btm.EndInit();
                    btm.Freeze();
                }

                MainImage.Source = btm;

                MainCanvas.Height = btm.Height;
                MainCanvas.Width = btm.Width;

                canvas.Height = btm.Height;
                canvas.Width = btm.Width;
            }
            else
            {
                MainImage.Source = null;
            }

            isPainting = false;
            LabelInfomation.Foreground = Brushes.White;
        }
        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Set paintingstatus.
            isPainting = true;
            LabelInfomation.Foreground = Brushes.Yellow;
        }
    }
}
