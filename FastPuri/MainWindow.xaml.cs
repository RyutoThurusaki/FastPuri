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
using Microsoft.VisualBasic.FileIO;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace FastPuri
{
    public partial class MainWindow : System.Windows.Window
    {
        string Defaultfilepath = null;
        string[] Filepaths = {"Dammy"};

        int FileOrder = 0;
        int OutlineSize = 5;

        bool isPainting = false;
        bool isOutline = false;
        bool isPointErace = false;

        BitmapImage MainImageBitmap = new BitmapImage();
        public MainWindow()
        {
            InitializeComponent();
            MainCanvas.ResizeEnabled = false;
        }

        private void ClicktoOpen(object sender, RoutedEventArgs e)
        {
            OpenFileDialog filedialog = new OpenFileDialog();
            filedialog.Filter = "ImageFIle (*.png/*.jpg/*.jpeg)|*.png;*.jpg;*.jpeg";
            filedialog.InitialDirectory = Defaultfilepath;

            if (filedialog.ShowDialog() == true)
            {
                Open_Savedialog(filedialog, 0, 0, true,true);
            }
        }

        private async void Open_Savedialog(FileDialog filedialog,int new_fileorder,int old_fileorder,bool isloadarray,bool isload)
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

                    Save_Image(Filepaths[old_fileorder]);
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
                Filepaths = System.IO.Directory.GetFiles(Defaultfilepath, extension, System.IO.SearchOption.TopDirectoryOnly);

                FileOrder = Array.IndexOf(Filepaths, filedialog.FileName);
            }else if(isloadarray == false && load == true)
            {
                FileOrder = new_fileorder;
            }

            //Image opening to canvas.
            if (load)
            {
                //Null check.
                if (System.IO.File.Exists(Filepaths[FileOrder]))
                {
                    OutlineCanvas.Strokes.Clear();
                    MainCanvas.Strokes.Clear();

                    BitmapImage btm = new BitmapImage();
                    LabelInfomation.Content = Filepaths[FileOrder];

                    using (FileStream str = File.OpenRead(Filepaths[FileOrder]))
                    {
                        btm.BeginInit();
                        btm.StreamSource = str;
                        btm.CacheOption = BitmapCacheOption.OnLoad;
                        btm.CreateOptions = BitmapCreateOptions.None;
                        btm.EndInit();
                        btm.Freeze();
                    }

                    MainImage.Source = btm;
                    MainImageBitmap = btm;

                    MainCanvas.Height = MainImageBitmap.Height;
                    MainCanvas.Width = MainImageBitmap.Width;

                    canvas.Height = MainImageBitmap.Height;
                    canvas.Width = MainImageBitmap.Width;
                }
                else
                {
                    MainImage.Source = null;
                }
            }
        }

        private void Save_Image(string Filepath)
        {
            FileSystem.DeleteFile(Filepath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

            var bitcache = MainImageBitmap;
            //var size = new Size(bitcache.Width, bitcache.Height);

            var renderbmp = new System.Drawing.Bitmap((int)bitcache.Width, (int)bitcache.Height);
            /*
            using (var os = new FileStream(Filepath, FileMode.Create))
            {
                Mat baseimg = new Mat(Filepath);
                Mat maincvs = OpenCvSharp.Extensions.BitmapConverter.ToMat(renderbmp);

                var addresult = new Mat();

                Cv2.Add(baseimg, maincvs, addresult);
                System.Drawing.Bitmap result = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(addresult);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(result));
                encoder.Save(os);
            }
            */
            isPainting = false;
            LabelInfomation.Foreground = Brushes.White;
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
                MessageBox.Show(hoge.ToString(), "StrokeCounts");
            }
        }

        private void Button_Prev_Click(object sender, RoutedEventArgs e)
        {
            if (FileOrder > 0)
            {
                Open_Savedialog(null, FileOrder - 1, FileOrder, false, true);
            }
            else
            {
                Open_Savedialog(null, Filepaths.Length - 1, FileOrder, false, true);
            }
        }

        private void Button_Next_Click(object sender, RoutedEventArgs e)
        {
            if (FileOrder < Filepaths.Length - 1)
            {
                Open_Savedialog(null, FileOrder + 1, FileOrder, false, true);
            }
            else
            {
                Open_Savedialog(null, 0, FileOrder, false, true);
            }
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
            if (isPointErace)
            {
                MainCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            }
            else
            {
                MainCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
            }
        }

        private void Mode_Pen_Click(object sender, RoutedEventArgs e)
        {
            MainCanvas.EditingMode = InkCanvasEditingMode.Ink;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save_Image(Filepaths[FileOrder]);
        }
    }
}
