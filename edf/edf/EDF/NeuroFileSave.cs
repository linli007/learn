using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ElectroencephalographController
{
    interface INeuroFileSave<CollectionType>
    {
        CollectionType GetFromFile(string fileName);
        bool SaveToFile(string fileName, CollectionType collection);
    }


    /// <summary>
    /// 将TTMT消息序列化至文件和反序列化
    /// </summary>
    class NeuroFileSave<CollectionType> : INeuroFileSave<CollectionType>
    {
        FileStream fileStream;
        BinaryFormatter binaryFormatter;

        /// <summary>
        /// 将消息集合序列化至文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public bool SaveToFile(string fileName, CollectionType collection)
        {
            try
            {
                fileStream = new FileStream(fileName, FileMode.Create);
                binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, collection);
                fileStream.Close();
                binaryFormatter = null;
            }
            catch (Exception e)
            {
                LogHelper.WriteLog(typeof(NeuroControl), "保存EDF文件出错:" + e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 将文件反序列化至集合,请使用try/catch
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>集合</returns>
        public CollectionType GetFromFile(string fileName)
        {
            fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            binaryFormatter = new BinaryFormatter();
            CollectionType collection = (CollectionType)binaryFormatter.Deserialize(fileStream);
            fileStream.Close();
            binaryFormatter = null;
            return collection;
        }
    }
}
