namespace LJSheng.API
{
    #region 属性类
    /// <summary>
    /// 爱配音属性
    /// </summary>
    /// <remarks>
    /// 2015-11-11 林建生
    /// </remarks>
    public class SPList
    {
        private string _name;
        private int _value;
        private string _ico;
        private object _list;
        public string name
        {
            get { return _name; }
        }
        public int value
        {
            get { return _value; }
        }
        public string ico
        {
            get { return _ico; }
        }
        public object list
        {
            get { return _list; }
        }

        public SPList(string name, int value, string ico, object list)
        {
            this._name = name;
            this._value = value;
            this._ico = ico;
            this._list = list;
        }
    }

    /// <summary>
    /// 评论属性
    /// </summary>
    /// <remarks>
    /// 2015-11-11 林建生
    /// </remarks>
    public class PLList
    {
        private string _nicheng;
        private string _nrong;
        private string _tupian;
        private string _rukusj;
        private string _dz;
        private object _list;
        public string nicheng
        {
            get { return _nicheng; }
        }
        public string nrong
        {
            get { return _nrong; }
        }
        public string tupian
        {
            get { return _tupian; }
        }
        public string rukusj
        {
            get { return _rukusj; }
        }
        public string dz
        {
            get { return _dz; }
        }
        public object list
        {
            get { return _list; }
        }

        public PLList(string nicheng, string nrong, string tupian, string rukusj, string dz, object list)
        {
            this._nicheng = nicheng;
            this._nrong = nrong;
            this._tupian = tupian;
            this._rukusj = rukusj;
            this._dz = dz;
            this._list = list;
        }
    }

    /// <summary>
    /// 意见反馈属性
    /// </summary>
    /// <remarks>
    /// 2015-11-11 林建生
    /// </remarks>
    public class YJFKList
    {
        private string _wenti;
        private string _huifu;
        private string _hfsj;
        private string _rukusj;
        private object _list;
        public string wenti
        {
            get { return _wenti; }
        }
        public string huifu
        {
            get { return _huifu; }
        }
        public string hfsj
        {
            get { return _hfsj; }
        }
        public string rukusj
        {
            get { return _rukusj; }
        }
        public object list
        {
            get { return _list; }
        }

        public YJFKList(string wenti, string huifu, string hfsj, string rukusj, object list)
        {
            this._wenti = wenti;
            this._huifu = huifu;
            this._hfsj = hfsj;
            this._rukusj = rukusj;
            this._list = list;
        }
    }

    /// <summary>
    /// 微信NameValue
    /// </summary>
    /// <remarks>
    /// 2015-11-11 林建生
    /// </remarks>
    public class NameValue
    {
        private string _name;
        private int _value;
        public string name
        {
            get { return _name; }
        }
        public int value
        {
            get { return _value; }
        }
        public NameValue(string name, int value)
        {
            this._name = name;
            this._value = value;
        }
    }
    #endregion
}
