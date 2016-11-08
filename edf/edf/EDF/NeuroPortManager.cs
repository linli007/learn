using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Collections;
using System.Timers;
using System.Threading;
using System.Windows.Forms;

namespace ElectroencephalographController
{
    /// <summary>
    /// 接收串口类  --by wdp
    /// </summary>
    public class NeuroPortManager
    {
        private string portNumber = null;                          // 串口号
        private string portBaud = null;                            // 波特率
        private SerialPort portNeuro = null;                          // 设备串口
        private byte[] allMsg;//  串口一次收到的所有消息
        //private byte[] EEGLeft;//每次从串口获取完整报告后剩余的数据，如假设一次读取到的数据为A5 A5 11 01 AA AA A5 A5 22，则该变量中存放的是A5 A5 22
        private int hardware=0x010101;//hardwareCmd = "A5A59561FFFFFF0BAAAA";
        private int firmware=0x010101;//firmwareCmd = "A5A59562FFFFFF08AAAA";
        private int software=0x010101;//softwareCmd = "A5A59563FFFFFF09AAAA";
        private string[] powerOnTestCmd = new string[4];
        private int registerConfig3_BiasSignal;
        private int registerLoff_ElectrodeDroppedOff;
        private int[] channelConfig;
        public int loff_Sensp;//0x4F	LOFF_SENSP	写：掉落检测正极配置（注2）
        public int loff_Senen;//0x50	LOFF_SENSN	写：掉落检测负极配置（注2）
        public int mm;
        public int nn;
        public int jj;//0x71	0xmm 0xnn 0xjj,其中mm、nn、jj的取值可以是：
        //0. 50 sps
        //1. 100 sps
        //2. 200 sps（默认）
        //3. 250 sps
        //4. 500 sps
        //5. 1k sps
        //	mm、nn、jj分别对应1-8、9-16、17-24通道的采样率，一般mm=nn=jj，但也可不同。
        //例如0x00 0x05 0x02表示1-8、9-16、17-24通道采样率为50sps、1ksps、200sps
        private int leadCode;

        public bool IsOpen
        {
            get { return portNeuro == null ? false : portNeuro.IsOpen; }
        }

        public delegate void ReceivedAllMsgEventHandler(byte[] msg);
        public event ReceivedAllMsgEventHandler ReceivedAllMsgEvent;

        /// <summary>
        /// 串口配置
        /// </summary>
        /// <param name="number"></param>
        /// <param name="baud"></param>
        /// <returns 1 错误></returns 0 正常>
        public int CommNeuroConfig(string number, string baud)
        {
            portNumber = number;
            portBaud = baud;
            try
            {
                portNeuro = new SerialPort(portNumber, Convert.ToInt32(portBaud), Parity.None, 8, StopBits.One);
                portNeuro.DataReceived -= new SerialDataReceivedEventHandler(CommRxOneMessage);
                portNeuro.DataReceived += new SerialDataReceivedEventHandler(CommRxOneMessage);
                portNeuro.RtsEnable = true;                            //  打开RTS，这一步很重要

                return 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("无法配置脑电仪串口! ");
                return 1;
            }
        }
		
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns 0 正常打开></returns 1 无法打开>
        public int CommNeuroOpen()
        {
            try
            {
                if (false == portNeuro.IsOpen)
                    portNeuro.Open();
                return 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("无法打开串口! \n" + ex.Message + "\n");
                return 1;
            }
        }
		
        /// <summary>
        /// 打开串口,无提示
        /// </summary>
        /// <returns 0 正常打开></returns 1 无法打开>
        public int CommNeuroOpen_WithoutWarning()
        {
            try
            {
                if (false == portNeuro.IsOpen)
                    portNeuro.Open();
                return 0;
            }
            catch (System.Exception ex)
            {
                //MessageBox.Show("无法打开串口! \n" + ex.Message + "\n");
                return 1;
            }
        }
		
        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns 0 正常关闭></returns 1 无法关闭>
        public int CommNeuroClose()
        {
            try
            {
                if (true == portNeuro.IsOpen)
                    portNeuro.Close();
                return 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("无法关闭串口! \n" + ex.Message + "\n");
                return 1;
            }
        }
        /// <summary>
        /// 串口发送
        /// </summary>
        /// <param name="msg">发送成功返回0，失败返回1</param>
        public int CommTxOneMessage(string msg)
        {
            try
            {
                portNeuro.Write(msg);
                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
        /// <summary>
        /// 串口发送0xA5A5开头的数据
        /// </summary>
        /// <param name="msg">字符串格式的十六进制数</param>
        /// <returns></returns>
        public int CommTxOneA5A5Message(string msg)
        {
            try
            {
                byte[] transmitA5A5Message = HexToByte(msg);
                portNeuro.Write(transmitA5A5Message, 0, transmitA5A5Message.Length);
                LogHelper.WriteLog(typeof(NeuroControl), "向串口发送命令"+msg);
                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
        /// <summary>
        /// 串口发送0xA5A5开头的数据
        /// </summary>
        /// <param name="opt">数据类型</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public int CommTxOneA5A5Message(string opt,string data)
        {
            string msg;
            string typ_opt_data = "95" + opt + data;
            msg = "A5A5" + typ_opt_data + GenerateCheckedSum(typ_opt_data) + "AAAA";
            return CommTxOneA5A5Message(msg);
        }

        /// <summary>
        /// 数据校验函数
        /// </summary>
        /// <param name="typ_opt_data">数据类型和数据</param>
        /// <returns>0,校验正确，非0，校验错误</returns>
        public string GenerateCheckedSum(string typ_opt_data)
        {
            Int16 i;
            byte checksum = 0;
            byte[] buf = HexToByte(typ_opt_data);
            for (i = 0; i < buf.Length; i++)
            {
                checksum ^= buf[i];
            }
            string checkedsum_str = checksum.ToString("X2");
            return checkedsum_str;
        }

        /// <summary>
        /// 由数据类型和数据值产生上位机控制命令
        /// </summary>
        /// <param name="typ">数据类型,0x95</param>
        /// <param name="opt">数据类型</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public string GenerateMsgCmd(string typ,string opt, string data)
        {
            string typ_opt_data = typ + opt + data;
            string msgCmd = "A5A5" + typ_opt_data + GenerateCheckedSum(typ_opt_data) + "AAAA";
            return msgCmd;
        }
        /// <summary>
        /// 开机流程检测，读取下位机硬件版本号、软件版本号、固件版本号，并写入各个寄存器
        /// </summary>
        /// <returns>0，开机自检通过，大于0，开机自检有问题</returns>
        public int PowerOnTest()
        {
            //powerOnTestCmd[0] = GenerateMsgCmd("95", "43", registerConfig3_BiasSignal.ToString("X6"));//写：Bias信号相关（注2），0x43
            //powerOnTestCmd[1] = GenerateMsgCmd("95", "44", registerLoff_ElectrodeDroppedOff.ToString("X6"));//写：电极掉落检测配置相关（注2）,0x44
            //powerOnTestCmd[2] = GenerateMsgCmd("95", "45", channelConfig[0].ToString("X6"));//写：通道1配置相关（注2）,0x45
            //powerOnTestCmd[3] = GenerateMsgCmd("95", "46", channelConfig[1].ToString("X6"));//写：通道2配置相关（注2）,0x46
            //powerOnTestCmd[4] = GenerateMsgCmd("95", "47", channelConfig[2].ToString("X6"));//写：通道3配置相关（注2）,0x47
            //powerOnTestCmd[5] = GenerateMsgCmd("95", "48", channelConfig[3].ToString("X6"));//写：通道4配置相关（注2）,0x48
            //powerOnTestCmd[6] = GenerateMsgCmd("95", "49", channelConfig[4].ToString("X6"));//写：通道5配置相关（注2）,0x49
            //powerOnTestCmd[7] = GenerateMsgCmd("95", "4a", channelConfig[5].ToString("X6"));//写：通道6配置相关（注2）,0x4a
            //powerOnTestCmd[8] = GenerateMsgCmd("95", "4b", channelConfig[6].ToString("X6"));//写：通道7配置相关（注2）,0x4b
            //powerOnTestCmd[9] = GenerateMsgCmd("95", "4c", channelConfig[7].ToString("X6"));//写：通道8配置相关（注2）,0x4c
            //powerOnTestCmd[10] = GenerateMsgCmd("95", "4F", loff_Sensp.ToString("X6"));//写：掉落检测正极配置（注2）,0x4F
            //powerOnTestCmd[11] = GenerateMsgCmd("95", "50", loff_Senen.ToString("X6"));//写：掉落检测负极配置（注2）,0x50
            //powerOnTestCmd[12] = GenerateMsgCmd("95", "71", mm.ToString("X2") + nn.ToString("X2") + jj.ToString("X2"));//写：掉落检测负极配置（注2）,0x71
            powerOnTestCmd[0] = GenerateMsgCmd("95", "61", hardware.ToString("X6"));// hardwareCmd;//读：硬件电路板版本号,opt=0x61
            powerOnTestCmd[1] = GenerateMsgCmd("95", "62", firmware.ToString("X6"));// firmwareCmd;//读：固件版本号,opt=0x62
            powerOnTestCmd[2] = GenerateMsgCmd("95", "63", software.ToString("X6")); //softwareCmd;//读：上位机软件版本号,opt=0x63
            powerOnTestCmd[3] = GenerateMsgCmd("95", "0B", leadCode.ToString("X6"));//写：通道1-24是否开关标志，0xfff8表示1-24通道全开
            int powerOnTestSucceed=0;
            int timeOut = 5;
            while (timeOut > 0)
            {
                powerOnTestSucceed = 0;
                for (int i = 0; i < powerOnTestCmd.Length; i++)
                {
                    powerOnTestSucceed += CommTxOneA5A5Message(powerOnTestCmd[i]);                    
                    Thread.Sleep(10);
                }
                //若返回值均为0，则所有自检命令全部发送成功，若不为0，则有部分命令未成功发送，过一段时间（100ms）后重新自检一次，最多自检5次，
                if (powerOnTestSucceed > 0)
                {
                    timeOut--;
                    Thread.Sleep(100);
                }
                else
                    break;
            }
            return powerOnTestSucceed;
        }

        /// <summary>
        /// 将公共数据池中的数据传递给串口操作对象
        /// </summary>
        /// <param name="registerConfig3_BiasSignal">Config3寄存器值，Bias信号相关</param>
        /// <param name="register_Loff_ElectrodeDroppedOff">LOFF寄存器值，电极脱落检测配置相关</param>
        /// <param name="channelConfig">通道1-8配置相关，长度为8的整型数组</param>
        /// <param name="loff_Sensp">掉落检测正极配置</param>
        /// <param name="loff_Senen">掉落检测负极配置</param>
        /// <param name="mm">1-8通道的采样率</param>
        /// <param name="nn">9-16通道的采样率</param>
        /// <param name="jj">17-24通道的采样率</param>
        public void SetRegisterChannelPara(int hardware,int firmware,int software,int registerConfig3_BiasSignal, int registerLoff_ElectrodeDroppedOff, int[] channelConfig, int loff_Sensp, int loff_Senen,int mm,int nn,int jj,int leadCodeList)
        {
            this.hardware = hardware;
            this.firmware = firmware;
            this.software = software;
            this.registerConfig3_BiasSignal = registerConfig3_BiasSignal;
            this.registerLoff_ElectrodeDroppedOff = registerLoff_ElectrodeDroppedOff;
            this.channelConfig = channelConfig;
            this.loff_Sensp = loff_Sensp;
            this.loff_Senen = loff_Senen;
            this.mm = mm;
            this.nn = nn;
            this.jj = jj;
            this.leadCode = leadCodeList;
        }

        /// <summary>
        /// 从string类型转换到hex
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// 串口接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommRxOneMessage(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //读取缓冲区数据长度
                int bytesToRead = portNeuro.BytesToRead;
                if (bytesToRead > 0)
                {
                    allMsg = new byte[bytesToRead];
                    if (portNeuro.Read(allMsg, 0, bytesToRead) > 0)
                    {
                        ReceivedAllMsgEvent(allMsg);//把串口数据放到公共数据池中
                    }
                }
                return;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("脑电数据接收出错！");
                return;
            }
        }

        /// <summary>
        /// 清空串口中的数据
        /// </summary>
        public void ClearPortData()
        {
            portNeuro.ReadExisting();
        }
    }

}
