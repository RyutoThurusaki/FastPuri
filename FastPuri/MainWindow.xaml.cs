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
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Image = System.Windows.Controls.Image;

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

        Stream maincvs_result = null;

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

        private void Open_Savedialog(FileDialog filedialog,int new_fileorder,int old_fileorder,bool isloadarray,bool isload)
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
                    LabelInfomation.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 173, 171, 189));

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
            BitmapImage btm = new BitmapImage();

            using (FileStream str = File.OpenRead(Filepath))
            {
                btm.BeginInit();
                btm.StreamSource = str;
                btm.CacheOption = BitmapCacheOption.OnLoad;
                btm.CreateOptions = BitmapCreateOptions.None;
                btm.EndInit();
                btm.Freeze();

                Mat image = BitmapConverter.ToMat(new Bitmap(btm.StreamSource));

                System.Drawing.Size printsize = new System.Drawing.Size((int)btm.Width, (int)btm.Height);
                System.Drawing.Size DPISize = new System.Drawing.Size((int)btm.DpiX, (int)btm.DpiY);

                Mat main = CanvastoMat(MainCanvas, printsize, DPISize);
                Mat outline = CanvastoMat(OutlineCanvas, printsize, DPISize);

                Mat penresult = new Mat();
                Mat result = new Mat();

                //Cv2.Add(outline, main, result);
                result = main + outline;
                //Cv2.Add(image, penresult, result);
                Bitmap tobitmap = BitmapConverter.ToBitmap(result, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                //FileSystem.DeleteFile(Filepath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                tobitmap.Save(Filepath + ".png");
            }


            /*
            using (var os = new FileStream(Filepath, FileMode.Create))
            {
                Mat main = CanvastoMat(MainCanvas, printsize, DPISize);
                Mat outline = CanvastoMat(OutlineCanvas, printsize, DPISize);

                OpenCvSharp.

                Mat result = null;
                Bitmap tobitmap = null;
                Cv2.Add(main, outline, result);
                BitmapConverter.ToBitmap(result,tobitmap);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(tobi));
                encoder.Save(os);
            }*/

            isPainting = false;
            LabelInfomation.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 173, 171, 189));
        }

        public Mat CanvastoMat(InkCanvas canvas, System.Drawing.Size imagesize, System.Drawing.Size imageDPIs)
        {
            Mat result = new Mat();

            using (var stream = new MemoryStream())
            {
                var renderbtm = new RenderTargetBitmap((int)imagesize.Width, (int)imagesize.Height, imageDPIs.Width, imageDPIs.Height, PixelFormats.Pbgra32);

                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderbtm));
                encoder.Save(stream);

                var btm = new System.Drawing.Bitmap(stream);
                result = BitmapConverter.ToMat(btm);
            }

            return result;
        }

        private async void MainCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!object.ReferenceEquals(sender, this))
            {
                //This function summon is mouse event other possibility.
                //if Mouse event only.
                LabelInfomation.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,249, 248, 113));
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

        private void ColorSelect_Click(object sender, RoutedEventArgs e)
        {
            var picker = new ColorSelect();
            picker.Show();
        }
    }
}
