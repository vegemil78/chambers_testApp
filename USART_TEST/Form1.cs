using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using System.IO.Ports;
using Util;




namespace DEVICE_SETTING_PROGRAM
{

    public partial class Form1 : Form 
    {
        SerialPort m_sp1;
        ModbusRtuCrc mrc = new ModbusRtuCrc();
        bool com_open_button_flag = false;
        //public static clsSerial cSerial = null;
        public static string commPort = "";
        public static string dataBits = ""; 
        public static string BaudRate = "";
        public static string stopBit = "";
        public static string parity = "";  //Even-2,Mark-3,None-0,Odd-1,Space-4
        public static string Handshaking;

        public string date_str = "";

        public static int m_count = 0;
        public static int rx_crc_chk_flag = 0;

        public byte[] device_data = new byte[130];



        public int mIRecSize = 0;
        public int mIRecSizeSum = 0;
        public byte[] mBuff = new byte[50];
        public byte[] mTxBuff = new byte[500];
        public int mAddr = 0;
        public int mLength = 0;
        public UInt16[] modbusMemory = new UInt16[400];
        public UInt16[] modbusMemoryUpdate = new UInt16[400];

        ChambersModbusData chamModData = null;
        List<ChambersModbusData> list = null;
        ChambersModbusData[] addrMap = null;

        public bool[] led_status_arry = new bool[16];
        List<bool> list2 = null;

        // 화면 표시
        delChangeText setChangeText;
        delUpdateListView updateModbusLV;

        Thread t;
        int crlf_count = 0;
        int rx_pos = 0;
        short mSec100 = 0;
	    short mSec10 = 0;
	    short mSec = 0;
	    short Sec = 0;
	    short Min = 0;
        UInt16 mod4haddress = 0;

        ListViewItem.ListViewSubItem curSB;
        ListViewItem curItem;

        ListViewItem curItem2;
        bool cancelEdit;


        public void threadRun()
        {

            
            while (true)
            {
               //Console.WriteLine("Helper.Run");
               Thread.Sleep(1);
               if (crlf_count > 0)crlf_count--;

               if (crlf_count == 1)
               {
                   UpdateTextBox("\r\n");
                   chk_command();
                   //updata_modbusMemory();
                     
                   rx_pos = 0;
               }
               if (crlf_count == 0)rx_pos = 0;




               if (++mSec >= 10)
               {
                   mSec = 0;



                   if (++mSec10 >= 10)
                   {
                       mSec10 = 0;
                       UpdateTextBoxTransmit(mod_send(1, 4, 305, 1));

                       if (++mSec100 >= 10)
                       {
                           mSec100 = 0; 
                           

                           if (++Sec >= 60)
                           {
                               Sec = 0;


                           }
                       }
                   }
               } 
               

            }

        }

        byte[] rx_mod_buf = new byte[300];
        byte[] tx_mod_buf = new byte[300];

        void chk_command()
        {
            UInt16 addr; 
            UInt16 data;
            UInt16 len;
            UInt16 pos;
            UInt16 checksum;
            UInt16 tx_checksum;
            UInt16 aa;
            byte[] res_crc = new byte[2];
	        if(rx_mod_buf[0]==0x01 && rx_mod_buf[1]==0x04)  //receive read input register 04H
	        {
                len = rx_mod_buf[2];
                Console.WriteLine("len {0}", len);
                res_crc = mrc.fn_makeCRC16_byte(rx_mod_buf, len+3);
                if (mod4haddress >= 40000) mod4haddress -= 40000;
                if (res_crc[0] == rx_mod_buf[len + 3] && res_crc[1] == rx_mod_buf[len + 3 + 1])
                {
                    Console.WriteLine("crc ok");
                    Console.WriteLine("mod4haddress = {0}", mod4haddress);
                    for(int i=0; i<len; i++){
                        if (i % 2 == 0)
                        {
                            modbusMemory[mod4haddress + i / 2] = (UInt16)(rx_mod_buf[3 + i] << 8);
                        }
                        else
                        {
                            modbusMemory[mod4haddress + i / 2] += (UInt16)rx_mod_buf[3 + i];
                        }                      

                    }
                    mod4haddress = 0;
                    this.BeginInvoke(updateModbusLV);
                }
               
                

                


              

                /*
		        if(crc_modbus(rx_mod_buf_str,6)==checksum)
		        {
			        modbusmemory[addr]=data;
			        hal_uart_transmit(&huart3,rx_mod_buf,8,100);
		        }
                 */
	        }
	        
        }

        void memory_setting()
        {
	        for(int i=0; i<400; i++)modbusMemory[i]=0;

            modbusMemory[Constants.MOD_RESET] = 0;
            modbusMemory[Constants.MOD_COMPRESSION_VALVE] = 0;
            modbusMemory[Constants.MOD_COMPRESSION_SHUTOFF_VALVE] = 0;
            modbusMemory[Constants.MOD_DECOMPRESSION_VALVE] = 0;
            modbusMemory[Constants.MOD_BREATHING_GAS_SELECT_VALVE] = 0;
            modbusMemory[Constants.MOD_RESIDUAL_PRESSURE_REMOVE_VALVE] = 0;
            modbusMemory[Constants.MOD_DRAIN_VALVE] = 0;
            modbusMemory[Constants.MOD_ENTRANCEDOOR_LEFTANDRIGHT_VALVE] = 0;
            modbusMemory[Constants.MOD_ENTRANCEDOOR_TOPANDBOTTOM_VALVE] = 0;
            modbusMemory[Constants.MOD_EMERGENCY_EXHAUST_VALVE] = 0;
            modbusMemory[Constants.MOD_FIRE_EXTTINGUISHING_WATER] = 0;
            modbusMemory[Constants.MOD_ENTRANCEDOOR_LEFTANDRIGHT_CHECK] = 0;
            modbusMemory[Constants.MOD_ENTRANCEDOOR_TOPANDBOTTOM_CHECK] = 0;
            modbusMemory[Constants.MOD_SAFETY_PHOTO_SENSOR] = 0;
            modbusMemory[Constants.MOD_SAFETY_MOTION_DETECTION1_SENSOR] = 0;
            modbusMemory[Constants.MOD_SAFETY_MOTION_DETECTION2_SENSOR] = 0;
            modbusMemory[Constants.MOD_SCRUBBER_FAN_VOLUME] = 0;
            modbusMemory[Constants.MOD_HEATINGANDCOOLING_FAN_VOLUME] = 0;
            modbusMemory[Constants.MOD_HEATINGANDCOOLING_SELECT] = 0;
            modbusMemory[Constants.MOD_LIGHT_VOLUME] = 0;
            modbusMemory[Constants.MOD_PRESSURE_SENSOR] = 0;
            modbusMemory[Constants.MOD_OXYGEN_SENSOR] = 0;
            modbusMemory[Constants.MOD_CARBON_DIOXIDE_SENSOR] = 0;
            modbusMemory[Constants.MOD_TEMPERATURE_SENSOR] = 0;
            modbusMemory[Constants.MOD_HUMIDITY_SENSOR] = 0;
            modbusMemory[Constants.MOD_SPARE1_SENSOR] = 0;
            modbusMemory[Constants.MOD_SPARE2_SENSOR] = 0;
            modbusMemory[Constants.MOD_SPARE3_SENSOR] = 0;


        }

        void MemoryListView()
        {
             
            addrMap[1].setup(0, Constants.MOD_RESET, modbusMemory[Constants.MOD_RESET], modbusMemoryUpdate[Constants.MOD_RESET], "reset");
            addrMap[2].setup(0, Constants.MOD_COMPRESSION_VALVE, modbusMemory[Constants.MOD_COMPRESSION_VALVE], modbusMemoryUpdate[Constants.MOD_COMPRESSION_VALVE], "compression");
            addrMap[3].setup(0, Constants.MOD_COMPRESSION_SHUTOFF_VALVE, modbusMemory[Constants.MOD_COMPRESSION_SHUTOFF_VALVE], modbusMemoryUpdate[Constants.MOD_COMPRESSION_SHUTOFF_VALVE], "compression shut off");
            addrMap[4].setup(0, Constants.MOD_DECOMPRESSION_VALVE, modbusMemory[Constants.MOD_DECOMPRESSION_VALVE], modbusMemoryUpdate[Constants.MOD_DECOMPRESSION_VALVE], "decompression");
            addrMap[5].setup(0, Constants.MOD_BREATHING_GAS_SELECT_VALVE, modbusMemory[Constants.MOD_BREATHING_GAS_SELECT_VALVE], modbusMemoryUpdate[Constants.MOD_BREATHING_GAS_SELECT_VALVE], "breathing gas select");
            addrMap[6].setup(0, Constants.MOD_RESIDUAL_PRESSURE_REMOVE_VALVE, modbusMemory[Constants.MOD_RESIDUAL_PRESSURE_REMOVE_VALVE], modbusMemoryUpdate[Constants.MOD_RESIDUAL_PRESSURE_REMOVE_VALVE], "residual pressure remove");
            addrMap[7].setup(0, Constants.MOD_DRAIN_VALVE, modbusMemory[Constants.MOD_DRAIN_VALVE], modbusMemoryUpdate[Constants.MOD_DRAIN_VALVE], "drain");
            addrMap[8].setup(0, Constants.MOD_ENTRANCEDOOR_LEFTANDRIGHT_VALVE, modbusMemory[Constants.MOD_ENTRANCEDOOR_LEFTANDRIGHT_VALVE], modbusMemoryUpdate[Constants.MOD_ENTRANCEDOOR_LEFTANDRIGHT_VALVE], "Entrance door left and right");
            addrMap[9].setup(0, Constants.MOD_ENTRANCEDOOR_TOPANDBOTTOM_VALVE, modbusMemory[Constants.MOD_ENTRANCEDOOR_TOPANDBOTTOM_VALVE], modbusMemoryUpdate[Constants.MOD_ENTRANCEDOOR_TOPANDBOTTOM_VALVE], "Entrance door top and bottom");
            addrMap[10].setup(0, Constants.MOD_EMERGENCY_EXHAUST_VALVE, modbusMemory[Constants.MOD_EMERGENCY_EXHAUST_VALVE], modbusMemoryUpdate[Constants.MOD_EMERGENCY_EXHAUST_VALVE], "emergency exhaust");
            addrMap[11].setup(0, Constants.MOD_FIRE_EXTTINGUISHING_WATER, modbusMemory[Constants.MOD_FIRE_EXTTINGUISHING_WATER], modbusMemoryUpdate[Constants.MOD_FIRE_EXTTINGUISHING_WATER], "fire extinguishing water");
            addrMap[21].setup(10000, Constants.MOD_ENTRANCEDOOR_LEFTANDRIGHT_CHECK, modbusMemory[Constants.MOD_ENTRANCEDOOR_LEFTANDRIGHT_CHECK], modbusMemoryUpdate[Constants.MOD_ENTRANCEDOOR_LEFTANDRIGHT_CHECK], "Entrance door left and right check");
            addrMap[22].setup(10000, Constants.MOD_ENTRANCEDOOR_TOPANDBOTTOM_CHECK, modbusMemory[Constants.MOD_ENTRANCEDOOR_TOPANDBOTTOM_CHECK], modbusMemoryUpdate[Constants.MOD_ENTRANCEDOOR_TOPANDBOTTOM_CHECK], "Entrance door top and bottom check");
            addrMap[23].setup(10000, Constants.MOD_SAFETY_PHOTO_SENSOR, modbusMemory[Constants.MOD_SAFETY_PHOTO_SENSOR], modbusMemoryUpdate[Constants.MOD_SAFETY_PHOTO_SENSOR], "safety photo sensor");
            addrMap[24].setup(10000, Constants.MOD_SAFETY_MOTION_DETECTION1_SENSOR, modbusMemory[Constants.MOD_SAFETY_MOTION_DETECTION1_SENSOR], modbusMemoryUpdate[Constants.MOD_SAFETY_MOTION_DETECTION1_SENSOR], "safety motion detection1 sensor");
            addrMap[25].setup(10000, Constants.MOD_SAFETY_MOTION_DETECTION2_SENSOR, modbusMemory[Constants.MOD_SAFETY_MOTION_DETECTION2_SENSOR], modbusMemoryUpdate[Constants.MOD_SAFETY_MOTION_DETECTION2_SENSOR], "safety motion detection2 sensor");
            addrMap[31].setup(40000, Constants.MOD_SCRUBBER_FAN_VOLUME, modbusMemory[Constants.MOD_SCRUBBER_FAN_VOLUME], modbusMemoryUpdate[Constants.MOD_SCRUBBER_FAN_VOLUME], "scrubber fan volume");
            addrMap[32].setup(40000, Constants.MOD_HEATINGANDCOOLING_FAN_VOLUME, modbusMemory[Constants.MOD_HEATINGANDCOOLING_FAN_VOLUME], modbusMemoryUpdate[Constants.MOD_HEATINGANDCOOLING_FAN_VOLUME], "heating and cooling fan volume");
            addrMap[33].setup(40000, Constants.MOD_HEATINGANDCOOLING_SELECT, modbusMemory[Constants.MOD_HEATINGANDCOOLING_SELECT], modbusMemoryUpdate[Constants.MOD_HEATINGANDCOOLING_SELECT], "heating and cooling select");
            addrMap[34].setup(40000, Constants.MOD_LIGHT_VOLUME, modbusMemory[Constants.MOD_LIGHT_VOLUME], modbusMemoryUpdate[Constants.MOD_LIGHT_VOLUME], "light volume");
            addrMap[35].setup(30000, Constants.MOD_STATUS_LED, modbusMemory[Constants.MOD_STATUS_LED], modbusMemoryUpdate[Constants.MOD_STATUS_LED], "led status");
            addrMap[36].setup(30000, 306, 0, 0, "Reserved");
            addrMap[37].setup(30000, Constants.MOD_PRESSURE_SENSOR, modbusMemory[Constants.MOD_PRESSURE_SENSOR], modbusMemoryUpdate[Constants.MOD_PRESSURE_SENSOR], "pressure sensor");
            addrMap[38].setup(30000, Constants.MOD_OXYGEN_SENSOR, modbusMemory[Constants.MOD_OXYGEN_SENSOR], modbusMemoryUpdate[Constants.MOD_OXYGEN_SENSOR], "oxygen sonsor");
            addrMap[39].setup(30000, Constants.MOD_CARBON_DIOXIDE_SENSOR, modbusMemory[Constants.MOD_CARBON_DIOXIDE_SENSOR], modbusMemoryUpdate[Constants.MOD_CARBON_DIOXIDE_SENSOR], "carbon dioxide sensor");
            addrMap[40].setup(30000, Constants.MOD_TEMPERATURE_SENSOR, modbusMemory[Constants.MOD_TEMPERATURE_SENSOR], modbusMemoryUpdate[Constants.MOD_TEMPERATURE_SENSOR], "temperature Senser");
            addrMap[41].setup(30000, Constants.MOD_HUMIDITY_SENSOR, modbusMemory[Constants.MOD_HUMIDITY_SENSOR], modbusMemoryUpdate[Constants.MOD_HUMIDITY_SENSOR], "humidity sensor");
            addrMap[42].setup(30000, Constants.MOD_SPARE1_SENSOR, modbusMemory[Constants.MOD_SPARE1_SENSOR], modbusMemoryUpdate[Constants.MOD_SPARE1_SENSOR], "spare1 sensor");
            addrMap[43].setup(30000, Constants.MOD_SPARE2_SENSOR, modbusMemory[Constants.MOD_SPARE2_SENSOR], modbusMemoryUpdate[Constants.MOD_SPARE2_SENSOR], "spare2 sensor");
            addrMap[44].setup(30000, Constants.MOD_SPARE3_SENSOR, modbusMemory[Constants.MOD_SPARE3_SENSOR], modbusMemoryUpdate[Constants.MOD_SPARE3_SENSOR], "spare3 sensor");

            list.Clear();
            for (int i = 0; i < 12; i++)
                list.Add(addrMap[i]);
            for (int i = 21; i < 26; i++)
                list.Add(addrMap[i]);
            for (int i = 31; i < 45; i++)
                list.Add(addrMap[i]);

            FillListView(list);
        }

        void LedStatusListView()
        {
            ushort mask_variable = 0;
            list2.Clear();
            for (int i = 0; i < 16; i++)
            {
                mask_variable = (ushort)(0x1 << i);
                if ((modbusMemory[Constants.MOD_STATUS_LED] & mask_variable) == 0) led_status_arry[i] = false;
                else led_status_arry[i] = true;
         
                list2.Add(led_status_arry[i]);
            }

            FillListView2(list2);
        }

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            t.Abort();
            MessageBox.Show("종료합니다.");
            Process.GetCurrentProcess().Kill();
            Application.Exit();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            radioButtonOff.Checked = true;
            
            
            //this.Size = new Size(1419, 829);
            //pMain.Controls.Add(ucSc1);
            
            setChangeText = new delChangeText(setGui);
            updateModbusLV = new delUpdateListView(updateListViewGui);
            
            commPort = "COM2";
            dataBits = "8"; 
            //BaudRate = "9600"; 
            BaudRate = "115200";
 
            //stopBit = "2";
            stopBit = "1";
            parity = "0";  //Even-2,Mark-3,None-0,Odd-1,Space-4 
            Handshaking = null;

            modTextBox1.Text = "1";
            modTextBox2.Text = "4";
            //modTextBox3.Text = "03EB";
            //modTextBox3.Text = "1004";
            modTextBox3.Text = "40301";
            modTextBox4.Text = "1";

            t = new Thread(threadRun);
            t.Start();
            addrMap = new ChambersModbusData[50];
            list = new List<ChambersModbusData>(); 

            for (int i = 0; i < 50; i++)
                addrMap[i] = new ChambersModbusData(0, 0, 0, 0, "");

            addrMap[1].setup(0, Constants.MOD_RESET, 0,0, "reset");
            addrMap[2].setup(0, Constants.MOD_COMPRESSION_VALVE, 0, 0, "compression");
            addrMap[3].setup(0, Constants.MOD_COMPRESSION_SHUTOFF_VALVE, 0, 0, "compression shut off");
            addrMap[4].setup(0, Constants.MOD_DECOMPRESSION_VALVE, 0, 0, "decompression");
            addrMap[5].setup(0, Constants.MOD_BREATHING_GAS_SELECT_VALVE, 0, 0, "breathing gas select");
            addrMap[6].setup(0, Constants.MOD_RESIDUAL_PRESSURE_REMOVE_VALVE, 0, 0, "residual pressure remove");
            addrMap[7].setup(0, Constants.MOD_DRAIN_VALVE, 0, 0, "drain");
            addrMap[8].setup(0, Constants.MOD_ENTRANCEDOOR_LEFTANDRIGHT_VALVE, 0, 0, "Entrance door left and right");
            addrMap[9].setup(0, Constants.MOD_ENTRANCEDOOR_TOPANDBOTTOM_VALVE, 0, 0, "Entrance door top and bottom");
            addrMap[10].setup(0, Constants.MOD_EMERGENCY_EXHAUST_VALVE, 0, 0, "emergency exhaust");
            addrMap[11].setup(0, Constants.MOD_FIRE_EXTTINGUISHING_WATER, 0, 0, "fire extinguishing water");
            addrMap[21].setup(10000, Constants.MOD_ENTRANCEDOOR_LEFTANDRIGHT_CHECK, 0, 0, "Entrance door left and right check");
            addrMap[22].setup(10000, Constants.MOD_ENTRANCEDOOR_TOPANDBOTTOM_CHECK, 0, 0, "Entrance door top and bottom check");
            addrMap[23].setup(10000, Constants.MOD_SAFETY_PHOTO_SENSOR, 0, 0, "safety photo sensor");
            addrMap[24].setup(10000, Constants.MOD_SAFETY_MOTION_DETECTION1_SENSOR, 0, 0, "safety motion detection1 sensor");
            addrMap[25].setup(10000, Constants.MOD_SAFETY_MOTION_DETECTION2_SENSOR, 0, 0, "safety motion detection2 sensor");
            addrMap[31].setup(40000, Constants.MOD_SCRUBBER_FAN_VOLUME, 0, 0, "scrubber fan volume");
            addrMap[32].setup(40000, Constants.MOD_HEATINGANDCOOLING_FAN_VOLUME, 0, 0, "heating and cooling fan volume");
            addrMap[33].setup(40000, Constants.MOD_HEATINGANDCOOLING_SELECT, 0, 0, "heating and cooling select");
            addrMap[34].setup(40000, Constants.MOD_LIGHT_VOLUME, 0, 0, "light volume");
            addrMap[35].setup(30000, Constants.MOD_STATUS_LED, 0, 0, "led status");
            addrMap[36].setup(30000, 306, 0, 0, "Reserved");
            addrMap[37].setup(30000, Constants.MOD_PRESSURE_SENSOR, 0, 0, "pressure sensor");
            addrMap[38].setup(30000, Constants.MOD_OXYGEN_SENSOR, 0, 0, "oxygen sonsor");
            addrMap[39].setup(30000, Constants.MOD_CARBON_DIOXIDE_SENSOR, 0, 0, "carbon dioxide sensor");
            addrMap[40].setup(30000, Constants.MOD_TEMPERATURE_SENSOR, 0, 0, "temperature Senser");
            addrMap[41].setup(30000, Constants.MOD_HUMIDITY_SENSOR, 0, 0, "humidity sensor");
            addrMap[42].setup(30000, Constants.MOD_SPARE1_SENSOR, 0, 0, "spare1 sensor");
            addrMap[43].setup(30000, Constants.MOD_SPARE2_SENSOR, 0, 0, "spare2 sensor");
            addrMap[44].setup(30000, Constants.MOD_SPARE3_SENSOR, 0, 0, "spare3 sensor");
 
            

            for (int i = 0; i < 11; i++) 
                list.Add(addrMap[i]);
            for (int i = 21; i < 26; i++)
                list.Add(addrMap[i]);
            for (int i = 31; i < 45; i++)
                list.Add(addrMap[i]);

            SetupListView(list);


            list2 = new List<bool>();
            for (int i = 0; i < 16; i++)
                list2.Add(led_status_arry[i]);
            SetupListView2(list2);
            
        }


        private void SetupListView(List<ChambersModbusData> list)
        {
            this.listView1.View = System.Windows.Forms.View.Details;

            ColumnHeader ch = new ColumnHeader()
            {
                Text = "PrimaryNum",
                Width = 90
            };
            this.listView1.Columns.Add(ch);

            ch = new ColumnHeader()
            {
                Text = "Address",
                Width = 60
            };
            this.listView1.Columns.Add(ch);

            ch = new ColumnHeader()
            {
                Text = "Value",
                Width = 40,
            };
            this.listView1.Columns.Add(ch);

            ch = new ColumnHeader()
            {
                Text = "UpdateValue",
                Width = 40,
            };
            this.listView1.Columns.Add(ch);

            ch = new ColumnHeader()
            {
                Text = "Description",
                Width = 230,
            };
            this.listView1.Columns.Add(ch);

            FillListView(list);
        }

        private void FillListView(List<ChambersModbusData> list)
        {
            this.listView1.Items.Clear();

            foreach (ChambersModbusData cmd in list)
            {
                ListViewItem item = new ListViewItem();

                item.Text = cmd.PrimaryNum.ToString();

                {
                    System.Windows.Forms.ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                    subItem.Text = cmd.Address.ToString();
                    item.SubItems.Add(subItem);
                }

                {
                    System.Windows.Forms.ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                    subItem.Text = cmd.Value.ToString();
                    item.SubItems.Add(subItem);
                }

                {
                    System.Windows.Forms.ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                    subItem.Text = cmd.UpdateValue.ToString();
                    item.SubItems.Add(subItem);
                }

                {
                    System.Windows.Forms.ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                    subItem.Text = cmd.Desc.ToString();
                    item.SubItems.Add(subItem);
                }

                this.listView1.Items.Add(item);
            }
        }

        private void SetupListView2(List<bool> list2)
        {
            this.listView2.View = System.Windows.Forms.View.Details;

            ColumnHeader ch = new ColumnHeader()
            {
                Text = "LED NUM",
                Width = 60
            };
            this.listView2.Columns.Add(ch);

            ch = new ColumnHeader()
            {
                Text = "LED STATUS",
                Width = 150
            };
            this.listView2.Columns.Add(ch);


            FillListView2(list2);
        }

        private void FillListView2(List<bool> list2)
        {
            this.listView2.Items.Clear();
            int i;
            i=1;
            foreach (bool cmd in list2)
            {
                ListViewItem item2 = new ListViewItem();
                
                item2.Text = i.ToString();

                {
                    System.Windows.Forms.ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                    subItem.Text = cmd.ToString();
                    item2.SubItems.Add(subItem);
                }

                this.listView2.Items.Add(item2);
                i++;
            }
        }

        static string sum_msg = null;

        private void setGui(string strMsg)  // 화면 표시
        {
            string msg1 = null;
            string msg2 = null;
            
            int index = 0;
            int len = 0;

            if (strMsg == "") return;

            try
            {
                UpdateTextBox(strMsg);
                
                /*   
                char[] charArr = strMsg.ToCharArray();
              
                if (charArr[strMsg.Length-1] == '\n')
                {

                    sum_msg += strMsg;
                    if (sum_msg != null)
                    {
                        UpdateTextBox(sum_msg);
                        sum_msg = "";
                    }
                }
                else
                {
                    sum_msg += strMsg;
                }
                */
                
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        private void updateListViewGui()  // 화면 표시
        {
            

            try
            {
                MemoryListView();
                LedStatusListView();

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private void COM_OPEN_BUTTON_Click(object sender, EventArgs e)
        {

            if (com_open_button_flag == true)
            {

                if (m_sp1 != null && m_sp1.IsOpen == true)
                {
                    com_open_button_flag = false;
                    COM_OPEN_BUTTON.Text = "Open";
                    //cSerial.PortClose();
                    m_sp1.Close();
                    textBox2.AppendText("Close" + "\r\n");
                }
                
            }
            else
            {
                m_sp1 = new SerialPort();

                try
                {
                        // m_sp1 값이 null 일때만 새로운 SerialPort 를 생성합니다.
                    if (null != m_sp1)
                    {
                        m_sp1 = new SerialPort();
                        m_sp1.PortName = commPort;   // 컴포트명
                        m_sp1.BaudRate = Convert.ToInt32(BaudRate);   // 보레이트
                        m_sp1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBit);
                        m_sp1.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                        m_sp1.DataReceived += new SerialDataReceivedEventHandler(EventDataReceived);
                        m_sp1.Open();
                    }

                    if (m_sp1.IsOpen == true)
                    {
                        textBox2.AppendText("Open" + "\r\n");
                        com_open_button_flag = true;
                        COM_OPEN_BUTTON.Text = "Close";
                    }
                    else
                    {
                        textBox2.AppendText("Can't port" + "\r\n");
                    }
                    }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    m_sp1.Dispose();
                    m_sp1 = null;
                }        
            }
        }

        private string strReturn = "";
       
      


        private void UpdateTextBox(string data)
        {
            
            if (textBox3.InvokeRequired)
            {
                // 작업쓰레드인 경우
                textBox3.BeginInvoke(new Action(() => textBox3.AppendText(data)));
            }
            else
            {
                textBox3.AppendText(data);
            }
           

        }
        private void UpdateTextBoxTransmit(string data)
        {

            if (textBox1.InvokeRequired)
            {
                // 작업쓰레드인 경우
                textBox1.BeginInvoke(new Action(() => textBox1.AppendText(data)));
            }
            else
            {
                textBox1.AppendText(data);
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            String comStr = "";
            String parity_disp = "";
            //Even-2,Mark-3,None-0,Odd-1,Space-4
            if (parity.Equals("0")) parity_disp = "None";
            if (parity.Equals("1")) parity_disp = "Odd";
            if (parity.Equals("2")) parity_disp = "Even";
            if (parity.Equals("3")) parity_disp = "Mark";
            if (parity.Equals("4")) parity_disp = "Space";

            comStr = commPort + " "
                     + dataBits + " "
                     + BaudRate + " "
                     + stopBit + " "
                     + parity_disp + " ";
            
            
            textBox2.AppendText(comStr + "\r\n");
        }



        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            commPort = this.comboBox1.SelectedItem.ToString();
            textBox2.AppendText(commPort + "\r\n");

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            BaudRate = this.comboBox2.SelectedItem.ToString();
            textBox2.AppendText(BaudRate + "\r\n");
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int a = this.comboBox3.SelectedIndex;
            parity = a.ToString();
            String str = this.comboBox3.SelectedItem.ToString();
            textBox2.AppendText(str + "\r\n");
            
        }

        
        private void button4_Click_1(object sender, EventArgs e)
        {
            textBox3.Clear();
        }
        

        private void eXITToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
            Application.Exit();
        }


        //Serial Port Event
        void EventDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            mIRecSize = m_sp1.BytesToRead; // 수신된 데이터 갯수
            string strRxData;
            ///*
            if (mIRecSize != 0)
            {
                strRxData = "";
                m_sp1.Read(mBuff, 0, mIRecSize);

                for (int z = 0; z < mIRecSize; z++)
                {
                    strRxData += mBuff[z].ToString("X2");
                    rx_mod_buf[rx_pos] = mBuff[z];
                    rx_pos++;
                }
                //strRxData += "\r\n";
                mIRecSize = 0;

                if (strRxData != null && strRxData != "")
                {
                    this.BeginInvoke(setChangeText, strRxData);
                }
                crlf_count = 5;
            }
            
        }

        private void button15_Click(object sender, EventArgs e)
        {
            byte[] res_crc = new byte[2];
            tx_mod_buf[0] = 0x01;
            tx_mod_buf[1] = 0x04;
            tx_mod_buf[2] = 0x03;
            tx_mod_buf[3] = 0xEB;
            tx_mod_buf[4] = 0x00;
            tx_mod_buf[5] = 0x01;

            res_crc = mrc.fn_makeCRC16_byte(tx_mod_buf, 6);

            MessageBox.Show(res_crc[0].ToString("x2") + "," + res_crc[1].ToString("x2"));
        }

        private void button6_Click(object sender, EventArgs e)  //Read Input Registers(Func 04H)
        {
            byte[] res_crc = new byte[2];
            tx_mod_buf[0] = (byte)UInt16.Parse(modTextBox1.Text);

            tx_mod_buf[1] = (byte)UInt16.Parse(modTextBox2.Text);

            UInt16 a = UInt16.Parse(modTextBox3.Text);
            mod4haddress = a;
            byte b_high = (byte)(a >> 8);
            byte b_low = (byte)a;
            tx_mod_buf[2] = b_high;
            tx_mod_buf[3] = b_low;
            a = UInt16.Parse(modTextBox4.Text);
            b_high = (byte)(a >> 8);
            b_low = (byte)a;
            tx_mod_buf[4] = b_high;
            tx_mod_buf[5] = b_low;

            res_crc = mrc.fn_makeCRC16_byte(tx_mod_buf, 6);
            tx_mod_buf[6] = res_crc[0];
            tx_mod_buf[7] = res_crc[1];

            m_sp1.Write(tx_mod_buf, 0, 8);
            //MessageBox.Show(res_crc[0].ToString("x2") + "," + res_crc[1].ToString("x2"));
        }

        string mod_send(UInt16 slave, UInt16 function, UInt16 addr, UInt16 lenordata)
        {
            byte[] res_crc = new byte[2];
            string ret = "";
            int c=0;
            if (com_open_button_flag == true)
            {
                tx_mod_buf[0] = (byte)slave;

                tx_mod_buf[1] = (byte)function;
                
                UInt16 a = addr;
                mod4haddress = a;
                byte b_high = (byte)(a >> 8);
                byte b_low = (byte)a;
                tx_mod_buf[2] = b_high;
                tx_mod_buf[3] = b_low;
                a = lenordata;
                b_high = (byte)(a >> 8);
                b_low = (byte)a;
                tx_mod_buf[4] = b_high;
                tx_mod_buf[5] = b_low;
                if (function != 0x10)
                {
                    res_crc = mrc.fn_makeCRC16_byte(tx_mod_buf, 6);
                    tx_mod_buf[6] = res_crc[0];
                    tx_mod_buf[7] = res_crc[1];

                    m_sp1.Write(tx_mod_buf, 0, 8);
                    byte[] buf = new byte[8];
                    for (int i = 0; i < 8; i++) buf[i] = tx_mod_buf[i];

                    ret =  string.Concat(Array.ConvertAll(buf, byt => byt.ToString("X2")));
                    ret+="\r\n";
                    return ret;
                }
                else  
                {
                    if (lenordata > 100) return "error\r\n";
                    tx_mod_buf[6] = (byte)(lenordata * 2);
                    
                    for (int i = 0; i < tx_mod_buf[6]; i++)
                    {
                        if (i % 2 == 0)
                        {
                            tx_mod_buf[i+7] = (byte)(modbusMemoryUpdate[addr + i / 2]>>8);
                        }
                        else
                        {
                            tx_mod_buf[i+7] = (byte)modbusMemoryUpdate[addr + i / 2];
                        }
                        
                    }
                    c = tx_mod_buf[6] + 6;
                    res_crc = mrc.fn_makeCRC16_byte(tx_mod_buf, c+1);
                    tx_mod_buf[c+1] = res_crc[0];
                    tx_mod_buf[c+2] = res_crc[1];
                    c += 3;
                    m_sp1.Write(tx_mod_buf, 0, c); 
                    byte[] buf = new byte[c];
                    for (int i = 0; i < c; i++) buf[i] = tx_mod_buf[i];

                    ret = string.Concat(Array.ConvertAll(buf, byt => byt.ToString("X2")));
                    ret += "\r\n";
                    return ret;
                }
            }
            else return "error\r\n";
            
            
            
        }


        private void button1_Click(object sender, EventArgs e)
        {
            t.Abort();
            Application.Exit();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked == true)
            {
                // Connect
                checkBox1.Text = "ON";
                mod_send(1,5,2,1);
            }
            else
            {
                // Disconnect
                checkBox1.Text = "OFF";
                mod_send(1,5,2,0);
            }

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                // Connect
                checkBox2.Text = "ON";
                mod_send(1, 5, 3, 1);
            }
            else
            {
                // Disconnect
                checkBox2.Text = "OFF";
                mod_send(1, 5, 3, 0);
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true)
            {
                // Connect
                checkBox3.Text = "ON";
                mod_send(1, 5, 4, 1);
            }
            else
            {
                // Disconnect
                checkBox3.Text = "OFF";
                mod_send(1, 5, 4, 0);
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true)
            {
                // Connect
                checkBox4.Text = "ON";
                mod_send(1, 5, 5, 1);
            }
            else
            {
                // Disconnect
                checkBox4.Text = "OFF";
                mod_send(1, 5, 5, 0);
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked == true)
            {
                // Connect
                checkBox5.Text = "ON";
                mod_send(1, 5, 6, 1);
            }
            else
            {
                // Disconnect
                checkBox5.Text = "OFF";
                mod_send(1, 5, 6, 0);
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked == true)
            {
                // Connect
                checkBox6.Text = "ON";
                mod_send(1, 5, 7, 1);
            }
            else
            {
                // Disconnect
                checkBox6.Text = "OFF";
                mod_send(1, 5, 7, 0);
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked == true)
            {
                // Connect
                checkBox7.Text = "ON";
                mod_send(1, 5, 8, 1);
            }
            else
            {
                // Disconnect
                checkBox7.Text = "OFF";
                mod_send(1, 5, 8, 0);
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked == true)
            {
                // Connect
                checkBox8.Text = "ON";
                mod_send(1, 5, 9, 1);
            }
            else
            {
                // Disconnect
                checkBox8.Text = "OFF";
                mod_send(1, 5, 9, 0);
            }
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9.Checked == true)
            {
                // Connect
                checkBox9.Text = "ON";
                mod_send(1, 5, 10, 1);
            }
            else
            {
                // Disconnect
                checkBox9.Text = "OFF";
                mod_send(1, 5, 10, 0);
            }
        }


        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox11.Checked == true)
            {
                // Connect
                checkBox11.Text = "ON";
                mod_send(1, 5, 11, 1);
            }
            else
            {
                // Disconnect
                checkBox11.Text = "OFF";
                mod_send(1, 5, 11, 0);
            }
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox12.Checked == true)
            {
                // Connect
                checkBox12.Text = "ON";
                mod_send(1, 5, 12, 1);
            }
            else
            {
                // Disconnect
                checkBox12.Text = "OFF";
                mod_send(1, 5, 12, 0); 
            }
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox13.Checked == true)
            {
                // Connect
                checkBox13.Text = "ON";
                mod_send(1, 5, 13, 1);
            }
            else
            {
                // Disconnect
                checkBox13.Text = "OFF";
                mod_send(1, 5, 13, 0);
            }
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox10.Checked == true)
            {
                // Connect
                checkBox9.Text = "ON";
                mod_send(1, 5, 1, 1);
            }
            else
            {
                // Disconnect
                checkBox10.Text = "OFF";
                mod_send(1, 5, 1, 0);
            }
        }




        private void button3_Click(object sender, EventArgs e)
        {
            //string str = "abc\r\n";
            modbusMemoryUpdate[301] = 21; modbusMemoryUpdate[306] = 71;
            modbusMemoryUpdate[302] = 31; modbusMemoryUpdate[307] = 81;
            modbusMemoryUpdate[303] = 41; modbusMemoryUpdate[308] = 91;
            modbusMemoryUpdate[304] = 51; modbusMemoryUpdate[309] = 101;
            modbusMemoryUpdate[305] = 61; modbusMemoryUpdate[310] = 111;

            //UpdateTextBoxTransmit(mod_send(1, 0x10, 301, 10));
            UpdateTextBoxTransmit(mod_send(1, 6, 301, 0));
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            curItem = listView1.GetItemAt(e.X, e.Y);
            if (curItem == null)
                return;

            curSB = curItem.GetSubItemAt(e.X, e.Y);
            int idxSub = curItem.SubItems.IndexOf(curSB);

            switch (idxSub)
            {
                case 5: //5번째 subItem만 수정가능하게
                    break;
                default: return;
            }
            int ILeft = curSB.Bounds.Left + 2;
            int IWidth = curSB.Bounds.Width;
            textBox4.SetBounds(ILeft + listView1.Left, curSB.Bounds.Top +
                 listView1.Top, IWidth, curSB.Bounds.Height);

            textBox1.Text = curSB.Text;
            textBox1.Show();
            textBox1.Focus();

        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            //엔터키 수정 ESC키 취소
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.Enter:
                    cancelEdit = false;
                    e.Handled = true;
                    textBox4.Hide();
                    break;
                case System.Windows.Forms.Keys.Escape:
                    cancelEdit = true;
                    e.Handled = true;
                    textBox4.Hide();
                    break;
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            textBox4.Hide();
            if(cancelEdit == false)
            {
                if(textBox4.Text.Trim() != "")
                {
                    curSB.Text = textBox4.Text;

                    int idxSub = curItem.SubItems.IndexOf(curSB);
                    int idx = curItem.Index;

                    Console.Write(curSB.Text);
                }
            }
            else
            {
                cancelEdit = false;
            }
            listView1.Focus();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            UInt16 addr = UInt16.Parse(textBox5.Text);
            modbusMemoryUpdate[addr] = UInt16.Parse(textBox6.Text);
         
            
            MemoryListView();
        }
        byte coil_test_flag = 0;
        private void button7_Click(object sender, EventArgs e)
        {
            byte[] res_crc = new byte[2];
            if (++coil_test_flag > 1) coil_test_flag = 0;
            
            tx_mod_buf[0] = 0x01;

            tx_mod_buf[1] = 0x05;

            UInt16 a = 3;
            mod4haddress = a;
            byte b_high = (byte)(a >> 8);
            byte b_low = (byte)a;
            tx_mod_buf[2] = b_high;
            tx_mod_buf[3] = b_low;
            if(coil_test_flag==1)a = 0xFF00;
            else a = 0x0000;
            b_high = (byte)(a >> 8);
            b_low = (byte)a;
            tx_mod_buf[4] = b_high;
            tx_mod_buf[5] = b_low;

            res_crc = mrc.fn_makeCRC16_byte(tx_mod_buf, 6);
            tx_mod_buf[6] = res_crc[0];
            tx_mod_buf[7] = res_crc[1];

            m_sp1.Write(tx_mod_buf, 0, 8);
            //MessageBox.Show(res_crc[0].ToString("x2") + "," + res_crc[1].ToString("x2"));
        }

        private void radioButtonOff_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonOff.Checked)
            {
                label33.Text = "OFF";
                UpdateTextBoxTransmit(mod_send(1, 6, 301, 0));
            }
            
        }

        private void radioButtonStep1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonStep1.Checked)
            {
                label33.Text = "약";
                UpdateTextBoxTransmit(mod_send(1, 6, 301, 1));
            }

        }

        private void radioButtonStep2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonStep2.Checked)
            {
                label33.Text = "중";
                UpdateTextBoxTransmit(mod_send(1, 6, 301, 2));
            }

        }

        private void radioButtonStep3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonStep3.Checked)
            {
                label33.Text = "강";
                UpdateTextBoxTransmit(mod_send(1, 6, 301, 3));
            }

        }

        private void button8_Click(object sender, EventArgs e)
        {
            modbusMemory[Constants.MOD_STATUS_LED] = 0x8080;
            LedStatusListView();
        }


        

         

   






    }

    public class ChambersModbusData
    {
        public int PrimaryNum;
        public int Address;
        public UInt16 Value;
        public UInt16 UpdateValue;
        public String Desc;

        public ChambersModbusData(int primaryNum, int address, UInt16 value, UInt16 updateValue, String desc)
        {
            PrimaryNum = primaryNum;
            Address = address;
            Value = value;
            UpdateValue = updateValue;
            Desc = desc;
        }

        public void setup(int primaryNum, int address, UInt16 value, UInt16 updateValue, String desc)
        {
            PrimaryNum = primaryNum;
            Address = address;
            Value = value;
            UpdateValue = updateValue;
            Desc = desc;
        }

        public string[] Values()
        {
            return new string[]
            {
                PrimaryNum.ToString(), Address.ToString(),Value.ToString(),UpdateValue.ToString(),Desc
            };
        }
    }
}
