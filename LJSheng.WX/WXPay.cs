using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml;

namespace LJSheng.WX
{
    /// <summary>
    /// 微信支付
    /// </summary>
    /// <param name="openid">用户openid</param>
    /// <param name="order_no">系统订单号</param>
    /// <param name="body">商品描述</param>
    /// <param name="beizhu">备注</param>
    /// <param name="payrmb">支付金额</param>
    /// <returns>返回调用结果</returns>
    /// <para name="result">200 是成功其他失败</para>
    /// <para name="data">对象结果</para>
    /// <remarks>
    /// 2018-08-18 林建生
    /// </remarks>
    public class WXPay
    {
        public static string APPID = Common.Help.appid;
        public static string TENPAY = "1";
        public static string PARTNER = Common.Help.mch_id;//商户号
        public static string PARTNER_KEY = Common.Help.api_key;

        //服务器异步通知页面路径(流量卡)
        public static readonly string NOTIFY_URL_Card_Store = Common.Help.Url+ "/wxpay.ashx";

        public static JObject Get_RequestHtml(string openid, string order_no, string body, string beizhu, string payrmb)
        {
            HttpContext Context = HttpContext.Current;
            //设置package订单参数
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();

            string total_fee = (decimal.Parse(payrmb) * 100).ToString("f0");
            string wx_timeStamp = "";
            string wx_nonceStr = getNoncestr();

            dic.Add("appid", APPID);
            dic.Add("mch_id", PARTNER);//财付通帐号商家
            dic.Add("device_info", "WEB");//可为空
            dic.Add("nonce_str", wx_nonceStr);
            dic.Add("trade_type", "JSAPI");
            dic.Add("attach", beizhu);
            dic.Add("openid", openid);
            dic.Add("out_trade_no", order_no);		//商家订单号
            dic.Add("total_fee", total_fee); //商品金额,以分为单位(money * 100).ToString()
            dic.Add("notify_url", NOTIFY_URL_Card_Store.ToLower());//接收通知的URL
            dic.Add("body", body);//商品描述
            dic.Add("spbill_create_ip", Context.Request.UserHostAddress);   //用户的公网ip，不是商户服务器IP

            string get_sign = BuildRequest(dic, PARTNER_KEY);

            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            string _req_data = "<xml>";
            _req_data += "<appid>" + APPID + "</appid>";
            _req_data += "<attach><![CDATA[" + beizhu + "]]></attach>";
            _req_data += "<body><![CDATA[" + body + "]]></body> ";
            _req_data += "<device_info><![CDATA[WEB]]></device_info> ";
            _req_data += "<mch_id><![CDATA[" + PARTNER + "]]></mch_id> ";
            _req_data += "<openid><![CDATA[" + openid + "]]></openid> ";
            _req_data += "<nonce_str><![CDATA[" + wx_nonceStr + "]]></nonce_str> ";
            _req_data += "<notify_url><![CDATA[" + NOTIFY_URL_Card_Store.ToLower() + "]]></notify_url> ";
            _req_data += "<out_trade_no><![CDATA[" + order_no + "]]></out_trade_no> ";
            _req_data += "<spbill_create_ip><![CDATA[" + Context.Request.UserHostAddress + "]]></spbill_create_ip> ";
            _req_data += "<total_fee><![CDATA[" + total_fee + "]]></total_fee> ";
            _req_data += "<trade_type><![CDATA[JSAPI]]></trade_type> ";
            _req_data += "<sign><![CDATA[" + get_sign + "]]></sign> ";
            _req_data += "</xml>";

            //通知支付接口，拿到prepay_id
            ReturnValue retValue = StreamReaderUtils.StreamReader(url, Encoding.UTF8.GetBytes(_req_data), System.Text.Encoding.UTF8, true);

            //设置支付参数
            XmlDocument xmldoc = new XmlDocument();

            xmldoc.LoadXml(retValue.Message);
            XmlNode Event = xmldoc.SelectSingleNode("/xml/prepay_id");

            string return_json = "";

            if (Event != null)
            {
                string _prepay_id = Event.InnerText;

                SortedDictionary<string, string> pay_dic = new SortedDictionary<string, string>();

                wx_timeStamp = WXPay.getTimestamp();
                wx_nonceStr = WXPay.getNoncestr();

                string _package = "prepay_id=" + _prepay_id;

                pay_dic.Add("appId", APPID);
                pay_dic.Add("timeStamp", wx_timeStamp);
                pay_dic.Add("nonceStr", wx_nonceStr);
                pay_dic.Add("package", _package);
                pay_dic.Add("signType", "MD5");

                string get_PaySign = BuildRequest(pay_dic, PARTNER_KEY);

                return_json = JsonUtils.SerializeToJson(new
                {
                    appId = APPID,
                    timeStamp = wx_timeStamp,
                    nonceStr = wx_nonceStr,
                    package = _package,
                    paySign = get_PaySign,
                    signType = "MD5"
                });
            }
            else
            {
                XmlNode return_msg = xmldoc.SelectSingleNode("/xml/return_msg");
                return_json = "{\"appId\":\"payerr\",\"return_msg\":\"下单错误代码:" + return_msg.InnerText + "\"}";
            }
            //Common.LogManager.WriteLog("微信支付package日志", return_json + "\r\n retValue =" + retValue.Message);
            return JsonConvert.DeserializeObject(return_json) as JObject;
        }

        public static string getNoncestr()
        {
            Random random = new Random();
            return GetMD5(random.Next(1000).ToString(), "GBK");
        }

        public static string getTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        public static string BuildRequest(SortedDictionary<string, string> sParaTemp, string key)
        {
            //获取过滤后的数组
            Dictionary<string, string> dicPara = new Dictionary<string, string>();
            dicPara = FilterPara(sParaTemp);

            //组合参数数组
            string prestr = CreateLinkString(dicPara);
            //拼接支付密钥
            string stringSignTemp = prestr + "&key=" + key;

            //获得加密结果
            string myMd5Str = GetMD5(stringSignTemp);

            //返回转换为大写的加密串
            return myMd5Str.ToUpper();
        }

        /// <summary>
        /// 除去数组中的空值和签名参数并以字母a到z的顺序排序
        /// </summary>
        /// <param name="dicArrayPre">过滤前的参数组</param>
        /// <returns>过滤后的参数组</returns>
        public static Dictionary<string, string> FilterPara(SortedDictionary<string, string> dicArrayPre)
        {
            Dictionary<string, string> dicArray = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> temp in dicArrayPre)
            {
                if (temp.Key != "sign" && !string.IsNullOrEmpty(temp.Value))
                {
                    dicArray.Add(temp.Key, temp.Value);
                }
            }

            return dicArray;
        }

        //组合参数数组
        public static string CreateLinkString(Dictionary<string, string> dicArray)
        {
            StringBuilder prestr = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in dicArray)
            {
                prestr.Append(temp.Key + "=" + temp.Value + "&");
            }

            int nLen = prestr.Length;
            prestr.Remove(nLen - 1, 1);

            return prestr.ToString();
        }

        //加密
        public static string GetMD5(string pwd)
        {
            MD5 md5Hasher = MD5.Create();

            byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(pwd));

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        /** 获取大写的MD5签名结果 */
        public static string GetMD5(string encypStr, string charset)
        {
            string retStr;
            MD5CryptoServiceProvider m5 = new MD5CryptoServiceProvider();

            //创建md5对象
            byte[] inputBye;
            byte[] outputBye;

            //使用GB2312编码方式把字符串转化为字节数组．
            try
            {
                inputBye = Encoding.GetEncoding(charset).GetBytes(encypStr);
            }
            catch (Exception ex)
            {
                inputBye = Encoding.GetEncoding("GB2312").GetBytes(encypStr);
            }
            outputBye = m5.ComputeHash(inputBye);

            retStr = System.BitConverter.ToString(outputBye);
            retStr = retStr.Replace("-", "").ToUpper();
            return retStr;
        }

        //=======【证书路径设置】===================================== 
        /* 证书路径,注意应该填写绝对路径（仅退款、撤销订单时需要）
        */
        public const string SSLCERT_PATH = "uploadfiles\\cert\\apiclient_cert.p12";
        public static string SSLCERT_PASSWORD = PARTNER;
        public const string URL = "https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers";

        /// <summary>
        /// 企业付款给个人
        /// </summary>       
        /// <returns></returns>
        public static string[] EnterprisePay(decimal rmb, string partner_trade_no, string openid, string real_name, string request_xml, string remarks)
        {
            string RequestXML = request_xml;
            if (remarks != "SYSTEMERROR")
            {
                string wx_nonceStr = getNoncestr();
                string amount = (rmb * 100).ToString("f0");
                string ip = IP();
                //设置签名
                SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                dic.Add("mch_appid", APPID);
                dic.Add("mchid", PARTNER);
                dic.Add("nonce_str", wx_nonceStr);
                dic.Add("partner_trade_no", partner_trade_no);
                dic.Add("openid", openid);
                //dic.Add("check_name", "NO_CHECK");
                dic.Add("check_name", "FORCE_CHECK");
                dic.Add("re_user_name", real_name);
                dic.Add("amount", amount);
                dic.Add("desc", "最惠联盟");
                dic.Add("spbill_create_ip", ip);
                string get_sign = LJSheng.WX.WXPay.BuildRequest(dic, PARTNER_KEY);
                //请求参数
                StringBuilder XML = new StringBuilder();
                XML.Append("<xml>");
                XML.Append("<mch_appid>" + APPID + "</mch_appid>");
                XML.Append("<mchid>" + PARTNER + "</mchid>");
                XML.Append("<nonce_str>" + wx_nonceStr + "</nonce_str>");
                XML.Append("<partner_trade_no>" + partner_trade_no + "</partner_trade_no>");
                XML.Append("<openid>" + openid + "</openid>");
                //XML.Append("<check_name>NO_CHECK</check_name>");
                XML.Append("<check_name>FORCE_CHECK</check_name>");
                XML.Append("<re_user_name>" + real_name + "</re_user_name>");
                XML.Append("<amount>" + amount + "</amount>");
                XML.Append("<desc>最惠联盟</desc>");
                XML.Append("<spbill_create_ip>" + ip + "</spbill_create_ip>");
                XML.Append("<sign>" + get_sign + "</sign>");
                XML.Append("</xml>");
                RequestXML = XML.ToString();
            }
            var result = HttpPost(URL, RequestXML, true, 300);
            string[] str = { result, RequestXML };
            return str;
        }

        public static string HttpPost(string postUrl, string paramData, Encoding dataEncode)
        {
            string ret = string.Empty;
            try
            {
                byte[] byteArray = dataEncode.GetBytes(paramData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (Exception ex)
            {
            }
            return ret;
        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //直接确认，否则打不开    
            return true;
        }

        /// <summary>
        /// post提交支付
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="url"></param>
        /// <param name="isUseCert">是否使用证书</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static string HttpPost(string url, string xml, bool isUseCert, int timeout)
        {
            System.GC.Collect();//垃圾回收，回收没有正常关闭的http连接

            string result = "";//返回结果

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream reqStream = null;

            try
            {
                //设置最大连接数
                ServicePointManager.DefaultConnectionLimit = 200;
                //设置https验证方式
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                /***************************************************************
                * 下面设置HttpWebRequest的相关属性
                * ************************************************************/
                request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "POST";
                request.Timeout = timeout * 1000;

                //设置代理服务器
                //WebProxy proxy = new WebProxy();                          //定义一个网关对象
                //proxy.Address = new Uri(PROXY_URL);              //网关服务器端口:端口
                //request.Proxy = proxy;

                //设置POST的数据类型和长度
                request.ContentType = "text/xml";
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                request.ContentLength = data.Length;

                //是否使用证书
                if (isUseCert)
                {
                    string path = HttpContext.Current.Request.PhysicalApplicationPath;
                    //X509Certificate2 cert = new X509Certificate2(path + SSLCERT_PATH, SSLCERT_PASSWORD);

                    //将上面的改成
                    X509Certificate2 cert = new X509Certificate2(path + SSLCERT_PATH, SSLCERT_PASSWORD, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);//线上发布需要添加
                    request.ClientCertificates.Add(cert);
                }

                //往服务器写入数据
                reqStream = request.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();

                //获取服务端返回
                response = (HttpWebResponse)request.GetResponse();

                //获取服务端返回数据
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd().Trim();
                sr.Close();
            }
            catch (System.Threading.ThreadAbortException e)
            {
                System.Threading.Thread.ResetAbort();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                }
                throw new Exception(e.ToString());
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                //关闭连接和流
                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
            }
            return result;
        }

        /// <summary>
        /// 处理http GET请求，返回数据
        /// </summary>
        /// <param name="url">请求的url地址</param>
        /// <returns>http GET成功后返回的数据，失败抛WebException异常</returns>
        public static string Get(string url)
        {
            System.GC.Collect();
            string result = "";

            HttpWebRequest request = null;
            HttpWebResponse response = null;

            //请求url以获取数据
            try
            {
                //设置最大连接数
                ServicePointManager.DefaultConnectionLimit = 200;
                //设置https验证方式
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                            new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                /***************************************************************
                * 下面设置HttpWebRequest的相关属性
                * ************************************************************/
                request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "GET";

                //设置代理
                //WebProxy proxy = new WebProxy();
                //proxy.Address = new Uri(PROXY_URL);
                //request.Proxy = proxy;

                //获取服务器返回
                response = (HttpWebResponse)request.GetResponse();

                //获取HTTP返回数据
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd().Trim();
                sr.Close();
            }
            catch (System.Threading.ThreadAbortException e)
            {
                System.Threading.Thread.ResetAbort();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                }
                throw new Exception(e.ToString());
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                //关闭连接和流
                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
            }
            return result;
        }

        #region 获取IP
        /// <summary>
        /// 获取IP
        /// </summary>
        public static string IP()
        {
            string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (String.IsNullOrEmpty(ip))
            {
                ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip;
        }
        #endregion
    }
}