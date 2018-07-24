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
    public class ShopController : SController
    {
        #region 商家中心
        /// <summary>
        /// 商家中心
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
                Guid gid = LCookie.GetMemberGid();
                var b = db.Shop.Where(l => l.MemberGid == gid).FirstOrDefault();
                ViewBag.Name = b.Name;
                ViewBag.State = b.State;
                ViewBag.Picture = Help.Shop + b.Picture;
                ViewBag.Order = db.ShopOrder.Where(l => l.Status == 1 && l.ShopGid==b.Gid).Count();
                LCookie.AddCookie("shop", DESRSA.DESEnljsheng(JsonConvert.SerializeObject(new
                {
                    b.Gid,
                    b.USCI,
                    b.LegalPerson,
                    b.Name
                })), 1);
            }
            return View();
        }
        /// <summary>
        /// 增加编辑
        /// </summary>
        public ActionResult Info()
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = LCookie.GetShopGid();
                if (Gid!=null)
                {
                    var b = db.Shop.Where(l => l.Gid == Gid).FirstOrDefault();
                    ViewBag.Name = b.Name;
                    ViewBag.Number = b.Number;
                    ViewBag.Money = b.Money;
                    ViewBag.Profile = b.Profile;
                    ViewBag.Content = b.Content;
                    ViewBag.Content = b.Address;
                    ViewBag.Sort = b.Sort;
                    ViewBag.Show = b.Show;
                    ViewBag.Picture = Help.Shop + b.Picture;
                    ViewBag.USCI = Help.Shop + b.USCI;
                    ViewBag.LegalPerson = Help.Shop + b.LegalPerson;
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
        public ActionResult ShopAU()
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = LCookie.GetShopGid();
                var b = db.Shop.Where(l => l.Gid == Gid).FirstOrDefault();
                b.Project = int.Parse(Request.Form["Project"]);
                b.Name = Request.Form["Name"];
                //b.Number = int.Parse(Request.Form["Number"]);
               //b.Money = decimal.Parse(Request.Form["Money"]);
                b.Profile = Request.Form["Profile"];
                b.Content = Request.Form["Content"];
                //b.Remarks = Request.Form["Remarks"];
                //b.Show = Int32.Parse(Request.Form["Show"]);
                //b.Sort = Int32.Parse(Request.Form["Sort"]);
                string picture = Helper.jsimg(Help.Shop, Request.Form["base64Data"]);
                if (!string.IsNullOrEmpty(picture))
                {
                    b.Picture = picture;
                }
                b.ContactNumber = Request.Form["ContactNumber"];
                b.Province = Request.Form["Province"];
                b.City = Request.Form["City"];
                b.Area = Request.Form["Area"];
                b.Address = Request.Form["Address"];
                //b.State = int.Parse(Request.Form["State"]);
                if (db.SaveChanges() == 1)
                {
                    return Helper.Redirect("操作成功！", "history.go(-1);", "操作成功!");
                }
                else
                {
                    return Helper.Redirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                }
            }
        }

        /// <summary>
        /// 法人证件
        /// </summary>
        public ActionResult LegalPerson()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LPAU()
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = LCookie.GetShopGid();
                var b = db.Shop.Where(l => l.Gid == Gid).FirstOrDefault();
                string picture = Helper.jsimg(Help.Shop, Request.Form["base64Data"]);
                if (!string.IsNullOrEmpty(picture))
                {
                    b.LegalPerson = picture;
                }
                if (db.SaveChanges() == 1)
                {
                    Helper.SLogin(Gid);
                    return Helper.Redirect("操作成功！", "history.go(-1);", "操作成功!");
                }
                else
                {
                    return Helper.Redirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                }
            }
        }

        /// <summary>
        /// 法人证件
        /// </summary>
        public ActionResult USCI()
        {
            return View();
        }
        [HttpPost]
        public ActionResult USCIAU()
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = LCookie.GetShopGid();
                var b = db.Shop.Where(l => l.Gid == Gid).FirstOrDefault();
                string picture = Helper.jsimg(Help.Shop, Request.Form["base64Data"]);
                if (!string.IsNullOrEmpty(picture))
                {
                    b.USCI = picture;
                }
                if (db.SaveChanges() == 1)
                {
                    Helper.SLogin(Gid);
                    return Helper.Redirect("操作成功！", "history.go(-1);", "操作成功!");
                }
                else
                {
                    return Helper.Redirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                }
            }
        }

        /// <summary>
        /// 彩链包兑换额度
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult CLMoney()
        {
            using (EFDB db = new EFDB())
            {
                Guid gid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                if (b.CLLevel > 21)
                {
                    //会员原来的彩链金额
                    decimal CLMoney = ViewBag.CLMoney = b.CLMoney;
                    Guid ProductGid = Helper.GetProductGid();
                    var p = db.Stock.Where(l => l.MemberGid == gid && l.ProductGid == ProductGid).FirstOrDefault();
                    if (p != null)
                    {
                        //会员原来的彩链库存
                        int MStock = ViewBag.Stock = p.Number;
                        //一个彩链兑换多少额度
                        decimal CLM = ViewBag.CLB = db.DictionariesList.Where(dl => dl.Key == "CLB" && dl.DGid == db.Dictionaries.Where(d => d.DictionaryType == "CL").FirstOrDefault().Gid).FirstOrDefault().Value;
                        if (!string.IsNullOrEmpty(Request["Stock"]))
                        {
                            //要兑换的数量
                            int Stock = int.Parse(Request["Stock"]);
                            p.Number = p.Number - Stock;
                            if (p.Number >= 0 && db.SaveChanges() == 1)
                            {
                                b.CLMoney = b.CLMoney + Stock * CLM;
                                if (db.SaveChanges() == 1)
                                {
                                    var cl = new CLRecord();
                                    cl.Gid = Guid.NewGuid();
                                    cl.AddTime = DateTime.Now;
                                    cl.MemberGid = gid;
                                    cl.Money = Stock * CLM;
                                    cl.OldMoney = CLMoney;
                                    cl.Number = Stock;
                                    cl.OldNumber = MStock;
                                    db.CLRecord.Add(cl);
                                    db.SaveChanges();
                                    return Helper.Redirect("成功", "/Shop/Index", "恭喜你,兑换成功!");
                                }
                                else
                                {
                                    LogManager.WriteLog("彩链库存扣除成功兑换失败", "MemberGid=" + gid.ToString() + ",Stock=" + Stock.ToString());
                                    return Helper.Redirect("失败", "history.go(-1);", "彩链库存扣除成功兑换失败,请联系客服人员!");
                                }
                            }
                            else
                            {
                                return Helper.Redirect("失败", "history.go(-1);", "彩链库存不足!");
                            }
                        }
                    }
                    else
                    {
                        return Helper.Redirect("库存不足", "history.go(-1);", "库存不足");
                    }
                }
                else
                {
                    return Helper.Redirect("权限不足", "history.go(-1);", "最少要VIP级别");
                }
            }
            return View();
        }
        #endregion

        #region 分类管理
        /// <summary>
        /// 我的订单
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult ShopClassify()
        {
            return View();
        }

        /// <summary>
        /// 获取分类
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult ShopClassifyData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            Guid Gid = LCookie.GetShopGid();
            using (EFDB db = new EFDB())
            {
                var b = db.ShopClassify.Where(l => l.ShopGid == Gid).AsQueryable();
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
        public JsonResult ShopClassifyDelete(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                if (db.ShopProduct.Where(l => l.ClassifyGid == Gid).Count() == 0)
                {
                    if (db.ShopClassify.Where(l => l.Gid == Gid).Delete() > 0)
                    {
                        return Json(new AjaxResult("删除成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult("删除失败"));
                    }
                }
                else
                {
                    return Json(new AjaxResult(300, "分类下还有商品,不能删除,请转移商品到其他分类!"));
                }
            }
        }
        #endregion

        #region 商家商品模块
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
                    ViewBag.OriginalPrice = 0;
                    ViewBag.Number = 0;
                    ViewBag.Sort = 1;
                    ViewBag.Show = 1;
                    ViewBag.ClassifyGid = 0;
                }
                Guid ShopGid = LCookie.GetShopGid();
                return View(db.ShopClassify.Where(l => l.ShopGid == ShopGid).ToList());
            }
        }
        [HttpPost]
        public ActionResult ProductAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                //编号不能重复
                string Prefix = Request.Form["Prefix"];
                if (db.ShopProduct.Where(l => l.Prefix == Prefix).Count() == 0)
                {
                    //判断额度
                    Guid gid = LCookie.GetMemberGid();
                    var m = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                    int Stock = int.Parse(Request.Form["Stock"]);
                    decimal Price = decimal.Parse(Request.Form["Price"]);
                    decimal RMB = Stock * Price;
                    if (m.CLMoney >= RMB)
                    {
                        m.CLMoney = m.CLMoney - RMB;
                        if (db.SaveChanges() == 1)
                        {
                            ShopProduct b;
                            if (Gid == null)
                            {
                                b = new ShopProduct();
                                b.Gid = Guid.NewGuid();
                                b.AddTime = DateTime.Now;
                                b.ShopGid = LCookie.GetShopGid();
                                b.Prefix = Request.Form["Prefix"];
                            }
                            else
                            {
                                b = db.ShopProduct.Where(l => l.Gid == Gid).FirstOrDefault();
                            }
                            b.ClassifyGid = Guid.Parse(Request.Form["ClassifyGid"]);
                            b.Name = Request.Form["Name"];
                            b.Price = Price;
                            b.OriginalPrice = decimal.Parse(Request.Form["Price"]);
                            b.Number = 0;
                            b.Stock = Stock;
                            b.Profile = Request.Form["Profile"];
                            b.Content = Request.Form["Content"];
                            //b.Remarks = Request.Form["Remarks"];
                            b.Show = 1;
                            b.Sort = 1;
                            b.ExpressFee = 0;
                            //b.Company = Request.Form["Company"];
                            //b.Brand = Request.Form["Brand"];
                            //b.Prefix = Request.Form["Prefix"];
                            string picture = Helper.jsimg(Help.Product, Request.Form["base64Data"]);
                            if (!string.IsNullOrEmpty(picture))
                            {
                                b.Picture = picture;
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
                                return Helper.Redirect("操作成功！", "/Shop/Product", "操作成功!");
                            }
                            else
                            {
                                LogManager.WriteLog("额度扣除成功发布商品失败", "MemberGid=" + gid.ToString() + ",RMB=" + RMB.ToString());
                                return Helper.Redirect("操作失败,请检查录入的数据！", "history.go(-1);", "操作失败,请检查录入的数据!");
                            }
                        }
                        else
                        {
                            return Helper.Redirect("操作失败！", "history.go(-1);", "扣除额度失败!");
                        }
                    }
                    else
                    {
                        return Helper.Redirect("操作失败！", "/Shop/CLMoney", "你的额度(" + m.CLMoney.ToString() +")不足发布总价("+ RMB.ToString() + ")" +",请先兑换!");
                    }
                }
                else
                {
                    return Helper.Redirect("编号已存在！", "history.go(-1);", "操作失败,编号已存在!");
                }
            }
        }

        // 列表管理
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
                Guid ShopGid = LCookie.GetShopGid();
                var b = db.ShopProduct.Where(l=>l.ShopGid== ShopGid).GroupJoin(db.ShopClassify,
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
                if (!string.IsNullOrEmpty(paramJson["ClassifyGid"].ToString()))
                {
                    Guid ClassifyGid = Guid.Parse(paramJson["ClassifyGid"].ToString());
                    b = b.Where(l => l.ClassifyGid == ClassifyGid);
                }
                int pageindex = Int32.Parse(paramJson["pageindex"].ToString());
                int pagesize = Int32.Parse(paramJson["pagesize"].ToString());
                return Json(new AjaxResult(new
                {
                    other = "",
                    count = b.Count(),
                    pageindex,
                    list = b.OrderBy(l => l.Show).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
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
                    if (db.ShopProduct.Where(l => l.Gid == Gid).Update(l => new ShopProduct { Show = 2 })== 1)
                    {
                        return Json(new AjaxResult("下架成功"));
                    }
                    else
                    {
                        return Json(new AjaxResult(300, "下架失败"));
                    }
                }
            }
            else
            {
                return Json(new AjaxResult(300, "非法参数"));
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
        public ActionResult Money(string Money)
        {
            using (EFDB db = new EFDB())
            {
                Guid gid = LCookie.GetShopGid();
                var shop = db.Shop.Where(l => l.Gid == gid).FirstOrDefault();
                ViewBag.Money = shop.Money;
                var b = db.Member.Where(l => l.Gid == shop.MemberGid).FirstOrDefault();
                if (string.IsNullOrEmpty(Money))
                {
                    ViewBag.Bank = b.Bank;
                    ViewBag.BankName = b.BankName;
                    ViewBag.BankNumber = b.BankNumber;
                    return View();
                }
                else
                {
                    decimal M = decimal.Parse(Money);
                    if (M < 100)
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "最少100积分起提");
                    }
                    else
                    {
                        if (M > shop.Money)
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "可提积分不足");
                        }
                        else
                        {
                            Guid? MRGid = Helper.ShopMoneyRecordAdd(gid, -M, "商家申请提现");
                            if (MRGid != null)
                            {
                                var wd = new ShopWithdrawals();
                                wd.Gid = Guid.NewGuid();
                                wd.AddTime = DateTime.Now;
                                wd.State = 1;
                                wd.Money = M;
                                wd.Bank = b.Bank;
                                wd.BankName = b.BankName;
                                wd.BankNumber = b.BankNumber;
                                wd.ShopGid = gid;
                                wd.MRGid = (Guid)MRGid;
                                db.ShopWithdrawals.Add(wd);
                                if (db.SaveChanges() == 1)
                                {
                                    return Helper.Redirect("成功", "/Shop/Money", "恭喜你,提现成功,等待财务审核后打款");
                                }
                                else
                                {
                                    LogManager.WriteLog("商家扣除成功打款记录失败", "gid=" + gid.ToString() + ",money=" + Money.ToString());
                                    return Helper.Redirect("失败", "history.go(-1);", "扣除成功打款记录失败");
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
        public ActionResult MoneyData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            Guid ShopGid = LCookie.GetShopGid();
            using (EFDB db = new EFDB())
            {
                var b = db.ShopWithdrawals.Where(l => l.ShopGid == ShopGid).AsQueryable();
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
            Guid ShopGid = LCookie.GetShopGid();
            using (EFDB db = new EFDB())
            {
                var b = db.ShopOrder.Where(l => l.PayStatus == 1 && l.ShopGid == ShopGid).GroupJoin(db.Member,
                    x => x.MemberGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        x.Gid,
                        x.AddTime,
                        x.ExpressStatus,
                        x.Product,
                        x.TotalPrice,
                        x.OrderNo,
                        y.FirstOrDefault().RealName,
                        y.FirstOrDefault().Account,
                        Number = db.OrderDetails.Where(od => od.OrderGid == x.Gid).Sum(od => od.Number)
                    }).AsQueryable();
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
        public ActionResult OrderDetail(Guid OrderGid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.ShopOrder.Where(l => l.Gid == OrderGid).FirstOrDefault();
                ViewBag.RealName = b.RealName;
                ViewBag.ContactNumber = b.ContactNumber;
                ViewBag.Address = b.Address;
                ViewBag.AddTime = b.AddTime;;
                ViewBag.TotalPrice = b.TotalPrice;
                ViewBag.OrderNo = b.OrderNo;
                ViewBag.Remarks = b.Remarks;
                ViewBag.Express = b.Express;
                ViewBag.ExpressNumber = b.ExpressNumber;
                //if (b.RobGid != null)
                //{
                //    ViewBag.Status = b.ExpressStatus;
                //    if (string.IsNullOrEmpty(b.ExpressNumber))
                //    {
                //        //抢单成功.待发货1
                //        ViewBag.btName = "发货";
                //    }
                //    else
                //    {
                //        //抢单成功.已发货2
                //        ViewBag.btName = "查看";
                //    }
                //}
                //else
                //{
                //    ViewBag.Status = 0;
                //    ViewBag.btName = "抢单";
                //}
                ViewBag.Express = b.Express;
                ViewBag.ExpressNumber = b.ExpressNumber;
                ViewBag.ExpressList = db.Express.Where(l => l.Show == 1).OrderBy(l => l.Sort).ToList();
                return View(db.OrderDetails.Where(l => l.OrderGid == OrderGid).ToList());
            }
        }
        #endregion
    }
}