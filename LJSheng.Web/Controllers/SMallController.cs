using EntityFramework.Extensions;
using LJSheng.Common;
using LJSheng.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LJSheng.Web.Controllers
{
    public class SMallController : Controller
    {
        // GET: SMall
        public ActionResult Index(string City)
        {
            using (EFDB db = new EFDB())
            {
                if (!string.IsNullOrEmpty(City))
                {
                    if (db.OpenCity.Where(l => l.City == City).Count() > 0)
                    {
                        LCookie.AddCookie("city", City, 30);
                    }
                    else
                    {
                        return Helper.Redirect("当前城市没有开通", "history.go(-1);", "当前城市没有开通");
                    }
                }
                //获取广告
                ViewBag.AD = db.AD.Where(l => l.Show == 1 && l.Project == 2 && l.Sort == 0 && l.Profile=="商城广告").ToList();
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
            string City = LCookie.GetCity();
            using (EFDB db = new EFDB())
            {
                var b = db.ShopProject.GroupJoin(db.Shop,
                    l => l.ShopGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.AddTime,
                        l.Project,
                        l.ShopGid,
                        j.FirstOrDefault().Picture,
                        j.FirstOrDefault().Name,
                        j.FirstOrDefault().Sort,
                        j.FirstOrDefault().Show,
                        j.FirstOrDefault().State,
                        j.FirstOrDefault().City
                    }).Where(l => l.Show == 1 && l.State == 2 && l.City == City).DistinctBy(l => new { l.ShopGid }).AsQueryable();
                if (paramJson["Project"].ToString()!="00")
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
                    list = b.OrderBy(l => l.Sort).ThenByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
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
        public ActionResult Detail(Guid ShopGid)
        {
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
                    l.AddTime,
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
                    list = b.OrderBy(l => l.Sort).ThenByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
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
        public ActionResult Product(Guid Gid, Guid ShopGid)
        {
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
                ViewBag.Profile = b.Profile;
                string path = "/uploadfiles/shop/" + ShopGid + "/" + Gid + "/";
                List<FileInfo> files = new List<FileInfo>();
                ///获取文件列表信息  
                if (Directory.Exists(System.Web.HttpContext.Current.Server.MapPath(path)))
                {
                    foreach (var file in Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath(path)))
                    {
                        if (!file.Contains("logo.png"))
                            files.Add(new FileInfo(file));
                    }
                }
                ViewBag.path = path;
                ///查询文件列表信息  
                var filevalues = from file in files
                                 orderby file.CreationTime descending
                                 select file;
                return View(filevalues.ToList());
            }
        }

        /// <summary>
        /// 添加购物车
        /// </summary>
        /// <param name="ShopGid">商家Gid</param>
        /// <param name="Gid">产品Gid</param>
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
                return Json(new AjaxResult(301, "请先登录!"));
            }
            else
            {
                if (ShopOrder(Gid, ShopGid, MemberGid, Number))
                {
                    return Json(new AjaxResult(Helper.OrderRMB(MemberGid)));
                }
                else
                {
                    return Json(new AjaxResult(300, "库存不足"));
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
                //商品信息
                var p = db.ShopProduct.Where(l => l.Gid == ProductGid).FirstOrDefault();
                //判断当前用户在此商家有没有未付款的订单数据
                var b = db.Cart.Where(l => l.ShopGid == ShopGid && l.MemberGid == MemberGid).FirstOrDefault();
                //订单的Gid
                Guid OrderGid = Guid.NewGuid();
                if (b == null)
                {
                    b = new Cart();
                    b.Gid = OrderGid;
                    b.AddTime = DateTime.Now;
                    b.MemberGid = MemberGid;
                    b.ShopGid = ShopGid;
                    b.State = 1;
                    db.Cart.Add(b);
                    db.SaveChanges();
                }
                else
                {
                    OrderGid = b.Gid;
                }
                //增加产品
                var od = db.OrderDetails.Where(l => l.ProductGid == ProductGid && l.OrderGid == OrderGid).FirstOrDefault();
                //产品价格
                decimal Price = p.Price;
                if (od == null)
                {
                    od = new OrderDetails();
                    od.Gid = Guid.NewGuid();
                    od.AddTime = DateTime.Now;
                    od.OrderGid = OrderGid;
                    od.ProductGid = ProductGid;
                    od.Number = Number;
                    od.State = 2;
                    od.Price = Price;
                    db.OrderDetails.Add(od);
                }
                else
                {
                    od.Number = od.Number + Number;
                    od.Price = Price;
                }
                if (od.Number <= 0)
                {
                    db.OrderDetails.Where(l => l.ProductGid == ProductGid && l.OrderGid == OrderGid).Delete();
                    //订单里没有产品删除订单
                    if (db.OrderDetails.Where(l => l.OrderGid == OrderGid).Count() == 0)
                    {
                        db.Cart.Where(l => l.Gid == OrderGid).Delete();
                    }
                }
                else
                {
                    if (p.Stock >= od.Number)
                    {
                        if (db.SaveChanges() != 1)
                        {
                            ok = false;
                        }
                    }
                    else//库存不足
                    {
                        ok = false;
                    }
                }
                return ok;
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
                ViewBag.RMB = Helper.OrderRMB(MemberGid);
                return View();
            }
        }

        [HttpPost]
        public JsonResult CartData()
        {
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                var b = db.Cart.Where(l => l.MemberGid == MemberGid).Select(l => new
                {
                        l.Gid,
                        l.ShopGid,
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

        /// <summary>
        /// 团队积分明细
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult TeamIntegral()
        {
            return View();
        }
        [HttpPost]
        public JsonResult TeamIntegralData()
        {
            string json = "";
            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                json = HttpUtility.UrlDecode(sr.ReadLine());
            }
            //解析参数
            JObject paramJson = JsonConvert.DeserializeObject(json) as JObject;
            //统计月份的第一天和最后一天
            DateTime MonthFirst = DTime.FirstDayOfMonth(DateTime.Parse("2018-08-08"));
            DateTime MonthLast = DTime.LastDayOfMonth(DateTime.Now);
            string date = paramJson["date"].ToString();
            if (!string.IsNullOrEmpty(date))
            {
                //统计月份的第一天和最后一天
                MonthFirst = DTime.FirstDayOfMonth(DateTime.Parse(date + "-" + "01"));
                MonthLast = DTime.LastDayOfMonth(DateTime.Parse(date + "-" + "01 23:59:59"));
            }
            using (EFDB db = new EFDB())
            {
                Guid MGid = LCookie.GetMemberGid(); ;
                var b = db.ShopRecord.Where(l => l.MemberGid == MGid && l.Type==3 && l.AddTime >= MonthFirst && l.AddTime <= MonthLast).GroupJoin(db.ShopOrder,
                    l => l.OrderGid,
                    j => j.Gid,
                    (l, j) => new
                    {
                        l.AddTime,
                        l.MIntegral,
                        j.FirstOrDefault().OrderNo
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
        /// 商家详情
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Shop(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                return View(db.Shop.Where(l => l.Gid == Gid).ToList());
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
        [HttpPost]
        public JsonResult ShopData()
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
                Guid ShopGid = Guid.Parse(paramJson["ShopGid"].ToString());
                var b = db.ShopProduct.Where(l => l.Show == 1 && l.ShopGid == ShopGid).Select(l => new
                {
                    l.Gid,
                    l.AddTime,
                    l.ShopGid,
                    l.Prefix,
                    l.Name,
                    l.Sort,
                    l.Picture,
                    l.Price,
                    l.OriginalPrice,
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
                    list = b.OrderBy(l => l.Sort).ThenByDescending(l => l.AddTime).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }

        /// <summary>
        /// 搜索页面
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Search()
        {
            using (EFDB db = new EFDB())
            {
                return View();
            }
        }

        /// <summary>
        /// 分类搜索
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult Classify(Guid ShopGid)
        {
            using (EFDB db = new EFDB())
            {
                return View(db.ShopClassify.Where(l => l.Show == 1 && l.ShopGid == ShopGid).ToList());
            }
        }

        /// <summary>
        /// 搜索列表
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public ActionResult SProduct()
        {
            return View();
        }
        /// <summary>
        /// 搜索列表
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
        public JsonResult SProductData()
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
                var b = db.ShopProduct.Where(l => l.Show == 1).Select(l => new
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
                    Sales = db.OrderDetails.Where(od => od.ProductGid == l.Gid).GroupJoin(db.ShopOrder,
                    x => x.OrderGid,
                    y => y.Gid,
                    (x, y) => new
                    {
                        y.FirstOrDefault().PayStatus
                    }).Where(s => s.PayStatus == 1).Count()
                }).AsQueryable();
                string Name = paramJson["Name"].ToString();
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
                    list = b.OrderByDescending(l => l.Sales).Skip(pagesize * (pageindex - 1)).Take(pagesize).ToList()
                }));
            }
        }
    }
}