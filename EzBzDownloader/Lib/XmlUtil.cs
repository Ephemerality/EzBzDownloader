using System.IO;
using System.Xml.Serialization;

namespace EzBzDownloader.Lib
{
    public static class XmlUtil
    {
        public static T Deserialize<T>(Stream input)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T) serializer.Deserialize(input);
        }
    }
}