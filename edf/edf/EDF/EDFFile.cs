using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using ElectroencephalographController;

namespace EDF
{
    /// <summary>
    /// EDFFile类
    /// </summary>
    public class EDFFile
    {
        public EDFFile()
        {
            //initialize EDFHeader as part of constructor
            _header = new EDFHeader();
            //initialize EDFDataRecord List as part of constructor
            _dataRecords = new List<EDFDataRecord>();

            //解析自定义格式数据时用到  --by zt
            _mydataRecords = new List<MyEDFDataRecord>(); 

            //保存数据时用到  --by zt
            _dataHex = new StringBuilder(); 
        }
        #region 变量
        /// <summary>
        /// 头文件
        /// </summary>
        private EDFHeader _header;

        /// <summary>
        /// 保存数据时用到  --by zt
        /// </summary>
        private StringBuilder _dataHex;
        
        /// <summary>
        /// dataRecord数据
        /// </summary>
        private List<EDFDataRecord> _dataRecords;
        
        /// <summary>
        /// 自定义文件数据
        /// </summary>
        private List<MyEDFDataRecord> _mydataRecords;

        /// <summary>
        /// 下位机采集时放大的倍率，上位机显示数据时要除以此倍率  --by zt
        /// </summary>
        private int multiplyingPower = 12;
        #endregion

        #region 访问器
        public EDFHeader Header
        {
            get
            {
                return _header;
            }
        }

        public List<EDFDataRecord> DataRecords
        {
            get
            {
                return _dataRecords;
            }
        }

        public StringBuilder DataHex
        {
            get
            {
                return _dataHex;
            }
        }

        public List<MyEDFDataRecord> MyDataRecords
        {
            get
            {
                return _mydataRecords;
            }
        }
        #endregion

        public void readFile(string file_path)
        {
            //open the file to read the header
            FileStream file = new FileStream(file_path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);
            readStream(sr);
            file.Close();
            sr.Close();
            file.Dispose();
            sr.Dispose();
        }

        /// <summary>
        /// 只解析头文件  --by zt
        /// </summary>
        /// <param name="file_path"></param>
        public void readFileHeader(string file_path)
        {
            //open the file to read the header
            FileStream file = new FileStream(file_path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);
            parseHeaderStream(sr);
            file.Close();
            sr.Close();
            file.Dispose();
            sr.Dispose();
        }
        
        /// <summary>
        /// 解析edf文件
        /// </summary>
        /// <param name="sr"></param>
        public void readStream(StreamReader sr)
        {
            parseHeaderStream(sr);
            if (this._header.Signals[0].DigitalMaximum.Equals(Convert.ToInt32("7FFFFF", 16))) { parseMyDataRecordStream(sr); }
            else { parseDataRecordStream(sr); }
        }

        public byte[] getEDFFileBytes()
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] byteArray = encoding.GetBytes(this.Header.ToString().ToCharArray());
            List<byte> byteList = new List<byte>(byteArray);
            byteList.AddRange(getCompressedDataRecordsBytes());
            return byteList.ToArray();
        }

        public List<byte> getCompressedDataRecordsBytes()
        {
            List<byte> byteList = new List<byte>();
            byte[] byteArraySample = new byte[2];
            foreach (EDFDataRecord dataRecord in this.DataRecords)
            {
                foreach (EDFSignal signal in this.Header.Signals)
                {
                    foreach (float sample in dataRecord[signal.IndexNumberWithLabel])
                    {
                        byteArraySample = BitConverter.GetBytes(Convert.ToInt16((sample / signal.AmplifierGain) - signal.Offset));
                        byteList.Add(byteArraySample[0]);
                        byteList.Add(byteArraySample[1]);
                    }
                }
            }
            return byteList;
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="file_path"></param>
        public void saveFile(string file_path)
        {
            if (File.Exists(file_path))
            {
                File.Delete(file_path);
            }
            FileStream newFile = new FileStream(file_path, FileMode.CreateNew, FileAccess.Write);

            StreamWriter sw = new StreamWriter(newFile);
            this.Header.NumberOfDataRecords = this.DataRecords.Count;

            char[] headerCharArray = this.Header.ToString().ToCharArray();
            sw.Write(headerCharArray, 0, headerCharArray.Length);
            sw.Flush();

            newFile.Seek((256 + this.Header.NumberOfSignalsInDataRecord * 256), SeekOrigin.Begin);
            BinaryWriter bw = new BinaryWriter(newFile);

            byte[] byteList = getCompressedDataRecordsBytes().ToArray();

            bw.Write(byteList, 0, byteList.Length);
            bw.Flush();
            sw.Close();
            bw.Close();
            newFile.Close();

        }
        /// <summary>
        /// 解析头文件
        /// </summary>
        /// <param name="sr"></param>
        private void parseHeaderStream(StreamReader sr)
        {
            //parse the header to get the number of Signals (size of the Singal Header)
            char[] header = new char[256];
            sr.ReadBlock(header, 0, 256);
            this._header = new EDFHeader(header);

            //parse the signals within the header
            char[] signals = new char[this.Header.NumberOfSignalsInDataRecord * 256];
            sr.ReadBlock(signals, 0, this.Header.NumberOfSignalsInDataRecord * 256);
            this.Header.parseSignals(signals);

        }
        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="sr"></param>
        private void parseDataRecordStream(StreamReader sr)
        {

            //set the seek position in the file stream to the beginning of the data records.
            sr.BaseStream.Seek((256 + this.Header.NumberOfSignalsInDataRecord * 256), SeekOrigin.Begin);

            int dataRecordSize = 0;
            foreach (EDFSignal signal in this.Header.Signals)
            {
                signal.SamplePeriodWithinDataRecord = (float)(this.Header.DurationOfDataRecordInSeconds / signal.NumberOfSamplesPerDataRecord);
                dataRecordSize += signal.NumberOfSamplesPerDataRecord;
            }

            byte[] dataRecordBytes = new byte[dataRecordSize * 2];

            while (sr.BaseStream.Read(dataRecordBytes, 0, dataRecordSize * 2) > 0)
            {

                EDFDataRecord dataRecord = new EDFDataRecord();
                int j = 0;
                int samplesWritten = 0;
                foreach (EDFSignal signal in this.Header.Signals)
                {
                    List<float> samples = new List<float>();
                    for (int l = 0; l < signal.NumberOfSamplesPerDataRecord; l++)
                    {
                        float value = (float)(((BitConverter.ToInt16(dataRecordBytes, (samplesWritten * 2)) + (int)signal.Offset)) * signal.AmplifierGain);
                        samples.Add(value);
                        samplesWritten++;
                    }
                    dataRecord.Add(signal.IndexNumberWithLabel, samples);
                    j++;

                }
                _dataRecords.Add(dataRecord);

            }

        }
        /// <summary>
        /// 解析自定义的文件  --by zt
        /// </summary>
        /// <param name="sr"></param>
        private void parseMyDataRecordStream(StreamReader sr)
        {

            //set the seek position in the file stream to the beginning of the data records.
            sr.BaseStream.Seek((256 + this.Header.NumberOfSignalsInDataRecord * 256), SeekOrigin.Begin);
            
            int dataRecordSize = 0;
            foreach (EDFSignal signal in this.Header.Signals)
            {
                signal.SamplePeriodWithinDataRecord = (float)(this.Header.DurationOfDataRecordInSeconds / signal.NumberOfSamplesPerDataRecord);
                dataRecordSize += signal.NumberOfSamplesPerDataRecord;
            }

           
            byte[] dataRecordBytes = new byte[dataRecordSize * 3];
            byte first00 = HexToByte("00")[0];
            while (sr.BaseStream.Read(dataRecordBytes, 0, dataRecordSize * 3) > 0)
            {

                //EDFDataRecord dataRecord = new EDFDataRecord();
                MyEDFDataRecord dataRecord = new MyEDFDataRecord();
                int j = 0;
                int samplesWritten = 0;
                List<EDFSignal> signals = this.Header.Signals;
                #region 解析与edf格式相似的方式，以后可能会用到  --by zt
                //for (int i = 0; i < signals.Count; i++) 
                //{
                //    List<float> samples = new List<float>();
                //    for (int l = 0; l < signals[i].NumberOfSamplesPerDataRecord; l++)
                //    {
                //        float refVoltage = signals[i].PhysicalMaximum;
                //        //从dataRecordBytes中取3个字节

                //        byte[] temp = new byte[4] { dataRecordBytes[samplesWritten * 3 + 2], dataRecordBytes[samplesWritten * 3 + 1], dataRecordBytes[samplesWritten * 3], first00 };

                //        float value = (float)(BitConverter.ToInt32(temp, 0));
                //        if (value >= 0 && value <= Math.Pow(2, 23) - 1)
                //            value = refVoltage * value / (float)(Math.Pow(2, 23) - 1);
                //        else
                //            value = refVoltage * ((value - (float)Math.Pow(2, 24)) / (float)(Math.Pow(2, 23) - 1));
                //        value = value / multiplyingPower;
                //        samples.Add(value);
                //        samplesWritten++;
                //    }
                //    dataRecord.Add(i.ToString(), samples);
                //    //dataRecord.Add(value);
                //    //dataRecord.Add(signal.IndexNumberWithLabel, value);
                //    j++;
                //}
                #endregion
                foreach (EDFSignal signal in this.Header.Signals)
                {
                    //List<float> samples = new List<float>();
                    //for (int l = 0; l < signal.NumberOfSamplesPerDataRecord; l++)
                    //{
                        float refVoltage = signal.PhysicalMaximum;
                        //从dataRecordBytes中取3个字节

                        byte[] temp = new byte[4] { dataRecordBytes[samplesWritten * 3 + 2], dataRecordBytes[samplesWritten * 3 + 1], dataRecordBytes[samplesWritten * 3], first00 };

                        float value = (float)(BitConverter.ToInt32(temp, 0));
                        if (value >= 0 && value <= Math.Pow(2, 23) - 1)
                            value = refVoltage * value / (float)(Math.Pow(2, 23) - 1);
                        else
                            value = refVoltage * ((value - (float)Math.Pow(2, 24)) / (float)(Math.Pow(2, 23) - 1));
                        value = value / multiplyingPower;
                        //samples.Add(value);
                        samplesWritten++;
                    //}
                    //dataRecord.Add(signal.IndexNumberWithLabel, samples);
                    dataRecord.Add(value);
                    //dataRecord.Add(signal.IndexNumberWithLabel, value);
                    j++;

                }
                _mydataRecords.Add(dataRecord);
                //_dataRecords.Add(dataRecord);
                
            }

        }

        public void deleteSignal(EDFSignal signal_to_delete)
        {
            if (this.Header.Signals.Contains(signal_to_delete))
            {
                //Remove Signal DataRecords
                foreach (EDFDataRecord dr in this.DataRecords)
                {
                    foreach (EDFSignal signal in this.Header.Signals)
                    {
                        if (signal.IndexNumberWithLabel.Equals(signal_to_delete.IndexNumberWithLabel))
                        {
                            dr.Remove(signal_to_delete.IndexNumberWithLabel);
                        }
                    }
                }
                //After removing the DataRecords then Remove the Signal from the Header
                this.Header.Signals.Remove(signal_to_delete);

                //Finally Decrement the NumberOfSignals in the Header by 1
                this.Header.NumberOfSignalsInDataRecord = this.Header.NumberOfSignalsInDataRecord - 1;

                //Change the Number Of Bytes in the Header.
                this.Header.NumberOfBytes = (256) + (256 * this.Header.Signals.Count);
            }
        }
        public void addSignal(EDFSignal signal_to_add, List<float> sampleValues)
        {

            if (this.Header.Signals.Contains(signal_to_add))
            {
                this.deleteSignal(signal_to_add);
            }

            //Remove Signal DataRecords
            int index = 0;
            foreach (EDFDataRecord dr in this.DataRecords)
            {
                dr.Add(signal_to_add.IndexNumberWithLabel, sampleValues.GetRange(index * signal_to_add.NumberOfSamplesPerDataRecord, signal_to_add.NumberOfSamplesPerDataRecord));
                index++;
            }
            //After removing the DataRecords then Remove the Signal from the Header
            this.Header.Signals.Add(signal_to_add);

            //Finally Decrement the NumberOfSignals in the Header by 1
            this.Header.NumberOfSignalsInDataRecord = this.Header.NumberOfSignalsInDataRecord + 1;

            //Change the Number Of Bytes in the Header.
            this.Header.NumberOfBytes = (256) + (256 * this.Header.Signals.Count);

        }
        public List<float> retrieveSignalSampleValues(EDFSignal signal_to_retrieve)
        {
            List<float> signalSampleValues = new List<float>();

            if (this.Header.Signals.Contains(signal_to_retrieve))
            {
                //Remove Signal DataRecords
                foreach (EDFDataRecord dr in this.DataRecords)
                {
                    foreach (EDFSignal signal in this.Header.Signals)
                    {
                        if (signal.IndexNumberWithLabel.Equals(signal_to_retrieve.IndexNumberWithLabel))
                        {
                            foreach (float value in dr[signal.IndexNumberWithLabel])
                            {
                                signalSampleValues.Add(value);
                            }
                        }
                    }
                }
            }
            return signalSampleValues;

        }
        public void exportAsCompumedics(string file_path)
        {
            foreach (EDFSignal signal in this.Header.Signals)
            {
                string signal_name = this.Header.StartDateTime.ToString("MMddyyyy_HHmm") + "_" + signal.Label;
                string new_path = string.Empty;
                if (file_path.LastIndexOf('/') == file_path.Length)
                {
                    new_path = file_path + signal_name.Replace(' ', '_');
                }
                else
                {
                    new_path = file_path + '/' + signal_name.Replace(' ', '_');
                }

                if (File.Exists(new_path))
                {
                    File.Delete(new_path);
                }
                FileStream newFile = new FileStream(new_path, FileMode.CreateNew, FileAccess.Write);

                StreamWriter sw = new StreamWriter(newFile);

                if (signal.NumberOfSamplesPerDataRecord <= 0)
                {
                    //need to pad it to be sampled every second.
                    sw.WriteLine(signal.Label + " " + "RATE:1.0Hz");
                }
                else
                {
                    sw.WriteLine(signal.Label + " " + "RATE:" + Math.Round((double)(signal.NumberOfSamplesPerDataRecord / this.Header.DurationOfDataRecordInSeconds), 2) + "Hz");
                }

                foreach (EDFDataRecord dataRecord in this.DataRecords)
                {
                    foreach (float sample in dataRecord[signal.IndexNumberWithLabel])
                    {
                        sw.WriteLine(sample);
                    }

                }
                sw.Flush();

            }


        }
        /// <summary>
        /// 保存新定义格式的数据—— by zt
        /// </summary>
        /// <param name="file_Path"></param>
        public bool saveNewFile(string file_path)
        {
            lock (this.DataHex)
            {
                DateTime startTime = DateTime.Now;
                
                this.Header.NumberOfDataRecords = this.DataHex.Length / (this.Header.NumberOfSignalsInDataRecord * 6);
                if (this.Header.NumberOfDataRecords < 100)
                    return false;//无脑电数据时不保存结果
                this.Header.DurationOfDataRecordInSeconds = (double)((this.Header.EndTimeEDF.Ticks - this.Header.StartDateTime.Ticks) / Math.Pow(10, 7)) / (this.Header.NumberOfDataRecords);
                if (File.Exists(file_path))
                {
                    File.Delete(file_path);
                }
                FileStream newFile = new FileStream(file_path, FileMode.Append, FileAccess.Write);

                StreamWriter sw = new StreamWriter(newFile);

                char[] headerCharArray = this.Header.ToString().ToCharArray();
                sw.Write(headerCharArray);
                //sw.Write(headerCharArray, 0, headerCharArray.Length);
                sw.Flush();

                newFile.Seek((256 + this.Header.NumberOfSignalsInDataRecord * 256), SeekOrigin.Begin);
                BinaryWriter bw = new BinaryWriter(newFile);

                //byte[] byteList = getCompressedDataRecordsBytes().ToArray();

                try
                {
                    string dataHexString = this.DataHex.ToString();
                    byte[] dataByteArray = HexToByte(dataHexString);
                    bw.Write(dataByteArray, 0, dataByteArray.Length);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(typeof(EDFFile), "写入文件时数据转换出错：" + ex.Message + "数据长度为：" + this.DataHex.ToString().Length);
                    LogHelper.WriteLog(typeof(EDFFile), "此时的数据为" + ":" + this.DataHex.ToString());
                    return false;
                }

                //byte[] dataByteArray = HexToByte(this.DataHex.ToString());

                //bw.Write(byteList, 0, byteList.Length);
            
                bw.Flush();
                sw.Close();
                bw.Close();
                newFile.Close();

                DateTime endTime = DateTime.Now;

                LogHelper.WriteLog(typeof(EDFFile), "保存文件" + file_path);
                LogHelper.WriteLog(typeof(EDFFile), "保存文件耗时" + (endTime - startTime).ToString());

                return true;
            }
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
    }

}
