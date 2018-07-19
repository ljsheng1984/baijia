//----------------------------------------
//创建描述: 接受/发送消息帮助类
//创建信息: 2014-07-21 林建生
//----------------------------------------
using LJSheng.Data;
using System;
using System.Linq;
using System.Xml;

namespace LJSheng.WX
{
    public class messageHelp
    {
        string Url = "";
        string path = "/uploadfiles/News/";

        #region 对微信发送过来的数据转换成,分析接收到的类型是什么类型(一个逻辑多个函数处理只用一个代码块) - 林XX 2015-03-18(不是本人创建的文件增加函数要备注)
        /// <summary>
        /// 接收 postStr 字符串 进行XML解析,获取消息类型(函数作用说明,可以把重要的函数逻辑写这里)
        /// </summary>
        /// <param name="postStr">解析的数据包</param>
        /// <returns>返回responseContent给微信</returns>
        public string ReturnMessage(string postStr)
        {
            string responseContent = "";//返回给微信的变量
            //进行字符串解析
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(new System.IO.MemoryStream(System.Text.Encoding.GetEncoding("utf-8").GetBytes(postStr)));
            XmlNode MsgType = xmldoc.SelectSingleNode("/xml/MsgType");
            //增加/修改判断消息类型 - 林建生 2015-03-18(修改或增加不是自己的写的函数加此备注)
            if (MsgType != null)
            {
                switch (MsgType.InnerText)
                {
                    case "event"://事件
                        responseContent = EventHandle(xmldoc);//事件处理
                        break;
                    case "text"://文本
                        responseContent = TextHandle(xmldoc);//接受文本消息处理
                        break;
                    case "image"://图片
                        responseContent = ImageHandle(xmldoc);
                        break;
                    case "voice": //声音

                        break;

                    case "video"://视频

                        break;

                    case "location"://地理位置

                        break;
                    case "link"://链接

                        break;
                    default:
                        break;
                }
            }
            return responseContent;
        }
        #endregion

        #region 处理图片消息类型并应答
        /// <summary>
        /// 图片消息类型的返回
        /// </summary>
        /// <param name="xmldoc">微信发过来的XML</param>
        /// <returns>返回XML格式的消息类型</returns>
        public string ImageHandle(XmlDocument xmldoc)
        {
            string responseContent = "";
            XmlNode ToUserName = xmldoc.SelectSingleNode("/xml/ToUserName");
            XmlNode FromUserName = xmldoc.SelectSingleNode("/xml/FromUserName");
            XmlNode PicUrl = xmldoc.SelectSingleNode("/xml/PicUrl");
            return responseContent;
        }
        #endregion

        #region 处理事件类型并应答
        /// <summary>
        /// 处理事件类型
        /// </summary>
        /// <param name="xmldoc">微信发过来的XML</param>
        /// <returns>返回事件类型的消息</returns>
        public string EventHandle(XmlDocument xmldoc)
        {
            XmlNode Event = xmldoc.SelectSingleNode("/xml/Event");
            XmlNode EventKey = xmldoc.SelectSingleNode("/xml/EventKey");
            XmlNode ToUserName = xmldoc.SelectSingleNode("/xml/ToUserName");
            XmlNode FromUserName = xmldoc.SelectSingleNode("/xml/FromUserName");
            string responseContent = "";
            using (EFDB db = new EFDB())
            {
                var query = db.WXKeyword.Where(l => l.Keyword.Contains("其他")).FirstOrDefault();
                if (query != null)
                {
                    responseContent = string.Format(ReplyType.Message_Text,
                    FromUserName.InnerText,
                    ToUserName.InnerText,
                    DateTime.Now.Ticks,
                    query.Content);
                }
                if (Event != null)
                {
                    switch (Event.InnerText)
                    {
                        //事件类型，subscribe(订阅)
                        case "subscribe":
                            query = db.WXKeyword.Where(l => l.Keyword.Contains("关注")).FirstOrDefault();
                            if (query != null)
                            {
                                if (query.Type == 2)
                                {
                                    responseContent = string.Format(
                                        ReplyType.Message_Text,
                                        FromUserName.InnerText,
                                        ToUserName.InnerText,
                                        DateTime.Now.Ticks,
                                        query.Content);
                                }
                                else
                                {
                                    string News_Item = "";
                                    string ArticleCount = "1";
                                    if (query.Content.IndexOf('$') != -1)
                                    {
                                        string[] wzlist = query.Content.Split('$');
                                        ArticleCount = wzlist.Length.ToString();
                                        for (int i = 0; i < wzlist.Length; i++)
                                        {
                                            Guid Gid = Guid.Parse(wzlist[i]);
                                            var News = db.News.Where(l => l.Gid == Gid).FirstOrDefault();
                                            if (News != null)
                                            {
                                                News_Item += string.Format(
                                                    ReplyType.Message_News_Item,
                                                    News.Profile,
                                                    News.Title,
                                                    Url + path + News.Picture,
                                                    string.IsNullOrEmpty(News.Url) ? Url + News.Gid : News.Url);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Guid Gid = Guid.Parse(query.Content);
                                        var News = db.News.Where(l => l.Gid == Gid).FirstOrDefault();
                                        if (News != null)
                                        {
                                            News_Item = string.Format(
                                                ReplyType.Message_News_Item,
                                                News.Profile,
                                                News.Title,
                                                Url + path + News.Picture,
                                                string.IsNullOrEmpty(News.Url) ? Url + News.Gid : News.Url);
                                        }
                                    }
                                    responseContent = string.Format(
                                        ReplyType.Message_News_Main,
                                        FromUserName.InnerText,
                                        ToUserName.InnerText,
                                        DateTime.Now.Ticks,
                                        ArticleCount,
                                        News_Item);
                                }
                            }
                            break;
                        //菜单单击事件
                        case "CLICK":
                            string imgid = media.UploadMultimedia(FromUserName.InnerText);
                            if (EventKey.InnerText == "二维码名片")
                            {
                                if (String.IsNullOrEmpty(imgid))
                                {
                                    responseContent = string.Format(
                                        ReplyType.Message_Text,
                                        FromUserName.InnerText,
                                        ToUserName.InnerText,
                                        DateTime.Now.Ticks,
                                        "获取推荐失败,请退出重新登录一次!");
                                }
                                else
                                {
                                    responseContent = string.Format(
                                        ReplyType.Message_image,
                                        FromUserName.InnerText,
                                        ToUserName.InnerText,
                                        DateTime.Now.Ticks,
                                        imgid);
                                }
                            }
                            else
                            {
                                query = db.WXKeyword.Where(l => l.Keyword.Contains(EventKey.InnerText)).FirstOrDefault();
                                if (query != null)
                                {
                                    if (query.Type == 2)
                                    {
                                        responseContent = string.Format(
                                            ReplyType.Message_Text,
                                            FromUserName.InnerText,
                                            ToUserName.InnerText,
                                            DateTime.Now.Ticks,
                                            query.Content);
                                    }
                                    else
                                    {
                                        string News_Item = "";
                                        string ArticleCount = "1";
                                        if (query.Content.IndexOf('$') != -1)
                                        {
                                            string[] wzlist = query.Content.Split('$');
                                            ArticleCount = wzlist.Length.ToString();
                                            for (int i = 0; i < wzlist.Length; i++)
                                            {
                                                Guid Gid = Guid.Parse(wzlist[i]);
                                                var News = db.News.Where(l => l.Gid == Gid).FirstOrDefault();
                                                if (News != null)
                                                {
                                                    News_Item += string.Format(
                                                        ReplyType.Message_News_Item,
                                                        News.Profile,
                                                        News.Title,
                                                        Url + path + News.Picture,
                                                        string.IsNullOrEmpty(News.Url) ? Url + News.Gid : News.Url);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Guid Gid = Guid.Parse(query.Content);
                                            var News = db.News.Where(l => l.Gid == Gid).FirstOrDefault();
                                            if (News != null)
                                            {
                                                News_Item = string.Format(
                                                    ReplyType.Message_News_Item,
                                                    News.Profile,
                                                    News.Title,
                                                    Url + path + News.Picture,
                                                    string.IsNullOrEmpty(News.Url) ? Url + News.Gid : News.Url);
                                            }
                                        }
                                        responseContent = string.Format(
                                            ReplyType.Message_News_Main,
                                            FromUserName.InnerText,
                                            ToUserName.InnerText,
                                            DateTime.Now.Ticks,
                                            ArticleCount,
                                            News_Item);
                                    }
                                }
                            }
                            break;
                        //菜单单击事件
                        case "VIEW":
                            responseContent = "";
                            break;
                        default:
                            break;
                    }
                }
            }
            return responseContent;
        }
        #endregion

        #region 处理文本消息类型并应答
        /// <summary>
        /// 文本消息类型的返回
        /// </summary>
        /// <param name="xmldoc">微信发过来的XML</param>
        /// <returns>返回XML格式的消息类型</returns>
        public string TextHandle(XmlDocument xmldoc)
        {
            XmlNode ToUserName = xmldoc.SelectSingleNode("/xml/ToUserName");
            XmlNode FromUserName = xmldoc.SelectSingleNode("/xml/FromUserName");
            XmlNode Content = xmldoc.SelectSingleNode("/xml/Content");
            string responseContent = "";
            using (EFDB db = new EFDB())
                {
                var query = db.WXKeyword.Where(l => l.Keyword.Contains("其他")).FirstOrDefault();
                if (query != null)
                {
                    responseContent = string.Format(ReplyType.Message_Text,
                                                    FromUserName.InnerText,
                                                    ToUserName.InnerText,
                                                    DateTime.Now.Ticks,
                                                    query.Content);
                }
                //匹配数据库对应的关键字
                if (Content != null)
                {
                    query = db.WXKeyword.Where(l => l.Keyword.Contains(Content.InnerText)).FirstOrDefault();
                    if (query != null)
                    {
                        if (query.Type == 2)
                        {
                            responseContent = string.Format(ReplyType.Message_Text,
                                                               FromUserName.InnerText,
                                                               ToUserName.InnerText,
                                                               DateTime.Now.Ticks,
                                                               query.Content);
                        }
                        else
                        {
                            string News_Item = "";
                            string ArticleCount = "1";
                            if (query.Content.IndexOf('$') != -1)
                            {
                                string[] wzlist = query.Content.Split('$');
                                ArticleCount = wzlist.Length.ToString();
                                for (int i = 0; i < wzlist.Length; i++)
                                {
                                    Guid Gid = Guid.Parse(wzlist[i]);
                                    var News = db.News.Where(l => l.Gid == Gid).FirstOrDefault();
                                    if (News != null)
                                    {
                                        News_Item += string.Format(ReplyType.Message_News_Item,
                                                    News.Profile,
                                                    News.Title,
                                                    Url + path + News.Picture,
                                                    string.IsNullOrEmpty(News.Url) ? Url + News.Gid : News.Url);
                                    }
                                }
                            }
                            else
                            {
                                Guid Gid = Guid.Parse(query.Content);
                                var News = db.News.Where(l => l.Gid == Gid).FirstOrDefault();
                                if (News != null)
                                {
                                    News_Item = string.Format(ReplyType.Message_News_Item,
                                                    News.Profile,
                                                    News.Title,
                                                    Url + path + News.Picture,
                                                    string.IsNullOrEmpty(News.Url) ? Url + News.Gid : News.Url);
                                }
                            }
                            responseContent = string.Format(ReplyType.Message_News_Main,
                                                                                      FromUserName.InnerText,
                                                                                      ToUserName.InnerText,
                                                                                      DateTime.Now.Ticks,
                                                                                      ArticleCount,
                                                                                      News_Item);
                        }
                    }
                }
            }
            return responseContent;
        }
        #endregion
    }

    #region 微信回复类型列表
    public class ReplyType
    {
        /// <summary>
        /// 普通文本消息
        /// </summary>
        public static string Message_Text
        {
            get
            {
                return @"<xml>
                                                    <ToUserName><![CDATA[{0}]]></ToUserName>
                                                    <FromUserName><![CDATA[{1}]]></FromUserName>
                                                    <CreateTime>{2}</CreateTime>
                                                    <MsgType><![CDATA[text]]></MsgType>
                                                    <Content><![CDATA[{3}]]></Content>
                                                    </xml>";
            }
        }

        /// <summary>
        /// 图文消息主体
        /// </summary>
        public static string Message_News_Main
        {
            get
            {
                return @"<xml>
                                                    <ToUserName><![CDATA[{0}]]></ToUserName>
                                                    <FromUserName><![CDATA[{1}]]></FromUserName>
                                                    <CreateTime>{2}</CreateTime>
                                                    <MsgType><![CDATA[News]]></MsgType>
                                                    <ArticleCount>{3}</ArticleCount>
                                                    <Articles>
                                                    {4}
                                                    </Articles>
                                                    </xml> ";
            }
        }

        /// <summary>
        /// 图文消息项
        /// </summary>
        public static string Message_News_Item
        {
            get
            {
                return @"<item>
                                                    <title><![CDATA[{0}]]></title> 
                                                    <Description><![CDATA[{1}]]></Description>
                                                    <PicUrl><![CDATA[{2}]]></PicUrl>
                                                    <Url><![CDATA[{3}]]></Url>
                                                    </item>";
            }
        }

        /// <summary>
        /// 回复图片
        /// </summary>
        public static string Message_image
        {
            get
            {
                return @"<xml>
                                                    <ToUserName><![CDATA[{0}]]></ToUserName>
                                                    <FromUserName><![CDATA[{1}]]></FromUserName>
                                                    <CreateTime>{2}</CreateTime>
                                                    <MsgType><![CDATA[image]]></MsgType>
                                                    <Image>
                                                    <MediaId><![CDATA[{3}]]></MediaId>
                                                    </Image>
                                                    </xml>";
            }
        }
    }
    #endregion
}