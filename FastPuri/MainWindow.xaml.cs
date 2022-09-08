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
        string[] Filepaths = {"Dammy"};

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
                Open_Savedialog(filedialog, filedialog.FileName, true,true);
            }
        }
        private void Button_Prev_Click(object sender, RoutedEventArgs e)
        {
            if (FileOrder > 0)
            {
                FileOrderCache = FileOrder;
                FileOrder--;
                Open_Savedialog(null, Filepaths[FileOrder], false,true);
            }
            else
            {
                FileOrderCache = FileOrder;
                FileOrder = Filepaths.Length-1;
                Open_Savedialog(null, Filepaths[FileOrder], false, true);
            }
        }
        private void Button_Next_Click(object sender, RoutedEventArgs e)
        {
            if (FileOrder < Filepaths.Length-1)
            {
                FileOrderCache = FileOrder;
                FileOrder++;
                Open_Savedialog(null, Filepaths[FileOrder], false, true);
            }
            else
            {
                FileOrderCache = FileOrder;
                FileOrder = 0;
                Open_Savedialog(null, Filepaths[FileOrder],false, true);
            }
        }
        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Set paintingstatus.
            isPainting = true;
            LabelInfomation.Foreground = Brushes.Yellow;
        }
        private void Open_Savedialog(FileDialog filedialog,string filename,bool isloadarray,bool isload)
        {
            //Array load cansel.
            bool load = isload;

            if (isPainting)
            {
                MessageBoxResult result = MessageBox.Show("プリクラを保存しますか？", "SaveDialog", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    load = true;
                    isPainting = false;
                    LabelInfomation.Foreground = Brushes.White;

                    //image saveing...under maintenance...

                }
                else if (result == MessageBoxResult.No)
                {
                    load = true;
                    isPainting = false;
                    LabelInfomation.Foreground = Brushes.White;

                    MainCanvas.Strokes.Clear();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    load = false;
                    FileOrder = FileOrderCache;
                    //do not
                }
            }

            if (isloadarray == true && load == true)
            {
                // Save directory path.
                FileInfo fi = new FileInfo(filedialog.FileName);
                Defaultfilepath = fi.DirectoryName;

                //Get all files path.Find the index for the opening file.
                string extension = "*" + System.IO.Path.GetExtension(filedialog.FileName);
                Filepaths = System.IO.Directory.GetFiles(Defaultfilepath, extension, SearchOption.TopDirectoryOnly);

                FileOrder = Array.IndexOf(Filepaths, filedialog.FileName);

                FileOrderCache = FileOrder;
            }

            if (load)
            {
                //Null check.
                if (System.IO.File.Exists(filename))
                {
                    BitmapImage btm = new BitmapImage();
                    LabelInfomation.Content = filename;

                    using (FileStream str = File.OpenRead(filename))
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
                    //MainImage.Source = null;
                }
            }
        }
    }
}
