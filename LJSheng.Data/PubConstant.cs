namespace LJSheng.Data
{
    public class PubConstant
    {        
        /// <summary>
        /// ��ȡ�����ַ���
        /// </summary>
        public static string ConnectionString
        {           
            get 
            {
                //string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString;
                string _connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["MSSQL"].ConnectionString;
                return _connectionString; 
            }
        }
    }
}