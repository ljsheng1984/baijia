using System;
using System.Linq;
using System.Text;
using System.Web;
using EntityFramework.Extensions;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
using LJSheng.Data;
using static LJSheng.Web.LJShengHelper;
using LJSheng.Common;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace LJSheng.Web
{
    /// <summary>
    /// 帮助类
    /// </summary>
    public static class Helper
    {
        #region 订单操作
        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="Product">商品Gid列表</param>
        /// <param name="MemberGid">会员Gid</param>
        /// <param name="Project">项目类型</param>
        /// <param name="Type">3=系统订单 4=合伙人转让</param>
        /// <param name="PayType">支付类型</param>
        /// <param name="Remarks">备注</param>
        /// <param name="Address">快递地址</param>
        /// <param name="RealName">收货人</param>
        /// <param name="ContactNumber">联系电话</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public static string Order(string Product, Guid MemberGid, int Project, int Type, int PayType, string Remarks, string Address = "", string RealName = "", string ContactNumber = "")
        {
            using (EFDB db = new EFDB())
            {
                //订单的Gid
                Guid OrderGid = Guid.NewGuid();
                //订单总金额
                decimal TotalPrice = 0;
                //进货价
                decimal BuyPrice = 0;
                //产品名称
                string body = "";
                //添加订单产品列表
                JArray json = (JArray)JsonConvert.DeserializeObject(Product);
                //产品数量
                int Number = 0;
                //购买后可获得
                decimal Money = 0;
                decimal Integral = 0;
                decimal StockRight = 0;
                foreach (var j in json)
                {
                    Guid ProductGid = Guid.Parse(j["gid"].ToString());
                    Number = int.Parse(j["pCount"].ToString());
                    var p = db.Product.Where(l => l.Gid == ProductGid).FirstOrDefault();
                    body += p.Name + ",";
                    BuyPrice += p.BuyPrice * Number;
                    Money += p.Money;
                    Integral += p.Integral;
                    StockRight += p.StockRight;
                    //订单详情
                    var od = new OrderDetails();
                    od.Gid = Guid.NewGuid();
                    od.AddTime = DateTime.Now;
                    od.OrderGid = OrderGid;
                    od.ProductGid = (Guid)ProductGid;
                    od.Number = Number;
                    od.Price = p.Price;
                    db.OrderDetails.Add(od);
                }
                if (db.SaveChanges() == json.Count())
                {
                    body = body.TrimEnd(',');
                    if (body.Length > 128)
                    {
                        body = body.Substring(0, 120) + "...";
                    }
                    //订单总价
                    TotalPrice = db.OrderDetails.Where(l => l.OrderGid == OrderGid).Sum(l => l.Price * l.Number);
                    //生成订单
                    var b = new Order();
                    b.Gid = OrderGid;
                    b.AddTime = DateTime.Now;
                    b.OrderNo = RandStr.CreateOrderNO();
                    b.MemberGid = MemberGid;
                    b.PayStatus = 2;
                    b.PayType = PayType;
                    b.TotalPrice = TotalPrice;
                    b.Price = TotalPrice;
                    b.PayPrice = 0;
                    b.CouponPrice = 0;
                    b.ExpressStatus = 1;
                    b.Address = Address;
                    b.RealName = RealName;
                    b.ContactNumber = ContactNumber;
                    b.Remarks = Remarks;
                    b.Product = body;
                    b.Status = Type;
                    b.Profit = TotalPrice - BuyPrice;
                    b.Money = Money;
                    b.Integral = Integral;
                    b.StockRight = StockRight;
                    b.Project = Project;
                    b.Type = Type;
                    db.Order.Add(b);
                    if (db.SaveChanges() == 1)
                    {
                        //body =商品名称
                        //TotalPrice =支付金额
                        //OrderNo =订单号
                        //OrderGid =订单Gid
                        return JsonConvert.SerializeObject(new { body, TotalPrice, b.OrderNo, OrderGid });
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
                return null;
            }
        }
        /// <summary>
        /// 彩链下单
        /// </summary>
        /// <param name="Type">订单类型[3=公司发货 4=会员发货 5=会员转让]</param>
        /// <param name="MNumber">转让数量</param>
        /// <param name="ProductList">商品Gid列表</param>
        /// <param name="MemberGid">会员Gid</param>
        /// <param name="Project">项目类型</param>
        /// <param name="PayType">支付类型</param>
        /// <param name="Remarks">备注</param>
        /// <param name="Address">快递地址</param>
        /// <param name="RealName">收货人</param>
        /// <param name="ContactNumber">联系电话</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public static string CLOrder(int Type, int MNumber, string ProductList, Guid MemberGid, int Project, int PayType, string Remarks = "", string Address = "", string RealName = "", string ContactNumber = "")
        {
            using (EFDB db = new EFDB())
            {
                string msg = "MemberGid=" + MemberGid.ToString();
                //订单的Gid
                Guid OrderGid = Guid.NewGuid();
                //商品Gid,有订单号为会员转让商品
                Guid ProductGid = Type == 5 ? Helper.GetProductGid() : Guid.Parse(ProductList);
                //商品信息
                var Product = db.Product.Where(l => l.Gid == ProductGid).Select(l => new
                {
                    l.Gid,
                    l.Name,
                    l.Price,
                    l.Remarks,
                    l.MPrice,
                    l.BuyPrice,
                    l.StockRight,
                    l.Stock,
                    AllStock = l.Stock + l.GiveStock
                }).FirstOrDefault();
                //查询下单会员的数据
                Member Member = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                //是否第一次购买
                //int MO = db.Order.Where(l => l.Project == 2 && l.MemberGid == MemberGid && l.PayStatus == 1 && l.Type == 3).Count();
                //发货人的的会员Gid
                Guid ShopGid = MemberGid;//转售商品类型时候,商家Gid为会员的Gid
                //购买增加的库存
                int Number = Product.AllStock;
                //需要支付金额
                decimal TotalPrice = Member.BuyPrice == 0 ? Product.Price : Member.BuyPrice * Product.Stock;
                //要是会员价大于实际商品价格按商品价格
                if (TotalPrice > Product.Price)
                {
                    TotalPrice = Product.Price;
                }
                //获得的积分和购物积分
                decimal Money = 0;
                decimal Integral = 0;
                //扣除状态判断
                bool DS = false;
                //公司发货 或 会员发货订单
                if (Type != 5)
                {
                    #region 分享奖计算 积分分享奖规则8月去掉
                    //第一次购买的话分享奖给购买会员的推荐人
                    //Guid? MGid = MO == 0 ? Member.MemberGid : MemberGid;
                    //if (MO != 0 || (MO == 0 && MGid != null))
                    //{
                    //    //分享奖的彩链等级
                    //    int CLLevel = Member.CLLevel;
                    //    //分享奖给推荐人,重设为查询推荐人的等级参数
                    //    if (MO == 0 && MGid != null)
                    //    {
                    //        CLLevel = db.Member.Where(l => l.Gid == MGid).FirstOrDefault().CLLevel;
                    //    }
                    //    //查询购买会员的等级参数(会员第一次购买则是购买会员推荐人的等级参数)
                    //    var b = db.Level.Where(l => l.LV == CLLevel).FirstOrDefault();
                    //    //分享奖比例
                    //    decimal Recommendation = b.Recommendation;
                    //    //购物比例
                    //    decimal ShopProfit = b.ShopProfit;
                    //    //分享奖
                    //    if (Recommendation > 0)
                    //    {
                    //        //拿多少比例
                    //        Money = TotalPrice * Recommendation;
                    //        //多少比例的购物积分比例
                    //        Integral = Money * ShopProfit;
                    //        //积分=总比例积分-购物比例
                    //        Money = Money - Integral;
                    //    }
                    //    else
                    //    {
                    //        msg += "没有分享奖比例";
                    //    }
                    //}
                    //else
                    //{
                    //    msg += "分享奖条件不满足";
                    //}
                    #endregion

                    //判断用户购买产品是否会升级
                    int CLLevel = Member.CLLevel;//原等级
                    int UPCLLevel = Member.CLLevel;//需要升级的等级
                    if (CLLevel < 26)
                    {
                        UPCLLevel = MLevel(MemberGid, TotalPrice, db.Level.Where(l => l.LV > 20).ToList(), CLLevel, UPCLLevel);
                    }
                    //会员第一次购买,合伙人购买,合伙人产品 全部公司发货
                    if (UPCLLevel > CLLevel || Member.CLLevel > 24 || Product.Remarks == "合伙人商品")
                    {
                        Type = 3;
                        ShopGid = GetConsignor();//公司发货默认发货人
                    }
                    //会员发货
                    else
                    {
                        //设置发货人
                        ShopGid = GetConsignor(MemberGid);
                        //是公司发货的发货人,订单变成公司订单
                        if (ShopGid == GetConsignor())
                        {
                            Type = 3;
                        }
                    }
                }
                //会员转让产品
                else
                {
                    Type = 5;
                    //转让的库存和价格(进货价X转让数量X0.75)
                    TotalPrice = Member.BuyPrice * MNumber * 0.75M;
                    Number = MNumber;
                }
                //扣除库存,转让产品没有积分
                DS = CLStock(OrderGid, MemberGid, ShopGid, Number, Money, Integral, Member.Account);
                msg += ",ShopGid=" + ShopGid.ToString();
                //没有返回失败消息,下单
                if (DS)
                {
                    //订单详情
                    var od = new OrderDetails();
                    od.Gid = Guid.NewGuid();
                    od.AddTime = DateTime.Now;
                    od.OrderGid = OrderGid;
                    od.ProductGid = ProductGid;
                    od.Number = Number;
                    //是第一单积分全部为0,因为要给推荐人
                    //od.Money = MO == 0 ? 0 : Money;
                    //od.Integral = MO == 0 ? 0 : Integral;
                    od.Money = 0;
                    od.Integral = 0;
                    od.Price = TotalPrice;
                    db.OrderDetails.Add(od);
                    if (db.SaveChanges() == 1)
                    {
                        //生成订单
                        var b = new Order();
                        b.Gid = OrderGid;
                        b.AddTime = DateTime.Now;
                        b.OrderNo = RandStr.CreateOrderNO();
                        //生成转售商品的时候发货人和购买会员都是一样的
                        b.MemberGid = MemberGid;
                        b.ShopGid = ShopGid;
                        b.CLLevel = Member.CLLevel;
                        b.PayStatus = 2;
                        b.PayType = PayType;
                        b.TotalPrice = TotalPrice;
                        b.Price = TotalPrice;
                        b.PayPrice = 0;
                        b.CouponPrice = Product.MPrice;
                        b.ExpressStatus = 1;
                        b.Address = Address;
                        b.RealName = RealName;
                        b.ContactNumber = ContactNumber;
                        b.Remarks = Remarks;
                        b.Product = Product.Name;
                        b.Status = 1;
                        b.Profit = TotalPrice - Product.BuyPrice;
                        //不是第一单没有推荐人的积分
                        //b.Money = MO != 0 ? 0 : Money;
                        //b.Integral = MO != 0 ? 0 : Integral;
                        b.Money = 0;
                        b.Integral = 0;
                        b.StockRight = Product.StockRight;
                        b.Project = Project;
                        b.Type = Type;
                        db.Order.Add(b);
                        if (db.SaveChanges() == 1)
                        {
                            //body =商品名称
                            //TotalPrice =支付金额
                            //OrderNo =订单号
                            //OrderGid =订单Gid
                            return JsonConvert.SerializeObject(new { body = b.Product, TotalPrice, b.OrderNo, OrderGid });
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
                    return null;
                }
                //扣除库存积分出错
                else
                {
                    return JsonConvert.SerializeObject(new { OrderNo = "", Title = "发货人库存不足", Error = "发货人库存不足，已短信通知你的发货人补货！" });
                }
            }
        }

        /// <summary>
        /// 支付成功更新订单
        /// </summary>
        /// <param name="OrderNo">网站订单号</param>
        /// <param name="TradeNo">网银订单号</param>
        /// <param name="PayType">支付类型</param>
        /// <param name="PayPrice">在线支付金额</param>
        /// <returns>返回调用结果</returns>
        public static bool PayOrder(string OrderNo, string TradeNo, int PayType, decimal PayPrice)
        {
            bool Pay = false;
            string rn = "\r\n-----------------------------------------------------\r\n";
            string msg = "订单号:" + OrderNo + ",网银订单号:" + TradeNo + ",支付类型:" + PayType.ToString() + ",网上支付金额:" + PayPrice.ToString() + rn;
            try
            {
                using (EFDB db = new EFDB())
                {
                    //支付类型
                    string payname = ((PayType)Enum.Parse(typeof(LJShengHelper.PayType), PayType.ToString())).ToString();
                    var b = db.Order.Where(l => l.OrderNo == OrderNo).FirstOrDefault();
                    if (b != null && b.PayStatus == 2)
                    {
                        Guid MGid = b.MemberGid;//下单会员Gid
                        Guid? ShopGid = b.ShopGid;//发货人/商家Gid
                        Guid OrderGid = b.Gid;//订单Gid
                        msg += "购买会员=" + MGid.ToString() + ",发货会员=" + (ShopGid == null ? "没有发货人" : ShopGid.ToString()) + rn;
                        PayPrice = b.Price;//测试要删
                        b.PayStatus = b.Price == PayPrice ? 1 : 5;
                        b.TradeNo = TradeNo;
                        b.PayTime = DateTime.Now;
                        b.PayType = PayType;
                        b.PayPrice = PayPrice;
                        b.ExpressStatus = 1;
                        if (db.SaveChanges() == 1)
                        {
                            Pay = true;
                            if (b.PayStatus == 1)
                            {
                                #region 彩链对账逻辑
                                if (b.Type != 5)//计算分成
                                {
                                    //购买用户数据
                                    Member Member = db.Member.Where(l => l.Gid == MGid).FirstOrDefault();
                                    Guid? MemberGid = Member.MemberGid;//推荐人的Gid
                                    //等级列表
                                    List<Level> lv = db.Level.Where(l => l.LV > 20).ToList();
                                    //非创始人需要判断是否升级
                                    int CLLevel = Member.CLLevel;//原等级
                                    int UPCLLevel = Member.CLLevel;//需要升级的等级
                                    if (CLLevel < 26)
                                    {
                                        UPCLLevel = MLevel(MGid, PayPrice, lv, CLLevel, UPCLLevel);
                                    }
                                    else
                                    {
                                        msg += "无须升级=" + CLLevel.ToString() + rn;
                                    }
                                    //升级奖
                                    if (UPCLLevel > CLLevel)
                                    {
                                        //更新进货价
                                        if (Member.BuyPrice == 0 || b.CouponPrice < Member.BuyPrice)
                                        {
                                            Member.BuyPrice = b.CouponPrice;//进货价
                                        }
                                        Member.CLLevel = UPCLLevel;
                                        Member.UPTime = DateTime.Now;
                                        if (db.SaveChanges() == 1)
                                        {
                                            msg += "会员升级成功原等级=" + CLLevel.ToString() + ",升等级=" + UPCLLevel.ToString();
                                            //更新发货人-防止上级以上有升级到发货人,所以要先处理自己作为发货人的情况
                                            if (UPCLLevel > 24)
                                            {
                                                msg += "，更新发货人=" + Consignor(MGid) + rn;
                                            }
                                            //判断上级团队升级条件
                                            if (MemberGid != null)
                                            {
                                                DateTime dt = DateTime.Now.AddMonths(-1);
                                                msg += "团队升级=" + CLTeam((Guid)MemberGid, lv) + rn;
                                                //升级奖
                                                var ML = db.Member.Where(l => l.Gid == MemberGid).GroupJoin(db.Level,
                                                                l => l.CLLevel,
                                                                j => j.LV,
                                                                (l, j) => new
                                                                {
                                                                    j.FirstOrDefault().Recommendation,
                                                                    j.FirstOrDefault().ShopProfit
                                                                }).FirstOrDefault();
                                                if (ML.Recommendation > 0)
                                                {
                                                    decimal Money = PayPrice * ML.Recommendation;
                                                    decimal Integral = Money * ML.ShopProfit;
                                                    if (MoneyRecordAdd(OrderGid, (Guid)MemberGid, Money - Integral, Integral, 20, "升级奖 百分" + ML.Recommendation * 100) == null)
                                                    {
                                                        msg += "升级奖失败:会员=" + MemberGid.ToString() + ",Price=" + PayPrice.ToString() + rn;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            msg += "会员升级失败:原等级=" + CLLevel.ToString() + ",升等级=" + UPCLLevel.ToString() + rn;
                                        }
                                    }
                                    else
                                    {
                                        //发货人截止的分享差奖励
                                        if (b.Type == 4 && MemberGid != null)
                                        {
                                            //购买会员彩链分享比例
                                            var ML = db.Member.Where(l => l.Gid == MGid).GroupJoin(db.Level,
                                                                l => l.CLLevel,
                                                                j => j.LV,
                                                                (l, j) => new
                                                                {
                                                                    j.FirstOrDefault().Recommendation
                                                                }).FirstOrDefault();
                                            msg += "发货人截止=" + Consignor((Guid)MemberGid, PayPrice, OrderGid, lv, ML.Recommendation, CLLevel, (Guid)ShopGid) + rn;
                                        }
                                    }

                                    //团队业绩和级差积分
                                    msg += "团队业绩=" + CLTeamMoney(MGid, PayPrice, OrderGid, b.Type) + rn;
                                }
                                else
                                {
                                    msg += "订单不进行处理Type=" + b.Type.ToString() + rn;
                                }

                                //增加购买人的库存
                                msg += "增加彩链库存=" + AddCLStock(OrderGid, MGid) + rn;

                                //发货人增加货款
                                if (ShopGid != null)
                                {
                                    if (db.Member.Where(l => l.Gid == b.ShopGid).Update(l => new Member { ProductMoney = l.ProductMoney + PayPrice }) != 1)
                                    {
                                        msg += "发货人的订单收入失败:金额=" + PayPrice.ToString() + rn;
                                    }
                                    else
                                    {
                                        msg += "发货人的订单收入金额 =" + PayPrice.ToString() + rn;
                                    }
                                }
                                else
                                {
                                    msg += "没有发货人" + rn;
                                }

                                #endregion

                                LogManager.WriteLog(payname + "对账记录", msg);
                            }
                            else
                            {
                                LogManager.WriteLog(payname + "金额不对", msg);
                            }
                        }
                        else
                        {
                            LogManager.WriteLog(payname + "支付成功更新订单失败", msg);
                        }
                    }
                    else
                    {
                        Pay = true;
                        LogManager.WriteLog(payname + "订单已对账", msg);
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("对账异常", msg + rn + err.Message);
            }
            return Pay;
        }

        /// <summary>
        /// 彩链团队业绩
        /// </summary>
        /// <param name="MemberGid">购买会员Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string CLTeamMoney(Guid MemberGid, decimal Price, Guid OrderGid, int Type)
        {
            string msg = "";
            try
            {
                using (EFDB db = new EFDB())
                {
                    var mr = db.MRelation.Where(l => l.MemberGid == MemberGid).Select(l => new
                    {
                        l.M1,
                        l.M2,
                        l.M3,
                        LM1 = l.M1 == null ? 0 : db.Member.Where(m => m.Gid == l.M1).FirstOrDefault().CLLevel,
                        LM2 = l.M2 == null ? 0 : db.Member.Where(m => m.Gid == l.M2).FirstOrDefault().CLLevel,
                        LM3 = l.M3 == null ? 0 : db.Member.Where(m => m.Gid == l.M3).FirstOrDefault().CLLevel
                    }).FirstOrDefault();
                    if (mr != null)
                    {
                        int Year = DateTime.Now.Year;
                        int Month = DateTime.Now.Month;
                        //自己购买业绩增加
                        if (Type == 3)
                        {
                            msg += Achievement("累计团队业绩", MemberGid, Year, Month, Price, Price);
                        }
                        if (mr.M1 != null)
                        {
                            //积分和购物积分
                            decimal Money = 0;
                            decimal Integral = 0;
                            //团队分红的参数
                            var LV = db.LV.Where(l => l.LVID == 25).ToList();
                            //第一级团队抽成
                            if (mr.LM1 > 24)
                            {
                                var LVM1 = LV.Where(l => l.Number == 1).FirstOrDefault();
                                Money = Price * LVM1.Differential;
                                Integral = Money * LVM1.ShopProfit;
                                if (MoneyRecordAdd(OrderGid, (Guid)mr.M1, Money - Integral, Integral, 22, "第1级级差 百分" + LVM1.Differential*100) == null)
                                {
                                    msg += "级差失败:会员=" + mr.M1.ToString() + ",Price=" + Price.ToString();
                                }
                            }
                            if (Type == 3)
                            {
                                msg += Achievement("累计团队业绩", (Guid)mr.M1, Year, Month, Price, 0);
                            }
                            if (mr.M2 != null)
                            {
                                if (mr.LM2 > 24)
                                {
                                    var LVM2 = LV.Where(l => l.Number == 2).FirstOrDefault();
                                    Money = Price * LVM2.Differential;
                                    Integral = Money * LVM2.ShopProfit;
                                    if (MoneyRecordAdd(OrderGid, (Guid)mr.M2, Money - Integral, Integral, 22, "第2级级差 百分" + LVM2.Differential * 100) == null)
                                    {
                                        msg += "级差失败:会员=" + mr.M2.ToString() + ",Price=" + Price.ToString();
                                    }
                                }
                                if (Type == 3)
                                {
                                    msg += Achievement("累计团队业绩", (Guid)mr.M2, Year, Month, Price, 0);
                                }
                                if (mr.M3 != null)
                                {
                                    if (mr.LM3 > 24)
                                    {
                                        var LVM3 = LV.Where(l => l.Number == 3).FirstOrDefault();
                                        Money = Price * LVM3.Differential;
                                        Integral = Money * LVM3.ShopProfit;
                                        if (MoneyRecordAdd(OrderGid, (Guid)mr.M3, Money - Integral, Integral, 22, "第3级级差 百分" + LVM3.Differential * 100) == null)
                                        {
                                            msg += "级差失败:会员=" + mr.M3.ToString() + ",Price=" + Price.ToString();
                                        }
                                    }
                                    if (Type == 3)
                                    {
                                        msg += Achievement("累计团队业绩", (Guid)mr.M3, Year, Month, Price, 0);
                                        //超过三级的业绩无限查找上一级累计
                                        CSR((Guid)mr.M3, Year, Month, Price);
                                    }
                                }
                            }
                        }
                        else
                        {
                            msg += "第一级没有会员";
                        }
                    }
                    else
                    {
                        msg = "会员关系数据不存在";
                    }
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return msg;
        }

        /// <summary>
        /// 发货人截止的分享差奖励
        /// </summary>
        /// <param name="MemberGid">推荐人Gid</param>
        /// <param name="Price">购买金额</param>
        /// <param name="OrderGid">订单Gid</param>
        /// <param name="Level">等级列表</param>
        /// <param name="Recommendation">当前的分享比例</param>
        /// <param name="CLLevel">购买会员等级</param>
        /// <param name="ShopGid">发货人</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string Consignor(Guid MemberGid, decimal Price, Guid OrderGid, List<Level> Level,decimal Recommendation,int CLLevel,Guid ShopGid)
        {
            string msg = "";
            try
            {
                using (EFDB db = new EFDB())
                {
                    var ML = db.Member.Where(l => l.Gid == MemberGid).GroupJoin(db.Level,
                                                            l => l.CLLevel,
                                                            j => j.LV,
                                                            (l, j) => new
                                                            {
                                                                l.Gid,
                                                                l.MemberGid,
                                                                l.CLLevel,
                                                                j.FirstOrDefault().Recommendation,
                                                                j.FirstOrDefault().ShopProfit
                                                            }).FirstOrDefault();
                    if (ML != null)
                    {
                        //上级级别大于购买会员的级别
                        if (ML.CLLevel > CLLevel)
                        {
                            //补货分享奖
                            decimal LMR = ML.Recommendation - Recommendation;
                            if (LMR > 0)
                            {
                                //积分和购物积分
                                decimal Money = Price * LMR;
                                decimal Integral = Money * ML.ShopProfit;
                                if (MoneyRecordAdd(OrderGid, MemberGid, Money - Integral, Integral, 20, "补货分享奖 百分" + LMR * 100) == null)
                                {
                                    msg += "补货分享奖失败:会员=" + MemberGid.ToString() + ",Price=" + Price.ToString();
                                }
                            }
                        }
                        //非发货人继续寻找
                        if (ML.MemberGid != null && MemberGid != ShopGid)
                        {
                            Consignor((Guid)ML.MemberGid, Price, OrderGid, Level, ML.Recommendation, ML.CLLevel, ShopGid);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return msg;
        }
        /// <summary>
        /// 三级以后的统计创始人的业绩算法
        /// </summary>
        /// <param name="Gid">注册用户</param>
        /// <returns>请求结果</returns>
        public static void CSR(Guid Gid, int Year, int Month, decimal Price)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                if (b.MemberGid != null)
                {
                    Achievement("累计团队业绩", (Guid)b.MemberGid, Year, Month, Price, 0);
                    CSR((Guid)b.MemberGid, Year, Month, Price);
                }
            }
        }
        /// <summary>
        /// 团队业绩累计
        /// </summary>
        public static string Achievement(string Name, Guid MemberGid, int Year, int Month, decimal TMoney = 0, decimal MMoney = 0, int State = 0, int CLLevel = 0, decimal Money = 0, decimal Integral = 0, decimal ProjectMoney = 0, decimal ProjectIntegral = 0, Guid? StockRightMRGid = null, Guid? ProjectMRGid = null, string Remarks = "", string ProjectRemarks = "", string StockRightRemarks = "")
        {
            string msg = "=MemberGid=" + MemberGid.ToString() + "\r\n";
            //msg += "\r\n State=" + State.ToString();
            //msg += "\r\n Year=" + Year.ToString();
            //msg += "\r\n Month=" + Month.ToString();
            //msg += "\r\n TMoney=" + TMoney.ToString();
            //msg += "\r\n MMoney=" + MMoney.ToString();
            //msg += "\r\n CLLevel=" + CLLevel.ToString();
            //msg += "\r\n Money=" + Money.ToString();
            //msg += "\r\n Integral=" + Integral.ToString();
            //msg += "\r\n ProjectMoney=" + ProjectMoney.ToString();
            //msg += "\r\n ProjectIntegral=" + ProjectIntegral.ToString();
            //msg += "\r\n StockRightMRGid=" + StockRightMRGid == null ? "" : StockRightMRGid.ToString();
            //msg += "\r\n ProjectMRGid=" + ProjectMRGid == null ? "" : ProjectMRGid.ToString();
            //msg += "\r\n Remarks=" + Remarks.ToString();
            //msg += "\r\n ProjectRemarks=" + ProjectRemarks.ToString();
            //msg += "\r\n StockRightRemarks=" + StockRightRemarks.ToString();
            using (EFDB db = new EFDB())
            {
                bool tb = false;
                Achievement b = db.Achievement.Where(l => l.Year == Year && l.Month == Month && l.MemberGid == MemberGid).FirstOrDefault();
                if (b == null)
                {
                    tb = true;
                    b = new Achievement();
                    b.Gid = Guid.NewGuid();
                    b.AddTime = DateTime.Now;
                    b.Year = Year;
                    b.Month = Month;
                    b.MemberGid = MemberGid;
                    b.State = 1;
                    b.CLLevel = 0;
                    b.TMoney = TMoney;
                    b.MMoney = MMoney;
                    //项目分红
                    b.ProjectMoney = ProjectMoney;
                    b.ProjectIntegral = ProjectIntegral;
                    //股东分红
                    b.Money = Money;
                    b.Integral = Integral;
                }
                else
                {
                    //自己的业绩
                    b.MMoney = b.MMoney + MMoney;
                    //项目分红
                    b.ProjectMoney = ProjectMoney;
                    b.ProjectIntegral = ProjectIntegral;
                    //股东分红
                    b.Money = Money;
                    b.Integral = Integral;
                    //团队业绩
                    b.TMoney = b.TMoney + TMoney;
                }
                if (tb)
                {
                    db.Achievement.Add(b);
                }
                if (db.SaveChanges() == 1)
                {
                    return Name + "成功" + msg;
                }
                else
                {
                    return Name + "失败" + msg;
                }
            }
        }


        /// <summary>
        /// 用户购买产品是否升级
        /// </summary>
        /// <param name="MemberGid">推荐人Gid</param>
        /// <param name="Price">支付金额</param>
        /// <param name="Level">等级列表</param>
        /// <param name="CLLevel">购买会员等级</param>
        /// <param name="UPCLLevel">升级等级</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static int MLevel(Guid MemberGid, decimal Price, List<Level> Level, int CLLevel, int UPCLLevel)
        {
            try
            {
                using (EFDB db = new EFDB())
                {
                    //消费总金额
                    decimal AllOrder = db.Order.Where(l => l.MemberGid == MemberGid && l.Project == 2 && l.PayStatus == 1).Select(l => l.Price).DefaultIfEmpty(0m).Sum();
                    //查找可让会员升级的级别列表
                    var uplv = Level.Where(l => l.LV > CLLevel).OrderBy(l => l.LV);
                    //升级条件判断
                    foreach (var dr in uplv)
                    {
                        //如果升级条件是一次性
                        if (dr.UP == 1)
                        {
                            AllOrder = Price;
                        }
                        //购买金额达到条件升级,创始人不能按此条件升级
                        if (dr.LV != 26 && AllOrder >= dr.BuyAmount)
                        {
                            UPCLLevel = dr.LV;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("判断用户购买升级异常", err.Message);
            }
            return UPCLLevel;
        }

        /// <summary>
        /// 彩链购买用户的推荐人是否升级
        /// </summary>
        /// <param name="MemberGid">升级会员的Gid</param>
        /// <param name="lv">等级参数列表</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string CLTeam(Guid MemberGid, List<Level> lv,string msg="")
        {
            try
            {
                using (EFDB db = new EFDB())
                {
                    //获取升级会员的数据
                    var m = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                    if (m.CLLevel < 26)
                    {
                        #region 判断升级没有
                        var MTeam = db.Member.Where(l => l.MemberGid == MemberGid).ToList();
                        int CLLevel21 = MTeam.Where(l => l.CLLevel >= 21).Count();//满足VIP的人数
                        int CLLevel22 = MTeam.Where(l => l.CLLevel >= 22).Count();//满足代理商的人数
                        int CLLevel23 = MTeam.Where(l => l.CLLevel >= 23).Count();//满足合伙人的人数
                        int CLLevel24 = MTeam.Where(l => l.CLLevel >= 24).Count();//满足联合创始人的人数
                        int CLLevel25 = MTeam.Where(l => l.CLLevel >= 25).Count();//满足创始人的人数
                        int Level22 = lv.Where(l => l.LV == 21).FirstOrDefault().LNumber;//升级需要VIP的人数
                        int Level23 = lv.Where(l => l.LV == 22).FirstOrDefault().LNumber;//升级需要代理商的人数
                        int Level24 = lv.Where(l => l.LV == 23).FirstOrDefault().LNumber;//升级需要合伙人的人数
                        int Level25 = lv.Where(l => l.LV == 24).FirstOrDefault().LNumber;//升级需要联合创始人的人数
                        int Level26 = lv.Where(l => l.LV == 25).FirstOrDefault().LNumber;//升级需要创始人的人数
                        //当前等级
                        int CLLevel = m.CLLevel;
                        //升级的等级
                        int UPLevel = m.CLLevel;
                        if (CLLevel25 >= Level26)
                        {
                            UPLevel = 26;
                        }
                        else
                        {
                            if (CLLevel24 >= Level25)
                            {
                                UPLevel = 25;
                            }
                            else
                            {
                                if (CLLevel23 >= Level24)
                                {
                                    UPLevel = 24;
                                }
                                else
                                {
                                    if (CLLevel22 >= Level23)
                                    {
                                        UPLevel = 23;
                                    }
                                    else
                                    {
                                        if (CLLevel21 >= Level22)
                                        {
                                            UPLevel = 22;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        //满足升级条件
                        if (UPLevel > CLLevel)
                        {
                            m.CLLevel = UPLevel;
                            if (db.SaveChanges() == 1)
                            {
                                msg += "团队升级会员="+ MemberGid.ToString() + "原等级="+ CLLevel + ",升级到=" + UPLevel.ToString();
                                //更新发货人
                                if (UPLevel > 24)
                                {
                                    Helper.Consignor(MemberGid);
                                }
                                //继续升级上面的推荐人
                                if (m.MemberGid != null)
                                {
                                    msg += CLTeam((Guid)m.MemberGid, lv, msg);
                                }
                            }
                            else
                            {
                                msg += "团队升级失败:会员=" + MemberGid.ToString() + "原等级=" + CLLevel + ",升级到=" + UPLevel.ToString();
                            }
                        }
                    }
                    else
                    {
                        msg += "最高等级了";
                    }
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return msg;
        }

        /// <summary>
        /// 购买用户的推荐人是否升级
        /// </summary>
        /// <param name="OldLevel">购买用户之前的等级</param>
        /// <param name="NewLevel">用户消费金额的等级</param>
        /// <param name="PayPrice">支付金额</param>
        /// <param name="lv">等级参数列表</param>
        /// <param name="list">会员关系列表</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string Team(int OldLevel, int NewLevel, decimal PayPrice, List<Level> lv, List<Guid?> list)
        {
            string msg = "";
            try
            {
                using (EFDB db = new EFDB())
                {
                    #region 购买会员推荐人团队累加和升级
                    if (list.Count > 0)
                    {
                        //团队人数,金额增加
                        for (int i = 0; i < list.Count; i++)
                        {
                            //下一个升级的等级ID，最低升级主任
                            int lvid = 7;
                            Guid MemberGid = (Guid)list[i];
                            //查询推荐人
                            var mb = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                            //团队金额增加
                            mb.TMoney = mb.TMoney + (int)PayPrice;
                            //购买会员升级以后是否大于原来的等级，来判断团队成员的变化
                            if (NewLevel > OldLevel)//必须是一级
                            {
                                //增加团队条件人员
                                switch (NewLevel)
                                {
                                    case 6:
                                        mb.Level6 = mb.Level6 + 1;
                                        break;
                                    case 7:
                                        mb.Level7 = mb.Level7 + 1;
                                        mb.Level6 = mb.Level6 - 1;
                                        break;
                                    case 8:
                                        mb.Level8 = mb.Level8 + 1;
                                        mb.Level7 = mb.Level7 - 1;
                                        break;
                                    case 9:
                                        mb.Level9 = mb.Level9 + 1;
                                        mb.Level8 = mb.Level8 - 1;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            //最少要求是项目合伙人才可以升级
                            if (mb.Level >= 6)
                            {
                                if (mb.Level >= 7)
                                {
                                    lvid = mb.Level + 1;
                                }
                                //要升级的等级条件数据
                                var lb = lv.Where(l => l.LV == lvid && l.LV < 10).FirstOrDefault();
                                if (lb != null)//没有符合的升级等级
                                {
                                    //用户要升级对应会员的团队人数
                                    int TNumber = mb.Level6;
                                    switch (lvid)
                                    {
                                        case 7:
                                            TNumber = mb.Level6;
                                            break;
                                        case 8:
                                            TNumber = mb.Level7;
                                            break;
                                        case 9:
                                            TNumber = mb.Level8;
                                            break;
                                        default:
                                            break;
                                    }
                                    //满足升级
                                    if (lb.BuyAmount <= mb.TMoney && lb.LNumber <= TNumber)
                                    {
                                        mb.Level = lvid;
                                    }
                                }
                            }
                            if (db.SaveChanges() != 1)
                            {
                                msg += "团队失败 Gid=" + MemberGid.ToString() + ",TMoney=" + mb.TMoney.ToString() + ",购买用户旧级别=" + OldLevel.ToString() + ",新级别=" + NewLevel.ToString() + "推荐人原级别=" + mb.Level.ToString() + ",升级ID=" + lvid.ToString();
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return msg;
        }

        /// <summary>
        /// 推荐奖
        /// </summary>
        /// <param name="OrderGid">订单Gid</param>
        /// <param name="MemberGid">购买会员的推荐人Gid</param>
        /// <param name="PayPrice">在线支付金额</param>
        /// <param name="lv">等级参数列表</param>
        /// <param name="ShopGid">是否扣除发货人积分</param>
        /// <returns>返回调用结果</returns>
        public static string Recommend(Guid OrderGid, Guid? MemberGid, decimal PayPrice, List<Level> lv, Guid? ShopGid = null, string Remarks = "")
        {
            string msg = "";
            try
            {
                if (MemberGid != null)
                {
                    using (EFDB db = new EFDB())
                    {
                        //查询推荐人等级的参数
                        var b = lv.Where(l => l.LV == db.Member.Where(m => m.Gid == MemberGid).FirstOrDefault().CLLevel).FirstOrDefault();
                        //推荐奖比例
                        decimal Recommendation = b.Recommendation;
                        //购物比例
                        decimal ShopProfit = b.ShopProfit;
                        //推荐奖
                        if (Recommendation > 0)
                        {
                            decimal Money = PayPrice * Recommendation;
                            decimal Integral = Money * ShopProfit;
                            //是否从发货人扣除积分
                            if (ShopGid != null)
                            {
                                if (MoneyRecordAdd(OrderGid, (Guid)ShopGid, -(Money - Integral), -Integral, 20, "发货扣除积分") == null)
                                {
                                    msg += "扣除发货人积分失败:Money=-" + Money.ToString() + ",Integral=-" + Integral.ToString();
                                }
                                else
                                {
                                    msg += "扣除发货人积分Money=-" + Money.ToString() + ",Integral=-" + Integral.ToString();
                                }
                            }
                            if (MoneyRecordAdd(OrderGid, (Guid)MemberGid, Money - Integral, Integral, 20, Remarks + "购买增加积分") == null)
                            {
                                msg += "失败:Money=" + Money.ToString() + ",Integral=" + Integral.ToString();
                            }
                            else
                            {
                                msg += "Money=" + Money.ToString() + ",Integral=" + Integral.ToString();
                            }
                        }
                    }
                }
                else
                {
                    msg += "没有推荐人";
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return msg;
        }

        /// <summary>
        /// 支付分成
        /// </summary>
        /// <param name="OrderGid">订单Gid</param>
        /// <param name="Gid">购买用户的Gid</param>
        /// <param name="PayPrice">在线支付金额</param>
        /// <param name="lv">等级参数列表</param>
        /// <param name="list">会员关系列表</param>
        /// <param name="level">购买用户现在的等级</param>
        /// <returns>返回调用结果</returns>
        public static string DividedInto(Guid OrderGid, Guid Gid, decimal PayPrice, List<Level> lv, List<Guid?> list, int level)
        {
            string msg = "";
            try
            {
                using (EFDB db = new EFDB())
                {
                    //十级数据
                    var lvlist = db.LV.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        Guid MemberGid = (Guid)list[i];
                        //推荐人的等级
                        var MLevel = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault().Level;
                        //是否达到条件-最少是主任
                        if (MLevel >= 7)
                        {
                            //对应的第几级比例
                            int Number = i + 1;
                            //查询对应等级的比例
                            var ds = lvlist.Where(l => l.LVID == MLevel && l.Number == Number).FirstOrDefault();
                            //奖励比例
                            decimal Money = (level >= MLevel ? ds.SameLevel : ds.Differential) * PayPrice;
                            //购物积分
                            decimal Integral = Money * lv.Where(l => l.LV == MLevel).FirstOrDefault().ShopProfit;
                            if (Money > 0 || Integral > 0)
                            {
                                if (Helper.MoneyRecordAdd(OrderGid, MemberGid, Money - Integral, Integral, level >= MLevel ? 7 : 6) == null)
                                {
                                    msg += "奖励失败 Gid=" + MemberGid.ToString() + ",PayPrice=" + PayPrice.ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return msg;
        }

        /// <summary>
        /// 增加合伙人分红
        /// </summary>
        /// <param name="OrderGid">订单Gid</param>
        /// <param name="MemberGid">购买人Gid</param>
        /// <param name="Project">项目</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string AddStockRight(Guid OrderGid, Guid MemberGid, int Project)
        {
            string msg = "";
            try
            {
                using (EFDB db = new EFDB())
                {
                    if (db.StockRight.Where(l => l.OrderGid == OrderGid && l.MemberGid == MemberGid).Count() < 1)
                    {
                        var sr = new StockRight();
                        sr.Gid = Guid.NewGuid();
                        sr.AddTime = DateTime.Now;
                        sr.OrderGid = OrderGid;
                        sr.MemberGid = MemberGid;
                        sr.Project = Project;
                        sr.Number = 1;
                        db.StockRight.Add(sr);
                        if (db.SaveChanges() != 1)
                        {
                            msg += "增加分红记录失败";
                        }
                    }
                    else
                    {
                        msg += "增加分红已存在";
                    }
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return msg;
        }

        /// <summary>
        /// 增加合伙人库存
        /// </summary>
        /// <param name="OrderGid">订单Gid</param>
        /// <param name="MemberGid">购买人Gid</param>
        /// <param name="Project">项目</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string AddStock(Guid OrderGid, Guid MemberGid, int Project)
        {
            string msg = "";
            try
            {
                using (EFDB db = new EFDB())
                {
                    Guid ProductGid = Helper.GetProductGid();
                    var od = db.OrderDetails.Where(l => l.OrderGid == OrderGid).ToList();
                    foreach (var j in od)
                    {
                        if (Project == 1)
                        {
                            ProductGid = j.ProductGid;
                        }
                        Stock s = db.Stock.Where(l => l.ProductGid == ProductGid && l.MemberGid == MemberGid).FirstOrDefault();
                        if (s == null)
                        {
                            s = new Stock();
                            s.Gid = Guid.NewGuid();
                            s.AddTime = DateTime.Now;
                            s.MemberGid = MemberGid;
                            s.ProductGid = ProductGid;
                            s.Number = j.Number;
                            db.Stock.Add(s);
                        }
                        else
                        {
                            s.Number = s.Number + j.Number;
                        }
                    }
                    int num = db.SaveChanges();
                    if (num != od.Count)
                    {
                        msg += "失败原因=查询条数:" + od.Count() + ".实际操作条数:" + num.ToString() + "\r\n";
                    }
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return msg;
        }

        /// <summary>
        /// 增加购买人的库存,积分
        /// </summary>
        /// <param name="OrderGid">订单Gid</param>
        /// <param name="MemberGid">购买人Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string AddCLStock(Guid OrderGid, Guid MemberGid)
        {
            string msg = "";
            try
            {
                using (EFDB db = new EFDB())
                {
                    Guid ProductGid = Helper.GetProductGid();
                    var od = db.OrderDetails.Where(l => l.OrderGid == OrderGid).FirstOrDefault();
                    Stock s = db.Stock.Where(l => l.ProductGid == ProductGid && l.MemberGid == MemberGid).FirstOrDefault();
                    if (s == null)
                    {
                        s = new Stock();
                        s.Gid = Guid.NewGuid();
                        s.AddTime = DateTime.Now;
                        s.MemberGid = MemberGid;
                        s.ProductGid = ProductGid;
                        s.Number = od.Number;
                        db.Stock.Add(s);
                    }
                    else
                    {
                        s.Number = s.Number + od.Number;
                    }
                    if (db.SaveChanges() == 1)
                    {
                        msg += "增加数量=" + od.Number.ToString();
                    }
                    else
                    {
                        msg += ",增加失败:数量=" + od.Number.ToString();
                    }
                }
            }
            catch (Exception err)
            {
                msg += err.Message;
            }
            return msg;
        }

        /// <summary>
        /// 扣除产品库存
        /// </summary>
        /// <param name="OrderGid">订单Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string Stock(Guid OrderGid)
        {
            string msg = "";
            try
            {
                using (EFDB db = new EFDB())
                {
                    int num = 0;
                    var od = db.OrderDetails.Where(l => l.OrderGid == OrderGid).ToList();
                    foreach (var dr in od)
                    {
                        num++;
                        db.Product.Where(l => l.Gid == dr.ProductGid).Update(l => new Product { Stock = l.Stock - dr.Number });
                    }
                    if (num != od.Count())
                    {
                        msg += "失败原因=查询条数:" + od.Count() + ".实际操作条数:" + num.ToString() + "\r\n";
                    }
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return msg;
        }
        #endregion

        #region 商城订单
        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="PayType">支付类型</param>
        /// <param name="Product">商品Gid列表</param>
        /// <param name="ShopGid">商家Gid</param>
        /// <param name="MemberGid">会员Gid</param>
        /// <param name="Remarks">备注</param>
        /// <param name="Address">快递地址</param>
        /// <param name="RealName">收货人</param>
        /// <param name="ContactNumber">联系电话</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public static string ShopOrder(int PayType, string Product, Guid ShopGid, Guid MemberGid, string Remarks, string Address = "", string RealName = "", string ContactNumber = "")
        {
            using (EFDB db = new EFDB())
            {
                //订单的Gid
                Guid OrderGid = Guid.NewGuid();
                //订单总金额
                decimal TotalPrice = 0;
                //产品名称
                string body = "";
                //添加订单产品列表
                JArray json = (JArray)JsonConvert.DeserializeObject(Product);
                //产品数量
                int Number = 0;
                foreach (var j in json)
                {
                    Guid ProductGid = Guid.Parse(j["gid"].ToString());
                    Number = int.Parse(j["pCount"].ToString());
                    var p = db.ShopProduct.Where(l => l.Gid == ProductGid).FirstOrDefault();
                    if (p.Stock >= Number)
                    {
                        body += p.Name + ",";
                        //订单详情
                        var od = new OrderDetails();
                        od.Gid = Guid.NewGuid();
                        od.AddTime = DateTime.Now;
                        od.OrderGid = OrderGid;
                        od.ProductGid = (Guid)ProductGid;
                        od.Number = Number;
                        od.Price = p.Price;
                        db.OrderDetails.Add(od);
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new { body = p.Name, TotalPrice = 1000000, OrderNo = "", OrderGid });
                    }
                }
                if (db.SaveChanges() == json.Count())
                {
                    body = body.TrimEnd(',');
                    if (body.Length > 128)
                    {
                        body = body.Substring(0, 120) + "...";
                    }
                    //订单总价
                    TotalPrice = db.OrderDetails.Where(l => l.OrderGid == OrderGid).Sum(l => l.Price * l.Number);
                    //生成订单
                    var b = new ShopOrder();
                    b.Gid = OrderGid;
                    b.AddTime = DateTime.Now;
                    b.OrderNo = RandStr.CreateOrderNO();
                    b.MemberGid = MemberGid;
                    b.ShopGid = ShopGid;
                    b.PayStatus = 2;
                    b.PayType = PayType;
                    b.TotalPrice = TotalPrice;
                    b.Price = TotalPrice;
                    b.CouponPrice = 0;
                    b.ExpressStatus = 1;
                    b.Address = Address;
                    b.RealName = RealName;
                    b.ContactNumber = ContactNumber;
                    b.Remarks = Remarks;
                    b.Product = body;
                    b.PayPrice = 0;
                    b.Profit = 0;
                    b.ConsumptionCode = RandStr.CreateValidateNumber(8);
                    b.Status = 1;
                    db.ShopOrder.Add(b);
                    if (db.SaveChanges() == 1)
                    {
                        //body =商品名称
                        //TotalPrice =支付金额
                        //OrderNo =订单号
                        //OrderGid =订单Gid
                        return JsonConvert.SerializeObject(new { body, TotalPrice, b.OrderNo, OrderGid });
                    }
                    else
                    {
                        db.OrderDetails.Where(l => l.OrderGid == OrderGid).Delete();
                        db.ShopOrder.Where(l => l.Gid == OrderGid).Delete();
                    }
                }
                else
                {
                    db.OrderDetails.Where(l => l.OrderGid == OrderGid).Delete();
                }
                return null;
            }
        }

        /// <summary>
        /// 商城支付成功更新订单
        /// </summary>
        /// <param name="OrderNo">网站订单号</param>
        /// <param name="TradeNo">网银订单号</param>
        /// <param name="PayType">支付类型</param>
        /// <param name="PayPrice">在线支付金额</param>
        /// <returns>返回调用结果</returns>
        public static bool ShopPayOrder(string OrderNo, string TradeNo, int PayType, decimal PayPrice)
        {
            bool Pay = false;
            string LogMsg = "订单号:" + OrderNo + ",网银订单号:" + TradeNo + ",支付类型:" + PayType.ToString() + ",网上支付金额:" + PayPrice.ToString();
            string rn = "\r\n-----------------------------------------------------\r\n";
            string msg = "订单号=" + OrderNo + rn;
            try
            {
                using (EFDB db = new EFDB())
                {
                    //支付类型
                    string payname = ((PayType)Enum.Parse(typeof(LJShengHelper.PayType), PayType.ToString())).ToString();
                    var b = db.ShopOrder.Where(l => l.OrderNo == OrderNo).FirstOrDefault();
                    if (b != null && b.PayStatus == 2)
                    {
                        msg += "购买会员=" + b.MemberGid.ToString() + ",商家=" + b.ShopGid.ToString() + rn;
                        PayPrice = b.Price;//测试要删
                        b.PayStatus = b.Price == PayPrice ? 1 : 5;
                        b.TradeNo = TradeNo;
                        b.PayTime = DateTime.Now;
                        b.PayType = PayType;
                        b.PayPrice = PayPrice;
                        b.TotalPrice = PayPrice;
                        b.ExpressStatus = 1;
                        b.Profit = PayPrice;
                        if (db.SaveChanges() == 1)
                        {
                            Pay = true;
                            //扣除库存
                            OrderStock(b.Gid);
                            if (b.PayStatus == 1)
                            {
                                //获取商家的会员GID
                                //Guid MGid = db.Shop.Where(s => s.Gid == b.ShopGid).FirstOrDefault().MemberGid;
                                //增加货款-8月变更确认收货后增加货款
                                //if (db.Member.Where(l => l.Gid == MGid).Update(l => new Member { ShopMoney = l.ShopMoney + PayPrice }) == 1)
                                //{
                                //    msg += "货款成功=" + b.Profit.ToString() + rn;
                                //}
                                //else
                                //{
                                //    msg += "货款失败:=" + b.Profit.ToString() + rn;
                                //}
                                //基数积分增加
                                if (ShopRecordAdd(b.Gid, b.MemberGid, PayPrice, 0, 3, 1) == null)
                                {
                                    msg += "基数积分失败:PayPrice=" + PayPrice.ToString() + rn;
                                }
                                else
                                {
                                    msg += "基数积分成功=" + PayPrice.ToString() + rn;
                                }
                                if (PayType != 5)
                                {
                                    //团队获取比例
                                    List<DictionariesList> dl = db.DictionariesList.Where(l => l.DGid == db.Dictionaries.Where(d => d.DictionaryType == "CL").FirstOrDefault().Gid).ToList();
                                    //累计团队积分
                                    msg += "商城团队业绩=" + ShopTeamIntegral(b.MemberGid, PayPrice, b.Gid, dl) + rn;
                                }
                                LogManager.WriteLog(payname + "商城对账记录", LogMsg + rn + msg);
                            }
                            else
                            {
                                LogManager.WriteLog(payname + "商城金额不对", LogMsg);
                            }
                        }
                        else
                        {
                            LogManager.WriteLog(payname + "商城支付成功更新订单失败", LogMsg);
                        }
                    }
                    else
                    {
                        Pay = true;
                        LogManager.WriteLog(payname + "商城订单已对账", LogMsg);
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("商城对账异常", LogMsg + rn + msg + rn + err.Message);
            }
            return Pay;
        }

        /// <summary>
        /// 彩链团队业绩
        /// </summary>
        /// <param name="MemberGid">购买会员Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string ShopTeamIntegral(Guid MemberGid, decimal Price, Guid OrderGid, List<DictionariesList> dl)
        {
            string msg = "";
            try
            {
                using (EFDB db = new EFDB())
                {
                    var mr = db.MRelation.Where(l => l.MemberGid == MemberGid).Select(l => new
                    {
                        l.M1,
                        l.M2,
                        l.M3,
                        LM = db.Member.Where(m => m.Gid == l.MemberGid).FirstOrDefault().CLLevel
                    }).FirstOrDefault();
                    if (mr != null)
                    {
                        if (dl != null && mr.M1 != null)
                        {
                            decimal TIntegral = Price * decimal.Parse(dl.Where(l => l.Key == "T1").FirstOrDefault().Value);
                            if (ShopRecordAdd(OrderGid, (Guid)mr.M1, 0, TIntegral, 4, 1) == null)
                            {
                                msg += "第1级失败:会员=" + mr.M1.ToString() + ",Price=" + Price.ToString();
                            }
                            if (mr.LM > 20)
                            {
                                if (mr.M2 != null)
                                {
                                    TIntegral = Price * decimal.Parse(dl.Where(l => l.Key == "T2").FirstOrDefault().Value);
                                    if (ShopRecordAdd(OrderGid, (Guid)mr.M2, 0, TIntegral, 5, 1) == null)
                                    {
                                        msg += "第2级失败:会员=" + mr.M2.ToString() + ",Price=" + Price.ToString();
                                    }
                                    if (mr.M3 != null)
                                    {
                                        TIntegral = Price * decimal.Parse(dl.Where(l => l.Key == "T3").FirstOrDefault().Value);
                                        if (ShopRecordAdd(OrderGid, (Guid)mr.M3, 0, TIntegral, 6, 1) == null)
                                        {
                                            msg += "第3级失败:会员=" + mr.M3.ToString() + ",Price=" + Price.ToString();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return msg;
        }

        /// <summary>
        /// 扣除商城库存
        /// </summary>
        /// <param name="OrderGid">订单Gig</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public static void OrderStock(Guid OrderGid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.OrderDetails.Where(l => l.OrderGid == OrderGid).ToList();
                foreach (var od in b)
                {
                    db.ShopProduct.Where(l => l.Gid == od.ProductGid).Update(l => new ShopProduct { Stock = l.Stock - od.Number });
                }
            }
        }
        #endregion

        #region 多级模块
        //查询关联总人数
        public static int AllMember(List<Member> Member, Guid gid, int loop)
        {
            loop--;
            int num = 0;
            var fs = Member.Where(l => l.MemberGid == gid).ToList();
            num = fs.Count();
            if (loop != 0)
            {
                foreach (var dr in fs)
                {
                    num += Helper.AllMember(Member, dr.Gid, loop);
                }
            }
            return num;
        }

        /// <summary>
        /// 查询推荐人
        /// </summary>
        /// <param name="orderGid">订单Gid</param>
        /// <param name="payPrice">在线支付金额</param>
        /// <param name="gid">用户Gid</param>
        /// <param name="level">支付用户的等级</param>
        /// <param name="number">第几级</param>
        /// <param name="loop">找几级</param>
        /// <returns>请求结果</returns>
        public static void M(Guid orderGid, decimal payPrice, Guid gid, int level, int number, int loop)
        {
            if (number <= loop)
            {
                using (EFDB db = new EFDB())
                {
                    var b = db.Member.Where(l => l.Gid == gid).FirstOrDefault();
                    if (b != null)
                    {
                        number++;
                        //用户等级
                        int thisLevel = b.Level;
                        //比例
                        decimal RMB = 0;
                        if (thisLevel == 7 || thisLevel == 8 || thisLevel == 9)
                        {
                            //查询用户的平级和级差比例
                            var LV = db.LV.Where(l => l.LVID == thisLevel && l.Number == number).FirstOrDefault();
                            if (LV != null)
                            {
                                //等级对比
                                RMB = thisLevel > level ? LV.Differential : LV.SameLevel;
                                //查询用户奖励购物比例
                                var lev = db.Level.Where(l => l.LV == thisLevel).FirstOrDefault();
                                decimal Money = payPrice * RMB;
                                decimal Integral = Money * db.Level.Where(l => l.LV == thisLevel).FirstOrDefault().ShopProfit;
                                MoneyRecordAdd(orderGid, b.Gid, Money - Integral, Integral, 6, "第" + number.ToString() + "级");
                            }
                        }
                        if (b.MemberGid != null)
                        {
                            M(orderGid, payPrice, (Guid)b.MemberGid, level, number, loop);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 增加推荐人人数
        /// </summary>
        /// <param name="Gid">注册用户</param>
        /// <param name="MemberGid">上级Gid</param>
        /// <param name="number">第几级</param>
        /// <param name="loop">找几级</param>
        /// <param name="list">会员关系列表</param>
        /// <returns>请求结果</returns>
        public static void Member(Guid Gid, Guid? MemberGid, int number, int loop, List<Guid> list)
        {
            using (EFDB db = new EFDB())
            {
                if (MemberGid != null)
                {
                    list.Add((Guid)MemberGid);
                    if (number <= loop)
                    {
                        number++;
                        var b = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                        if (b != null && b.MemberGid != null)
                        {
                            Member(Gid, b.MemberGid, number, loop, list);
                        }
                    }
                }
                //条件查找结束,增加会员关系
                MRelation(Gid, list);
            }
        }

        /// <summary>
        /// 会员关系
        /// </summary>
        /// <param name="Gid">注册用户</param>
        /// <param name="list">会员关系列表</param>
        public static void MRelation(Guid Gid, List<Guid> list)
        {
            using (EFDB db = new EFDB())
            {
                Boolean Add = false;
                MRelation MR = db.MRelation.Where(l => l.MemberGid == Gid).FirstOrDefault();
                if (MR == null)
                {
                    Add = true;
                    MR = new MRelation();
                    MR.Gid = Guid.NewGuid();
                    MR.AddTime = DateTime.Now;
                    MR.MemberGid = Gid;
                }
                else
                {
                    LogManager.WriteLog("会员关系已存在", "Gid=" + Gid.ToString() + " \r\n count=" + list.Count.ToString() + " \r\n list=" + string.Join(",", list.ToArray()));
                }
                int count = list.Count;
                if (count > 0)
                {
                    MR.M1 = list[0];
                    MR.M1Time = DateTime.Now;
                }
                if (count > 1)
                {
                    MR.M2 = list[1];
                    MR.M2Time = DateTime.Now;
                }
                if (count > 2)
                {
                    MR.M3 = list[2];
                    MR.M3Time = DateTime.Now;
                }
                if (Add)
                {
                    db.MRelation.Add(MR);
                }
                if (db.SaveChanges() != 1)
                {
                    LogManager.WriteLog("会员关系失败", "Gid=" + Gid.ToString() + " \r\n count=" + count.ToString() + " \r\n list=" + string.Join(",", list.ToArray()));
                }
            }
        }
        #endregion

        #region 用户数据模块
        /// <summary>
        /// 用户资金记录
        /// </summary>
        /// <param name="OrderGid">订单Gid</param>
        /// <param name="Gid">会员Gid</param>
        /// <param name="Money">积分</param>
        /// <param name="Integral">购物积分</param>
        /// <param name="Type">类型[查看表MoneyRecord说明]</param>
        /// <param name="Remarks">备注</param>
        /// <returns>返回调用结果</returns>
        public static Guid? MoneyRecordAdd(Guid? OrderGid, Guid Gid, decimal Money, decimal Integral, int Type, string Remarks = "")
        {
            Guid? MRGid = null;
            string LogMsg = "订单=" + OrderGid + ",会员=" + Gid + ",Money=" + Money + ",Integral=" + Integral + ",Type=" + Type.ToString();
            try
            {
                using (EFDB db = new EFDB())
                {
                    if (OrderGid == null || db.MoneyRecord.Where(l => l.OrderGid == OrderGid && l.MemberGid == Gid && l.Type == Type && l.Remarks == Remarks).Count() == 0)
                    {
                        //查询用户之前的数据
                        var m = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                        if (m.Jurisdiction != "冻结")
                        {
                            var b = new MoneyRecord();
                            b.Gid = Guid.NewGuid();
                            b.AddTime = DateTime.Now;
                            b.OrderGid = OrderGid;
                            b.MemberGid = Gid;
                            b.Type = Type;
                            b.Money = Money;
                            b.Integral = Integral;
                            b.OldMoney = m.Money;
                            b.OldIntegral = m.Integral;
                            b.Remarks = Remarks;
                            db.MoneyRecord.Add(b);
                            if (db.SaveChanges() == 1)
                            {
                                //更新用户数据
                                m.Money = m.Money + Money;
                                m.Integral = m.Integral + Integral;
                                if (db.SaveChanges() == 1)
                                {
                                    MRGid = b.Gid;
                                }
                                else
                                {
                                    db.MoneyRecord.Where(l => l.Gid == b.Gid).Delete();
                                    LogManager.WriteLog("更新用户资金错误", LogMsg);
                                }
                            }
                            else
                            {
                                LogManager.WriteLog("资金记录失败", LogMsg);
                            }
                        }
                        else
                        {
                            LogManager.WriteLog("用户被冻结", LogMsg);
                        }
                    }
                    else
                    {
                        LogManager.WriteLog("资金记录已存在", LogMsg);
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("资金记录异常", LogMsg + "/r/n" + err.Message);
            }
            return MRGid;
        }
        #endregion

        #region 文件上传
        /// <summary> 
        /// 多文件上传的操作
        /// </summary> 
        /// <param name="path">上传路径</param>
        /// <param name="files">文件集合</param> 
        public static string UploadFiles(string path, HttpFileCollection files)
        {
            StringBuilder sb = new StringBuilder();
            if (!Directory.Exists(HttpContext.Current.Server.MapPath(path)))  //判断当前目录是否存在。
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));  //建立上传文件存放目录。
            }
            foreach (string f in files.AllKeys)
            {
                HttpPostedFile file = files[f];
                string fl = RandStr.CreateOrderNO() + file.FileName;
                file.SaveAs(HttpContext.Current.Server.MapPath(path + fl));
                sb.Append(fl + "$");
            }
            return sb.ToString().TrimEnd('$');
        }

        /// <summary>
        /// 压缩上传
        /// </summary>
        /// <param name="base64str"></param>
        /// <returns></returns>
        public static string jsimg(string path, string base64str)
        {
            string FileName = "";
            try
            {
                if (!string.IsNullOrEmpty(base64str))
                {
                    string imgData = base64str.Split(',')[1];
                    //过滤特殊字符即可   
                    string dummyData = imgData.Trim().Replace("%", "").Replace(",", "").Replace(" ", "+");
                    if (dummyData.Length % 4 > 0)
                    {
                        dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');
                    }
                    byte[] byteArray = Convert.FromBase64String(dummyData);
                    using (MemoryStream ms = new MemoryStream(byteArray))
                    {
                        Image img = Image.FromStream(ms);
                        path = HttpContext.Current.Server.MapPath(path);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        FileName = Guid.NewGuid() + ".jpg";
                        img.Save(path + FileName);
                    }
                }
            }
            catch { }
            return FileName;
        }

        #endregion

        #region POST文件上传
        /// <summary> 
        /// POST有多文件上传的操作
        /// </summary> 
        /// <param name="path">上传路径</param>
        /// <param name="file">文件</param> 
        public static string UploadFiles(string path, HttpPostedFileBase file)
        {
            StringBuilder sb = new StringBuilder();
            if (!Directory.Exists(HttpContext.Current.Server.MapPath(path)))  //判断当前目录是否存在。
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));  //建立上传文件存放目录。
            }
            string filename = RandStr.CreateOrderNO() + Path.GetExtension(file.FileName);
            file.SaveAs(HttpContext.Current.Server.MapPath(path + filename));
            return filename;
        }
        #endregion

        #region POST
        /// <summary>  
        ///   POST请求得到返回数据
        /// </summary>  
        /// <param name="url">调用的Api地址</param>  
        /// <param name="requestJson">表单数据（json格式）</param>  
        /// <returns></returns> 
        public static string Post(string url, string requestJson)
        {
            HttpContent httpContent = new StringContent(requestJson);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                return "出错了,StatusCode:" + response.StatusCode.ToString();
            }
        }
        #endregion

        #region 请求时候的IP
        /// <summary>
        /// 请求时候的IP
        /// </summary>
        public static string IP
        {
            get
            {
                string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (String.IsNullOrEmpty(ip))
                {
                    ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
                return ip;
            }
        }
        #endregion

        #region 操作提示
        public static RedirectResult WebRedirect(string title, string url, string msg)
        {
            return new RedirectResult("/LJSheng/Page?msg=" + msg + "&url=" + HttpContext.Current.Server.UrlEncode(url) + "&title=" + title);
        }
        public static RedirectResult Redirect(string title, string url, string msg)
        {
            return new RedirectResult("/Home/PageMsg?msg=" + msg + "&url=" + HttpContext.Current.Server.UrlEncode(url) + "&title=" + title);
        }
        #endregion

        #region 更新登录信息
        /// <summary>
        /// 用户登录信息CK
        /// </summary>
        /// <param name="Gid">用户Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public static void MLogin(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                LCookie.DelALLCookie();
                //会员登录信息
                var b = db.Member.Where(l => l.Gid == Gid).Select(l => new
                {
                    l.Gid,
                    l.Account,
                    l.LoginIdentifier,
                    l.MemberGid,
                    l.Picture,
                    l.RealName,
                    l.NickName,
                    l.Gender,
                    l.Level,
                    l.City
                }).FirstOrDefault();
                LCookie.AddCookie("linjiansheng", DESRSA.DESEnljsheng(JsonConvert.SerializeObject(new
                {
                    b.Gid,
                    b.Account,
                    b.LoginIdentifier,
                    b.MemberGid
                })), 30);
                LCookie.AddCookie("member", DESRSA.DESEnljsheng(JsonConvert.SerializeObject(new
                {
                    b.Picture,
                    b.RealName,
                    b.NickName,
                    b.Gender,
                    b.Level
                })), 30);
                //设置用户读取数据的城市
                LCookie.AddCookie("city", b.City, 30);
                LCookie.AddCookie("ordercity", b.City, 30);
            }
        }

        /// <summary>
        /// 商家登录信息CK
        /// </summary>
        /// <param name="Gid">用户Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <Remarks>
        /// 2018-08-18 林建生
        /// </Remarks>
        public static void SLogin(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                LCookie.DelCookie("shop");
                //会员登录信息
                var b = db.Shop.Where(l => l.Gid == Gid).Select(l => new
                {
                    l.Gid,
                    l.Name,
                    l.USCI,
                    l.LegalPerson
                }).FirstOrDefault();
                LCookie.AddCookie("shop", DESRSA.DESEnljsheng(JsonConvert.SerializeObject(new
                {
                    b.Gid,
                    b.USCI,
                    b.LegalPerson,
                    b.Name
                })), 1);
            }
        }
        #endregion

        #region 短信模块
        /// <summary>
        /// 插入短信
        /// </summary>
        /// <param name="PhoneNumber">发送号码</param>
        /// <param name="Content">短信内容</param>
        /// <param name="Number">计费条数</param>
        /// <param name="Type">发送类型</param>
        /// <param name="ReceiptContent">短信返回状态</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2016-06-30 林建生
        /// </remarks>
        public static void SMSAdd(string PhoneNumber, string Content, int Number, int Type, string ReceiptContent)
        {
            var b = new Data.SMS()
            {
                Gid = Guid.NewGuid(),
                AddTime = DateTime.Now,
                PhoneNumber = PhoneNumber,
                Content = Content,
                Number = Number,
                Type = Type,
                ReceiptContent = ReceiptContent
            };
            using (EFDB db = new EFDB())
            {
                db.SMS.Add(b);
                if (db.SaveChanges() != 1)
                {
                    LogManager.WriteLog("短信插入记录失败日志", "发送号码:" + PhoneNumber + ",发送内容:" + Content + ",发送返回:" + ReceiptContent);
                }
            }
        }
        #endregion

        #region 产品,模块
        /// <summary>
        /// 获取订单里的产品
        /// </summary>
        /// <param name="ProductGid">产品Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string GetProduct(Guid ProductGid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Product.Where(l => l.Gid == ProductGid).FirstOrDefault();
                return "<img src=\"/uploadfiles/product/" + b.Picture + "\" /><h1>" + b.Name + "</h1>";
            }
        }

        /// <summary>
        /// 获取订单里的产品
        /// </summary>
        /// <param name="ProductGid">产品Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string GetShopProduct(Guid ProductGid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.ShopProduct.Where(l => l.Gid == ProductGid).FirstOrDefault();
                return "<img src=\"/uploadfiles/product/" + b.Picture + "\" /><h1>" + b.Name + "</h1>";
            }
        }
        #endregion

        #region 查看合伙人库存
        /// <summary>
        /// 查看合伙人库存
        /// </summary>
        /// <param name="ProductGid">产品Gid</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">对象结果</para>
        /// <remarks>
        /// 2018-08-18 林建生
        /// </remarks>
        public static string GetStock(Guid ProductGid)
        {
            Guid MemberGid = LCookie.GetMemberGid();
            using (EFDB db = new EFDB())
            {
                string Stock = "库存不足";
                var b = db.Stock.Where(l => l.ProductGid == ProductGid && l.MemberGid == MemberGid).FirstOrDefault();
                if (b != null)
                {
                    Stock = "库存 " + b.Number.ToString();
                }
                return Stock;
            }
        }
        #endregion

        #region 提现记录
        /// <summary>
        /// 提现记录
        /// </summary>
        /// <param name="Gid">会员/商家Gid</param>
        /// <param name="Money">积分</param>
        /// <param name="Type">类型[1=彩链 2=商城]</param>
        /// <param name="Remarks">备注</param>
        /// <returns>返回调用结果</returns>
        public static bool RMBRecordAdd(Guid Gid, decimal Money, int Type, string Remarks = "")
        {
            string LogMsg = "用户=" + Gid + ",Money=" + Money + ",Type=" + Type;
            try
            {
                using (EFDB db = new EFDB())
                {
                    //查询用户之前的数据
                    var m = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                    var b = new RMBRecord();
                    b.Gid = Guid.NewGuid();
                    b.AddTime = DateTime.Now;
                    b.MemberGid = Gid;
                    b.Type = Type;
                    b.Money = Money;
                    b.OldMoney = Type == 1 ? m.ProductMoney : m.ShopMoney;
                    b.Remarks = Remarks;
                    db.RMBRecord.Add(b);
                    if (db.SaveChanges() == 1)
                    {
                        //更新用户数据
                        if (Type == 1)
                        {
                            m.ProductMoney = m.ProductMoney - Money;
                        }
                        else
                        {
                            m.ShopMoney = m.ShopMoney - Money;
                        }
                        if (db.SaveChanges() == 1)
                        {
                            return true;
                        }
                        else
                        {
                            db.RMBRecord.Where(l => l.Gid == b.Gid).Delete();
                            LogManager.WriteLog("提现扣除失败", LogMsg);
                        }
                    }
                    else
                    {
                        LogManager.WriteLog("提现记录失败", LogMsg);
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("提现记录异常", LogMsg + "/r/n" + err.Message);
            }
            return false;
        }
        #endregion

        #region 商城积分模块
        /// <summary>
        /// 商城积分记录
        /// </summary>
        /// <param name="OrderGid">订单Gid</param>
        /// <param name="Gid">会员Gid</param>
        /// <param name="Integral">积分</param>
        /// <param name="Type">类型[1=会员支取基数 2=团队累计满足冻结 3=购买获取基数 4=第1级奖励 5=第2级奖励 6=第3级奖励]</param>
        /// <param name="State">状态[1=成功 2=冻结 3=取消]</param>
        /// <param name="Multiple">基数分倍数</param>
        /// <param name="Remarks">备注</param>
        /// <returns>返回调用结果</returns>
        public static Guid? ShopRecordAdd(Guid? OrderGid, Guid Gid, decimal MIntegral, decimal TIntegral, int Type, int State, int Multiple = 0, string Remarks = "")
        {
            Guid? MRGid = null;
            string LogMsg = "订单=" + OrderGid + ",会员=" + Gid + ",MIntegral=" + MIntegral + ",TIntegral=" + TIntegral + ",Type=" + Type + ",State=" + State + ",Multiple=" + Multiple;
            try
            {
                using (EFDB db = new EFDB())
                {
                    if (OrderGid == null || db.ShopRecord.Where(l => l.OrderGid == OrderGid && l.MemberGid == Gid && l.Remarks == Remarks).Count() == 0)
                    {
                        //查询用户之前的数据
                        var m = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                        var b = new ShopRecord();
                        b.Gid = Guid.NewGuid();
                        b.AddTime = DateTime.Now;
                        b.OrderGid = OrderGid;
                        b.MemberGid = Gid;
                        b.MIntegral = MIntegral;
                        b.TIntegral = TIntegral;
                        b.OldMIntegral = m.MIntegral;
                        b.OldTIntegral = m.TIntegral;
                        b.Type = Type;
                        b.State = State;
                        b.Multiple = Multiple;
                        if (Type == 2)
                        {
                            b.ThawTime = b.AddTime.AddDays(30);
                        }
                        b.Remarks = Remarks;
                        db.ShopRecord.Add(b);
                        if (db.SaveChanges() == 1)
                        {
                            //更新用户数据
                            m.MIntegral = m.MIntegral + MIntegral;
                            string FRemarks = "";
                            if (Type == 1)
                            {
                                FRemarks = "提取基数分满足冻结";
                            }
                            else if (Type == 2)
                            {
                                //满足解冻的时候基数分提取到商城可转积分
                                m.TIntegral = 0;
                                m.ShopIntegral = m.ShopIntegral - MIntegral;
                            }
                            else if (Type == 3)
                            {
                                FRemarks = "增加基数分满足冻结";
                            }
                            else
                            {
                                m.TIntegral = m.TIntegral + TIntegral;
                            }
                            if (db.SaveChanges() == 1)
                            {
                                MRGid = b.Gid;
                                //是否满足冻结要求
                                if (Type !=2)
                                {
                                    FrozenIntegral(Gid, m.MIntegral, m.TIntegral, 2, 2, FRemarks);
                                }
                            }
                            else
                            {
                                db.ShopRecord.Where(l => l.Gid == b.Gid).Delete();
                                LogManager.WriteLog("商城积分操作用户积分记录失败", LogMsg);
                            }
                        }
                        else
                        {
                            LogManager.WriteLog("商城积分记录失败", LogMsg);
                        }
                    }
                    else
                    {
                        LogManager.WriteLog("商城积分记录已存在", LogMsg);
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("商城资金记录异常", LogMsg + "/r/n" + err.Message);
            }
            return MRGid;
        }

        /// <summary>
        /// 冻结积分
        /// </summary>
        /// <param name="Gid">会员Gid</param>
        /// <param name="MIntegral">个人基数积分</param>
        /// <param name="TIntegral">团队积分</param>
        /// <param name="Type">类型</param>
        /// <param name="State">冻结状态</param>
        /// <param name="Remarks">备注</param>
        /// <returns>返回调用结果</returns>
        public static void FrozenIntegral(Guid Gid, decimal MIntegral, decimal TIntegral, int Type, int State, string Remarks = "")
        {
            string LogMsg = "会员=" + Gid + ",MIntegral=" + MIntegral.ToString() + ",TIntegral=" + TIntegral.ToString() + ",Type=" + Type.ToString() + ",State=" + State.ToString();
            try
            {
                using (EFDB db = new EFDB())
                {
                    //查询用户之前的数据
                    var m = db.Member.Where(l => l.Gid == Gid).GroupJoin(db.Level,
                    l => l.CLLevel,
                    j => j.LV,
                    (l, j) => new
                    {
                        l.MIntegral,
                        l.TIntegral,
                        j.FirstOrDefault().Multiple
                    }).FirstOrDefault();
                    //需要团队积分对应的倍数
                    decimal Integral = MIntegral * m.Multiple;
                    if (Integral > 0 && TIntegral >= Integral)
                    {
                        if (ShopRecordAdd(null, Gid, -MIntegral, Integral, 2, 2, m.Multiple, "清空分=" + (m.TIntegral - Integral).ToString()) == null)
                        {
                            LogManager.WriteLog("商城积分冻结失败", LogMsg);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("冻结记录异常", LogMsg + "/r/n" + err.Message);
            }
        }
        #endregion

        #region 积分兑换记录
        /// <summary>
        /// 积分兑换记录
        /// </summary>
        /// <param name="Gid">数据的Gid</param>
        /// <param name="MemberGid">会员/商家Gid</param>
        /// <param name="Money">积分</param>
        /// <param name="Token">兑换多少Token</param>
        /// <param name="Type">类型[1=彩链积分 2=商城基数积分 3=商城积分]</param>
        /// <param name="TB">兑换币种[1=BCCB, 2=FBCC]</param>
        /// <param name="Remarks">备注</param>
        /// <returns>返回调用结果</returns>
        public static bool TokenRecordAdd(Guid Gid, Guid MemberGid, decimal Integral, decimal Token, int Type, int TB, string Remarks = "")
        {
            string LogMsg = "Gid=" + Gid + ",用户=" + MemberGid + ",Integral=" + Integral + ",Token=" + Token + ",Type=" + Type + ",TB=" + TB;
            try
            {
                using (EFDB db = new EFDB())
                {
                    //查询用户之前的数据
                    var m = db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault();
                    var b = new TokenRecord();
                    b.Gid = Gid;
                    b.AddTime = DateTime.Now;
                    b.MemberGid = MemberGid;
                    b.Type = Type;
                    b.TB = TB;
                    b.Integral = Integral;
                    b.Token = Token;
                    if (Type == 1)
                    {
                        b.OldIntegral = m.Money;
                    }
                    else if (Type == 2)
                    {
                        b.OldIntegral = m.MIntegral;
                        //string TR = "，剩团队分=" + m.TIntegral;
                        //if (Integral == m.MIntegral)
                        //{
                        //    TR="，清空团队分=" + m.TIntegral;
                        //}
                        //b.Remarks = "提取基数分="+ Integral+ TR;
                    }
                    else if (Type == 3)
                    {
                        b.OldIntegral = m.ShopIntegral;
                    }
                    b.Remarks = Remarks;
                    db.TokenRecord.Add(b);
                    if (db.SaveChanges() == 1)
                    {
                        //更新用户数据
                        if (Type == 1)
                        {
                            m.Money = m.Money - Integral;
                        }
                        else if (Type == 2)
                        {
                            m.MIntegral = m.MIntegral - Integral;
                            //if (Integral == m.MIntegral)
                            //{
                            //    m.TIntegral = 0;
                            //}
                        }
                        else if (Type == 3)
                        {
                            m.ShopIntegral = m.ShopIntegral - Integral;
                        }
                        if (db.SaveChanges() == 1)
                        {
                            return true;
                        }
                        else
                        {
                            db.TokenRecord.Where(l => l.Gid == Gid).Delete();
                            LogManager.WriteLog("积分兑换记录扣除失败", LogMsg);
                        }
                    }
                    else
                    {
                        LogManager.WriteLog("积分兑换记录失败", LogMsg);
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("积分兑换记录异常", LogMsg + "/r/n" + err.Message);
            }
            return false;
        }
        #endregion

        #region 彩链模块
        /// <summary>
        /// 查询用户出售彩链库存数量
        /// </summary>
        public static int GetCLStock(Guid MemberGid)
        {
            using (EFDB db = new EFDB())
            {
                //出售库存对应的商品GID
                Guid ProductGid = Helper.GetProductGid();
                var p = db.Stock.Where(l => l.MemberGid == MemberGid && l.ProductGid == ProductGid).FirstOrDefault();
                return p != null ? p.Number : 0;
            }
        }

        /// <summary>
        /// 扣除出售彩链库存
        /// </summary>
        public static Boolean UPCLStock(Guid MemberGid, int Number)
        {
            using (EFDB db = new EFDB())
            {
                //出售库存对应的商品GID
                Guid ProductGid = Helper.GetProductGid();
                int Stock = db.Stock.Where(l => l.MemberGid == MemberGid && l.ProductGid == ProductGid).Update(l => new Stock { Number = l.Number - Number });
                if (Stock == 1)
                {
                    return true;
                }
                else
                {
                    LogManager.WriteLog("出售库存失败", "会员=" + MemberGid.ToString() + ",数量=" + Number.ToString() + ",执行行数=" + Stock.ToString());
                    return false;
                }
            }
        }

        /// <summary>
        /// 设置发货人
        /// </summary>
        /// <param name="MGid">会员Gid</param>
        /// <param name="MemberGid">会员的推荐人Gid</param>
        public static void SetConsignor(Guid MGid, Guid? MemberGid)
        {
            using (EFDB db = new EFDB())
            {
                //增加彩链发货人
                Consignor c = new Consignor();
                c.Gid = Guid.NewGuid();
                c.AddTime = DateTime.Now;
                c.MemberGid = MGid;
                c.MGid = GetConsignor();//目前发货人
                c.ShopGid = GetConsignor();//默认发货人
                if (MemberGid != null)
                {
                    if (db.Member.Where(l => l.Gid == MemberGid).FirstOrDefault().CLLevel > 24)
                    {
                        c.MGid = (Guid)MemberGid;
                        c.ShopGid = (Guid)MemberGid;
                    }
                    else
                    {
                        var cc = db.Consignor.Where(l => l.MemberGid == MemberGid).FirstOrDefault();
                        if (cc.MGid != null)
                        {
                            c.MGid = cc.MGid;
                            c.ShopGid = cc.MGid;
                        }
                    }
                }
                c.MTime = DateTime.Now;
                db.Consignor.Add(c);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量发货人
        /// </summary>
        public static string Consignor(Guid MemberGid)
        {
            try
            {
                using (EFDB db = new EFDB())
                {
                    return UPConsignor(db.Member.ToList(), MemberGid, MemberGid);
                }
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }
        /// <summary>
        /// 更新发货人
        /// </summary>
        /// <param name="Member">会员列表</param>
        /// <param name="MemberGid">发货人GID</param>
        /// <param name="Gid">要更新会员的推荐人</param>
        /// <returns>请求结果</returns>
        public static string UPConsignor(List<Member> Member, Guid MemberGid, Guid Gid)
        {
            string msg = "";
            using (EFDB db = new EFDB())
            {
                //查找推荐人是发货人的下级
                var m = Member.Where(l => l.MemberGid == Gid).ToList();
                foreach (var dr in m)
                {
                    if (dr.CLLevel < 25)
                    {
                        if (db.Consignor.Where(l => l.MemberGid == dr.Gid).Update(l => new Consignor { MGid = MemberGid, MTime = DateTime.Now }) == 1)
                        {
                            msg += "更新发货人会员Gid=" + dr.Gid.ToString() + ",发货人=" + MemberGid.ToString();
                        }
                        else
                        {
                            msg += "更新发货人失败:会员Gid=" + dr.Gid.ToString() + ",发货人=" + MemberGid.ToString();
                            LogManager.WriteLog("更新发货人失败", "Gid=" + dr.Gid.ToString() + ",发货人=" + MemberGid.ToString());
                        }
                        UPConsignor(Member, MemberGid, dr.Gid);
                    }
                }
                return msg;
            }
        }

        /// <summary>
        /// 冻结寻找发货人模式
        /// </summary>
        public static Guid GetConsignor(Guid Gid)
        {
            using (EFDB db = new EFDB())
            {
                //发货人的的会员Gid
                var m = db.Consignor.Where(l => l.MemberGid == Gid).GroupJoin(db.Member,
                            x => x.MGid,
                            y => y.Gid,
                            (x, y) => new
                            {
                                x.MemberGid,
                                x.MGid,
                                y.FirstOrDefault().Jurisdiction
                            }).FirstOrDefault();
                if (m == null)
                {
                    return GetConsignor();
                }
                else
                {
                    //如果会员被封,为默认发货人
                    if (m.Jurisdiction == "冻结")
                    {
                        return GetConsignor(m.MGid);
                    }
                    else
                    {
                        return m.MGid;
                    }
                }
            }
        }
        /// <summary>
        /// 默认发货人
        /// </summary>
        public static Guid GetConsignor()
        {
            return Guid.Parse("3A44A42D-867D-4AB9-9B2F-D0A020ECD089");
        }
        /// <summary>
        /// 默认出售库存商品
        /// </summary>
        public static Guid GetProductGid()
        {
            return Guid.Parse("06BF4E6F-E27D-47B0-B621-B70FCE31AEFA");
        }

        /// <summary>
        /// 库存不足短信
        /// </summary>
        /// <param name="MAccount">发货人手机</param>
        /// <param name="MemberAccount">购买会员手机</param>
        /// <returns>返回调用结果</returns>
        public static string CLSMS(string MAccount, string MemberAccount, Guid ShopGid, Guid MemberGid, int Number, decimal Money, decimal Integral)
        {
            string msg = "";
            try
            {
                SMS.SMS sms = new SMS.SMS();
                string Content = "尊敬的发货人，有会员(" + MemberAccount + ")补货，你的库存不足(补货信息请查看会员中心)，请及时补货。";
                string ReceiptContent = sms.SendSMS(MAccount, "240054", MemberAccount);
                Helper.SMSAdd(MAccount, Content, 1, 2, ReceiptContent);
                //库存不足消息记录
                using (EFDB db = new EFDB())
                {
                    MProduct MP = new MProduct();
                    MP.Gid = Guid.NewGuid();
                    MP.AddTime = DateTime.Now;
                    MP.MemberGid = MemberGid;
                    MP.ShopGid = ShopGid;
                    MP.Price = 0;
                    MP.Stock = Number;
                    MP.Money = Money;
                    MP.Integral = Integral;
                    db.MProduct.Add(MP);
                    db.SaveChanges();
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return msg;
        }

        /// <summary>
        /// 扣除库存
        /// </summary>
        /// <param name="OrderGid">订单Gid</param>
        /// <param name="MemberGid">购买人的Gid</param>
        /// <param name="ShopGid">发货人的Gid</param>
        /// <param name="Number">扣除的库存</param>
        /// <param name="Money">扣除的积分</param>
        /// <param name="Integral">扣除的购物积分</param>
        /// <param name="MemberAccount">购买会员手机</param>
        /// <returns>返回调用结果</returns>
        public static bool CLStock(Guid OrderGid, Guid MemberGid, Guid ShopGid, int Number, decimal Money, decimal Integral, string MemberAccount)
        {
            bool tf = false;
            string msg = "OrderGid=" + OrderGid.ToString() + ",ShopGid=" + ShopGid.ToString() + ",Number=" + Number.ToString() + ",Money=" + Money.ToString() + ",Integral=" + Integral.ToString();
            try
            {
                using (EFDB db = new EFDB())
                {
                    //查看合伙人库存足够不
                    Guid PGid = Helper.GetProductGid();
                    Stock Stock = db.Stock.Where(l => l.MemberGid == ShopGid && l.ProductGid == PGid).FirstOrDefault();
                    //查询发货人的数据
                    Member M = db.Member.Where(l => l.Gid == ShopGid).FirstOrDefault();
                    if (Stock != null && M != null)
                    {
                        //if (Stock.Number >= Number && M.Money >= Money && M.Integral >= Integral)
                        if (Stock.Number >= Number)
                        {
                            //减去库存
                            Stock.Number = Stock.Number - Number;
                            if (db.SaveChanges() == 1)
                            {
                                LogManager.WriteLog("彩链扣除库存", msg);

                                #region 发货扣除积分.逻辑8月去除
                                //减去积分
                                //if (Money > 0 || Integral > 0)
                                //{
                                //    //从发货人扣除积分
                                //    if (MoneyRecordAdd(OrderGid, ShopGid, -Money, -Integral, 24, "发货扣除积分") == null)
                                //    {
                                //        LogManager.WriteLog("彩链库存扣除成功扣除积分失败", msg);
                                //        return JsonConvert.SerializeObject(new { OrderNo = "", Title = "发货人库存异常", Error = "彩链库存扣除成功,扣除积分失败!" });
                                //    }
                                //    else
                                //    {
                                //        LogManager.WriteLog("彩链扣除积分", msg);
                                //    }
                                //}
                                #endregion

                                //如果正常返回ok
                                tf = true;
                            }
                            else
                            {
                                LogManager.WriteLog("彩链库存更新失败", msg);
                                //return JsonConvert.SerializeObject(new { OrderNo = "", Title = "发货人库存异常", Error = "彩链库存扣除失败!" });
                            }
                        }
                        else
                        {
                            CLSMS(M.Account, MemberAccount, ShopGid, MemberGid, Number, Money, Integral);
                            //return JsonConvert.SerializeObject(new { OrderNo = "", Title = "库存不足", Error = "你的发货人库存不足,请联系发货人(" + M.Account + ")补货!" });
                        }
                    }
                    else
                    {
                        if (M != null)
                        {
                            CLSMS(M.Account, MemberAccount, ShopGid, MemberGid, Number, Money, Integral);
                        }
                        //return JsonConvert.SerializeObject(new { OrderNo = "", Title = "发货人库存不足", Error = "请联系你的发货人(" + M.Account + ")补货!" });
                    }
                }
            }
            catch (Exception err)
            {
                msg += err.Message + "\r\n";
            }
            return tf;
        }

        #endregion

        #region 签名算法
        public static string BuildRequest(SortedDictionary<string, string> sParaTemp)
        {
            //获取过滤后的数组
            Dictionary<string, string> dicPara = new Dictionary<string, string>();
            sParaTemp.Add("api_key", "OGFhNDk5NGYzZjgxYzk0ZTJmN2UxNTUyMThmNTE5YTA=");
            dicPara = FilterPara(sParaTemp);
            //组合参数数组
            string prestr = CreateLinkString(dicPara);
            //获得加密结果转换为大写的加密串
            return MD5.GetMD5(prestr).ToUpper();
        }
        /// <summary>
        /// 把参数串成URL
        /// </summary>
        /// <param name="sParaTemp"></param>
        /// <returns></returns>
        public static string PostUrl(SortedDictionary<string, string> sParaTemp)
        {
            Dictionary<string, string> dicArray = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> temp in sParaTemp)
            {
                if (temp.Key != "api_key")
                {
                    dicArray.Add(temp.Key, temp.Value);
                }
            }
            //组合参数数组
            return CreateLinkString(dicArray);
        }

        /// <summary>
        /// 除去数组中的空值和签名参数并以字母a到z的顺序排序
        /// </summary>
        /// <param name="dicArrayPre">过滤前的参数组</param>
        /// <returns>过滤后的参数组</returns>
        public static Dictionary<string, string> FilterPara(SortedDictionary<string, string> dicArrayPre)
        {
            Dictionary<string, string> dicArray = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> temp in dicArrayPre)
            {
                if (temp.Key != "sign" && temp.Key != "ff")
                {
                    dicArray.Add(temp.Key, temp.Value);
                }
            }

            return dicArray;
        }

        //组合参数数组
        public static string CreateLinkString(Dictionary<string, string> dicArray)
        {
            StringBuilder prestr = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in dicArray)
            {
                prestr.Append(temp.Key + "=" + temp.Value + "&");
            }

            int nLen = prestr.Length;
            prestr.Remove(nLen - 1, 1);

            return prestr.ToString();
        }
        #endregion

        #region 额度记录
        /// <summary>
        /// 额度记录
        /// </summary>
        /// <param name="Gid">会员Gid</param>
        /// <param name="Money">操作的额度</param>
        /// <param name="CLMoney">原额度</param>
        /// <param name="Number">操作的彩链包</param>
        /// <param name="OldNumber">原彩链包</param>
        /// <param name="Remarks">备注</param>
        /// <returns>返回调用结果</returns>
        public static void CLRecordAdd(Guid Gid, decimal Money, decimal CLMoney, int Number, int OldNumber, string Remarks = "")
        {
            string LogMsg = "会员=" + Gid + ",Money=" + Money;
            try
            {
                using (EFDB db = new EFDB())
                {
                    var b = new CLRecord();
                    b.Gid = Guid.NewGuid();
                    b.AddTime = DateTime.Now;
                    b.MemberGid = Gid;
                    b.Money = Money;
                    b.OldMoney = CLMoney;
                    b.Number = Number;
                    b.OldNumber = OldNumber;
                    b.Remarks = Remarks;
                    db.CLRecord.Add(b);
                    if (db.SaveChanges() == 1)
                    {
                    }
                    else
                    {
                        LogManager.WriteLog("额度记录失败", LogMsg);
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.WriteLog("额度记录异常", LogMsg + "/r/n" + err.Message);
            }
        }
        #endregion

        #region 生成唯一邀请码
        public static int CreateMNumber()
        {
            int MID = int.Parse(RandStr.GenerateRandomCode(8));
            using (EFDB db = new EFDB())
            {
                if (db.Member.Where(l => l.MID == MID).Count() != 0)
                {
                    CreateMNumber();
                }
            }
            return MID;
        }
        #endregion

        #region 判断是否自己的下线
        public static bool IsMember(Guid Gid, Guid MemberGid)
        {
            using (EFDB db = new EFDB())
            {
                var b = db.Member.Where(l => l.Gid == Gid).FirstOrDefault();
                if (b.MemberGid != null)
                {
                    if (b.MemberGid != MemberGid)
                    {
                        return IsMember((Guid)b.MemberGid, MemberGid);
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region 查询自己所有下线
        public static List<Guid> MGidALL(Guid Gid, List<Member> Member, List<Guid> list)
        {
            using (EFDB db = new EFDB())
            {
                var m = Member.Where(l => l.MemberGid == Gid).ToList();
                foreach (var dr in m)
                {
                    list.Add(dr.Gid);
                    MGidALL(dr.Gid, Member, list);
                }
            }
            return list;
        }
        #endregion
    }
}