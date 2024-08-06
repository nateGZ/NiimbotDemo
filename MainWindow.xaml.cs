using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
//using System.Windows.Forms;

namespace JCSDKExample
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool isSDKInit;
        public bool isConnectedFlag;

        private string printImagePath;
        private string[] printImagesPath = { };

        int printLabelType = 1; // 标签类型
        int printDensity = 1; // 打印浓度
        int printMode = 1; // 打印模式（热敏、热转印） 
        int printCount = 1; // 打印份数 

        string connectedPrintInfo = "";
        private System.Threading.Timer printerStatusTimer;

        //string-打印机名和端口好（K1S-E818013369：9100）,int-设备类型（1-USB，2-Wifi）
        Dictionary<string, int> printers = new Dictionary<string, int>();

        private bool isPrinting = false;    //仅打印机空闲时才上报心跳，否则会报错
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();    //定时器，控制子线程从阻塞中恢复
        AutoResetEvent autoEvent = new AutoResetEvent(false);   //阻塞变量， autoEvent.WaitOne()-阻塞，autoEvent.Set()-恢复 
        SynchronizationContext SyncContext = null;  //子线程更新主线程UI的  
        private bool quitCheckPrinterOnline = false;    //子线程退出标志

        private void checkPrinterStatusTimer()
        {
            /*if (printerStatusTimer == null)
            {
                // 每3s检查一次打印机状态
                printerStatusTimer = new System.Threading.Timer(new System.Threading.TimerCallback(checkPrinterStatus), null, 0, 3000);
            }*/
            SyncContext = SynchronizationContext.Current;

            timer.Interval = 2000;
            timer.Tick += new EventHandler(tm_Tick);

            timer.Start();
            Thread t = new Thread(checkPrinterOnlie);
            t.Start();
        }

        void tm_Tick(object sender, EventArgs e)
        {
            //定时器超时执行函数
            autoEvent.Set();
        }

        void dealConnectFial(Object state)
        {
            //主线程提示设备断连
            printers.Remove(connectedPrintInfo);
            connectedPrintInfo = "";
            isConnectedFlag = false;
            SelectPrinterCB.Items.Clear();
            MessageBox.Show("Printer disconnected！");
            Printer.disconnectPrinter();
        }
        public void checkPrinterOnlie()
        {
            while (true)
            {
                if (quitCheckPrinterOnline)
                {
                    timer.Stop();
                    quitCheckPrinterOnline = false;
                    return;
                }

                if (isPrinting)
                {
                    Thread.Sleep(100);
                    continue;
                    //return;
                }

                if (!isSDKInit || !isConnectedFlag)
                {
                    Thread.Sleep(100);
                    continue;
                    //return;
                }

                // 打印机连接状态下监测断连状态
                bool checkPrinterOnline = Printer.isConnected();

                if (!checkPrinterOnline)
                {
                    SyncContext.Post(dealConnectFial, "");
                }

                autoEvent.WaitOne();
            }
        }
        /*public void checkPrinterStatus(object o)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (isPrinting)
                {
                    return;
                }

                if (!isSDKInit || !isConnectedFlag)
                {
                    return;
                }

                // 打印机连接状态下监测断连状态
                bool checkPrinterOnline = Printer.isConnected();

                if (!checkPrinterOnline)
                {
                    printers.Remove(connectedPrintInfo);
                    connectedPrintInfo = "";
                    isConnectedFlag = false;
                    SelectPrinterCB.Items.Clear();
                    MessageBox.Show("打印机连接断开！");
                    Printer.disconnectPrinter();
                }
            }));
        }*/

        private void refreshImages()
        {
            printImagesPath = Directory.GetFiles("TestImages", "*.txt", SearchOption.AllDirectories);
        }

        private void DrawPagePrintResultHandler(bool result)
        {
            if (!result)
            {
                isConnectedFlag = false;
                SelectPrinterCB.Items.Clear();
                SelectPrinterCB.SelectedIndex = -1;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            // 选择图片数据源
            refreshImages();
            foreach (var item in printImagesPath)
            {
                SelectPrintImageCB.Items.Add(item);

                // 默认选择第一个
                SelectPrintImageCB.SelectedIndex = 0;
            }

            // 选择打印浓度数据源
            for (int index = 1; index < 6; index++)
            {
                PrintDensityCB.Items.Add(index);
            }
            PrintDensityCB.SelectedIndex = printDensity - 1;

            // 选择纸张类型数据源
            LabelTypeCB.Items.Add("gap paper");
            LabelTypeCB.Items.Add("⿊standard paper");
            LabelTypeCB.Items.Add("continuous paper");
            LabelTypeCB.Items.Add("via hole paper");
            LabelTypeCB.Items.Add("Transparent paper");
            LabelTypeCB.SelectedIndex = printLabelType - 1;

            // Select print type data source
            PrintModeCB.Items.Add("Thermal sensitive");
            PrintModeCB.Items.Add("Thermal transfer");
            PrintModeCB.SelectedIndex = printMode - 1;

            // 初始化打印
            isSDKInit = Printer.initSdk("./font");

            checkPrinterStatusTimer();
        }

        private void GetAllPrintersBTN_Click(object sender, RoutedEventArgs e)
        {
            isConnectedFlag = false;
            timer.Stop();
            if (!isSDKInit)
            {
                MessageBox.Show("SDK initialization failed");
                return;
            }

            SelectPrinterCB.Items.Clear();
            printers.Clear();

            IntPtr usbPrintersP = Printer.getAllPrinters();
            string usbPrintersJson = Marshal.PtrToStringAnsi(usbPrintersP);
            Printer.FreeBuffer(usbPrintersP);
            IntPtr wifiPrintersP = Printer.scanWifiPrinter();
            string wifiPrintersJson = Marshal.PtrToStringAnsi(wifiPrintersP);
            Printer.FreeBuffer(wifiPrintersP);

            if (usbPrintersJson == null && wifiPrintersJson == null)
            {
                MessageBox.Show("Printer not found");
                return;
            }

            if (usbPrintersJson != null)
            {
                JObject jUSB = (JObject)JsonConvert.DeserializeObject(usbPrintersJson);
                foreach (var item in jUSB)
                {
                    string name = item.Key;
                    string value = item.Value.ToString();
                    string printerName = name + ":" + value;
                    if (!printers.ContainsKey(printerName))
                        printers.Add(printerName, 1);
                }
            }

            if (wifiPrintersJson != null)
            {
                JArray jWIFI = (JArray)JsonConvert.DeserializeObject(wifiPrintersJson);
                foreach (JObject jo in jWIFI)
                {
                    string printerName = jo["deviceName"].ToString() + ":" + jo["tcpPort"].ToString();
                    if (!printers.ContainsKey(printerName))
                        printers.Add(printerName, 2);
                }
            }

            foreach (var printInfo in printers)
            {
                SelectPrinterCB.Items.Add(printInfo.Key);

                // 如果搜索到打印机，默认选择第一个
                SelectPrinterCB.SelectedIndex = 0;
            }

        }

        private void SelectPrinterCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isConnectedFlag = false;
            timer.Stop();
            if (SelectPrinterCB.SelectedItem == null) return;
            int index = 0;
            string printInfo = SelectPrinterCB.SelectedItem.ToString();
            string[] infos = printInfo.Split(':');
            if (infos.Length < 1) return; // error
            string printerName = infos[0];
            int printerPort = int.Parse(infos[1]);
            foreach (var printer in printers)
            {
                if (printInfo == printer.Key)
                {
                    index = printer.Value;
                }
            }

            if (index == 1)
            {
                isConnectedFlag = Printer.selectPrinter(printerName, printerPort);
            }
            else if (index == 2)
            {
                isConnectedFlag = Printer.connectWifiPrinter(printerName, printerPort);
            }

            if (isConnectedFlag)
            {
                connectedPrintInfo = SelectPrinterCB.SelectedItem.ToString();
                timer.Start();
            }
        }

        private void SelectPrintImageCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectPrintImageCB.SelectedItem != null)
            {
                printImagePath = SelectPrintImageCB.SelectedItem.ToString();
            }
        }

        private void RefreshImagesDataBTN_Click(object sender, RoutedEventArgs e)
        {
            SelectPrintImageCB.Items.Clear();
            refreshImages();
            foreach (var item in printImagesPath)
            {
                SelectPrintImageCB.Items.Add(item);

                // The first one is selected by default
                SelectPrintImageCB.SelectedIndex = 0;
            }

            if (printImagesPath.Length == 0)
            {
                MessageBox.Show("No image data");
            }
        }

        private void CustomDrawBTN_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnectedFlag)
            {
                MessageBox.Show("\r\nPlease connect the printer first");
                return;
            }
            DrawImageWindow drawImageWindow = new DrawImageWindow();
            drawImageWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            drawImageWindow.printResultHandler = DrawPagePrintResultHandler;
            drawImageWindow.printCount = printCount;
            drawImageWindow.printDensity = printDensity;
            drawImageWindow.printLabelType = printLabelType;
            drawImageWindow.printMode = printMode;

            drawImageWindow.Left = this.Left;
            drawImageWindow.Top = this.Top;
            drawImageWindow.ShowDialog();
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            isPrinting = true;
            Console.WriteLine("Click print button time：{0}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));
            string msg = "";
            if (!isConnectedFlag || printImagePath == null)
            {
                msg = "Please select a printer and print data";
            }

            string imageData;
            try
            {
                imageData = File.ReadAllText(printImagePath);
                if (imageData == null) return;
            }
            catch
            {
                return;
            }

            bool startJobResult, printResult;
            // 设置打印参数
            startJobResult = Printer.startJob(printLabelType, printDensity, printMode, printCount);
            if (startJobResult)
            {
                // 发送打印数据，开始打印
                printResult = Printer.picturePrint(Marshal.StringToHGlobalAnsi(imageData), Convert.ToUInt32(imageData.Length), 0, 0, 127);
                if (!printResult)
                {
                    msg = string.Format("picturePrint Something went wrong, error code：{0}", Printer.getPrintLastError());
                    //SelectPrinterCB.Items.Clear();
                    //SelectPrinterCB.SelectedIndex = -1;
                }
                else
                {
                    msg = "Print successfully";
                }
                Printer.endJob();
            }
            else
            {
                msg = string.Format("startJob Something went wrong, error code：{0}", Printer.getPrintLastError());
                //SelectPrinterCB.Items.Clear();
                //SelectPrinterCB.SelectedIndex = -1;
            }
            isPrinting = false;
            MessageBox.Show(this, msg);
        }

        private void DisconnectBTN_Click(object sender, RoutedEventArgs e)
        {
            Printer.disconnectPrinter();

            isConnectedFlag = false;
            timer.Stop();

            SelectPrinterCB.Items.Clear();
            SelectPrinterCB.SelectedIndex = -1;
        }

        private void PrintCout_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                printCount = int.Parse(PrintPageTextBox.Text);
            }
            catch
            {
                printCount = 1;
            }
            finally
            {
                PrintPageTextBox.Text = printCount.ToString();
            }
        }

        private void PrintDensityCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            printDensity = PrintDensityCB.SelectedIndex + 1;
        }

        private void LabelTypeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            printLabelType = LabelTypeCB.SelectedIndex + 1;
        }

        private void PrintModeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            printMode = PrintModeCB.SelectedIndex + 1;
        }

        private void configureWifi_Click(object sender, RoutedEventArgs e)
        {
            isPrinting = true;
            string strWifiName = wifiName.Text;
            string strWifiPassWord = wifiPassWord.Text;
            bool ret = Printer.configurationWifi(strWifiName, strWifiPassWord);
            if (ret)
            {
                MessageBox.Show("Network configuration successful");
            }
            else
            {
                MessageBox.Show("Failed to configure network");
            }
            isPrinting = false;
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            quitCheckPrinterOnline = true;
            autoEvent.Set();

            while (quitCheckPrinterOnline)  //Ensure that when the program exits, the sub-thread to obtain the device online status has exited.
            {
                Thread.Sleep(100);
            }
            e.Cancel = false;
        }
        const float leftPadding = 1;
        const float topBottomPadding = 1;
        private float height = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
          //  Printer.InitDrawingBoard(50, 20, 0, "ZT001.ttf", 0, 0);

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

                
            }

            MessageBox.Show(msg);
            this.Close();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Printer.InitDrawingBoard(50, 20, 0, "ZT001.ttf", 0, 0);
            string str = "MKT-10090";
            Printer.DrawLableText(leftPadding, height + 10, 50, 25, Encoding.UTF8.GetBytes(str + (char)0), "ZT001.ttf", 6, 0, 1, 1, 2, 0, 0, new byte[] { 0, 0, 0, 0 });
            height += (topBottomPadding + 20);
            Print();
        }
    }
}
