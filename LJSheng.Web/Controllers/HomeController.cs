using LJSheng.Common;
using LJSheng.Data;
using System;
using System.Linq;
using System.Web.Mvc;
using EntityFramework.Extensions;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Collections;
using System.Collections.Generic;

namespace LJSheng.Web.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 项目跳转
        /// </summary>
        public new ActionResult Url()
        {
            string url = LCookie.Project() == 1 ? "Tea" : "Index";
            return new RedirectResult("/Home/" + url);
        }
        /// <summary>
        /// 线下汇款
        /// </summary>
        public ActionResult Bank(string Type, string OrderNo, string base64Data)
        { return View(); }
        /// <summary>
        /// 上传凭证
        /// </summary>
        public ActionResult Voucher(string Type, string OrderNo, string base64Data)
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = LCookie.GetMemberGid();
                string picture = Helper.jsimg(LJShengHelper.Voucher, base64Data);
                if (!string.IsNullOrEmpty(picture))
                {
                    if (Type == "2")
                    {
                        var b = db.Order.Where(l => l.MemberGid == Gid && l.OrderNo == OrderNo).FirstOrDefault();
                        b.Voucher = picture;
                        if (db.SaveChanges() == 1)
                        {
                            return Helper.Redirect("成功", "history.go(-1);", "成功");
                        }
                        else
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "失败,请重试");
                        }
                    }
                    else
                    {
                        var b = db.ShopOrder.Where(l => l.MemberGid == Gid && l.OrderNo == OrderNo).FirstOrDefault();
                        b.Voucher = picture;
                        if (db.SaveChanges() == 1)
                        {
                            return Helper.Redirect("成功", "history.go(-1);", "成功");
                        }
                        else
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "失败,请重试");
                        }
                    }
                }
                else
                { return Helper.Redirect("失败", "history.go(-1);", "图片上传失败"); }
            }
        }
        /// <summary>
        /// 页面操作提示
        /// </summary>
        public ActionResult PageMsg(string msg, string url, string title)
        {
            return View();
        }
        /// <summary>
        /// 退出登录
        /// </summary>
        public ActionResult LoginOut()
        {
            LCookie.DelCookie("linjiansheng");
            return new RedirectResult(Request.QueryString["login"]);
        }

        /// <summary>
        /// 分享
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Share()
        {
            string Account = Request.QueryString["m"];
            Guid Gid = Guid.Parse(Request.QueryString["gid"]);
            if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/member/") + Account + ".jpg"))
            {
                imgaddimg.Ctjr(Account, Gid);
            }
            //推广连接
            ViewBag.Account = Account + ".jpg";
            ViewBag.URL = Help.Url + "/Home/Register?m=" + Gid.ToString();
            ViewBag.MID = Request.QueryString["MID"];
            return View();
        }

        #region 会员模块
        /// <summary>
        /// 会员登录
        /// </summary>
        /// <param name="account">会员帐号</param>
        /// <param name="pwd">会员密码</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Login(string account, string pwd)
        {
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(pwd))
            {
                return View();
            }
            else
            {
                if (account.Length == 11 && account.Substring(0, 1) == "1" && pwd.Length > 5)
                {
                    using (EFDB db = new EFDB())
                    {
                        string pwdMD5 = MD5.GetMD5ljsheng(pwd);
                        var b = db.Member.Where(l => l.Account == account && l.PWD == pwdMD5).FirstOrDefault();
                        if (b != null)
                        {
                            //更新登录时间戳
                            b.LoginIdentifier = LCommon.TimeToUNIX(DateTime.Now);
                            db.SaveChanges();
                            Helper.MLogin(b.Gid);
                            string url = LCookie.Project() == 1 ? "Tea" : "Index";
                            return new RedirectResult("/Home/"+ url);
                        }
                        else
                        {
                            return Helper.Redirect("登录失败！", "history.go(-1);", "帐号或密码错误！");
                        }
                    }
                }
                else
                {
                    return Helper.Redirect("登录失败！", "history.go(-1);", "必须是11位的手机号,密码最少需要6位！");
                }
            }
        }

        /// <summary>
        /// 会员注册
        /// </summary>
        /// <param name="account">会员帐号</param>
        /// <param name="pwd">会员密码</param>
        /// <param name="paypwd">支付密码</param>
        /// <param name="identifyingCode">注册验证码</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Register(string account, string pwd, string paypwd, string identifyingCode)
        {
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(identifyingCode))
            {
                //是否有推荐人
                if (!string.IsNullOrEmpty(Request.QueryString["m"]))
                {
                    LCookie.AddCookie("m", Request.QueryString["m"], 1);
                }
                return View();
            }
            else
            {
                //判断是否有推荐人
                string m = LCookie.GetCookie("m");
                Guid? MemberGid = null;
                string ID = "";
                if (!string.IsNullOrEmpty(m))
                {
                    MemberGid = Guid.Parse(m);
                }
                using (EFDB db = new EFDB())
                {
                    if (account.Length == 11 && account.Substring(0, 1) == "1" && pwd.Length > 5 && paypwd.Length == 6)
                    {
                        var sms = db.SMS.Where(l => l.PhoneNumber == account && l.Content == identifyingCode).OrderByDescending(l => l.AddTime).FirstOrDefault();
                        if (sms != null)
                        {
                            TimeSpan ts = DateTime.Now - sms.AddTime;
                            if (identifyingCode == DateTime.Now.ToString("MMdd") || ts.TotalMinutes <= 10)
                            {
                                Guid Gid = Guid.NewGuid();
                                try
                                {
                                    int MID = Helper.CreateMNumber();//注册用户的邀请码
                                    var b = new Member();
                                    b.Gid = Gid;
                                    b.AddTime = DateTime.Now;
                                    b.Account = account;
                                    b.RealName = "请实名";
                                    b.LoginIdentifier = "0000000000";
                                    b.IP = Helper.IP;
                                    b.Money = 0;
                                    b.Integral = 0;
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
                                    b.PWD = MD5.GetMD5ljsheng(pwd);
                                    b.PayPWD = MD5.GetMD5ljsheng(paypwd);
                                    b.MID = MID;
                                    b.MemberGid = null;
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
                                    if (MemberGid != null)
                                    {
                                        b.MemberGid = MemberGid;
                                        ID = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault().MID.ToString();
                                    }
                                    b.APP = AppApi.AppMR(account, pwd, paypwd, account, MID.ToString()) ? 2 : 1;
                                    //b.Jurisdiction = Request.Form["Jurisdiction"];
                                    //b.Gender = Request.Form["Gender"];
                                    //b.NickName = Request.Form["NickName"];
                                    //b.RealName = b.RealName;
                                    //b.Gender = b.Gender;
                                    //b.ContactNumber = Request.Form["ContactNumber"];
                                    //b.Province = Request.Form["Province"];
                                    //b.City = Request.Form["City"];
                                    //b.Area = Request.Form["Area"];
                                    //b.Address = Request.Form["Address"];
                                    //b.Openid = b.Openid;
                                    //b.Money = decimal.Parse(Request.Form["Money"]);
                                    //b.Integral = int.Parse(Request.Form["Integral"]);
                                    //b.ProductMoney = decimal.Parse(Request.Form["ProductMoney"]);
                                    //b.StockRight = int.Parse(Request.Form["StockRight"]););
                                    //b.Bank = Request.Form["Bank"];
                                    //b.BankName = Request.Form["BankName"];
                                    //b.BankNumber = Request.Form["BankNumber"];
                                    //if (!string.IsNullOrEmpty(Picture))
                                    //{
                                    //    b.Picture = Picture;
                                    //}
                                    db.Member.Add(b);
                                    if (db.Member.Where(l => l.Account == account || l.MID == MID).Count() == 0 && db.SaveChanges() == 1)
                                    {
                                        //增加彩链发货人
                                        Helper.SetConsignor(b.Gid, b.MemberGid);
                                        //增加推荐人的人数
                                        List<Guid> list = new List<Guid>();
                                        if (b.MemberGid != null)
                                        {
                                            Helper.Member(Gid, b.MemberGid, 1, 3, list);
                                        }
                                        else
                                        {
                                            Helper.MRelation(Gid, list);
                                        }
                                        LCookie.DelCookie("linjiansheng");
                                        return Helper.Redirect("成功", "/Home/Login", "注册成功,请登录");
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
                                    return Helper.Redirect("失败", "history.go(-1);", "服务器请求超时");
                                }
                            }
                            else
                            {
                                return Helper.Redirect("失败", "history.go(-1);", "验证码已过期,请重新获取");
                            }
                        }
                        else
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "请先获取验证码");
                        }
                    }
                    else
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "必须是11位的手机号,密码最少需要6位");
                    }
                }
            }
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="account">会员帐号</param>
        /// <param name="pwd">会员密码</param>
        /// <param name="identifyingCode">注册验证码</param>
        /// <param name="type">密码类型 2-登录 3=支付</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2017-08-18 林建生
        /// </remarks>
        public ActionResult RetrievePWD(string account, string pwd, string identifyingCode, string type)
        {
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(identifyingCode))
            {
                return View();
            }
            else
            {
                if (account.Length == 11 && account.Substring(0, 1) == "1" && pwd.Length > 5)
                {
                    using (EFDB db = new EFDB())
                    {
                        //判断该手机号十分钟之内是否有验证码
                        var sms = db.SMS.Where(l => l.PhoneNumber == account && l.Content == identifyingCode).OrderByDescending(l => l.AddTime).FirstOrDefault();
                        if (sms != null)
                        {
                            TimeSpan ts = DateTime.Now - sms.AddTime;
                            if (ts.TotalMinutes <= 10)
                            {
                                string pwdMD5 = MD5.GetMD5ljsheng(pwd);
                                var b = db.Member.Where(l => l.Account == account).FirstOrDefault();
                                if (type == "3")
                                {
                                    b.PayPWD = pwdMD5;
                                }
                                else
                                {
                                    b.PWD = pwdMD5;
                                }
                                b.LoginIdentifier = LCommon.TimeToUNIX(DateTime.Now);
                                if (db.SaveChanges() == 1)
                                {
                                    LCookie.DelCookie("linjiansheng");
                                    AppApi.PWD(account, pwd, type);
                                    return Helper.Redirect("成功！", "/Home/Login", "修改密码成功,请点确定重新登录!");
                                }
                                else
                                {
                                    return Helper.Redirect("失败！", "history.go(-1);", "帐号不存在,点确定返回重新修改!");
                                }
                            }
                            else
                            {
                                return Helper.Redirect("失败！", "history.go(-1);", "验证码已失效,请重新获取");
                            }
                        }
                        else
                        {
                            return Helper.Redirect("失败！", "history.go(-1);", "短信验证码错误");
                        }
                    }
                }
                else
                {
                    return Helper.Redirect("失败！", "history.go(-1);", "必须是11位的手机号,密码最少需要6位");
                }
            }
        }
        #endregion

        #region 首页
        /// <summary>
        /// 项目首页
        /// </summary>
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
                //项目数据的类型
                //if (!string.IsNullOrEmpty(Request.QueryString["project"]))
                //{
                //    LCookie.AddCookie("project", Request.QueryString["project"], 30);
                //}
                //获取项目分类
                //int Project = LCookie.Project();
                LCookie.AddCookie("project", "2", 30);
                int Project = 2;
                //获取广告
                var AD = db.AD.Where(l => l.Show == 1 && l.Project == Project && l.Sort == 1).FirstOrDefault();
                if (AD != null)
                {
                    ViewBag.ADTopPicture = AD.Picture;
                    ViewBag.ADTopUrl = AD.Url;
                }
                //获取产品
                var p = db.Product.Where(l=>l.Show==1).GroupJoin(db.Classify,
                    x => x.ClassifyGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        x.Gid,
                        x.Name,
                        x.Picture,
                        x.Price,
                        x.Company,
                        x.Stock,
                        x.Prefix,
                        y.FirstOrDefault().Project
                    }).Where(l => l.Project == Project).ToList();
                string phtm = "";
                foreach (var dr in p)
                {
                    phtm += "<li>";
                    phtm += "<a href = \"/Home/Detail?gid="+ dr.Gid + "\">";
                    phtm += "<p ><img src = \"/uploadfiles/product/" + dr.Picture + "\" /></p>";
                    phtm += "<p class=\"title\">" + dr.Prefix + dr.Name + "</p>";
                    phtm += "<p class=\"price\">"+ dr.Price + "/" + dr.Stock + " " + dr.Company + "</p>";
                    phtm += "</a>";
                    phtm += "</li>";
                }
                ViewBag.Product = phtm;
            }
            return View();
        }

        /// <summary>
        /// 茶叶
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Tea()
        {
            using (EFDB db = new EFDB())
            {
                //项目数据的类型
                //if (!string.IsNullOrEmpty(Request.QueryString["project"]))
                //{
                //    LCookie.AddCookie("project", Request.QueryString["project"], 30);
                //}
                //获取项目分类
                //int Project = LCookie.Project();
                LCookie.AddCookie("project", "1", 30);
                int Project = 1;
                //获取广告
                var AD = db.AD.Where(l => l.Show == 1 && l.Project== Project).ToList();
                var ADTop = AD.Where(l => l.Sort == 1).FirstOrDefault();
                if (ADTop != null)
                {
                    ViewBag.ADTopPicture = ADTop.Picture;
                    ViewBag.ADTopUrl = ADTop.Url;
                }
                var ADC = AD.Where(l => l.Sort == 2).FirstOrDefault();
                if (ADC != null)
                {
                    ViewBag.ADCPicture = ADC.Picture;
                    ViewBag.ADCUrl = ADC.Url;
                }
                //获取产品
                ViewBag.Product = db.Product.Where(l => l.Show == 3).GroupJoin(db.Classify,
                    x => x.ClassifyGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        x.Gid,
                        x.Name,
                        x.Picture,
                        x.Price,
                        x.Company,
                        y.FirstOrDefault().Project
                    }).Where(l=>l.Project == Project).Take(4).ToList();
                ViewBag.HotShow = db.Product.Where(l => l.Show == 1).OrderByDescending(l=>l.Number).Take(4).ToList();
                ViewBag.Classify = db.Classify.Where(l => l.Project == Project).ToList();
            }
            return View();
        }

        /// <summary>
        /// 城市切换
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult City()
        {
            using (EFDB db = new EFDB())
            {
                return View(db.OpenCity.Where(l => l.Show == 1).OrderByDescending(l => l.Sort).ToList());
            }
        }
        #endregion

        #region 产品展示
        /// <summary>
        /// 产品展示
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Classify()
        {
            //using (EFDB db = new EFDB())
            //{
            //    int Type = LCookie.Type();
            //    var b = db.Classify.Where(l => l.Show == 1 && l.Type == Type).OrderByDescending(l => l.Sort);
            //    ViewBag.nav = b.Count();
            //    ViewBag.Sort = b.FirstOrDefault().Sort;
            //    return View(b.ToList());
            //}
            return View();
        }

        /// <summary>
        /// 分类列表
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult ClassifyList()
        {
            using (EFDB db = new EFDB())
            {
                int Project = LCookie.Project();
                return Json(new AjaxResult(new
                {
                    list = db.Classify.Where(l => l.Show == 1 && l.Project == Project).OrderByDescending(l => l.Sort).Select(l => new { l.Gid,l.Name,Count = db.Product.Where(p=>p.ClassifyGid==l.Gid).Count() }).Where(l=>l.Count!=0).ToList()
                }));
            }
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="Name">产品名称</param>
        /// <param name="ClassifyGid">产品分类</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult PList()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            //解析参数
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string Name = paramJson["Name"].ToString();
            using (EFDB db = new EFDB())
            {
                int Project = LCookie.Project();
                var b = db.Product.Where(l => l.Show == 1).GroupJoin(db.Classify,
                    l => l.ClassifyGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.Prefix,
                        l.Name,
                        l.Sort,
                        l.Picture,
                        l.Price,
                        l.Company,
                        l.Brand,
                        l.ClassifyGid,
                        j.FirstOrDefault().Project
                    }).Where(l => l.Project == Project).AsQueryable();
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }
                string ClassifyGid = paramJson["ClassifyGid"].ToString();
                if (!string.IsNullOrEmpty(ClassifyGid))
                {
                    Guid Gid = Guid.Parse(ClassifyGid);
                    b = b.Where(l => l.ClassifyGid == Gid);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = ClassifyGid,
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.Sort).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 产品详情
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Detail()
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                var b = db.Product.Where(l => l.Gid == Gid).FirstOrDefault();
                if (b != null)
                {
                    ViewBag.Gid = b.Gid;
                    ViewBag.Prefix = b.Prefix;
                    ViewBag.Name = b.Name;
                    ViewBag.Price = b.Price;
                    ViewBag.Company = b.Company;
                    ViewBag.Picture = Help.Product + b.Picture;
                    ViewBag.Brand = b.Brand;
                    ViewBag.Content = b.Content;
                    b.Number = b.Number + 1;
                    db.SaveChanges();
                }
                ViewBag.RMB = 0;
                ViewBag.Stock = "";
                if (LCookie.Project() == 2)
                {
                    string ck = LCookie.GetCookie("linjiansheng");
                    if (!string.IsNullOrEmpty(ck))
                    {
                        Guid MemberGid = LCookie.GetMemberGid();
                        var m = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                        decimal BuyPrice = 0;
                        if (m != null)
                        {
                            BuyPrice = m.BuyPrice;
                        }
                        if (string.IsNullOrEmpty(Request.QueryString["MPGid"]))
                        {
                            ViewBag.Stock = b.Stock;
                            //进货价是0就是产品价格X库存,有进货价的话就是进货价X库存
                            ViewBag.Price = BuyPrice == 0 ? b.Price : BuyPrice * b.Stock;
                            if (BuyPrice * b.Stock > b.Price)
                            {
                                ViewBag.Price = b.Price;
                            }
                            ViewBag.RMB = b.Price;
                        }
                        else
                        {
                            //会员出售
                        }
                    }
                    else
                    {
                        return Helper.Redirect("请先登录", "/Home/Login", "登录查看你的购买价格!");
                    }
                }
            }
            return View();
        }
        #endregion

        #region 购物车模块
        /// <summary>
        /// 购物车
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult ShopCart()
        {
            return View();
        }

        #endregion

        #region 搜索
        /// <summary>
        /// 搜索
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Search()
        {
            return View();
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="Name">产品名称</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult SearchData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            //解析参数
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string Name = paramJson["Name"].ToString();
            if (!string.IsNullOrEmpty(Name))
            {
                using (EFDB db = new EFDB())
                {
                    int Project = LCookie.Project();
                    var b = db.Product.Where(l => l.Show == 1).GroupJoin(db.Classify,
                        l => l.ClassifyGid,
                        j => j.Gid,
                        (l, j) => new
                        {
                            l.Gid,
                            l.Name,
                            l.Sort,
                            l.Picture,
                            l.Price,
                            l.Company,
                            l.Brand,
                            j.FirstOrDefault().Project
                        }).Where(l => l.Project == Project).AsQueryable();
                    if (!string.IsNullOrEmpty(Name))
                    {
                        b = b.Where(l => l.Name.Contains(Name));
                    }
                    int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                    int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                    return Json(new AjaxResult(new
                    {
                        other = "",
                        count = b.Count(),
                        pageindex,
                        list = b.OrderByDescending(l => l.Sort).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                    }));
                }
            }
            else
            {
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = 0,
                    pageindex =1,
                    list = new string[0]
                }));
            }
        }
        #endregion

        #region 商城
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult Shop()
        {
            using (EFDB db = new EFDB())
            {
                Guid DGid = db.Dictionaries.Where(l => l.DictionaryType == "Shop").FirstOrDefault().Gid;
                return View(db.DictionariesList.Where(dl => dl.DGid == DGid).ToList());
            }
        }

        /// <summary>
        /// 获取订单
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult ShopData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string Name = paramJson["Name"].ToString();
            using (EFDB db = new EFDB())
            {
                var b = db.Shop.Where(l => l.Show==1 && l.State==2).AsQueryable();
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }
                if (!string.IsNullOrEmpty(paramJson["Project"].ToString()))
                {
                    foreach (string g in paramJson["Project"].ToString().Split(','))
                    {
                        if (!string.IsNullOrEmpty(g))
                        {
                            int Project = int.Parse(g);
                            b = b.Where(l => l.Project != Project);
                        }
                    }
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
                        l.Name,
                        l.Remarks,
                        l.Picture,
                        l.Profile,
                        l.ContactNumber,
                        l.Province,
                        l.City,
                        l.Area,
                        l.Address
                    }).ToList()
                }));
            }
        }

        /// <summary>
        /// 商家中心
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult ShopDetail()
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = Guid.Parse(Request.QueryString["Gid"]);
                var b = db.Shop.Where(l => l.Gid == Gid).FirstOrDefault();
                ViewBag.Gid = Gid;
                ViewBag.Name = b.Name;
                ViewBag.State = b.State;
                ViewBag.Profile = b.Profile;
                ViewBag.Content = b.Content;
                ViewBag.ContactNumber = b.ContactNumber;
                ViewBag.Picture = Help.Shop + b.Picture;
                ViewBag.Address = b.Province + b.City + b.Area + b.Address;
            }
            return View();
        }

        #region 产品展示
        /// <summary>
        /// 产品展示
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult ShopClassify()
        {
            LCookie.AddCookie("ShopGid", Request.QueryString["Gid"], 0);
            return View();
        }

        /// <summary>
        /// 分类列表
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult ShopClassifyList()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            Guid ShopGid = Guid.Parse(json);
            using (EFDB db = new EFDB())
            {
                return Json(new AjaxResult(new
                {
                    list = db.ShopClassify.Where(l => l.Show == 1 && l.ShopGid == ShopGid).OrderByDescending(l => l.Sort).Select(l => new { l.Gid, l.Name, Count = db.ShopProduct.Where(p => p.ClassifyGid == l.Gid).Count() }).Where(l => l.Count != 0).ToList()
                }));
            }
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="Name">产品名称</param>
        /// <param name="ClassifyGid">产品分类</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult ShopPList()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            //解析参数
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string Name = paramJson["Name"].ToString();
            using (EFDB db = new EFDB())
            {
                Guid ShopGid = Guid.Parse(paramJson["ShopGid"].ToString());
                var b = db.ShopProduct.Where(l => l.Show == 1 && l.ShopGid == ShopGid && l.Stock>0).GroupJoin(db.ShopClassify,
                    l => l.ClassifyGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.ShopGid,
                        l.Prefix,
                        l.Name,
                        l.Sort,
                        l.Picture,
                        l.Price,
                        l.Company,
                        l.Brand,
                        l.ClassifyGid
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }
                string ClassifyGid = paramJson["ClassifyGid"].ToString();
                if (!string.IsNullOrEmpty(ClassifyGid))
                {
                    Guid Gid = Guid.Parse(ClassifyGid);
                    b = b.Where(l => l.ClassifyGid == Gid);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = ClassifyGid,
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.Sort).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 产品详情
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult ShopProduct()
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                var b = db.ShopProduct.Where(l => l.Gid == Gid).FirstOrDefault();
                if (b != null)
                {
                    ViewBag.Gid = b.Gid;
                    ViewBag.Prefix = b.Prefix;
                    ViewBag.Name = b.Name;
                    ViewBag.Price = b.Price;
                    ViewBag.Company = b.Company;
                    ViewBag.Picture = Help.Product + b.Picture;
                    ViewBag.Brand = b.Brand;
                    ViewBag.Content = b.Content;
                    ViewBag.Stock = b.Stock;
                    b.Number = b.Number + 1;
                    db.SaveChanges();
                }
            }
            return View();
        }
        #endregion

        /// <summary>
        /// 商城购物车
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Cart()
        {
            return View();
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult ShopSearch()
        {
            return View();
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="Name">产品名称</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult ShopSearchData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            //解析参数
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string Name = paramJson["Name"].ToString();
            if (!string.IsNullOrEmpty(Name))
            {
                using (EFDB db = new EFDB())
                {
                    Guid ShopGid = Guid.Parse(LCookie.GetCookie("shop"));
                    var b = db.ShopProduct.Where(l => l.Show == 1).Where(l => l.ShopGid == ShopGid).AsQueryable();
                    if (!string.IsNullOrEmpty(Name))
                    {
                        b = b.Where(l => l.Name.Contains(Name));
                    }
                    int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                    int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                    return Json(new AjaxResult(new
                    {
                        other = "",
                        count = b.Count(),
                        pageindex,
                        list = b.OrderByDescending(l => l.Sort).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                    }));
                }
            }
            else
            {
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = 0,
                    pageindex = 1,
                    list = new string[0]
                }));
            }
        }
        #endregion

        #region 市场
        /// <summary>
        /// 市场
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Market()
        {
            return View();
        }

        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="Name">产品名称</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult MarketData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            //解析参数
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            using (EFDB db = new EFDB())
            {
                var b = db.Order.Where(l => l.Type == 5 && l.PayStatus == 2).GroupJoin(db.OrderDetails,
                    x => x.Gid,
                    y => y.OrderGid,
                    (x, y) => new
                    {
                        x.Gid,
                        x.AddTime,
                        x.OrderNo,
                        x.Price,
                        y.FirstOrDefault().Number,
                        RealName = db.Member.Where(l => l.Gid == x.ShopGid).FirstOrDefault().RealName
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
    }
}