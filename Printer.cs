using System;
using System.Runtime.InteropServices;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCSDKExample
{
    class Printer
    {
        /*****************************************打印流程************************************************************/
        [DllImport(@"jcPrinterApi.dll", EntryPoint = "initSdk", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 初始化字体路径，可默认空
         * @param[in] font 字体文件夹
         * @return 初始化成功返回true
         */
        public extern static Boolean initSdk(string font);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "getAllPrinters", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 查找USB打印机
         * 以json形式返回{打印机名称，端口}
         * Example： "{"S6_0011223344":"4"}"
         * @return 返回数据的指针 char *， 用完后需要手动释放内存
         */
        public extern static IntPtr getAllPrinters();

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "scanWifiPrinter", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 连接WIFI打印机
         * 以json形式返回
         * {"errorCode":0,"info":"[{"deviceName":"K1S-E818013369","IP":"192.168.1.125",
         * "tcpPort":"9100","avaliableClient":"6"}]","result":"true"}
         * @return 返回数据的指针 char *， 用完后需要手动释放内存
         */
        public extern static IntPtr scanWifiPrinter();

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "selectPrinter", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 连接USB打印机
         * @param[in] printerName 打印机名称（使用 `getAllPrinters `获取的打印机名称）
         * @param[in] port 打印机端口, `getAllPrinters`获取的打印机端口
         * @return 连接成功返回true
         */
        public extern static Boolean selectPrinter(string printerName, int port);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "connectWifiPrinter", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 连接WIFI打印机
         * @param[in] printerName 打印机名称（使用 `scanWifiPrinter `获取的打印机名称）
         * @param[in] port tcp端口, `getAllPrinters`获取的tcpPort
         * @return 连接成功返回true
         */
        public extern static Boolean connectWifiPrinter(string printerName, int port);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "configurationWifi", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 配置wifi
         * @param[in] wifiName 局域网WiFi名
         * @param[in] wifiPassword 局域网wifi密码
         * @return 连接成功返回true
         */
        public extern static Boolean configurationWifi(string wifiName, string wifiPassword);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "getWifiConfiguration", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 获取WiFi打印机当前配置的wifi网络名
         * 以json形式返回{打印机名称，端口}
         * Example： "{"wifiName" : "TEST_WIFI"}"
         * @return 返回数据的指针 char *，用完后需要手动释放内存
         */
        public extern static IntPtr getWifiConfiguration();

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "disconnectPrinter", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 断开连接
         * @return 操作成功返回true
         */
        public extern static Boolean disconnectPrinter();

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "isConnected", CallingConvention = CallingConvention.Cdecl)]
        /**
        * 检测USB设备连接状态
        */
        public extern static Boolean isConnected();

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "getPrintLastError", CallingConvention = CallingConvention.Cdecl)]
        /*
         * 获取打印异常码
         * @return 错误码
         *  | 错误码                         | 说明                    |
         *  | ----------------------------- | ------------------------|
         *  | E_NO_ERROR = 0                | 无错误                   |
            | E_BOX_OPENED = 1              | 盒盖打开                 |
            | E_NO_PAPER = 2                | 缺纸                     |
            | E_LOW_VOLTAGE = 3             | 电量不足                 |
            | E_BATTERY_UNNORMAL = 4        | 电池异常                 |
            | E_HANDLE_STOP = 5             | 手动停止                 |
            | E_DATA_ERROR = 6              | 数据错误                 |
            | E_HIGH_TEMPRATURE = 7         | 温度过高                 |
            | E_PAPER_OUT = 8               | 走纸异常                 |
            | E_IS_PRINTING = 9             | 正在打印                 |
            | E_NO_PRINTHEAD_DETECT = 10    | 未检测到打印头           |
            | E_TEMPRATURE_LOW = 11         | 环境温度过低             |
            | E_PRINTHEAD_FLEXIBLE = 12     | 打印头松动               |
            | E_NO_CARBON_BANDS = 13        | 未检测到碳带             |
            | E_NO_MATCH_CARBON_BANDS = 14  | 耗材不匹配               |
            | E_USED_CARBON_BANDS = 15      | 碳带用完                 |
            | E_PARPER_UNNORMAL = 16        | 纸张类型不支持           |
            | E_SET_PARPER_FAIL = 17        | 设置纸张类型失败         |
            | E_SET_PRINT_MODE_FAIL = 18    | 设置打印模式失败         |
            | E_SET_DENSITY_FAIL = 19       | 设置浓度失败             |
            | E_WRITE_RFID_FAIL = 20        | 写入rfid失败             |
            | E_MARGIN_ERROR = 21           | 边距参数错误             |
            | E_TIMEOUT = 22                | 超时错误                 |
            | E_DISCONNECT = 23             | 断开连接                 |
            | E_DRAWBOARD_ERROR = 24        | 画板参数设置错误         |
            | E_ORIENTATION_ERROR = 25      | 旋转角度参数错误         |
            | E_JSON_ERROR = 26             | JSON参数错误             |
            | E_PAPER_OUT_S = 27            | 出纸异常（关闭上盖检测）（已废弃） |
            | E_CHECK_PAPER = 28            | 检查纸张类型             |
            | E_PRINT_MODE = 29             | 碳带与打印模式不匹配     |
            | E_SET_DENSITY_NO_SUPPORT = 30 | 设置浓度不支持           |
            | E_PRINT_MODE_NO_SUPPORT  = 31 | 打印模式不支持           |
            | E_UNKNOW_ERROR = 255          | 未定义错误               |
         */
        public extern static int getPrintLastError();


        [DllImport(@"jcPrinterApi.dll", EntryPoint = "startJob", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 设置打印信息
         * @param[in] printLabelType 纸张类型：
         *                           1-间隙纸，2-⿊标纸，3-连续纸，4-过孔纸，5-透明纸
         * @param[in] printDensity 浓度，浓度范围1-5
         * @param[in] printMode 打印类型：
         *                      1-热敏
         *                      2-热转印
         * @param[in] printCount 打印份数
         * @return 设置成功返回true
         */
        public extern static Boolean startJob(int printLabelType, int printDensity, int printMode, int printCount);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "commitJob", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 提交打印任务
         * 使用绘制接口绘制打印数据需要调用此接口打印数据
         */
        public extern static Boolean commitJob();

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "picturePrint", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 图片打印
         * @param printData 图片数据
         * @param dataLenght 图片数据长度
         * @param verticalShift 水平偏移
         * @param horizontalShift 垂直偏移
         * @param threshold 二值化临界点（默认使用127）
         * @return 
         */
        public extern static Boolean picturePrint(IntPtr printData, uint dataLenght, int verticalShift, int horizontalShift, int threshold);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "endJob", CallingConvention = CallingConvention.Cdecl)]
        /**
         * 结束打印
         */
        public extern static Boolean endJob();

        /*****************************************绘制接口************************************************************/
        [DllImport(@"jcPrinterApi.dll", EntryPoint = "InitDrawingBoard", CallingConvention = CallingConvention.Cdecl)]
        /*
         * 初始化画板
         * @param width 画板宽度，单位mm
         * @param height 画板高度，单位mm
         * @param rotate 旋转角度，仅支持0,90,180,270
         * @param font 字体,默认使用 "ZT001.ttf"
         * @param verticalShift 垂直偏移
         * @param horizontalShift 水平偏移
         * @return 初始化成功返回 true
         */
        public extern static Boolean InitDrawingBoard(float width, float height, int rotate, string font, int verticalShift, int horizontalShift);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "DrawLableText", CallingConvention = CallingConvention.Cdecl)]
        /*
         * 绘制文本
         * @param x 水平坐标
         * @param y 垂直坐标
         * @param width 宽度， 单位mm
         * @param height 高度，单位mm
         * @param value 绘制的文本（UTF8）
         * @param fontFamily 字体，默认使用 "ZT001.ttf"
         * @param fontSize 字体大小
         * @param textAlignHorizonral 水平对齐方式，0:左对齐 1:居中对齐 2:右对齐
         * @param textAlignVertical 垂直对齐方式，0:顶对齐 1:垂直居中 2:底对齐
         * @param lineMode 行模式，默认2
         *                  1:宽高固定，内容大小自适应（字号/字符间距/行间距 按比例缩放）
         *                  2:宽度固定，高度自适应  
         *                  3:宽高固定，超出内容后面加...
         *                  4:宽高固定,超出内容直裁切
         * @param letterSpacing 字间距，单位mm
         * @param lineSpacing 行间距，单位mm
         * @param fontStyle 字体风格（加粗、斜体、下划线），数据格式[加粗，斜体，下划线，保留] 0代表不生效，1代表生效
         * @return 
         */
        public extern static Boolean DrawLableText(float x, float y, float width, float height, byte[] value, string fontFamily, float fontSize, int rotate, int textAlignHorizonral, int textAlignVertical, int lineMode, float letterSpacing, float lineSpacing, byte[] fontStyle);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "DrawLableBarCode", CallingConvention = CallingConvention.Cdecl)]
        /*
         * 绘制一维码
         * @param x 水平坐标
         * @param y 垂直坐标
         * @param width 宽，单位mm
         * @param height 高，单位mm
         * @param rotate: 旋转角度，仅支持0,90,180,270
         * @param value 文本内容（UTF8）
         * @param fontSize 字号
         * @param codeType 条码类型：
         *               20:CODE12821:UPC-A
         *               22:UPC-E
         *               23:EAN8
         *               24:EAN13
         *               25:CODE93
         *               26:CODE39
         *               27:CODEBAR
         *               28:ITF25
         * @param textHeight 文本高度
         * @param textPosition 文本位置（上、下、无）
         *                      0：下方
         *                      1：上方，
         *                      2：不显示
         */
        public extern static Boolean DrawLableBarCode(float x, float y, float width, float height, int codeType, byte[] value, float fontSize,
        int rotate, float textHeight, int textPosition);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "DrawLableQrCode", CallingConvention = CallingConvention.Cdecl)]
        /*
         * 绘制二维码
         * @param x 起始点左上角水平方向位置
         * @param y 起始点左上角垂直方向位置
         * @param width 宽
         * @param height 高 
         * @param value 文本内容（UTF8）
         * @param codeType 条码类型 
         *                  31:QR_CODE
         *                  32:PDF417
         *                  33:DATA_MATRIX
         *                  34:AZTEC
         * @param rotate: 旋转角度，0、90、180、270
         */
        public extern static Boolean DrawLableQrCode(float x, float y, float width, float height, byte[] value, int codeType, int rotate);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "DrawLableLine", CallingConvention = CallingConvention.Cdecl)]
        /*
         * 绘制线条
         * @param x 起始点左上角水平方向位置
         * @param y 起始点左上角垂直方向位置
         * @param width 宽
         * @param height 高 
         * @param rotate: 旋转角度，0、90、180、270
         * @param lineType 线类型（虚线、实线）
         * @param dashwidth 数据格式[实线段长度，空线段长度]
         */
        public extern static Boolean DrawLableLine(float x, float y, float width, float height, int rotate, int lineType, float[] dashwidth);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "DrawLableGraph", CallingConvention = CallingConvention.Cdecl)]
        /*
         * 绘制图形
         * @param x 起始点左上角水平方向位置
         * @param y 起始点左上角垂直方向位置
         * @param width 宽
         * @param height 高 
         * @param graphType 图形类型
         *                  1:圆
         *                  2:椭圆
         *                  3:矩形
         * @param rotate 旋转角度, 0、90、180、270
         * @param cornerRadius 圆角半径
         * @param lineWidth 线宽
         * @param lineType 线类型, 
         *                  1:实线 
         *                  2:虚线类型
         * @param dashwidth 数据格式[实线段长度，空线段长度]
         */
        public extern static Boolean DrawLableGraph(float x, float y, float width, float height, int graphType, int rotate, float cornerRadius, float lineWidth, int lineType, float[] dashwidth);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "DrawLableImage", CallingConvention = CallingConvention.Cdecl)]
        /*
         * 绘制图片
         * @param imageData 图片数据
         * @param x 起始点左上角水平方向位置
         * @param y 起始点左上角垂直方向位置
         * @param width 宽
         * @param height 高 
         * @param imageProcessingType 图像处理方式（默认使用0） 
         *  							1:阈值法 
         *								2:渐变
         * @param imageProcessingValue 图像处理阈值（默认使用0） 
         */
        public extern static Boolean DrawLableImage(string imageData, float x, float y, float width, float height, int rotate, int imageProcessingType, float imageProcessingValue);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "GeneratePrintPreviewImage", CallingConvention = CallingConvention.Cdecl)]
        /*
        * 生成预览图
        * @param generateImageJson 生成图片的Json数据
        * @param displayMultiple 图片显示dpi
        * @param printMultiple 打印机dpi
        * @return 图像数据,用完后需要手动释放内存
        */
        public extern static IntPtr GeneratePrintPreviewImage(float displayMultiple);

        [DllImport(@"jcPrinterApi.dll", EntryPoint = "FreeBuffer", CallingConvention = CallingConvention.Cdecl)]
        /*
        * 释放内存buffer
        */
        public extern static void FreeBuffer(IntPtr ptr);
    }
}
