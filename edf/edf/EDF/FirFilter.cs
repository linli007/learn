using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroencephalographController
{
    class FirFilter
    {
        private ArrayList filterDataArrayList;
        private int channelNum = 25;
        private int filterQueueDataNum = 501;//滤波数据点数(阶次)

        private const double PI = 3.1415926;
        private double[] h;
        //以下四个参数存储起来是为了与上次滤波配置参数，若参数变化，需重新计算系统函数h的值
        private bool is50HzFiltered=false;
        private bool isBandpassFiltered=false;
        private double low=0;
        private double high=10000;

        public FirFilter()
        {            
            filterDataArrayList = new ArrayList();
            //定义25个队列，对应25个通道，每个队列存放filterQueueDataNum个数据，用于滤波分析，每次采集n个数据时，放于队列最后n个位置，队列前n个位置数据删除
            for (int i = 0; i < channelNum; i++)
            {
                Queue dataQueue = Queue.Synchronized(new Queue());
                filterDataArrayList.Add(dataQueue);
            }
            h = new double[filterQueueDataNum];
        }
        /// <summary>
        /// 单位脉冲响应
        /// </summary>
        /// <param name="n">阶次</param>
        /// <param name="band"></param>
        /// <param name="fln">低频</param>
        /// <param name="fhn">高频</param>
        /// <param name="wn">窗函数类型</param>
        /// <param name="h"></param>
        private void ImpulseResponse(int n, int band, double fln, double fhn, bool is50HzFiltered, int wn, int rateOfSample, params double[] h)//求频率响应函数
        {
            int n2, mid;
            double delay, s, beta;
            beta = 0.0;
            if (wn == 7)
            {
                //Console.WriteLine("Input beta parameter of Kaiser window (3<beta<10)\n");
                //beta = Int32.Parse(Console.ReadLine());
                beta = 4;
            }
            if (n % 2 == 0)
            {
                n2 = n / 2 - 1;
                mid = 1;
            }
            else
            {
                n2 = n / 2;
                mid = 0;
            }
            delay = n / 2;
            double wc1 = 2 * PI * fln;
            double wc2 = 2 * PI * fhn;
            double bandstop1 = 2 * PI * (50-0.3)/rateOfSample;//阻带频率0.3Hz
            double bandstop2 = 2 * PI * (50+0.3)/rateOfSample;//阻带频率0.3Hz
            double w;
            //陷波程序仅修改了case 3带通滤波情况，其他情况若需修改，请参照case 3
            switch (band)
            {
                case 1:
                    {
                        for (int i = 0; i <= n2; i++)
                        {
                            s = i - delay;
                            Window(wn, n + 1, i, beta, out w);
                            h[i] = (Math.Sin(wc1 * s) / (PI * s)) * w;
                            h[n - 1 - i] = h[i];
                        }
                        if (mid == 0)
                            h[n / 2] = wc1 / PI;
                        break;
                    }
                case 2:
                    {
                        for (int i = 0; i <= n2; i++)
                        {
                            s = i - delay;
                            Window(wn, n + 1, i, beta, out w);                            
                            h[i] = (Math.Sin(PI * s) - Math.Sin(wc1 * s)) / (PI * s);
                            h[i] = h[i] * w;
                            h[n - 1 - i] = h[i];
                        }
                        if (mid == 0)
                            h[n / 2] = 1.0 - wc1 / PI;
                        break;
                    }
                case 3:
                    {
                        for (int i = 0; i <= n2; i++)
                        {
                            s = i - delay;
                            Window(wn, n + 1, i, beta, out w);
                            if (is50HzFiltered && fln < 50 && fhn > 50 && fhn<1000)
                                h[i] = (Math.Sin(wc2 * s) - Math.Sin(bandstop2 * s) + Math.Sin(bandstop1 * s) - Math.Sin(wc1 * s)) / (PI * s);
                            else if (is50HzFiltered && fln == 0 && fhn > 999)
                                h[i] = (Math.Sin(bandstop2 * s) - Math.Sin(bandstop1 * s)) / (PI * s);//仅50Hz陷波
                            else
                                h[i] = (Math.Sin(wc2 * s) - Math.Sin(wc1 * s)) / (PI * s);
                            h[i] = h[i] * w;
                            h[n - 1 - i] = h[i];
                        }
                        if (mid == 0)
                        {
                            if (is50HzFiltered && fln < 50 && fhn > 50 && fhn < 1000)
                                h[n / 2] = (wc2 - bandstop2 + bandstop1 - wc1) / PI;
                            else if (is50HzFiltered && fln == 0 && fhn > 999)
                                h[n / 2] = (bandstop2 - bandstop1) / PI;//仅50Hz陷波
                            else
                                h[n / 2] = (wc2 - wc1) / PI;
                        }
                        break;
                    }
                case 4:
                    {
                        for (int i = 0; i <= n - 1; i++)
                        {
                            s = i - delay;
                            Window(wn, n + 1, i, beta, out w);
                            h[i] = (Math.Sin(wc1 * s) + Math.Sin(PI * s) - Math.Sin(wc2 * s)) / (PI * s);
                            h[i] = h[i] * w;
                            h[n - 1 - i] = h[i];
                        }
                        if (mid == 0)
                            h[n / 2] = (wc1 + PI - wc2) / PI;
                        break;
                    }
            }

        }

        private void Window(int type, int n, int i, double beta, out double w)//窗函数
        {
            int k;
            w = 1.0;
             switch (type)
            {
                case 1:
                    {
                        w = 1.0;
                        break;
                    }
                case 2:
                    {
                        k = (n - 2) / 10;
                        if (i <= k)
                            w = 0.5 * (1.0 - Math.Cos(i * PI / (k + 1)));
                        if (i > n - k - 2)
                            w = 0.5 * (1.0 - Math.Cos((n - i - 1) * PI / (k + 1)));
                        break;
                    }
                case 3:
                    {
                        w = 1.0 - Math.Abs(1.0 - 2 * i / (n - 1.0));
                        break;
                    }
                case 4:
                    {
                        w = 0.5 * (1.0 - Math.Cos(2 * i * PI / (n - 1)));
                        break;
                    }
                case 5:
                    {
                        w = 0.54 - 0.46 * Math.Cos(2 * i * PI / (n - 1));
                        break;
                    }
                case 6:
                    {
                        w = 0.42 - 0.5 * Math.Cos(2 * i * PI / (n - 1)) + 0.08 * Math.Cos(4 * i * PI / (n - 1));
                        break;
                    }
                case 7:
                    {
                        kaiser(i, n, beta, out w);
                        break;
                    }

            }
        }

        private void kaiser(int i, int n, double beta, out double w)//kaiser窗函数
        {
            double a, a2, b1, b2, beta1;
            besse10(beta, out b1);
            a = 2.0 * i / (double)(n - 1) - 1.0;
            a2 = a * a;
            beta1 = beta * Math.Sqrt(1.0 - a2);
            besse10(beta1, out b2);
            w = b2 / b1;
        }
        private void besse10(double x, out double sum)
        {
            int i;
            double d, y, d2;
            y = x / 2.0;
            d = 1.0;
            sum = 1.0;
            for (i = 1; i <= 25; i++)
            {
                d = d * y / i;
                d2 = d * d;
                sum = sum + d2;
                if (d2 < sum * (1.0e-8))
                    break;
            }
        }
        /// <summary>
        /// 卷积计算
        /// </summary>
        /// <param name="x">输入信号</param>
        /// <param name="h">单位脉冲响应</param>
        /// <returns></returns>
        private double[] xcorr(double[] x, double[] h)
        {            
            if(x.Length>h.Length)
            {
                double[] r = new double[x.Length-h.Length];
                double sum_Input = 0;//N阶输入信号和
                double sum_h = 0;//系统函数和
                double average_Input = 0.0;//N阶输入信号均值
                //计算系统函数h各元素之和
                for (int m = 0; m < h.Length; m++)
                    sum_h = sum_h + h[m];
                //滤除直流偏置后求卷积
                for(int i=0;i<x.Length-h.Length;i++)
                {
                    sum_Input = 0;
                    for(int j=0;j<h.Length;j++)
                    {
                        r[i] =r[i]+ x[i+j] * h[j];
                        sum_Input = sum_Input + x[i + j];
                    }
                    average_Input = sum_Input / h.Length;
                    r[i] = r[i] - average_Input * sum_h;
                }
                for (int m = 0; m < h.Length; m++)
                {
                    if (h[m] > 1)
                         sum_h = 0;
                }
                return r;
            }
            return x;
        }  
        private void xcorr(double[] x, double[] y, int N, double[] r)
        {
            double sxy;
            int delay, i, j;

            for (delay = -N + 1; delay < N; delay++)
            {
                //Calculate the numerator
                sxy = 0;
                for (i = 0; i < N; i++)
                {
                    j = i + delay;
                    if ((j < 0) || (j >= 5000)) //The series are no wrapped,so the value is ignored
                        continue;
                    else
                        sxy += (x[i] * y[j]);
                }
                r[delay + N - 1] = sxy;
            }
        }
        /// <summary>
        /// 对某一信道进行带通滤波   --by zt
        /// </summary>
        /// <param name="channelIndex"></param>
        /// <param name="data"></param>
        /// <param name="is50HzFiltered"></param>
        /// <param name="isBandpassFiltered"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <param name="rateOfSample"></param>
        /// <returns></returns>
        public double[] BandpassFilter(int channelIndex, double[] data, bool is50HzFiltered, bool isBandpassFiltered, double low, double high, int rateOfSample)
        {
            //如果不进行50Hz滤波,和带通滤波，则直接返回原始数据
            if (is50HzFiltered == false && isBandpassFiltered == false)
            {
                this.is50HzFiltered = false;
                this.isBandpassFiltered = false;
                return data;
            }
            Queue dataQueue = Queue.Synchronized(new Queue());
            dataQueue = (Queue)filterDataArrayList[channelIndex];
            //存储采集数据
            for (int i = 0; i < data.Length; i++)
            {
                //新来的数据全部入列
                dataQueue.Enqueue(data[i]);                
            }
            //获取队列数据个数
            int queueCount = dataQueue.Count;
            //待滤波数据量小于滤波阶次时，不进行滤波
            if (queueCount < filterQueueDataNum)
                return data;
            double[] filterSrcData = new double[queueCount];
            //读取待滤波序列，不改变队列数据存储格式
            for (int i = 0; i < queueCount; i++)
            {
                filterSrcData[i] = (double)dataQueue.Dequeue();
                //保持滤波初始队列数据量为滤波阶次
                if (i >= queueCount - filterQueueDataNum)
                     dataQueue.Enqueue(filterSrcData[i]);
            }

            int n, band, wn;
            double fl, fh, fs;
            band = 3;//bandpass
            n = filterQueueDataNum;//滤波阶次
            fs = rateOfSample;
            wn = 5;//hamming

            fl =  low / fs;
            fh =  high / fs;
            //若滤波参数修改，则更新系统函数h
            if (this.is50HzFiltered != is50HzFiltered || this.isBandpassFiltered != isBandpassFiltered || this.low != low || this.high != high)
            {
                if (isBandpassFiltered == true)
                    ImpulseResponse(n, band, fl, fh, is50HzFiltered, wn,rateOfSample, this.h);//若有带通滤波
                else
                    ImpulseResponse(n, band, 0, 10000, is50HzFiltered, wn, rateOfSample, this.h);//若无带通滤波，则此处只进行50Hz陷波
                this.is50HzFiltered = is50HzFiltered;
                this.isBandpassFiltered = isBandpassFiltered;
                this.low = low;
                this.high = high;
            }
            double[] filterData = new double[filterSrcData.Length];
            filterData = xcorr(filterSrcData, this.h);
            if(is50HzFiltered==true && isBandpassFiltered==false)
            {
                for(int k=0;k<filterData.Length;k++)
                {
                    filterData[k] = data[k] - filterData[k];
                }
            }
            ///返回滤波后的数据
            return filterData;
        }
    }
}
