using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace 京东
{
    class Cookie
    {
        public static void Write(string file, CookieContainer cookies)
        {
            using (Stream stream = File.Create(file))
            {
                IFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
                formatter.Serialize(stream, cookies);
#pragma warning restore SYSLIB0011
            }
        }

        public static CookieContainer Read(string file)
        {
            using (Stream stream = File.Open(file, FileMode.Open))
            {
                IFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
                return (CookieContainer)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
            }
        }
    }
}
