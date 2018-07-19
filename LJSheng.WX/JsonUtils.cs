using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace LJSheng.WX
{
    public class JsonUtils
    {
        /// <summary>
        /// 把对象转成json字符串
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeToJson(object o)
        {
            StringBuilder sb = new StringBuilder();

            JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
            json.Serialize(o, sb);

            return sb.ToString();
        }

        /// <summary>
        /// 把json字符串转成对象
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="data">json字符串</param> 
        public static T DeserializeToModel<T>(string data)
        {
            JsonConvert.DeserializeObject<object>(data);
            System.Web.Script.Serialization.JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
            return json.Deserialize<T>(data);
        }
    }
}