using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LibraryOperation;

namespace LibrarySeatManageSystemAdmin
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region +全局字段
        public  string sno = string.Empty;
        public readonly int roomNum;
        public readonly string roomName;
        public readonly int tableCount;
        SelectSeat _ss=null;
        OperationSeat _os = null;
        public int curSeatnum;
        public bool isRandom;
        public bool isBook = false;
        public DateTime stopBook;//停止预约座位时间。
        public int during = 1;
        public delegate void RemoteSetColor(int seatnum, Brush color);
        public DispatcherTimer DT_sendMessage;
        public DispatcherTimer DT_SetIsOutDate;

        private SpeechSynthesizer speech;
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            DateTime now = DateTime.Now;
            string[] stoptime = ConfigurationManager.AppSettings["bookstart"].Trim().Split(":".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            int hour = int.Parse(stoptime[0]);
            int minute = int.Parse(stoptime[1]);
            stopBook = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);//设置截止时间
            tableCount = Load_AppSetting();
            roomNum = Convert.ToInt32(ConfigurationManager.AppSettings["room_num"]);
            roomName = ConfigurationManager.AppSettings["room_name"];
            room.Text = roomName;
            during = int.Parse(ConfigurationManager.AppSettings["during"]);
            //加载信息
            inputCard.Text = "";
            for (int i = 0; i <tableCount; i++)
                sp.Children.Add(CreateTable(i));
            _ss= new SelectSeat();
            _os = new OperationSeat();
            DT_SetIsOutDate = new DispatcherTimer();
            DT_sendMessage = new DispatcherTimer();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {                    
            FreshTime();
            curSeatnum = -1;
            isRandom = true; //默认选座方式是随机选座。
            inputCard.Focus();
            SetIsOutDate();
            Dt_IsOutDate(null, null);
            SendMessage();
            Od_SendMessage(null, null);
            _os.Scan(roomNum, ColorSeat);
            // Thread th = new Thread(new ThreadStart(ReceivedCard)); //用来接收服务器返回的刷卡信息（单独线程中执行while（true）接收命令。
            Thread th = new Thread(new ThreadStart(ReceivedCardFromPort));  //从单片机返回刷卡信息（while(true)）
            th.IsBackground = true;
            th.Start();
            speech = new SpeechSynthesizer();//初始化语音合成对象
        }

        #region+按照秒刷新显示在屏幕左下角的时间
        private void FreshTime()
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += Dt_Tick;
            dt.IsEnabled = true;
        }
        private void Dt_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            tblock_date.Text = now.ToShortDateString();
            tblock_time.Text = now.ToLongTimeString();
            if (now < stopBook)
                isBook = true;
            else if (Math.Abs((stopBook-now).TotalSeconds)<1)
                startBook(roomNum);
            else
                isBook = false;
        }
        private void startBook(int room)
        {
            string sql = "update tb_seat set available=1 where room=@room;delete from tb_order where room=@room;update tb_room set book=total where no=@room";
            SqlServerHelper.ExecuteNonQuery(sql,new SqlParameter[] { new SqlParameter("@room",room) });
        }
        #endregion

        #region +设置过期每隔3秒扫描一次。
        private void SetIsOutDate()
        {
            DT_SetIsOutDate.Interval = TimeSpan.FromSeconds(3);
            DT_SetIsOutDate.IsEnabled = true;
            DT_SetIsOutDate.Tick += Dt_IsOutDate;
            DT_SetIsOutDate.Start();
        }
        /// <summary>
        /// 设置过期座位的状态信息，并且更新服务器的座位数量信息。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dt_IsOutDate(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            _os.SetIsOutDateClient(roomNum,ColorSeat,now);
            int remainCur = _os.GetRedundancyCur(roomNum);
            int remain15min = _os.SetRedundancy(roomNum,15,now);
            remain15min += remainCur;

            available_cur.Text = remainCur.ToString();
            available_15minafter.Text = remain15min.ToString();
            _os.SetRoomSeatsClient(roomNum, remainCur, remain15min);
        }
        #endregion

        #region +找需要发送提醒消息的用户并发送消息。
        private void SendMessage()
        {
            DT_sendMessage.Interval = TimeSpan.FromSeconds(60);
            DT_sendMessage.Tick += Od_SendMessage;
            DT_sendMessage.Start();
        }
        private void Od_SendMessage(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            _os.SendSeatOutdateMessage(roomNum,roomName,20,now);
            _os.SendSeatOutdateMessage(roomNum,roomName,5, now);
        }
        #endregion

        #region+加载程序配置信息；
        private int Load_AppSetting()
        {
            string connStr = ConfigurationManager.ConnectionStrings["db_library"].ToString();
            string sql = "select name,tables from tb_room where no=" + ConfigurationManager.AppSettings["room_num"];
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    SqlDataReader sdr = cmd.ExecuteReader();
                    sdr.Read();
                    Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    ConfigurationManager.RefreshSection("appSettings");
                    cfa.AppSettings.Settings["room_name"].Value =( (string)sdr[0]).Trim();
                    cfa.AppSettings.Settings["tableCount"].Value = ((int)sdr[1]).ToString().Trim();
                    cfa.Save();
                }
            }
            return int.Parse(ConfigurationManager.AppSettings["tableCount"]);
        }
        #endregion

        #region+初始化操作(生成座位分布图）
        /// <summary>
        /// 绘制一个桌子。
        /// </summary>
        /// <param name="num">桌子的编号</param>
        /// <returns>绘制好的Grid对象</returns>
        private Grid CreateTable(int num)
        {
            string seatContent = string.Empty;
            Grid g = new Grid();
            g.Margin = new Thickness { Bottom = 9, Left = 9, Right = 9, Top = 9 };
            g.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/assets/image/timg.jpg", UriKind.RelativeOrAbsolute)) };
            #region   生成grid内部行列组件
            g.RowDefinitions.Add(new RowDefinition());
            g.RowDefinitions.Add(new RowDefinition());
            g.RowDefinitions.Add(new RowDefinition());
            g.RowDefinitions.Add(new RowDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            #endregion
            int[] row = { 0, 0, 3, 3 };
            int[] column = { 0, 3, 0, 3 };
            for(int i=0;i<4;i++)
            {
                int seatNum = num * 4 + i;
                Button seat = new Button();
                string name = "num" + seatNum.ToString();
                seat.Name =name;
                seat.ToolTip = seatNum+1;
                seat.Tag = true;
                seat.Content = (Seat)i;
                seat.FontSize = 20;
                seat.Background = Brushes.Green;
                seat.Click += Seat_Click;
                sp.RegisterName(name, seat); //将所有座位都注册到名为sp的StackPanel控件上，以便于以后查找；
                g.Children.Add(seat);
                Grid.SetColumn(seat,column[i]);
                Grid.SetRow(seat, row[i]);
            }
            TextBlock tb = new TextBlock();
            tb.Text = (num+1).ToString("D3");
            tb.FontSize = 25;
            tb.FontWeight = FontWeights.Bold;
            g.Children.Add(tb);
            Grid.SetRow(tb, 1);
            Grid.SetRowSpan(tb, 2);
            Grid.SetColumn(tb, 1);
            Grid.SetColumnSpan(tb, 2);
            return g;
        }
        #endregion

        #region+随机选座 RandomSeatSelected(string sno,out int seatnum)
        public void RandomSeatSelected(string sno, DateTime start, int during, out int seatnum,out bool sucess)
        {
            //调用一个函数 给他学号，返回一个 座位号，（学号不在的情况下，给他分配的座位是以前又没人座）座位号是否在tb_seat_student中，学号是否在tb_seat_student中。
            _ss.RandomSelectSeat(roomNum, sno,start,during, out seatnum,out sucess);
        }
        #endregion

        #region+自定义选座 CustomSeatSelected(string sno,out int seatnum)
        public void CustomSeatSelected(string sno,DateTime start,int during,ref int seatnum,out bool success)
        {
            seatnum = curSeatnum;
            _ss.CustomSeatSelected(roomNum, sno,start,during,ref seatnum,out success);
        }
        #endregion

        #region+预约座位处理程序
        public void BookSeat(string sno,DateTime start,int during,out int seatnum,out bool success)
        {
            seatnum = -1;
            SqlServerHelper.IsBookSeat(roomNum,sno, start, during, out seatnum,out success);
        }
        #endregion

        #region+ 点击座位事件处理程序
        /// <summary>
        /// 点击座位事件处理程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Seat_Click(object sender, RoutedEventArgs e)
        {
            Button temp = (Button)sender;
            DT_SetIsOutDate.Stop();
            if (temp.Background == Brushes.Red)
            {
                MessageBox.Show("请选择有效座位");
            }
            else
            {
                temp.Background = Brushes.Yellow;
                isRandom = false;
                curSeatnum = (int)(((Button)sender).ToolTip) - 1;
                #region +超过5秒钟没有刷卡，座位颜色自动恢复绿色；
                DispatcherTimer dt = new DispatcherTimer();
                dt.Interval = TimeSpan.FromSeconds(5);
                dt.Tick += (o, s) => { if (!isRandom) { ColorSeat(curSeatnum, Brushes.Green); isRandom = true; } curSeatnum = -1; DT_SetIsOutDate.Start(); dt.Stop(); };
                dt.Start();
                #endregion
                inputCard.Focus();
            }
        }
        #endregion

        #region+刷卡事件处理程序
        /// <summary>
        /// 刷卡处理程序。接收传输过来的sno，依据系统的IsRandom值来决定是随机选座还是自定义选座。
        /// </summary>
        private void ReadCard(string sno)
        {        
            int seatnum = -1;
            bool success=false;
           
            Dt_IsOutDate(null, null);
            DateTime start = DateTime.Now;
            //判断一下是不是随机选座。 isRandom为true表示随机，isRandom为false表示自定义选座
            if (isRandom)
            {
                if (_os.IsHasSeat(room, sno, out seatnum))
                {
                    if(MessageBox.Show("是否结束使用并释放座位？", "提示", MessageBoxButton.YesNo)==MessageBoxResult.Yes)
                    {
                        _os.ReleaseSeat(roomNum, seatnum,sno);
                        return;
                    }
                }
                if (isBook)
                    BookSeat(sno, start, during, out seatnum,out success);
                if(!success)
                    RandomSeatSelected(sno, start, during, out seatnum, out success);
            }                         
            else
                CustomSeatSelected(sno, start, during, ref seatnum, out success);
            if (!success)
                MessageBox.Show("出错了！");
            else
            {
                string name = _ss.FindName(sno);//显示信息并且立即刷新剩余座位数；
                int table = seatnum / 4 + 1;
                int seat = seatnum % 4;
                ShowReaderInfo(sno, name, roomName, table, (Seat)seat, start);
                ColorSeat(seatnum, Brushes.Red);
                _os.UpdateSeat(roomNum, ColorSeat);
                Thread speak = new Thread(new ParameterizedThreadStart(Speak));
                speak.Start(name+",座位号"+table.ToString("D3")+((Seat)seat).ToString()+",选座成功！");
            }
            Dt_IsOutDate(null, null);
            //   Od_SendMessage(null, null);
            //恢复系统状态到随机选座，并且焦点置于输入框中；
            isRandom = true;
            inputCard.Text = "";
            inputCard.Focus();
        }
        #endregion

        #region+键盘输入方式刷卡处理程序
        private void inputCard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sno = inputCard.Text.Trim();
                ReadCard(sno);
            }           
        }
        #endregion

        #region+服务器中转模式的手机NFC刷卡处理程序
        /// <summary>
        /// 刷卡程序。建立本地与服务器123.206.26.20：10010之间的Tcp长连接。不断尝试接收服务器传输过来的数据，
        /// 解析出传过来的卡号。向UI线程调用ReadCard(sno)函数。表示一次刷卡操作。
        /// </summary>
        public void ReceivedCard()
        {
            byte[] data = new byte[48 * 3 * 3];
            IPEndPoint remote = new IPEndPoint(IPAddress.Parse("123.206.26.20"), 10010);
            byte end = Encoding.UTF8.GetBytes(".")[0];
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(remote);
            if (client.Connected)
                Console.WriteLine("已连接！");
            while (true)
            {
                client.Receive(data);
                int endindex = 0;
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == end)
                    {
                        endindex = i;
                        break;
                    }
                string res = Encoding.UTF8.GetString(data, 0, endindex);
                string[] info = res.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                sno = info[0];
                Dispatcher.Invoke(new Action(() => { ReadCard(sno); }));   //通过其他线程向主线程发送调用函数的命令。
            }
        }
        #endregion

        #region+RC522下位机方式刷卡处理程序
        public void ReceivedCardFromPort()
        {
            SerialPort serialPort = new SerialPort();
            serialPort.PortName = "COM3";
            serialPort.BaudRate = 9600; //设置波特率9600
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Parity = Parity.None;
            serialPort.ReadTimeout = 3000;
            serialPort.WriteTimeout = 3000;
            serialPort.ReceivedBytesThreshold = 1;
            serialPort.DataReceived += (o, e) =>
            {
                try
                {
                    int len = serialPort.BytesToRead;
                    byte[] readBuffer = new Byte[len];
                    serialPort.Read(readBuffer, 0, len);
                    string rec = Bytes2String(readBuffer).Substring(0, 11);
                    string no = _ss.FindNo(rec);                 
                    Dispatcher.Invoke(new Action(() => ReadCard(no)));
                }
                catch (Exception) { }
            };
            try
            {
                serialPort.Open();
                while (true)
                {
                }
            }
            catch (Exception)
            {
                Console.WriteLine("串口打开失败");
            }
        }
        #endregion

        #region+调用微软语音合成功能
        private void Speak(object msg)
        {
            speech.Rate =0;
            speech.SelectVoice("Microsoft Huihui Desktop");
            speech.Volume = 100;
            speech.SpeakAsync((string)msg);
        }
        #endregion

        #region+字节数组转换成16进制字符串形式
        private string Bytes2String(byte[] buffer)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte item in buffer)
            {
                sb.AppendFormat("{0} ", Convert.ToString(item, 16).ToUpper().PadLeft(2, '0'));
            }
            sb.AppendLine();
            return sb.ToString();
        }
        #endregion

        #region+ 座位选定后在屏幕右侧显示刷卡人信息
        /// <summary>
        /// 座位选定后在屏幕右侧显示刷卡人信息
        /// </summary>
        /// <param name="sno">卡号</param>
        /// <param name="name">姓名</param>
        /// <param name="floor">楼层</param>
        /// <param name="table">桌号</param>
        /// <param name="seat">座位号</param>
        /// <param name="time">有效时间</param>
        public void ShowReaderInfo(string sno, string name, string room, int table, Seat seat, DateTime time)
        {
            tb_sno.Text = sno;
            tb_name.Text = name;
            tb_floor.Text = room.ToString();
            tb_table.Text = table.ToString("D3");
            tb_seat.Text = seat.ToString();
            tb_time.Text = time.ToShortTimeString() + "----" + time.AddMinutes(during).ToShortTimeString();
        }
        #endregion

        #region +将选定座位颜色变成红色
        public void ColorSeat(int seatnum,Brush color)
        {
            Button curSeat = (Button)FindName(("num" + seatnum).ToString());
            curSeat.Background = color;
        }
        #endregion

        #region+全局快捷键
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.WindowState = WindowState.Normal;
        }

        private void CommandBinding_CanExecute_1(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.WindowStyle != WindowStyle.None)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
        }

        #endregion
    }
}