//-----------------------------------------------------------
// 描    述: Web.Config 操作
// 修改标识: 林建生 1984-02-04
// 修改内容: LJSheng 项目通用类
//-----------------------------------------------------------
using System.Configuration;
using System.Web.Configuration;

namespace LJSheng.Common
{
    public static class WebConfig
    {
        public static void Add(string appname, string appvalue)
        {
            //打开配置文件
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
            //获取appSettings节点
            AppSettingsSection appSection = (AppSettingsSection)config.GetSection("appSettings");
            //在appSettings节点中添加元素
            appSection.Settings.Add(appname, appvalue);
            config.Save();

        }

        public static void Delete(string appname)
        {
            //打开配置文件
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
            //获取appSettings节点
            AppSettingsSection appSection = (AppSettingsSection)config.GetSection("appSettings");
            //删除appSettings节点中的元素
            appSection.Settings.Remove(appname);
            config.Save();
        }

        public static void Update(string appname, string appvalue)
        {
            //打开配置文件
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
            //获取appSettings节点
            AppSettingsSection appSection = (AppSettingsSection)config.GetSection("appSettings");
            //修改appSettings节点中的元素
            appSection.Settings[appname].Value = appvalue;
            config.Save();
        }
    }
}
