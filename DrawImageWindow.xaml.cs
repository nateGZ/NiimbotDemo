using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Runtime.InteropServices;
using System;
using System.Windows.Media.Imaging;

namespace JCSDKExample
{

    /// <summary>
    /// DrawImageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DrawImageWindow : Window
    {
        public delegate void PrintResultHandler(bool printResult); 
        public PrintResultHandler printResultHandler;

        public int printLabelType = 3;
        public int printDensity = 3;
        public int printMode = 1;
        public int printCount = 1;

        int paperHeight = 20;

        const float itemHeight = 20;
        //const float leftPadding = 5;
        //const float topBottomPadding = 5;
        const float leftPadding = 1;
        const float topBottomPadding = 1;
        private float height = 0; // 已组合的所有元素的高度

        public DrawImageWindow()
        {
            InitializeComponent();

            // Initialize the drawing board and set the height. Areas exceeding the settings will not be drawn.
            Printer.InitDrawingBoard(50, paperHeight, 0, "ZT001.ttf", 0, 0);
        }

        private void PaperHeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                paperHeight = int.Parse(PaperHeightTextBox.Text);
            }
            catch
            {
                paperHeight = 20;
            }
            finally
            {
                PaperHeightTextBox.Text = paperHeight.ToString();
            }

            Printer.InitDrawingBoard(50, paperHeight, 0, "ZT001.ttf", 0, 0);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            Print();
        }

        private void Print()
        {
            string msg = "";
            bool printResult = false;
            if (height > 0)
            {
                bool startJobResult;
                // 设置打印参数
                startJobResult = Printer.startJob(printLabelType, printDensity, printMode, printCount);
                if (startJobResult)
                {
                    printResult = Printer.commitJob();
                    if (printResult)
                    {
                        //this.Hide();
                        msg = "Printing completed";
                    }
                    else
                    {
                        msg = string.Format("commitJob error, error code：{0}", Printer.getPrintLastError());
                    }
                }
                else
                {
                    msg = string.Format("startJob error, error code：{0}", Printer.getPrintLastError());
                }

                Printer.endJob();

                printResultHandler(printResult);
            }

            MessageBox.Show(msg);
            this.Close();
        }

        private void Draw_Label_Click(object sender, RoutedEventArgs e)
        {
            //string str = File.ReadAllText(@"text.txt");
            string str = "MKT-10001";
            //  Printer.DrawLableText(leftPadding, height + topBottomPadding, 50, 25, Encoding.UTF8.GetBytes(str + (char)0), "ZT001.ttf", 4, 0, 1, 1, 2, 0, 0, new byte[] { 0, 0, 0, 0 });
            Printer.DrawLableText(leftPadding, height + 10, 50, 25, Encoding.UTF8.GetBytes(str + (char)0), "ZT001.ttf", 6, 0, 1, 1, 2, 0, 0, new byte[] { 0, 0, 0, 0 });
            height += (topBottomPadding + 20);

            Print();
        }

        private void Draw_BarCode_Click(object sender, RoutedEventArgs e)
        {
            Printer.DrawLableBarCode(leftPadding, height + topBottomPadding, itemHeight, itemHeight, 20, Encoding.UTF8.GetBytes("12345678" + (char)0), 4, 0, 5, 0);
            height += (itemHeight + topBottomPadding);
        }

        private void Draw_QrCode_Click(object sender, RoutedEventArgs e)
        {
            bool result = Printer.DrawLableQrCode(leftPadding, height + topBottomPadding, itemHeight, itemHeight, Encoding.UTF8.GetBytes("精臣智慧标识" + (char)0), 31, 0);
            height += (itemHeight + topBottomPadding);
        }

        private void Draw_Line_Click(object sender, RoutedEventArgs e)
        {
            Printer.DrawLableLine(0, height + topBottomPadding, itemHeight, 2, 0, 1, new float[] { 4, 2 });
            height += (itemHeight + topBottomPadding);
        }

        private void Draw_Graphics_Click(object sender, RoutedEventArgs e)
        {
            Printer.DrawLableGraph(leftPadding, height + topBottomPadding, itemHeight, itemHeight, 1, 0, 0, 1, 1, new float[] { 2, 5 });
            height += (itemHeight + topBottomPadding);
        }

        private void Draw_Image_Click(object sender, RoutedEventArgs e)
        {
            string imageData = File.ReadAllText("TestDrawImage.txt");
            Printer.DrawLableImage(imageData, leftPadding, height + topBottomPadding, itemHeight, itemHeight, 0, 1, 127);
            height += (itemHeight + topBottomPadding);
        }

        private void Preview_Image_Click(object sender, RoutedEventArgs e)
        {
            float displayMultiple = 8.0f;
            IntPtr previewImageP = Printer.GeneratePrintPreviewImage(displayMultiple);
            string previewImageStr = Marshal.PtrToStringAnsi(previewImageP);
            Printer.FreeBuffer(previewImageP);
            byte[] imageData = Convert.FromBase64String(previewImageStr);
            //读入MemoryStream对象
            MemoryStream memoryStream = new MemoryStream(imageData, 0, imageData.Length);
            memoryStream.Write(imageData, 0, imageData.Length);
            //转成图片
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(imageData);
            image.EndInit();

            previewImageShow.Source = image;
            image.Freeze();
        }
    }
}
