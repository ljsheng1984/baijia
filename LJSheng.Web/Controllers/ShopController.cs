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
                Guid MemberGid = LCookie.GetMemberGid();
                var b = db.Shop.Where(l => l.MemberGid == MemberGid).FirstOrDefault();
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
                ViewBag.ShopMoney = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault().ShopMoney;
                //库存
                Guid ProductGid = Helper.GetProductGid();
                var ms = db.Stock.Where(l => l.MemberGid == MemberGid && l.ProductGid == ProductGid).FirstOrDefault();
                ViewBag.Stock = ms == null ? 0 : ms.Number;
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
                    //ViewBag.Project = b.Project;
                    var sp = db.ShopProject.Where(l => l.ShopGid == Gid).Select(l => new { l.Project }).ToList();
                    if (sp != null)
                    {
                        string s = "";
                        foreach (var dr in sp)
                        {
                            s += dr.Project.ToString() + ",";
                        }
                        ViewBag.Project = s;
                    }
                    ViewBag.Remarks = b.Remarks;
                    ViewBag.ContactNumber = b.ContactNumber;
                    ViewBag.Province = b.Province;
                    ViewBag.City = b.City;
                    ViewBag.Area = b.Area;
                    ViewBag.Address = b.Address;
                    ViewBag.State = b.State;
                }
                Guid DGid = db.Dictionaries.Where(l => l.DictionaryType == "Shop").FirstOrDefault().Gid;
                return View(db.DictionariesList.Where(dl => dl.DGid == DGid).OrderBy(dl=>dl.Sort).ToList());
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
                b.Project = 0;
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
                //更新分类
                string[] Project = Request.Form["Project"].Split(',');
                db.ShopProject.Where(l => l.ShopGid == Gid).Delete();
                for (int i = 0; i < Project.Length - 1; i++)
                {
                    var sp = new ShopProject();
                    sp.Gid = Guid.NewGuid();
                    sp.AddTime = DateTime.Now;
                    sp.ShopGid = (Guid)Gid;
                    sp.Project = int.Parse(Project[i]);
                    db.ShopProject.Add(sp);
                }
                if (db.SaveChanges() >0)
                {
                    return Helper.Redirect("操作成功！", "history.go(-1);", "操作成功!");
                }
                else
                {
                    return Helper.Redirect("操作失败", "history.go(-1);", "商家基本资料更新失败,如果是更新分类忽略即可!");
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
                //会员原来的彩链金额
                decimal CLMoney = ViewBag.CLMoney = b.CLMoney;
                Guid ProductGid = Helper.GetProductGid();
                var p = db.Stock.Where(l => l.MemberGid == gid && l.ProductGid == ProductGid).FirstOrDefault();
                if (p != null)
                {
                    //会员原来的彩链库存
                    int MStock = ViewBag.Stock = p.Number;
                    //一个彩链兑换多少额度
                    decimal CLM = ViewBag.CLB = decimal.Parse(db.DictionariesList.Where(dl => dl.Key == "CLB" && dl.DGid == db.Dictionaries.Where(d => d.DictionaryType == "CL").FirstOrDefault().Gid).FirstOrDefault().Value);
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
                                Helper.CLRecordAdd(gid, Stock * CLM, CLMoney, Stock, MStock, "额度兑换");
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
            try
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
                        ViewBag.Prefix = "";
                        ViewBag.Stock = "";
                        ViewBag.Price = "";
                    }
                    Guid ShopGid = LCookie.GetShopGid();
                    var sc = db.ShopClassify.Where(l => l.ShopGid == ShopGid).ToList();
                    if (sc.Count() != 0)
                    {
                        return View(sc);
                    }
                    else
                    {
                        return Helper.Redirect("错误！", "/Shop/ShopClassify", "请先设置商品分类!");
                    }
                }
            }
            catch { return Helper.Redirect("错误！", "/Shop/Index", "请选择合适的图片!"); }
        }
        [HttpPost]
        public ActionResult ProductAU(Guid? Gid)
        {
            using (EFDB db = new EFDB())
            {
                //判断额度
                Guid gid = LCookie.GetMemberGid();
                var m = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                int Stock = int.Parse(Request.Form["Stock"]);
                decimal Price = decimal.Parse(Request.Form["Price"]);
                decimal RMB = Stock * Price;
                decimal CLMoney = m.CLMoney;
                if (CLMoney >= RMB)
                {
                    m.CLMoney = CLMoney - RMB;
                    if (Gid != null || db.SaveChanges() == 1)
                    {
                        ShopProduct b;
                        if (Gid == null)
                        {
                            Guid spgid = Guid.NewGuid();
                            Helper.CLRecordAdd(gid, -RMB, CLMoney, 0, 0, "额度扣除=" + spgid.ToString());
                            b = new ShopProduct();
                            b.Gid = spgid;
                            b.AddTime = DateTime.Now;
                            b.ShopGid = LCookie.GetShopGid();
                            string bh = db.ShopProduct.Max(l => l.Prefix);
                            if (string.IsNullOrEmpty(bh))
                            {
                                b.Prefix = "000001";
                            }
                            else
                            {
                                b.Prefix = (int.Parse(db.ShopProduct.Max(l => l.Prefix)) + 1).ToString().PadLeft(6, '0');
                            }
                            b.Stock = Stock;
                            b.Price = Price;
                            b.OriginalPrice = Price;
                            b.Show = 1;
                            b.Number = 0;
                        }
                        else
                        {
                            b = db.ShopProduct.Where(l => l.Gid == Gid).FirstOrDefault();
                        }
                        b.ClassifyGid = Guid.Parse(Request.Form["ClassifyGid"]);
                        b.Name = Request.Form["Name"];
                        b.Profile = Request.Form["Profile"];
                        b.Content = Request.Form["Content"];
                        //b.Remarks = Request.Form["Remarks"];
                        b.Sort = 1;//为0代表额度已退还
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
                    return Helper.Redirect("操作失败！", "/Shop/CLMoney", "你的额度(" + CLMoney.ToString() + ")不足发布总价(" + RMB.ToString() + ")" + ",请先兑换!");
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
                    var b = db.ShopProduct.Where(l => l.Gid == Gid).FirstOrDefault();
                    b.Show = 2;
                    if (db.SaveChanges()== 1)
                    {
                        Guid gid = LCookie.GetMemberGid();
                        decimal Money = b.Stock * b.Price;
                        var m = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                        decimal CLMoney = m.CLMoney;
                        m.CLMoney = CLMoney + Money;
                        if (db.SaveChanges() == 1)
                        {
                            Helper.CLRecordAdd(gid, Money, CLMoney, 0, 0, "额度退回="+ Gid.ToString());
                            return Json(new AjaxResult("下架成功,额度返回=" + Money.ToString()));
                        }
                        else
                        {
                            LogManager.WriteLog("下架成功额度失败", "会员=" + gid.ToString() + ",商品=" + Gid.ToString() + ",额度=" + Money.ToString());
                            return Json(new AjaxResult("下架成功,额度返回失败,请联系客服!"));
                        }
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
        /// 提现
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
        public ActionResult RMB(decimal Money = 0)
        {
            using (EFDB db = new EFDB())
            {
                Guid gid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                ViewBag.Money = b.ShopMoney;
                ViewBag.Token = decimal.Parse(db.DictionariesList.Where(dl => dl.Key == "Token" && dl.DGid == db.Dictionaries.Where(d => d.DictionaryType == "CL").FirstOrDefault().Gid).FirstOrDefault().Value);
                ViewBag.BCCB = AppApi.MB(b.Account, "BCCB");
                ViewBag.Token24 = AppApi.AVG(1);
                if (Money == 0)
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
                    if (Money < 100)
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "最少100积分起提");
                    }
                    else
                    {
                        if (ViewBag.Token24 > 0)
                        {
                            if (Money > b.ShopMoney)
                            {
                                return Helper.Redirect("失败", "history.go(-1);", "可提积分不足");
                            }
                            else
                            {
                                decimal bccb = Money * ViewBag.Token / ViewBag.Token24;
                                if (ViewBag.BCCB >= bccb)
                                {
                                    if (AppApi.UPMB(b.Account, "BCCB", bccb.ToString()))
                                    {
                                        if (Helper.RMBRecordAdd(gid, Money, 2))
                                        {
                                            var wd = new ShopWithdrawals();
                                            wd.Gid = Guid.NewGuid();
                                            wd.AddTime = DateTime.Now;
                                            wd.State = 1;
                                            wd.Money = Money;
                                            wd.Bank = b.Bank;
                                            wd.BankName = b.BankName;
                                            wd.BankNumber = b.BankNumber;
                                            wd.ShopGid = gid;
                                            wd.Token = bccb;
                                            db.ShopWithdrawals.Add(wd);
                                            if (db.SaveChanges() == 1)
                                            {
                                                return Helper.Redirect("成功", "/Shop/Money", "恭喜你,提现成功,等待财务审核后打款");
                                            }
                                            else
                                            {
                                                LogManager.WriteLog("商家提现扣除成功打款记录失败", "gid=" + gid.ToString() + ",money=" + Money.ToString() + ",bccb=" + bccb.ToString());
                                                return Helper.Redirect("失败", "history.go(-1);", "扣除成功打款记录失败");
                                            }
                                        }
                                        else
                                        {
                                            LogManager.WriteLog("商家BCCB扣除成功提现记录失败", "gid=" + gid.ToString() + ",money=" + Money.ToString() + ",bccb=" + bccb.ToString());
                                            return Helper.Redirect("失败", "history.go(-1);", "扣除失败");
                                        }
                                    }
                                    else
                                    {
                                        return Helper.Redirect("失败", "history.go(-1);", "BCCB手续费扣除失败");
                                    }
                                }
                                else
                                {
                                    return Helper.Redirect("失败", "history.go(-1);", "BCCB手续费不足");
                                }
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
        public ActionResult Money(decimal Money=0)
        {
            using (EFDB db = new EFDB())
            {
                Guid gid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                ViewBag.Money = b.ShopMoney;
                ViewBag.Token = decimal.Parse(db.DictionariesList.Where(dl => dl.Key == "Token" && dl.DGid == db.Dictionaries.Where(d => d.DictionaryType == "CL").FirstOrDefault().Gid).FirstOrDefault().Value);
                ViewBag.BCCB = AppApi.MB(b.Account, "BCCB");
                ViewBag.Token24 = AppApi.AVG(1);
                if (ViewBag.Token24 > 0)
                {
                    if (Money == 0)
                    {
                        ViewBag.Bank = b.Bank;
                        ViewBag.BankName = b.BankName;
                        ViewBag.BankNumber = b.BankNumber;
                        return View();
                    }
                    else
                    {
                        if (Money < 100)
                        {
                            return Helper.Redirect("失败", "history.go(-1);", "最少100积分起提");
                        }
                        else
                        {
                            if (Money > b.ShopMoney)
                            {
                                return Helper.Redirect("失败", "history.go(-1);", "可提积分不足");
                            }
                            else
                            {
                                decimal bccb = Money * ViewBag.Token / ViewBag.Token24;
                                if (ViewBag.BCCB >= bccb)
                                {
                                    if (AppApi.UPMB(b.Account, "BCCB", bccb.ToString()))
                                    {
                                        if (Helper.RMBRecordAdd(gid, Money, 2))
                                        {
                                            var wd = new ShopWithdrawals();
                                            wd.Gid = Guid.NewGuid();
                                            wd.AddTime = DateTime.Now;
                                            wd.State = 1;
                                            wd.Money = Money;
                                            wd.Bank = b.Bank;
                                            wd.BankName = b.BankName;
                                            wd.BankNumber = b.BankNumber;
                                            wd.ShopGid = gid;
                                            wd.Token = bccb;
                                            db.ShopWithdrawals.Add(wd);
                                            if (db.SaveChanges() == 1)
                                            {
                                                return Helper.Redirect("成功", "/Shop/Money", "恭喜你,提现成功,等待财务审核后打款");
                                            }
                                            else
                                            {
                                                LogManager.WriteLog("商家扣除成功打款记录失败", "gid=" + gid.ToString() + ",money=" + Money.ToString() + ",bccb=" + bccb.ToString());
                                                return Helper.Redirect("失败", "history.go(-1);", "扣除成功打款记录失败");
                                            }
                                        }
                                        else
                                        {
                                            LogManager.WriteLog("BCCB扣除成功增加资金失败", "gid=" + gid.ToString() + ",money=" + Money.ToString() + ",bccb=" + bccb.ToString());
                                            return Helper.Redirect("失败", "history.go(-1);", "扣除失败");
                                        }
                                    }
                                    else
                                    {
                                        return Helper.Redirect("失败", "history.go(-1);", "BCCB手续费扣除失败");
                                    }
                                }
                                else
                                {
                                    return Helper.Redirect("失败", "history.go(-1);", "BCCB手续费不足");
                                }
                            }
                        }
                    }
                }
                else
                {
                    return Helper.Redirect("失败", "history.go(-1);", "APP返回均价失败");
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
            Guid ShopGid = LCookie.GetMemberGid();
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
        public ActionResult ToAPP(decimal Integral = 0, int TB = 0, int Type = 0)
        {
            using (EFDB db = new EFDB())
            {
                Guid gid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                ViewBag.FBCC = AppApi.AVG(2);
                string T = "";//查询兑换比例类型
                if (Type == 2)
                {
                    T = "MIntegral";
                    ViewBag.Integral = b.MIntegral;//商城基数积分
                }
                else if (Type == 3)
                {
                    T = "ShopIntegral";
                    ViewBag.Integral = b.ShopIntegral;//商城积分
                }
                //查询兑换比例
                //ViewBag.MT = decimal.Parse(db.DictionariesList.Where(dl => dl.Key == T && dl.DGid == db.Dictionaries.Where(d => d.DictionaryType == "Token").FirstOrDefault().Gid).FirstOrDefault().Value);
                if (Integral <= 0 || TB <= 0 || Type <= 0)
                {
                    return View();
                }
                else
                {
                    string LogMsg = "gid=" + gid.ToString() + ",Integral=" + Integral.ToString() + ",Type=" + Type + ",TB=" + TB;
                    if (Integral > ViewBag.Integral)
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "可提积分不足");
                    }
                    else
                    {
                        //获取兑换比例积分
                        decimal Token = Integral;
                        Guid TRGid = Guid.NewGuid();
                        if (Helper.TokenRecordAdd(TRGid, gid, Integral, Token, Type, TB))
                        {
                            //提交到APP
                            if (AppApi.AddMB(b.Account, TB == 1 ? "BCCB" : "FBCC", Token.ToString()))
                            {
                                //提取基数分后判断剩下的是否满足冻结条件
                                if (Type == 2)
                                {
                                    Helper.FrozenIntegral(gid, b.MIntegral - Integral, b.TIntegral, 2, 2, "提取基数分满足冻结");
                                }
                                return Helper.Redirect("成功", "/Shop/IntegralAPP?type=" + Type, "恭喜你,兑换成功!");
                            }
                            else
                            {
                                LogManager.WriteLog(T + "积分兑换记录成功兑换APP钱包失败", LogMsg);
                                string Remarks = "钱包兑换接口失败";
                                db.TokenRecord.Where(l => l.Gid == TRGid).Update(l => new TokenRecord { Remarks = Remarks, State=2 });
                                return Helper.Redirect("失败", "/Shop/IntegralAPP?type=" + Type, "积分兑换记录成功兑换APP钱包失败");
                            }
                        }
                        else
                        {
                            LogManager.WriteLog(T + "积分兑换记录失败", LogMsg);
                            return Helper.Redirect("失败", "history.go(-1);", "积分兑换记录失败");
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
        /// <param name="Type">类型[1=彩链积分 2=商城基数积分 3=商城积分]</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult IntegralAPP(decimal Integral = 0, int TB = 0, int Type = 0)
        {
            using (EFDB db = new EFDB())
            {
                Guid gid = LCookie.GetMemberGid();
                var b = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                string T="";//查询兑换比例类型
                if (Type == 2)
                {
                    T = "MIntegral";
                    ViewBag.Integral = b.MIntegral;//商城基数积分
                }
                else if (Type == 3)
                {
                    T = "ShopIntegral";
                    ViewBag.Integral = b.ShopIntegral;//商城积分
                }
                //查询兑换比例
                ViewBag.MT = decimal.Parse(db.DictionariesList.Where(dl => dl.Key == T && dl.DGid == db.Dictionaries.Where(d => d.DictionaryType == "Token").FirstOrDefault().Gid).FirstOrDefault().Value);
                if (Integral <= 0 || TB <= 0 || Type <= 0)
                {
                    return View();
                }
                else
                {
                    string LogMsg = "gid=" + gid.ToString() + ",Integral=" + Integral.ToString() + ",Type=" + Type + ",TB=" + TB;
                    if (Integral > ViewBag.Integral)
                    {
                        return Helper.Redirect("失败", "history.go(-1);", "可提积分不足");
                    }
                    else
                    {
                        //获取兑换比例积分
                        decimal Token = Integral * ViewBag.MT;
                        Guid TRGid = Guid.NewGuid();
                        if (Helper.TokenRecordAdd(TRGid, gid, Integral, Token, Type, TB))
                        {
                            //提交到APP
                            if (AppApi.AddMB(b.Account, TB == 1 ? "BCCB" : "FBCC", Token.ToString()))
                            {
                                //提取基数分后判断剩下的是否满足冻结条件
                                if (Type == 2)
                                {
                                    Helper.FrozenIntegral(gid, b.MIntegral- Integral, b.TIntegral, 2, 2, "提取基数分满足冻结");
                                }
                                return Helper.Redirect("成功", "/Shop/IntegralAPP?type=" + Type, "恭喜你,兑换成功!");
                            }
                            else
                            {
                                LogManager.WriteLog(T + "积分兑换记录成功兑换APP钱包失败", LogMsg);
                                db.TokenRecord.Where(l => l.Gid == TRGid).Update(l => new TokenRecord { Remarks = "钱包兑换接口失败" });
                                return Helper.Redirect("失败", "/Shop/IntegralAPP?type=" + Type, "积分兑换记录成功兑换APP钱包失败");
                            }
                        }
                        else
                        {
                            LogManager.WriteLog(T + "积分兑换记录失败", LogMsg);
                            return Helper.Redirect("失败", "history.go(-1);", "积分兑换记录失败");
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
            int Type = Int32.Parse(paramJson["Type"].ToString());
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.TokenRecord.Where(l => l.MemberGid == MemberGid && l.Type == Type).AsQueryable();
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
        /// 冻结记录
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult FrozenList()
        {
            return View();
        }
        [HttpPost]
        public JsonResult FrozenListData()
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
                var b = db.ShopRecord.Where(l => l.MemberGid == MemberGid && l.Type==2).GroupJoin(db.Member,
                    l => l.MemberGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.AddTime,
                        l.TIntegral,
                        l.ThawTime,
                        l.State,
                        l.MemberGid,
                        j.FirstOrDefault().Account
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
        /// 许可证
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult Licence()
        {
            return View();
        }

        /// <summary>
        /// 证件上传
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public ActionResult UPCertificates()
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = LCookie.GetShopGid();
                var b = db.Shop.Where(l => l.Gid == Gid).FirstOrDefault();
                if (b != null)
                {
                    ViewBag.LegalPerson = Help.Shop + b.LegalPerson;
                    ViewBag.USCI = Help.Shop + b.USCI;
                    ViewBag.Licence = Help.Shop + b.Licence;
                }
                return View();
            }
        }
        [HttpPost]
        public ActionResult UPShop()
        {
            using (EFDB db = new EFDB())
            {
                Guid Gid = LCookie.GetShopGid();
                var b = db.Shop.Where(l => l.Gid == Gid).FirstOrDefault();
                string LegalPerson = Helper.jsimg(Help.Shop, Request.Form["base64Data"]);
                string USCI = Helper.jsimg(Help.Shop, Request.Form["USCI"]);
                string Licence = Helper.jsimg(Help.Shop, Request.Form["Licence"]);
                if (!string.IsNullOrEmpty(LegalPerson))
                {
                    b.LegalPerson = LegalPerson;
                }
                if (!string.IsNullOrEmpty(USCI))
                {
                    b.USCI = USCI;
                }
                if (!string.IsNullOrEmpty(Licence))
                {
                    b.Licence = Licence;
                }
                if (db.SaveChanges() == 1)
                {
                    return Helper.Redirect("上传成功", "/Shop/UPCertificates", "上传成功");
                }
                else
                {
                    return Helper.Redirect("操作失败,请检查录入的数据！", "history.go(-1);", "上传失败,请检查和你的图片!");
                }
            }
        }
        #endregion
    }
}