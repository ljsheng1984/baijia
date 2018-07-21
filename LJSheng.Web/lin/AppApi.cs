﻿using System;
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
                dic.Add("username", username);
                dic.Add("password", password);
                dic.Add("pay_password", pay_password);
                dic.Add("phone", phone);
                dic.Add("invite_code", invite_code);
                dic.Add("sign", Helper.BuildRequest(dic));
                string json = PostGet.Post("http://bccbtoken.com/api/member/register", dic);
                JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
                tl = bool.Parse(paramJson["success"].ToString());
                LogManager.WriteLog("APP接口", paramJson["message"].ToString());
                LogManager.WriteLog("APP参数", Helper.PostUrl(dic));
            }
            catch(Exception err) {
                LogManager.WriteLog("APP接口异常", "注册=" + err.Message);
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
        public static void PWD(string phone, string password, int type)
        {
            try
            {
                SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                dic.Add("phone", phone);
                dic.Add("password", password);
                dic.Add("type", type.ToString());
                string postdata = Helper.PostUrl(dic);
                dic.Add("sign", Helper.BuildRequest(dic));
                string data = PostGet.GetPage("http://bccbtoken.com/api/Memberapi/changePwd", postdata);
                LogManager.WriteLog("APP接口", data);
                LogManager.WriteLog("APP参数", postdata);
            }
            catch (Exception err)
            {
                LogManager.WriteLog("APP接口异常", "用户修改密码=" + err.Message);
            }
        }

        /// <summary>
        /// 查询用户余额
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="pay_password">交易密码</param>
        /// <param name="coin_name">币种名称：BCCB, FBCC</param>
        /// <returns>返回调用结果</returns>
        public static void MB(string phone, string pay_password, string coin_name)
        {
            try
            {
                SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                dic.Add("phone", phone);
                dic.Add("pay_password", pay_password);
                dic.Add("coin_name", coin_name);
                string postdata = Helper.PostUrl(dic);
                dic.Add("sign", Helper.BuildRequest(dic));
                string data = PostGet.GetPage("http://bccbtoken.com/api/Memberapi/getUserBalance", postdata);
                LogManager.WriteLog("APP接口", data);
                LogManager.WriteLog("APP参数", postdata);
            }
            catch (Exception err)
            {
                LogManager.WriteLog("APP接口异常", "查询用户余额=" + err.Message);
            }
        }

        /// <summary>
        /// 扣除用户余额
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="pay_password">交易密码</param>
        /// <param name="coin_name">币种名称：BCCB, FBCC</param>
        /// <param name="amount">金额</param>
        /// <returns>返回调用结果</returns>
        public static void UPMB(string phone, string pay_password, string coin_name, string amount)
        {
            try
            {
                SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                dic.Add("phone", phone);
                dic.Add("pay_password", pay_password);
                dic.Add("coin_name", coin_name);
                dic.Add("amount", amount);
                string postdata = Helper.PostUrl(dic);
                dic.Add("sign", Helper.BuildRequest(dic));
                string data = PostGet.GetPage("http://bccbtoken.com/api/Memberapi/deductUserBalance", postdata);
                LogManager.WriteLog("APP接口", data);
                LogManager.WriteLog("APP参数", postdata);
            }
            catch (Exception err)
            {
                LogManager.WriteLog("APP接口异常", "扣除用户余额=" + err.Message);
            }
        }

        /// <summary>
        /// 转入积分
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="pay_password">交易密码</param>
        /// <param name="coin_name">币种名称：BCCB, FBCC</param>
        /// <param name="amount">金额</param>
        /// <returns>返回调用结果</returns>
        public static void AddMB(string phone, string pay_password, string coin_name, string amount)
        {
            try
            {
                SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                dic.Add("phone", phone);
                dic.Add("pay_password", pay_password);
                dic.Add("coin_name", coin_name);
                dic.Add("amount", amount);
                string postdata = Helper.PostUrl(dic);
                dic.Add("sign", Helper.BuildRequest(dic));
                string data = PostGet.GetPage("http://bccbtoken.com/api/Memberapi/transfer", postdata);
                LogManager.WriteLog("APP接口", data);
                LogManager.WriteLog("APP参数", postdata);
            }
            catch (Exception err)
            {
                LogManager.WriteLog("APP接口异常", "转入积分=" + err.Message);
            }
        }

        /// <summary>
        /// 24小时均值
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
        public static void AVG()
        {
            try
            {
                string data = PostGet.Get("http://bccbtoken.com/api/Memberapi/dailyAvg");
                LogManager.WriteLog("APP接口", data);
            }
            catch (Exception err)
            {
                LogManager.WriteLog("APP接口异常", "24小时均值=" + err.Message);
            }
        }


        public static bool TEST(string account, string pwd, string paypwd, string mid, string id)
        {
            bool tl = false;
            try
            {
                SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
                dic.Add("ff", "appmr");
                dic.Add("account", account);
                dic.Add("pwd", pwd);
                dic.Add("paypwd", paypwd);
                dic.Add("mid", mid);
                dic.Add("id", id);
                dic.Add("sign", Helper.BuildRequest(dic));
                string json = PostGet.Post("http://baijmc.com/ajax/api.ashx", dic);
                //JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
                //tl = bool.Parse(paramJson["success"].ToString());
                LogManager.WriteLog("APP接口", json);
                LogManager.WriteLog("APP参数", Helper.PostUrl(dic));
            }
            catch (Exception err)
            {
                LogManager.WriteLog("APP接口异常", "注册=" + err.Message);
            }
            return tl;
        }
        #endregion
    }
}