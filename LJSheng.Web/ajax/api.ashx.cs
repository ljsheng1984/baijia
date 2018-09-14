using System;
using System.Linq;
using System.Web;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LJSheng.Data;
using LJSheng.Common;
using System.IO;
using System.Collections.Generic;
using System.Text;
using EntityFramework.Extensions;

namespace LJSheng.Web.ajax
{
    /// <summary>
    /// api 的摘要说明
    /// </summary>
    public class api : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            object returnstr = "当前设备被禁止访问";
            try
            {
                switch (context.Request["ff"])
                {
                    case "Express":
                        returnstr = Express(context);
                        break;
                    case "GetICode":
                        returnstr = GetICode(context.Request["Account"], int.Parse(context.Request["type"]));
                        break;
                    case "Rob":
                        returnstr = Rob(Guid.Parse(context.Request["gid"]));
                        break;
                    case "setExpress":
                        returnstr = setExpress(Guid.Parse(context.Request["gid"]), context.Request["Express"], context.Request["ExpressNumber"]);
                        break;
                    case "setShopExpress":
                        returnstr = setShopExpress(Guid.Parse(context.Request["gid"]), context.Request["Express"], context.Request["ExpressNumber"]);
                        break;
                    case "setCode":
                        returnstr = setCode(Guid.Parse(context.Request["gid"]), context.Request["ConsumptionCode"]);
                        break;
                    case "referee":
                        returnstr = Referee(context.Request["referee"]);
                        break;
                    case "top":
                        returnstr = Top();
                        break;
                    case "AddSC":
                        returnstr = AddSC(context.Request["Name"]);
                        break;
                    case "SendCL":
                        returnstr = SendCL(int.Parse((context.Request["MID"])), int.Parse(context.Request["Number"]));
                        break;
                    case "sign ":
                        returnstr = Sign(context);
                        break;
                    case "appmr":
                        returnstr = AppMR(context);
                        break;
                    case "apppwd":
                        returnstr = AppPWD(context);
                        break;
                    case "mb":
                        returnstr = MB(context);
                        break;
                    case "mr":
                        returnstr = MR(context);
                        break;
                    default:
                        break;
                }
            }
            catch {
                returnstr = "哇哈哈哈哈哈哈哈哈哈哈哈!你被黑了~";
            }
            context.Response.Write(JsonConvert.SerializeObject(returnstr));
            context.Response.End();
        }

        #region 快递
        /// <summary> 
        /// 快递查询
        /// </summary> 
        /// <param name="逻辑说明"></param> 
        /// <param>修改备注</param> 
        /// 2014-5-20 林建生
        public object Express(HttpContext context)
        {
            string json = "";
            using (StreamReader sr = new StreamReader(context.Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine()); //Server.UrlDecode
            }
            //解析参数
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string Express = paramJson["Express"].ToString();
            string ExpressNumber = paramJson["ExpressNumber"].ToString();
            if (!string.IsNullOrEmpty(Express) && !string.IsNullOrEmpty(ExpressNumber))
            {
                string kd = KDAPI.requestKD("type=" + Express + "&number=" + ExpressNumber);
                if (kd.Length > 10)
                {
                    JObject KDJson = JsonConvert.DeserializeObject(kd) as JObject;
                    //LogManager.WriteLog("物流", KDJson["result"]["list"].ToString());
                    return new AjaxResult(new { list = KDJson["result"]["list"] });
                }
                else
                {
                    return new AjaxResult(300, "获取物流信息失败=" + kd);
                }
            }
            else
            {
                return new AjaxResult(300, "未发货!!!");
            }
        }
        #endregion

        #region 获取验证码
        /// <summary> 
        /// 获取验证码
        /// </summary> 
        /// <param name="shouji">发送的手机号</param> 
        /// <param name="type">发送类型[1=注册验证码 2=验证码]</param> 
        /// <param name="return">success:本次发送短信编号,如果发送失败，则返回：error:错误描述</param> 
        /// <param name="逻辑说明"></param> 
        /// <param>修改备注</param> 
        /// 2014-5-20 林建生
        /// 
        public object GetICode(string account, int type)
        {
            int result = 300;
            string data = "";
            if (account.Length == 11 && account.Substring(0, 1) == "1")
            {
                using (EFDB db = new EFDB())
                {
                    //获取短信的时候先判断是否已经被注册
                    var m = db.Member.Where(l => l.Account == account).FirstOrDefault();
                    if ((m == null && type == 1) || (m != null && type == 2))
                    {
                        string Content = RandStr.CreateValidateNumber(4);
                        var time = db.SMS.Where(l => l.PhoneNumber == account && l.Type == 1).OrderByDescending(l => l.AddTime).FirstOrDefault();
                        if (time != null)
                        {
                            //判断是否十分钟内重发
                            TimeSpan ts = DateTime.Now - time.AddTime;
                            if (ts.TotalMinutes <= 10)
                            {
                                Content = time.Content;
                            }
                        }
                        SMS.SMS sms = new SMS.SMS();
                        string ReceiptContent = sms.SendSMS(account, "240053", Content);
                        Helper.SMSAdd(account, Content, 1, type, ReceiptContent);
                        JObject paramJson = JsonConvert.DeserializeObject(ReceiptContent) as JObject;
                        if (paramJson["resp"]["respCode"].ToString() == "000000")
                        {
                            result = 200;
                            data = "已发送,有效期十分钟!";
                        }
                        else
                        { data = "短信接口异常"; }

                    }
                    else
                    {
                        if (type == 2)
                        {
                            data = "手机号未注册";
                        }
                        else
                        {
                            data = "手机号已注册";
                        }
                    }
                }
            }
            else
            {
                data = "手机号码错误";
            }
            return new AjaxResult(result, data);
        }

        #endregion

        #region 推荐人
        /// <summary>
        /// 设置推荐人
        /// </summary>
        public object Referee(string referee)
        {
            using (EFDB db = new EFDB())
            {
                Guid gid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                if (b.MemberGid == null)
                {
                    var m = db.Member.Where(l => l.Account == referee && l.Gid!=gid).FirstOrDefault();
                    if (m != null)
                    {
                        b.MemberGid = m.Gid;
                        if (db.SaveChanges() == 1)
                        {
                            Helper.MLogin(gid);
                            List<Guid> list = new List<Guid>();
                            Helper.Member(gid, b.MemberGid, 1, 10, list);
                            return new AjaxResult("设置推荐人成功");
                        }
                        else
                        {
                            return new AjaxResult(300, "设置推荐人失败,请重试");
                        }
                    }
                    else
                    {
                        return new AjaxResult(300, "该帐号未注册");
                    }
                }
                else
                {
                    return new AjaxResult(300, "推荐人不能更改");
                }
            }
        }
        #endregion

        #region 抢单
        /// <summary>
        /// 抢单
        /// </summary>
        /// <param name="gid">订单Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public object Rob(Guid gid)
        {
            using (EFDB db = new EFDB())
            {
                Guid MemberGid = LCookie.GetMemberGid();
                var b = db.Order.Where(l => l.Gid == gid).FirstOrDefault();
                if (b != null && b.PayStatus == 1 && b.RobGid == null)
                {
                    //扣除库存
                    var od = db.OrderDetails.Where(l => l.OrderGid == gid).ToList();
                    foreach (var j in od)
                    {
                        Stock s = db.Stock.Where(l => l.ProductGid == j.ProductGid && l.MemberGid == MemberGid).FirstOrDefault();
                        s.Number = s.Number - j.Number;
                    }
                    if (db.SaveChanges() != od.Count)
                    {
                        LogManager.WriteLog("扣除库存失败", "MemberGid=" + MemberGid.ToString() + ",OrderGid=" + gid.ToString());
                        return new AjaxResult(300, "抢单扣除库存异常");
                    }
                    else
                    {
                        //更新抢单信息
                        b.RobGid = LCookie.GetMemberGid();
                        b.RobTime = DateTime.Now;
                        if (db.SaveChanges() == 1)
                        {
                            return new AjaxResult("恭喜你,抢单成功.请录入物流信息");
                        }
                        else
                        {
                            LogManager.WriteLog("扣除库存成功抢单失败", "MemberGid=" + MemberGid.ToString() + ",OrderGid=" + gid.ToString());
                            return new AjaxResult(300, "抢单超时,请返回订单界面重新获取");
                        }
                    }
                }
                else
                {
                    return new AjaxResult(300, "你手慢了,订单已失效");
                }
            }
        }

        /// <summary>
        /// 设置物流信息
        /// </summary>
        /// <param name="gid">订单Gid</param
        /// <param name="Express">快递公司</param>
        /// <param name="ExpressNumber">物流号</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public object setExpress(Guid gid,string Express, string ExpressNumber)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Order.Where(l => l.Gid == gid && l.PayStatus == 1 && l.RobGid != null).FirstOrDefault();
                if (b != null)
                {
                    //更新抢单信息
                    b.Express = Express;
                    b.ExpressNumber = ExpressNumber;
                    b.ExpressStatus = 2;
                    if (db.SaveChanges() == 1)
                    {
                        return new AjaxResult("发货成功");
                    }
                    else
                    {
                        return new AjaxResult(300, "发货失败,请重试!");
                    }
                }
                else
                {
                    return new AjaxResult(300, "订单已失效");
                }
            }
        }
        #endregion

        #region 后台头部
        /// <summary>
        /// 后台头部
        /// </summary>
        public object Top()
        {
            using (EFDB db = new EFDB())
            {
                int member = db.Member.Count();
                var o = db.Order.Where(l => l.PayStatus == 1);
                int order = o.Count();
                int orderStatus = o.Where(l => l.Status == 1).Count();
                int orderES = o.Where(l => l.ExpressStatus == 1).Count();
                decimal money = o.Where(l => l.PayType != 5).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                decimal integral = o.Where(l => l.PayType == 5).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                var w = db.Withdrawals;
                decimal bank = w.Select(l => l.Money).DefaultIfEmpty(0m).Sum();
                int bankState = w.Where(l => l.State == 1).Count();
                int shop = db.Shop.Where(l => l.State == 1).Count();
                int shoporder = db.ShopOrder.Where(l => l.PayStatus == 1).Count();
                var sw = db.ShopWithdrawals;
                decimal shopbank = sw.Select(l => l.Money).DefaultIfEmpty(0m).Sum();
                int shopbankState = sw.Where(l => l.State == 1).Count();
                return new AjaxResult(new { member , order , orderStatus , orderES, money, integral, bank, bankState, shop, shoporder, shopbank, shopbankState });
            }
        }
        #endregion

        #region 商家
        /// <summary>
        /// 抢单
        /// </summary>
        /// <param name="Name">分类名称</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public object AddSC(string Name)
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = LCookie.GetShopGid();
                var b = db.ShopClassify.Where(l => l.Name == Name && l.ShopGid==Gid).FirstOrDefault();
                if (b == null )
                {
                    b = new ShopClassify();
                    b.Gid = Guid.NewGuid();
                    b.AddTime = DateTime.Now;
                    b.ShopGid = Gid;
                    b.Name = Name;
                    b.Sort = 1;
                    b.Show = 1;
                    db.ShopClassify.Add(b);
                    if (db.SaveChanges() == 1)
                    {
                        return new AjaxResult("添加分类成功");
                    }
                    else
                    {
                        return new AjaxResult(300, "添加分类失败");
                    }
                }
                else
                {
                    return new AjaxResult(300, "分类名称已存在");
                }
            }
        }
        
        /// <summary>
        /// 设置物流信息
        /// </summary>
        /// <param name="gid">订单Gid</param
        /// <param name="Express">快递公司</param>
        /// <param name="ExpressNumber">物流号</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public object setShopExpress(Guid gid, string Express, string ExpressNumber)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.ShopOrder.Where(l => l.Gid == gid && l.PayStatus == 1).FirstOrDefault();
                if (b != null)
                {
                    //更新抢单信息
                    b.Express = Express;
                    b.ExpressNumber = ExpressNumber;
                    b.ExpressStatus = 2;
                    b.DeliveryTime = DateTime.Now;
                    if (db.SaveChanges() == 1)
                    {
                        return new AjaxResult("发货成功");
                    }
                    else
                    {
                        return new AjaxResult(300, "发货失败,请重试!");
                    }
                }
                else
                {
                    return new AjaxResult(300, "订单已失效");
                }
            }
        }

        /// <summary>
        /// 设置物流信息
        /// </summary>
        /// <param name="gid">订单Gid</param
        /// <param name="ConsumptionCode">消费码</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public object setCode(Guid gid, string ConsumptionCode)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.ShopOrder.Where(l => l.Gid == gid && l.PayStatus == 1 && l.ConsumptionCode == ConsumptionCode).FirstOrDefault();
                if (b != null)
                {
                    b.Status = 2;
                    b.ExpressStatus = 3;
                    if (db.SaveChanges() == 1)
                    {
                        return new AjaxResult("验证成功");
                    }
                    else
                    {
                        return new AjaxResult(300, "验证失败,请重试!");
                    }
                }
                else
                {
                    return new AjaxResult(300, "消费码有误,请检查!");
                }
            }
        }
        #region APP接口

        /// <summary>
        /// 重新向APP注册
        /// </summary>
        /// <param name="param">请求的参数</param>
        /// <returns>请求结果</returns>
        public object MR(HttpContext context)
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = Guid.Parse(context.Request["gid"]);
                var b = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                if (AppApi.AppMR(b.RealName, "123456", "123456", b.Account, b.MID.ToString()))
                {
                    b.APP = 2;
                    b.PWD = MD5.GetMD5ljsheng("123456");
                    b.PayPWD = MD5.GetMD5ljsheng("123456");
                    if (db.SaveChanges() == 1)
                    {
                        return new AjaxResult("同步成功");
                    }
                    else
                    {
                        return new AjaxResult(300, "同步成功,本地更新数据失败!");
                    }
                }
                return new AjaxResult(300,"同步失败");
            }
        }

        public object Sign(HttpContext context)
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("phone", "130130");
            dic.Add("name", "111");
            return Helper.BuildRequest(dic);
        }
        /// <summary>
        /// APP用户注册
        /// </summary>
        /// <param name="param">请求的参数</param>
        /// <returns>请求结果</returns>
        public object AppMR(HttpContext context)
        {
            string Account = context.Request.Form["account"];
            string PWD = context.Request.Form["pwd"];
            string PayPWD = context.Request.Form["paypwd"];
            int MID = Helper.CreateMNumber();//注册用户的邀请码
            int ID = int.Parse(context.Request.Form["id"]);//推荐人邀请码
            //LogManager.WriteLog("APP注册", "Account=" + Account + ",ID=" + ID.ToString());
            string Sign = context.Request.Form["sign"];
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("account", Account);
            dic.Add("pwd", PWD);
            dic.Add("paypwd", PayPWD);
            dic.Add("mid", "0");
            dic.Add("id", ID.ToString());
            if (Sign == Helper.BuildRequest(dic))
            {
                if (Account.Length == 11 && PWD.Length>5 && PayPWD.Length==6)
                {
                    using (EFDB db = new EFDB())
                    {
                        Guid Gid = Guid.NewGuid();
                        try
                        {
                            var b = new Member();
                            b.Gid = Gid;
                            b.AddTime = DateTime.Now;
                            b.Account = Account;
                            b.RealName = "请实名";
                            b.LoginIdentifier = "0000000000";
                            b.IP = Helper.IP;
                            b.Money = 0;
                            b.Integral = 0;
                            b.Money = 0;
                            b.ShopIntegral = 0;
                            b.MIntegral = 0;
                            b.TIntegral = 0;
                            b.ShopMoney = 0;
                            b.ProductMoney = 0;
                            b.StockRight = 0;
                            b.CLMoney = 0;
                            b.Level = 1;
                            b.Level6 = 0;
                            b.Level7 = 0;
                            b.Level8 = 0;
                            b.Level9 = 0;
                            b.TMoney = 0;
                            b.TNumber = 0;
                            b.PWD = MD5.GetMD5ljsheng(context.Request.Form["PWD"]);
                            b.PayPWD = MD5.GetMD5ljsheng(context.Request.Form["PayPWD"]);
                            b.MID = MID;
                            b.Jurisdiction = "正常";
                            b.Gender = "男";
                            b.CLLevel = 21;
                            b.BuyPrice = 0;
                            b.Level22 = 0;
                            b.Level23 = 0;
                            b.Level24 = 0;
                            b.Level25 = 0;
                            b.CLTMoney = 0;
                            b.CLTNumber = 0;
                            b.APP = 3;
                            if (ID != 0)
                            {
                                var m = db.Member.Where(l => l.MID == ID).FirstOrDefault();
                                if (m != null)
                                {
                                    b.MemberGid = m.Gid;
                                }
                            }
                            if (db.Member.Where(l => l.Account == Account).Count() == 0)
                            {
                                db.Member.Add(b);
                                if (db.SaveChanges() == 1)
                                {
                                    //删除重复注册数据
                                    //db.Member.Where(l => l.Account == Account && l.Gid != Gid).Delete();
                                    //var md = db.Member.Where(l => l.Account == Account && l.Gid != Gid).ToList();
                                    //foreach (var dr in md)
                                    //{
                                    //    db.Member.Where(l => l.MemberGid == dr.MemberGid).Delete();
                                    //    db.MRelation.Where(l => l.MemberGid == dr.MemberGid).Delete();
                                    //    db.Consignor.Where(l => l.MemberGid == dr.MemberGid).Delete();
                                    //}
                                    //增加彩链发货人
                                    Helper.SetConsignor(b.Gid, b.MemberGid);
                                    //增加推荐人
                                    if (b.MemberGid != null)
                                    {
                                        Helper.MRelation(Gid, (Guid)b.MemberGid);
                                    }
                                    return new AjaxResult(MID);
                                }
                                else
                                {
                                    return new AjaxResult(302, "注册失败");
                                }
                            }
                            else
                            {
                                return Helper.Redirect("失败", "history.go(-1);", "帐号已存在");
                            }
                        }
                        catch
                        {
                            db.Member.Where(l => l.Gid == Gid).Delete();
                            db.MRelation.Where(l => l.MemberGid == Gid).Delete();
                            db.Achievement.Where(l => l.MemberGid == Gid).Delete();
                            return new AjaxResult(300, "服务器请求超时!!!");
                        }
                    }
                }
                else
                {
                    return new AjaxResult(301, "手机号必须是11位,密码不少于6位!");
                }
            }
            else
            {
                return new AjaxResult(300, "签名异常");
            }
        }

        /// <summary>
        /// APP用户修改密码
        /// </summary>
        /// <param name="param">请求的参数</param>
        /// <returns>请求结果</returns>
        public object AppPWD(HttpContext context)
        {
            string Account = context.Request.Form["account"];
            string PWD = context.Request.Form["pwd"];
            string PayPWD = context.Request.Form["paypwd"];
            string Sign = context.Request.Form["sign"];
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("account", Account);
            dic.Add("pwd", PWD);
            dic.Add("paypwd", PayPWD);
            if (Sign == Helper.BuildRequest(dic))
            {
                using (EFDB db = new EFDB())
                {
                    var b = db.Member.Where(l => l.Account == Account).FirstOrDefault();
                    if (b != null)
                    {
                        if (!string.IsNullOrEmpty(PWD))
                        {
                            b.PWD = MD5.GetMD5ljsheng(PWD);
                        }
                        if (!string.IsNullOrEmpty(PayPWD))
                        {
                            b.PayPWD = MD5.GetMD5ljsheng(PayPWD);
                        }
                        b.LoginIdentifier = "";
                        if (db.SaveChanges() == 1)
                        {
                            return new AjaxResult("修改成功!!!");
                        }
                        else
                        {
                            return new AjaxResult(300, "更新失败!!!");
                        }
                    }
                    else
                    {
                        return new AjaxResult(301, "用户不存在!!!");
                    }
                }
            }
            else
            {
                return new AjaxResult(300, "签名异常");
            }
        }

        /// <summary>
        /// 查询用户钱包
        /// </summary>
        /// <param name="param">请求的参数</param>
        /// <returns>请求结果</returns>
        public object MB(HttpContext context)
        {
            string Account = context.Request.Form["account"];
            string B = context.Request.Form["b"];
            return new AjaxResult(B + "余额=" + AppApi.MB(Account, B));
        }
        #endregion

        #endregion

        /// <summary>
        /// 抢单
        /// </summary>
        /// <param name="Name">分类名称</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public object SendCL(int MID,int Number)
        {
            using (EFDB db = new EFDB())
            {
                var Member = db.Member.Where(l => l.MID == MID).FirstOrDefault();
                if (MID.ToString().Length==8 && Member != null)
                {
                    Guid Gid = Member.Gid;
                    Guid MemberGid = LCookie.GetMemberGid();
                    string msg = "转让人=" + MemberGid + ",接受人=" + Gid + ",MID=" + MID + ",Number=" + Number;
                    if (Helper.IsMember(Gid, MemberGid))
                    {
                        //订单的Gid
                        Guid OrderGid = Guid.NewGuid();
                        //商品Gid,有订单号为会员转让商品
                        Guid ProductGid = Helper.GetProductGid();
                        //扣除库存
                        if (Helper.CLStock(OrderGid, Gid, MemberGid, Number, 0, 0, Member.Account))
                        {
                            LogManager.WriteLog("个人转让扣除库存成功", msg);
                            //订单详情
                            var od = new OrderDetails();
                            od.Gid = Guid.NewGuid();
                            od.AddTime = DateTime.Now;
                            od.OrderGid = OrderGid;
                            od.ProductGid = ProductGid;
                            od.Number = Number;
                            od.Money = 0;
                            od.Integral = 0;
                            od.Price = 0;
                            db.OrderDetails.Add(od);
                            if (db.SaveChanges() == 1)
                            {
                                //生成订单
                                var b = new Order();
                                b.Gid = OrderGid;
                                b.AddTime = DateTime.Now;
                                b.OrderNo = "0";
                                b.MemberGid = Gid;
                                b.ShopGid = MemberGid;
                                b.CLLevel = Member.CLLevel;
                                b.PayStatus = 1;
                                b.PayType = 4;
                                b.TotalPrice = 0;
                                b.Price = 0;
                                b.PayPrice = 0;
                                b.CouponPrice = 0;
                                b.ExpressStatus = 3;
                                b.Remarks = "个人转让";
                                b.Product = "个人转让";
                                b.Status = 2;
                                b.Profit = 0;
                                b.Money = 0;
                                b.Integral = 0;
                                b.StockRight = 0;
                                b.Project = 2;
                                b.Type = 5;
                                db.Order.Add(b);
                                if (db.SaveChanges() == 1)
                                {
                                    if (db.Stock.Where(l => l.MemberGid == Gid && l.ProductGid == ProductGid).Update(l => new Stock { Number = l.Number + Number }) == 1)
                                    {
                                        return new AjaxResult("转让成功");
                                    }
                                    else
                                    {
                                        LogManager.WriteLog("个人转让扣除库存成功增加失败", msg);
                                        return new AjaxResult(300, "扣除成功转让失败");
                                    }
                                }
                                else
                                {
                                    db.OrderDetails.Where(l => l.OrderGid == OrderGid).Delete();
                                    db.Order.Where(l => l.Gid == OrderGid).Delete();
                                }
                            }
                            else
                            {
                                db.OrderDetails.Where(l => l.OrderGid == OrderGid).Delete();
                            }
                        }
                        else {
                            return new AjaxResult(300, "扣除库存异常");
                        }
                        return new AjaxResult(300, "转让失败");
                    }
                    else
                    {
                        return new AjaxResult(300, "只能转让给自己的下线");
                    }
                }
                else
                {
                    return new AjaxResult(300, "请输入正确的下级ID");
                }
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