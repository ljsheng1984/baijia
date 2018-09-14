using EntityFramework.Extensions;
using LJSheng.Common;
using LJSheng.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LJSheng.Web.Controllers
{
    public class SMallController : Controller
    {
        // GET: SMall
        public ActionResult Index()
        {
            using (EFDB db = new EFDB())
            {
                //获取广告
                var AD = db.AD.Where(l => l.Show == 1 && l.Project == 2 && l.Sort == 9).FirstOrDefault();
                if (AD != null)
                {
                    ViewBag.ADTopPicture = AD.Picture;
                    ViewBag.ADTopUrl = AD.Url;
                }
                Guid DGid = db.Dictionaries.Where(l => l.DictionaryType == "Shop").FirstOrDefault().Gid;
                return View(db.DictionariesList.Where(dl => dl.DGid == DGid).OrderBy(dl => dl.Sort).ToList());
            }
        }

        /// <summary>
        /// 商家列表
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult IndexData()
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
                var b = db.ShopProject.GroupJoin(db.Shop,
                    l => l.ShopGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Project,
                        l.ShopGid,
                        j.FirstOrDefault().Picture,
                        j.FirstOrDefault().Name,
                        j.FirstOrDefault().Sort,
                        j.FirstOrDefault().Show,
                        j.FirstOrDefault().State
                    }).Where(l => l.Show == 1 && l.State == 2).DistinctBy(l => new { l.ShopGid }).AsQueryable();
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }
                if (!string.IsNullOrEmpty(paramJson["Project"].ToString()))
                {
                    int Project = int.Parse(paramJson["Project"].ToString());
                    b = b.Where(l => l.Project == Project);
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
        /// 产品列表
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Detail()
        {
            Guid ShopGid = Guid.Parse(Request.QueryString["ShopGid"]);
            using (EFDB db = new EFDB())
            {
                return View(db.ShopClassify.Where(l => l.Show == 1 && l.ShopGid == ShopGid).ToList());
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
        public JsonResult DetailData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            //解析参数
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            string Name = paramJson["Name"].ToString();
            int sales = int.Parse(paramJson["sales"].ToString());
            int price = int.Parse(paramJson["price"].ToString());
            using (EFDB db = new EFDB())
            {
                Guid ShopGid = Guid.Parse(paramJson["ShopGid"].ToString());
                var b = db.ShopProduct.Where(l => l.Show == 1 && l.ShopGid == ShopGid).Select(l => new
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
                    l.ClassifyGid,
                    Sales = db.OrderDetails.Where(od => od.ProductGid == l.Gid).GroupJoin(db.ShopOrder,
                    x => x.OrderGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        y.FirstOrDefault().PayStatus
                    }).Where(s => s.PayStatus == 1).Count()
                }).AsQueryable();
                if (sales == 1)
                {
                    b = b.OrderBy(l => l.Sales);
                }
                else
                {
                    b = b.OrderByDescending(l => l.Sales);
                }
                if (price == 1)
                {
                    b = b.OrderBy(l => l.Price);
                }
                else
                {
                    b = b.OrderByDescending(l => l.Price);
                }
                if (!string.IsNullOrEmpty(Name))
                {
                    b = b.Where(l => l.Name.Contains(Name));
                }

                string ClassifyGid = paramJson["ClassifyGid"].ToString();
                if (!string.IsNullOrEmpty(ClassifyGid))
                {
                    Guid CGid = Guid.Parse(ClassifyGid);
                    b = b.Where(l => l.ClassifyGid == CGid);
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
        /// 产品详情
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Product()
        {
            Guid ShopGid = Guid.Parse(Request.QueryString["ShopGid"]);
            Guid Gid = Guid.Parse(Request.QueryString["Gid"]);
            using (EFDB db = new EFDB())
            {
                var s = db.Shop.Where(l => l.Gid == ShopGid).FirstOrDefault();
                ViewBag.Address = s.Province + s.City + s.Area + s.Address;
                ViewBag.ShopName = s.Name;
                ViewBag.ContactNumber = s.ContactNumber;
                var b = db.ShopProduct.Where(l => l.Gid == Gid).FirstOrDefault();
                ViewBag.Price = b.Price;
                ViewBag.Stock = b.Stock;
                ViewBag.Picture = b.Picture;
                ViewBag.Name = b.Name;
                ViewBag.Content = b.Content;
                return View();
            }
        }

        /// <summary>
        /// 添加购物车
        /// </summary>
        /// <param name="ShopGid">商家GID</param>
        /// <param name="Gid">产品gID</param>
        /// <param name="Number">数量</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        [HttpPost]
        public JsonResult AddCart(Guid ShopGid, Guid Gid, int Number)
        {
            Guid MemberGid = LCookie.GetMemberGid();
            if (MemberGid == Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                return Json(new AjaxResult(300, "请先登录!"));
            }
            else
            {
                if (ShopOrder(Gid, ShopGid, MemberGid, Number))
                {
                    return Json(new AjaxResult(OrderRMB(MemberGid)));
                }
                else
                {
                    return Json(new AjaxResult(300, "添加失败,请重新登陆!"));
                }
            }
        }

        /// <summary>
        /// 添加购物车订单
        /// </summary>
        /// <param name="ProductGid">商品Gid</param>
        /// <param name="ShopGid">商家Gid</param>
        /// <param name="MemberGid">会员Gid</param>
        /// <param name="Number">数量</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public static bool ShopOrder(Guid ProductGid, Guid ShopGid, Guid MemberGid, int Number)
        {
            using (EFDB db = new EFDB())
            {
                bool ok = true;
                //判断当前用户在此商家有没有未付款的订单数据
                var b = db.ShopOrder.Where(l => l.ShopGid == ShopGid && l.MemberGid == MemberGid && l.PayStatus == 2 && l.ALLOrderNo == null).FirstOrDefault();
                //订单的Gid
                Guid OrderGid = Guid.NewGuid();
                if (b == null)
                {
                    b = new ShopOrder();
                    b.Gid = OrderGid;
                    b.AddTime = DateTime.Now;
                    b.MemberGid = MemberGid;
                    b.ShopGid = ShopGid;
                    b.OrderNo = RandStr.CreateOrderNO();
                    b.PayStatus = 2;
                    b.PayType = 3;
                    b.TotalPrice = 0;
                    b.Price = 0;
                    b.CouponPrice = 0;
                    b.ExpressStatus = 1;
                    b.PayPrice = 0;
                    b.Profit = 0;
                    b.ConsumptionCode = RandStr.CreateValidateNumber(8);
                    b.Status = 1;
                    db.ShopOrder.Add(b);
                    db.SaveChanges();
                }
                else
                {
                    OrderGid = b.Gid;
                }
                //增加产品
                var od = db.OrderDetails.Where(l => l.ProductGid == ProductGid && l.OrderGid == OrderGid).FirstOrDefault();
                if (od == null)
                {
                    od = new OrderDetails();
                    od.Gid = Guid.NewGuid();
                    od.AddTime = DateTime.Now;
                    od.OrderGid = OrderGid;
                    od.ProductGid = ProductGid;
                    od.Number = Number;
                    od.Price = db.ShopProduct.Where(l => l.Gid == ProductGid).FirstOrDefault().Price;
                    db.OrderDetails.Add(od);
                }
                else
                {
                    od.Number = od.Number + Number;
                    od.Price = db.ShopProduct.Where(l => l.Gid == ProductGid).FirstOrDefault().Price;
                }
                if (od.Number <= 0)
                {
                    db.OrderDetails.Where(l => l.ProductGid == ProductGid && l.OrderGid == OrderGid).Delete();
                    //订单里没有产品删除订单
                    if (db.OrderDetails.Where(l => l.OrderGid == OrderGid).Count() == 0)
                    {
                        db.ShopOrder.Where(l => l.Gid == ShopGid && l.PayStatus==2).Delete();
                    }
                }
                else
                {
                    if (db.SaveChanges() != 1)
                    {
                        ok = false;
                    }
                }
                return ok;
            }
        }

        /// <summary>
        /// 就算用户需要付款的金额
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public static decimal OrderRMB(Guid MemberGid)
        {
            using (EFDB db = new EFDB())
            {
                return db.OrderDetails.GroupJoin(db.ShopOrder,
                    l => l.OrderGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.Number,
                        l.Price,
                        j.FirstOrDefault().PayStatus,
                        j.FirstOrDefault().MemberGid
                    }).Where(l => l.MemberGid == MemberGid && l.PayStatus == 2).Select(l => new
                    {
                        rmb = l.Number * l.Price
                    }).Select(l => l.rmb).DefaultIfEmpty(0m).Sum();
            }
        }

        /// <summary>
        /// 购物车
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Cart()
        {
            Guid MemberGid = LCookie.GetMemberGid();
            if (MemberGid == Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                return new RedirectResult("/Home/Login");
            }
            else
            {
                ViewBag.RMB = OrderRMB(MemberGid);
                return View();
            }
        }

        [HttpPost]
        public JsonResult CartData()
        {
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.ShopOrder.Where(l => l.MemberGid == MemberGid && l.PayStatus == 2).Select(l => new
                {
                        l.Gid,
                        l.ShopGid,
                        l.Remarks,
                        db.Shop.Where(s=>s.Gid==l.ShopGid).FirstOrDefault().Name,
                        list = db.OrderDetails.Where(o => o.OrderGid == l.Gid).GroupJoin(db.ShopProduct,
                                x => x.ProductGid,
                                y => y.Gid,
                                (x, y) => new
                                {
                                    x.Gid,
                                    x.ProductGid,
                                    x.Number,
                                    y.FirstOrDefault().Name,
                                    y.FirstOrDefault().Picture,
                                    y.FirstOrDefault().Price
                                })
                    });
                return Json(new AjaxResult(new
                {
                    count = 888888,
                    pageindex = 1,
                    list = b.ToList()
                }));
            }
        }
    }
}