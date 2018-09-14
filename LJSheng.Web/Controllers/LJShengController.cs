using EntityFramework.Extensions;
using LJSheng.Common;
using LJSheng.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LJSheng.Web.Controllers
{
    public class LJShengController : LJSController
    {
        #region 后台管理
        // 订单调试
        public ActionResult OrderPay()
        {
            string msg = "请输入订单数据操作";
            string OrderNo = Request.QueryString["OrderNo"];
            int type = int.Parse(Request.QueryString["type"]);
            if (!string.IsNullOrEmpty(OrderNo))
            {
                decimal PayPrice = decimal.Parse(Request.QueryString["PayPrice"]);
                int PayType = int.Parse(Request.QueryString["PayType"]);
                using (EFDB db = new EFDB())
                {
                    if (type == 1)
                    {
                        if (db.Order.Where(l => l.OrderNo == OrderNo && l.Price == PayPrice && l.PayType == PayType && l.PayStatus == 2).Count() == 1)
                        {
                            if (Helper.PayOrder(OrderNo, Request.QueryString["TradeNo"], PayType, PayPrice))
                            {
                                msg = "彩链订单(" + OrderNo + ")已操作成功";
                            }
                        }
                        else
                        {
                            msg = "彩链订单(" + OrderNo + ")无效";
                        }
                    }
                    else if (type == 2)
                    {
                        if (db.ShopOrder.Where(l => l.OrderNo == OrderNo && l.Price == PayPrice && l.PayType == PayType && l.PayStatus == 2).Count() == 1)
                        {
                            if (Helper.ShopPayOrder(OrderNo, Request.QueryString["TradeNo"], PayType, PayPrice))
                            {
                                msg = "商城订单(" + OrderNo + ")已操作成功";
                            }
                        }
                        else
                        {
                            msg = "商城订单(" + OrderNo + ")无效";
                        }
                    }
                    else
                    {
                        msg = "找不到对账类型";
                    }
                }
            }
            ViewBag.TF = msg;
            return View();
        }

        // 后台操作提示
        public ActionResult Page()
        {
            ViewBag.Msg = Request.QueryString["msg"];
            ViewBag.Title = Request.QueryString["title"];
            ViewBag.Url = Request.QueryString["url"];
            return View();
        }
        // 后台登录
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string Account, string PWD, string Code)
        {
            if (!LCookie.GetCookie("CheckCode").Equals(Code.Trim()))
            {
                return Helper.WebRedirect("登录失败！", "history.go(-1);", "验证码错误!");
            }
            else
            {
                using (EFDB db = new EFDB())
                {
                    PWD = MD5.GetMD5ljsheng(PWD);
                    var b = db.LJSheng.Where(l => l.Account == Account && l.PWD == PWD).FirstOrDefault();
                    if (b != null)
                    {
                        LCookie.DelCookie("CheckCode");
                        LCookie.AddCookie("ljsheng", DESRSA.DESEnljsheng(JsonConvert.SerializeObject(new
                        {
                            b.Gid,
                            b.Account,
                            b.RealName,
                            b.LoginIdentifier,
                            b.Jurisdiction
                        })), 0);
                        return RedirectToAction("/Index");
                    }
                    else
                    {
                        return Helper.WebRedirect("登录失败！", "history.go(-1);", "您输入的用户或密码错误");
                    }
                }
            }
        }
        // 菜单
        //[Authorize(Roles = "admins")]
        public ActionResult Menu()
        {
            using (EFDB db = new EFDB())
            {
                var d = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault();
                return View(db.DictionariesList.Where(l => l.DGid == d.Gid && l.Value=="2").ToList());
            }
        }
        // 头部
        public ActionResult Top()
        {
            return View();
        }

        // 数据备份
        public ActionResult DBBak()
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/dbbak/");
            string name = "backup_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            DbHelperSQL.ExecuteSql("BACKUP DATABASE [baijia] TO  DISK = N'" + path + name + ".bak' WITH  RETAINDAYS = 7, NOFORMAT, NOINIT,  NAME = N'" + name + "', SKIP, REWIND, NOUNLOAD,  STATS = 10");
            List<FileInfo> files = new List<FileInfo>();
            ///获取文件列表信息  
            foreach (var file in Directory.GetFiles(path))
            {
                files.Add(new FileInfo(file));
            }
            ///查询文件列表信息  
            var filevalues = from file in files
                             where file.Extension == ".bak"
                             orderby file.CreationTime descending
                             select file;
            return View(filevalues.ToList());
        }
        /// <summary>
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult DBBakDelete(string name)
        {
            string file = System.Web.HttpContext.Current.Server.MapPath("/uploadfiles/dbbak/" + name);
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
                return Json(new AjaxResult(name + " 删除成功"));
            }
            else
            {
                return Json(new AjaxResult(300, name + " 删除失败"));
            }
        }
        // 后台中心
        public ActionResult Index()
        {
            using (EFDB db = new EFDB())
            {
                //返还24小时被扣除的彩链订单库存
                {
                    DateTime dt = DateTime.Now.AddHours(-24);
                    var b = db.Order.Where(l => l.ShopGid != null && l.PayStatus == 2 && l.AddTime < dt).Select(l => new
                    {
                        l.Gid,
                        l.ShopGid,
                        Number = db.OrderDetails.Where(od => od.OrderGid == l.Gid).Select(od => od.Number).DefaultIfEmpty(0).Sum()
                    }).ToList();
                    //默认商品
                    Guid ProductGid = Helper.GetProductGid();
                    foreach (var dr in b)
                    {
                        if (db.Order.Where(l => l.Gid == dr.Gid).Update(l => new Order { PayStatus = 4 }) == 1)
                        {
                            if (dr.Number > 0)
                            {
                                if (db.Stock.Where(l => l.MemberGid == dr.ShopGid && l.ProductGid == ProductGid).Update(l => new Stock { Number = l.Number + dr.Number }) == 1)
                                {
                                    LogManager.WriteLog("订单库存赎回成功", "订单=" + dr.Gid.ToString() + "库存=" + dr.Number.ToString());
                                }
                                else
                                {
                                    LogManager.WriteLog("订单关闭成功库存赎回失败", "订单=" + dr.Gid.ToString() + "库存=" + dr.Number.ToString());
                                }
                            }
                        }
                        else
                        {
                            LogManager.WriteLog("订单库存赎回状态失败", "订单=" + dr.Gid.ToString() + "库存=" + dr.Number.ToString());
                        }
                    }
                }
                //商城15天后自动确认收货打款
                {
                    DateTime dt = DateTime.Now.AddDays(-15);
                    var so = db.ShopOrder.Where(l => l.ShopGid != null && l.PayStatus == 1 && l.Status==1 && l.DeliveryTime < dt).Select(l => new{l.Gid}).ToList();
                    foreach (var dr in so)
                    {
                        var b = db.ShopOrder.Where(l => l.Gid == dr.Gid && l.Status == 1).FirstOrDefault();
                        if (b != null && b.PayStatus == 1)
                        {
                            b.Status = 2;
                            b.ExpressStatus = 3;
                            b.Remarks = "自动确认收货";
                            if (db.SaveChanges() == 1)
                            {
                                Guid ShopGid = db.Shop.Where(l => l.Gid == b.ShopGid).FirstOrDefault().MemberGid;
                                if (db.Member.Where(l => l.Gid == ShopGid).Update(l => new Member { ShopMoney = l.ShopMoney + b.PayPrice }) == 1)
                                {
                                    LogManager.WriteLog("自动确认收货成功", "订单=" + dr.Gid);
                                }
                                else
                                {
                                    LogManager.WriteLog("自动确认收货成功货款失败", "订单=" + dr.Gid);
                                }
                            }
                        }
                    }
                }
            }
            return View();
        }
        //快递
        public ActionResult Express()
        {
            return View();
        }
        #endregion

        #region 数据字典
        /// <summary>
        /// 字典管理
        /// </summary>
        public ActionResult DictionariesList()
        {
            string DictionaryType = Request.Form["DictionaryType"];
            if (!string.IsNullOrEmpty(DictionaryType))
            {
                using (EFDB db = new EFDB())
                {
                    Dictionaries b;
                    if (db.Dictionaries.Where(l => l.DictionaryType == DictionaryType).Count() > 0)
                    {
                        return Helper.WebRedirect("已存在！", "history.go(-1);", "已存在");
                    }
                    else
                    {
                        b = new Dictionaries();
                        b.Gid = Guid.NewGuid();
                        b.AddTime = DateTime.Now;
                        b.DictionaryType = Request.Form["DictionaryType"];
                        b.Remarks = Request.Form["Remarks"]; ;
                        b.Sort = Int32.Parse(Request.Form["Sort"]);
                        db.Dictionaries.Add(b);
                        if (db.SaveChanges() == 1)
                        {
                            return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                        }
                        else
                        {
                            return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                        }
                    }
                }
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult Dictionaries()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string DictionaryType = paramJson["DictionaryType"].ToString();
                string Remarks = paramJson["Remarks"].ToString();
                var b = db.Dictionaries.AsQueryable();
                if (!string.IsNullOrEmpty(DictionaryType))
                {
                    b = b.Where(l => l.DictionaryType.Contains(DictionaryType));
                }
                if (!string.IsNullOrEmpty(Remarks))
                {
                    b = b.Where(l => l.Remarks.Contains(Remarks));
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderBy(l => l.Sort).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult DictionariesDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    if (db.Dictionaries.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        db.DictionariesList.Where(l => l.DGid == Gid).Delete();
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }

        /// <summary>
        /// 键值管理
        /// </summary>
        public ActionResult DicList()
        {
            string Key = Request.Form["Key"];
            if (!string.IsNullOrEmpty(Key))
            {
                Guid DGid = Guid.Parse(Request.QueryString["gid"]);
                using (EFDB db = new EFDB())
                {
                    DictionariesList b;
                    if (db.DictionariesList.Where(l => l.Key == Key && l.DGid == DGid).Count() > 0)
                    {
                        return Helper.WebRedirect("已存在！", "history.go(-1);", "已存在");
                    }
                    else
                    {
                        b = new DictionariesList();
                        b.Gid = Guid.NewGuid();
                        b.AddTime = DateTime.Now;
                        b.Key = Request.Form["Key"];
                        b.Value = Request.Form["Value"];
                        b.Remarks = Request.Form["Remarks"]; ;
                        b.Sort = Int32.Parse(Request.Form["Sort"]);
                        b.DGid = DGid;
                        db.DictionariesList.Add(b);
                        if (db.SaveChanges() == 1)
                        {
                            return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                        }
                        else
                        {
                            return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                        }
                    }
                }
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult Dic()
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
                string Key = paramJson["Key"].ToString();
                string Value = paramJson["Value"].ToString();
                string Remarks = paramJson["Remarks"].ToString();
                Guid DGid = Guid.Parse(paramJson["gid"].ToString());
                var b = db.DictionariesList.Where(l => l.DGid == DGid).AsQueryable();
                if (!string.IsNullOrEmpty(Key))
                {
                    b = b.Where(l => l.Key.Contains(Key));
                }
                if (!string.IsNullOrEmpty(Value))
                {
                    b = b.Where(l => l.Value.Contains(Value));
                }
                if (!string.IsNullOrEmpty(Remarks))
                {
                    b = b.Where(l => l.Remarks.Contains(Remarks));
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderBy(l => l.Sort).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult DicDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    if (db.DictionariesList.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 会员等级模块
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult LevelAU()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
            {
                using (EFDB db = new EFDB())
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.Level.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Name = b.Name;
                    ViewBag.LV = b.LV;
                    ViewBag.LNumber = b.LNumber;
                    ViewBag.BuyAmount = b.BuyAmount;
                    ViewBag.Distribution = b.Distribution;
                    ViewBag.Recommendation = b.Recommendation * 100;
                    ViewBag.Differential = b.Differential * 100;
                    ViewBag.SameLevel = b.SameLevel * 100;
                    ViewBag.Bonus = b.Bonus * 100;
                    ViewBag.Profit = b.Profit * 100;
                    ViewBag.EquityProfit = b.EquityProfit * 100;
                    ViewBag.ShopProfit = b.ShopProfit * 100;
                    ViewBag.Multiple = b.Multiple;
                    ViewBag.Label = b.Label;
                    ViewBag.Remarks = b.Remarks;
                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult LevelAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                Level b;
                int LV = int.Parse(Request.Form["LV"]);
                if (db.Level.Where(l => l.LV == LV && l.Gid != Gid).Count() > 0)
                {
                    return Helper.WebRedirect("操作失败！", "history.go(-1);", "等级标识已存在!");
                }
                else
                {
                    if (Gid == null)
                    {
                        b = new Data.Level();
                        b.Gid = Guid.NewGuid();
                        b.AddTime = DateTime.Now;
                        b.Project = int.Parse(Request.QueryString["project"]);
                        b.UP = 1;
                        b.SellStock = 1;
                    }
                    else
                    {
                        b = db.Level.Where(l => l.Gid == Gid).FirstOrDefault();
                    }
                    b.Name = Request.Form["Name"];
                    b.Recommendation = decimal.Parse(Request.Form["Recommendation"]) / 100;
                    b.Differential = decimal.Parse(Request.Form["Differential"]) / 100;
                    b.SameLevel = decimal.Parse(Request.Form["SameLevel"]) / 100;
                    b.Bonus = decimal.Parse(Request.Form["Bonus"]) / 100;
                    b.Profit = decimal.Parse(Request.Form["Profit"]) / 100;
                    b.EquityProfit = decimal.Parse(Request.Form["EquityProfit"]) / 100;
                    b.ShopProfit = decimal.Parse(Request.Form["ShopProfit"]) / 100;
                    b.LV = LV;
                    b.Multiple = int.Parse(Request.Form["Multiple"]);
                    b.LNumber = int.Parse(Request.Form["LNumber"]);
                    b.BuyAmount = int.Parse(Request.Form["BuyAmount"]);
                    b.Distribution = int.Parse(Request.Form["Distribution"]);
                    b.Label = Request.Form["Label"];
                    b.Remarks = Request.Form["Remarks"];
                    if (Gid == null)
                    {
                        db.Level.Add(b);
                    }
                    if (db.SaveChanges() == 1)
                    {
                        return Helper.WebRedirect("操作成功！", "history.go(-1);", "恭喜你,操作成功!");
                    }
                    else
                    {
                        return Helper.WebRedirect("操作失败！", "history.go(-1);", "操作失败,请检查录入的数据!");
                    }
                }
            }
        }

        // 列表管理
        public ActionResult LevelList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Level()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Name = paramJson["Name"].ToString();
                var b = db.Level.AsQueryable();
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }
                int Project = int.Parse(paramJson["Project"].ToString());
                b = b.Where(l => l.Project == Project);
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderBy(l => l.LV).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult LevelDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    if (db.Level.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }

        /// <summary>
        /// 升级条件和出售库存
        /// </summary>
        /// <param name="Gid">Gid</param>
        /// <param name="Type">1=升级条件 2=出售库存</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult UPSellStock(Guid Gid,int Type)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Level.Where(l => l.Gid == Gid).FirstOrDefault();
                if (b != null)
                {
                    if (Type == 1)
                    {
                        b.UP = b.UP == 1 ? 2 : 1;
                    }
                    if (Type == 2)
                    {
                        b.SellStock = b.SellStock == 1 ? 2 : 1;
                    }
                    if (db.SaveChanges() == 1)
                    {
                        return Json(new AjaxResult("更改成功,请重新打开本菜单确认!"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "更改失败!"));
                    }
                }
                else
                {
                    return Json(new AjaxResult(300, "找不到!"));
                }
            }
        }
        #endregion

        #region 奖励比例模块
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult LVAU()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
            {
                using (EFDB db = new EFDB())
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.LV.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Number = b.Number;
                    ViewBag.LVID = b.LVID;
                    ViewBag.Differential = b.Differential * 100;
                    ViewBag.SameLevel = b.SameLevel * 100;
                    ViewBag.ShopProfit = b.ShopProfit * 100;
                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult LVAU(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.LV.Where(l => l.Gid == Gid).FirstOrDefault();
                b.Number = int.Parse(Request.Form["Number"]);
                b.SameLevel = decimal.Parse(Request.Form["SameLevel"]) / 100;
                b.Differential = decimal.Parse(Request.Form["Differential"]) / 100;
                b.ShopProfit = decimal.Parse(Request.Form["ShopProfit"]) / 100;
                if (db.SaveChanges() == 1)
                {
                    return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                }
                else
                {
                    return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                }
            }
        }
        /// <summary>
        /// 奖励比例
        /// </summary>
        public ActionResult LVList()
        {
            if (!string.IsNullOrEmpty(Request.Form["Number"]))
            {
                int num = int.Parse(Request.Form["Number"]);
                int LVID = int.Parse(Request.QueryString["lvid"]);
                using (EFDB db = new EFDB())
                {
                    if (db.LV.Where(l => l.Number == num && l.LVID == LVID).Count() > 0)
                    {
                        return Helper.WebRedirect("已存在！", "history.go(-1);", "已存在");
                    }
                    else
                    {
                        var b = new LV();
                        b.Gid = Guid.NewGuid();
                        b.AddTime = DateTime.Now;
                        b.Number = num;
                        b.LVID = LVID;
                        b.ShopProfit = 0;
                        b.SameLevel = decimal.Parse(Request.Form["SameLevel"]) / 100;
                        b.Differential = decimal.Parse(Request.Form["Differential"]) / 100;
                        db.LV.Add(b);
                        if (db.SaveChanges() == 1)
                        {
                            return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                        }
                        else
                        {
                            return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                        }
                    }
                }
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult LV()
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
                int LVID = int.Parse(paramJson["LVID"].ToString());
                var b = db.LV.Where(l => l.LVID == LVID).AsQueryable();
                if (!string.IsNullOrEmpty(paramJson["Number"].ToString()))
                {
                    int Number = int.Parse(paramJson["Number"].ToString());
                    b = b.Where(l => l.Number == Number);
                }
                if (!string.IsNullOrEmpty(paramJson["Differential"].ToString()))
                {
                    decimal Differential = decimal.Parse(paramJson["Differential"].ToString()) / 100;
                    b = b.Where(l => l.Differential == Differential);
                }
                if (!string.IsNullOrEmpty(paramJson["SameLevel"].ToString()))
                {
                    decimal SameLevel = decimal.Parse(paramJson["SameLevel"].ToString()) / 100;
                    b = b.Where(l => l.SameLevel == SameLevel);
                }
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
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult LVDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    if (db.LV.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 会员模块
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult MemberAU()
        {
            using (EFDB db = new EFDB())
            {
                if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    Member b = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Account = b.Account;
                    ViewBag.PWD = b.PWD;
                    ViewBag.RealName = b.RealName;
                    ViewBag.Gender = b.Gender;
                    ViewBag.NickName = b.NickName;
                    ViewBag.Openid = b.Openid;
                    ViewBag.Jurisdiction = b.Jurisdiction;
                    ViewBag.ContactNumber = b.ContactNumber;
                    ViewBag.IP = b.IP;
                    ViewBag.Province = b.Province;
                    ViewBag.City = b.City;
                    ViewBag.Area = b.Area;
                    ViewBag.Address = b.Address;
                    ViewBag.Money = b.Money;
                    ViewBag.Integral = b.Integral;
                    ViewBag.ProductMoney = b.ProductMoney;
                    ViewBag.StockRight = b.StockRight;
                    ViewBag.Level = b.Level;
                    ViewBag.CLLevel = b.CLLevel;
                    ViewBag.Bank = b.Bank;
                    ViewBag.BankName = b.BankName;
                    ViewBag.BankNumber = b.BankNumber;
                    ViewBag.MID = b.MID;
                    ViewBag.Picture = Help.Member + b.Picture;
                }
                else
                {
                    ViewBag.Gender = "男";
                    ViewBag.Jurisdiction = "正常";
                    ViewBag.Level = "1";
                    ViewBag.Level = "21";
                }
                var lv = db.Level.OrderBy(l => l.LV);
                ViewBag.LevelList = lv.Where(l => l.Project == 1 && (l.LV == 1 || l.LV == 11)).ToList();
                ViewBag.CLLevelList = lv.Where(l => l.Project == 2).ToList();
                return View();
            }
        }
        [HttpPost]
        public ActionResult MemberAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                Member b;
                string Account = Request.Form["Account"];
                string PWD = Request.Form["PWD"];
                string Picture = Request.Form["Picture"];
                if (db.Member.Where(l => l.Account == Account && l.Gid != Gid).Count() > 0)
                {
                    return Helper.WebRedirect("操作失败！", "history.go(-1);", "帐号已存在!");
                }
                else
                {
                    if (Gid == null)
                    {
                        b = new Member();
                        b.Gid = Guid.NewGuid();
                        b.AddTime = DateTime.Now;
                        b.Account = Account;
                        b.LoginIdentifier = "0000000000";
                        b.IP = Helper.IP;
                        b.Money = 0;
                        b.Integral = 0;
                        b.ProductMoney = 0;
                        b.Level6 = 0;
                        b.Level7 = 0;
                        b.Level8 = 0;
                        b.Level9 = 0;
                        b.TMoney = 0;
                        b.TNumber = 0;
                        b.MID = db.Member.Max(l => l.MID) + 1;
                    }
                    else
                    {
                        b = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                    }
                    b.RealName = Request.Form["RealName"];
                    //if (b.PWD != PWD)
                    //{
                    //    b.PWD = MD5.GetMD5ljsheng(PWD);
                    //}
                    b.StockRight= decimal.Parse(Request.Form["StockRight"]);
                    b.Jurisdiction = Request.Form["Jurisdiction"];
                    if (b.Jurisdiction == "冻结")
                    {
                        b.LockTime = DateTime.Now;
                    }
                    b.Gender = Request.Form["Gender"];
                    b.NickName = Request.Form["NickName"];
                    b.RealName = b.RealName;
                    b.Gender = b.Gender;
                    b.ContactNumber = Request.Form["ContactNumber"];
                    b.Province = Request.Form["Province"];
                    b.City = Request.Form["City"];
                    b.Area = Request.Form["Area"];
                    b.Address = Request.Form["Address"];
                    //b.Openid = b.Openid;
                    //b.Money = decimal.Parse(Request.Form["Money"]);
                    //b.Integral = int.Parse(Request.Form["Integral"]);
                    //b.ProductMoney = decimal.Parse(Request.Form["ProductMoney"]);
                    //b.StockRight = int.Parse(Request.Form["StockRight"]);
                    b.Level = int.Parse(Request.Form["Level"]);
                    int CLLevel = int.Parse(Request.Form["CLLevel"]);
                    b.CLLevel = CLLevel;
                    b.Bank = Request.Form["Bank"];
                    b.BankName = Request.Form["BankName"];
                    b.BankNumber = Request.Form["BankNumber"];
                    if (!string.IsNullOrEmpty(Picture))
                    {
                        b.Picture = Picture;
                    }
                    if (Gid == null)
                    {
                        db.Member.Add(b);
                    }
                    if (db.SaveChanges() == 1)
                    {
                        if (CLLevel > 24)
                        {
                            Helper.Consignor(b.Gid);
                        }
                        return Helper.WebRedirect("操作成功！", "history.go(-1);", "恭喜你,操作成功!");
                    }
                    else
                    {
                        return Helper.WebRedirect("操作失败！", "history.go(-1);", "操作失败,请检查录入的数据!");
                    }
                }
            }

        }

        // 列表管理
        //同时支持Get和Post
        //[AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult MemberList()
        {
            using (EFDB db = new EFDB())
            {
                var lv = db.Level.OrderBy(l => l.LV);
                ViewBag.Level = lv.Where(l=>l.Project==1 && (l.LV==1 || l.LV==11)).ToList();
                ViewBag.CLLevel = lv.Where(l => l.Project == 2).ToList();
                return View();
            }
        }
        [HttpPost]
        public JsonResult Member()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Account = paramJson["Account"].ToString();
                string RealName = paramJson["RealName"].ToString();
                string MAccount = paramJson["MAccount"].ToString();
                int Level = int.Parse(paramJson["Level"].ToString());
                int CLLevel = int.Parse(paramJson["CLLevel"].ToString());
                var b = db.Member.GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Account,
                        l.RealName,
                        l.Gender,
                        l.ContactNumber,
                        l.Picture,
                        l.Jurisdiction,
                        l.Money,
                        l.Integral,
                        l.ProductMoney,
                        l.StockRight,
                        l.MID,
                        l.Level,
                        l.MemberGid,
                        l.Level6,
                        l.Level7,
                        l.Level8,
                        l.Level9,
                        l.CLLevel,
                        l.BuyPrice,
                        l.LockTime,
                        l.UPTime,
                        l.MIntegral,
                        l.TIntegral,
                        l.ShopMoney,
                        l.ShopIntegral,
                        l.APP,
                        l.CLMoney,
                        MRealName = Nullable<Guid>.Equals(l.MemberGid, null) ? "" : j.FirstOrDefault().RealName,
                        MAccount = Nullable<Guid>.Equals(l.MemberGid, null) ? "" : j.FirstOrDefault().Account
                    }).GroupJoin(db.Level,
                    l => l.Level,
                    j => j.LV,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Account,
                        l.RealName,
                        l.Gender,
                        l.ContactNumber,
                        l.Picture,
                        l.Jurisdiction,
                        l.Money,
                        l.Integral,
                        l.ProductMoney,
                        l.StockRight,
                        l.MID,
                        l.Level,
                        l.MemberGid,
                        l.Level6,
                        l.Level7,
                        l.Level8,
                        l.Level9,
                        l.MRealName,
                        l.MAccount,
                        l.CLLevel,
                        l.BuyPrice,
                        l.LockTime,
                        l.UPTime,
                        l.MIntegral,
                        l.TIntegral,
                        l.ShopMoney,
                        l.ShopIntegral,
                        l.APP,
                        l.CLMoney,
                        LevelName = j.FirstOrDefault().Name,
                        j.FirstOrDefault().Label
                    }).GroupJoin(db.Level,
                    l => l.CLLevel,
                    j => j.LV,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Account,
                        l.RealName,
                        l.Gender,
                        l.ContactNumber,
                        l.Picture,
                        l.Jurisdiction,
                        l.Money,
                        l.Integral,
                        l.ProductMoney,
                        l.StockRight,
                        l.MID,
                        l.Level,
                        l.MemberGid,
                        l.Level6,
                        l.Level7,
                        l.Level8,
                        l.Level9,
                        l.MRealName,
                        l.MAccount,
                        l.CLLevel,
                        l.BuyPrice,
                        l.LevelName,
                        l.Label,
                        l.LockTime,
                        l.UPTime,
                        l.MIntegral,
                        l.TIntegral,
                        l.ShopMoney,
                        l.ShopIntegral,
                        l.APP,
                        l.CLMoney,
                        CLLevelName = j.FirstOrDefault().Name,
                        CLLabel = j.FirstOrDefault().Label,
                        AllMoney = db.Order.Where(o => o.MemberGid == l.Gid && o.PayStatus == 1 && o.PayType != 5).Select(o => o.TotalPrice).DefaultIfEmpty(0m).Sum(),
                        AllShopMoney = db.ShopOrder.Where(o => o.MemberGid == l.Gid && o.PayStatus == 1 && o.PayType != 5).Select(o => o.TotalPrice).DefaultIfEmpty(0m).Sum(),
                        //AllIntegral = db.Order.Where(o => o.MemberGid == l.Gid && o.PayStatus == 1 && o.PayType == 5).Select(o => o.TotalPrice).DefaultIfEmpty(0m).Sum(),
                        Stock = db.Stock.Where(s=>s.MemberGid==l.Gid).Select(s => s.Number).DefaultIfEmpty(0).Sum(),
                        TNumber = db.MRelation.Where(mr => mr.M1 == l.Gid || mr.M2 == l.Gid | mr.M3 == l.Gid).Count(),
                        Level22 = db.Member.Where(m => m.MemberGid == l.Gid  && m.CLLevel==22).Count(),
                        Level23 = db.Member.Where(m => m.MemberGid == l.Gid && m.CLLevel == 23).Count(),
                        Level24 = db.Member.Where(m => m.MemberGid == l.Gid && m.CLLevel == 24).Count(),
                        Level25 = db.Member.Where(m => m.MemberGid == l.Gid && m.CLLevel == 25).Count(),
                        Level26 = db.Member.Where(m => m.MemberGid == l.Gid && m.CLLevel == 26).Count()
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account.Contains(Account));
                }
                if (!string.IsNullOrEmpty(RealName))
                {
                    b = b.Where(l => l.RealName.Contains(RealName));
                }
                if (!string.IsNullOrEmpty(MAccount))
                {
                    b = b.Where(l => l.MAccount == MAccount);
                }
                if (!string.IsNullOrEmpty(paramJson["MID"].ToString()))
                {
                    int MID = int.Parse(paramJson["MID"].ToString());
                    b = b.Where(l => l.MID == MID);
                }
                if (Level != 0)
                {
                    b = b.Where(l => l.Level == Level);
                }
                if (CLLevel != 0)
                {
                    b = b.Where(l => l.CLLevel == CLLevel);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总积分=" + b.Select(l => l.Money).DefaultIfEmpty(0m).Sum()+ ",购物积分=" + b.Select(l => l.Integral).DefaultIfEmpty(0m).Sum() + " | 股权=" + b.Select(l => l.StockRight).DefaultIfEmpty(0m).Sum(),
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
        public JsonResult MemberDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    string file = System.Web.HttpContext.Current.Server.MapPath(Help.Member + db.Member.Where(l => l.Gid == Gid).FirstOrDefault().Picture);
                    if (db.Member.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        if (System.IO.File.Exists(file))
                            System.IO.File.Delete(file);
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        /// <summary>
        /// 重设密码
        /// </summary>
        [HttpPost]
        public JsonResult MemberPWD(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    string PWD = RandStr.CreateValidateNumber(6);
                    var b = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                    b.PWD = MD5.GetMD5ljsheng(PWD);
                    b.PayPWD = MD5.GetMD5ljsheng(PWD);
                    b.LoginIdentifier = "";
                    if (db.SaveChanges() == 1)
                    {
                        AppApi.PWD(b.Account, PWD, "2");
                        AppApi.PWD(b.Account, PWD, "3");
                        return Json(new AjaxResult("新密码为:" + PWD));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }

        /// <summary>
        /// 团队关系
        /// </summary>
        public ActionResult TeamList()
        {
            using (EFDB db = new EFDB())
            {
                var lv = db.Level.OrderBy(l => l.LV);
                ViewBag.Level = lv.Where(l => l.Project == 1).ToList();
                ViewBag.CLLevel = lv.Where(l => l.Project == 2).ToList();
                return View();
            }
        }
        [HttpPost]
        public JsonResult Team()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string STime = paramJson["STime"].ToString();
            string ETime = paramJson["ETime"].ToString();
            string Account = paramJson["Account"].ToString();
            string RealName = paramJson["RealName"].ToString();
            string MAccount = paramJson["MAccount"].ToString();
            int Level = int.Parse(paramJson["Level"].ToString());
            int CLLevel = int.Parse(paramJson["CLLevel"].ToString());
            int Number = int.Parse(paramJson["Number"].ToString());
            Guid MemberGid = Guid.Parse(paramJson["MemberGid"].ToString());
            //按月份查询业绩
            string Year = paramJson["Year"].ToString();
            string Month = paramJson["Month"].ToString();
            DateTime MonthFirst = string.IsNullOrEmpty(Year) ? DateTime.Parse("1984-02-04") : DTime.FirstDayOfMonth(DateTime.Parse(Year + "-" + Month + "-" + "01"));
            DateTime MonthLast = string.IsNullOrEmpty(Year) ? DateTime.Now : DTime.LastDayOfMonth(DateTime.Parse(Year + "-" + Month + "-" + "01 23:59:59"));
            using (EFDB db = new EFDB())
            {
                var b = db.MRelation.Where(l => l.M1 == MemberGid || l.M2 == MemberGid || l.M3 == MemberGid).GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        MAddTime = l.AddTime,
                        Number = l.M1 == MemberGid ? 1 : l.M2 == MemberGid ? 2 : 3,
                        j.FirstOrDefault().Account,
                        j.FirstOrDefault().Picture,
                        j.FirstOrDefault().RealName,
                        j.FirstOrDefault().Level,
                        j.FirstOrDefault().Gid,
                        j.FirstOrDefault().AddTime,
                        j.FirstOrDefault().Gender,
                        j.FirstOrDefault().Jurisdiction,
                        j.FirstOrDefault().Money,
                        j.FirstOrDefault().Integral,
                        j.FirstOrDefault().ProductMoney,
                        j.FirstOrDefault().StockRight,
                        j.FirstOrDefault().MID,
                        j.FirstOrDefault().MemberGid,
                        j.FirstOrDefault().TMoney,
                        j.FirstOrDefault().TNumber,
                        j.FirstOrDefault().Level6,
                        j.FirstOrDefault().Level7,
                        j.FirstOrDefault().Level8,
                        j.FirstOrDefault().Level9,
                        j.FirstOrDefault().CLLevel
                    }).GroupJoin(db.Level,
                    l => l.CLLevel,
                    j => j.LV,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Account,
                        l.RealName,
                        l.Gender,
                        l.Picture,
                        l.Jurisdiction,
                        l.Money,
                        l.Integral,
                        l.ProductMoney,
                        l.StockRight,
                        l.MID,
                        l.Level,
                        l.MemberGid,
                        l.TMoney,
                        l.TNumber,
                        l.Level6,
                        l.Level7,
                        l.Level8,
                        l.Level9,
                        l.CLLevel,
                        MRealName = Nullable<Guid>.Equals(l.MemberGid, null) ? "" : db.Member.Where(m => m.Gid == l.MemberGid).FirstOrDefault().RealName,
                        MAccount = Nullable<Guid>.Equals(l.MemberGid, null) ? "" : db.Member.Where(m => m.Gid == l.MemberGid).FirstOrDefault().Account,
                        l.MAddTime,
                        l.Number,
                        j.FirstOrDefault().Label,
                        LevelName = j.FirstOrDefault().Name,
                        AllMoney = db.Order.Where(o => o.MemberGid == l.Gid && o.PayStatus == 1 && o.Type==3 && o.PayTime>= MonthFirst && o.PayTime<=MonthLast).Select(o => o.Price).DefaultIfEmpty(0m).Sum(),
                        Stock = db.Stock.Where(s => s.MemberGid == l.Gid).Select(s => s.Number).DefaultIfEmpty(0).Sum()
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(paramJson["Type"].ToString()))
                {
                    b = b.Where(l => l.AllMoney > 0);
                }
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account.Contains(Account));
                }
                if (!string.IsNullOrEmpty(RealName))
                {
                    b = b.Where(l => l.RealName.Contains(RealName));
                }
                if (!string.IsNullOrEmpty(MAccount))
                {
                    b = b.Where(l => l.MAccount == MAccount);
                }
                if (!string.IsNullOrEmpty(paramJson["MID"].ToString()))
                {
                    int MID = int.Parse(paramJson["MID"].ToString());
                    b = b.Where(l => l.MID == MID);
                }
                if (Number != 0)
                {
                    b = b.Where(l => l.Number == Number);
                }
                if (Level != 0)
                {
                    b = b.Where(l => l.Level == Level);
                }
                if (CLLevel != 0)
                {
                    b = b.Where(l => l.CLLevel == CLLevel);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
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
        /// 下线列表
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public ActionResult MList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult M()
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
                Guid Gid = Guid.Parse(paramJson["Gid"].ToString());
                //查询所有等级
                List<Guid> list = new List<Guid>();
                list = Helper.MGidALL(Gid, db.Member.Where(l => l.MemberGid != null).ToList(), list);
                var b = db.Member.AsQueryable();
                string GList = string.Join(",", list.ToArray());
                b = b.Where(l => GList.Contains(l.Gid.ToString()));
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

        #region 管理员模块
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult LJShengAU()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
            {
                using (EFDB db = new EFDB())
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.LJSheng.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Account = b.Account;
                    ViewBag.PWD = b.PWD;
                    ViewBag.RealName = b.RealName;
                    ViewBag.Gender = b.Gender;
                    ViewBag.Jurisdiction = b.Jurisdiction;
                    ViewBag.ContactNumber = b.ContactNumber;
                    ViewBag.Picture = Help.LJSheng + b.Picture;
                }
            }
            else
            {
                ViewBag.Gender = "男";
                ViewBag.Jurisdiction = "管理员";
            }
            return View();
        }
        [HttpPost]
        public ActionResult LJShengAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                Data.LJSheng b;
                string Account = Request.Form["Account"];
                string PWD = Request.Form["PWD"];
                string RealName = Request.Form["RealName"];
                string Gender = Request.Form["Gender"];
                string Jurisdiction = Request.Form["Jurisdiction"];
                string ContactNumber = Request.Form["ContactNumber"];
                string Picture = Request.Form["Picture"];
                if (db.LJSheng.Where(l => l.Account == Account && l.Gid != Gid).Count() > 0)
                {
                    return Helper.WebRedirect("操作失败！", "history.go(-1);", "帐号已存在!");
                }
                else
                {
                    if (Gid == null)
                    {
                        b = new Data.LJSheng();
                        b.Gid = Guid.NewGuid();
                        b.AddTime = DateTime.Now;
                        b.Account = Account;
                        b.LoginIdentifier = "0000000000";
                    }
                    else
                    {
                        b = db.LJSheng.Where(l => l.Gid == Gid).FirstOrDefault();
                    }
                    b.RealName = RealName;
                    if (b.PWD != PWD)
                    {
                        b.PWD = MD5.GetMD5ljsheng(PWD);
                    }
                    b.Gender = Gender;
                    b.Jurisdiction = Jurisdiction;
                    b.ContactNumber = ContactNumber;
                    if (!string.IsNullOrEmpty(Picture))
                    {
                        b.Picture = Picture;
                    }
                    if (Gid == null)
                    {
                        db.LJSheng.Add(b);
                    }
                    if (db.SaveChanges() == 1)
                    {
                        return Helper.WebRedirect("操作成功！", "history.go(-1);", "恭喜你,操作成功!");
                    }
                    else
                    {
                        return Helper.WebRedirect("操作失败！", "history.go(-1);", "操作失败,请检查录入的数据!");
                    }
                }
            }
        }

        // 列表管理
        public ActionResult LJShengList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult LJSheng()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Account = paramJson["Account"].ToString();
                string RealName = paramJson["RealName"].ToString();
                var b = db.LJSheng.AsQueryable();
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account.Contains(Account));
                }
                if (!string.IsNullOrEmpty(RealName))
                {
                    b = b.Where(l => l.RealName.Contains(RealName));
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
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
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult LJShengDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    string file = System.Web.HttpContext.Current.Server.MapPath(Help.LJSheng + db.LJSheng.Where(l => l.Gid == Gid).FirstOrDefault().Picture);
                    if (db.LJSheng.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        if (System.IO.File.Exists(file))
                            System.IO.File.Delete(file);
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        /// <summary>
        /// 重设密码
        /// </summary>
        [HttpPost]
        public JsonResult LJShengPWD(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    string pwd = RandStr.CreateValidateNumber(6);
                    var b = db.LJSheng.Where(l => l.Gid == Gid).FirstOrDefault();
                    b.PWD = MD5.GetMD5ljsheng(pwd);
                    b.LoginIdentifier = LCommon.TimeToUNIX(DateTime.Now);
                    if (db.SaveChanges() == 1)
                    {
                        return Json(new AjaxResult("新密码为:" + pwd));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 分类模块
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult ClassifyAU()
        {
            using (EFDB db = new EFDB())
            {
                if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.Classify.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Name = b.Name;
                    ViewBag.Remarks = b.Remarks;
                    ViewBag.Sort = b.Sort;
                    ViewBag.Show = b.Show;
                    ViewBag.Project = b.Project;
                    ViewBag.Picture = Help.Classify + b.Picture;
                }
                else
                {
                    ViewBag.Sort = 1;
                    ViewBag.Show = 1;
                }
                var d = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault();
                return View(db.DictionariesList.Where(l => l.DGid == d.Gid).ToList());
            }
        }
        [HttpPost]
        public ActionResult ClassifyAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                Classify b;
                string Name = Request.Form["Name"];
                if (db.Classify.Where(l => l.Name == Name && l.Gid != Gid).Count() > 0)
                {
                    return Helper.WebRedirect("名称已存在！", "history.go(-1);", "名称已存在");
                }
                else
                {
                    if (Gid == null)
                    {
                        b = new Classify();
                        b.Gid = Guid.NewGuid();
                        b.AddTime = DateTime.Now;
                        b.Project = Int32.Parse(Request.Form["Project"]);
                    }
                    else
                    {
                        b = db.Classify.Where(l => l.Gid == Gid).FirstOrDefault();
                    }
                    b.Name = Name;
                    b.Remarks = Request.Form["Remarks"];
                    b.Sort = Int32.Parse(Request.Form["Sort"]);
                    b.Show = Int32.Parse(Request.Form["Show"]);
                    if (!string.IsNullOrEmpty(Request.Form["Picture"]))
                    {
                        b.Picture = Request.Form["Picture"];
                    }
                    if (Gid == null)
                    {
                        db.Classify.Add(b);
                    }
                    if (db.SaveChanges() == 1)
                    {
                        return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                    }
                    else
                    {
                        return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                    }
                }
            }
        }

        // 列表管理
        public ActionResult ClassifyList()
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault();
                return View(db.DictionariesList.Where(l => l.DGid == b.Gid).ToList());
            }
        }
        [HttpPost]
        public JsonResult Classify()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Name = paramJson["Name"].ToString();
                int Project = int.Parse(paramJson["Project"].ToString());
                var b = db.Classify.AsQueryable();
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }
                if (Project != 0)
                {
                    b = b.Where(l => l.Project == Project);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                Guid DGid = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault().Gid;
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Select(l => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Name,
                        l.Picture,
                        l.Show,
                        l.Sort,
                        l.Remarks,
                        l.Project,
                        ProjectName = l.Project == 0 ? "全部" : db.DictionariesList.Where(dl => dl.DGid == DGid && dl.Value == SqlFunctions.StringConvert((double)l.Project).Trim()).FirstOrDefault().Key
                    }).Take(pagesize).ToList()
                }));
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult ClassifyDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    if (db.Classify.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 分红模块
        // 列表管理
        public ActionResult StockRightList()
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault();
                return View(db.DictionariesList.Where(l => l.DGid == b.Gid).ToList());
            }
        }
        [HttpPost]
        public JsonResult StockRight()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Account = paramJson["Account"].ToString();
                int Project = int.Parse(paramJson["Project"].ToString());
                Guid DGid = db.Dictionaries.Where(d => d.DictionaryType == "ClassifyType").FirstOrDefault().Gid;
                var b = db.StockRight.GroupJoin(db.Order,
                    x => x.OrderGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        x.Gid,
                        x.AddTime,
                        x.MemberGid,
                        x.Number,
                        x.Project,
                        y.FirstOrDefault().Product,
                        y.FirstOrDefault().OrderNo,
                        y.FirstOrDefault().Price,
                        TypeName = db.DictionariesList.Where(l => l.DGid == DGid && l.Value == SqlFunctions.StringConvert((double)x.Project).Trim()).FirstOrDefault().Key,
                        db.Member.Where(l => l.Gid == x.MemberGid).FirstOrDefault().Account
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account.Contains(Account));
                }
                if (Project != 0)
                {
                    b = b.Where(l => l.Project == Project);
                }
                if (!string.IsNullOrEmpty(paramJson["MemberGid"].ToString()))
                {
                    Guid MemberGid = Guid.Parse(paramJson["MemberGid"].ToString());
                    b = b.Where(l => l.MemberGid == MemberGid);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总金额=" + b.Select(l => l.Price).DefaultIfEmpty(0m).Sum() + ",分红=" + b.Select(l => l.Number).DefaultIfEmpty(0m).Sum(),
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        #endregion

        #region 清空推荐
        /// <summary>
        /// 清空商品的推荐
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        [HttpPost]
        public JsonResult ProductShow()
        {
            using (EFDB db = new EFDB())
            {
                db.Product.Where(l => l.Show == 3).Update(l => new Product { Show = 1 });
                return Json(new AjaxResult("取消所有商品推荐成功"));
            }
        }
        #endregion

        #region 商品模块
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult ProductAU()
        {
            using (EFDB db = new EFDB())
            {
                if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.Product.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Name = b.Name;
                    ViewBag.Price = b.Price;
                    ViewBag.OriginalPrice = b.OriginalPrice;
                    ViewBag.BuyPrice = b.BuyPrice;
                    ViewBag.Number = b.Number;
                    ViewBag.Stock = b.Stock;
                    ViewBag.GiveStock = b.GiveStock;
                    ViewBag.Money = b.Money;
                    ViewBag.Integral = b.Integral;
                    ViewBag.StockRight = b.StockRight;
                    ViewBag.Type = b.Type;
                    ViewBag.Profile = b.Profile;
                    ViewBag.Content = b.Content;
                    ViewBag.Sort = b.Sort;
                    ViewBag.Show = b.Show;
                    ViewBag.Picture = Help.Product + b.Picture;
                    //ViewBag.GraphicDetails = b.GraphicDetails;
                    ViewBag.ClassifyGid = b.ClassifyGid;
                    ViewBag.Remarks = b.Remarks;
                    ViewBag.ExpressFee = b.ExpressFee;
                    ViewBag.Company = b.Company;
                    ViewBag.Brand = b.Brand;
                    ViewBag.Prefix = b.Prefix;
                    ViewBag.MPrice = b.MPrice;
                }
                else
                {
                    ViewBag.OriginalPrice = 0;
                    ViewBag.Number = 0;
                    ViewBag.Money = 0;
                    ViewBag.Integral = 0;
                    ViewBag.StockRight = 0;
                    ViewBag.Type = 1;
                    ViewBag.Sort = 1;
                    ViewBag.Show = 1;
                    ViewBag.MPrice = 0;
                    ViewBag.ClassifyGid = Guid.Parse("00000000-0000-0000-0000-000000000000");
                }
                int Project = Int32.Parse(Request.QueryString["Project"]);
                return View(db.Classify.Where(l => l.Project == Project).ToList());
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ProductAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                Product b;
                string Name = Request.Form["Name"];
                Guid ClassifyGid = Guid.Parse(Request.Form["ClassifyGid"]);
                if (db.Product.Where(l => l.Name == Name && l.Gid != Gid && l.ClassifyGid == ClassifyGid).Count() > 0)
                {
                    return Helper.WebRedirect("名称已存在！", "history.go(-1);", "名称已存在");
                }
                else
                {
                    if (Gid == null)
                    {
                        b = new Product();
                        b.Gid = Guid.NewGuid();
                        b.AddTime = DateTime.Now;
                    }
                    else
                    {
                        b = db.Product.Where(l => l.Gid == Gid).FirstOrDefault();
                    }
                    b.ClassifyGid = ClassifyGid;
                    b.Name = Name;
                    b.Price = decimal.Parse(Request.Form["Price"]);
                    b.OriginalPrice = decimal.Parse(Request.Form["OriginalPrice"]);
                    b.BuyPrice = decimal.Parse(Request.Form["BuyPrice"]);
                    b.Number = int.Parse(Request.Form["Number"]);
                    b.Stock = int.Parse(Request.Form["Stock"]);
                    b.GiveStock = int.Parse(Request.Form["GiveStock"]);
                    b.Money = decimal.Parse(Request.Form["Money"]);
                    b.Integral = decimal.Parse(Request.Form["Integral"]);
                    b.StockRight = decimal.Parse(Request.Form["StockRight"]);
                    b.Type = int.Parse(Request.Form["Type"]);
                    b.Profile = Request.Form["Profile"];
                    b.Content = Request.Form["Content"];
                    b.Remarks = Request.Form["Remarks"];
                    b.Show = Int32.Parse(Request.Form["Show"]);
                    b.Sort = Int32.Parse(Request.Form["Sort"]);
                    b.ExpressFee = Int32.Parse(Request.Form["ExpressFee"]);
                    b.Company = Request.Form["Company"];
                    b.Brand = Request.Form["Brand"];
                    b.Prefix = Request.Form["Prefix"];
                    b.MPrice= decimal.Parse(Request.Form["MPrice"]);
                    if (!string.IsNullOrEmpty(Request.Form["Picture"]))
                    {
                        b.Picture = Request.Form["Picture"];
                    }
                    //if (!string.IsNullOrEmpty(Request.Form["GraphicDetails"]))
                    //{
                    //    b.GraphicDetails = Request.Form["GraphicDetails"];
                    //}
                    if (Gid == null)
                    {
                        db.Product.Add(b);
                    }
                    if (db.SaveChanges() == 1)
                    {
                        return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                    }
                    else
                    {
                        return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                    }
                }
            }
        }

        // 列表管理
        public ActionResult ProductList()
        {
            using (EFDB db = new EFDB())
            {
                int Project = Int32.Parse(Request.QueryString["project"]);
                return View(db.Classify.Where(l => l.Project == Project).ToList());
            }
        }
        [HttpPost]
        public JsonResult Product()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Name = paramJson["Name"].ToString();
                int Project = Int32.Parse(paramJson["Project"].ToString());
                var b = db.Product.GroupJoin(db.Classify,
                    x => x.ClassifyGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        x.Gid,
                        x.AddTime,
                        x.Prefix,
                        x.Brand,
                        x.Name,
                        x.Picture,
                        x.Show,
                        x.Sort,
                        x.Price,
                        x.OriginalPrice,
                        x.BuyPrice,
                        x.ClassifyGid,
                        x.Number,
                        x.Stock,
                        x.Money,
                        x.Integral,
                        x.StockRight,
                        x.MPrice,
                        ClassifyName = y.FirstOrDefault().Name,
                        y.FirstOrDefault().Project
                    }).Where(l => l.Project == Project).AsQueryable();
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }
                if (paramJson["ClassifyGid"].ToString() != "0" || !string.IsNullOrEmpty(Request.QueryString["ClassifyGid"]))
                {
                    Guid ClassifyGid = paramJson["ClassifyGid"].ToString() != "0"?Guid.Parse(paramJson["ClassifyGid"].ToString()): Guid.Parse(Request.QueryString["ClassifyGid"]);
                    b = b.Where(l => l.ClassifyGid == ClassifyGid);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
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
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult ProductDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    string file = System.Web.HttpContext.Current.Server.MapPath(Help.Product + db.Product.Where(l => l.Gid == Gid).FirstOrDefault().Picture);
                    if (db.Product.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        if (System.IO.File.Exists(file))
                            System.IO.File.Delete(file);
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 图片上传
        /// <summary>
        /// layui图片上传
        /// </summary>
        [HttpPost]
        public JsonResult UploadPicture(string Path)
        {
            HttpPostedFileBase Picture = Request.Files["file"];
            string FileName = "";
            if (Picture != null)
            {
                FileName = Helper.UploadFiles(Path, Picture);
            }
            return Json(new { result = 200, src = Path + FileName, title = FileName });
        }
        #endregion

        #region 评论管理
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult CommentAU()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
            {
                using (EFDB db = new EFDB())
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.Comment.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Content = b.Content;
                    ViewBag.Reply = b.Reply;
                    ViewBag.Sort = b.Sort;
                    ViewBag.Show = b.Show;
                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult CommentAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Comment.Where(l => l.Gid == Gid).FirstOrDefault();
                b.Content = Request.Form["Content"];
                string Reply = Request.Form["Reply"];
                if (!string.IsNullOrEmpty(Request.Form["Reply"]))
                {
                    b.Reply = Request.Form["Reply"];
                    b.ReplyTime = DateTime.Now;
                }
                b.Show = int.Parse(Request.Form["Show"]);
                b.Sort = int.Parse(Request.Form["Sort"]);
                if (db.SaveChanges() == 1)
                {
                    return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                }
                else
                {
                    return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                }
            }
        }
        public ActionResult CommentList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Comment()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                var b = db.Comment.Select(l => new
                {
                    l.Gid,
                    l.AddTime,
                    l.MemberGid,
                    l.ProductGid,
                    l.Content,
                    l.Reply,
                    l.ReplyTime,
                    l.Show,
                    Member = db.Member.Where(m => m.Gid == l.MemberGid).FirstOrDefault().NickName,
                    Product = db.Product.Where(p => p.Gid == l.ProductGid).FirstOrDefault().Name
                }).AsQueryable();
                string Content = paramJson["Content"].ToString();
                string Reply = paramJson["Reply"].ToString();
                string Member = paramJson["Member"].ToString();
                string Product = paramJson["Product"].ToString();
                if (!string.IsNullOrEmpty(Content))
                {
                    b = b.Where(l => l.Content.Contains(Content));
                }
                if (!string.IsNullOrEmpty(Reply))
                {
                    b = b.Where(l => l.Reply.Contains(Reply));
                }
                if (!string.IsNullOrEmpty(Member))
                {
                    b = b.Where(l => l.Member.Contains(Member));
                }
                if (!string.IsNullOrEmpty(Product))
                {
                    b = b.Where(l => l.Product.Contains(Product));
                }
                if (!string.IsNullOrEmpty(paramJson["MemberGid"].ToString()))
                {
                    Guid MemberGid = Guid.Parse(paramJson["MemberGid"].ToString());
                    b = b.Where(l => l.MemberGid == MemberGid);
                }
                if (!string.IsNullOrEmpty(paramJson["ProductGid"].ToString()))
                {
                    Guid ProductGid = Guid.Parse(paramJson["ProductGid"].ToString());
                    b = b.Where(l => l.ProductGid == ProductGid);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
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
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult CommentDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    if (db.Comment.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 新闻资讯模块
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult NewsAU()
        {
            using (EFDB db = new EFDB())
            {
                if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.News.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Title = b.Title;
                    ViewBag.Profile = b.Profile;
                    ViewBag.Content = b.Content;
                    ViewBag.Sort = b.Sort;
                    ViewBag.Show = b.Show;
                    ViewBag.Picture = Help.News + b.Picture;
                    //ViewBag.GraphicDetails = b.GraphicDetails;
                    ViewBag.Url = b.Url;
                    ViewBag.Author = b.Author;
                    ViewBag.Number = b.Number;
                    ViewBag.Project = b.Project;
                }
                else
                {
                    ViewBag.Number = 1;
                    ViewBag.Sort = 1;
                    ViewBag.Show = 1;
                    ViewBag.Project = 1;
                }
                var d = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault();
                return View(db.DictionariesList.Where(l => l.DGid == d.Gid).ToList());
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NewsAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                News b;
                string Title = Request.Form["Title"];
                if (db.News.Where(l => l.Title == Title && l.Gid != Gid).Count() > 0)
                {
                    return Helper.WebRedirect("名称已存在！", "history.go(-1);", "名称已存在");
                }
                else
                {
                    if (Gid == null)
                    {
                        b = new News();
                        b.Gid = Guid.NewGuid();
                        b.AddTime = DateTime.Now;
                    }
                    else
                    {
                        b = db.News.Where(l => l.Gid == Gid).FirstOrDefault();
                    }
                    b.Project = Int32.Parse(Request.Form["Project"]);
                    b.Title = Title;
                    b.Number = int.Parse(Request.Form["Number"]);
                    b.Profile = Request.Form["Profile"];
                    b.Content = Request.Form["Content"];
                    b.Url = Request.Form["Url"];
                    b.Author = Request.Form["Author"];
                    b.Show = Int32.Parse(Request.Form["Show"]);
                    b.Sort = Int32.Parse(Request.Form["Sort"]); ;
                    if (!string.IsNullOrEmpty(Request.Form["Picture"]))
                    {
                        b.Picture = Request.Form["Picture"];
                    }
                    //if (!string.IsNullOrEmpty(Request.Form["GraphicDetails"]))
                    //{
                    //    b.GraphicDetails = Request.Form["GraphicDetails"];
                    //}
                    if (Gid == null)
                    {
                        db.News.Add(b);
                    }
                    if (db.SaveChanges() == 1)
                    {
                        return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                    }
                    else
                    {
                        return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                    }
                }
            }
        }

        // 列表管理
        public ActionResult NewsList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult News()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Title = paramJson["Title"].ToString();
                var b = db.News.AsQueryable();
                if (!string.IsNullOrEmpty(Title))
                {
                    b = b.Where(l => l.Title.Contains(Title));
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
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
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult NewsDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    string file = System.Web.HttpContext.Current.Server.MapPath(Help.News + db.News.Where(l => l.Gid == Gid).FirstOrDefault().Picture);
                    if (db.News.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        if (System.IO.File.Exists(file))
                            System.IO.File.Delete(file);
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 广告模块
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult ADAU()
        {
            using (EFDB db = new EFDB())
            {
                if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.AD.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Title = b.Title;
                    ViewBag.Profile = b.Profile;
                    ViewBag.Remarks = b.Remarks;
                    ViewBag.Sort = b.Sort;
                    ViewBag.Show = b.Show;
                    ViewBag.Picture = Help.AD + b.Picture;
                    ViewBag.Url = b.Url;
                    ViewBag.Number = b.Number;
                    ViewBag.Project = b.Project;
                }
                else
                {
                    ViewBag.Number = 1;
                    ViewBag.Sort = 1;
                    ViewBag.Show = 1;
                    ViewBag.Project = 1;
                }
                var d = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault();
                return View(db.DictionariesList.Where(l => l.DGid == d.Gid).ToList());
            }
        }
        [HttpPost]
        public ActionResult ADAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                AD b;
                if (Gid == null)
                {
                    b = new AD();
                    b.Gid = Guid.NewGuid();
                    b.AddTime = DateTime.Now;
                    b.ExpireTime = DateTime.Now;
                }
                else
                {
                    b = db.AD.Where(l => l.Gid == Gid).FirstOrDefault();
                }
                b.Project = Int32.Parse(Request.Form["Project"]);
                b.Title = Request.Form["Title"]; ;
                b.Number = int.Parse(Request.Form["Number"]);
                b.Profile = Request.Form["Profile"];
                b.Url = Request.Form["Url"];
                b.Remarks = Request.Form["Remarks"];
                b.Show = Int32.Parse(Request.Form["Show"]);
                b.Sort = Int32.Parse(Request.Form["Sort"]); ;
                if (!string.IsNullOrEmpty(Request.Form["Picture"]))
                {
                    b.Picture = Request.Form["Picture"];
                }
                if (Gid == null)
                {
                    db.AD.Add(b);
                }
                if (db.SaveChanges() == 1)
                {
                    return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                }
                else
                {
                    return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                }
            }
        }

        // 列表管理
        public ActionResult ADList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult AD()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Title = paramJson["Title"].ToString();
                var b = db.AD.AsQueryable();
                if (!string.IsNullOrEmpty(Title))
                {
                    b = b.Where(l => l.Title.Contains(Title));
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
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
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult ADDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    string file = System.Web.HttpContext.Current.Server.MapPath(Help.AD + db.AD.Where(l => l.Gid == Gid).FirstOrDefault().Picture);
                    if (db.AD.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        if (System.IO.File.Exists(file))
                            System.IO.File.Delete(file);
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 快递相关
        /// <summary>
        /// 更新快递公司
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        [HttpPost]
        public JsonResult ExpressList()
        {
            using (EFDB db = new EFDB())
            {
                //读取json文件  
                //string jsonPath = Server.MapPath("/Express.json");
                //string jsonstr = string.Empty;
                //using (FileStream fs = new FileStream(jsonPath, FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite))
                //{
                //    using (StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("gb2312")))
                //    {
                //        jsonstr = sr.ReadToEnd().ToString();
                //    }
                //}
                string KD = KDAPI.request();
                if (KD.Length > 100)
                {
                    JObject paramJson = JsonConvert.DeserializeObject(KD) as JObject;
                    JArray json = (JArray)JsonConvert.DeserializeObject(paramJson["result"].ToString());
                    if (json.Count() > 0)
                    {
                        db.Express.Delete();
                        foreach (var j in json)
                        {
                            var b = new Express();
                            b.Gid = Guid.NewGuid();
                            b.AddTime = DateTime.Now;
                            b.Sort = 1;
                            b.Show = 1;
                            b.Name = j["Name"].ToString();
                            b.Type = j["Type"].ToString();
                            b.Letter = j["Letter"].ToString();
                            b.Tel = j["Tel"].ToString().Length >= 50 ? "" : j["Tel"].ToString();
                            b.Number = j["Number"].ToString();
                            db.Express.Add(b);
                        }
                        if (db.SaveChanges() > 0)
                        {
                            return Json(new AjaxResult("更新成功"));
                        }
                    }
                }
                return Json(new AjaxResult(300, "失败,请查看快递接口次数是否已经用完=" + KD));
            }
        }
        #endregion

        #region 城市管理
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult OpenCityList()
        {
            string City = Request.Form["City"];
            if (City != "请选择城市" && !string.IsNullOrEmpty(City))
            {
                using (EFDB db = new EFDB())
                {
                    OpenCity b;
                    if (db.OpenCity.Where(l => l.City == City).Count() > 0)
                    {
                        return Helper.WebRedirect("已存在！", "history.go(-1);", "已存在");
                    }
                    else
                    {
                        b = new OpenCity();
                        b.Gid = Guid.NewGuid();
                        b.AddTime = DateTime.Now;
                        b.Province = Request.Form["Province"];
                        b.City = City;
                        b.Sort = Int32.Parse(Request.Form["Sort"]);
                        b.Show = Int32.Parse(Request.Form["Show"]);
                        db.OpenCity.Add(b);
                        if (db.SaveChanges() == 1)
                        {
                            return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                        }
                        else
                        {
                            return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                        }
                    }
                }
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult OpenCity()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Province = paramJson["Province"].ToString();
                string City = paramJson["City"].ToString();
                var b = db.OpenCity.AsQueryable();
                if (!string.IsNullOrEmpty(Province) && Province != "请选择省份")
                {
                    b = b.Where(l => l.Province.Contains(Province));
                }
                if (!string.IsNullOrEmpty(City) && City != "请选择城市")
                {
                    b = b.Where(l => l.City.Contains(City));
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderBy(l => l.Sort).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult OpenCityDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    if (db.OpenCity.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 订单管理
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult OrderAU()
        {
            using (EFDB db = new EFDB())
            {
                if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.Order.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.OrderNo = b.OrderNo;
                    ViewBag.TradeNo = b.TradeNo;
                    ViewBag.PayPrice = b.PayPrice;
                    ViewBag.Remarks = b.Remarks;
                    ViewBag.ExpressStatus = b.ExpressStatus;
                    ViewBag.Express = b.Express;
                    ViewBag.ExpressNumber = b.ExpressNumber;
                    ViewBag.Address = b.Address;
                    ViewBag.RealName = b.RealName;
                    ViewBag.ContactNumber = b.ContactNumber;
                    ViewBag.Product = b.Product;
                }
                else
                {
                    ViewBag.Sort = 1;
                }
                return View(db.Express.Where(l => l.Show == 1).OrderBy(l => l.Sort).ToList());
            }
        }

        [HttpPost]
        public ActionResult OrderAU(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Order.Where(l => l.Gid == Gid).FirstOrDefault();
                b.Remarks = Request.Form["Remarks"];
                b.Express = Request.Form["Express"];
                b.ExpressNumber = Request.Form["ExpressNumber"];
                b.Address = Request.Form["Address"];
                b.RealName = Request.Form["RealName"];
                b.ContactNumber = Request.Form["ContactNumber"];
                b.ExpressStatus = Int32.Parse(Request.Form["ExpressStatus"]);
                b.Status = 2;
                if (db.SaveChanges() == 1)
                {
                    return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                }
                else
                {
                    return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                }
            }
        }

        public ActionResult OrderList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Order()
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
                var b = db.Order.GroupJoin(db.Member,
                    x => x.MemberGid,
                    y => y.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.MemberGid,
                        l.Product,
                        l.OrderNo,
                        l.TradeNo,
                        l.PayType,
                        l.PayStatus,
                        l.TotalPrice,
                        l.PayPrice,
                        l.Price,
                        l.ExpressStatus,
                        l.Express,
                        l.ExpressNumber,
                        l.PayTime,
                        l.Project,
                        l.Remarks,
                        l.Status,
                        l.Profit,
                        l.Type,
                        l.CLLevel,
                        l.ShopGid,
                        l.RobTime,
                        l.Money,
                        l.Integral,
                        l.Voucher,
                        j.FirstOrDefault().Account,
                        j.FirstOrDefault().RealName
                    }).GroupJoin(db.Level,
                    x => x.CLLevel,
                    y => y.LV,
                    (l, y) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.MemberGid,
                        l.Product,
                        l.OrderNo,
                        l.TradeNo,
                        l.PayType,
                        l.PayStatus,
                        l.TotalPrice,
                        l.PayPrice,
                        l.Price,
                        l.ExpressStatus,
                        l.Express,
                        l.ExpressNumber,
                        l.PayTime,
                        l.Project,
                        l.Remarks,
                        l.Status,
                        l.Profit,
                        l.Type,
                        l.CLLevel,
                        l.Account,
                        l.RealName,
                        l.ShopGid,
                        l.Money,
                        l.Integral,
                        l.Voucher,
                        LevelName = y.FirstOrDefault().Name,
                        y.FirstOrDefault().Label,
                        Shop = l.Type == 3 ? "公司发货" : db.Member.Where(m => m.Gid == l.ShopGid).FirstOrDefault().Account,
                        Stock = db.OrderDetails.Where(o => o.OrderGid == l.Gid).Select(o => o.Number).DefaultIfEmpty(0).Sum(),
                        AllMoney = db.MoneyRecord.Where(mr => mr.OrderGid == l.Gid).Select(mr => mr.Money).DefaultIfEmpty(0m).Sum(),
                        AllIntegral = db.MoneyRecord.Where(mr => mr.OrderGid == l.Gid).Select(mr => mr.Integral).DefaultIfEmpty(0m).Sum()
                    }).ToList().AsQueryable();
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string OrderNo = paramJson["OrderNo"].ToString();
                string TradeNo = paramJson["TradeNo"].ToString();
                string Account = paramJson["Account"].ToString();
                string Product = paramJson["Product"].ToString();
                //发货记录
                if (!string.IsNullOrEmpty(paramJson["ShopGid"].ToString()))
                {
                    Guid ShopGid = Guid.Parse(paramJson["ShopGid"].ToString());
                    b = b.Where(l => l.ShopGid == ShopGid);
                }
                //进货,业绩会员订单
                if (!string.IsNullOrEmpty(paramJson["MemberGid"].ToString()))
                {
                    Guid MemberGid = Guid.Parse(paramJson["MemberGid"].ToString());
                    b = b.Where(l => l.MemberGid == MemberGid && l.ShopGid != MemberGid);
                }
                if (!string.IsNullOrEmpty(paramJson["Project"].ToString()))
                {
                    int Project = int.Parse(paramJson["Project"].ToString());
                    b = b.Where(l => l.Project == Project);
                }
                if (!string.IsNullOrEmpty(OrderNo))
                {
                    b = b.Where(l => l.OrderNo == OrderNo);
                }
                if (!string.IsNullOrEmpty(paramJson["TradeNo"].ToString()))
                {
                    b = b.Where(l => l.TradeNo == TradeNo);
                }
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account == Account);
                }
                if (!string.IsNullOrEmpty(Product))
                {
                    b = b.Where(l => l.Product == Product);
                }
                if (paramJson["PayType"].ToString() != "0")
                {
                    int PayType = int.Parse(paramJson["PayType"].ToString());
                    b = b.Where(l => l.PayType == PayType);
                }
                if (paramJson["PayStatus"].ToString() != "0")
                {
                    int PayStatus = int.Parse(paramJson["PayStatus"].ToString());
                    b = b.Where(l => l.PayStatus == PayStatus);
                }
                if (paramJson["ExpressStatus"].ToString() != "0")
                {
                    int ExpressStatus = int.Parse(paramJson["ExpressStatus"].ToString());
                    b = b.Where(l => l.ExpressStatus == ExpressStatus);
                }
                if (paramJson["Status"].ToString() != "0")
                {
                    int Status = int.Parse(paramJson["Status"].ToString());
                    b = b.Where(l => l.Status == Status);
                }
                if (paramJson["Type"].ToString() != "0")
                {
                    int Type = int.Parse(paramJson["Type"].ToString());
                    b = b.Where(l => l.Type == Type);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.PayTime >= st && l.PayTime <= et);
                }
                //按月份查询业绩
                string Year = paramJson["Year"].ToString();
                string Month = paramJson["Month"].ToString();
                if (Year!="0" && Month!="0")
                {
                    DateTime MonthFirst = DTime.FirstDayOfMonth(DateTime.Parse(Year + "-" + Month + "-" + "01"));
                    DateTime MonthLast = DTime.LastDayOfMonth(DateTime.Parse(Year + "-" + Month + "-" + "01 23:59:59"));
                    b = b.Where(l => l.PayTime >= MonthFirst && l.AddTime <= MonthLast);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总额=" + b.Select(l => l.TotalPrice).DefaultIfEmpty(0m).Sum(),
                    count = b.Count(),
                    pageindex,
                    list = b.ToList().OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize)
                }));
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult OrderDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    if (db.Order.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        db.OrderDetails.Where(l => l.OrderGid == Gid).Delete();
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }

        /// <summary>
        /// 商品详情
        /// </summary>
        public ActionResult OrderDetailsList()
        {
            return View();
        }
        [HttpPost]
        public ActionResult OrderDetails()
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
                Guid Gid = Guid.Parse(paramJson["Gid"].ToString());
                var b = db.OrderDetails.Where(l => l.OrderGid == Gid).GroupJoin(db.Product,
                    x => x.ProductGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        x.Price,
                        x.Number,
                        x.Remarks,
                        x.AddTime,
                        y.FirstOrDefault().Name
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

        #region 财务报表
        /// <summary>
        /// 报表
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public ActionResult ReportForm(string STime, string ETime, int Project = 0 )
        {
            //时间查询
            DateTime? st = DateTime.Now;
            DateTime? et = DateTime.Now;
            if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
            {
                if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                {
                    st = et = DateTime.Parse(STime);
                }
                else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                {
                    st = et = DateTime.Parse(ETime);
                }
                else
                {
                    st = DateTime.Parse(STime);
                    et = DateTime.Parse(ETime);
                }
            }
            using (EFDB db = new EFDB())
            {
                //彩链订单统计
                {
                    var order = db.Order.AsQueryable();
                    if (order.Count() > 0)
                    {
                        ViewBag.norder = order.Where(l => l.PayStatus == 2).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.order = order.Where(l=> l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.alipay = order.Where(l => l.PayType == 1 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.wxpay = order.Where(l => l.PayType == 2 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.rmb = order.Where(l => l.PayType == 3 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.integral = order.Where(l => l.PayType == 5 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                    }
                    order = order.Where(l => l.AddTime >= st && l.AddTime <= et);
                    if (order.Count() > 0)
                    {
                        ViewBag.nordertime = order.Where(l => l.PayStatus == 2).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.ordertime = order.Where(l => l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.alipaytime = order.Where(l => l.PayType == 1 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.wxpaytime = order.Where(l => l.PayType == 2 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.rmbtime = order.Where(l => l.PayType == 3 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.integraltime = order.Where(l => l.PayType == 5 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                    }
                    //提现
                    var w = db.Withdrawals.Where(l => l.State == 2).AsQueryable();
                    if (w.Count() > 0)
                    {
                        ViewBag.bank = w.Select(l => l.Money).DefaultIfEmpty(0m).Sum();
                    }
                    w = w.Where(l => l.AddTime >= st && l.AddTime <= et);
                    if (w.Count() > 0)
                    {
                        ViewBag.banktime = w.Select(l => l.Money).DefaultIfEmpty(0m).Sum();
                    }
                }
                //商城订单统计
                {
                    var shoporder = db.ShopOrder.AsQueryable();
                    if (shoporder.Count() > 0)
                    {
                        ViewBag.nshoporder = shoporder.Where(l => l.PayStatus == 2).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.shoporder = shoporder.Where(l => l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.shopalipay = shoporder.Where(l => l.PayType == 1 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.shopwxpay = shoporder.Where(l => l.PayType == 2 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.shoprmb = shoporder.Where(l => l.PayType == 3 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.shopintegral = shoporder.Where(l => l.PayType == 5 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                    }
                    shoporder = shoporder.Where(l => l.AddTime >= st && l.AddTime <= et);
                    if (shoporder.Count() > 0)
                    {
                        ViewBag.nshopordertime = shoporder.Where(l => l.PayStatus == 2).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.shopordertime = shoporder.Where(l => l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.shopalipaytime = shoporder.Where(l => l.PayType == 1 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.shopwxpaytime = shoporder.Where(l => l.PayType == 2 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.shoprmbtime = shoporder.Where(l => l.PayType == 3 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                        ViewBag.shopintegraltime = shoporder.Where(l => l.PayType == 5 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                    }
                    //提现
                    var sw = db.ShopWithdrawals.Where(l => l.State == 2).AsQueryable();
                    if (sw.Count() > 0)
                    {
                        ViewBag.shopbank = sw.Select(l => l.Money).DefaultIfEmpty(0m).Sum();
                    }
                    sw = sw.Where(l => l.AddTime >= st && l.AddTime <= et);
                    if (sw.Count() > 0)
                    {
                        ViewBag.shopbanktime = sw.Select(l => l.Money).DefaultIfEmpty(0m).Sum();
                    }
                    //团队积分
                    var team = db.ShopRecord.Where(l => l.Type >3).AsQueryable();
                    if (team.Count() > 0)
                    {
                        ViewBag.team = team.Select(l => l.TIntegral).DefaultIfEmpty(0m).Sum();
                    }
                    team = team.Where(l => l.AddTime >= st && l.AddTime <= et);
                    if (team.Count() > 0)
                    {
                        ViewBag.teamtime = team.Select(l => l.TIntegral).DefaultIfEmpty(0m).Sum();
                    }
                    //团队积分冻结
                    var mteam = db.ShopRecord.Where(l => l.Type ==2).AsQueryable();
                    if (mteam.Count() > 0)
                    {
                        ViewBag.mteam = mteam.Select(l => l.TIntegral).DefaultIfEmpty(0m).Sum();
                    }
                    mteam = mteam.Where(l => l.AddTime >= st && l.AddTime <= et);
                    if (mteam.Count() > 0)
                    {
                        ViewBag.mteamtime = mteam.Select(l => l.TIntegral).DefaultIfEmpty(0m).Sum();
                    }
                }
                //会员统计
                {
                    var member = db.Member.AsQueryable();
                    if (member.Count() > 0)
                    {
                        ViewBag.member = member.Count();
                        ViewBag.lv = member.Where(l => l.Level == 11).Count();
                        ViewBag.lv22 = member.Where(l => l.CLLevel == 22).Count();
                        ViewBag.lv23 = member.Where(l => l.CLLevel == 23).Count();
                        ViewBag.lv24 = member.Where(l => l.CLLevel == 24).Count();
                        ViewBag.lv25 = member.Where(l => l.CLLevel == 25).Count();
                        ViewBag.lv26 = member.Where(l => l.CLLevel == 26).Count();
                    }
                    member = member.Where(l => l.AddTime >= st && l.AddTime <= et);
                    if (member.Count() > 0)
                    {
                        ViewBag.membertime = member.Count();
                        ViewBag.lvtime = member.Where(l => l.Level == 11).Count();
                        ViewBag.lv22time = member.Where(l => l.CLLevel == 22).Count();
                        ViewBag.lv23time = member.Where(l => l.CLLevel == 23).Count();
                        ViewBag.lv24time = member.Where(l => l.CLLevel == 24).Count();
                        ViewBag.lv25time = member.Where(l => l.CLLevel == 25).Count();
                        ViewBag.lv26time = member.Where(l => l.CLLevel == 26).Count();
                    }
                }
                var d = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault();
                return View(db.DictionariesList.Where(l => l.DGid == d.Gid).ToList());
            }
        }

        /// <summary>
        /// 每日分红
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public ActionResult ReportList()
        {
            using (EFDB db = new EFDB())
            {
                var d = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault();
                return View(db.DictionariesList.Where(l => l.DGid == d.Gid).ToList());
            }
        }
        [HttpPost]
        public ActionResult Report()
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
                string STime = paramJson["STime"].ToString();
                int Project = int.Parse(paramJson["Project"].ToString());
                Guid DGid = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault().Gid;
                var b = db.Report.AsQueryable();
                if (Project != 0)
                {
                    b = b.Where(l => l.Project == Project);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime))
                {
                    DateTime Date = DateTime.Parse(STime);
                    b = b.Where(l => SqlFunctions.DateDiff("day", l.RTime, Date) == 0);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总金额=" + b.Where(l=>l.Project==0).Select(l => l.OrderPrice).DefaultIfEmpty(0m).Sum() + ",当前查询总利润=" + b.Where(l => l.Project == 0).Select(l => l.ProfitPrice).DefaultIfEmpty(0m).Sum(),
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).Select(l => new
                    {
                        l.Gid,
                        l.RTime,
                        l.AddTime,
                        l.Differential,
                        l.Recommendation,
                        l.SameLevel,
                        l.SDifferential,
                        l.SRecommendation,
                        l.SSameLevel,
                        l.OrderPrice,
                        l.ProfitPrice,
                        l.Remarks,
                        Money = l.Project == 0 ? db.ReportList.GroupJoin(db.Report,
                                x => x.ReportGid,
                                y => y.Gid,
                                (x, y) => new
                                {
                                    x.Money,
                                    y.FirstOrDefault().RTime
                                }).Where(rl => rl.RTime == l.RTime).Select(rl => rl.Money).DefaultIfEmpty(0m).Sum() : db.ReportList.Where(rl => rl.ReportGid == l.Gid).Select(rl => rl.Money).DefaultIfEmpty(0m).Sum(),
                        Integral = l.Project == 0 ? db.ReportList.GroupJoin(db.Report,
                                x => x.ReportGid,
                                y => y.Gid,
                                (x, y) => new
                                {
                                    x.Integral,
                                    y.FirstOrDefault().RTime
                                }).Where(rl => rl.RTime == l.RTime).Select(rl => rl.Integral).DefaultIfEmpty(0m).Sum() : db.ReportList.Where(rl => rl.ReportGid == l.Gid).Select(rl => rl.Integral).DefaultIfEmpty(0m).Sum(),
                        ProjectName = l.Project == 0 ? "分红" : db.DictionariesList.Where(dl => dl.DGid == DGid && dl.Value == SqlFunctions.StringConvert((double)l.Project).Trim()).FirstOrDefault().Key
                    }).ToList()
                }));
            }
        }

        // 列表管理
        //同时支持Get和Post
        //[AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult MReportList()
        {
            using (EFDB db = new EFDB())
            {
                var d = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault();
                ViewBag.Project = db.DictionariesList.Where(l => l.DGid == d.Gid).ToList();
                var lv = db.Level.OrderBy(l => l.LV);
                ViewBag.Level = lv.Where(l => l.Project == 1).ToList();
                ViewBag.CLLevel = lv.Where(l => l.Project == 2).ToList();
                return View();
            }
        }
        [HttpPost]
        public JsonResult MReport()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Account = paramJson["Account"].ToString();
                string RealName = paramJson["RealName"].ToString();
                int Level = int.Parse(paramJson["Level"].ToString());
                int CLLevel = int.Parse(paramJson["CLLevel"].ToString());
                int Type = int.Parse(paramJson["Type"].ToString());
                int Project = int.Parse(paramJson["Level"].ToString());
                int State = int.Parse(paramJson["State"].ToString());
                string ReportGid = paramJson["Gid"].ToString();
                var b = db.ReportList.GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.ReportGid,
                        l.State,
                        l.Type,
                        TypeName = db.Level.Where(lv => lv.LV == l.Type).FirstOrDefault().Name,
                        l.Remarks,
                        l.Money,
                        l.Integral,
                        l.Number,
                        j.FirstOrDefault().Account,
                        j.FirstOrDefault().RealName,
                        j.FirstOrDefault().Level,
                        j.FirstOrDefault().CLLevel
                    }).GroupJoin(db.Level,
                    l => l.Level,
                    j => j.LV,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.ReportGid,
                        l.State,
                        l.Type,
                        l.TypeName,
                        l.Remarks,
                        l.Money,
                        l.Integral,
                        l.Number,
                        l.Account,
                        l.RealName,
                        l.Level,
                        l.CLLevel,
                        LevelName = j.FirstOrDefault().Name,
                        j.FirstOrDefault().Label
                    }).GroupJoin(db.Report,
                    l => l.ReportGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.ReportGid,
                        l.State,
                        l.Type,
                        l.TypeName,
                        l.Remarks,
                        l.Money,
                        l.Integral,
                        l.Number,
                        l.Account,
                        l.RealName,
                        l.Level,
                        l.LevelName,
                        l.Label,
                        l.CLLevel,
                        j.FirstOrDefault().Project,
                        j.FirstOrDefault().RTime
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(ReportGid))
                {
                    Guid Gid = Guid.Parse(ReportGid);
                    b = b.Where(l => l.ReportGid== Gid);
                }
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account.Contains(Account));
                }
                if (!string.IsNullOrEmpty(RealName))
                {
                    b = b.Where(l => l.RealName.Contains(RealName));
                }
                if (Level != 0)
                {
                    b = b.Where(l => l.Level == Level);
                }
                if (CLLevel != 0)
                {
                    b = b.Where(l => l.CLLevel == CLLevel);
                }
                if (Project != 0)
                {
                    b = b.Where(l => l.Project == Project);
                }
                if (Type != 0)
                {
                    b = b.Where(l => l.Type == Type);
                }
                if (State != 0)
                {
                    b = b.Where(l => l.State == State);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.RTime >= st && l.RTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总积分=" + b.Select(l => l.Money).DefaultIfEmpty(0m).Sum() + ",购物积分=" + b.Select(l => l.Integral).DefaultIfEmpty(0m).Sum(),
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 给用户分红
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        [HttpPost]
        public JsonResult Bonus(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                if (db.ReportList.Where(l => l.Gid == Gid && l.State == 1).Update(l => new ReportList { State = 2 }) == 1)
                {
                    var b = db.ReportList.Where(l => l.Gid == Gid).GroupJoin(db.Report,
                        l => l.ReportGid,
                        j => j.Gid,
                        (l, j) => new
                        {
                            l.MemberGid,
                            l.Money,
                            l.Integral,
                            l.Type,
                            j.FirstOrDefault().RTime,
                            j.FirstOrDefault().Project
                        }).FirstOrDefault();
                    if (b != null)
                    {
                        if (Helper.MoneyRecordAdd(null, b.MemberGid, b.Money, b.Integral, b.Type < 10 ? 8 : 9, "项目=" + b.Project.ToString() + ",日期=" + b.RTime.ToString().Split(' ')[0]) != null)
                        {
                            return Json(new AjaxResult("用户分红成功"));
                        }
                        else
                        {
                            db.ReportList.Where(l => l.Gid == Gid).Update(l => new ReportList { State = 1 });
                            LogManager.WriteLog("审核分红失败", "状态成功,资金增加失败,Gid=" + Gid.ToString());
                        }
                    }
                }
                else
                {
                    LogManager.WriteLog("审核分红失败", "更新分红状态失败,Gid=" + Gid.ToString());
                }
                return Json(new AjaxResult(300,"用户分红失败"));
            }
        }

        /// <summary>
        /// 每日分红日历
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public ActionResult ReportCalendar()
        {
            return View();
        }

        /// <summary>
        /// 每日分红列表
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        [HttpPost]
        public JsonResult PReport()
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Report.Select(l => new
                {
                    l.Gid,
                    l.RTime,
                    l.OrderPrice,
                    l.ProfitPrice,
                    l.Recommendation,
                    l.Differential,
                    l.SameLevel
                }).ToList();
                //报表开始日期
                DateTime start = b.OrderBy(l => l.RTime).FirstOrDefault().RTime.Date;
                //报表结束日期
                DateTime end = DateTime.Now.Date;
                List<object> list = new List<object>();
                for (int i = 0; i < 88888; i++)
                {
                    if (start < end)
                    {
                        var thisDate = b.Where(l => l.RTime == start).FirstOrDefault();
                        if (thisDate != null)
                        {
                            list.Add(new { allDay = true, data = thisDate.Gid, desc = "推荐(" + thisDate.Recommendation.ToString() + "),级差(" + thisDate.Differential.ToString() + "),平级(" + thisDate.SameLevel.ToString() + ")", title = "订单:" + thisDate.OrderPrice.ToString() + "(利润:" + thisDate.ProfitPrice.ToString() + ")", start = start.ToString(), end = start.ToString() });
                            list.Add(new { allDay = true, data = thisDate.Gid, desc = "查看详情报表", title = "查看明细", start = start.ToString(), end = start.ToString() });
                        }
                        else
                        {
                            list.Add(new { allDay = true, data = 0, desc = "单击开始统计", title = "统计分红", start = start.ToString(), end = start.ToString() });
                        }
                    }
                    else
                    {
                        break;
                    }
                    start = start.AddDays(1);
                }
                return Json(new AjaxResult(list));
            }
        }

        /// <summary>
        /// 每日分红统计
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        [HttpPost]
        public JsonResult SReport(DateTime Date)
        {
            using (EFDB db = new EFDB())
            {
                //资金记录
                var mr = db.MoneyRecord.Where(l => l.Type >= 5 && l.Type <= 7).GroupJoin(db.Order,
                    l => l.OrderGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Type,
                        l.Money,
                        l.Integral,
                        j.FirstOrDefault().PayTime,
                        j.FirstOrDefault().PayStatus,
                        j.FirstOrDefault().Project
                    }).Where(l => l.PayStatus == 1 && SqlFunctions.DateDiff("day", l.PayTime, Date) == 0).ToList();
                //级别比例数据
                var level = db.Level.Where(l => l.LV > 5).Select(l => new { l.LV, l.Profit, l.ShopProfit,l.Bonus });
                //等级的利润和购物比例
                decimal Profit6 = level.Where(l => l.LV == 6).Select(l => new { Profit = l.Profit - (l.ShopProfit / 10) }).FirstOrDefault().Profit;
                decimal ShopProfit6 = level.Where(l => l.LV == 6).Select(l => new { l.ShopProfit }).FirstOrDefault().ShopProfit / 10;
                decimal Profit7 = level.Where(l => l.LV == 7).Select(l => new { Profit = l.Profit - (l.ShopProfit / 10) }).FirstOrDefault().Profit;
                decimal ShopProfit7 = level.Where(l => l.LV == 8).Select(l => new { l.ShopProfit }).FirstOrDefault().ShopProfit / 10;
                decimal Profit8 = level.Where(l => l.LV == 8).Select(l => new { Profit = l.Profit - (l.ShopProfit / 10) }).FirstOrDefault().Profit;
                decimal ShopProfit8 = level.Where(l => l.LV == 8).Select(l => new { l.ShopProfit }).FirstOrDefault().ShopProfit / 10;
                decimal Profit9 = level.Where(l => l.LV == 9).Select(l => new { Profit = l.Profit - (l.ShopProfit / 10) }).FirstOrDefault().Profit;
                decimal ShopProfit9 = level.Where(l => l.LV == 9).Select(l => new { l.ShopProfit }).FirstOrDefault().ShopProfit / 10;
                decimal Profit10 = level.Where(l => l.LV == 10).Select(l => new { Profit = l.Profit - (l.ShopProfit / 10) }).FirstOrDefault().Profit;
                decimal ShopProfit10 = level.Where(l => l.LV == 10).Select(l => new { l.ShopProfit }).FirstOrDefault().ShopProfit / 10;
                decimal Profit11 = level.Where(l => l.LV == 11).Select(l => new { Profit = l.Profit - (l.ShopProfit / 10) }).FirstOrDefault().Profit;
                decimal ShopProfit11 = level.Where(l => l.LV == 11).Select(l => new { l.ShopProfit }).FirstOrDefault().ShopProfit / 10;
                //查找在分红当天符合分红的会员
                var m = db.Member.Where(l => l.Level > 5).Select(l => new
                {
                    l.Gid,
                    l.Level,
                    l.StockRight
                });
                //当前日期的订单
                List<Order> o = db.Order.Where(l => l.PayStatus == 1 && l.PayType != 5 && SqlFunctions.DateDiff("day", l.PayTime, Date) == 0).ToList();

                #region 生态圈股东分红
                //生态圈和股东分红数量
                decimal sr10 = m.Where(l => l.Level == 10).Select(l => l.StockRight).DefaultIfEmpty(0).Sum();
                decimal sr11 = m.Where(l => l.Level == 11).Select(l => l.StockRight).DefaultIfEmpty(0).Sum();
                //总订单分销金额
                decimal Recommendation = mr.Where(q => q.Type == 5).Select(q => q.Money).DefaultIfEmpty(0m).Sum();
                decimal Differential = mr.Where(q => q.Type == 6).Select(q => q.Money).DefaultIfEmpty(0m).Sum();
                decimal SameLevel = mr.Where(q => q.Type == 7).Select(q => q.Money).DefaultIfEmpty(0m).Sum();
                decimal SRecommendation = mr.Where(q => q.Type == 5).Select(q => q.Integral).DefaultIfEmpty(0m).Sum();
                decimal SDifferential = mr.Where(q => q.Type == 6).Select(q => q.Integral).DefaultIfEmpty(0m).Sum();
                decimal SSameLevel = mr.Where(q => q.Type == 7).Select(q => q.Integral).DefaultIfEmpty(0m).Sum();
                //增加总项目订单日报(生态圈股东分红)
                Guid ReportGid = Guid.NewGuid();
                //返回总利润
                decimal pp = R(ReportGid, 0, Date, o, Recommendation, Differential, SameLevel, SRecommendation, SDifferential, SSameLevel, "生态圈总数=" + sr10.ToString() + ",股东总数=" + sr11.ToString());
                if (pp != 0)
                {
                    //总利润分红比例
                    decimal Bonus10 = level.Where(l => l.LV == 10).Select(l => new { Bonus = l.Bonus - (l.ShopProfit / 10) }).FirstOrDefault().Bonus;
                    decimal Bonus11 = level.Where(l => l.LV == 11).Select(l => new { Bonus = l.Bonus - (l.ShopProfit / 10) }).FirstOrDefault().Bonus;
                    //会员详情分红
                    var mgd = m.Where(l => l.Level > 9);//股东生态圈会员
                    foreach (var mb in mgd)
                    {
                        decimal Money = 0;
                        decimal Integral = 0;
                        switch (mb.Level)
                        {
                            case 10:
                                if (sr10 > 0)
                                {
                                    //用户的分红点 X (总利润分红比例/当前等级的所有分红点)
                                    Money = mb.StockRight * ((Bonus10 * pp) / sr10);
                                    Integral = mb.StockRight * ((ShopProfit10 * pp) / sr10);
                                }
                                break;
                            case 11:
                                if (sr11 > 0)
                                {
                                    Money = mb.StockRight * ((Bonus11 * pp) / sr11);
                                    Integral = mb.StockRight * ((ShopProfit11 * pp) / sr11);
                                }
                                break;
                            default:
                                break;
                        }
                        if (Money > 0 || Integral > 0)
                        {
                            RL(ReportGid, mb.Gid, mb.Level, Money, Integral, mb.StockRight);
                        }
                    }
                }
                #endregion

                #region 合伙人分红
                //项目
                var p = db.DictionariesList.Where(l => l.DGid == db.Dictionaries.Where(d => d.DictionaryType == "ClassifyType").FirstOrDefault().Gid).Select(l => new { l.Value }).ToList();
                //合伙人总分红数量
                DateTime dt = Date.AddDays(1);//只查询报表当天以前的分红数量
                //查询项目合伙人会员
                m = m.Where(l => l.Level > 5);
                int Project = 0;//当前项目
                foreach (var dr in p)
                {
                    Project = int.Parse(dr.Value);
                    //会员当前项目的分红数据
                    var sr = db.StockRight.Where(s => s.Project == Project && s.AddTime < dt).ToList();
                    //当前项目合伙人总分红数量
                    decimal srnum = sr.Select(l => l.Number).DefaultIfEmpty(0m).Sum();
                    //当前项目订单分销金额
                    Recommendation = mr.Where(q => q.Type == 5 && q.Project == Project).Select(q => q.Money).DefaultIfEmpty(0m).Sum();
                    Differential = mr.Where(q => q.Type == 6 && q.Project == Project).Select(q => q.Money).DefaultIfEmpty(0m).Sum();
                    SameLevel = mr.Where(q => q.Type == 7 && q.Project == Project).Select(q => q.Money).DefaultIfEmpty(0m).Sum();
                    SRecommendation = mr.Where(q => q.Type == 5 && q.Project == Project).Select(q => q.Integral).DefaultIfEmpty(0m).Sum();
                    SDifferential = mr.Where(q => q.Type == 6 && q.Project == Project).Select(q => q.Integral).DefaultIfEmpty(0m).Sum();
                    SSameLevel = mr.Where(q => q.Type == 7 && q.Project == Project).Select(q => q.Integral).DefaultIfEmpty(0m).Sum();
                    //增加项目日报(合伙人项目分红)
                    ReportGid = Guid.NewGuid();
                    pp = R(ReportGid, Project, Date, o, Recommendation, Differential, SameLevel, SRecommendation, SDifferential, SSameLevel, "总数=" + srnum.ToString());
                    if (pp != 0)
                    {
                        int i = m.Count();
                        //会员详情分红
                        foreach (var mb in m)
                        {
                            //当前会员项目的分红数量
                            decimal msr = sr.Where(l => l.MemberGid == mb.Gid).Select(l => l.Number).DefaultIfEmpty(0m).Sum();
                            decimal Money = 0;
                            decimal Integral = 0;
                            if (msr > 0)
                            {
                                //当前项目等级的利润和购物比例/分红数量
                                switch (mb.Level)
                                {
                                    case 6:
                                        Money = msr * ((Profit6 * pp) / srnum);
                                        Integral = msr * ((ShopProfit6 * pp) / srnum);
                                        break;
                                    case 7:
                                        Money = msr * ((Profit7 * pp) / srnum);
                                        Integral = msr * ((ShopProfit7 * pp) / srnum);
                                        break;
                                    case 8:
                                        Money = msr * ((Profit8 * pp) / srnum);
                                        Integral = msr * ((ShopProfit8 * pp) / srnum);
                                        break;
                                    case 9:
                                        Money = msr * ((Profit9 * pp) / srnum);
                                        Integral = msr * ((ShopProfit9 * pp) / srnum);
                                        break;
                                    case 10:
                                        Money = msr * ((Profit10 * pp) / srnum);
                                        Integral = msr * ((ShopProfit10 * pp) / srnum);
                                        break;
                                    case 11:
                                        Money = msr * ((Profit11 * pp) / srnum);
                                        Integral = msr * ((ShopProfit11 * pp) / srnum);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            if (Money > 0 || Integral > 0)
                            {
                                RL(ReportGid, mb.Gid, 6, Money, Integral, msr);
                            }
                        }
                    }
                }
                #endregion

                return Json(new AjaxResult(Date));
            }
        }
        /// <summary>
        /// 日报记录,返回订单利润
        /// </summary>
        public decimal R(Guid ReportGid, int Project, DateTime Date, List<Order> o, decimal Recommendation, decimal Differential, decimal SameLevel, decimal SRecommendation, decimal SDifferential, decimal SSameLevel, string Remarks = "")
        {
            using (EFDB db = new EFDB())
            {
                if (db.Report.Where(l => l.Project == Project && SqlFunctions.DateDiff("day", l.RTime, Date) == 0).FirstOrDefault() == null)
                {
                    //各项目日报
                    var b = new Report();
                    b.Gid = ReportGid;
                    b.AddTime = DateTime.Now;
                    b.RTime = Date;
                    b.Project = Project;
                    if (o != null)
                    {
                        if (Project == 0)
                        {
                            b.OrderPrice = o.Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                            b.ProfitPrice = o.Select(l => l.Profit).DefaultIfEmpty(0m).Sum();
                        }
                        else
                        {
                            b.OrderPrice = o.Where(l => l.Project == Project).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                            b.ProfitPrice = o.Where(l => l.Project == Project).Select(l => l.Profit).DefaultIfEmpty(0m).Sum();
                        }
                    }
                    else
                    {
                        b.OrderPrice = 0;
                        b.ProfitPrice = 0;
                    }
                    b.Recommendation = Recommendation;
                    b.Differential = Differential;
                    b.SameLevel = SameLevel;
                    b.SRecommendation = SRecommendation;
                    b.SDifferential = SDifferential;
                    b.SSameLevel = SSameLevel;
                    b.Remarks = Remarks;
                    db.Report.Add(b);
                    if (db.SaveChanges() == 1)
                    {
                        return b.ProfitPrice;
                    }
                    else
                    {
                        LogManager.WriteLog("每日分红失败", "日期=" + Date.ToLongDateString() + ",项目=" + Project.ToString());
                    }
                }
                else
                {
                    LogManager.WriteLog("每日分红已存在", "日期=" + Date.ToLongDateString() + ",项目=" + Project.ToString());
                }
                return 0;
            }
        }
        /// <summary>
        /// 分红记录
        /// </summary>
        public int RL(Guid ReportGid, Guid MemberGid, int Type, decimal Money, decimal Integral, decimal Number,string Remarks="")
        {
            using (EFDB db = new EFDB())
            {
                var rl = new ReportList();
                rl.Gid = Guid.NewGuid();
                rl.AddTime = DateTime.Now;
                rl.ReportGid = ReportGid;
                rl.MemberGid = MemberGid;
                rl.State = 1;
                rl.Type = Type;
                rl.Money = Money;
                rl.Integral = Integral;
                rl.Number = Number;
                rl.Remarks = Remarks;
                db.ReportList.Add(rl);
                return db.SaveChanges();
            }
        }
        #endregion

        #region 短信模块
        // 列表管理
        public ActionResult SMSList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult SMS()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Content = paramJson["Content"].ToString();
                var b = db.SMS.AsQueryable();
                if (!string.IsNullOrEmpty(Content))
                {
                    b = b.Where(l => l.Content.Contains(Content));
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
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
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult SMSDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    if (db.Level.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 库存
        /// <summary>
        /// 库存管理
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult StockList()
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
        public JsonResult Stock()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string STime = paramJson["STime"].ToString();
            string ETime = paramJson["ETime"].ToString();
            string Name = paramJson["Name"].ToString();
            Guid MemberGid = Guid.Parse(paramJson["membergid"].ToString());
            using (EFDB db = new EFDB())
            {
                var b = db.Stock.Where(l => l.MemberGid == MemberGid).GroupJoin(db.Product,
                    l => l.ProductGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Number,
                        l.AddTime,
                        j.FirstOrDefault().Name,
                        j.FirstOrDefault().Picture,
                        j.FirstOrDefault().Price
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
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
        #endregion

        #region 提现管理
        // 列表管理
        public ActionResult WithdrawalsList()
        {
            if (Request.Form["daochu"] == "daochu")
            {
                using (EFDB db = new EFDB())
                {
                    string STime = Request.Form["STime"];
                    string ETime = Request.Form["ETime"];
                    string Bank = Request.Form["Bank"];
                    string BankName = Request.Form["BankName"];
                    string BankNumber = Request.Form["BankNumber"];
                    string Account = Request.Form["Account"];
                    int State = string.IsNullOrEmpty(Request.Form["State"]) ? 0 : Int32.Parse(Request.Form["State"]);
                    var b = db.Withdrawals.GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Bank,
                        l.BankName,
                        l.BankNumber,
                        l.Money,
                        l.PayTime,
                        l.Remarks,
                        l.State,
                        l.MemberGid,
                        j.FirstOrDefault().Account
                    }).AsQueryable();
                    if (!string.IsNullOrEmpty(Request.Form["MemberGid"]))
                    {
                        Guid gid = Guid.Parse(Request.Form["MemberGid"]);
                        b = b.Where(l => l.MemberGid == gid);
                    }
                    if (!string.IsNullOrEmpty(Bank))
                    {
                        b = b.Where(l => l.Bank.Contains(Bank));
                    }
                    if (!string.IsNullOrEmpty(BankName))
                    {
                        b = b.Where(l => l.BankName.Contains(BankName));
                    }
                    if (!string.IsNullOrEmpty(BankNumber))
                    {
                        b = b.Where(l => l.BankNumber.Contains(BankNumber));
                    }
                    if (!string.IsNullOrEmpty(Account))
                    {
                        b = b.Where(l => l.Account == Account);
                    }
                    if (State != 0)
                    {
                        b = b.Where(l => l.State == State);
                    }
                    //时间查询
                    if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                    {
                        DateTime? st = null;
                        DateTime? et = null;
                        if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                        {
                            st = et = DateTime.Parse(STime);
                        }
                        else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                        {
                            st = et = DateTime.Parse(ETime);
                        }
                        else
                        {
                            st = DateTime.Parse(STime);
                            et = DateTime.Parse(ETime);
                        }
                        b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                    }
                    //获取list数据
                    var list = b.OrderByDescending(l => l.AddTime).OrderBy(l => l.AddTime).ToList();

                    //创建Excel文件的对象
                    NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                    //添加一个sheet
                    NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");

                    //给sheet1添加第一行的头部标题
                    NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
                    row1.CreateCell(0).SetCellValue("开户行");
                    row1.CreateCell(1).SetCellValue("用户名");
                    row1.CreateCell(2).SetCellValue("卡号");
                    row1.CreateCell(3).SetCellValue("金额");
                    row1.CreateCell(4).SetCellValue("状态");
                    //将数据逐步写入sheet1各个行
                    int i = 0;
                    for (i = 0; i < list.Count; i++)
                    {
                        NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                        rowtemp.CreateCell(0).SetCellValue(list[i].Bank.ToString());
                        rowtemp.CreateCell(1).SetCellValue(list[i].BankName.ToString());
                        rowtemp.CreateCell(2).SetCellValue(list[i].BankNumber.ToString());
                        rowtemp.CreateCell(3).SetCellValue(list[i].Money.ToString());
                        rowtemp.CreateCell(4).SetCellValue(list[i].State == 1 ? "待付款" : "已付款");
                    }
                    NPOI.SS.UserModel.IRow rowend = sheet1.CreateRow(i + 1);
                    // 写入到客户端 
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    book.Write(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    return File(ms, "application/vnd.ms-excel", "会员提现"+ DateTime.Now.ToString("MMddHHmm") + ".xls");
                }
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public JsonResult Withdrawals()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Bank = paramJson["Bank"].ToString();
                string BankName = paramJson["BankName"].ToString();
                string BankNumber = paramJson["BankNumber"].ToString();
                string Account = paramJson["Account"].ToString();
                int State = int.Parse(paramJson["State"].ToString());
                var b = db.Withdrawals.GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Bank,
                        l.BankName,
                        l.BankNumber,
                        l.Money,
                        l.PayTime,
                        l.Remarks,
                        l.State,
                        l.MemberGid,
                        j.FirstOrDefault().Account
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(paramJson["MemberGid"].ToString()))
                {
                    Guid gid = Guid.Parse(paramJson["MemberGid"].ToString());
                    b = b.Where(l => l.MemberGid == gid);
                }
                if (!string.IsNullOrEmpty(Bank))
                {
                    b = b.Where(l => l.Bank.Contains(Bank));
                }
                if (!string.IsNullOrEmpty(BankName))
                {
                    b = b.Where(l => l.BankName.Contains(BankName));
                }
                if (!string.IsNullOrEmpty(BankNumber))
                {
                    b = b.Where(l => l.BankNumber.Contains(BankNumber));
                }
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account == Account);
                }
                if (State != 0)
                {
                    b = b.Where(l => l.State == State);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总提现=" + b.Select(l => l.Money).DefaultIfEmpty(0m).Sum(),
                    count = b.Count(),
                    pageindex,
                    list = b.OrderBy(l => l.State).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 提现审核
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        [HttpPost]
        public JsonResult PayMoney(Guid Gid,int State)
        {
            using (EFDB db = new EFDB())
            {
                if (db.Withdrawals.Where(l => l.Gid == Gid && l.State == 1).Update(l => new Withdrawals { State = State }) == 1)
                {
                    return Json(new AjaxResult("操作成功"));
                }
                else
                {
                    LogManager.WriteLog("提现操作失败", "Gid=" + Gid.ToString() + ",State=" + State.ToString());
                }
                return Json(new AjaxResult(300, "操作失败"));
            }
        }
        #endregion

        #region 支付货款
        /// <summary>
        /// ljsheng
        /// </summary>
        /// <param name="Gid">订单Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult PMoney(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Order.Where(l => l.Gid == Gid && l.Status == 1 && l.Type == 1 && l.RobGid != null).FirstOrDefault();
                if (b != null)
                {
                    b.Status = 2;
                    if (db.SaveChanges() == 1)
                    {
                        if (Helper.MoneyRecordAdd(Gid, (Guid)b.RobGid, b.Price, 0, 3) != null)
                        {
                            return Json(new AjaxResult("货款支付成功=" + b.Price.ToString()));
                        }
                        else
                        {
                            LogManager.WriteLog("支付货款失败", "货款状态更新成功,支付失败,请反馈给技术人员,错误代码=" + Gid.ToString());
                            return Json(new AjaxResult(300, "货款状态更新成功,支付失败,请反馈给技术人员,错误订单号=" + b.OrderNo));
                        }
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "货款支付失败"));
                    }
                }
                else
                {
                    return Json(new AjaxResult(300, "订单异常或未被接单,不可支付货款"));
                }
            }
        }
        #endregion

        #region 会员资金记录
        /// <summary>
        /// 资金记录
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult IntegralList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Integral()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string Account = paramJson["Account"].ToString();
            string OrderNo = paramJson["OrderNo"].ToString();
            using (EFDB db = new EFDB())
            {
                var b = db.MoneyRecord.GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.Money,
                        l.Integral,
                        l.OldIntegral,
                        l.OldMoney,
                        l.AddTime,
                        l.Type,
                        l.Remarks,
                        OrderNo = l.OrderGid==null?"分红":db.Order.Where(o=>o.Gid==l.OrderGid).FirstOrDefault().OrderNo,
                        j.FirstOrDefault().Account,
                        j.FirstOrDefault().RealName
                    }).AsQueryable();
                int Type = int.Parse(paramJson["Type"].ToString());
                if (Type != 0)
                {
                    b = b.Where(l => l.Type == Type);
                }
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account == Account);
                }
                if(!string.IsNullOrEmpty(OrderNo))
                {
                    b = b.Where(l => l.OrderNo == OrderNo);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总积分=" + b.Select(l => l.Money).DefaultIfEmpty(0m).Sum() + ",购物积分=" + b.Select(l => l.Integral).DefaultIfEmpty(0m).Sum(),
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        #endregion

        #region 商家模块
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult ShopAU()
        {
            using (EFDB db = new EFDB())
            {
                if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.Shop.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Name = b.Name;
                    ViewBag.Number = b.Number;
                    ViewBag.Money = b.Money;
                    ViewBag.Profile = b.Profile;
                    ViewBag.Content = b.Content;
                    ViewBag.Sort = b.Sort;
                    ViewBag.Show = b.Show;
                    ViewBag.Picture = Help.Shop + b.Picture;
                    ViewBag.USCI = Help.Shop + b.USCI;
                    ViewBag.LegalPerson = Help.Shop + b.LegalPerson;
                    ViewBag.Licence = Help.Shop + b.Licence;
                    ViewBag.Project = b.Project;
                    ViewBag.Remarks = b.Remarks;
                    ViewBag.ContactNumber = b.ContactNumber;
                    ViewBag.Province = b.Province;
                    ViewBag.City = b.City;
                    ViewBag.Area = b.Area;
                    ViewBag.Address = b.Address;
                    ViewBag.State = b.State;
                }
                Guid DGid = db.Dictionaries.Where(l => l.DictionaryType == "Shop").FirstOrDefault().Gid;
                return View(db.DictionariesList.Where(dl => dl.DGid == DGid).ToList());
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ShopAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                string Name = Request.Form["Name"];
                int Project = int.Parse(Request.Form["Project"]);
                if (db.Shop.Where(l => l.Name == Name && l.Gid != Gid && l.Project == Project).Count() > 0)
                {
                    return Helper.WebRedirect("名称已存在！", "history.go(-1);", "名称已存在");
                }
                else
                {
                    var b = db.Shop.Where(l => l.Gid == Gid).FirstOrDefault();
                    b.Project = Project;
                    b.Name = Name;
                    b.Number = int.Parse(Request.Form["Number"]);
                    b.Money = decimal.Parse(Request.Form["Money"]);
                    b.Profile = Request.Form["Profile"];
                    b.Content = Request.Form["Content"];
                    b.Remarks = Request.Form["Remarks"];
                    b.Show = Int32.Parse(Request.Form["Show"]);
                    b.Sort = Int32.Parse(Request.Form["Sort"]);
                    if (!string.IsNullOrEmpty(Request.Form["Picture"]))
                    {
                        b.Picture = Request.Form["Picture"];
                    }
                    b.ContactNumber = Request.Form["ContactNumber"];
                    b.Province = Request.Form["Province"];
                    b.City = Request.Form["City"];
                    b.Area = Request.Form["Area"];
                    b.Address = Request.Form["Address"];
                    b.State = int.Parse(Request.Form["State"]);
                    if (db.SaveChanges() == 1)
                    {
                        return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                    }
                    else
                    {
                        return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                    }
                }
            }
        }
        /// <summary>
        /// 商家列表
        /// </summary>
        public ActionResult ShopList()
        {
            using (EFDB db = new EFDB())
            {
                Guid DGid = db.Dictionaries.Where(l => l.DictionaryType == "Shop").FirstOrDefault().Gid;
                return View(db.DictionariesList.Where(dl => dl.DGid == DGid).ToList());
            }
        }
        [HttpPost]
        public JsonResult Shop()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Account = paramJson["Account"].ToString();
                string Name = paramJson["Name"].ToString();
                int State = int.Parse(paramJson["State"].ToString()); 
                int Project = int.Parse(paramJson["Project"].ToString());
                Guid DGid = db.Dictionaries.Where(l => l.DictionaryType == "Shop").FirstOrDefault().Gid;
                var b = db.Shop.GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Name,
                        l.Money,
                        l.Picture,
                        l.State,
                        l.Project,
                        l.ContactNumber,
                        j.FirstOrDefault().Account,
                        ProjectName = db.DictionariesList.Where(dl => dl.DGid == DGid && dl.Value == SqlFunctions.StringConvert((double)l.Project).Trim()).FirstOrDefault().Key
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account.Contains(Account));
                }
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }
                if (State!=0)
                {
                    b = b.Where(l => l.State == State);
                }
                if (Project != 0)
                {
                    b = b.Where(l => l.Project == Project);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
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

        #region 商家商品模块
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult ShopProductAU()
        {
            using (EFDB db = new EFDB())
            {
                if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.ShopProduct.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Name = b.Name;
                    ViewBag.Price = b.Price;
                    ViewBag.OriginalPrice = b.OriginalPrice;
                    ViewBag.Number = b.Number;
                    ViewBag.Stock = b.Stock;
                    ViewBag.Profile = b.Profile;
                    ViewBag.Content = b.Content;
                    ViewBag.Sort = b.Sort;
                    ViewBag.Show = b.Show;
                    ViewBag.Picture = Help.Product + b.Picture;
                    //ViewBag.GraphicDetails = b.GraphicDetails;
                    ViewBag.ClassifyGid = b.ClassifyGid;
                    ViewBag.Remarks = b.Remarks;
                    ViewBag.ExpressFee = b.ExpressFee;
                    ViewBag.Company = b.Company;
                    ViewBag.Brand = b.Brand;
                    ViewBag.Prefix = b.Prefix;
                }
                else
                {
                    ViewBag.Prefix = "";
                    ViewBag.Stock = "";
                    ViewBag.Price = "";
                    ViewBag.OriginalPrice ="";
                    ViewBag.Number = 0;
                    ViewBag.Sort = 1;
                    ViewBag.Show = 1;
                    ViewBag.ClassifyGid = Guid.Parse("00000000-0000-0000-0000-000000000000");
                }
                Guid ShopGid = Guid.Parse(Request.QueryString["ShopGid"]);
                return View(db.ShopClassify.Where(l => l.ShopGid == ShopGid).ToList());
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ShopProductAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                ShopProduct b;
                if (Gid == null)
                {
                    b = new ShopProduct();
                    b.Gid = Guid.NewGuid();
                    b.AddTime = DateTime.Now;
                    b.ShopGid = Guid.Parse(Request.Form["ShopGid"]);
                    b.Price = decimal.Parse(Request.Form["Price"]);
                    b.Stock = int.Parse(Request.Form["Stock"]);
                    b.Prefix = Request.Form["Prefix"];
                    b.Show = Int32.Parse(Request.Form["Show"]);
                }
                else
                {
                    b = db.ShopProduct.Where(l => l.Gid == Gid).FirstOrDefault();
                }
                b.ClassifyGid = Guid.Parse(Request.Form["ClassifyGid"]);
                b.Name = Request.Form["Name"];
                b.OriginalPrice = decimal.Parse(Request.Form["OriginalPrice"]);
                b.Number = int.Parse(Request.Form["Number"]);
                b.Profile = Request.Form["Profile"];
                b.Content = Request.Form["Content"];
                b.Remarks = Request.Form["Remarks"];
                b.Sort = Int32.Parse(Request.Form["Sort"]);
                b.ExpressFee = Int32.Parse(Request.Form["ExpressFee"]);
                b.Company = Request.Form["Company"];
                b.Brand = Request.Form["Brand"];

                if (!string.IsNullOrEmpty(Request.Form["Picture"]))
                {
                    b.Picture = Request.Form["Picture"];
                }
                //if (!string.IsNullOrEmpty(Request.Form["GraphicDetails"]))
                //{
                //    b.GraphicDetails = Request.Form["GraphicDetails"];
                //}
                if (Gid == null)
                {
                    db.ShopProduct.Add(b);
                }
                if (db.SaveChanges() == 1)
                {
                    return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                }
                else
                {
                    return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                }
            }
        }

        // 列表管理
        public ActionResult ShopProductList()
        {
            using (EFDB db = new EFDB())
            {
                Guid ShopGid = Guid.Parse(Request.QueryString["ShopGid"]);
                return View(db.ShopClassify.Where(l => l.ShopGid == ShopGid).ToList());
            }
        }
        [HttpPost]
        public JsonResult ShopProduct()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Name = paramJson["Name"].ToString();
                var b = db.ShopProduct.GroupJoin(db.ShopClassify,
                    x => x.ClassifyGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        x.Gid,
                        x.AddTime,
                        x.Prefix,
                        x.Brand,
                        x.Name,
                        x.Picture,
                        x.Show,
                        x.Sort,
                        x.Price,
                        x.OriginalPrice,
                        x.ClassifyGid,
                        x.Number,
                        x.Stock,
                        x.ShopGid,
                        ClassifyName = y.FirstOrDefault().Name
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }
                if (paramJson["ClassifyGid"].ToString() != "0")
                {
                    Guid ClassifyGid = Guid.Parse(paramJson["ClassifyGid"].ToString());
                    b = b.Where(l => l.ClassifyGid == ClassifyGid);
                }
                if (!string.IsNullOrEmpty(paramJson["ShopGid"].ToString()))
                {
                    Guid ShopGid = Guid.Parse(paramJson["ShopGid"].ToString());
                    b = b.Where(l => l.ShopGid == ShopGid);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
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
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult ShopProductDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    string file = System.Web.HttpContext.Current.Server.MapPath(Help.Product + db.Product.Where(l => l.Gid == Gid).FirstOrDefault().Picture);
                    if (db.ShopProduct.Where(l => l.Gid == Gid).Delete() == 1)
                    {
                        if (System.IO.File.Exists(file))
                            System.IO.File.Delete(file);
                        return Json(new AjaxResult("成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 商家提现管理
        // 列表管理
        public ActionResult ShopWithdrawalsList()
        {
            if (Request.Form["daochu"] == "daochu")
            {
                using (EFDB db = new EFDB())
                {
                    string STime = Request.Form["STime"];
                    string ETime = Request.Form["ETime"];
                    string Bank = Request.Form["Bank"];
                    string BankName = Request.Form["BankName"];
                    string BankNumber = Request.Form["BankNumber"];
                    string Name = Request.Form["Name"];
                    int State = string.IsNullOrEmpty(Request.Form["State"]) ? 0 : Int32.Parse(Request.Form["State"]);
                    var b = db.ShopWithdrawals.GroupJoin(db.Shop,
                    l => l.ShopGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Bank,
                        l.BankName,
                        l.BankNumber,
                        l.Money,
                        l.PayTime,
                        l.Remarks,
                        l.State,
                        l.ShopGid,
                        j.FirstOrDefault().Name
                    }).AsQueryable();
                    if (!string.IsNullOrEmpty(Request.Form["ShopGid"]))
                    {
                        Guid gid = Guid.Parse(Request.Form["ShopGid"]);
                        b = b.Where(l => l.ShopGid == gid);
                    }
                    if (!string.IsNullOrEmpty(Bank))
                    {
                        b = b.Where(l => l.Bank.Contains(Bank));
                    }
                    if (!string.IsNullOrEmpty(BankName))
                    {
                        b = b.Where(l => l.BankName.Contains(BankName));
                    }
                    if (!string.IsNullOrEmpty(BankNumber))
                    {
                        b = b.Where(l => l.BankNumber.Contains(BankNumber));
                    }
                    if (!string.IsNullOrEmpty(Name))
                    {
                        b = b.Where(l => l.Name == Name);
                    }
                    if (State != 0)
                    {
                        b = b.Where(l => l.State == State);
                    }
                    //时间查询
                    if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                    {
                        DateTime? st = null;
                        DateTime? et = null;
                        if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                        {
                            st = et = DateTime.Parse(STime);
                        }
                        else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                        {
                            st = et = DateTime.Parse(ETime);
                        }
                        else
                        {
                            st = DateTime.Parse(STime);
                            et = DateTime.Parse(ETime);
                        }
                        b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                    }
                    //获取list数据
                    var list = b.OrderByDescending(l => l.AddTime).OrderBy(l => l.AddTime).ToList();

                    //创建Excel文件的对象
                    NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                    //添加一个sheet
                    NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");

                    //给sheet1添加第一行的头部标题
                    NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
                    row1.CreateCell(0).SetCellValue("开户行");
                    row1.CreateCell(1).SetCellValue("用户名");
                    row1.CreateCell(2).SetCellValue("卡号");
                    row1.CreateCell(3).SetCellValue("金额");
                    row1.CreateCell(4).SetCellValue("状态");
                    //将数据逐步写入sheet1各个行
                    int i = 0;
                    for (i = 0; i < list.Count; i++)
                    {
                        NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                        rowtemp.CreateCell(0).SetCellValue(list[i].Bank.ToString());
                        rowtemp.CreateCell(1).SetCellValue(list[i].BankName.ToString());
                        rowtemp.CreateCell(2).SetCellValue(list[i].BankNumber.ToString());
                        rowtemp.CreateCell(3).SetCellValue(list[i].Money.ToString());
                        rowtemp.CreateCell(4).SetCellValue(list[i].State == 1 ? "待付款" : "已付款");
                    }
                    NPOI.SS.UserModel.IRow rowend = sheet1.CreateRow(i + 1);
                    // 写入到客户端 
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    book.Write(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    return File(ms, "application/vnd.ms-excel", "商家提现"+DateTime.Now.ToString("MMddHHmm") +".xls");
                }
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public JsonResult ShopWithdrawals()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Bank = paramJson["Bank"].ToString();
                string BankName = paramJson["BankName"].ToString();
                string BankNumber = paramJson["BankNumber"].ToString();
                string Name = paramJson["Name"].ToString();
                int State = int.Parse(paramJson["State"].ToString());
                var b = db.ShopWithdrawals.GroupJoin(db.Shop,
                    l => l.ShopGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Bank,
                        l.BankName,
                        l.BankNumber,
                        l.Money,
                        l.PayTime,
                        l.Remarks,
                        l.State,
                        l.ShopGid,
                        j.FirstOrDefault().Name
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(paramJson["ShopGid"].ToString()))
                {
                    Guid gid = Guid.Parse(paramJson["ShopGid"].ToString());
                    b = b.Where(l => l.ShopGid == gid);
                }
                if (!string.IsNullOrEmpty(Bank))
                {
                    b = b.Where(l => l.Bank.Contains(Bank));
                }
                if (!string.IsNullOrEmpty(BankName))
                {
                    b = b.Where(l => l.BankName.Contains(BankName));
                }
                if (!string.IsNullOrEmpty(BankNumber))
                {
                    b = b.Where(l => l.BankNumber.Contains(BankNumber));
                }
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name == Name);
                }
                if (State != 0)
                {
                    b = b.Where(l => l.State == State);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总提现=" + b.Select(l => l.Money).DefaultIfEmpty(0m).Sum(),
                    count = b.Count(),
                    pageindex,
                    list = b.OrderBy(l => l.State).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 提现审核
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        [HttpPost]
        public JsonResult ShopPayMoney(Guid Gid, int State)
        {
            using (EFDB db = new EFDB())
            {
                if (db.ShopWithdrawals.Where(l => l.Gid == Gid && l.State == 1).Update(l => new ShopWithdrawals { State = State }) == 1)
                {
                    return Json(new AjaxResult("操作成功"));
                }
                else
                {
                    LogManager.WriteLog("提现操作失败", "Gid=" + Gid.ToString() + ",State=" + State.ToString());
                }
                return Json(new AjaxResult(300, "操作失败"));
            }
        }
        #endregion

        #region 商家订单管理
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult ShopOrderAU()
        {
            using (EFDB db = new EFDB())
            {
                if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.ShopOrder.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.OrderNo = b.OrderNo;
                    ViewBag.Remarks = b.Remarks;
                    ViewBag.ExpressStatus = b.ExpressStatus;
                    ViewBag.Express = b.Express;
                    ViewBag.ExpressNumber = b.ExpressNumber;
                    ViewBag.Address = b.Address;
                    ViewBag.RealName = b.RealName;
                    ViewBag.ContactNumber = b.ContactNumber;
                    ViewBag.Product = b.Product;
                    ViewBag.ConsumptionCode = b.ConsumptionCode;
                    ViewBag.Price = b.Price;
                }
                else
                {
                    ViewBag.Sort = 1;
                }
                return View(db.Express.Where(l => l.Show == 1).OrderBy(l => l.Sort).ToList());
            }
        }

        [HttpPost]
        public ActionResult ShopOrderAU(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.ShopOrder.Where(l => l.Gid == Gid).FirstOrDefault();
                b.Remarks = Request.Form["Remarks"];
                b.Express = Request.Form["Express"];
                b.ExpressNumber = Request.Form["ExpressNumber"];
                b.Address = Request.Form["Address"];
                b.RealName = Request.Form["RealName"];
                b.ContactNumber = Request.Form["ContactNumber"];
                b.ExpressStatus = Int32.Parse(Request.Form["ExpressStatus"]);
                b.Status = Request.Form["ExpressStatus"] == "3" ? 2 : 1;
                if (db.SaveChanges() == 1)
                {
                    return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                }
                else
                {
                    return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                }
            }
        }

        public ActionResult ShopOrderList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ShopOrder()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                var b = db.ShopOrder.GroupJoin(db.Shop,
                    l => l.ShopGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.MemberGid,
                        ShopGid=j.FirstOrDefault().MemberGid,
                        l.Product,
                        l.OrderNo,
                        l.PayType,
                        l.PayStatus,
                        l.TotalPrice,
                        l.PayPrice,
                        l.Price,
                        l.Profit,
                        l.ExpressStatus,
                        l.Express,
                        l.ExpressNumber,
                        l.Remarks,
                        l.Status,
                        l.ConsumptionCode,
                        l.Voucher,
                        j.FirstOrDefault().Name,
                        j.FirstOrDefault().ContactNumber
                    }).GroupJoin(db.Member,
                    x => x.MemberGid,
                    y => y.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.MemberGid,
                        l.ShopGid,
                        l.Product,
                        l.OrderNo,
                        l.PayType,
                        l.PayStatus,
                        l.TotalPrice,
                        l.Price,
                        l.PayPrice,
                        l.Profit,
                        l.ExpressStatus,
                        l.Express,
                        l.ExpressNumber,
                        l.Remarks,
                        l.Status,
                        l.ConsumptionCode,
                        l.Name,
                        l.ContactNumber,
                        l.Voucher,
                        j.FirstOrDefault().Account,
                        j.FirstOrDefault().RealName,
                        ShopAccount = db.Member.Where(m=>m.Gid==l.ShopGid).FirstOrDefault().Account,
                        T1 = db.ShopRecord.Where(s => s.OrderGid == l.Gid && s.Type == 4).Select(s => s.TIntegral).DefaultIfEmpty(0m).Sum(),
                        T2 = db.ShopRecord.Where(s => s.OrderGid == l.Gid && s.Type == 5).Select(s => s.TIntegral).DefaultIfEmpty(0m).Sum(),
                        T3 = db.ShopRecord.Where(s => s.OrderGid == l.Gid && s.Type == 6).Select(s => s.TIntegral).DefaultIfEmpty(0m).Sum(),
                        MIntegral = db.ShopRecord.Where(s => s.OrderGid == l.Gid && s.Type == 3).Select(s => s.MIntegral).DefaultIfEmpty(0m).Sum()
                    }).AsQueryable();
                string Account = paramJson["Account"].ToString();
                string OrderNo = paramJson["OrderNo"].ToString();
                string Name = paramJson["Name"].ToString();
                string Product = paramJson["Product"].ToString();
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account == Account);
                }
                if (!string.IsNullOrEmpty(OrderNo))
                {
                    b = b.Where(l => l.OrderNo == OrderNo);
                }
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name == Name);
                }
                if (!string.IsNullOrEmpty(Product))
                {
                    b = b.Where(l => l.Product == Product);
                }
                if (!string.IsNullOrEmpty(paramJson["ShopGid"].ToString()))
                {
                    Guid ShopGid = Guid.Parse(paramJson["ShopGid"].ToString());
                    b = b.Where(l => l.ShopGid == ShopGid);
                }
                if (!string.IsNullOrEmpty(paramJson["MemberGid"].ToString()))
                {
                    Guid MemberGid = Guid.Parse(paramJson["MemberGid"].ToString());
                    b = b.Where(l => l.MemberGid == MemberGid);
                }
                if (paramJson["PayStatus"].ToString() != "0")
                {
                    int PayStatus = int.Parse(paramJson["PayStatus"].ToString());
                    b = b.Where(l => l.PayStatus == PayStatus);
                }
                if (paramJson["ExpressStatus"].ToString() != "0")
                {
                    int ExpressStatus = int.Parse(paramJson["ExpressStatus"].ToString());
                    b = b.Where(l => l.ExpressStatus == ExpressStatus);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总积分=" + b.Select(l => l.TotalPrice).DefaultIfEmpty(0m).Sum(),
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 商品详情
        /// </summary>
        public ActionResult ShopOrderDetailsList()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ShopOrderDetails()
        {
            //查询的参数json
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            //解析参数
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string STime = paramJson["STime"].ToString();
            string ETime = paramJson["ETime"].ToString();
            using (EFDB db = new EFDB())
            {
                Guid Gid = Guid.Parse(paramJson["Gid"].ToString());
                var b = db.OrderDetails.Where(l => l.OrderGid == Gid).GroupJoin(db.ShopProduct,
                    x => x.ProductGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        x.Price,
                        x.Number,
                        x.Remarks,
                        x.AddTime,
                        y.FirstOrDefault().Name
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

        #region 商家分类模块
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult ShopClassifyAU()
        {
            using (EFDB db = new EFDB())
            {
                if (!string.IsNullOrEmpty(Request.QueryString["gid"]))
                {
                    Guid Gid = Guid.Parse(Request.QueryString["gid"]);
                    var b = db.ShopClassify.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Name = b.Name;
                    ViewBag.Remarks = b.Remarks;
                    ViewBag.Sort = b.Sort;
                    ViewBag.Show = b.Show;
                    ViewBag.Picture = Help.Classify + b.Picture;
                }
                else
                {
                    ViewBag.Sort = 1;
                    ViewBag.Show = 1;
                }
                return View();
            }
        }
        [HttpPost]
        public ActionResult ShopClassifyAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                ShopClassify b;
                if (Gid == null)
                {
                    b = new ShopClassify();
                    b.Gid = Guid.NewGuid();
                    b.AddTime = DateTime.Now;
                    b.ShopGid = Guid.Parse(Request.Form["ShopGid"]);
                }
                else
                {
                    b = db.ShopClassify.Where(l => l.Gid == Gid).FirstOrDefault();
                }
                b.Name = Request.Form["Name"];
                b.Remarks = Request.Form["Remarks"];
                b.Sort = Int32.Parse(Request.Form["Sort"]);
                b.Show = Int32.Parse(Request.Form["Show"]);
                if (!string.IsNullOrEmpty(Request.Form["Picture"]))
                {
                    b.Picture = Request.Form["Picture"];
                }
                if (Gid == null)
                {
                    db.ShopClassify.Add(b);
                }
                if (db.SaveChanges() == 1)
                {
                    return Helper.WebRedirect("操作成功！", "history.go(-1);", "操作成功!");
                }
                else
                {
                    return Helper.WebRedirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                }
            }
        }

        // 列表管理
        public ActionResult ShopClassifyList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ShopClassify()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Name = paramJson["Name"].ToString();
                Guid ShopGid = Guid.Parse(paramJson["ShopGid"].ToString());
                var b = db.ShopClassify.Where(l => l.ShopGid == ShopGid).AsQueryable();
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Select(l => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Name,
                        l.Picture,
                        l.Show,
                        l.Sort,
                        l.Remarks
                    }).Take(pagesize).ToList()
                }));
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        [HttpPost]
        public JsonResult ShopClassifyDelete(Guid Gid)
        {
            if (Gid != null)
            {
                using (EFDB db = new EFDB())
                {
                    if (db.ShopProduct.Where(l => l.ClassifyGid == Gid).Count() > 0)
                    {
                        return Json(new AjaxResult(300, "该分类下还有商品.请先设置成其他分类才可以删除!"));
                    }
                    else
                    {
                        if (db.ShopClassify.Where(l => l.Gid == Gid).Delete() == 1)
                        {
                            return Json(new AjaxResult("成功"));
                        }
                        else
                        {
                            return Json(new AjaxResult(300, "失败"));
                        }
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
            }
        }
        #endregion

        #region 彩链
        // 列表管理
        //同时支持Get和Post
        //[AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult CLTeamList()
        {
            using (EFDB db = new EFDB())
            {
                //ar d = db.Dictionaries.Where(l => l.DictionaryType == "ClassifyType").FirstOrDefault();
                //ViewBag.Project = db.DictionariesList.Where(l => l.DGid == d.Gid).ToList();
                var lv = db.Level.OrderBy(l => l.LV);
                //ViewBag.Level = lv.Where(l => l.Project == 1).ToList();
                ViewBag.CLLevel = lv.Where(l => l.Project == 2).ToList();
                return View();
            }
        }
        [HttpPost]
        public JsonResult CLTeam()
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
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Account = paramJson["Account"].ToString();
                string RealName = paramJson["RealName"].ToString();
                int CLLevel = int.Parse(paramJson["CLLevel"].ToString());
                int State = int.Parse(paramJson["State"].ToString());
                int Year = int.Parse(paramJson["Year"].ToString());
                int Month = int.Parse(paramJson["Month"].ToString());
                var b = db.Achievement.GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.State,
                        l.Year,
                        l.Month,
                        l.TMoney,
                        l.MMoney,
                        l.MemberGid,
                        l.CLLevel,
                        l.BonusTime,
                        l.Remarks,
                        l.ProjectRemarks,
                        l.StockRightRemarks,
                        l.ProjectMRGid,
                        l.StockRightMRGid,
                        l.ProjectMoney,
                        l.ProjectIntegral,
                        l.Money,
                        l.Integral,
                        j.FirstOrDefault().Account,
                        j.FirstOrDefault().RealName,
                        MCLLevel = j.FirstOrDefault().CLLevel
                    }).GroupJoin(db.Level,
                    l => l.CLLevel,
                    j => j.LV,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.State,
                        l.Year,
                        l.Month,
                        l.TMoney,
                        l.MMoney,
                        l.MemberGid,
                        l.CLLevel,
                        l.BonusTime,
                        l.Remarks,
                        l.ProjectRemarks,
                        l.StockRightRemarks,
                        l.ProjectMRGid,
                        l.StockRightMRGid,
                        l.ProjectMoney,
                        l.ProjectIntegral,
                        l.Money,
                        l.Integral,
                        l.Account,
                        l.RealName,
                        l.MCLLevel,
                        LevelName = j.FirstOrDefault().Name,
                        j.FirstOrDefault().Label
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account.Contains(Account));
                }
                if (!string.IsNullOrEmpty(RealName))
                {
                    b = b.Where(l => l.RealName.Contains(RealName));
                }
                if (CLLevel != 0)
                {
                    b = b.Where(l => l.MCLLevel == CLLevel);
                }
                if (State != 0)
                {
                    b = b.Where(l => l.State == State);
                }
                if (Year != 0)
                {
                    b = b.Where(l => l.Year == Year);
                }
                if (Month != 0)
                {
                    b = b.Where(l => l.Month == Month);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.BonusTime >= st && l.BonusTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                decimal ProjectMoney = b.Select(l => l.ProjectMoney).DefaultIfEmpty(0m).Sum();
                decimal ProjectIntegral =  b.Select(l => l.ProjectIntegral).DefaultIfEmpty(0m).Sum();
                decimal Money = b.Select(l => l.Money).DefaultIfEmpty(0m).Sum();
                decimal Integral =  b.Select(l => l.Integral).DefaultIfEmpty(0m).Sum();

                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总分红=" + (ProjectMoney + Money).ToString() +  ",购=" + (ProjectIntegral + Integral).ToString() + ",其中项目="+ ProjectMoney.ToString() + ",购=" + ProjectIntegral.ToString() + ",股东="+ Money.ToString()+",购="+ Integral.ToString(),
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 给用户分红
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        [HttpPost]
        public JsonResult CLBonus(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Achievement.Where(l => l.Gid == Gid && l.State == 2).FirstOrDefault();
                if (b != null)
                {
                    //项目分红
                    if (b.ProjectMoney > 0 || b.ProjectIntegral > 0)
                    {
                        b.ProjectMRGid = Helper.MoneyRecordAdd(null, b.MemberGid, b.ProjectMoney, b.ProjectIntegral, 23, "彩链" + b.Year.ToString() + b.Month.ToString());
                        if (b.ProjectMRGid == null)
                        {
                            b.ProjectRemarks = "项目分红失败";
                        }
                    }
                    //股东分红
                    if (b.Money > 0 || b.Integral > 0)
                    {
                        b.StockRightMRGid = Helper.MoneyRecordAdd(null, b.MemberGid, b.Money, b.Integral, 9, "彩链" + b.Year.ToString() + b.Month.ToString());
                        if (b.StockRightMRGid == null)
                        {
                            b.StockRightRemarks = "股东分红失败";
                        }
                    }
                    b.State = 3;
                    if (db.SaveChanges() == 1)
                    {
                        return Json(new AjaxResult("分红成功"));
                    }
                    else
                    {
                        LogManager.WriteLog("彩链更新分红失败", "资金增加成功.更新分红状态失败,Gid=" + Gid.ToString() + ",ProjectMRGid=" + b.ProjectMRGid.ToString() + ",StockRightMRGid=" + b.StockRightMRGid.ToString());
                    }
                    return Json(new AjaxResult(300, "用户分红失败!"));
                }
                else
                {
                    return Json(new AjaxResult(300, "用户分红失败,请先统计月份分红"));
                }
            }
        }

        /// <summary>
        /// 统计所有未分红的月份
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        [HttpPost]
        public JsonResult CLAllBonus(int Year,int Month)
        {
            using (EFDB db = new EFDB())
            {
                //统计月份的第一天和最后一天
                DateTime MonthFirst = DTime.FirstDayOfMonth(DateTime.Parse(Year + "-" + Month + "-" + "01"));
                DateTime MonthLast = DTime.LastDayOfMonth(DateTime.Parse(Year + "-" + Month + "-" + "01 23:59:59"));
                //当前月份总业绩
                decimal AllMoney = db.Order.Where(l => l.PayStatus == 1 && l.Type == 3 && l.PayTime >= MonthFirst && l.AddTime <= MonthLast).Select(l => l.PayPrice).DefaultIfEmpty(0).Sum();

                #region 分红会员
                //先判断股东是否在分红数据里
                var m = db.Member.Where(l => l.Level == 11).ToList();
                //没有的话增加单月的股东数据
                foreach (var dr in m)
                {
                    if (db.Achievement.Where(l => l.MemberGid == dr.Gid).Count() == 0)
                    {
                        Helper.Achievement("股东", dr.Gid, Year, Month, 0, 0);
                    }
                }
                var b = db.Achievement.Where(l => l.Year == Year && l.Month == Month).GroupJoin(db.Member,
                                    x => x.MemberGid,
                                    y => y.Gid,
                                    (x, y) => new
                                    {
                                        x.Gid,
                                        x.TMoney,
                                        x.State,
                                        MGid = x.MemberGid,
                                        y.FirstOrDefault().CLLevel,
                                        y.FirstOrDefault().Level,
                                        y.FirstOrDefault().MemberGid,
                                        y.FirstOrDefault().Account,
                                        y.FirstOrDefault().StockRight
                                    }).GroupJoin(db.MRelation,
                                    x => x.Gid,
                                    y => y.MemberGid,
                                    (x, y) => new
                                    {
                                        x.Gid,
                                        x.TMoney,
                                        x.State,
                                        x.MGid,
                                        x.MemberGid,
                                        x.CLLevel,
                                        x.Level,
                                        x.Account,
                                        x.StockRight,
                                        MRM1 = y.FirstOrDefault().M1,
                                        MRM2 = y.FirstOrDefault().M2,
                                        MRM3 = y.FirstOrDefault().M3
                                    }).Where(l => l.CLLevel == 26 || l.Level == 11).ToList();
                #endregion

                if (b.Count() > 0 && b.Where(l=>l.State==3).Count()==0)
                {
                    //创始人分红数据
                    var ml26 = db.Level.Where(l => l.LV == 26).FirstOrDefault();
                    //创始人分红比例
                    decimal Project26 = ml26.Profit;
                    decimal ShopProfit = ml26.ShopProfit;
                    //创始人满足金额的总业绩
                    decimal MMoney = db.Achievement.Where(l => l.Year == Year && l.Month == Month && l.TMoney >= 600000).Select(l => l.TMoney).DefaultIfEmpty(0).Sum();
                    foreach (var dr in b)
                    {
                        //更新分红
                        var ach = db.Achievement.Where(l => l.MemberGid == dr.MGid).FirstOrDefault();
                        ach.CLLevel = dr.CLLevel;
                        ach.BonusTime = DateTime.Now;
                        //更新的时候已分红的状态不变
                        ach.State = ach.State == 3 ? 3 : 2;

                        #region 创始人分红
                        if (dr.CLLevel == 26)
                        {
                            //业绩要满足60W才分红
                            if (dr.TMoney >= 600000)
                            {
                                //计算项目分红比例
                                decimal ProfitMoney = AllMoney * Project26;
                                //会员团队业绩/全部满足条件会员的业绩X项目分工比例
                                decimal RMB = dr.TMoney / MMoney * ProfitMoney;
                                ach.ProjectMoney = RMB - RMB * ShopProfit;
                                ach.ProjectIntegral = RMB * ShopProfit; ;
                                ach.ProjectRemarks = "项目总金额=" + AllMoney.ToString() + ",积分比例=" + Project26.ToString() + ",购物积分比例=" + ShopProfit.ToString() + ",分配金额=" + ProfitMoney.ToString() + ",团队业绩=" + dr.TMoney.ToString() + ",满足的总业绩=" + MMoney.ToString() + ",月份总业绩=" + AllMoney.ToString();
                            }
                            else
                            {
                                ach.ProjectRemarks = "项目金额不足";
                            }
                        }
                        #endregion

                        #region 股东分红
                        if (dr.Level == 11)
                        {
                            if (dr.StockRight > 0)
                            {
                                //共有多少股权
                                decimal StockRight = m.Select(l => l.StockRight).DefaultIfEmpty(0).Sum();
                                var SRLV = db.Level.Where(l => l.LV == 11).FirstOrDefault();
                                //股东分红比例
                                decimal Bonus = SRLV.Bonus;
                                //分红的金额
                                decimal Money = AllMoney * Bonus;
                                //每个分红点多少钱
                                decimal RMB = Money / StockRight;
                                ach.Money = RMB * dr.StockRight;
                                ach.StockRightRemarks = "总分红积分=" + Money.ToString() + ",总分红点=" + StockRight.ToString() + ",分红股权=" + dr.StockRight.ToString() + ",每个股权积分=" + RMB.ToString();
                            }
                            else
                            {
                                ach.StockRightRemarks = "未分配股权";
                            }
                        }
                        #endregion

                        if (db.SaveChanges() != 1)
                        {
                            LogManager.WriteLog("分红失败", "会员=" + dr.MGid.ToString() + ",Year=" + Year.ToString() + ",Month=" + Month.ToString());
                        }
                    }
                    return Json(new AjaxResult(Year.ToString() + Month.ToString() + " 统计成功!"));
                }
                else
                {
                    LogManager.WriteLog("彩链失败", "当前没有可统计的数据或已有分红的数据,Year=" + Year.ToString() + ",Month=" + Month.ToString());
                }
                return Json(new AjaxResult(300, "没有需要分红的数据!"));
            }
        }

        /// <summary>
        /// 团队业绩列表
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para Name="result">200 是成功其他失败</para>
        /// <para Name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public ActionResult CLTOrderList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult CLTOrder()
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
                int Year = int.Parse(paramJson["Year"].ToString());
                int Month = int.Parse(paramJson["Month"].ToString());
                Guid Gid = Guid.Parse(paramJson["Gid"].ToString());
                //统计月份的第一天和最后一天
                DateTime MonthFirst = DTime.FirstDayOfMonth(DateTime.Parse(Year + "-" + Month + "-" + "01"));
                DateTime MonthLast = DTime.LastDayOfMonth(DateTime.Parse(Year + "-" + Month + "-" + "01 23:59:59"));
                //查询所有等级
                List <Guid> list = new List<Guid>();
                list = Helper.MGidALL(Gid, db.Member.Where(l => l.MemberGid != null).ToList(), list);
                var b = db.Order.Where(l=>l.PayStatus==1 && l.Type==3 && l.PayTime>= MonthFirst && l.PayTime<= MonthLast).GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.MemberGid,
                        l.CLLevel,
                        l.Remarks,
                        l.OrderNo,
                        l.PayPrice,
                        l.Product,
                        j.FirstOrDefault().Account
                    }).AsQueryable();
                string GList = string.Join(",", list.ToArray());
                b = b.Where(l => GList.Contains(l.MemberGid.ToString()));
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

        #region 最先结算的会员-无用
        /// <summary>
        /// 最后一个级别的会员
        /// </summary>
        /// <returns></returns>
        public ActionResult AMList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult AM()
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
                #region 分红会员
                var b = db.Achievement.GroupJoin(db.Member,
                                    x => x.MemberGid,
                                    y => y.Gid,
                                    (x, y) => new
                                    {
                                        x.Gid,
                                        x.TMoney,
                                        MGid = x.MemberGid,
                                        y.FirstOrDefault().CLLevel,
                                        y.FirstOrDefault().Level,
                                        y.FirstOrDefault().MemberGid,
                                        y.FirstOrDefault().Account
                                    }).GroupJoin(db.MRelation,
                                    x => x.Gid,
                                    y => y.MemberGid,
                                    (x, y) => new
                                    {
                                        x.Gid,
                                        x.TMoney,
                                        x.MGid,
                                        x.MemberGid,
                                        x.CLLevel,
                                        x.Level,
                                        x.Account,
                                        MRM1 = y.FirstOrDefault().M1,
                                        MRM2 = y.FirstOrDefault().M2,
                                        MRM3 = y.FirstOrDefault().M3
                                    }).ToList();
                var mr = db.MRelation.ToList();
                for (int i = 0; i < b.Count; i++)
                {
                    if (mr.Where(l=>l.M1== b[i].MGid || l.M2 == b[i].MGid|| l.M3 == b[i].MGid).Count()!=0)
                    {
                        b.Remove(b[i]);
                    }
                }
                #endregion
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        #endregion

        #endregion

        #region 彩链兑换额度记录
        /// <summary>
        /// 资金记录
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult CLRecordList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult CLRecord()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string Account = paramJson["Account"].ToString();
            using (EFDB db = new EFDB())
            {
                var b = db.CLRecord.GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Remarks,
                        l.Money,
                        l.Number,
                        l.OldMoney,
                        l.OldNumber,
                        j.FirstOrDefault().Account,
                        j.FirstOrDefault().RealName
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account == Account);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总额度=" + b.Select(l => l.Money).DefaultIfEmpty(0m).Sum() + ",兑换数量=" + b.Select(l => l.Number).DefaultIfEmpty(0m).Sum(),
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        #endregion

        #region 商城资金记录
        /// <summary>
        /// 资金记录
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult ShopRecordList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ShopRecord()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string STime = paramJson["STime"].ToString();
            string ETime = paramJson["ETime"].ToString();
            string Account = paramJson["Account"].ToString();
            string OrderNo = paramJson["OrderNo"].ToString();
            using (EFDB db = new EFDB())
            {
                var b = db.ShopRecord.Where(l=>l.Type!=2).GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.MIntegral,
                        l.TIntegral,
                        l.OldMIntegral,
                        l.OldTIntegral,
                        l.AddTime,
                        l.Remarks,
                        OrderNo = l.OrderGid == null ? "无订单" : db.ShopOrder.Where(o => o.Gid == l.OrderGid).FirstOrDefault().OrderNo,
                        j.FirstOrDefault().Account,
                        j.FirstOrDefault().RealName
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account == Account);
                }
                if (!string.IsNullOrEmpty(OrderNo))
                {
                    b = b.Where(l => l.OrderNo == OrderNo);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总积分=" + b.Select(l => l.MIntegral).DefaultIfEmpty(0m).Sum() + ",团队积分=" + b.Select(l => l.TIntegral).DefaultIfEmpty(0m).Sum(),
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
        #endregion

        #region 冻结记录
        /// <summary>
        /// 冻结记录
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult FrozenIntegralList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult FrozenIntegral()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string STime = paramJson["STime"].ToString();
            string ETime = paramJson["ETime"].ToString();
            string Account = paramJson["Account"].ToString();
            int State = int.Parse(paramJson["State"].ToString());
            using (EFDB db = new EFDB())
            {
                var b = db.ShopRecord.Where(sr=>sr.Type==2).GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Gid,
                        l.AddTime,
                        l.Remarks,
                        l.TIntegral,
                        l.MIntegral,
                        l.Type,
                        l.State,
                        l.Multiple,
                        l.ThawTime,
                        j.FirstOrDefault().Account,
                        j.FirstOrDefault().RealName
                    }).AsQueryable();
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account == Account);
                }
                if (State != 0)
                {
                    b = b.Where(l => l.State == State);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = " | 当前查询总基数积分=" + b.Select(l => l.MIntegral).DefaultIfEmpty(0m).Sum() + ",团队条件积分=" + b.Select(l => l.TIntegral).DefaultIfEmpty(0m).Sum(),
                    count = b.Count(),
                    pageindex,
                    list = b.OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 解冻
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult UnFrozen()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            Guid gid = Guid.Parse(paramJson["gid"].ToString());
            using (EFDB db = new EFDB())
            {
                var b = db.ShopRecord.Where(l => l.Gid == gid && l.State == 2).FirstOrDefault();
                if(b!=null)
                {
                    b.State = 1;
                    if (db.SaveChanges() == 1)
                    {
                        if (db.Member.Where(l => l.Gid == b.MemberGid).Update(l => new Member { ShopIntegral = l.ShopIntegral + b.TIntegral }) == 1)
                        {
                            return Json(new AjaxResult("解冻成功"));
                        }
                        else
                        {
                            LogManager.WriteLog("解冻成功积分更新失败", "gid=" + gid.ToString());
                            return Json(new AjaxResult(300,"解冻成功积分更新失败"));
                        }
                    }
                    else
                    {
                        return Json(new AjaxResult(300,"解冻失败"));
                    }
                }
                else
                { return Json(new AjaxResult(300, "解冻数据不存在!")); }
                
            }
        }
        #endregion

        #region 积分兑换
        /// <summary>
        /// 积分兑换管理
        /// </summary>
        /// <param name="Integral">兑换积分</param>
        /// <param name="TB">兑换币种[1=BCCB, 2=FBCC]</param>
        /// <param name="Type">类型[1=彩链积分 2=商城基数积分 3=商城积分]</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult TokenRecordList()
        {
            return View();
        }
        [HttpPost]
        public ActionResult TokenRecord()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string STime = paramJson["STime"].ToString();
            string ETime = paramJson["ETime"].ToString();
            using (EFDB db = new EFDB())
            {
                var b = db.TokenRecord.GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.AddTime,
                        l.Gid,
                        l.Integral,
                        l.OldIntegral,
                        l.Token,
                        l.Type,
                        l.State,
                        l.Remarks,
                        j.FirstOrDefault().Account,
                        j.FirstOrDefault().RealName
                    }).AsQueryable();
                string Account = paramJson["Account"].ToString();
                int Type = Int32.Parse(paramJson["Type"].ToString());
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.Account == Account);
                }
                if (Type != 0)
                {
                    b = b.Where(l => l.Type == Type);
                }
                int State = Int32.Parse(paramJson["State"].ToString());
                if (State != 0)
                {
                    b = b.Where(l => l.State == State);
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
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
        /// 重新兑换
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult APPToken(Guid Gid)
        {
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.TokenRecord.Where(l => l.Gid == Gid && l.State == 2).GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.TB,
                        l.Token,
                        l.Type,
                        j.FirstOrDefault().Account
                    }).FirstOrDefault();
                if (b != null)
                {
                    //提交到APP
                    if (AppApi.AddMB(b.Account, b.TB == 1 ? "BCCB" : "FBCC", b.Token.ToString()))
                    {
                        if (db.TokenRecord.Where(l => l.Gid == Gid && l.State == 2).Update(l => new TokenRecord { State = 1, Remarks = l.Remarks + ",重新兑换成功" }) == 1)
                        {
                            return Json(new AjaxResult("重新兑换成功"));
                        }
                        else
                        {
                            LogManager.WriteLog("重新兑换APP成功更新记录失败", "记录GID=" + Gid.ToString());
                            return Json(new AjaxResult("重新兑换APP成功更新记录失败"));
                        }
                    }
                    else
                    {
                        return Json(new AjaxResult("重新兑换APP失败"));
                    }
                }
                else
                {
                    return Json(new AjaxResult("当前记录发生变化,请刷新!"));
                }
            }
            return Json(new AjaxResult(300, "未付款不能确认订单"));
        }
        #endregion

        #region 发货提现,发货人
        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult MProductList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MProduct()
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
                var b = db.MProduct.Select(l => new
                {
                    l.Gid,
                    l.AddTime,
                    l.MemberGid,
                    l.ShopGid,
                    l.Stock,
                    MA = db.Member.Where(m => m.Gid == l.MemberGid).FirstOrDefault().Account,
                    SA = db.Member.Where(m => m.Gid == l.ShopGid).FirstOrDefault().Account
                }).AsQueryable();
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Account = paramJson["Account"].ToString();
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.MA.Contains(Account) || l.SA.Contains(Account));
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.ToList().OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize)
                }));
            }
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult ConsignorList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Consignor()
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
                var b = db.Consignor.Select(l => new
                {
                    l.Gid,
                    l.AddTime,
                    l.MemberGid,
                    l.MGid,
                    TA = l.MGid == l.ShopGid?"上级发货人": "公司发货",
                    MA = db.Member.Where(m => m.Gid == l.MemberGid).FirstOrDefault().Account,
                    SA = db.Member.Where(m => m.Gid == l.MGid).FirstOrDefault().Account
                }).AsQueryable();
                string STime = paramJson["STime"].ToString();
                string ETime = paramJson["ETime"].ToString();
                string Account = paramJson["Account"].ToString();
                if (!string.IsNullOrEmpty(Account))
                {
                    b = b.Where(l => l.MA.Contains(Account) || l.SA.Contains(Account));
                }
                //时间查询
                if (!string.IsNullOrEmpty(STime) || !string.IsNullOrEmpty(ETime))
                {
                    DateTime? st = null;
                    DateTime? et = null;
                    if (!string.IsNullOrEmpty(STime) && string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(STime);
                    }
                    else if (string.IsNullOrEmpty(STime) && !string.IsNullOrEmpty(ETime))
                    {
                        st = et = DateTime.Parse(ETime);
                    }
                    else
                    {
                        st = DateTime.Parse(STime);
                        et = DateTime.Parse(ETime);
                    }
                    b = b.Where(l => l.AddTime >= st && l.AddTime <= et);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.ToList().OrderByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize)
                }));
            }
        }

        /// <summary>
        /// 变更发货人
        /// </summary>
        [HttpPost]
        public JsonResult MPC(Guid Gid,int Type)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Consignor.Where(l => l.Gid == Gid).FirstOrDefault();
                Guid ShopGid = b.ShopGid;
                if (Type == 2)
                {
                    ShopGid = Helper.GetConsignor();
                }
                b.MGid = ShopGid;
                if (db.SaveChanges() == 1)
                {
                    return Json(new AjaxResult("变更发货人成功"));
                }
                else
                {
                    return Json(new AjaxResult(300," 你没有推荐人或推荐人没有变化!"));
                }
            }
        }

        #endregion
    }
}