using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using System.Text;
using LJSheng.Common;
using LJSheng.Data;
using System.Linq;
using EntityFramework.Extensions;

namespace LJSheng.Web
{
    /// <summary>
    /// wxpay 的摘要说明
    /// </summary>
    public class wxpay : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            try
            {
                string wxNotifyXml = "";
                byte[] bytes = HttpContext.Current.Request.BinaryRead(HttpContext.Current.Request.ContentLength);
                wxNotifyXml = System.Text.Encoding.UTF8.GetString(bytes);
                //LogManager.WriteLog("微信支付返回XML", wxNotifyXml);
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.XmlResolver = null;
                xmldoc.LoadXml(wxNotifyXml);
                string ResultCode = xmldoc.SelectSingleNode("/xml/result_code").InnerText;
                string ReturnCode = xmldoc.SelectSingleNode("/xml/return_code").InnerText;
                if (ReturnCode == "SUCCESS" && ResultCode == "SUCCESS")//验证成功
                {
                    //微信签名验证
                    string sign = xmldoc.SelectSingleNode("/xml/sign").InnerText;
                    //取结果参数做业务处理
                    string out_trade_no = xmldoc.SelectSingleNode("/xml/out_trade_no").InnerText;
                    string responseContent = @"<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
                    HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
                    HttpContext.Current.Response.ContentType = "text/xml";
                    //HttpContext.Current.Response.Charset = "UTF-8";
                    HttpContext.Current.Response.Clear();

                    SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                    foreach (XmlNode xn in xmldoc.DocumentElement.ChildNodes)
                    {
                        dic.Add(xn.Name, xn.InnerText);
                    }
                    if (Help.BuildRequest(dic, Help.api_key) != sign)
                    {
                        LogManager.WriteLog("微信签名失败", "订单号:" + out_trade_no + "\r\n----------------------------------------------------\r\n" + "签名:" + sign);
                        HttpContext.Current.Response.Write(responseContent);
                    }
                    else
                    {
                        //微信支付订单号
                        string trade_no = xmldoc.SelectSingleNode("/xml/transaction_id").InnerText;
                        //现金支付金额,以分为单位
                        string cash_fee = xmldoc.SelectSingleNode("/xml/cash_fee").InnerText;
                        //金额总金额,以分为单位
                        string total_fee = xmldoc.SelectSingleNode("/xml/total_fee").InnerText;
                        decimal pay_amount = decimal.Parse(total_fee) / 100;
                        //用户在商户appid下的唯一标识
                        string openid = xmldoc.SelectSingleNode("/xml/openid").InnerText;
                        //备注
                        string attach = xmldoc.SelectSingleNode("/xml/attach").InnerText;
                        //更新订单
                        using (EFDB db = new EFDB())
                        {
                            if (attach == "shop")
                            {
                                bool tl = false;
                                var b = db.ShopOrder.Where(l => l.OrderNo == out_trade_no && l.PayStatus == 2).ToList();
                                if (b.Select(l => l.Price).Sum() == pay_amount)
                                {
                                    db.ShopOrder.Where(l => l.OrderNo == out_trade_no && l.PayStatus == 2).Update(l => new ShopOrder { RMB = pay_amount });
                                    foreach (var dr in b)
                                    {
                                        tl = Helper.ShopPayOrder(dr.Gid, trade_no, 2, dr.Price);
                                    }
                                }
                                else
                                {
                                    LogManager.WriteLog("合并金额和支付金额不对", "OrderNo=" + out_trade_no + ",PayPrice=" + pay_amount);
                                }
                                if (tl)
                                {
                                    LogManager.WriteLog("微信商城支付成功", "订单号:" + out_trade_no + ",网银订单号:" + trade_no + ",网上支付金额:" + pay_amount + ",状态:" + ResultCode + " - " + ReturnCode + "\r\n" + responseContent);
                                }
                                else
                                {
                                    LogManager.WriteLog("微信支付成功对账失败", "订单号:" + out_trade_no + ",网银订单号:" + trade_no + ",网上支付金额:" + pay_amount + ",状态:" + ResultCode + " - " + ReturnCode);
                                }
                            }
                            else
                            {
                                if (Helper.PayOrder(out_trade_no, trade_no, 2, pay_amount))
                                {
                                    LogManager.WriteLog("微信支付成功", "订单号:" + out_trade_no + ",网银订单号:" + trade_no + ",网上支付金额:" + pay_amount + ",状态:" + ResultCode + " - " + ReturnCode + "\r\n" + responseContent);
                                }
                                else
                                {
                                    LogManager.WriteLog("微信支付成功对账失败", "订单号:" + out_trade_no + ",网银订单号:" + trade_no + ",网上支付金额:" + pay_amount + ",状态:" + ResultCode + " - " + ReturnCode);
                                }
                            }
                        }
                        HttpContext.Current.Response.Write(responseContent);
                    }
                }
                else
                {
                    LogManager.WriteLog("微信对账失败", ResultCode + " - " + ReturnCode);
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("微信BUG:", err.Message + "/r/nSource:" + err.Source + "/r/nStackTrace:" + err.StackTrace);
            }
            finally
            {
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}