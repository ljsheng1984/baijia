using EntityFramework.Extensions;
using LJSheng.Common;
using LJSheng.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LJSheng.Web.Controllers
{
    public class MemberController : MController
    {
        #region 收货地址
        /// <summary>
        /// 收货地址列表
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Address()
        {
            using (EFDB db = new EFDB())
            {
                Guid MemberGid = LCookie.GetMemberGid();
                return View(db.Address.Where(l => l.MemberGid == MemberGid).ToList());
            }
        }

        /// <summary>
        /// 地址增加
        /// </summary>
        /// <param name="type">1=联系方式 0=收货地址</param>
        /// <param name="RealName">真实姓名</param>
        /// <param name="ContactNumber">联系方式</param>
        /// <param name="Addr">地址</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult AddAddress(int type, string RealName, string ContactNumber, string Province, string City, string Area, string Addr, string url)
        {
            if (string.IsNullOrEmpty(RealName) || string.IsNullOrEmpty(ContactNumber) || string.IsNullOrEmpty(Addr))
            {
                if (type == 1)
                {
                    using (EFDB db = new EFDB())
                    {
                        Guid Gid = LCookie.GetMemberGid();
                        var b = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                        ViewBag.RealName = b.RealName;
                        ViewBag.ContactNumber = b.ContactNumber;
                        ViewBag.province = b.Province;
                        ViewBag.city = b.City;
                        ViewBag.area = b.Area;
                        ViewBag.Address = b.Address;
                    }
                }
                return View();
            }
            else
            {
                if (RealName.Length > 1 && ContactNumber.Length > 10 && Addr.Length > 5)
                {
                    using (EFDB db = new EFDB())
                    {
                        if (type == 0)
                        {
                            var b = new Address();
                            b.Gid = Guid.NewGuid();
                            b.AddTime = DateTime.Now;
                            b.MemberGid = LCookie.GetMemberGid();
                            b.Default = 1;
                            b.RealName = RealName;
                            b.ContactNumber = ContactNumber;
                            b.Addr = Province + City + Area + Addr;
                            db.Address.Add(b);
                        }
                        else
                        {
                            Guid Gid = LCookie.GetMemberGid();
                            var b = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                            b.RealName = RealName;
                            b.ContactNumber = ContactNumber;
                            b.Province = Province;
                            b.City = City;
                            b.Area = Area;
                            b.Address = Addr;
                            LCookie.AddCookie("ordercity", City, 30);
                        }
                        if (db.SaveChanges() == 1)
                        {
                            return Helper.Redirect("成功", type == 0 ? "/Home/Index" : "/Member/Address?type=" + type.ToString() + "&url=" + url, "操作成功,请跳转");
                        }
                        else
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "操作失败,点确定返回重新操作");
                        }
                    }
                }
                else
                {
                    return Helper.Redirect("失败", "history.go(-1);", "请填写真实的收货地址");
                }
            }
        }

        /// <summary>
        /// 删除收货地址
        /// </summary>
        [HttpPost]
        public JsonResult AddressDelete(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                if (db.Address.Where(l => l.Gid == Gid).Delete() == 1)
                {
                    return Json(new AjaxResult("成功"));
                }
                else
                {
                    return Json(new AjaxResult(300, "失败"));
                }
            }
        }

        /// <summary>
        /// 设置默认收货地址
        /// </summary>
        [HttpPost]
        public JsonResult AddressSet(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                Guid MGid = LCookie.GetMemberGid();
                db.Address.Where(l=>l.MemberGid==MGid).Update(l => new Address { Default = 1 });
                if (db.Address.Where(l => l.Gid == Gid).Update(l => new Address { Default = 2 }) == 1)
                {
                    return Json(new AjaxResult("成功"));
                }
                else
                {
                    return Json(new AjaxResult(300, "失败"));
                }
            }
        }
        #endregion

        #region 个人中心
        /// <summary>
        /// 个人中心
        /// </summary>
        /// <param name="Gid">会员帐号</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Index()
        {
            using (EFDB db = new EFDB())
            {
                ViewBag.Project = LCookie.Project();
                Guid MemberGid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                ViewBag.Picture = b.Picture;
                ViewBag.Gender = b.Gender;
                ViewBag.RealName = b.RealName;
                ViewBag.Account = b.Account;
                ViewBag.MID = b.MID;
                ViewBag.StockRight = b.StockRight;
                ViewBag.Shop = db.Shop.Where(s => s.MemberGid == MemberGid).Count();
                ViewBag.Level = b.Level;
                //var lv = db.Level.Where(l => l.LV == b.Level).FirstOrDefault();
                //ViewBag.Label = lv.Label;
                //ViewBag.LevelName = lv.Name;
                //ViewBag.CLLevel = b.CLLevel;
                var cllv = db.Level.Where(l => l.LV == b.CLLevel).FirstOrDefault();
                ViewBag.CLLabel = cllv.Label;
                ViewBag.CLLevelName = cllv.Name;
                ViewBag.SellStock = cllv.SellStock;
                //进货价
                ViewBag.BuyPrice = b.BuyPrice;
                //库存
                Guid ProductGid = Helper.GetProductGid();
                var ms = db.Stock.Where(l => l.MemberGid == MemberGid && l.ProductGid == ProductGid).FirstOrDefault();
                ViewBag.Stock = ms == null ? 0 : ms.Number;
                //未付款库存
                ViewBag.LockStock = db.Order.Where(l => l.ShopGid == MemberGid && l.PayStatus == 2).Select(l => new
                {
                    Number = db.OrderDetails.Where(od => od.OrderGid == l.Gid).Select(od => od.Number).DefaultIfEmpty(0).Sum()
                }).Select(l => l.Number).DefaultIfEmpty(0).Sum();
                //积分
                ViewBag.Money = b.Money;
                ViewBag.Integral = b.Integral;
                ViewBag.ShopIntegral = b.ShopIntegral;
                ViewBag.MIntegral = b.MIntegral;
                ViewBag.TIntegral = b.TIntegral;
                //商城查询
                var so = db.ShopOrder.Where(l => l.PayStatus == 1).ToList();
                //待处理订单
                Guid ShopGid = LCookie.GetShopGid();
                ViewBag.SOC = so.Where(l => l.ShopGid == ShopGid && l.Status==1).Count();
                //商城消费金额
                ViewBag.ShopOrder = so.Where(l => l.MemberGid == MemberGid).Select(l => l.TotalPrice).DefaultIfEmpty(0).Sum();
                //商城货款
                ViewBag.ShopMoney = b.ShopMoney;
                //冻结积分
                ViewBag.ShopRecord = db.ShopRecord.Where(l => l.MemberGid == MemberGid && l.Type == 2 && l.State == 2).Select(l => l.TIntegral).DefaultIfEmpty(0).Sum();
                //倍数积分
                ViewBag.MM = cllv.Multiple * b.MIntegral;
                //团队业绩
                int Year = DateTime.Now.Year;
                int Month = DateTime.Now.Month;
                ViewBag.TMoney = db.Achievement.Where(l => l.Year == Year && l.Month == Month && l.MemberGid == MemberGid).Select(l => l.TMoney).DefaultIfEmpty(0).Sum();
                //APP数据
                ViewBag.BCCB = AppApi.MB(b.Account, "BCCB");
                ViewBag.FBCC = AppApi.MB(b.Account, "FBCC");
            }
            return View();
        }

        /// <summary>
        /// 个人信息
        /// </summary>
        /// <param name="Type">1=提交数据</param>
        /// <param name="RealName">真实姓名</param>
        /// <param name="NickName">昵称</param>
        /// <param name="Gender">性别</param>
        /// <param name="Picture">头像</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Info(string Type, string RealName, string NickName, string Gender, string base64Data)
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                if (Type != "1")
                {
                    ViewBag.Picture = b.Picture;
                    ViewBag.RealName = b.RealName;
                    ViewBag.NickName = b.NickName;
                    ViewBag.Gender = b.Gender;
                    return View();
                }
                else
                {
                    b.RealName = RealName;
                    b.NickName = NickName;
                    b.Gender = Gender;
                    //接收图片
                    string picture = Helper.jsimg(Help.Member, base64Data);
                    if (!string.IsNullOrEmpty(picture))
                    {
                        b.Picture = picture;
                    }
                    //var Picture = Request.Files["Picture"];
                    //if (Picture != null)
                    //{
                    //    b.Picture = Helper.UploadFiles("/uploadfiles/member/", Picture);
                    //}
                    if (db.SaveChanges() == 1)
                    {
                        Helper.MLogin(Gid);
                        return Helper.Redirect("成功", "history.go(-2);", "修改成功");
                    }
                    else
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "失败,请重试");
                    }
                }
            }
        }

        /// <summary>
        /// 推荐人
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Referee()
        {
            Guid Gid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                ViewBag.Member = db.Member.Where(l => l.Gid == Gid).GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        Member = l.MemberGid == null ? "" : j.FirstOrDefault().Account
                    }).FirstOrDefault().Member;
            }
            return View();
        }

        /// <summary>
        /// 银行卡
        /// </summary>
        /// <param name="BankName">开户人</param>
        /// <param name="BankNumber">卡号</param>
        /// <param name="Bank">开户行</param>
        /// <param name="PayPWD">支付密码</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult AddBank(string BankName, string BankNumber, string Bank, string PayPWD,int type=0)
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                if (string.IsNullOrEmpty(BankName) || string.IsNullOrEmpty(BankNumber) || string.IsNullOrEmpty(Bank) || string.IsNullOrEmpty(PayPWD))
                {
                    ViewBag.BankName = b.BankName;
                    ViewBag.BankNumber = b.BankNumber;
                    ViewBag.Bank = b.Bank;
                    return View();
                }
                else
                {
                    PayPWD = MD5.GetMD5ljsheng(PayPWD);
                    if (b != null && b.PayPWD == PayPWD)
                    {
                        b.BankName = BankName;
                        b.BankNumber = BankNumber;
                        b.Bank = Bank;
                        if (db.SaveChanges() == 1)
                        {
                            if (type==1)
                            {
                                return new RedirectResult("/Shop/RMB");
                            }
                            else if(type==2)
                            {
                                return new RedirectResult("/Member/RMB");
                            }
                            else
                            {
                                return Helper.Redirect("成功", "history.go(-1);", "修改成功");
                            }
                        }
                        else
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "提交失败,请重试");
                        }
                    }
                    else
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "支付密码不正确");
                    }
                }
            }
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="oldpwd">旧密码</param>
        /// <param name="pwd">新密码</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult PWD(string oldpwd, string pwd)
        {
            if (string.IsNullOrEmpty(oldpwd) || string.IsNullOrEmpty(pwd))
            {
                return View();
            }
            else
            {
                using (EFDB db = new EFDB())
                {
                    oldpwd = MD5.GetMD5ljsheng(oldpwd);
                    pwd = MD5.GetMD5ljsheng(pwd);
                    Guid Gid = LCookie.GetMemberGid();
                    var b = db.Member.Where(l => l.Gid == Gid && l.PWD == oldpwd).FirstOrDefault();
                    if (b != null)
                    {
                        b.PWD = pwd;
                        b.LoginIdentifier = "";
                        if (db.SaveChanges() == 1)
                        {
                            AppApi.PWD(b.Account, pwd, "2");
                            return Helper.Redirect("成功", "/Home/Login", "修改成功,请用新密码重新登录");
                        }
                        else
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "失败,请重试");
                        }
                    }
                    else
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "旧密码不正确");
                    }
                }
            }
        }

        /// <summary>
        /// 修改支付密码
        /// </summary>
        /// <param name="oldpwd">旧密码</param>
        /// <param name="pwd">新密码</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult PayPWD(string oldpaypwd, string paypwd)
        {
            if (string.IsNullOrEmpty(oldpaypwd) || string.IsNullOrEmpty(paypwd))
            {
                return View();
            }
            else
            {
                using (EFDB db = new EFDB())
                {
                    oldpaypwd = MD5.GetMD5ljsheng(oldpaypwd);
                    paypwd = MD5.GetMD5ljsheng(paypwd);
                    Guid Gid = LCookie.GetMemberGid();
                    var b = db.Member.Where(l => l.Gid == Gid && l.PayPWD == oldpaypwd).FirstOrDefault();
                    if (b != null)
                    {
                        b.PayPWD = paypwd;
                        if (db.SaveChanges() == 1)
                        {
                            AppApi.PWD(b.Account, paypwd, "3");
                            return Helper.Redirect("成功", "/Home/Index", "修改成功!");
                        }
                        else
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "失败,请重试");
                        }
                    }
                    else
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "旧支付密码不正确");
                    }
                }
            }
        }
        #endregion

        #region 订单中心
        /// <summary>
        /// 我的订单
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Order()
        {
            return View();
        }

        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="ExpressStatus">快递状态</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult OrderData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            int ExpressStatus = int.Parse(paramJson["ExpressStatus"].ToString());
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.Order.Where(l => l.MemberGid == MemberGid && l.PayStatus < 3).AsQueryable();
                if (ExpressStatus != 0)
                {
                    if (ExpressStatus == 4)
                    {
                        b = b.Where(l => l.PayStatus == 2 && l.PayType == 3);
                    }
                    else
                    {
                        b = b.Where(l => l.PayStatus == 1 && l.ExpressStatus == ExpressStatus);
                    }
                }
                else
                {
                    b = b.Where(l => l.PayStatus == 1);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).Select(l => new
                    {
                        l.Gid,
                        l.Product,
                        l.ExpressStatus,
                        l.TotalPrice,
                        l.OrderNo,
                        l.TradeNo,
                        l.RealName,
                        l.ContactNumber,
                        l.Address,
                        l.Voucher,
                        Number = db.OrderDetails.Where(od => od.OrderGid == l.Gid).Sum(od => od.Number),
                        list = db.OrderDetails.Where(o => o.OrderGid == l.Gid).GroupJoin(db.Product,
                                x => x.ProductGid,
                                j => j.Gid,
                                (x, j) => new
                                {
                                    x.ProductGid,
                                    x.Number,
                                    x.Price,
                                    j.FirstOrDefault().Name,
                                    j.FirstOrDefault().Picture
                                })
                    }).ToList()
                }));
            }
        }

        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="OrderGid">订单GID</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult OrderDetail(Guid OrderGid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Order.Where(l => l.Gid == OrderGid).FirstOrDefault();
                ViewBag.RealName = b.RealName;
                ViewBag.ContactNumber = b.ContactNumber;
                ViewBag.Address = b.Address;
                ViewBag.AddTime = b.AddTime;
                ViewBag.PayTime = b.PayTime;
                ViewBag.TotalPrice = b.TotalPrice;
                ViewBag.OrderNo = b.OrderNo;
                ViewBag.TradeNo = b.TradeNo;
                ViewBag.Remarks = b.Remarks;
                ViewBag.Express = b.Express;
                ViewBag.ExpressNumber = b.ExpressNumber;
                if (b.RobGid != null)
                {
                    ViewBag.Status = b.ExpressStatus;
                    if (string.IsNullOrEmpty(b.ExpressNumber))
                    {
                        //抢单成功.待发货1
                        ViewBag.btName = "发货";
                    }
                    else
                    {
                        //抢单成功.已发货2
                        ViewBag.btName = "查看";
                    }
                }
                else
                {
                    ViewBag.Status = 0;
                    ViewBag.btName = "抢单";
                }
                ViewBag.ExpressList = db.Express.Where(l => l.Show == 1).OrderBy(l => l.Sort).ToList();
                return View(db.OrderDetails.Where(l => l.OrderGid == OrderGid).ToList());
            }
        }
        #endregion

        #region 合伙人后台
        /// <summary>
        /// 抢单
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult GetOrder()
        {
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                if (db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault().Level >= 6)
                {
                    if (string.IsNullOrEmpty(LCookie.GetOrderCity()))
                    {
                        return Helper.Redirect("失败", "/Member/AddAddress?type=1", "请设置你的所属地址,才可以抢单");
                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    return Helper.Redirect("失败", "/Member/Index", "权限不足");
                }
            }
        }

        [HttpPost]
        public ActionResult GetOrderData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            //1=查询当前订单城市 0=全部
            int type = int.Parse(paramJson["type"].ToString());
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                int Project = LCookie.Project();
                var b = db.Order.Where(l => l.PayStatus == 1 && l.RobGid == null && l.Type == 1 && l.Project == Project && l.Status == 1 && l.ExpressStatus == 1).AsQueryable();
                if (type == 1)
                {
                    string city = LCookie.GetOrderCity();
                    b = b.Where(l => l.Address.Contains(city));
                }
                else
                {
                    DateTime dt = DateTime.Now.AddDays(-1);
                    b = b.Where(l => l.AddTime <= dt);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).Select(l => new
                    {
                        l.Gid,
                        l.Product,
                        l.ExpressStatus,
                        l.TotalPrice,
                        l.OrderNo,
                        l.TradeNo,
                        l.RealName,
                        l.ContactNumber,
                        l.Address,
                        Number = db.OrderDetails.Where(od => od.OrderGid == l.Gid).Sum(od => od.Number)
                    }).ToList()
                }));
            }
        }

        /// <summary>
        /// 已抢订单
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult RobOrder()
        {
            return View();
        }

        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="ExpressStatus">快递状态</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult RobOrderData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            int ExpressStatus = int.Parse(paramJson["ExpressStatus"].ToString());
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.Order.Where(l => l.PayStatus == 1 && l.RobGid == MemberGid).AsQueryable();
                if (ExpressStatus != 0)
                {
                    b = b.Where(l => l.ExpressStatus == ExpressStatus);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).Select(l => new
                    {
                        l.Gid,
                        l.Product,
                        l.ExpressStatus,
                        l.TotalPrice,
                        l.OrderNo,
                        l.TradeNo,
                        l.RealName,
                        l.ContactNumber,
                        l.Address,
                        l.Status,
                        l.Remarks,
                        Number = db.OrderDetails.Where(od => od.OrderGid == l.Gid).Sum(od => od.Number),
                        list = db.OrderDetails.Where(o => o.OrderGid == l.Gid).GroupJoin(db.Product,
                                x => x.ProductGid,
                                j => j.Gid,
                                (x, j) => new
                                {
                                    x.ProductGid,
                                    x.Number,
                                    x.Price,
                                    j.FirstOrDefault().Name,
                                    j.FirstOrDefault().Picture
                                })
                    }).ToList()
                }));
            }
        }

        /// <summary>
        /// 库存管理
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Stock()
        {
            return View();
        }

        /// <summary>
        /// 库存管理
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult StockData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.Stock.Where(l => l.MemberGid == MemberGid).GroupJoin(db.Product,
                    l => l.ProductGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Number,
                        l.ProductGid,
                        j.FirstOrDefault().Name,
                        j.FirstOrDefault().Picture,
                        j.FirstOrDefault().Price
                    }).AsQueryable();
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderBy(l => l.Number).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 我的货款
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult PMoney()
        {
            return View();
        }

        [HttpPost]
        public JsonResult PMoneyData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            int Status = int.Parse(paramJson["Status"].ToString());
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.Order.Where(l => l.PayStatus == 1 && l.RobGid == MemberGid).AsQueryable();
                if (Status != 0)
                {
                    b = b.Where(l => l.Status == Status);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).Select(l => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Product,
                        l.ExpressStatus,
                        l.TotalPrice,
                        l.OrderNo,
                        l.TradeNo,
                        l.RealName,
                        l.ContactNumber,
                        l.Address,
                        Number = db.OrderDetails.Where(od => od.OrderGid == l.Gid).Sum(od => od.Number),
                        list = db.OrderDetails.Where(o => o.OrderGid == l.Gid).GroupJoin(db.Product,
                                x => x.ProductGid,
                                j => j.Gid,
                                (x, j) => new
                                {
                                    x.ProductGid,
                                    x.Number,
                                    x.Price,
                                    j.FirstOrDefault().Name,
                                    j.FirstOrDefault().Picture
                                })
                    }).ToList()
                }));
            }
        }

        /// <summary>
        /// 合伙人分红
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult StockRight()
        {
            return View();
        }

        [HttpPost]
        public JsonResult StockRightData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                Guid DGid = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault().Gid;
                var b = db.StockRight.Where(l => l.MemberGid == MemberGid).GroupJoin(db.Order,
                    l => l.OrderGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.AddTime,
                        l.Number,
                        Project = db.DictionariesList.Where(d => d.DGid == DGid && d.Value == SqlFunctions.StringConvert((double)l.Project).Trim()).FirstOrDefault().Key,
                        j.FirstOrDefault().Product,
                        j.FirstOrDefault().OrderNo,
                    }).AsQueryable();
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        #endregion

        #region 我要提现
        /// <summary>
        /// 提现管理
        /// </summary>
        /// <param name="BankName">开户人</param>
        /// <param name="BankNumber">卡号</param>
        /// <param name="Bank">开户行</param>
        /// <param name="Money">提现积分</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult RMB(string Money)
        {
            using (EFDB db = new EFDB())
            {
                Guid MemberGid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                ViewBag.Money = b.ProductMoney;
                if (string.IsNullOrEmpty(Money))
                {
                    if (string.IsNullOrEmpty(b.Bank) || string.IsNullOrEmpty(b.BankName) || string.IsNullOrEmpty(b.BankNumber))
                    {
                        ViewBag.Number = "请完善资料";
                    }
                    else
                    {
                        ViewBag.Bank = b.Bank;
                        ViewBag.BankName = b.BankName;
                        ViewBag.BankNumber = b.BankNumber;
                        ViewBag.Number = b.BankNumber.Substring(b.BankNumber.Length - 4, 4);
                    }
                    return View();
                }
                else
                {
                    decimal PM = decimal.Parse(Money);
                    if (PM < 100)
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "最少100起提");
                    }
                    else
                    {
                        if (PM > b.ProductMoney)
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "可提金额不足");
                        }
                        else
                        {
                            if (Helper.RMBRecordAdd(MemberGid, PM, 1))
                            {
                                var wd = new Withdrawals();
                                wd.Gid = Guid.NewGuid();
                                wd.AddTime = DateTime.Now;
                                wd.State = 1;
                                wd.Money = PM;
                                wd.Bank = b.Bank;
                                wd.BankName = b.BankName;
                                wd.BankNumber = b.BankNumber;
                                wd.MemberGid = MemberGid;
                                wd.MRGid = null;
                                db.Withdrawals.Add(wd);
                                if (db.SaveChanges() == 1)
                                {
                                    return Helper.Redirect("成功", "/Member/Withdrawals", "恭喜你,提现成功,等待财务审核后打款");
                                }
                                else
                                {
                                    LogManager.WriteLog("链商城扣除金额成功打款记录失败", "MemberGid=" + MemberGid.ToString() + ",Money=" + Money);
                                    return Helper.Redirect("失败", "history.go(-1);", "扣除金额成功提现记录失败,请联系客服人员!");
                                }
                            }
                            else
                            {
                                return Helper.Redirect("失败", "history.go(-1);", "扣除失败");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 提现管理
        /// </summary>
        /// <param name="BankName">开户人</param>
        /// <param name="BankNumber">卡号</param>
        /// <param name="Bank">开户行</param>
        /// <param name="Money">提现积分</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult Withdrawals(string Money)
        {
            using (EFDB db = new EFDB())
            {
                Guid MemberGid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                ViewBag.Money = b.ProductMoney;
                if (string.IsNullOrEmpty(Money))
                {
                    ViewBag.Bank = b.Bank;
                    ViewBag.BankName = b.BankName;
                    ViewBag.BankNumber = b.BankNumber;
                    return View();
                }
                else
                {
                    decimal PM = decimal.Parse(Money);
                    if (PM < 100)
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "最少100起提");
                    }
                    else
                    {
                        if (PM > b.ProductMoney)
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "可提金额不足");
                        }
                        else
                        {

                            if (Helper.RMBRecordAdd(MemberGid, PM, 1))
                            {
                                var wd = new Withdrawals();
                                wd.Gid = Guid.NewGuid();
                                wd.AddTime = DateTime.Now;
                                wd.State = 1;
                                wd.Money = PM;
                                wd.Bank = b.Bank;
                                wd.BankName = b.BankName;
                                wd.BankNumber = b.BankNumber;
                                wd.MemberGid = MemberGid;
                                wd.MRGid = null;
                                db.Withdrawals.Add(wd);
                                if (db.SaveChanges() == 1)
                                {
                                    return Helper.Redirect("成功", "/Member/Withdrawals", "恭喜你,提现成功,等待财务审核后打款");
                                }
                                else
                                {
                                    LogManager.WriteLog("扣除金额成功提现记录失败", "MemberGid=" + MemberGid.ToString() + ",Money=" + Money);
                                    return Helper.Redirect("失败", "history.go(-1);", "扣除金额成功提现记录失败,请联系客服人员!");
                                }
                            }
                            else
                            {
                                return Helper.Redirect("失败", "history.go(-1);", "扣除失败");
                            }
                        }
                    }
                }
            }
        }
        [HttpPost]
        public ActionResult WithdrawalsData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            int type = int.Parse(paramJson["type"].ToString());
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.Withdrawals.Where(l => l.MemberGid == MemberGid).AsQueryable();
                if (type != 0)
                {
                    b = b.Where(l => l.State == type);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        #endregion

        #region 分红
        /// <summary>
        /// 合伙人分红
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult Profit()
        {
            using (EFDB db = new EFDB())
            {
                Guid MemberGid = LCookie.GetMemberGid();
                var b = db.ReportList.Where(l => l.MemberGid == MemberGid && l.Type > 9).Select(l => new { l.Money, l.Integral });
                ViewBag.Money = b.Select(l => l.Money).DefaultIfEmpty(0m).Sum();
                ViewBag.Integral = b.Select(l => l.Integral).DefaultIfEmpty(0m).Sum();
                return View();
            }
        }
        [HttpPost]
        public JsonResult ProfitData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                Guid DGid = db.Dictionaries.Where(d => d.DictionaryType == "ClassifyType").FirstOrDefault().Gid;
                var b = db.ReportList.Where(l => l.MemberGid == MemberGid && l.Type > 9).GroupJoin(db.Report,
                    l => l.ReportGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.State,
                        l.Money,
                        l.Integral,
                        l.Type,
                        j.FirstOrDefault().Project,
                        j.FirstOrDefault().RTime
                    }).GroupJoin(db.Level,
                    l => l.Type,
                    j => j.LV,
                    (l, j) => new
                    {
                        l.State,
                        l.Money,
                        l.Integral,
                        l.RTime,
                        LevelName = j.FirstOrDefault().Name,
                        j.FirstOrDefault().Label,
                        ProjectName = db.DictionariesList.Where(dl => dl.DGid == DGid && dl.Value == SqlFunctions.StringConvert((double)l.Project).Trim()).FirstOrDefault().Key,
                    }).AsQueryable();
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.RTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 股东分红
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult Bonus()
        {
            using (EFDB db = new EFDB())
            {
                int Project = int.Parse(Request.QueryString["Project"]);
                Guid MemberGid = LCookie.GetMemberGid();
                var b = db.ReportList.Where(l => l.MemberGid == MemberGid && l.Type < 10).GroupJoin(db.Report,
                    l => l.ReportGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Money,
                        l.Integral,
                        j.FirstOrDefault().Project,
                    }).Where(l => l.Project == Project).Select(l => new { l.Money, l.Integral });
                ViewBag.Money = b.Select(l => l.Money).DefaultIfEmpty(0m).Sum();
                ViewBag.Integral = b.Select(l => l.Integral).DefaultIfEmpty(0m).Sum();
                return View();
            }
        }
        [HttpPost]
        public JsonResult BonusData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            int Project = int.Parse(paramJson["Project"].ToString());
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                Guid DGid = db.Dictionaries.Where(d => d.DictionaryType == "ClassifyType").FirstOrDefault().Gid;
                var b = db.ReportList.Where(l => l.MemberGid == MemberGid).GroupJoin(db.Report,
                    l => l.ReportGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.State,
                        l.Money,
                        l.Integral,
                        l.Type,
                        j.FirstOrDefault().Project,
                        j.FirstOrDefault().RTime
                    }).Where(l => l.Project == Project).GroupJoin(db.Level,
                    l => l.Type,
                    j => j.LV,
                    (l, j) => new
                    {
                        l.State,
                        l.Money,
                        l.Integral,
                        l.RTime,
                        l.Type,
                        LevelName = Project == 2 ? "创始人" : j.FirstOrDefault().Name,
                        j.FirstOrDefault().Label,
                        ProjectName = db.DictionariesList.Where(dl => dl.DGid == DGid && dl.Value == SqlFunctions.StringConvert((double)l.Project).Trim()).FirstOrDefault().Key,
                    }).AsQueryable();
                if (Project == 2)
                {
                    b = b.Where(l => l.Type == 26).AsQueryable();
                }
                else
                {
                    b = b.Where(l => l.Type < 10).AsQueryable();
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.RTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        #endregion

        #region 我的团队
        /// <summary>
        /// 我的团队
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult Team()
        {
            using (EFDB db = new EFDB())
            {
                Guid gid = LCookie.GetMemberGid();
                ViewBag.Level1 = db.MRelation.Where(l => l.M1 == gid).Count();
                var b = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                ViewBag.Level21 = db.Member.Where(l => l.MemberGid == gid && l.CLLevel == 21).Count();
                ViewBag.Level22 = db.Member.Where(l => l.MemberGid == gid && l.CLLevel == 22).Count();
                ViewBag.Level23 = db.Member.Where(l => l.MemberGid == gid && l.CLLevel == 23).Count();
                ViewBag.Level24 = db.Member.Where(l => l.MemberGid == gid && l.CLLevel == 24).Count();
                ViewBag.Level25 = db.Member.Where(l => l.MemberGid == gid && l.CLLevel == 25).Count();
                ViewBag.Level26 = db.Member.Where(l => l.MemberGid == gid && l.CLLevel == 26).Count();
                return View();
            }
        }
        [HttpPost]
        public JsonResult TeamData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            Guid MemberGid = LCookie.GetMemberGid();
            if (!string.IsNullOrEmpty(paramJson["MemberGid"].ToString()))
            {
                MemberGid = Guid.Parse(paramJson["MemberGid"].ToString());
            }
            int type = int.Parse(paramJson["type"].ToString());
            //统计月份的第一天和最后一天
            //int Year = DateTime.Now.Year;
            //int Month = DateTime.Now.Month;
            using (EFDB db = new EFDB())
            {
                var b = db.Member.Where(l => l.MemberGid == MemberGid).GroupJoin(db.Level,
                    l => l.CLLevel,
                    j => j.LV,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        Account = l.Account.Substring(0, 3) + "****" +l.Account.Substring(7,4),
                        l.Picture,
                        l.RealName,
                        l.Gender,
                        l.CLLevel,
                        j.FirstOrDefault().Label,
                        LevelName = j.FirstOrDefault().Name,
                        Number = db.Member.Where(m=>m.MemberGid==l.Gid).Count()
                        //Money = db.Achievement.Where(a => a.Year == Year && a.Month == Month && l.MemberGid == l.Gid).Select(a => a.TMoney).DefaultIfEmpty(0).Sum()
                    }).AsQueryable();
                if (type != 0)
                {
                    b = b.Where(l => l.CLLevel == type);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        #endregion

        #region 我的积分
        /// <summary>
        /// 我的积分
        /// </summary>
        /// <param name="type">1=我的积分 2=购物积分</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult Integral()
        {
            using (EFDB db = new EFDB())
            {
                int tab = int.Parse(Request.QueryString["tab"]);
                Guid gid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                ViewBag.Integral = tab == 1 ? "我的积分(" + b.Money + ")" : "购物积分(" + b.Integral + ")";
                var mr = db.MoneyRecord.Where(l => l.MemberGid == gid);
                ViewBag.Money = mr.Select(l => tab == 1 ? l.Money : l.Integral).DefaultIfEmpty(0m).Sum();
                ViewBag.Money1 = mr.Where(l => l.Type == 1).Select(l => tab == 1 ? l.Money : l.Integral).DefaultIfEmpty(0m).Sum();
                ViewBag.Money2 = mr.Where(l => l.Type == 2).Select(l => tab == 1 ? l.Money : l.Integral).DefaultIfEmpty(0m).Sum();
                ViewBag.Money9 = mr.Where(l => l.Type == 9).Select(l => tab == 1 ? l.Money : l.Integral).DefaultIfEmpty(0m).Sum();
                ViewBag.Money20 = mr.Where(l => l.Type == 20).Select(l => tab == 1 ? l.Money : l.Integral).DefaultIfEmpty(0m).Sum();
                ViewBag.Money22 = mr.Where(l => l.Type == 22).Select(l => tab == 1 ? l.Money : l.Integral).DefaultIfEmpty(0m).Sum();
                ViewBag.Money23 = mr.Where(l => l.Type == 23).Select(l => tab == 1 ? l.Money : l.Integral).DefaultIfEmpty(0m).Sum();
                ViewBag.Money24 = mr.Where(l => l.Type == 24).Select(l => tab == 1 ? l.Money : l.Integral).DefaultIfEmpty(0m).Sum();
                return View();
            }
        }

        [HttpPost]
        public JsonResult IntegralData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            int type = int.Parse(paramJson["type"].ToString());
            int tab = int.Parse(paramJson["tab"].ToString());
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.MoneyRecord.Where(l => l.MemberGid == MemberGid && l.OrderGid != null).GroupJoin(db.Order,
                    l => l.OrderGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        Integral = tab == 1 ? l.Money : l.Integral,
                        l.AddTime,
                        l.Type,
                        j.FirstOrDefault().Product,
                        j.FirstOrDefault().MemberGid
                    }).GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.Integral,
                        l.AddTime,
                        l.Type,
                        l.Product,
                        j.FirstOrDefault().Account,
                        j.FirstOrDefault().Picture,
                        j.FirstOrDefault().RealName,
                        j.FirstOrDefault().Gender
                    }).AsQueryable();
                if (type != 0)
                {
                    b = b.Where(l => l.Type == type);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        #endregion

        #region 商家
        /// <summary>
        /// 商家申请入驻
        /// </summary>
        [HttpPost]
        public JsonResult OpenShop()
        {
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                if (db.Shop.Where(s => s.MemberGid == MemberGid).FirstOrDefault() == null)
                {
                    var b = new Shop();
                    b.Gid = Guid.NewGuid();
                    b.AddTime = DateTime.Now;
                    b.MemberGid = MemberGid;
                    b.Name = "请完善商家信息";
                    b.Sort = 1;
                    b.Show = 1;
                    b.Money = 0;
                    b.Project = 1;
                    b.Number = 0;
                    b.State = 1;
                    b.Borrow = 0;
                    b.Consignment = 0;
                    db.Shop.Add(b);
                    if (db.SaveChanges() == 1)
                    {
                        return Json(new AjaxResult("申请成功,请上传商家证件等待审核!"));
                    }
                }
                return Json(new AjaxResult(300, "申请失败!"));
            }
        }

        /// <summary>
        /// 我的订单
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult ShopOrder()
        {
            if (Request.QueryString["ReturnType"] == "1" && !string.IsNullOrEmpty(Request.QueryString["OrderGid"]))
            {
                using (EFDB db = new EFDB())
                {
                    Guid OrderGid = Guid.Parse(Request.QueryString["OrderGid"]);
                    var b = db.ShopOrder.Where(l => l.Gid == OrderGid).FirstOrDefault();
                    b.ReturnType = 2;
                    //归还地址
                    var s = db.Shop.Where(l => l.Gid == b.ShopGid).FirstOrDefault();
                    b.RName = s.Name;
                    b.RAddress = s.Address;
                    b.RName = s.RealName;
                    b.RContactNumber = s.ContactNumber;
                    db.SaveChanges();
                }
            }
            return View();
        }

        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="ExpressStatus">快递状态</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult ShopOrderData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            int ExpressStatus = int.Parse(paramJson["ExpressStatus"].ToString());
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.ShopOrder.Where(l => l.MemberGid == MemberGid && l.PayStatus < 3).GroupJoin(db.Member,
                    x => x.MemberGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        x.Gid,
                        x.AddTime,
                        x.ExpressStatus,
                        x.Status,
                        x.Product,
                        x.PayType,
                        x.TotalPrice,
                        x.OrderNo,
                        x.ConsumptionCode,
                        x.Voucher,
                        x.PayStatus,
                        x.ShopGid,
                        x.ReturnType,
                        x.RExpress,
                        x.RExpressNumber,
                        y.FirstOrDefault().RealName,
                        y.FirstOrDefault().Account
                    }).AsQueryable();
                int ReturnType = int.Parse(paramJson["ReturnType"].ToString());
                if (ReturnType == 0)
                {
                    b = b.Where(l => l.ReturnType == 0);
                    if (ExpressStatus != 0)
                    {
                        if (ExpressStatus == 4)
                        {
                            b = b.Where(l => l.PayStatus == 2 && l.PayType == 3);
                        }
                        else
                        {
                            b = b.Where(l => l.PayStatus == 1 && l.ExpressStatus == ExpressStatus);
                        }
                    }
                    else
                    {
                        b = b.Where(l => l.PayStatus == 1);
                    }
                }
                else
                {
                    b = b.Where(l => l.ReturnType != 0);
                    if (ExpressStatus == 0)
                    {
                        b = b.Where(l => l.PayStatus == 1);
                    }
                    else if (ExpressStatus == 1)
                    {
                        b = b.Where(l => l.PayStatus == 1 && l.ExpressStatus == 1);
                    }
                    else if (ExpressStatus == 2)
                    {
                        b = b.Where(l => l.PayStatus == 1 && l.ReturnType != 0);
                    }
                    else if (ExpressStatus == 3)
                    {
                        b = b.Where(l => l.PayStatus == 1 && (l.ReturnType == 2 || (l.ReturnType == 1 && l.RExpressNumber!="")));
                    }
                    else if (ExpressStatus == 4)
                    {
                        b = b.Where(l => l.PayStatus == 3 && l.ReturnType != 0);
                    }
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="OrderGid">订单GID</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult ShopOrderDetail(Guid OrderGid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.ShopOrder.Where(l => l.Gid == OrderGid).FirstOrDefault();
                ViewBag.Gid = b.Gid;
                ViewBag.Status = b.Status;
                ViewBag.RealName = b.RealName;
                ViewBag.ContactNumber = b.ContactNumber;
                ViewBag.Address = b.Address;
                ViewBag.AddTime = b.AddTime;
                ViewBag.TotalPrice = b.TotalPrice;
                ViewBag.OrderNo = b.OrderNo;
                ViewBag.Remarks = b.Remarks;
                ViewBag.Express = b.Express;
                ViewBag.ExpressNumber = b.ExpressNumber;
                ViewBag.ReturnType = b.ReturnType;
                ViewBag.RName = b.RName;
                ViewBag.RContactNumber = b.RContactNumber;
                ViewBag.RAddress = b.RAddress;
                ViewBag.ExpressList = db.Express.Where(l => l.Show == 1).OrderBy(l => l.Sort).ToList();
                return View(db.OrderDetails.Where(l => l.OrderGid == OrderGid).ToList());
            }
        }

        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="ExpressStatus">快递状态</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult ShopOrderOK(Guid Gid)
        {
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.ShopOrder.Where(l => l.Gid == Gid && l.MemberGid== MemberGid && l.Status == 1).FirstOrDefault();
                if (b != null && b.PayStatus == 1)
                {
                    b.Status = 2;
                    b.ExpressStatus = 3;
                    if (db.SaveChanges() == 1)
                    {
                        Guid ShopGid = db.Shop.Where(l => l.Gid == b.ShopGid).FirstOrDefault().MemberGid;
                        if (db.Member.Where(l => l.Gid == ShopGid).Update(l => new Member { ShopMoney = l.ShopMoney + b.PayPrice }) == 1)
                        {
                            return Json(new AjaxResult("确认成功"));
                        }
                        else
                        {
                            LogManager.WriteLog("确认订单成功货款失败", "商家=" + ShopGid + ",订单=" + Gid+ ",操作会员="+ MemberGid +", PayPrice=" + b.PayPrice);
                        }
                    }
                }
            }
            return Json(new AjaxResult(300, "未付款不能确认订单"));
        } 
        #endregion

        #region 商品转让
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult ProductAU()
        {
            using (EFDB db = new EFDB())
            {
                //if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
                //{
                //    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                //    var b = db.MProduct.Where(l => l.Gid == Gid).FirstOrDefault();
                //    ViewBag.Name = b.Name;
                //    ViewBag.Price = b.Price;
                //    ViewBag.OriginalPrice = b.OriginalPrice;
                //    ViewBag.Number = b.Number;
                //    ViewBag.OldStock = ViewBag.Stock = b.Stock;
                //    ViewBag.Profile = b.Profile;
                //    ViewBag.Content = b.Content;
                //    ViewBag.Sort = b.Sort;
                //    ViewBag.Show = b.Show;
                //    ViewBag.Picture = Help.Product + b.Picture;
                //    //ViewBag.GraphicDetails = b.GraphicDetails;
                //    ViewBag.Remarks = b.Remarks;
                //    ViewBag.ExpressFee = b.ExpressFee;
                //    ViewBag.Company = b.Company;
                //}
                //else
                //{
                //    ViewBag.OriginalPrice = 0;
                //    ViewBag.Number = 0;
                //    ViewBag.Sort = 1;
                //    ViewBag.Show = 1;
                //    //查询用户的库存
                //    Guid MemberGid = LCookie.GetMemberGid();
                //    ViewBag.Stock = Helper.GetCLStock(MemberGid);
                //    ViewBag.OldStock = 0;
                //}
                //查询用户的库存
                ViewBag.Stock = Helper.GetCLStock(LCookie.GetMemberGid());
                return View();
            }
        }
        [HttpPost]
        public ActionResult ProductAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                Guid MemberGid = LCookie.GetMemberGid();
                //查询用户的进货价
                decimal BuyPrice = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault().BuyPrice;
                int Number = int.Parse(Request.Form["Number"]);
                if (BuyPrice > 0 && Number > 0 && Helper.CLOrder(5, Number, "", MemberGid, 2, 1) != null)
                {
                    return Helper.Redirect("操作成功！", "/Member/Product?Type=5", "操作成功!");
                }
                else
                {
                    return Helper.Redirect("操作失败！", "history.go(-1);", "你当前可出售的库存不足!");
                }
            }
        }

        // 列表管理
        [ValidateInput(false)]
        public ActionResult Product()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ProductData()
        {
            //查询的参数json
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            //解析参数
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            using (EFDB db = new EFDB())
            {
                Guid MemberGid = LCookie.GetMemberGid();
                int Type = Int32.Parse(paramJson["Type"].ToString());
                var b = db.Order.Where(l => l.ShopGid == MemberGid && l.Type== Type).GroupJoin(db.OrderDetails,
                    x => x.Gid,
                    y => y.OrderGid,
                    (x, y) => new
                    {
                        x.Gid,
                        x.AddTime,
                        x.Money,
                        x.Integral,
                        x.Price,
                        x.ShopGid,
                        State = x.PayStatus==2? x.MemberGid == x.ShopGid ? 2 : 3 : x.PayStatus,
                        y.FirstOrDefault().Number,
                        MMoney = y.FirstOrDefault().Money,
                        MIntegral = y.FirstOrDefault().Integral
                    }).AsQueryable();
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.State).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        /// <summary>
        /// 赎回转让订单
        /// </summary>
        [HttpPost]
        public JsonResult RedeemOrder(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    Guid MemberGid = LCookie.GetMemberGid();
                    if (db.Order.Where(l => l.Gid == Gid && l.Type == 5 && l.PayStatus == 2 && l.ShopGid == MemberGid && l.MemberGid == MemberGid).Update(l => new Order { PayStatus = 4 }) == 1)
                    {
                        var od = db.OrderDetails.Where(l => l.OrderGid == Gid).FirstOrDefault();
                        //转让产品赎回库存
                        Guid ProductGid = Helper.GetProductGid();
                        if (db.Stock.Where(l=>l.MemberGid==MemberGid && l.ProductGid== ProductGid).Update(l => new Stock { Number = l.Number+od.Number })==1)
                        {
                            return Json(new AjaxResult("库存赎回成功"));
                        }
                        else
                        {
                            LogManager.WriteLog("转让商品订单状态关闭成功库存赎回失败", "订单=" + Gid.ToString() + "库存=" + od.Number.ToString());
                            return Json(new AjaxResult(300, "赎回失败,请联系客服!"));
                        }
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "赎回失败,订单被锁定,请等待24小时解锁后再操作!"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 库存提醒
        /// <summary>
        /// 库存提醒
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult MProduct()
        {
            return View();
        }
        [HttpPost]
        public ActionResult MProductData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.MProduct.Where(l => l.ShopGid == MemberGid).GroupJoin(db.Member,
                    x => x.MemberGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        x.Gid,
                        x.AddTime,
                        x.Price,
                        x.Stock,
                        x.Money,
                        x.Integral,
                        y.FirstOrDefault().Account,
                        y.FirstOrDefault().RealName
                    }).AsQueryable();
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult MProductDelete(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                if (db.MProduct.Where(l => l.Gid == Gid).Delete() == 1)
                {
                    return Json(new AjaxResult("成功"));
                }
                else
                {
                    return Json(new AjaxResult(300, "失败"));
                }
            }
        }
        #endregion

        #region 积分兑换
        /// <summary>
        /// 积分兑换管理
        /// </summary>
        /// <param name="Integral">兑换积分</param>
        /// <param name="TB">兑换币种[1=BCCB, 2=FBCC]</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult ToAPP(decimal Integral = 0, int TB = 0)
        {
            using (EFDB db = new EFDB())
            {
                Guid gid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                ViewBag.Integral = b.Money;
                //获取兑换比例积分
                decimal avg = AppApi.AVG(TB);
                ViewBag.FBCC = avg;
                ////查询兑换比例
                //ViewBag.BCCB = 1 / AppApi.AVG(1);
                //ViewBag.FBCC = 1 / AppApi.AVG(2);
                if (Integral <= 0 || TB <= 0)
                {
                    return View();
                }
                else
                {
                    string LogMsg = "gid=" + gid.ToString() + ",Integral=" + Integral.ToString() + ",TB=" + TB;
                    if (Integral > ViewBag.Integral)
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "可提积分不足");
                    }
                    else
                    {
                        if (avg > 0)
                        {
                            decimal Token = Integral * (1 / avg);
                            LogMsg += ",Token=" + Token;
                            Guid TRGid = Guid.NewGuid();
                            if (Helper.TokenRecordAdd(TRGid, gid, Integral, Token, 1, TB))
                            {
                                //提交到APP
                                if (AppApi.AddMB(b.Account, TB == 1 ? "BCCB" : "FBCC", Token.ToString()))
                                {
                                    return Helper.Redirect("成功", "/Member/IntegralAPP", "恭喜你,兑换成功!");
                                }
                                else
                                {
                                    LogManager.WriteLog("链商城积分兑换记录成功兑换APP钱包失败", LogMsg);
                                    string Remarks = "钱包兑换接口失败";
                                    //撤销兑换的积分
                                    //b.Money = b.Money + Integral;
                                    //if (db.SaveChanges() == 1)
                                    //{
                                    //    Remarks = "已退回账户";
                                    //}
                                    db.TokenRecord.Where(l => l.Gid == TRGid).Update(l => new TokenRecord { Remarks = Remarks, State = 2 });
                                    return Helper.Redirect("失败", "/Member/IntegralAPP", "链商城积分兑换记录成功兑换APP钱包失败");
                                }
                            }
                            else
                            {
                                LogManager.WriteLog("链商城积分兑换记录失败", LogMsg);
                                return Helper.Redirect("失败", "history.go(-1);", "积分兑换记录失败");
                            }
                        }
                        else
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "APP返回均价失败");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 积分兑换管理
        /// </summary>
        /// <param name="Integral">兑换积分</param>
        /// <param name="TB">兑换币种[1=BCCB, 2=FBCC]</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult IntegralAPP(decimal Integral=0, int TB=0)
        {
            using (EFDB db = new EFDB())
            {
                Guid gid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                ViewBag.Integral = b.Money;
                ////查询兑换比例
                //ViewBag.BCCB = 1 / AppApi.AVG(1);
                //ViewBag.FBCC = 1 / AppApi.AVG(2);
                if (Integral <= 0 || TB <= 0)
                {
                    return View();
                }
                else
                {
                    string LogMsg = "gid=" + gid.ToString() + ",Integral=" + Integral.ToString() + ",TB=" + TB;
                    if (Integral > ViewBag.Integral)
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "可提积分不足");
                    }
                    else
                    {
                        //获取兑换比例积分
                        decimal avg = AppApi.AVG(TB);
                        if (avg > 0)
                        {
                            decimal Token = Integral * (1 / avg);
                            Guid TRGid = Guid.NewGuid();
                            if (Helper.TokenRecordAdd(TRGid, gid, Integral, Token, 1, TB))
                            {
                                //提交到APP
                                if (AppApi.AddMB(b.Account, TB == 1 ? "BCCB" : "FBCC", Token.ToString()))
                                {
                                    return Helper.Redirect("成功", "/Member/IntegralAPP", "恭喜你,兑换成功!");
                                }
                                else
                                {
                                    LogManager.WriteLog("链商城积分兑换记录成功兑换APP钱包失败", LogMsg);
                                    db.TokenRecord.Where(l => l.Gid == TRGid).Update(l => new TokenRecord { Remarks = "钱包兑换接口失败" });
                                    return Helper.Redirect("失败", "/Member/IntegralAPP", "链商城积分兑换记录成功兑换APP钱包失败");
                                }
                            }
                            else
                            {
                                LogManager.WriteLog("链商城积分兑换记录失败", LogMsg);
                                return Helper.Redirect("失败", "history.go(-1);", "积分兑换记录失败");
                            }
                        }
                        else
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "APP返回均价失败");
                        }
                    }
                }
            }
        }
        [HttpPost]
        public ActionResult IntegralAPPData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.TokenRecord.Where(l => l.MemberGid == MemberGid && l.Type==1).AsQueryable();
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        #endregion

        #region 新页面
        /// <summary>
        /// 积分中心
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult MY()
        {
            using (EFDB db = new EFDB())
            {
                Guid MemberGid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                ViewBag.Picture = b.Picture;
                ViewBag.Gender = b.Gender;
                ViewBag.RealName = b.RealName;
                ViewBag.Account = b.Account;
                var cllv = db.Level.Where(l => l.LV == b.CLLevel).FirstOrDefault();
                ViewBag.CLLabel = cllv.Label;
                ViewBag.CLLevelName = cllv.Name;
                //积分
                ViewBag.Money = b.Money;
                ViewBag.Integral = b.Integral;
                ViewBag.ProductMoney = b.ProductMoney;
            }
            return View();
        }

        /// <summary>
        /// 商城积分中心
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult MyShop()
        {
            using (EFDB db = new EFDB())
            {
                ViewBag.Project = LCookie.Project();
                Guid MemberGid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                ViewBag.Picture = b.Picture;
                ViewBag.Gender = b.Gender;
                ViewBag.RealName = b.RealName;
                ViewBag.Account = b.Account;
                var cllv = db.Level.Where(l => l.LV == b.CLLevel).FirstOrDefault();
                ViewBag.CLLabel = cllv.Label;
                ViewBag.CLLevelName = cllv.Name;
                ViewBag.ShopIntegral = b.ShopIntegral;
                ViewBag.TIntegral = b.TIntegral;
                ViewBag.MIntegral = b.MIntegral;
                //商城查询
                var so = db.ShopOrder.Where(l => l.PayStatus == 1).ToList();
                //商城消费金额
                ViewBag.ShopOrder = so.Where(l => l.MemberGid == MemberGid).Select(l => l.TotalPrice).DefaultIfEmpty(0).Sum();
                //冻结积分
                ViewBag.ShopRecord = db.ShopRecord.Where(l => l.MemberGid == MemberGid && l.Type == 2 && l.State == 2).Select(l => l.TIntegral).DefaultIfEmpty(0).Sum();
                //倍数积分
                ViewBag.MM = cllv.Multiple * b.MIntegral;
            }
            return View();
        }

        /// <summary>
        /// 我的链商城
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult CL()
        {
            using (EFDB db = new EFDB())
            {
                Guid MemberGid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                ViewBag.CLLevel = b.CLLevel;
                //库存
                Guid ProductGid = Helper.GetProductGid();
                var ms = db.Stock.Where(l => l.MemberGid == MemberGid && l.ProductGid == ProductGid).FirstOrDefault();
                ViewBag.Stock = ms == null ? 0 : ms.Number;
            }
            return View();
        }

        /// <summary>
        /// 链商城转让
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult SendCL()
        {
            using (EFDB db = new EFDB())
            {
                Guid MemberGid = LCookie.GetMemberGid();
                Guid ProductGid = Helper.GetProductGid();
                var ms = db.Stock.Where(l => l.MemberGid == MemberGid && l.ProductGid == ProductGid).FirstOrDefault();
                ViewBag.Stock = ms == null ? 0 : ms.Number;
            }
            return View();
        }

        /// <summary>
        /// 链商城转让记录
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult SendList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult SendListData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            Guid MemberGid = LCookie.GetMemberGid();
            int type = int.Parse(paramJson["type"].ToString());
            using (EFDB db = new EFDB())
            {
                var b = db.Order.Where(l => l.PayStatus == 1 && l.PayType == 4 && l.TotalPrice == 0).Select(l => new
             
                    {
                        l.AddTime,
                        l.ShopGid,
                        l.MemberGid,
                        Number = db.OrderDetails.Where(od => od.OrderGid == l.Gid).Select(od => od.Number).DefaultIfEmpty(0).Sum(),
                        Account = type == 1?db.Member.Where(m=>m.Gid==l.ShopGid).FirstOrDefault().Account: db.Member.Where(m => m.Gid == l.MemberGid).FirstOrDefault().Account
                }).AsQueryable();
                if (type == 1)
                {
                    b = b.Where(l => l.ShopGid == MemberGid);
                }
                else
                {
                    b = b.Where(l => l.MemberGid == MemberGid);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        /// <summary>
        /// 成为商家
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult ToShop()
        {
            return View();
        }

        /// <summary>
        /// 寄快递
        /// </summary>
        public ActionResult UED()
        {
            //using (EFDB db = new EFDB())
            //{
            //    var b = db.Express.OrderBy(l=>l.Sort).ToList();
            //    System.Text.StringBuilder sb = new System.Text.StringBuilder("[");
            //    int n = 1;
            //    foreach (var dr in b)
            //    {
            //        sb.Append("{id: " + n.ToString() + ",value: \"" + dr.Name + "\"},");
            //        n++;
            //    }
            //    ViewBag.Express = sb.ToString().TrimEnd(',') + "]";
            //}
            return View();
        }
        /// <summary>
        /// 快递归还
        /// </summary>
        [HttpPost]
        public JsonResult UEDOK(Guid OrderGid,string expressName, string expressNum)
        {
            if (expressName.Length > 1 && expressNum.Length > 3)
            {
                using (EFDB db = new EFDB())
                {
                    var b = db.ShopOrder.Where(l => l.Gid == OrderGid).FirstOrDefault();
                    if (b.ReturnType != 0)
                    {
                        b.ReturnType = 1;
                        b.RExpress = expressName;
                        b.RExpressNumber = expressNum;
                        db.SaveChanges();
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "非借用订单"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "请输入正确的快递方式"));
            }
        }

        /// <summary>
        /// 借用协议
        /// </summary>
        public ActionResult Clause()
        {
            if (string.IsNullOrEmpty(Request.QueryString["PGid"]))
            {
                return Helper.Redirect("非法访问", "history.go(-1);", "请重新操作");
            }
            else
            {
                Guid MGid = LCookie.GetMemberGid();
                if (!string.IsNullOrEmpty(Request.QueryString["MGid"]))
                {
                    MGid = Guid.Parse(Request.QueryString["MGid"]);
                }
                using (EFDB db = new EFDB())
                {
                    var m = db.Member.Where(l => l.Gid == MGid).FirstOrDefault();
                    if (string.IsNullOrEmpty(m.Address) || string.IsNullOrEmpty(m.RealName) || string.IsNullOrEmpty(m.ContactNumber))
                    {
                        return Helper.Redirect("请完善个人资料", "/Member/AddAddress?type=1", "请完善个人资料");
                    }
                    else if (string.IsNullOrEmpty(m.Bank) || string.IsNullOrEmpty(m.BankName) || string.IsNullOrEmpty(m.BankNumber))
                    {
                        return Helper.Redirect("请完善提现信息", "/Member/AddBank", "请完善提现信息");
                    }
                    else
                    {
                        Guid PGid = Guid.Parse(Request.QueryString["PGid"]);
                        if (!string.IsNullOrEmpty(Request.QueryString["OrderGid"]))
                        {
                            Guid OrderGid = Guid.Parse(Request.QueryString["OrderGid"]);
                            var o = db.ShopOrder.Where(l => l.Gid == OrderGid).FirstOrDefault();
                            ViewBag.SDT = o.AddTime;
                            ViewBag.EDT = o.BorrowTime;
                        }
                        else
                        {
                            ViewBag.SDT = DateTime.Now;
                            ViewBag.EDT = DateTime.Now.AddMonths(3);
                        }
                        var p = db.ShopProduct.Where(l => l.Gid == PGid).FirstOrDefault();
                        var s = db.Shop.Where(l => l.Gid == p.ShopGid).FirstOrDefault();
                        ViewBag.Address = m.Address;
                        ViewBag.RealName = m.RealName;
                        ViewBag.ContactNumber = m.ContactNumber;
                        ViewBag.Bank = m.Bank;
                        ViewBag.BankName = m.BankName;
                        ViewBag.BankNumber = m.BankNumber;
                        ViewBag.PName = p.Name;
                        ViewBag.ShopGid = p.ShopGid;
                        ViewBag.Price = p.Price;
                        ViewBag.SName = s.Name;
                        ViewBag.SAddress = s.Address;
                        ViewBag.SRealName = s.RealName;
                        ViewBag.SContactNumber = s.ContactNumber;
                    }
                }
            }
            return View();
        }

        /// <summary>
        /// 产品归还
        /// </summary>
        public ActionResult PReturn()
        {
            return View();
        }
        #endregion
    }
}