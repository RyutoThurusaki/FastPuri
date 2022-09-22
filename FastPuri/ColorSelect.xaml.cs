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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FastPuri
{
    public partial class ColorSelect : Window
    {
        Button SelectButton = null;
        MainWindow main = null;

        DispatcherTimer timer = new DispatcherTimer();
        public ColorSelect(MainWindow mainwindow)
        {
            InitializeComponent();

            main = mainwindow;

            timer.Interval = new TimeSpan(5);
            timer.Tick += new EventHandler(Palette_Update);
            timer.Start();

            //Current pen setting into variable.
            Button_PenColor.Background = new SolidColorBrush(main.color_pen);
            Button_OutlineColor.Background = new SolidColorBrush(main.color_outline);
            Slider_Pen.Value = main.pensize;
            Slider_Outline.Value = main.outlinesize;

            Button_PenColor_Click(null, null);
        }

        private void Palette_Update(object sender, EventArgs e)
        {
            //ColorPalette update
            if (SelectButton != null)
            {
                var mybrush = new SolidColorBrush(ColorPicker.Color);
                SelectButton.Background = mybrush;
            }
        }

        private void Button_PenColor_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker.Color = (Color)ColorConverter.ConvertFromString(Button_PenColor.Background.ToString());
            SelectButton = Button_PenColor;
        }

        private void Button_OutlineColor_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker.Color = (Color)ColorConverter.ConvertFromString(Button_OutlineColor.Background.ToString());
            SelectButton = Button_OutlineColor;
        }

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            main.color_pen = (Color)ColorConverter.ConvertFromString(Button_PenColor.Background.ToString());
            main.color_outline = (Color)ColorConverter.ConvertFromString(Button_OutlineColor.Background.ToString());
            main.pensize = (int)Slider_Pen.Value;
            main.outlinesize = (int)Slider_Outline.Value;

            this.DialogResult = true;
            this.Close();
        }

        void ColorSelect_Closing(object sender, ConsoleCancelEventArgs e)
        {
            timer.Tick -= new EventHandler(Palette_Update);
            timer.Stop();
        }
    }
}
