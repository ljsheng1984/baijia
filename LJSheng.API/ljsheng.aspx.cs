using Newtonsoft.Json;
using System;

namespace LJSheng.API
{
    public partial class ljsheng : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write(JsonConvert.SerializeObject(goapi.regjpush("sssssssssssss")));
        }
    }
}