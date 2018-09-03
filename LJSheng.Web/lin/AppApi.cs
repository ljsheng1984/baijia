using System;
using LJSheng.Common;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LJSheng.Web
{
    /// <summary>
    /// APP接口
    /// </summary>
    public static class AppApi
    {
        #region 接口
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="pay_password">交易密码</param>
        /// <param name="phone">手机号</param>
        /// <param name="invite_code">邀请码</param>
        /// <returns>返回调用结果</returns>
        public static bool AppMR(string username, string password, string pay_password, string phone, string invite_code)
        {
            bool tl = false;
            try
            {
                SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                dic.Add("httpid", LCommon.TimeToUNIX(DateTime.Now));
                dic.Add("username", username);
                dic.Add("password", password);
                dic.Add("pay_password", pay_password);
                dic.Add("phone", phone);
                dic.Add("invite_code", invite_code);
                dic.Add("sign", Helper.BuildRequest(dic));
                string json = PostGet.Post("http://bccbtoken.com/api/Memberapi/register", dic);
                LogManager.WriteLog("APP接口", json);
                JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
                tl = bool.Parse(paramJson["success"].ToString());
                if (!tl)
                {
                    //LogManager.WriteLog("注册", paramJson["message"].ToString());
                    if (paramJson["message"].ToString() == "此手机号已被注册")
                    {
                        tl = true;
                    }
                    else
                    {
                        LogManager.WriteLog("APP注册失败", paramJson["message"].ToString() + ":会员=" + phone + ",invite_code=" + invite_code);
                    }
                }
                //LogManager.WriteLog("APP接口", paramJson["message"].ToString());
                //LogManager.WriteLog("APP参数", Helper.PostUrl(dic));
            }
            catch(Exception err) {
                LogManager.WriteLog("APP接口异常", "注册("+ phone + ")=" + err.Message);
            }
            return tl;
        }

        /// <summary>
        /// 用户修改密码
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="password">密码</param>
        /// <param name="type">类型:2=登陆密码，3=交易密码</param>
        /// <returns>返回调用结果</returns>
        public static bool PWD(string phone, string password, string type)
        {
            bool tl = false;
            try
            {
                SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                dic.Add("httpid", LCommon.TimeToUNIX(DateTime.Now));
                dic.Add("phone", phone);
                dic.Add("password", password);
                dic.Add("type", type);
                dic.Add("sign", Helper.BuildRequest(dic));
                string json = PostGet.Post("http://bccbtoken.com/api/Memberapi/changePwd", dic);
                LogManager.WriteLog("APP接口", json);
                JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
                tl = bool.Parse(paramJson["success"].ToString());
                if (!tl)
                {
                    LogManager.WriteLog("APP修改密码失败", paramJson["message"].ToString() + ":会员=" + phone + ",type=" + type);
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("APP接口异常", "修改密码(" + phone + ")=" + err.Message);
            }
            return tl;
        }

        /// <summary>
        /// 查询用户余额GET
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="coin_name">币种名称：BCCB, FBCC</param>
        /// <returns>返回调用结果</returns>
        public static decimal MB(string phone, string coin_name)
        {
            decimal b = 0;
            try
            {
                SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                //dic.Add("httpid", LCommon.TimeToUNIX(DateTime.Now));
                dic.Add("phone", phone);
                dic.Add("coin_name", coin_name);
                string sign = Helper.BuildRequest(dic);
                string json = PostGet.Get("http://bccbtoken.com/api/Memberapi/getUserBalance?phone=" + phone + "&coin_name=" + coin_name + "&sign=" + sign);
                LogManager.WriteLog("APP接口", json);
                JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
                if (paramJson["success"].ToString() == "True")
                {
                    b = decimal.Parse(paramJson["data"].ToString());
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("APP接口异常", "查询余额(" + phone + ")=" + err.Message);
            }
            return b;
        }

        /// <summary>
        /// 扣除用户余额
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="coin_name">币种名称：BCCB, FBCC</param>
        /// <param name="amount">金额</param>
        /// <returns>返回调用结果</returns>
        public static bool UPMB(string phone, string coin_name, string amount)
        {
            bool tl = false;
            try
            {
                SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                dic.Add("httpid", LCommon.TimeToUNIX(DateTime.Now));
                dic.Add("phone", phone);
                dic.Add("coin_name", coin_name);
                dic.Add("amount", amount);
                dic.Add("sign", Helper.BuildRequest(dic));
                string json = PostGet.Post("http://bccbtoken.com/api/Memberapi/deductUserBalance", dic);
                LogManager.WriteLog("APP接口", json);
                JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
                tl = bool.Parse(paramJson["success"].ToString());
                if (!tl)
                {
                    LogManager.WriteLog("APP扣除用户余额失败", paramJson["message"].ToString() + ":会员=" + phone + ",币种=" + coin_name + ",金额=" + amount);
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("APP接口异常", "扣除余额(" + phone + ")=" + err.Message);
            }
            return tl;
        }

        /// <summary>
        /// 积分兑换
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="coin_name">币种名称：BCCB, FBCC</param>
        /// <param name="amount">金额</param>
        /// <returns>返回调用结果</returns>
        public static bool AddMB(string phone, string coin_name, string amount)
        {
            bool tl = false;
            try
            {
                SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                dic.Add("httpid", LCommon.TimeToUNIX(DateTime.Now));
                dic.Add("phone", phone);
                dic.Add("coin_name", coin_name);
                dic.Add("amount", amount);
                dic.Add("sign", Helper.BuildRequest(dic));
                string json = PostGet.Post("http://bccbtoken.com/api/Memberapi/transfer", dic);
                LogManager.WriteLog("APP接口", json);
                JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
                tl = bool.Parse(paramJson["success"].ToString());
                if (!tl)
                {
                    LogManager.WriteLog("APP积分兑换失败", paramJson["message"].ToString()+ ":会员="+ phone + ",币种=" + coin_name + ",金额=" + amount);
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("APP接口异常", "积分兑换(" + phone + ")=" + err.Message);
            }
            return tl;
        }

        /// <summary>
        /// 24小时均值Get
        /// </summary>
        /// <returns>
        /// {
        //  "success": true,
        //  "data": [
        //    {
        //      "coin_name": "BCCB",
        //      "total_num": "13.0000000",
        //      "total_amount": "31.0000000",
        //      "avg_price": "2.38462"
        //    },
        //    {
        //      "coin_name": "FBCC",
        //      "total_num": "13.0000000",
        //      "total_amount": "31.0000000",
        //      "avg_price": "2.38462"
        //    },
        //  ]
        //}
        /// </returns>
        public static decimal AVG(int TB)
        {
            decimal price = 0;
            try
            {
                string json = PostGet.Get("http://bccbtoken.com/api/Memberapi/dailyAvg");
                LogManager.WriteLog("APP接口", json);
                JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
                if (paramJson["success"].ToString() == "True")
                {
                    if (paramJson["data"].ToString()!= "[]")
                    {
                        price = TB== 1? decimal.Parse(paramJson["data"][0]["avg_price"].ToString()): decimal.Parse(paramJson["data"][1]["avg_price"].ToString());
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("APP接口异常", "24小时均值=" + err.Message);
            }
            return price;
        }
        #endregion
    }
}