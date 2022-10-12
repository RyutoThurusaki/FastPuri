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
using System.Drawing;
using System.Drawing.Imaging;
using Image = System.Windows.Controls.Image;
using Color = System.Windows.Media.Color;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.FileIO;
using System.Web;
using System.Drawing.Drawing2D;
using System.Windows.Media.Animation;
using System.Security.AccessControl;
using System.Web.UI.WebControls;

namespace FastPuri
{
    public partial class MainWindow : System.Windows.Window
    {
        //焼いて
        string Defaultfilepath = null;
        string[] Filepaths = { null };

        int FileOrder = 0;

        bool isPainting = false;
        bool isPointErace = false;
        bool isAllextention = true;

        public Color color_pen = Colors.Black;
        public Color color_outline = Colors.White;
        public int pensize;
        public int outlinesize;

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
                Open_Savedialog(filedialog, 0, 0, true, true);
            }
        }

        private void Open_Savedialog(FileDialog filedialog, int new_fileorder, int old_fileorder, bool isloadarray, bool isloadimage)
        {
            //Array load cancel.
            bool isload = isloadimage;

            //Ask if want to save image.
            if (isPainting)
            {
                MessageBoxResult result = MessageBox.Show("プリクラを保存しますか？", "SaveDialog", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    isload = true;

                    Save_Image(Filepaths[old_fileorder]);
                }
                else if (result == MessageBoxResult.No)
                {
                    isload = true;
                    isPainting = false;
                    LabelInfomation.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 173, 171, 189));

                    OutlineCanvas.Strokes.Clear();
                    MainCanvas.Strokes.Clear();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    isload = false;
                    //do not
                }
            }

            //Reload file pathes in array.
            if (isloadarray == true && isload == true)
            {
                // Save directory path.
                FileInfo fi = new FileInfo(filedialog.FileName);
                Defaultfilepath = fi.DirectoryName;

                //Get all files path.Find the index for the opening file.
                if (isAllextention)
                {
                    string[] src_jpg = System.IO.Directory.GetFiles(Defaultfilepath, "*.jpg", System.IO.SearchOption.TopDirectoryOnly);
                    string[] src_jpeg = System.IO.Directory.GetFiles(Defaultfilepath, "*.jpeg", System.IO.SearchOption.TopDirectoryOnly);
                    string[] src_png = System.IO.Directory.GetFiles(Defaultfilepath, "*.png", System.IO.SearchOption.TopDirectoryOnly);

                    string[] dst = new string[src_jpg.Length + src_jpeg.Length + src_png.Length];

                    src_jpg.CopyTo(dst, 0);
                    src_jpeg.CopyTo(dst, src_jpg.Length);
                    src_png.CopyTo(dst,src_jpg.Length+src_jpeg.Length);

                    Filepaths = dst;
                }
                else
                {
                    string extension = "*" + System.IO.Path.GetExtension(filedialog.FileName);
                    Filepaths = System.IO.Directory.GetFiles(Defaultfilepath, extension, System.IO.SearchOption.TopDirectoryOnly);
                }

                FileOrder = Array.IndexOf(Filepaths, filedialog.FileName);
            }
            else if (isloadarray == false && isload == true)
            {
                FileOrder = new_fileorder;
            }

            //Image opening to canvas.
            if (isload)
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


                    //Canvas change scale.
                    Rect ViewRect = new Rect();
                    ViewRect.Width = ScrollViewer.ActualWidth;
                    ViewRect.Height = ScrollViewer.ActualHeight;

                    Rect ImageRect = new Rect();
                    ImageRect.Width = MainImageBitmap.Width;
                    ImageRect.Height = MainImageBitmap.Height;

                    Rect ResultRect = new Rect();
                    ResultRect.Width = ViewRect.Width / ImageRect.Width;
                    ResultRect.Height = ViewRect.Height / ImageRect.Height;

                    float Scale = 0;

                    switch (ResultRect.Width.CompareTo(ResultRect.Height))
                    {
                        case 0:
                            Scale = (float)ResultRect.Height;
                            break;
                        case 1:
                            Scale = (float)ResultRect.Height;
                            break;
                        case -1:
                            Scale = (float)ResultRect.Width;
                            break ;
                    }

                    slider.Value = Scale;
                }
                else
                {
                    MainImage.Source = null;
                }
            }
        }

        private void Save_Image(string Filepath)
        {
            ShowInfomation("セーブ中…");

            if (File.Exists(Filepath))
            {
                Bitmap image;

                using (FileStream str = new FileStream(Filepath, FileMode.Open, FileAccess.Read))
                {
                    image = new Bitmap(str);

                    str.Close();
                }

                FileSystem.DeleteFile(Filepath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                using (FileStream str = new FileStream(Filepath, FileMode.Create,FileAccess.ReadWrite))
                {
                    //Apply print size setting.
                    System.Drawing.Size printsize = new System.Drawing.Size((int)MainImageBitmap.Width, (int)MainImageBitmap.Height);
                    System.Drawing.Size DPISize = new System.Drawing.Size((int)MainImageBitmap.DpiX, (int)MainImageBitmap.DpiY);

                    Bitmap main = Canvas2Bitmap(MainCanvas, printsize, DPISize);
                    Bitmap outline = Canvas2Bitmap(OutlineCanvas, printsize, DPISize);

                    Graphics g = Graphics.FromImage(image);
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                    System.Drawing.Color color = System.Drawing.Color.FromArgb(0,0,0,0);

                    //rendering
                    outline.MakeTransparent(color);
                    main.MakeTransparent(color);
                    g.DrawImage(outline, 0,0);
                    g.DrawImage(main,0,0);
                    g.Save();

                    //A choose format from extention
                    ImageFormat format = null;

                    switch (System.IO.Path.GetExtension(Filepath))
                    {
                        case ".jpg":
                            format = ImageFormat.Jpeg;
                            break;
                        case ".jpeg":
                            format = ImageFormat.Jpeg;
                            break;
                        case ".png":
                            format = ImageFormat.Png;
                            break;
                    }

                    Bitmap dst = new Bitmap(image);
                    dst.Save(str, format);

                    isPainting = false;
                    LabelInfomation.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 173, 171, 189));

                    str.Close();
                }

                BitmapImage btm = new BitmapImage();

                using (FileStream str = File.OpenRead(Filepath))
                {
                    btm.BeginInit();
                    btm.StreamSource = str;
                    btm.CacheOption = BitmapCacheOption.OnLoad;
                    btm.CreateOptions = BitmapCreateOptions.None;
                    btm.EndInit();
                    btm.Freeze();
                }

                ShowInfomation("セーブしました");
                MainImage.Source = btm;
            }
            else
            {
                ShowInfomation("セーブに失敗しました");
            }
        }

        public Bitmap Canvas2Bitmap(InkCanvas canvas, System.Drawing.Size imagesize, System.Drawing.Size imageDPIs)
        {
            Bitmap result;
            MemoryStream str = new MemoryStream();

            using (var stream = new MemoryStream())
            {
                var renderbtm = new RenderTargetBitmap
                    ((int)imagesize.Width, (int)imagesize.Height, imageDPIs.Width, imageDPIs.Height, PixelFormats.Pbgra32);


                BitmapEncoder encoder = new BmpBitmapEncoder();
                renderbtm.Render(canvas);

                encoder.Frames.Add(BitmapFrame.Create(renderbtm));
                encoder.Save(stream);

                result = new Bitmap(stream);
            }

            canvas.Strokes.Clear();

            return result;
        }

        private async void MainCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!object.ReferenceEquals(sender, this))
            {
                //This function summon is mouse event other possibility.
                //if Mouse event only.
                LabelInfomation.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 249, 248, 113));
                isPainting = true;
            }

            await Task.Delay(1);

            if (outlinesize != 0)
            {
                OutlineCanvas.Strokes = MainCanvas.Strokes.Clone();

                for (int i = 0; i < OutlineCanvas.Strokes.Count; i++)
                {
                    OutlineCanvas.Strokes[i].DrawingAttributes.Width = OutlineCanvas.Strokes[i].DrawingAttributes.Width + outlinesize;
                    OutlineCanvas.Strokes[i].DrawingAttributes.Height = OutlineCanvas.Strokes[i].DrawingAttributes.Height + outlinesize;

                    OutlineCanvas.Strokes[i].DrawingAttributes.Color = color_outline;
                }
                //本当は以前の配列と比較して増えた分に対してのみDA.Setしたい
                //PointEraserで消したとき増えた分の処理がむずい
                //現状だとifでfalseなやつはそのままの色に成るから太さが変わって見える、意図しない挙動
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

        private void Mode_Eraser_Click(object sender, RoutedEventArgs e)
        {
            isPointErace = !isPointErace;

            if (isPointErace)
            {
                MainCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                ShowInfomation("ポイント消しゴム");
            }
            else
            {
                MainCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                ShowInfomation("ストローク消しゴム");
            }
        }

        private void Mode_Pen_Click(object sender, RoutedEventArgs e)
        {
            MainCanvas.EditingMode = InkCanvasEditingMode.Ink;

            ShowInfomation("ペン");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save_Image(Filepaths[FileOrder]);

            Open_Savedialog(null, FileOrder, FileOrder, false, true);
        }

        private void ColorSelect_Click(object sender, RoutedEventArgs e)
        {
            var picker = new ColorSelect(this);
            picker.ShowDialog();

            if ((Boolean)picker.DialogResult)
            {
                var DDA = MainCanvas.DefaultDrawingAttributes;
                DDA.Width = pensize;
                DDA.Height = pensize;
                DDA.Color = color_pen;

                Preview_Pen.Fill = new SolidColorBrush(color_pen);
                Preview_Pen.Stroke = new SolidColorBrush(color_outline);

                MainCanvas_PreviewMouseUp(this, null);
            }
            else
            {
                //Do not
            }
        }

        private void ShowInfomation(string message)
        {
            Popup_Text.Content = message;
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 1;
            animation.To = 0;
            animation.Duration = new Duration(TimeSpan.FromSeconds(3));

            Popup_Infomation.BeginAnimation(System.Windows.Controls.Button.OpacityProperty, animation);
        }
    }
}
