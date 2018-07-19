using System;
using System.Linq;
using LJSheng.Data.EF;
using System.Collections.Generic;
using System.Data;
using static LJSheng.Data.Helps;
using LJSheng.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LJSheng.API
{
    /// <summary>
    /// APP接口调用类
    /// </summary>
    public static class goapi
    {
        #region 启动接口
        /// <summary>
        /// 启动接口
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">不为空时请吐丝出本参数内容</para>
        /// <para name="data">----如下----</para>
        /// <para name="app">-----APP版本更新对象-----</para>
        /// <para name="sfgx">是否强制更新[1=不是,2-是]</para>
        /// <para name="bbh">APP版本号</para>
        /// <para name="nbbbh">APP内部版本号</para>
        /// <para name="url">下载地址</para>
        /// <para name="jpush">-----jpush对象-----</para>
        /// <para name="AppKey">极光推送用到的Key</para>
        /// <para name="fxkey">-----分享要用到的KEY-----</para>
        /// <para name="AppID">微信ID</para>
        /// <para name="AppSecret">微信AppSecret</para>
        /// <para name="system">-----系统设置对象-----</para>
        /// <para name="cache">如果不对等,需要删除APP所有缓存数据</para>
        /// <para name="biaoti">弹出窗的标题提示语</para>
        /// <para name="lanjie">拦截次数</para>
        /// <para name="mbsj">---各个模板最新数据时间对象---</para>
        /// <para name="dxmb">防骗短信模板数据最新时间</para>
        /// <para name="dhmb">内置电话号码数据最新时间</para>
        /// <para name="dxlx">短信类型数据最新时间</para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object appstart()
        {
            EFDB db = new EFDB();
            //检查APP版本
            var app = db.app.Where(l => l.sjxt == ApiHelper.sjxt).OrderByDescending(l => l.rukusj).Select(l => new
            {
                l.sjxt,
                l.bbh,
                l.nbbbh,
                l.sfgx,
                l.gxnr,
                l.url
            });
            var zd = db.zdlb;
            return new ApiResult("", new
            {
                app,
                jpush = new { AppKey = "03c587853bcab3ad4ab82c29" /*zd.Where(l => l.jian == "AppKey").FirstOrDefault().zhi*/ },
                fxkey = new { AppID = "wxb86101a4f652e38c", AppSecret = "bb6d4b16cd5738474c83fc39a5451a73" },
                system = new
                {
                    cache = zd.Where(l => l.jian == "cache").FirstOrDefault().zhi,
                    biaoti = zd.Where(l => l.jian == "biaoti").FirstOrDefault().zhi,
                    lanjie = db.lanjie.Sum(l => l.cishu)
                },
                mbsj = new {
                    dxmb = zd.Where(l => l.jian == "dxmb").OrderByDescending(l => l.zhi).FirstOrDefault().zhi,
                    dhmb = zd.Where(l => l.jian == "dianhua").OrderByDescending(l => l.zhi).FirstOrDefault().zhi,
                    dxlx = zd.Where(l => l.jian == "dxlx").OrderByDescending(l => l.zhi).FirstOrDefault().zhi
                }
            });
        }
        #endregion

        #region 获取模版
        /// <summary>
        /// 获取防骗模版
        /// </summary>
        /// <param name="dxmb">本机模版的最新数据时间</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">最新模版时间</para>
        /// <para name="data">---对象如下---</para>
        /// <para name="lx">防骗类型[参数请参考getdxlx接口]</para>
        /// <para name="duanxin">短信内容</para>
        /// <para name="gjz">匹配关键字多个关键字|分隔</para>
        /// <para name="cishu">受骗人数</para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object getmuban(DateTime dxmb)
        {
            EFDB db = new EFDB();
            DateTime dx = DateTime.Parse(db.zdlb.Where(l => l.jian == "dxmb").FirstOrDefault().zhi);
            return new ApiResult(db.zdlb.Where(l => l.jian == "dxmb").FirstOrDefault().zhi, dx > dxmb ? db.dxmb.Where(l => l.zt == 1).Select(l => new { l.lx, l.duanxin, l.gjz, l.cishu }) : db.dxmb.Where(l => l.zt == 0).Select(l => new { l.lx, l.duanxin, l.gjz, l.cishu }));
        }

        /// <summary>
        /// 获取内置号码显名称模版
        /// </summary>
        /// <param name="dhmb">本机内置号码名称的最新数据时间</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">最新模版时间</para>
        /// <para name="data">---对象如下---</para>
        /// <para name="haoma">电话号码</para>
        /// <para name="xsmc">对应名称</para>
        /// <para name="xs">为1的时候不拦截此来电号码所有来电短信</para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object gethaoma(DateTime dhmb)
        {
            EFDB db = new EFDB();
            DateTime dh = DateTime.Parse(db.zdlb.Where(l => l.jian == "dianhua").FirstOrDefault().zhi);
            return new ApiResult(db.zdlb.Where(l => l.jian == "dianhua").FirstOrDefault().zhi, dh > dhmb ? db.dianhua.Select(l => new { l.haoma, l.xsmc,l.xs }) : db.dianhua.Where(l => l.haoma == "13960838300").Select(l => new { l.haoma, l.xsmc,l.xs }));
        }
        #endregion

        #region 短信类型
        /// <summary>
        /// 获取短信类型
        /// </summary>
        /// <param name="lxmb">短信类型的最新数据时间</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">最新短信类型时间</para>
        /// <para name="data">---对象如下---</para>
        /// <para name="name">类型名称</para>
        /// <para name="value">类型值</para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object getdxlx(DateTime lxmb)
        {
            List<object> list = new List<object>();
            foreach (string n in Enum.GetNames(typeof(dxlx)))
            {
                dxlx lx = (dxlx)Enum.Parse(typeof(dxlx), n, true);
                list.Add(new NameValue(n, (int)lx));
            }
            EFDB db = new EFDB();
            DateTime dxlx = DateTime.Parse(db.zdlb.Where(l => l.jian == "dxlx").FirstOrDefault().zhi);
            return new ApiResult(db.zdlb.Where(l => l.jian == "dxlx").FirstOrDefault().zhi, dxlx > lxmb ? list : null);
        }
        #endregion

        #region 获取分享
        /// <summary>
        /// 获取分享下载数据
        /// </summary>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">提示信息</para>
        /// <para name="data">---数据如下---</para>
        /// <para name="url">分享页面</para>
        /// <para name="img">封面图片</para>
        /// <para name="desc">说明</para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object getfx()
        {
            return new ApiResult("分享数据", new { url = "http://www.fpzs110.com/xiazai.html", img = "http://www.fpzs110.com/images/logo.png", desc = "防骗助手下载" });
        }
        #endregion

        #region 检测
        /// <summary>
        /// 提交检测
        /// </summary>
        /// <param name="duanxin">检测的短信</param>
        /// <param name ="lx">防骗类型[请参考短信类型接口]</param >
        /// <param name="weihai">危害程度</param>
        /// <param name="jingdu">经度</param>
        /// <param name="weidu">纬度</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">提示信息</para>
        /// <para name="data">返回参数lanjie=次数</para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object addjc(string duanxin, int lx, int weihai, string jingdu = "", string weidu = "")
        {
            if (duanxin.Length > 8)
            {
                EFDB db = new EFDB();
                jiance ef;
                ef = db.jiance.Where(l => l.duanxin == duanxin).FirstOrDefault();
                if (ef == null)
                {
                    ef = new jiance();
                    ef.gid = Guid.NewGuid();
                    ef.rukusj = DateTime.Now;
                    ef.duanxin = duanxin;
                    ef.lx = lx;
                    ef.weihai = weihai;
                    ef.jingdu = jingdu;
                    ef.weidu = weidu;
                    ef.zt = 1;
                    ef.cishu = 1;
                    ef.hygid = ApiHelper.gid;
                    db.jiance.Add(ef);
                }
                else
                {
                    ef.cishu = ef.cishu + 1;
                    ef.rukusj = DateTime.Now;
                }
                if (db.SaveChanges() == 1)
                {
                    return new ApiResult("检测成功", new { lanjie = db.lanjie.Sum(l => l.cishu) });
                }
                else
                {
                    return new ApiResult("检测失败,请重试");
                }
            }
            else
            {
                return new ApiResult("内容太少了", duanxin);
            }
        }
        #endregion

        #region 举报
        /// <summary>
        /// 提交举报
        /// </summary>
        /// <param name="list">举报的短信对象{duanxin=短信内容,haoma=短信号码}</param>
        /// <param name="jingdu">经度</param>
        /// <param name="weidu">纬度</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">提示信息</para>
        /// <para name="data"></para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object addjb(string list, string jingdu = "", string weidu = "")
        {
            JArray json = (JArray)JsonConvert.DeserializeObject(list);
            EFDB db = new EFDB();
            jubao ef;
            int num = 0;
            foreach (var j in json)
            {
                string duanxin = j["duanxin"].ToString();
                if (duanxin.Length > 8)
                {
                    string haoma = j["haoma"].ToString();
                    ef = db.jubao.Where(l => l.duanxin == duanxin).FirstOrDefault();
                    if (ef == null)
                    {
                        ef = new jubao();
                        ef.gid = Guid.NewGuid();
                        ef.rukusj = DateTime.Now;
                        ef.duanxin = duanxin;
                        ef.haoma = haoma;
                        ef.jingdu = jingdu;
                        ef.weidu = weidu;
                        ef.zt = 1;
                        ef.cishu = 1;
                        ef.lx = 0;
                        ef.weihai = 0;
                        ef.hygid = ApiHelper.gid;
                        ef.hdzt = 1;
                        db.jubao.Add(ef);
                    }
                    else
                    {
                        ef.cishu = ef.cishu + 1;
                        ef.rukusj = DateTime.Now;
                    }
                    num += db.SaveChanges();
                }
                else
                { return new ApiResult("内容太少了", num); }
            }
            if (num == 0)
            {
                return new ApiResult("失败");
            }
            else
            {
                return new ApiResult("成功", num);
            }
        }
        #endregion

        #region 拦截
        /// <summary>
        /// 拦截到短信时候调用
        /// </summary>
        /// <param name="duanxin">拦截到的短信</param>
        /// <param name="haoma">发送号码</param>
        /// <param name="lx">防骗类型[0=危险短信 1=中奖短信 2=防骗短信	3=保险理财]</param>
        /// <param name="weihai">危害程度</param>
        /// <param name="jingdu">经度</param>
        /// <param name="weidu">纬度</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">提示信息</para>
        /// <para name="data"></para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object addlanjie(string duanxin, string haoma, int lx, int weihai, string jingdu = "", string weidu = "")
        {
            EFDB db = new EFDB();
            lanjie ef;
            ef = db.lanjie.Where(l => l.duanxin == duanxin).FirstOrDefault();
            if (ef == null)
            {
                ef = new lanjie();
                ef.gid = Guid.NewGuid();
                ef.rukusj = DateTime.Now;
                ef.duanxin = duanxin;
                ef.haoma = haoma;
                ef.jingdu = jingdu;
                ef.weidu = weidu;
                ef.cishu = 1;
                ef.lx = lx;
                ef.weihai = weihai;
                db.lanjie.Add(ef);
            }
            else
            {
                ef.cishu = ef.cishu + 1;
                ef.rukusj = DateTime.Now;
            }
            if (db.SaveChanges() == 1)
            {
                return new ApiResult("拦截成功", new { });
            }
            else
            {
                return new ApiResult("次数成功,拦截数据插入失败", "");
            }
        }
        #endregion

        #region 新闻资讯
        /// <summary>
        /// 获取新闻资讯
        /// </summary>
        /// <param name="shuliang">获取的数量</param>
        /// <param name="yeshu">当前页面(1=为第一页)</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">提示信息</para>
        /// <para name="data">----如下----</para>
        /// <para name="tupianurl">图片访问的前缀URL</para>
        /// <para name="url">新闻访问的前缀URL加上参数gid</para>
        /// <para name="list">---新闻列表---</para>
        /// <para name="gid">新闻的GID</para>
        /// <para name="rukusj">录入时间</para>
        /// <para name="tupian">封面图片名称</para>
        /// <para name="biaoti">标题</para>
        /// <para name="fubiao">副标题摘要</para>
        /// <para name="nrong">新闻正文</para>
        /// <para name="laiyuan">文章来源</para>
        /// <para name="zuozhe">作者</para>
        /// <para name="fwl">访问量</para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object getxinwen(int shuliang, int yeshu)
        {
            DataTable list = Tables.Table_List("xinwen", "px DESC,rukusj DESC", "gid,rukusj,tupian,biaoti,fubiao,nrong,laiyuan,zuozhe,fwl", shuliang, yeshu, "xs=1").Tables[0];
            return new ApiResult("新闻资讯", new { url = url,tupianurl = imgurl + Helps.xinwen,list });
        }
        #endregion

        #region 推送相关
        /// <summary>
        /// 绑定推送用户
        /// </summary>
        /// <param name="registrationid">手机ID</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">提示信息</para>
        /// <para name="data">-----如下-----</para>
        /// <para name="alias">绑定的推送别名</para>
        /// <para name="gid">手机ID注册得到的GID</para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object regjpush(string registrationid)
        {
            if (string.IsNullOrEmpty(registrationid))
            {
                return new ApiResult("registrationid不能为空");
            }
            else
            {
                EFDB db = new EFDB();
                //根据手机ID注册用户
                var hy = db.huiyuan.Where(l => l.openid == registrationid).FirstOrDefault();
                Guid hygid = Guid.NewGuid();
                if (hy == null)
                {
                    var b = new LJSheng.Data.EF.huiyuan
                    {
                        gid = hygid,
                        zhanghao = registrationid,
                        mima = LJSheng.Common.MD5.GetMD5ljsheng(registrationid),
                        xb = 0,
                        openid = registrationid,
                        rukusj = DateTime.Now
                    };
                    db.huiyuan.Add(b);
                    db.SaveChanges();
                }
                else
                {
                    hygid = hy.gid;
                }

                string alias = DateTime.Now.ToString("yyMMddHHmmss") + Common.RandStr.GetRandomNumberString(2) + Common.RandStr.CreateValidateNumber(2);
                var jpush = db.jpush.Where(l => l.registrationid == registrationid).FirstOrDefault();
                if (jpush == null)//没有手机ID记录
                {
                    //JPushClient jc = new JPushClient("03c587853bcab3ad4ab82c29", "1e5f04cb6b33a116ef4d7aa4");
                    ////HashSet<string> tagsToAdd = new HashSet<string>();
                    ////tagsToAdd.Add("add");
                    //DefaultResult result = jc.updateDeviceTagAlias(registrationid, alias, null, null);
                    //if (result.isResultOK() == true)
                    //{
                        Guid gid = Guid.NewGuid();
                        var j = new jpush
                        {
                            gid = gid,
                            lx = ApiHelper.sjxt,
                            alias = alias,
                            registrationid = registrationid,
                            rukusj = DateTime.Now
                        };
                        db.jpush.Add(j);
                        if (db.SaveChanges() == 1)
                        {
                            return new ApiResult("注册别名成功", new { alias, gid = hygid });
                        }
                        else {
                            return new ApiResult("入库失败");
                        }
                    //}
                    //else
                    //{
                    //    return new ApiResult("极光绑定别名失败");
                    //}
                }
                else//有手机ID记录
                {
                    return new ApiResult("获取别名", new { jpush.alias, gid = hygid });
                }
            }
        }

        /// <summary>
        /// 注销别名绑定的手机ID
        /// </summary>
        /// <param name="registrationid">手机ID</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">提示信息</para>
        /// <para name="data"></para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object qxjpush(string registrationid)
        {
            EFDB db = new EFDB();
            var jpush = db.jpush.Where(l => l.registrationid == registrationid).FirstOrDefault();
            if (jpush != null)
            {
                jpush.alias = null;
                db.SaveChanges();
                return new ApiResult("注销成功", new { jpush.alias });
            }
            else
            {
                return new ApiResult("注销失败", new { registrationid });
            }
        }
        #endregion

        #region 意见反馈
        /// <summary>
        /// 提交反馈
        /// </summary>
        /// <param name="wenti">提问的问题</param>
        /// <param name="lxfs">联系方式</param>
        /// <param name="tplist">反馈的图片</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">提示信息</para>
        /// <para name="data"></para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object addyjfk(string wenti = "", string lxfs = "", string tplist = "")
        {
            if (wenti.Length > 5 || !string.IsNullOrEmpty(tplist))
            {
                EFDB db = new EFDB();
                Guid gid = Guid.NewGuid();
                var b = new yjfk
                {
                    gid = gid,
                    wenti = wenti,
                    zt = 2,
                    beizhu = ApiHelper.sjxh + "," + ((sjxt)Enum.Parse(typeof(LJSheng.Data.Helps.sjxt), ApiHelper.sjxt.ToString(), true)).ToString() + "," + ApiHelper.bbh,
                    lxfs = lxfs,
                    rukusj = DateTime.Now
                };
                db.yjfk.Add(b);
                if (db.SaveChanges() == 1)
                {
                    //插入反馈的图片
                    if (!String.IsNullOrEmpty(tplist))
                    {
                        string[] list = tplist.Split('#');
                        foreach (string s in list)
                        {
                            Guid yjgid = Guid.NewGuid();
                            var tp = new yjtp
                            {
                                gid = yjgid,
                                yjgid = gid,
                                tupian = s,
                                rukusj = DateTime.Now
                            };
                            db.yjtp.Add(tp);
                            db.SaveChanges();
                        }
                    }
                    return new ApiResult("提交成功", new { });
                }
                else
                {
                    return new ApiResult("提交失败");
                }
            }
            else
            {
                return new ApiResult("请详细描述问题!");
            }
        }

        /// <summary>
        /// 获取意见反馈
        /// </summary>
        /// <param name="shuliang">一次获取的条数</param>
        /// <param name="yeshu">当前页</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">提示信息</para>
        /// <para name="data">----如下----</para>
        /// <para name="tupianurl">图片访问服务器URL</para>
        /// <para name="list">----如下----</para>
        /// <para name="wenti">提问问题</para>
        /// <para name="huifu">回复内容</para>
        /// <para name="hfsj">回复时间</para>
        /// <para name="rukusj">提问时间</para>
        /// <para name="list">意见反馈图片列表(tupiao)</para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object getyjfk(int shuliang, int yeshu)
        {
            DataSet ds = Data.Tables.Table_List("yjfk", "rukusj DESC", "*", shuliang, yeshu, "hygid='" + ApiHelper.gid.ToString() + "'");
            List<object> list = new List<object>();
            if (ds.Tables[0].Rows.Count > 0)
            {
                EFDB db = new EFDB();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    Guid gid = Guid.Parse(dr["gid"].ToString());
                    list.Add(new YJFKList(dr["wenti"].ToString(), dr["huifu"].ToString(), dr["hfsj"].ToString(), dr["rukusj"].ToString(), db.yjtp.Where(tp => tp.yjgid == gid).Select(t => new { t.tupian })));
                }
                return new ApiResult("意见反馈",new { tupianurl = imgurl + Helps.yjfk, list });
            }
            else
            {
                return new ApiResult("你没有提交过意见反馈",new { });
            }
        }
        #endregion

        #region APP-BUG
        /// <summary>
        /// APP提交BUG
        /// </summary>
        /// <param name="xiaoxi">手机型号系统</param>
        /// <param name="canshu">错误日记</param>
        /// <returns>返回调用结果</returns>
        /// <para name="code">200=成功 其他请参看代码表</para>
        /// <para name="msg">提示信息</para>
        /// <para name="data">返回成功的APP版本</para>
        /// <remarks>
        /// 2016-03-12 林建生
        /// </remarks>
        public static object appbug(string xiaoxi, string canshu)
        {
            EFDB db = new EFDB();
            apibug bug = new apibug()
            {
                gid = Guid.NewGuid(),
                rukusj = DateTime.Now,
                ffm = ((Helps.sjxt)Enum.Parse(typeof(Helps.sjxt), ApiHelper.sjxt.ToString(), true)).ToString(),
                mcheng = ApiHelper.sjxh,
                xiaoxi = xiaoxi,
                duizhai = ((Helps.sjxt)Enum.Parse(typeof(Helps.sjxt), ApiHelper.sjxt.ToString(), true)).ToString(),
                canshu = canshu,
                deskey = ApiHelper.bbh.ToString()
            };
            db.apibug.Add(bug);
            db.SaveChanges();
            return new ApiResult("提交成功", ApiHelper.bbh);
        }
        #endregion
    }
}