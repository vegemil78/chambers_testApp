using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO.Ports;
using System.Threading;
namespace Util
{
    class BuffControl
    {
        private string ERRMSG = "_END_";
        private int Head = 0;
        private int Tail = 0;
        private int BUFF_SIZE = 0;
        private byte[] Data;
        private byte[] TempData;
        
        public BuffControl(int buffsize)
        {
            BUFF_SIZE = buffsize;
            Data = new byte[BUFF_SIZE];
            TempData = new byte[BUFF_SIZE];
        }

        public void RxInBuff(byte[] buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Data[Head] = buffer[i];
                if (Head >= BUFF_SIZE - 1)
                {
                    Head = 0;
                }
                else
                {
                    Head++;
                }
            }
        }

        public void RxInBuff(char[] buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Data[Head] = (byte)buffer[i];
                if (Head >= BUFF_SIZE - 1)
                {
                    Head = 0;
                }
                else
                {
                    Head++;
                }
            }
        }

        public void RxInBuff(string buffer)
        {
            char[] temp = buffer.ToCharArray();

            RxInBuff(temp, temp.Length);
            /*
            for (int i = 0; i < temp.Length; i++)
            {
                Data[Head] = (byte)temp[i];

                if (Head >= (BUFF_SIZE - 1))
                {
                    Head = 0;
                }
                else
                {
                    Head++;
                }
            }
            */
        }

        public string ReceivePacket()
        {
            string str = null;

            int buffcount = 0;
            int temptail = 0;
            bool checktail = true;


            ///////////////////////////////////////////////////////////////////////////////////////////////
            if (Head == Tail)
            {
                return ERRMSG;
            }
            else
            {
                temptail = Tail;
            }
            ///////////////////////////////////////////////////////////////////////////////////////////////
            do
            {
                if (Data[temptail] != 10)//<LF>=EXT
                {
                    TempData[buffcount] = Data[temptail];

                    if (temptail >= (BUFF_SIZE - 1)) temptail = 0;
                    else temptail++;

                    if (Head == temptail) return ERRMSG;
                }
                else
                {
                    TempData[buffcount] = Data[temptail];

                    if (temptail >= (BUFF_SIZE - 1)) temptail = 0;
                    else temptail++;


                    checktail = false;
                }
                buffcount++;
            }
            while (checktail);

            Tail = temptail;
            ///////////////////////////////////////////////////////////////////////////////////////////////
            //데이터 수신 처리//
            ///////////////////////////////////////////////////////////////////////////////////////////////
            if (buffcount != 0)
            {
                str = null;
                for (int i = 0; i < buffcount; i++)
                {
                    if (TempData[i] != 10 && TempData[i] != 13)
                    {
                        str += (char)TempData[i];
                    }
                }
                if (str == null || str == "")
                {
                    return ERRMSG;
                }
                else
                {
                    return str;
                }
            }
            else
            {
                return ERRMSG;
            }
        }
    }
    
    public class clsSerial
    {
        private string ERRMSG = "_END_";
        public SerialPort serial = new SerialPort();
        BuffControl USART;
        Queue<string> RxData = new Queue<string>();
        Queue<string> RxPacket = new Queue<string>();
        public int test_rx_thead_count = 0;
        public int test_rx_count1 = 0;
        public int test_rx_count2 = 0;
        public int test_rx_count3 = 0;
        public string PortName
        {
            get { return serial.PortName; }
            set { serial.PortName = value; }
        }
        public int BaudRate
        {
            get { return serial.BaudRate; }
            set { serial.BaudRate = value; }
        }
        public int DataBits
        {
            get { return serial.DataBits; }
            set { serial.DataBits = value; }
        }
        public Parity Parity
        {
            get { return serial.Parity; }
            set { serial.Parity = value; }
        }
        public StopBits StopBits
        {
            get { return serial.StopBits; }
            set { serial.StopBits = value; }
        }
        public bool RtsEnable
        {
            get { return serial.RtsEnable; }
            set { serial.RtsEnable = value; }
        }
        public bool IsOpen
        {
            get { return serial.IsOpen; }
        }

        public clsSerial(string portName, int baudRate, int rxBuffSize)
        {
            PortName = portName;
            BaudRate = baudRate;
            DataBits = 8;
            StopBits = System.IO.Ports.StopBits.One;
            Parity = System.IO.Ports.Parity.None;
            RtsEnable = false;

            USART = new BuffControl(rxBuffSize);
        }


        public clsSerial(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
        {
            PortName = portName;
            BaudRate = baudRate;
            DataBits = dataBits;
            StopBits = stopBits;
            Parity = parity;
            //RtsEnable = rtsEnable;

            //USART = new BuffControl(rxBuffSize);
        }

        public clsSerial(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity, bool rtsEnable, int rxBuffSize)
        {
            PortName = portName;
            BaudRate = baudRate;
            DataBits = dataBits;
            StopBits = stopBits;
            Parity = parity;
            RtsEnable = rtsEnable;

            USART = new BuffControl(rxBuffSize);
        }

        public bool PortOpen()
        {
            try
            {
                serial.Open();
                //Thread RxThread = new Thread(new ThreadStart(ReceiveThread));
                //RxThread.IsBackground = true;
                //RxThread.Start();
                return true;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }

        public bool PortClose()
        {
            serial.Close();
            return IsOpen;
        }

        public void sendData(string strMsg)
        {
            //serial.WriteLine(strMsg);
            if(serial.IsOpen == true)
               serial.Write(strMsg + "\r\n");
        }

        public void sendLoopcardData(string strMsg)
        {

            char[] dataByte = new char[10];
            //serial.WriteLine(strMsg);
            if (serial.IsOpen == true) {
                dataByte[0] = Convert.ToChar(0x02);
                dataByte[1] = 'V';
                dataByte[2] = 'A';
                dataByte[3] = '1';
                dataByte[4] = '0';
                dataByte[5] = Convert.ToChar(0x03);
                string charsStr = new string(dataByte);
                Console.WriteLine(charsStr);
                serial.Write(charsStr);
                
           }
        }

        private string RXDLoop = null;
        public string ReadData = null;
        private const int QueueOverBuff = 1000;
        /*
        private void ReceiveThread()
        {
            try
            {
                while (serial.IsOpen)
                {
                    try
                    {
                        ReadData = serial.ReadExisting();
                        test_rx_thead_count++;
                        
                        
                        if (ReadData.Length != 0 && ReadData != null)
                        {
                            if (RxData.Count > QueueOverBuff)
                            {
                                RxData.Clear();
                            }
                            else
                            {
                                RxData.Enqueue(ReadData);
                                
                            }

                            USART.RxInBuff(ReadData.ToCharArray(), ReadData.Length);
                        }
                        
                        RXDLoop = null;
                        
                        while (RXDLoop != ERRMSG)
                        {
                            RXDLoop = USART.ReceivePacket();

                            if (RXDLoop != ERRMSG)
                            {
                                if (RxPacket.Count > QueueOverBuff)
                                {
                                    RxPacket.Clear();
                                }
                                else
                                {
                                    RxPacket.Enqueue(RXDLoop);
                                    test_rx_count1 = ReadData.Length;
                                }
                            }
                        }
                       
                        Thread.Sleep(1);
                    }
                    catch { };
                }
            }
            catch { };
        }
         */

        private void ReceiveThread()
        {
            try
            {
                while (serial.IsOpen)
                {
                    try
                    {
                        ReadData = serial.ReadExisting();
                   


                        if (ReadData.Length != 0 && ReadData != null)
                        {
                            if (RxData.Count > QueueOverBuff)
                            {
                                RxData.Clear();
                            }
                            else
                            {
                                RxData.Enqueue(ReadData);

                            }

                            USART.RxInBuff(ReadData.ToCharArray(), ReadData.Length);
                        }

                        RXDLoop = null;

                        while (RXDLoop != ERRMSG)
                        {
                            RXDLoop = USART.ReceivePacket();

                            if (RXDLoop != ERRMSG)
                            {
                                if (RxPacket.Count > QueueOverBuff)
                                {
                                    RxPacket.Clear();
                                }
                                else
                                {
                                    RxPacket.Enqueue(RXDLoop);
                                    test_rx_count1 = ReadData.Length;
                                }
                            }
                        }

                        Thread.Sleep(1);
                    }
                    catch { };
                }
            }
            catch { };
        }



        public int IsRxDataCount
        {
            get { return RxData.Count; }
        }
        public string IsRxData
        {
            get { return RxData.Dequeue(); }
        }
        public int IsRxPacketCount
        {
            get { return RxPacket.Count; }
        }
        public string IsRxPacket
        {
            get { return RxPacket.Dequeue(); }
        }
        public string IsTxData
        {
            set { serial.Write(value); }
        }   
    }
}


