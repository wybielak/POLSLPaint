using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace POLSLPaint
{
    /// <summary>
    /// Logika interakcji dla klasy ColorEditorWindow.xaml
    /// </summary>
    public partial class ColorEditorWindow : Window
    {
        public ColorEditorWindow()
        {
            InitializeComponent();
            colorPreview.Background = new SolidColorBrush(ColorStore.MAIN_COLOR);

            int rVal = (int)ColorStore.MAIN_COLOR.R;
            int gVal = (int)ColorStore.MAIN_COLOR.G;
            int bVal = (int)ColorStore.MAIN_COLOR.B;
            int aVal = (int)ColorStore.MAIN_COLOR.A;

            Rinput.Text = (rVal).ToString();
            Ginput.Text = (gVal).ToString();
            Binput.Text = (bVal).ToString();
            alphaslider.Value = aVal;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(Rinput.Text, "^[0-9]")) Rinput.Text = "0";
            if (!System.Text.RegularExpressions.Regex.IsMatch(Ginput.Text, "^[0-9]")) Ginput.Text = "0";
            if (!System.Text.RegularExpressions.Regex.IsMatch(Binput.Text, "^[0-9]")) Binput.Text = "0";

            //if (String.IsNullOrEmpty(Rinput.Text)) Rinput.Text = "0";
            //if (String.IsNullOrEmpty(Ginput.Text)) Ginput.Text = "0";
            //if (String.IsNullOrEmpty(Binput.Text)) Binput.Text = "0";

            if (Convert.ToInt32(Rinput.Text) > 255) Rinput.Text = "255";
            if (Convert.ToInt32(Rinput.Text) < 0) Rinput.Text = "0";

            if (Convert.ToInt32(Ginput.Text) > 255) Ginput.Text = "255";
            if (Convert.ToInt32(Ginput.Text) < 0) Ginput.Text = "0";

            if (Convert.ToInt32(Binput.Text) > 255) Binput.Text = "255";
            if (Convert.ToInt32(Binput.Text) < 0) Binput.Text = "0";

            int rVal = Convert.ToInt32(Rinput.Text);
            int gVal = Convert.ToInt32(Ginput.Text);
            int bVal = Convert.ToInt32(Binput.Text);
            byte aVal = (byte)alphaslider.Value;

            double hVal = 0;
            double sVal;
            double vVal;

            double rPrim = (double)rVal / 255.0;
            double gPrim = (double)gVal / 255.0;
            double bPrim = (double)bVal / 255.0;

            double cMax = Math.Max(Math.Max(rPrim, gPrim), bPrim);
            double cMin = Math.Min(Math.Min(rPrim, gPrim), bPrim);

            double delta = cMax - cMin;

            vVal = cMax;

            if (cMax == 0)
            {
                sVal = 0;
            }

            else
            {
                sVal = delta / cMax;
            }

            if (delta == 0)
            { 
                hVal = 0;
            }

            else if (cMax == rPrim)
            {
                hVal = 60 * (((gPrim - bPrim) / delta) % 6) ;
            }

            else if (cMax == gPrim)
            {
                hVal = 60 * (((bPrim - rPrim) / delta) + 2);
            }

            else if (cMax == bPrim)
            {
                hVal = 60 * (((rPrim - gPrim) / delta) + 4);
            }

            //hVal = hVal * 100;
            sVal = sVal * 100;
            vVal = vVal * 100;

            Hinput.Text = (Math.Round(hVal,2)).ToString();
            Sinput.Text = (Math.Round(sVal, 2)).ToString();
            Vinput.Text = (Math.Round(vVal, 2)).ToString();


            ColorStore.MAIN_COLOR = System.Windows.Media.Color.FromArgb(aVal, (byte)rVal, (byte)gVal, (byte)bVal);

            colorPreview.Background = new SolidColorBrush(ColorStore.MAIN_COLOR);
        }
    }
}
