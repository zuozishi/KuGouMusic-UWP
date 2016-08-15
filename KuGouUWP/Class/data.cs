using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace KuGouUWP.Class
{
    public class data
    {
        public static string ToJsonData(object item)
        {
            DataContractJsonSerializer serialize = new DataContractJsonSerializer(item.GetType());
            string result = String.Empty;
            using (MemoryStream ms=new MemoryStream())
            {
                serialize.WriteObject(ms, item);
                ms.Position = 0;
                using (StreamReader reader=new StreamReader(ms))
                {
                    result = reader.ReadToEnd();
                    reader.Dispose();
                }
            }
            return result;
        }
        public static T DataContractJsonDeSerialize<T>(string json)
        {
            try
            {
                var ds = new DataContractJsonSerializer(typeof(T));
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
                T obj = (T)ds.ReadObject(ms);
                ms.Dispose();
                return obj;
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
