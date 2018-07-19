using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace LJSheng.Data
{
    public static class EFOperation<T> where T : class
    {
        //实例化EF框架  
        static EFDB db = new EFDB();

        //添加  
        public static T AddEntities(T entity)
        {
            db.Entry<T>(entity).State = EntityState.Added;
            db.SaveChanges();
            return entity;
        }

        //修改  
        public static bool UpdateEntities(T entity)
        {
            db.Set<T>().Attach(entity);
            db.Entry<T>(entity).State = EntityState.Modified;
            return db.SaveChanges() > 0;
        }

        //删除  
        public static bool DeleteEntities(T entity)
        {
            db.Set<T>().Attach(entity);
            db.Entry<T>(entity).State = EntityState.Deleted;
            return db.SaveChanges() > 0;
        }

        //查询  
        public static IQueryable<T> LoadEntities(Func<T, bool> wherelambda)
        {
            return db.Set<T>().Where<T>(wherelambda).AsQueryable();
        }

        //分页  
        public static IQueryable<T> LoadPagerEntities<S>(int pageSize, int pageIndex, out int total,
            Func<T, bool> whereLambda, bool isAsc, Func<T, S> orderByLambda)
        {
            var tempData = db.Set<T>().Where<T>(whereLambda);

            total = tempData.Count();

            //排序获取当前页的数据  
            if (isAsc)
            {
                tempData = tempData.OrderBy<T, S>(orderByLambda).
                      Skip<T>(pageSize * (pageIndex - 1)).
                      Take<T>(pageSize).AsQueryable();
            }
            else
            {
                tempData = tempData.OrderByDescending<T, S>(orderByLambda).
                     Skip<T>(pageSize * (pageIndex - 1)).
                     Take<T>(pageSize).AsQueryable();
            }
            return tempData.AsQueryable();
        }

        #region EF分页查询
        /// <summary>
        /// EF lanbda 分页
        /// </summary>
        /// <param name="select">需要查询的字段</param>
        /// <param name="where">查询条件</param>
        /// <param name="OrderBy">排序</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示的条数</param>
        /// <param name="isAsc">升序还是降序</param>
        /// <param name="Total">查询总条数</param>
        /// <returns></returns>
        public static List<dynamic> getPageDate(Func<T, dynamic> select, Func<T, bool> where, Func<T, dynamic> OrderBy, int pageIndex, int pageSize, bool isAsc, out int Total)
        {
            EFDB db = new EFDB();
            Total = db.Set<T>().Where(where).Count();
            if (isAsc)
            {
                return (db.Set<T>().Where(where).OrderBy(OrderBy).Select(select).Skip((pageIndex - 1) * pageSize).Take(pageSize)).ToList();
            }
            else
            {
                return (db.Set<T>().Where(where).OrderByDescending(OrderBy).Select(select).Skip((pageIndex - 1) * pageSize).Take(pageSize)).ToList();
            }
        }
        #endregion
    }
}
