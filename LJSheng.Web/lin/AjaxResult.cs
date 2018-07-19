namespace LJSheng.Web
{
    public class AjaxResult
    {
        public int result { get; set; }
        public object data { get; set; }

        public AjaxResult(int result, object data)
        {
            this.result = result;
            this.data = data;
        }

        public AjaxResult(object data)
        {
            this.result = 200;
            this.data = data;
        }
    }
}