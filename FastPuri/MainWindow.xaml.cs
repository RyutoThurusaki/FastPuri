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
using System.Windows.Ink;

namespace FastPuri
{
    public partial class MainWindow : Window
    {
        string Defaultfilepath = null;
        string[] Filepaths = {"Dammy"};

        int FileOrder = 0;
        int OutlineSize = 5;

        bool isPainting = false;
        bool isOutline = false;

        DispatcherTimer updatetimer = null;
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
            updatetimer.Interval = new TimeSpan(0,0,0);
            updatetimer.Tick += (e, s) => { Debug_Update(); };

            updatetimer.Start();
        }

        public void Debug_Update()
        {

        }*/

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
                FileOrder--;
                Open_Savedialog(null, Filepaths[FileOrder], false,true);
            }
            else
            {
                FileOrder = Filepaths.Length-1;
                Open_Savedialog(null, Filepaths[FileOrder], false, true);
            }
        }

        private void Button_Next_Click(object sender, RoutedEventArgs e)
        {
            if (FileOrder < Filepaths.Length-1)
            {
                FileOrder++;
                Open_Savedialog(null, Filepaths[FileOrder], false, true);
            }
            else
            {
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
            //Array load cancel.
            bool load = isload;

            //Ask if want to save image.
            if (isPainting)
            {
                MessageBoxResult result = MessageBox.Show("プリクラを保存しますか？", "SaveDialog", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    load = true;

                    Save_Image();
                }
                else if (result == MessageBoxResult.No)
                {
                    load = true;
                    isPainting = false;
                    LabelInfomation.Foreground = Brushes.White;

                    OutlineCanvas.Strokes.Clear();
                    MainCanvas.Strokes.Clear();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    load = false;
                    //do not
                }
            }

            //Reload file pathes in array.
            if (isloadarray == true && load == true)
            {
                // Save directory path.
                FileInfo fi = new FileInfo(filedialog.FileName);
                Defaultfilepath = fi.DirectoryName;

                //Get all files path.Find the index for the opening file.
                string extension = "*" + System.IO.Path.GetExtension(filedialog.FileName);
                Filepaths = System.IO.Directory.GetFiles(Defaultfilepath, extension, SearchOption.TopDirectoryOnly);

                FileOrder = Array.IndexOf(Filepaths, filedialog.FileName);
            }

            //Image opening to canvas.
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
                    MainImage.Source = null;
                }
            }
        }

        private void Save_Image()
        {

            OutlineCanvas.Strokes.Clear();
            MainCanvas.Strokes.Clear();

            isPainting = false;
            LabelInfomation.Foreground = Brushes.White;
        }

        private void OutlineEnable_Click(object sender, RoutedEventArgs e)
        {
            isOutline = !isOutline;
            MainCanvas_PreviewMouseUp(this,null);

            if (!isOutline)
            {
                OutlineCanvas.Strokes.Clear();
            }
        }

        private void Mode_Eraser_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Mode_Pen_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void MainCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!object.ReferenceEquals(sender, this))
            {
                //This function summon is mouse event other possibility.
                //if Mouse event only.
                LabelInfomation.Foreground = Brushes.Yellow;
                isPainting = true;
            }

            await Task.Delay(1);

            if (isOutline)
            {
                DrawingAttributes inkda = new DrawingAttributes();

                inkda.Width = MainCanvas.DefaultDrawingAttributes.Width + OutlineSize;
                inkda.Height = MainCanvas.DefaultDrawingAttributes.Height + OutlineSize;

                inkda.Color = Colors.White;

                OutlineCanvas.Strokes = MainCanvas.Strokes.Clone();

                for (int i = 0; i < MainCanvas.Strokes.Count; i++)
                {
                    this.OutlineCanvas.Strokes[i].DrawingAttributes = inkda;
                }

                int hoge = MainCanvas.Strokes.Count;
                //MessageBox.Show(hoge.ToString(), "StrokeCounts");
            }
        }
    }
}
