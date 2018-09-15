using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace LJSheng.Data
{
    public class EFDB : DbContext
    {
        /// <summary>
        /// EF对象
        /// </summary>
        public EFDB(): base("name=MSSQL")
        {
            //模型更改时重新创建数据库
            //Database.SetInitializer<EFDB>(new DropCreateDatabaseIfModelChanges<EFDB>());
            //数据库不存在时重新创建数据库,存在的话会报错
            Database.SetInitializer<EFDB>(new CreateDatabaseIfNotExists<EFDB>());
            //每次启动应用程序时创建数据库
            //Database.SetInitializer<EFDB>(new DropCreateDatabaseAlways<EFDB>());
            //从不创建数据库
            //Database.SetInitializer<EFDB>(null);
        }

        /// <summary>
        /// 禁止创建表的时候表名复数
        /// </summary>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
        /// <summary>
        /// ApiBug
        /// </summary>
        public DbSet<ApiBug> ApiBug { get; set; }
        /// <summary>
        /// App
        /// </summary>
        public DbSet<App> App { get; set; }
        /// <summary>
        /// AppApi
        /// </summary>
        public DbSet<AppApi> AppApi { get; set; }
        /// <summary>
        /// 商品分类
        /// </summary>
        public DbSet<Classify> Classify { get; set; }
        /// <summary>
        /// 评论
        /// </summary>
        public DbSet<Comment> Comment { get; set; }
        /// <summary>
        /// 字典
        /// </summary>
        public DbSet<Dictionaries> Dictionaries { get; set; }
        /// <summary>
        /// 字典列表
        /// </summary>
        public DbSet<DictionariesList> DictionariesList { get; set; }
        /// <summary>
        /// 快递
        /// </summary>
        public DbSet<Express> Express { get; set; }
        /// <summary>
        /// LJSheng
        /// </summary>
        public DbSet<LJSheng> LJSheng { get; set; }
        /// <summary>
        /// 会员
        /// </summary>
        public DbSet<Member> Member { get; set; }
        /// <summary>
        /// 新闻资讯
        /// </summary>
        public DbSet<News> News { get; set; }
        /// <summary>
        /// 开通城市
        /// </summary>
        public DbSet<OpenCity> OpenCity { get; set; }
        /// <summary>
        /// 订单
        /// </summary>
        public DbSet<Order> Order { get; set; }
        /// <summary>
        /// 订单详情
        /// </summary>
        public DbSet<OrderDetails> OrderDetails { get; set; }
        /// <summary>
        /// 产品
        /// </summary>
        public DbSet<Product> Product { get; set; }
        /// <summary>
        /// 短信
        /// </summary>
        public DbSet<SMS> SMS { get; set; }
        /// <summary>
        /// WXKeyword
        /// </summary>
        public DbSet<WXKeyword> WXKeyword { get; set; }
        /// <summary>
        /// 资金记录
        /// </summary>
        public DbSet<MoneyRecord> MoneyRecord { get; set; }
        /// <summary>
        /// 分润详情报表
        /// </summary>
        public DbSet<ReportList> ReportList { get; set; }
        /// <summary>
        /// 报表
        /// </summary>
        public DbSet<Report> Report { get; set; }
        /// <summary>
        /// AdminLog
        /// </summary>
        public DbSet<AdminLog> AdminLog { get; set; }
        /// <summary>
        /// 会员等级
        /// </summary>
        public DbSet<Level> Level { get; set; }
        /// <summary>
        /// 等级关联
        /// </summary>
        public DbSet<LV> LV { get; set; }
        /// <summary>
        /// 广告
        /// </summary>
        public DbSet<AD> AD { get; set; }
        /// <summary>
        /// 送货地址
        /// </summary>
        public DbSet<Address> Address { get; set; }
        /// <summary>
        /// 项目股权分配
        /// </summary>
        public DbSet<StockRight> StockRight { get; set; }
        /// <summary>
        /// 会员注册上下级关系
        /// </summary>
        public DbSet<MRelation> MRelation { get; set; }
        /// <summary>
        /// 库存详情
        /// </summary>
        public DbSet<Stock> Stock { get; set; }
        /// <summary>
        /// 提现
        /// </summary>
        public DbSet<Withdrawals> Withdrawals { get; set; }
        /// <summary>
        /// 商家
        /// </summary>
        public DbSet<Shop> Shop { get; set; }
        /// <summary>
        /// 商家分类
        /// </summary>
        public DbSet<ShopClassify> ShopClassify { get; set; }
        /// <summary>
        /// 商家产品
        /// </summary>
        public DbSet<ShopProduct> ShopProduct { get; set; }
        /// <summary>
        /// 商家提现
        /// </summary>
        public DbSet<ShopWithdrawals> ShopWithdrawals { get; set; }
        /// <summary>
        /// 商家订单
        /// </summary>
        public DbSet<ShopOrder> ShopOrder { get; set; }
        /// <summary>
        /// 业绩
        /// </summary>
        public DbSet<Achievement> Achievement { get; set; }

        /// <summary>
        /// 会员库存转让不足
        /// </summary>
        public DbSet<MProduct> MProduct { get; set; }
        /// <summary>
        /// 发货人
        /// </summary>
        public DbSet<Consignor> Consignor { get; set; }

        /// <summary>
        /// 积分转Token记录
        /// </summary>
        public DbSet<TokenRecord> TokenRecord { get; set; }
        /// <summary>
        /// 商城积分记录
        /// </summary>
        public DbSet<ShopRecord> ShopRecord { get; set; }
        /// <summary>
        /// 彩链包兑换记录
        /// </summary>
        public DbSet<CLRecord> CLRecord { get; set; }
        /// <summary>
        /// 金额记录
        /// </summary>
        public DbSet<RMBRecord> RMBRecord { get; set; }
        /// <summary>
        /// 商家分类关联列表
        /// </summary>
        public DbSet<ShopProject> ShopProject { get; set; }
        /// <summary>
        /// 购物车
        /// </summary>
        public DbSet<Cart> Cart { get; set; }
    }
}
