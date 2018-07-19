using LJSheng.Data;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using LJSheng.Common;
using Newtonsoft.Json;

namespace LJSheng.WX
{
    /// <summary>
    /// 微信支付类
    /// </summary>
    /// <remarks>
    /// 2018-08-18 林建生
    /// </remarks>
    public class WXPay
    {
        public static string APPID = Common.Help.appid;
        public static string TENPAY = "1";
        public static string PARTNER = Common.Help.mch_id;
        public static string PARTNER_KEY = Common.Help.api_key;

        //服务器异步通知页面路径(流量卡)
        public static readonly string NOTIFY_URL_Card_Store = System.Web.Configuration.WebConfigurationManager.AppSettings["url"].ToString() + "/wxpay.ashx";

        /// <summary>
        /// 微信支付
        /// </summary>
        /// <param name="openid">用户openid</param>
        /// <param name="OrderNo">系统订单号</param>
        /// <param name="payrmb">支付金额</param>
        /// <param name="body">商品描述</param>
        /// <param name="remarks">备注</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string Get_RequestHtml(string openid, string OrderNo, decimal payrmb, string body, string attach)
        {
            HttpContext Context = HttpContext.Current;
            //设置package订单参数
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            string total_fee = (decimal.Parse(payrmb.ToString()) * 100).ToString("f0");
            //string wx_timeStamp = "";
            string wx_nonceStr = getNoncestr();

            dic.Add("appid", APPID);
            dic.Add("mch_id", PARTNER);//财付通帐号商家
            dic.Add("device_info", "WEB");//可为空
            dic.Add("nonce_str", wx_nonceStr);
            dic.Add("trade_type", "JSAPI");
            dic.Add("attach", attach);
            dic.Add("openid", openid);
            dic.Add("out_trade_no", OrderNo);		//商家订单号
            dic.Add("total_fee", total_fee); //商品金额,以分为单位(money * 100).ToString()
            dic.Add("notify_url", NOTIFY_URL_Card_Store.ToLower());//接收通知的URL
            dic.Add("body", body);//商品描述
            dic.Add("spbill_create_ip", Context.Request.UserHostAddress);   //用户的公网ip，不是商户服务器IP
            string get_sign = BuildRequest(dic, PARTNER_KEY);
            //设置查询订餐参数的签名
            SortedDictionary<string, string> paydic = new SortedDictionary<string, string>();
            paydic.Add("appid", APPID);
            paydic.Add("mch_id", PARTNER);
            paydic.Add("nonce_str", wx_nonceStr);
            paydic.Add("out_trade_no", OrderNo);
            string pay_sign = BuildRequest(paydic, PARTNER_KEY);

            string url = "https://api.mch.weixin.qq.com/Pay/unifiedorder";
            string _req_data = "<xml>";
            _req_data += "<appid>" + APPID + "</appid>";
            _req_data += "<attach><![CDATA[" + attach + "]]></attach>";
            _req_data += "<body><![CDATA[" + body + "]]></body> ";
            _req_data += "<device_info><![CDATA[WEB]]></device_info> ";
            _req_data += "<mch_id><![CDATA[" + PARTNER + "]]></mch_id> ";
            _req_data += "<openid><![CDATA[" + openid + "]]></openid> ";
            _req_data += "<nonce_str><![CDATA[" + wx_nonceStr + "]]></nonce_str> ";
            _req_data += "<notify_url><![CDATA[" + NOTIFY_URL_Card_Store.ToLower() + "]]></notify_url> ";
            _req_data += "<out_trade_no><![CDATA[" + OrderNo + "]]></out_trade_no> ";
            _req_data += "<spbill_create_ip><![CDATA[" + Context.Request.UserHostAddress + "]]></spbill_create_ip> ";
            _req_data += "<total_fee><![CDATA[" + total_fee + "]]></total_fee> ";
            _req_data += "<trade_type><![CDATA[JSAPI]]></trade_type> ";
            _req_data += "<sign><![CDATA[" + get_sign + "]]></sign> ";
            _req_data += "</xml>";
            //订单查询参数
            string Pay = "<xml>";
            Pay += "<appid>" + APPID + "</appid>";
            Pay += "<mch_id><![CDATA[" + PARTNER + "]]></mch_id> ";
            Pay += "<nonce_str><![CDATA[" + wx_nonceStr + "]]></nonce_str> ";
            Pay += "<out_trade_no><![CDATA[" + OrderNo + "]]></out_trade_no> ";
            Pay += "<sign><![CDATA[" + pay_sign + "]]></sign> ";
            Pay += "</xml>";
            using (EFDB db = new EFDB())
            {
                var b = db.Order.Where(l => l.OrderNo == OrderNo).FirstOrDefault();
                if (b != null)
                {
                    b.Pay = Pay;
                    if (db.SaveChanges() != 1)
                    {
                        LogManager.WriteLog("支付参数失败", "订单号=" + OrderNo + "----------参数=" + _req_data);
                    }
                }
                else
                {
                    {
                        LogManager.WriteLog("找不到订单", "订单号=" + OrderNo + "----------参数=" + _req_data);
                    }
                }
            }
            //通知支付接口，拿到prepay_id
            ReturnValue retValue = StreamReaderUtils.StreamReader(url, Encoding.UTF8.GetBytes(_req_data), System.Text.Encoding.UTF8, true);

            //设置支付参数
            XmlDocument xmldoc = new XmlDocument();

            xmldoc.LoadXml(retValue.Message);
            XmlNode Event = xmldoc.SelectSingleNode("/xml/prepay_id");

            string return_json = "";

            if (Event != null)
            {
                return_json = "prepay_id=" + Event.InnerText;
                //小程序不需要以下代码
                //string _prepay_id = Event.InnerText;
                //SortedDictionary<string, string> pay_dic = new SortedDictionary<string, string>();

                //wx_timeStamp = WXPay.getTimestamp();
                //wx_nonceStr = WXPay.getNoncestr();

                //string _package = "prepay_id=" + _prepay_id;

                //pay_dic.Add("appId", APPID);
                //pay_dic.Add("timeStamp", wx_timeStamp);
                //pay_dic.Add("nonceStr", wx_nonceStr);
                //pay_dic.Add("package", _package);
                //pay_dic.Add("signType", "MD5");

                //string get_PaySign = BuildRequest(pay_dic, PARTNER_KEY);

                //return_json = JsonUtils.SerializeToJson(new
                //{
                //    appId = APPID,
                //    timeStamp = wx_timeStamp,
                //    nonceStr = wx_nonceStr,
                //    package = _package,
                //    paySign = get_PaySign,
                //    signType = "MD5"
                //});
            }
            else
            {
                XmlNode return_msg = xmldoc.SelectSingleNode("/xml/return_msg");
                return_json = "下单错误代码: " + return_msg.InnerText;
            }
            //Common.LogManager.WriteLog("微信支付package日志", return_json + "\r\n retValue =" + retValue.Message);
            return return_json;
        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="order">订单号</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string Get_Order(string order)
        {
            using (EFDB db = new EFDB())
            {
                string return_json = null;
                var b = db.Order.Where(l => l.OrderNo == order && l.PayStatus == 2).FirstOrDefault();
                if (b != null)
                {
                    ReturnValue retValue = StreamReaderUtils.StreamReader("https://api.mch.weixin.qq.com/Pay/orderquery", Encoding.UTF8.GetBytes(b.Pay), System.Text.Encoding.UTF8, true);
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(retValue.Message);
                    string return_code = xmldoc.SelectSingleNode("/xml/return_code").InnerText;
                    string result_code = xmldoc.SelectSingleNode("/xml/result_code").InnerText;
                    if (return_code == "SUCCESS" && result_code == "SUCCESS")//验证成功
                    {
                        //判断支付状态
                        string trade_state = xmldoc.SelectSingleNode("/xml/trade_state").InnerText;
                        if (trade_state == "SUCCESS")
                        {
                            //微信支付订单号
                            string trade_no = xmldoc.SelectSingleNode("/xml/transaction_id").InnerText;
                            //金额总金额,以分为单位
                            string total_fee = xmldoc.SelectSingleNode("/xml/total_fee").InnerText;
                            decimal pay_amount = decimal.Parse(total_fee) / 100;
                            //备注
                            string attach = xmldoc.SelectSingleNode("/xml/attach").InnerText;
                            return_json = JsonConvert.SerializeObject(new { trade_no, pay_type = 2, pay_amount, attach });
                        }
                        else
                        {
                            LogManager.WriteLog("微信支付状态错误", "trade_state=" + trade_state);
                        }
                    }
                    else
                    {
                        LogManager.WriteLog("微信对账失败", return_code + " - " + result_code);
                    }
                }
                return return_json;
            }
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
            System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();

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
            catch
            {
                inputBye = Encoding.GetEncoding("GB2312").GetBytes(encypStr);
            }
            outputBye = m5.ComputeHash(inputBye);

            retStr = System.BitConverter.ToString(outputBye);
            retStr = retStr.Replace("-", "").ToUpper();
            return retStr;
        }
    }
}