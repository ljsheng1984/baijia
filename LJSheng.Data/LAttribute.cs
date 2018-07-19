namespace LJSheng.Data
{
    public class KeyValue
    {
        private string _key;
        private int _value;
        public string key
        {
            get { return _key; }
        }
        public int value
        {
            get { return _value; }
        }
        public KeyValue(string key, int value)
        {
            this._key = key;
            this._value = value;
        }
    }
}
