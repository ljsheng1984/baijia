using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using Aop.Api.Util;
using EntityFramework.Extensions;
using LJSheng.Common;
using LJSheng.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;

namespace LJSheng.Web.Controllers
{
    public class PayController : Controller
    {
        /// <summary>
        /// 支付成功
        /// </summary>
        public ActionResult PayOK()
        {
            string OrderNo = Request.QueryString["OrderNo"];
            int type = int.Parse(Request.QueryString["type"]);
            using (EFDB db = new EFDB())
            {
                if (type==2)
                {
                    var b = db.ShopOrder.Where(l => l.OrderNo == OrderNo).FirstOrDefault();
                    ViewBag.TotalPrice = b.TotalPrice;
                    ViewBag.Gid = b.Gid;
                    ViewBag.Address = b.Address;
                    ViewBag.ContactNumber = b.ContactNumber;
                    ViewBag.RealName = b.RealName;
                }
                else
                {
                    var b = db.Order.Where(l => l.OrderNo == OrderNo).FirstOrDefault();
                    ViewBag.TotalPrice = b.TotalPrice;
                    ViewBag.Gid = b.Gid;
                    ViewBag.Address = b.Address;
                    ViewBag.ContactNumber = b.ContactNumber;
                    ViewBag.RealName = b.RealName;
                }
            }
            return View();
        }

        /// <summary>
        /// 支付选择页面
        /// </summary>
        public ActionResult GoPay()
        {
            using (EFDB db = new EFDB())
            {
                //当前项目
                int Project = LCookie.Project();
                ViewBag.Project = Project;
                Guid MemberGid = LCookie.GetMemberGid();
                if (MemberGid.ToString() != "00000000-0000-0000-0000-000000000000")
                {
                    var b = db.Address.Where(l => l.MemberGid == MemberGid && l.Default == 2).FirstOrDefault();
                    if (b != null)
                    {
                        ViewBag.Addr = b.Addr;
                        ViewBag.RealName = b.RealName;
                        ViewBag.ContactNumber = b.ContactNumber;
                    }
                    else
                    {
                        ViewBag.RealName = "请设置你的收货地址";
                    }
                    //获取用户积分
                    ViewBag.Integral = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault().Integral;
                    //获取最低合伙人金额
                    //ViewBag.BuyAmount = db.Level.Where(l => l.LV == 6).FirstOrDefault().BuyAmount;
                    return View();
                }
                else
                {
                    return Helper.Redirect("失败", "/Home/Login", "请先登录!");
                }
            }
            
        }

        /// <summary>
        /// 下单支付
        /// </summary>
        [HttpPost]
        public ActionResult Pay()
        {
            string RealName = Request.Form["RealName"];
            string ContactNumber = Request.Form["ContactNumber"];
            string Address = Request.Form["Addr"];
            string ProductList = Request.Form["shopcart"];
            int PayType = int.Parse(Request.Form["PayType"]);
            string Remarks = Request.Form["Remarks"];
            string OrderNo = Request.Form["OrderNo"];
            //购买会员Gid
            Guid MemberGid = LCookie.GetMemberGid();
            //当前项目
            int Project = LCookie.Project();
            ViewBag.Project = Project;
            //订单信息
            string Order = "";
            //下单返回信息
            if (string.IsNullOrEmpty(OrderNo))
            {
                Order = Helper.CLOrder(4, 0, ProductList, MemberGid, Project, PayType, Remarks, Address, RealName, ContactNumber);
            }
            else
            {
                using (EFDB db = new EFDB())
                {
                    var b =db.Order.Where(l => l.OrderNo == OrderNo).FirstOrDefault();
                    if (b != null && b.PayStatus == 2 && b.ShopGid != MemberGid &&b.ShopGid==b.MemberGid)
                    {
                        b.MemberGid = MemberGid;
                        if (db.SaveChanges() == 1)
                        {
                            Order = JsonConvert.SerializeObject(new { body = b.Product, b.Price, b.OrderNo, b.Gid });
                        }
                        else
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "获取订单失败!");
                        }
                    }
                    else
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "下手慢了,被人抢了!");
                    }
                }
            }
            if (!string.IsNullOrEmpty(Order))
            {
                JObject paramJson = JsonConvert.DeserializeObject(Order) as JObject;
                {
                    if (string.IsNullOrEmpty(paramJson["OrderNo"].ToString()))
                    {
                        return Helper.Redirect(paramJson["Title"].ToString(), "history.go(-1);", paramJson["Error"].ToString());
                    }
                    else
                    {
                        switch (PayType)
                        {
                            case 1:
                                return Alipay(paramJson["OrderNo"].ToString(), paramJson["body"].ToString(), paramJson["TotalPrice"].ToString(), 1);
                            case 2:
                                if (!string.IsNullOrEmpty(LCookie.GetCookie("openid")))
                                {
                                    string beizhu = "cl";//备注
                                    //开始微信统一下单
                                    JObject j = WX.WXPay.Get_RequestHtml(LCookie.GetCookie("openid"), paramJson["OrderNo"].ToString(), "彩链订单", beizhu, paramJson["TotalPrice"].ToString());
                                    return Json(new AjaxResult(new
                                    {
                                        OrderNo = paramJson["OrderNo"].ToString(),
                                        appId = j["appId"].ToString(),
                                        timeStamp = j["timeStamp"].ToString(),
                                        nonceStr = j["nonceStr"].ToString(),
                                        package = j["package"].ToString(),
                                        paySign = j["paySign"].ToString(),
                                        signType = j["signType"].ToString()
                                    }));
                                }
                                else
                                {
                                    return Json(new AjaxResult(301,"微信支付,请在微信里打开重新登录进行支付"));
                                }
                            case 3:
                                return new RedirectResult("/Home/Bank?Type=1&OrderNo=" + paramJson["OrderNo"].ToString() + "&Money=" + paramJson["TotalPrice"].ToString());
                            default:
                                return Helper.Redirect("失败", "history.go(-1);", "非法支付");
                        }
                    }
                }
            }
            else
            {
                return Helper.Redirect("失败", "history.go(-1);", "提交订单失败");
            }
        }

        /// <summary>
        /// 支付成功
        /// </summary>
        public ActionResult Buy()
        {
            return View(Guid.NewGuid());
        }

        #region 积分支付
        /// <summary>
        /// 积分支付
        /// </summary>
        /// <param name="out_trade_no">外部订单号，商户网站订单系统中唯一的订单号</param>
        /// <param name="subject">订单名称</param>
        /// <param name="total_amout">会员帐号</param>
        /// <param name="OrderGid">订单Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult MPay(string out_trade_no, string subject, string total_amout, Guid OrderGid)
        {
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                decimal Price = decimal.Parse(total_amout);
                if (b.Integral- Price >= 0)
                {
                    if (Helper.MoneyRecordAdd(OrderGid, MemberGid, 0, -Price,1) != null)
                    {
                        if (db.Order.Where(l => l.OrderNo == out_trade_no).Update(l => new Order { PayStatus = 1,PayTime=DateTime.Now }) == 1)
                        {
                            return new RedirectResult("/Pay/PayOK?OrderNo=" + out_trade_no);
                        }
                        else
                        {
                            LogManager.WriteLog("积分支付成功订单更新失败", out_trade_no);
                        }
                    }
                }
                return Helper.Redirect("失败", "history.go(-1);", "积分支付异常");
            }
        }
        #endregion

        #region 支付宝
        /// <summary>
        /// 支付宝支付
        /// </summary>
        /// <param name="out_trade_no">外部订单号，商户网站订单系统中唯一的订单号</param>
        /// <param name="subject">订单名称</param>
        /// <param name="total_amout">会员帐号</param>
        /// <param name="type">对账类型[1=链商城 2=商城]</param>
        public ActionResult Alipay(string out_trade_no, string subject,string total_amout,int type,string ReturnType = "0")
        {
            // 商品描述
            string body = "链商支付";

            // 支付中途退出返回商户网站地址
            string quit_url = Help.quit_url;

            // 组装业务参数model
            AlipayTradeWapPayModel model = new AlipayTradeWapPayModel();
            model.Body = body;
            model.Subject = subject;
            model.TotalAmount = total_amout;
            model.OutTradeNo = out_trade_no;
            model.ProductCode = "QUICK_WAP_PAY";
            model.QuitUrl = quit_url;
            AlipayTradeWapPayRequest request = new AlipayTradeWapPayRequest();
            // 设置支付完成同步回调地址
            request.SetReturnUrl(Help.Url + "/Pay/PayOK?type="+ type.ToString() + "&OrderNo=" + out_trade_no + "&ReturnType=" + ReturnType);
            // 设置支付完成异步通知接收地址
            if (type == 1)
            {
                request.SetNotifyUrl(Help.notify_url);
            }
            else if (type == 2)
            {
                request.SetNotifyUrl(Help.shopnotify_url);
            }
            // 将业务model载入到request
            request.SetBizModel(model);

            AlipayTradeWapPayResponse response = null;
            try
            {
                DefaultAopClient client = new DefaultAopClient(Help.gatewayUrl, Help.app_id, Help.private_key, "json", "1.0", Help.sign_type, Help.alipay_public_key, Help.charset, false);
                response = client.pageExecute(request, null, "post");
                return Content(response.Body);
            }
            catch (Exception exp)
            {
                return Helper.Redirect("支付失败", "history.go(-1);", "订单异常=" + exp.Message);
            }
        }

        /// <summary>
        /// 自动对账
        /// </summary>
        public ActionResult NotifyUrl()
        {
            /* 实际验证过程建议商户添加以下校验。
            1、商户需要验证该通知数据中的out_trade_no是否为商户系统中创建的订单号，
            2、判断total_amount是否确实为该订单的实际金额（即商户订单创建时的金额），
            3、校验通知中的seller_id（或者seller_email) 是否为out_trade_no这笔单据的对应的操作方（有的时候，一个商户可能有多个seller_id/seller_email）
            4、验证app_id是否为该商户本身。
            */
            Dictionary<string, string> sArray = GetRequestPost();
            if (sArray.Count != 0)
            {
                bool flag = AlipaySignature.RSACheckV1(sArray, Help.alipay_public_key, Help.charset, Help.sign_type, false);
                if (flag)
                {
                    //交易状态
                    //判断该笔订单是否在商户网站中已经做过处理
                    //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                    //请务必判断请求时的total_amount与通知时获取的total_fee为一致的
                    //如果有做过处理，不执行商户的业务程序

                    //注意：
                    //退款日期超过可退款期限后（如三个月可退款），支付宝系统发送该交易状态通知
                    //商户订单号
                    string out_trade_no = Request.Form["out_trade_no"];
                    //支付宝交易号
                    string trade_no = Request.Form["trade_no"];
                    //获取总金额 
                    string total_amount = Request.Form["total_amount"];
                    //交易状态
                    string trade_status = Request.Form["trade_status"];
                    if (trade_status == "TRADE_SUCCESS" || trade_status == "TRADE_FINISHED")
                    {
                        if (Helper.PayOrder(out_trade_no, trade_no, 1, decimal.Parse(total_amount)))
                        {
                            return Content("success");
                        }
                        else
                        {
                            LogManager.WriteLog("支付宝对账失败", out_trade_no);
                        }
                    }
                    else
                    {
                        LogManager.WriteLog("支付宝支付状态失败", out_trade_no);
                    }
                }
                else
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    var form = Request.Form;
                    string str = "异步通知：\r\n";
                    foreach (var key in form)
                    {
                        dic[key.ToString()] = Request.Form[key.ToString()];
                        var value = Request.Form[key.ToString()];
                        //记录日志使用
                        str += $"{key.ToString()}:{value}\r\n";
                    }
                    LogManager.WriteLog("支付宝签名不通过", str);
                }
            }
            else
            {
                LogManager.WriteLog("支付宝参数失败", sArray.Count.ToString());
            }
            return Content("fail");
        }

        /// <summary>
        /// 商城自动对账
        /// </summary>
        public ActionResult ShopNotifyUrl()
        {
            Dictionary<string, string> sArray = GetRequestPost();
            if (sArray.Count != 0)
            {
                bool flag = AlipaySignature.RSACheckV1(sArray, Help.alipay_public_key, Help.charset, Help.sign_type, false);
                if (flag)
                {
                    string out_trade_no = Request.Form["out_trade_no"];
                    //支付宝交易号
                    string trade_no = Request.Form["trade_no"];
                    //获取总金额 
                    string total_amount = Request.Form["total_amount"];
                    //交易状态
                    string trade_status = Request.Form["trade_status"];
                    if (trade_status == "TRADE_SUCCESS" || trade_status == "TRADE_FINISHED")
                    {
                        if (Helper.ShopOrder(out_trade_no, trade_no, 1, decimal.Parse(total_amount)))
                        {
                            return Content("success");
                        }
                    }
                }
            }
            return Content("fail");
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        public ActionResult Query()
        {
            DefaultAopClient client = new DefaultAopClient(Help.gatewayUrl, Help.app_id, Help.private_key, "json", "1.0", Help.sign_type, Help.alipay_public_key, Help.charset, false);

            // 商户订单号，和交易号不能同时为空
            string out_trade_no = "22222";

            // 支付宝交易号，和商户订单号不能同时为空
            string trade_no = "222222";

            AlipayTradeQueryModel model = new AlipayTradeQueryModel();
            model.OutTradeNo = out_trade_no;
            model.TradeNo = trade_no;

            AlipayTradeQueryRequest request = new AlipayTradeQueryRequest();
            request.SetBizModel(model);

            AlipayTradeQueryResponse response = null;
            try
            {
                response = client.Execute(request);
                return Content(response.Body);

            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        /// <summary>
        /// 封装
        /// </summary>
        public Dictionary<string, string> GetRequestPost()
        {
            int i = 0;
            Dictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;
            //coll = Request.Form;
            coll = Request.Form;
            String[] requestItem = coll.AllKeys;
            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }
            return sArray;

        }
        #endregion

        #region 商城支付模块
        /// <summary>
        /// 支付选择页面
        /// </summary>
        public ActionResult ShopPay()
        {
            using (EFDB db = new EFDB())
            {
                Guid MemberGid = LCookie.GetMemberGid();
                if (MemberGid.ToString() != "00000000-0000-0000-0000-000000000000")
                {
                    if (db.Cart.Where(l => l.MemberGid == MemberGid && l.State == 1).Count() > 0)
                    {
                        var b = db.Address.Where(l => l.MemberGid == MemberGid && l.Default == 2).FirstOrDefault();
                        if (b != null)
                        {
                            ViewBag.Addr = b.Addr;
                            ViewBag.RealName = b.RealName;
                            ViewBag.ContactNumber = b.ContactNumber;
                        }
                        else
                        {
                            ViewBag.RealName = "请设置你的收货地址";
                        }
                        //获取用户积分
                        ViewBag.Integral = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault().Integral;
                        //订单金额
                        ViewBag.RMB = Helper.OrderRMB(MemberGid);
                    }
                    else
                    {
                        return Helper.Redirect("失败", "/SMall/Index", "请先添加商品!");
                    }
                    return View();
                }
                else
                {
                    return Helper.Redirect("失败", "/Home/login", "请先登录!");
                }
            }
        }

        /// <summary>
        /// 商城下单支付
        /// </summary>
        [HttpPost]
        public ActionResult PayShop()
        {
            string RealName = Request.Form["RealName"];
            string ContactNumber = Request.Form["ContactNumber"];
            string Address = Request.Form["Addr"];
            string Remarks = Request.Form["Remarks"];
            int PayType = int.Parse(Request.Form["PayType"]);
            //订单类型 Type=0 非借用订单
            int ReturnType = int.Parse(Request.Form["ReturnType"]);
            string OrderNo = RandStr.CreateOrderNO();
            decimal RMB = 0;//全部订单金额
            using (EFDB db = new EFDB())
            {
                Guid MGid = LCookie.GetMemberGid();
                //如果是积分支付先验证支付密码
                var m = db.Member.Where(l => l.Gid == MGid).FirstOrDefault();
                if (PayType == 5)
                {
                    if (m.PayPWD != MD5.GetMD5ljsheng(Request.Form["PayPWD"]))
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "支付密码错误!");
                    }
                }
                //购物车转化成订单
                var c = db.Cart.Where(l => l.MemberGid == MGid && l.State == 1).ToList();
                if (c != null)
                {
                    foreach (var dr in c)
                    {
                        //借用订单的产品Gid
                        string pg ="";
                        decimal Price = 0;//订单金额
                        //扣除库存
                        var od = db.OrderDetails.Where(l => l.OrderGid == dr.Gid).ToList();
                        foreach (var d in od)
                        {
                            var p = db.ShopProduct.Where(l => l.Gid == d.ProductGid).FirstOrDefault();
                            pg = p.Gid.ToString();
                            if (ReturnType == 0 || (ReturnType != 0 && p.Borrow == 1 && db.ShopOrder.Where(l => l.Product == pg && l.PayStatus == 1).Count() == 0))
                            {
                                if (p.Stock >= d.Number)
                                {
                                    //扣除库存
                                    p.Stock = p.Stock - d.Number;
                                    if (db.SaveChanges() == 1)
                                    {
                                        //库存扣除状态
                                        if (db.OrderDetails.Where(l => l.Gid == d.Gid).Update(l => new OrderDetails { State = 1 }) == 1)
                                        {
                                            RMB += d.Number * p.Price;
                                            Price += d.Number * p.Price;
                                        }
                                        else
                                        {
                                            LogManager.WriteLog("库存扣除成功更新OD失败", "Gid=" + d.Gid);
                                        }
                                    }
                                }
                                else
                                {
                                    //库存不足的直接删除
                                    db.OrderDetails.Where(l => l.Gid == d.Gid).Delete();
                                }
                            }
                            else
                            {
                                return Helper.Redirect("你下手慢了", "history.go(-1);", "你下手慢了,已被借用");
                            }
                        }
                        //生成订单
                        var b = new ShopOrder();
                        b.Gid = dr.Gid;
                        b.AddTime = DateTime.Now;
                        b.MemberGid = MGid;
                        b.ShopGid = dr.ShopGid;
                        b.OrderNo = OrderNo;
                        b.PayStatus = 2;
                        b.PayType = PayType;
                        b.RMB = 0;
                        b.TotalPrice = Price;
                        b.Price = Price;
                        b.CouponPrice = 0;
                        b.ExpressStatus = 1;
                        b.PayPrice = 0;
                        b.Profit = 0;
                        b.ConsumptionCode = RandStr.CreateValidateNumber(8);
                        b.Status = 1;
                        b.Remarks = Remarks;
                        b.Address = Address;
                        b.ContactNumber = ContactNumber;
                        b.RealName = RealName;
                        b.ReturnType = ReturnType;
                        if (ReturnType != 0)
                        {
                            b.BorrowTime = b.AddTime.AddMonths(3);
                            b.Product = pg;
                        }
                        db.ShopOrder.Add(b);
                        if (db.SaveChanges() == 1)
                        {
                            if (ReturnType != 0 && pg!="")
                            {
                                Guid PGid = Guid.Parse(pg);
                                if (db.ShopProduct.Where(l => l.Gid == PGid).Update(l => new ShopProduct { Borrow = 2 }) != 1)
                                {
                                    LogManager.WriteLog("借用订单状态失败", "产品Gid=" + pg + ",订单号=" + b.Gid);
                                }
                            }
                            if (db.Cart.Where(l => l.Gid == dr.Gid).Delete() != 1)
                            {
                                LogManager.WriteLog("删除购物车订单失败", "Gid=" + dr.Gid);
                            }
                        }
                        else
                        {
                            LogManager.WriteLog("购物车转订单失败", "Gid=" + dr.Gid);
                            return Helper.Redirect("失败", "history.go(-1);", "购物车转订单失败");
                        }
                    }
                    if (RMB > 0)
                    {
                        switch (PayType)
                        {
                            case 1:
                                return Alipay(OrderNo, "商城订单支付", RMB.ToString(), 2, Request.Form["ReturnType"]);                                                                                 //return MPay(paramJson["OrderNo"].ToString(), paramJson["body"].ToString(), paramJson["TotalPrice"].ToString(), Guid.Parse(paramJson["OrderGid"].ToString()));
                            case 5:
                                return MShopPay(OrderNo, "商城订单支付", RMB, m.Integral, Request.Form["ReturnType"]);
                            case 3:
                                db.ShopOrder.Where(l => l.OrderNo == OrderNo && l.PayType == 3).Update(l => new ShopOrder { RMB = RMB });
                                return new RedirectResult("/Home/Bank?Type=2&OrderNo=" + OrderNo + "&Money=" + RMB.ToString());
                            case 2:
                                if (!string.IsNullOrEmpty(LCookie.GetCookie("openid")))
                                {
                                    string beizhu = "cl";//备注
                                    //开始微信统一下单
                                    JObject j = WX.WXPay.Get_RequestHtml(LCookie.GetCookie("openid"), OrderNo, "彩链商城订单", beizhu, RMB.ToString());
                                    return Json(new AjaxResult(new
                                    {
                                        OrderNo,
                                        appId = j["appId"].ToString(),
                                        timeStamp = j["timeStamp"].ToString(),
                                        nonceStr = j["nonceStr"].ToString(),
                                        package = j["package"].ToString(),
                                        paySign = j["paySign"].ToString(),
                                        signType = j["signType"].ToString()
                                    }));
                                }
                                else
                                {
                                    return Json(new AjaxResult(301, "微信支付,请在微信里打开重新登录进行支付"));
                                }
                            default:
                                return Helper.Redirect("失败", "history.go(-1);", "非法支付");
                        }
                    }
                    else
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "提交订单失败!");
                    }
                }
                else
                {
                    return Helper.Redirect("失败", "history.go(-1);", "购物车发生变化!");
                }
            }
        }

        #region 商城积分支付
        /// <summary>
        /// 商城积分支付
        /// </summary>
        /// <param name="OrderNo">外部订单号，商户网站订单系统中唯一的订单号</param>
        /// <param name="subject">订单名称</param>
        /// <param name="PayPrice">支付金额</param>
        /// <param name="Integral">用户的积分</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult MShopPay(string OrderNo, string subject, decimal PayPrice, decimal Integral,string ReturnType="0")
        {
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                if (Integral >= PayPrice)
                {
                    var b = db.ShopOrder.Where(l => l.OrderNo == OrderNo && l.PayStatus == 2).ToList();
                    if (b.Select(l => l.Price).Sum() == PayPrice)
                    {
                        db.ShopOrder.Where(l => l.OrderNo == OrderNo && l.PayStatus == 2).Update(l => new ShopOrder { RMB = PayPrice });
                        foreach (var dr in b)
                        {
                            if (Helper.MoneyRecordAdd(dr.Gid, MemberGid, 0, -dr.Price, 1) != null)
                            {
                                if (!Helper.ShopPayOrder(dr.Gid, "", 5, dr.Price))
                                {
                                    LogManager.WriteLog("积分支付成功更新订单失败", "订单=" + dr.Gid);
                                }
                            }
                        }
                        return new RedirectResult("/Pay/PayOK?type=2&OrderNo=" + OrderNo+ "&ReturnType=" + ReturnType);
                    }
                    else
                    {
                        LogManager.WriteLog("合并金额和支付金额不对", "OrderNo=" + OrderNo + ",PayPrice=" + PayPrice);
                        return Helper.Redirect("失败", "history.go(-1);", "支付异常,请联系客服!");
                    }
                }
                else
                {
                    return Helper.Redirect("失败", "history.go(-1);", "积分不足!");
                }
            }
        }

        /// <summary>
        /// 扣除库存
        /// </summary>
        /// <param name="OrderGid">订单Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static void Stock(Guid OrderGid)
        {
            try
            {
                using (EFDB db = new EFDB())
                {
                    int num = 0;
                    var od = db.OrderDetails.Where(l => l.OrderGid == OrderGid).ToList();
                    foreach (var dr in od)
                    {
                        num++;
                        db.ShopProduct.Where(l => l.Gid == dr.ProductGid).Update(l => new ShopProduct { Stock = l.Stock - dr.Number });
                    }
                    if (num != od.Count())
                    {
                        LogManager.WriteLog("商城库存失败", "失败原因=OrderGid="+ OrderGid.ToString() + ",查询条数:" + od.Count() + ".实际操作条数:" + num.ToString() + "\r\n");
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("商城库存异常", "失败原因=OrderGid=" + OrderGid.ToString() + ",err:" + err.Message + "\r\n");
            }
        }
        #endregion
        #endregion
    }
}