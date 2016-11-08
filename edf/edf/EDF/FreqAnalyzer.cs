using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroencephalographController
{
    /// <summary>
    /// 频率分析器
    /// </summary>
    public class FreqAnalyzer
    {
        public ArrayList filterDataArrayList;
        public int channelNum = 25;
        private int filterQueueDataNum = 1000;//滤波数据点数

        public FreqAnalyzer()
        {
            filterDataArrayList = new ArrayList();
            //定义25个队列，对应25个通道，每个队列存放1024个数据，用于滤波分析，每次采集n个数据时，放于队列最后n个位置，队列前n个位置数据删除
            for (int i = 0; i < channelNum; i++)
            {
                Queue dataQueue = Queue.Synchronized(new Queue());
                filterDataArrayList.Add(dataQueue);
            }
        }
        /// <summary>
        /// 求复数complex数组的模modul数组
        /// </summary>
        /// <param name="input">复数数组</param>
        /// <returns>模数组</returns>
        public double[] Cmp2Mdl(Complex[] input)
        {
            ///有输入数组的长度确定输出数组的长度
            double[] output = new double[input.Length];

            ///对所有输入复数求模
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].Real > 0)
                {
                    output[i] = -input[i].ToModul();
                }
                else
                {
                    output[i] = input[i].ToModul();
                }
            }

            ///返回模数组
            return output;
        }

        /// <summary>
        /// 傅立叶变换或反变换，递归实现多级蝶形运算
        /// 作为反变换输出需要再除以序列的长度
        /// ！注意：输入此类的序列长度必须是2^n
        /// </summary>
        /// <param name="input">输入实序列</param>
        /// <param name="invert">false=正变换，true=反变换</param>
        /// <returns>傅立叶变换或反变换后的序列</returns>
        public Complex[] FFT(double[] input, bool invert)
        {
            ///由输入序列确定输出序列的长度
            Complex[] output = new Complex[input.Length];

            ///将输入的实数转为复数
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = new Complex(input[i]);
            }

            ///返回FFT或IFFT后的序列
            return output = FFT(output, invert);
        }

        /// <summary>
        /// 傅立叶变换或反变换，递归实现多级蝶形运算
        /// 作为反变换输出需要再除以序列的长度
        /// ！注意：输入此类的序列长度必须是2^n
        /// </summary>
        /// <param name="input">复数输入序列</param>
        /// <param name="invert">false=正变换，true=反变换</param>
        /// <returns>傅立叶变换或反变换后的序列</returns>
        private Complex[] FFT(Complex[] input, bool invert)
        {
            ///输入序列只有一个元素，输出这个元素并返回
            if (input.Length == 1)
            {
                return new Complex[] { input[0] };
            }

            ///输入序列的长度
            int length = input.Length;

            ///输入序列的长度的一半
            int half = length / 2;

            ///有输入序列的长度确定输出序列的长度
            Complex[] output = new Complex[length];

            ///正变换旋转因子的基数
            double fac = -2.0 * Math.PI / length;

            ///反变换旋转因子的基数是正变换的相反数
            if (invert)
            {
                fac = -fac;
            }

            ///序列中下标为偶数的点
            Complex[] evens = new Complex[half];

            for (int i = 0; i < half; i++)
            {
                evens[i] = input[2 * i];
            }

            ///求偶数点FFT或IFFT的结果，递归实现多级蝶形运算
            Complex[] evenResult = FFT(evens, invert);

            ///序列中下标为奇数的点
            Complex[] odds = new Complex[half];

            for (int i = 0; i < half; i++)
            {
                odds[i] = input[2 * i + 1];
            }

            ///求偶数点FFT或IFFT的结果，递归实现多级蝶形运算
            Complex[] oddResult = FFT(odds, invert);

            for (int k = 0; k < half; k++)
            {
                ///旋转因子
                double fack = fac * k;

                ///进行蝶形运算
                Complex oddPart = oddResult[k] * new Complex(Math.Cos(fack), Math.Sin(fack));
                output[k] = evenResult[k] + oddPart;
                output[k + half] = evenResult[k] - oddPart;
            }

            ///返回FFT或IFFT的结果
            return output;
        }

        /// <summary>
        /// 频域滤波器
        /// </summary>
        /// <param name="data">待滤波的数据</param>
        /// <param name="Nc">滤波器截止波数</param>
        /// <param name="Hd">滤波器的权函数</param>
        /// <returns>滤波后的数据</returns>
        private double[] FreqFilter(double[] data, int Nc, Complex[] Hd)
        {
            ///最高波数==数据长度
            int N = data.Length;

            ///输入数据进行FFT
            Complex[] fData = FFT(data, false);

            ///频域滤波
            for (int i = 0; i < N; i++)
            {
                fData[i] = Hd[i] * fData[i];
            }

            ///滤波后数据计算IFFT
            ///！未除以数据长度
            fData = FFT(fData, true);

            ///复数转成模
            data = Cmp2Mdl(fData);

            ///除以数据长度
            for (int i = 0; i < N; i++)
            {
                data[i] = -data[i] / N;
            }

            ///返回滤波后的数据
            return data;
        }

        /// <summary>
        /// 对某一信道进行带通滤波
        /// </summary>
        /// <param name="channelIndex">通道号</param>
        /// <param name="data">采集数据</param>
        /// <param name="is50HzFiltered">是否进行50Hz滤波</param>
        /// <param name="is60HzFiltered">是否进行60Hz滤波</param>
        /// <param name="isBandpassFiltered">是否进行带通滤波</param>
        /// <param name="low">滤波下限频率</param>
        /// <param name="high">滤波上限频率</param>
        /// <param name="rateOfSample">采样频率</param>
        /// <returns></returns>
        public double[] BandpassFilter(int channelIndex, double[] data, bool is50HzFiltered, bool is60HzFiltered ,bool isBandpassFiltered ,int low, int high, int rateOfSample)
        {
            //如果不进行50Hz滤波,60Hz滤波和带通滤波，则直接返回原始数据
            if (is50HzFiltered == false && is60HzFiltered == false && isBandpassFiltered == false)
                return data;
            Queue dataQueue = Queue.Synchronized(new Queue());
            dataQueue = (Queue)filterDataArrayList[channelIndex];
            //存储采集数据
            for (int i = 0; i < data.Length; i++)
            {
                //数据个数不满1000时，直接存储数据，超过1000时，保持队列数据1000不变
                if (dataQueue.Count < 1000)
                {
                    dataQueue.Enqueue(data[i]);
                }
                else
                {
                    dataQueue.Dequeue();
                    dataQueue.Enqueue(data[i]);
                }
            }
            //获取队列数据个数
            int queueCount = dataQueue.Count;
            double[] filterSrcData = new double[queueCount];
            //读取待滤波序列，不改变队列数据存储格式
            for (int i = 0; i < queueCount; i++)
            {
                filterSrcData[i] = (double)dataQueue.Dequeue();
                dataQueue.Enqueue(filterSrcData[i]);
            }

            ///最高波数==数据长度
            int N = filterSrcData.Length;

            ///补全位数,2的n次方
            int NFFT = (int)Math.Pow(2.0, Math.Ceiling(Math.Log10((double)N) / Math.Log10(2.0)));
            double[] fftdata = new double[NFFT];
            Array.Copy(filterSrcData, fftdata, N);

            for (int i = N; i < NFFT; i++)
            {
                fftdata[i] = 0.0;
            }
            ///输入数据进行FFT
            Complex[] fData = FFT(fftdata, false);

            ///频域滤波
            //计算出每个点的频率
            double[] frequency = new double[NFFT];
            for (int i = 0; i < NFFT; i++)
            {
                frequency[i] = Convert.ToDouble(i * rateOfSample * 10 / NFFT) / 10;
            }
            
            //带通滤波
            for (int i = 0; i < NFFT; i++)
            {
                //进行50Hz频率滤波
                if(is50HzFiltered==true)
                {
                    if (frequency[i]>=49 && frequency[i] <= 51)
                        fData[i] = new Complex();
                }
                //进行60Hz频率滤波
                if (is60HzFiltered == true)
                {
                    if (frequency[i] >= 59 && frequency[i] <= 61)
                        fData[i] = new Complex();
                }
                //进行指定频带带通滤波
                if (isBandpassFiltered == true)
                {
                    if (frequency[i] < low || frequency[i] > high)
                    {
                        fData[i] = new Complex();
                    }
                }
            }

            ///滤波后数据计算IFFT
            ///！未除以数据长度
            fData = FFT(fData, true);

            ///复数转成模
            filterSrcData = Cmp2Mdl(fData);

            ///除以数据长度
            for (int i = 0; i < N; i++)
            {
                filterSrcData[i] = -filterSrcData[i] / N;
            }

            double[] filterData = new double[data.Length];

            int startIndex = N - data.Length;
            //加入待滤波数据长度超过滤波数据长度，只获取滤波数据长度的结果，目前不会出现这种情况
            if (startIndex < 0)
                Array.Copy(filterSrcData, 0, filterData, 0, data.Length);
            else
                Array.Copy(filterSrcData, startIndex, filterData, 0, data.Length);
            //while(true)
            //{
            //    Array.Copy(filterSrcData,N-data.Length, filterData,0, N);
            //}
            //返回滤波后的数据
            return filterData;
        }
        
        /// <summary>
        /// 对某一信道进行带通滤波
        /// </summary>
        /// <param name="channelIndex">通道号</param>
        /// <param name="data">采集数据</param>
        /// <param name="is50HzFiltered">是否进行50Hz滤波</param>
        /// <param name="isBandpassFiltered">是否进行带通滤波</param>
        /// <param name="low">滤波下限频率</param>
        /// <param name="high">滤波上限频率</param>
        /// <param name="rateOfSample">采样频率</param>
        /// <returns></returns>
        public double[] BandpassFilter(int channelIndex, double[] data, bool is50HzFiltered, bool isBandpassFiltered, int low, int high, int rateOfSample)
        {
            //如果不进行50Hz滤波,60Hz滤波和带通滤波，则直接返回原始数据
            if (is50HzFiltered == false  && isBandpassFiltered == false)
                return data;
            Queue dataQueue = Queue.Synchronized(new Queue());
            dataQueue = (Queue)filterDataArrayList[channelIndex];
            //存储采集数据
            for (int i = 0; i < data.Length; i++)
            {
                //数据个数不满1000时，直接存储数据，超过1000时，保持队列数据1000不变
                if (dataQueue.Count < this.filterQueueDataNum)
                {
                    dataQueue.Enqueue(data[i]);
                }
                else
                {
                    dataQueue.Dequeue();
                    dataQueue.Enqueue(data[i]);
                }
            }
            //获取队列数据个数
            int queueCount = dataQueue.Count;
            double[] filterSrcData = new double[queueCount];
            //读取待滤波序列，不改变队列数据存储格式
            for (int i = 0; i < queueCount; i++)
            {
                filterSrcData[i] = (double)dataQueue.Dequeue();
                dataQueue.Enqueue(filterSrcData[i]);
            }

            ///最高波数==数据长度
            int N = filterSrcData.Length;

            ///补全位数,2的n次方
            int NFFT = (int)Math.Pow(2.0, Math.Ceiling(Math.Log10((double)N) / Math.Log10(2.0)));
            double[] fftdata = new double[NFFT];
            Array.Copy(filterSrcData, fftdata, N);

            for (int i = N; i < NFFT; i++)
            {
                fftdata[i] = 0.0;
            }
            ///输入数据进行FFT
            Complex[] fData = FFT(fftdata, false);

            ///频域滤波
            //计算出每个点的频率
            double[] frequency = new double[NFFT];
            for (int i = 0; i < NFFT; i++)
            {
                frequency[i] = Convert.ToDouble(i * rateOfSample*10 / NFFT)/10;//0.1Hz间隔
            }

            //带通滤波
            for (int i = 0; i < NFFT; i++)
            {
                //进行50Hz频率滤波
                if (is50HzFiltered == true)
                {
                    if (frequency[i] >= 49 && frequency[i] <= 51)
                        fData[i] = new Complex();
                }
                
                //进行指定频带带通滤波
                if (isBandpassFiltered == true)
                {
                    if (frequency[i] < low || frequency[i] > high)
                    {
                        fData[i] = new Complex();
                    }
                }
            }

            ///滤波后数据计算IFFT
            ///！未除以数据长度
            fData = FFT(fData, true);

            ///复数转成模
            filterSrcData = Cmp2Mdl(fData);

            ///除以数据长度
            for (int i = 0; i < N; i++)
            {
                filterSrcData[i] = -filterSrcData[i] / N;
            }
            //防止滤波数据长度大于1000
            int filterDataNum = Math.Min(data.Length, this.filterQueueDataNum);
            double[] filterData = new double[filterDataNum];

            int startIndex = N - data.Length;
            //加入待滤波数据长度超过滤波数据长度，只获取滤波数据长度的结果，目前不会出现这种情况
            if (startIndex < 0)
                Array.Copy(filterSrcData, 0, filterData, 0, filterDataNum);
            else
                Array.Copy(filterSrcData, startIndex, filterData, 0, filterDataNum);
            //while(true)
            //{
            //    Array.Copy(filterSrcData,N-data.Length, filterData,0, N);
            //}
            //返回滤波后的数据
            return filterData;
        }

        /// <summary>
        /// 对某一通道信号进行带通滤波
        /// </summary>
        /// <param name="channelIndex">通道号</param>
        /// <param name="data">采集数据</param>
        /// <param name="low">滤波下限频率</param>
        /// <param name="high">滤波上限频率</param>
        /// <param name="rateOfSample">采样频率</param>
        /// <returns></returns>
        public double[] BandpassFilter(int channelIndex,double[] data, int low, int high, int rateOfSample)
        {
            Queue dataQueue = Queue.Synchronized(new Queue());
            dataQueue=(Queue)filterDataArrayList[channelIndex];
            //存储采集数据
            for(int i=0;i<data.Length;i++)
            {
                //数据个数不满1000时，直接存储数据，超过1000时，保持队列数据1000不变
                if (dataQueue.Count < 1000)
                {
                    dataQueue.Enqueue(data[i]);
                }
                else
                {
                    dataQueue.Dequeue();
                    dataQueue.Enqueue(data[i]);
                }
            }
            //获取队列数据个数
            int queueCount = dataQueue.Count;
            double[] filterSrcData = new double[queueCount];
            //读取待滤波序列，不改变队列数据存储格式
            for (int i = 0; i < queueCount; i++)
            {
                filterSrcData[i] = (double)dataQueue.Dequeue();
                dataQueue.Enqueue(filterSrcData[i]);
            }

            ///最高波数==数据长度
            int N = filterSrcData.Length;

            ///补全位数,2的n次方
            int NFFT = (int)Math.Pow(2.0, Math.Ceiling(Math.Log10((double)N) / Math.Log10(2.0)));
            double[] fftdata = new double[NFFT];
            Array.Copy(filterSrcData, fftdata, N);

            for (int i = N; i < NFFT; i++)
            {
                fftdata[i] = 0.0;
            }
            ///输入数据进行FFT
            Complex[] fData = FFT(fftdata, false);

            ///频域滤波
            //计算出每个点的频率
            double[] frequency = new double[NFFT];
            for (int i = 0; i < NFFT; i++)
            {
                frequency[i] = i * rateOfSample / NFFT;
            }
            //带通滤波
            for (int i = 0; i < NFFT; i++)
            {
                if (frequency[i] < low || frequency[i] > high)
                {
                    fData[i] = new Complex();
                }
            }

            ///滤波后数据计算IFFT
            ///！未除以数据长度
            fData = FFT(fData, true);

            ///复数转成模
            filterSrcData = Cmp2Mdl(fData);

            ///除以数据长度
            for (int i = 0; i < N; i++)
            {
                filterSrcData[i] = -filterSrcData[i] / N;
            }

            double[] filterData = new double[N];

            int startIndex = N - data.Length;
            //加入待滤波数据长度超过滤波数据长度，只获取滤波数据长度的结果，目前不会出现这种情况
            if(startIndex<0)
                Array.Copy(filterSrcData, 0, filterData, 0, N);
            else
                Array.Copy(filterSrcData, startIndex, filterData, 0, data.Length);
            //while(true)
            //{
            //    Array.Copy(filterSrcData,N-data.Length, filterData,0, N);
            //}
            //返回滤波后的数据
            return filterData;
        }

        public double[] BandpassFilter(double[] data, int low, int high, int rateOfSample)
        {            
            ///最高波数==数据长度
            int N = data.Length;

            ///补全位数,2的n次方
            int NFFT = (int)Math.Pow(2.0, Math.Ceiling(Math.Log10((double)N) / Math.Log10(2.0)));
            double[] fftdata = new double[NFFT];
            Array.Copy(data, fftdata, N);

            for (int i = N; i < NFFT; i++)
            {
                fftdata[i] = 0.0;
            }
            ///输入数据进行FFT
            Complex[] fData = FFT(fftdata, false);

            ///频域滤波
            //计算出每个点的频率
            double[] frequency = new double[NFFT];
            for (int i = 0; i < NFFT; i++)
            {
                frequency[i] = i * rateOfSample / NFFT;
            }
            //带通滤波
            for (int i = 0; i < NFFT; i++)
            {
                if (frequency[i] < low || frequency[i] > high)
                {
                    fData[i] = new Complex();
                }
            }

            ///滤波后数据计算IFFT
            ///！未除以数据长度
            fData = FFT(fData, true);

            ///复数转成模
            data = Cmp2Mdl(fData);

            ///除以数据长度
            for (int i = 0; i < N; i++)
            {
                data[i] = -data[i] / N;
            }

            double[] filterData = new double[N];
            Array.Copy(data, filterData, N);

            ///返回滤波后的数据
            return filterData;

        }
    }
}
