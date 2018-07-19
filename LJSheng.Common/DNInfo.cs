//获取IP
//Request.ServerVariables["LOCAL_ADDR"].ToString()
//获取域名
//Request.ServerVariables["SERVER_NAME"].ToString()
//获取语言
//Request.ServerVariables["HTTP_ACCEPT_LANGUAGE"].ToString()
//获取端口
//Request.ServerVariables["SERVER_PORT"].ToString()
//IIS版本
//Request.ServerVariables["SERVER_SOFTWARE"].ToString()
//CPU数量
//Request.ServerVariables["NUMBER_OF_PROCESSORS"].ToString()
//操作系统
//Request.ServerVariables["OS"].ToString()
//用户代理的信息
//Request.ServerVariables["HTTP_USER_AGENT"].ToString()
//支持文件类型
//Request.ServerVariables["HTTP_Accept"].ToString()
//获取/wenjian/wenjian.aspx不带参数

//Request.ServerVariables("Url") 
//返回服务器地址
//Request.ServerVariables("Path_Info") 
//客户端提供的路径信息
//Request.ServerVariables("Appl_Physical_Path") 
//与应用程序元数据库路径相应的物理路径
//Request.ServerVariables("Path_Translated") 
//通过由虚拟至物理的映射后得到的路径
//Request.ServerVariables("Script_Name") 
//执行脚本的名称
//Request.ServerVariables("Query_String") 
//查询字符串內容
//Request.ServerVariables("Http_Referer") 
//请求的字符串內容
//Request.ServerVariables("Server_Port") 
//接受请求的服务器端口号
//Request.ServerVariables("Remote_Addr") 
//发出请求的远程主机的IP地址
//Request.ServerVariables("Remote_Host") 
//发出请求的远程主机名称
//Request.ServerVariables("Local_Addr") 
//返回接受请求的服务器地址
//Request.ServerVariables("Http_Host") 
//返回服务器地址
//Request.ServerVariables("Server_Name") 
//服务器的主机名、DNS地址或IP地址
//Request.ServerVariables("Request_Method") 
//提出请求的方法比如GET、HEAD、POST等等
//Request.ServerVariables("Server_Port_Secure")
//如果接受请求的服务器端口为安全端口时，则为1，否则为0
//Request.ServerVariables("Server_Protocol")
//服务器使用的协议的名称和版本
//Request.ServerVariables("Server_Software")
//应答请求并运行网关的服务器软件的名称和版本
//Request.ServerVariables("All_Http")
//客户端发送的所有HTTP标头，前缀HTTP_
//Request.ServerVariables("All_Raw")
//客户端发送的所有HTTP标头,其结果和客户端发送时一样，没有前缀HTTP_
//Request.ServerVariables("Appl_MD_Path")
//应用程序的元数据库路径
//Request.ServerVariables("Content_Length")
//客户端发出內容的长度
//Request.ServerVariables("Https")
//如果请求穿过安全通道（SSL），则返回ON如果请求来自非安全通道，则返回OFF
//Request.ServerVariables("Instance_ID")
//IIS实例的ID号
//Request.ServerVariables("Instance_Meta_Path")
//响应请求的IIS实例的元数据库路径
//Request.ServerVariables("Http_Accept_Encoding")
//返回內容如：gzip,deflate
//Request.ServerVariables("Http_Accept_Language")
//返回內容如：en-us
//Request.ServerVariables("Http_Connection")
//返回內容：Keep-Alive
//Request.ServerVariables("Http_Cookie")
//返回內容如：nVisiT%2DYum=125;ASPSESSIONIDCARTQTRA=FDOBFFABJGOECBBKHKGPFIJI;ASPSESSIONIDCAQQTSRB=LKJJPLABABILLPCOGJGAMKAM;ASPSESSIONIDACRRSSRA=DKHHHFBBJOJCCONPPHLKGHPB
//Request.ServerVariables("Http_User_Agent")
//返回內容：Mozilla/4.0(compatible;MSIE6.0;WindowsNT5.1;SV1)
//Request.ServerVariables("Https_Keysize")
//安全套接字层连接关键字的位数，如128
//Request.ServerVariables("Https_Secretkeysize")
//服务器验证私人关键字的位数如1024
//Request.ServerVariables("Https_Server_Issuer")
//服务器证书的发行者字段
//Request.ServerVariables("Https_Server_Subject")
//服务器证书的主题字段
//Request.ServerVariables("Auth_Password")
//当使用基本验证模式时，客户在密码对话框中输入的密码
//Request.ServerVariables("Auth_Type")
//是用户访问受保护的脚本时，服务器用於检验用户的验证方法
//Request.ServerVariables("Auth_User")
//代证的用户名
//Request.ServerVariables("Cert_Cookie")
//唯一的客户证书ID号
//Request.ServerVariables("Cert_Flag")
//客户证书标誌，如有客户端证书，则bit0为0如果客户端证书验证无效，bit1被设置为1
//Request.ServerVariables("Cert_Issuer")
//用户证书中的发行者字段
//Request.ServerVariables("Cert_Keysize")
//安全套接字层连接关键字的位数，如128
//Request.ServerVariables("Cert_Secretkeysize")
//服务器验证私人关键字的位数如1024
//Request.ServerVariables("Cert_Serialnumber")
//客户证书的序列号字段
//Request.ServerVariables("Cert_Server_Issuer")
//服务器证书的发行者字段
//Request.ServerVariables("Cert_Server_Subject")
//服务器证书的主题字段
//Request.ServerVariables("Cert_Subject")
//客户端证书的主题字段
//Request.ServerVariables("Content_Type")
//客户发送的form內容或HTTPPUT的数据类型

using System;
using System.Runtime.InteropServices;

namespace LJSheng.Common
{
    public class DNInfo
    {
        /// <summary>
        /// 穿过代理服务器获取真实IP
        /// </summary>
        /// <returns></returns>
        public string GetClientIP()
        {
            string user_IP = null;
            if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
            {
                user_IP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            else
            {
                user_IP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
            }
            return user_IP;
        }

        [DllImport("Iphlpapi.dll")]
        private static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 length);
        [DllImport("Ws2_32.dll")]
        private static extern Int32 inet_addr(string ip);

        public static string GetMAC()
        {
            // 在此处放置用户代码以初始化页面
            string mac_dest = "";
            try
            {
                string userip = System.Web.HttpContext.Current.Request.UserHostAddress;
                string strClientIP = System.Web.HttpContext.Current.Request.UserHostAddress.ToString().Trim();
                Int32 ldest = inet_addr(strClientIP); //目的地的ip 
                Int32 lhost = inet_addr("");   //本地服务器的ip 
                Int64 macinfo = new Int64();
                Int32 len = 6;
                int res = SendARP(ldest, 0, ref macinfo, ref len);
                string mac_src = macinfo.ToString("X");
                if (mac_src == "0")
                {
                    if (userip == "127.0.0.1")
                    {
                        mac_dest = "Localhost";
                    }
                    else
                    {
                        mac_dest = userip;
                    }
                    return mac_dest;
                }

                while (mac_src.Length < 12)
                {
                    mac_src = mac_src.Insert(0, "0");
                }


                for (int i = 0; i < 11; i++)
                {
                    if (0 == (i % 2))
                    {
                        if (i == 10)
                        {
                            mac_dest = mac_dest.Insert(0, mac_src.Substring(i, 2));
                        }
                        else
                        {
                            mac_dest = "-" + mac_dest.Insert(0, mac_src.Substring(i, 2));
                        }
                    }
                }

                return mac_dest;
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }
    }
}
