using System.Web;
using System.Web.Optimization;

namespace LJSheng.Web
{
    public class BundleConfig
    {
        // 有关捆绑的详细信息，请访问 https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/js").Include(
                      "~/js/jquery.js",
                      "~/plugins/mlayer/layer.js"
                      ));
            bundles.Add(new StyleBundle("~/css").Include(
                      "~/css/main.css"));

            bundles.Add(new ScriptBundle("~/mlist").Include(
          "~/js/template-web.js",
          "~/plugins/mescroll/mescroll.js",
          "~/js/mlist.js"));

            bundles.Add(new StyleBundle("~/mcss").Include(
                      "~/plugins/mescroll/mescroll.css"));
            BundleTable.EnableOptimizations = false;

            bundles.Add(new StyleBundle("~/zcss").Include(
                      "~/plugins/mzui/css/mzui.min.css"));
            bundles.Add(new ScriptBundle("~/zjs").Include(
                      "~/plugins/mzui/js/mzui.min.js"
                      ));
        }
    }
}
