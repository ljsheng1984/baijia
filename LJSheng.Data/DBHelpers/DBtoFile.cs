using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Configuration;

namespace LJSheng.Data
{
    public static class DBtoFile
    {
        public static string Getbiao(string tablename)
        {
            try
            {
                //XDocument xmldoc = XDocument.Load(Server.MapPath("~/bin/LJSheng.API.XML"));
                XDocument xmldoc = XDocument.Load(ConfigurationManager.AppSettings["efxml"]);
                var tlist = (from table in xmldoc.Descendants("member")
                             where table.Attribute("name").Value == tablename
                             select new
                             {
                                 summary = table.Element("summary").Value.Replace("\n", "")
                             }).FirstOrDefault();
                return tlist.summary;
            }
            catch
            { return ""; }
        }

        #region 生成html格式
        public static string DBToHtml(string DbName)
        {
            try
            {
                StringBuilder htmlBody = new StringBuilder();
                //htmlBody.Append("<div style=\"font-size:14px;\">数据库:" + DbName + " 数据字典</div>");
                DBHelper dbhelper = new DBHelper();
                List<string> TableList = dbhelper.GetTables(DbName);

                #region 循环每个表
                string tablename;//表名
                for (int i = 0; i < TableList.Count; i++)
                {
                    #region 循环每一个列，产生一行数据
                    tablename = TableList[i].ToString();
                    List<ColumnInfo> collist = dbhelper.GetColumnInfoList(DbName, tablename);
                    int rc = collist.Count;
                    if ((collist != null) && (collist.Count > 0))
                    {
                        htmlBody.Append("<div id=\"" + tablename + "\" style=\"font-size:20px;padding-top:15px;text-align:center;\">" + Getbiao("T:LJSheng.Data." + tablename) + ":" + tablename + "</div>");
                        htmlBody.Append("<div><table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"90%\">");
                        htmlBody.Append("<tr><td bgcolor=\"#F5F9FF\">");
                        htmlBody.Append("<table cellspacing=\"0\" cellpadding=\"5\" border=\"1\" width=\"100%\" bordercolorlight=\"#4F7FC9\" bordercolordark=\"#D3D8E0\">");
                        htmlBody.Append("<tr bgcolor=\"#E3EFFF\">");
                        htmlBody.Append("<td>序号</td>");
                        htmlBody.Append("<td>列名</td>");
                        htmlBody.Append("<td>数据类型</td>");
                        htmlBody.Append("<td>长度</td>");
                        htmlBody.Append("<td>小数位</td>");
                        htmlBody.Append("<td>标识</td>");
                        htmlBody.Append("<td>主键</td>");
                        htmlBody.Append("<td>允许空</td>");
                        htmlBody.Append("<td>默认值</td>");
                        htmlBody.Append("<td>说明</td>");
                        htmlBody.Append("</tr>");

                        for (int r = 0; r < rc; r++)
                        {
                            ColumnInfo col = (ColumnInfo)collist[r];
                            string order = col.Colorder;
                            string colum = col.ColumnName;
                            string typename = col.TypeName;
                            string length = col.Length == "" ? "&nbsp;" : col.Length;
                            string scale = col.Scale == "" ? "&nbsp;" : col.Scale;
                            string IsIdentity = col.IsIdentity.ToString().ToLower() == "true" ? "是" : "&nbsp;";
                            string PK = col.IsPK.ToString().ToLower() == "true" ? "是" : "&nbsp;";
                            string isnull = col.cisNull.ToString().ToLower() == "true" ? "是" : "否";
                            string defaultstr = col.DefaultVal.ToString().Trim() == "" ? "&nbsp;" : col.DefaultVal.ToString();
                            string description = col.DeText.ToString().Trim() == "" ? "&nbsp;" : col.DeText.ToString();
                            if (length.Trim() == "-1")
                            {
                                length = "MAX";
                            }

                            htmlBody.Append("<tr>");
                            htmlBody.Append("<td>" + order + "</td>");
                            htmlBody.Append("<td>" + colum + "</td>");
                            htmlBody.Append("<td>" + typename + "</td>");
                            htmlBody.Append("<td>" + length + "</td>");
                            htmlBody.Append("<td>" + scale + "</td>");
                            htmlBody.Append("<td>" + IsIdentity + "</td>");
                            htmlBody.Append("<td>" + PK + "</td>");
                            htmlBody.Append("<td>" + isnull + "</td>");
                            htmlBody.Append("<td>" + defaultstr + "</td>");
                            htmlBody.Append("<td>" + Getbiao("P:LJSheng.Data." + tablename +"."+ colum) + "</td>");
                            htmlBody.Append("</tr>");

                        }

                    }
                    htmlBody.Append("</table>");
                    htmlBody.Append("</td>");
                    htmlBody.Append("</tr>");
                    htmlBody.Append("</table>");
                    htmlBody.Append("</div>");

                    #endregion
                }
                return htmlBody.ToString();
                #endregion
            }
            catch (System.Exception ex)
            {
                return "操作异常:" + ex.ToString();
            }
        }
        #endregion
    }
}
