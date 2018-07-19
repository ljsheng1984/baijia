/******************************************************  
Copyright (c) 2005-2011 林木木办公室
Depiction：表通用操作
Author：林建生
Create Date：2011-1-11
*******************************************************/
using System;
using System.Data;
using System.Data.SqlClient;

namespace LJSheng.Data
{
    public class Tables
    {
        #region  成员方法

        /// <summary>
        /// 表通用数据删除
        /// </summary>
        /// <param name="TableName">要操作的表名 </param>
        /// <param name="IDName">要操作的表的字段名</param>
        /// <param name="IDValues">要操作的表的字段的值</param>
        /// <returns>返回删除的对应条数  </returns>
        public static int Delete(string TableName, string IDName, string IDValues)
        {
            int rowsAffected;
            SqlParameter[] parameters = {
                    new SqlParameter("@TableName", SqlDbType.VarChar,20),
                    new SqlParameter("@IDName", SqlDbType.VarChar,20),
					new SqlParameter("@IDValues", SqlDbType.VarChar,200)};
            parameters[0].Value = TableName;
            parameters[1].Value = IDName;
            parameters[2].Value = IDValues;

            DbHelperSQL.RunProcedure("Table_Delete", parameters, out rowsAffected);
            return rowsAffected;
        }

        /// <summary>
        /// 查询条目数
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="Where">条件</param>
        /// <returns>总条数</returns>
        public static int GetCount(string TableName, string Where)
        {
            SqlParameter[] parameters = {
                    new SqlParameter("@TableName", SqlDbType.VarChar,50),
                    new SqlParameter("@Where", SqlDbType.VarChar,500)};
            parameters[0].Value = TableName;
            parameters[1].Value = Where;

            //return Int32.Parse(DbHelperSQL.RunProcedure("Table_Count", parameters, TableName).Tables[TableName].Rows[0]["count"].ToString());
            //不调用存储过程直接执行查询语句
            string TableWhere = parameters[0].SqlValue.ToString();
            if (String.IsNullOrEmpty(parameters[1].SqlValue.ToString()))
            {
                TableWhere = parameters[0].SqlValue.ToString() + " where " + parameters[1].SqlValue.ToString();
            }
            return Int32.Parse(DbHelperSQL.Query("select count(*) as count from " + TableWhere).Tables[0].Rows[0][0].ToString());
        }

        /// <summary>
        /// 分页查询返回DATASET
        /// </summary>
        /// <param name="TableName">需要查询的表名</param>
        /// <param name="Fields">需要返回的列(所以列*)</param>
        /// <param name="PageSize">页容量(每页显示页的数量)</param>
        /// <param name="PageIndex">当前页</param>
        /// <param name="Where">查询条件(Id=Id)</param>
        /// <param name="OrderBy">排序(ID desc)</param>
        /// <returns></returns>
        public static DataSet Table_List(string TableName, string OrderBy, string Fields, int PageSize, int PageIndex, string Where)
        {
            SqlParameter[] parameters = {
                    new SqlParameter("@TableName", SqlDbType.VarChar,20), 
                    new SqlParameter("@Fields", SqlDbType.VarChar,500), 
                    new SqlParameter("@Where", SqlDbType.VarChar,500),
                    new SqlParameter("@OrderBy", SqlDbType.VarChar,200),
                    new SqlParameter("@PageSize", SqlDbType.Int),
                    new SqlParameter("@PageIndex", SqlDbType.Int)};
            parameters[0].Value = TableName;
            parameters[1].Value = Fields;
            parameters[2].Value = Common.LCommon.ClearSql(Where);
            parameters[3].Value = OrderBy;
            parameters[4].Value = PageSize;
            parameters[5].Value = PageIndex;

            //return DbHelperSQL.RunProcedure("Table_List", parameters, "ds");
            //不调用存储过程直接执行查询语句
            string Row = "";
            string TableWhere = parameters[0].SqlValue.ToString();
            int StartNumber;
            int EndNumber;
            if (!String.IsNullOrEmpty(parameters[2].SqlValue.ToString()))
            {
                TableWhere = parameters[0].SqlValue.ToString() + " where " + parameters[2].SqlValue.ToString();
            }

            if (parameters[4].SqlValue.ToString() != "0")
            {
                StartNumber = (Int32.Parse(parameters[5].SqlValue.ToString()) - 1) * Int32.Parse(parameters[4].SqlValue.ToString()) + 1;
                EndNumber = (Int32.Parse(parameters[5].SqlValue.ToString()) - 1) * Int32.Parse(parameters[4].SqlValue.ToString()) + Int32.Parse(parameters[4].SqlValue.ToString());
                Row = " where Row between " + StartNumber.ToString() + " and " + EndNumber.ToString();
            }
            return DbHelperSQL.Query("with temptbl AS (SELECT ROW_NUMBER() OVER (ORDER BY " + parameters[3].SqlValue.ToString() + ")AS Row, " + parameters[1].SqlValue.ToString() + " FROM " + TableWhere + ") SELECT * FROM temptbl" + Row);
        }

        #endregion  成员方法
    }
}