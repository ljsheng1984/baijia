using System.Web;
using System.Xml;
using System.IO;

namespace LJSheng.Common
{
    public class LXML
    {
        /// <summary>
        /// 读取,添加，修改xml文件
        /// </summary>
        /// <param name="Xmlpath">Xml路径</param>
        /// <param name="Node">新的子节点名称</param>
        /// <param name="Value">新节点对应的值</param>
        /// <param name="flag">1：读取，否则为 修改或者添加</param>
        /// <returns>1：修改添加成功，为空字符串表示修改添加成功，否则是读取成功</returns>
        public static string getXML(string Xmlpath, string Node, string Value, int flag)
        {
            try
            {
                string filepath = HttpContext.Current.Server.MapPath(Xmlpath);
                XmlDocument xmlDoc = new XmlDocument();
                if (!File.Exists(filepath))
                {
                    XmlDeclaration xn = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                    XmlElement root = xmlDoc.CreateElement("rss");
                    XmlElement root1 = xmlDoc.CreateElement("item");

                    root.AppendChild(root1);
                    xmlDoc.AppendChild(xn);
                    xmlDoc.AppendChild(root);
                    xmlDoc.Save(filepath);//本地路径名字
                }
                xmlDoc.Load(filepath);//你的xml文件
                string ReStr = string.Empty;
                XmlElement xmlObj = xmlDoc.DocumentElement;

                XmlNodeList xmlList = xmlDoc.SelectSingleNode(xmlObj.Name.ToString()).ChildNodes;

                foreach (XmlNode xmlNo in xmlList)
                {
                    if (xmlNo.NodeType != XmlNodeType.Comment)//判断是不是注释类型
                    {
                        XmlElement xe = (XmlElement)xmlNo;
                        {
                            if (xe.Name == xmlObj.FirstChild.Name)
                            {
                                XmlNodeList xmlNList = xmlObj.FirstChild.ChildNodes;

                                foreach (XmlNode xmld in xmlNList)
                                {
                                    XmlElement xe1 = (XmlElement)xmld;
                                    {
                                        if (xe1.Name == Node)
                                        {
                                            if (flag == 1)//读取值
                                            {
                                                if (xmld.InnerText != null && xmld.InnerText != "")
                                                {
                                                    ReStr = xmld.InnerText;
                                                }
                                            }
                                            else//修改值
                                            {
                                                xmld.InnerText = Value;//给节点赋值
                                                xmlDoc.Save(filepath);
                                                ReStr = Value.Trim();
                                            }
                                        }
                                    }
                                }
                                if (ReStr == string.Empty)// 添加节点
                                {
                                    XmlNode newNode;
                                    newNode = xmlDoc.CreateNode("element", Node, Value);//创建节点
                                    newNode.InnerText = Value;//给节点赋值
                                    xe.AppendChild(newNode);//把节点添加到doc
                                    xmlDoc.Save(filepath);
                                    ReStr = Value.Trim();
                                }
                            }
                        }
                    }
                }
                return ReStr;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
