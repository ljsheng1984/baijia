using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using LJSheng.Data.EF;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LJSheng.Data;

namespace LJSheng.API
{
    /// <summary>
    /// API实现类
    /// </summary>
    public static class toapi
    {
        /// <summary>
        /// Api异常
        /// </summary>
        private class ApiException : Exception
        {
            public ApiResult ApiResult { get; set; }
            public ApiException(ApiResult apiResult)
            {
                this.ApiResult = apiResult;
            }
        }

        /// <summary>
        /// 调用函数,反射接口
        /// </summary>
        /// <param name="Method">方法名称</param>
        /// <param name="param">参数列表</param>
        /// <returns></returns>
        public static ApiResult InvokeFunction(string Method, object[] param)
        {
            object data = (typeof(goapi).InvokeMember(Method, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static | BindingFlags.OptionalParamBinding, null, null, param));
            if (data is ApiResult)
                return data as ApiResult;
            else
                return new ApiResult(ApiResultCodeEnum.Success, "ok", data);
        }

        /// <summary>
        /// 手机端调用函数
        /// </summary>
        /// <param name="Method">方法名称</param>
        /// <param name="ip">用户请求的IP地址</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static string ProcessRequest(string Method,string ip, string param)
        {
            //接口运行时间统计
            HttpContext.Current.Session["StartTime"] = DateTime.Now.ToString();
            Stopwatch stopwatch = new Stopwatch();
            HttpContext.Current.Session["Stopwatch"] = stopwatch;
            HttpContext.Current.Session["StopwatchTimes"] = new List<long>();
            stopwatch.Start();
            EFDB db = new EFDB();
            try
            {
                #region 参数判断
                //判断调用的方法是否为空
                if (String.IsNullOrEmpty(Method))
                {
                    throw (new ApiException(new ApiResult(ApiResultCodeEnum.ParameterError, "方法名不能为空",Method)));
                }

                //判断调用的方法是否存在
                MethodInfo methodInfo = typeof(goapi).GetMethod(Method);
                if (methodInfo==null)
                {
                    throw (new ApiException(new ApiResult(ApiResultCodeEnum.ParameterError, "方法名不存在", Method)));
                }
                //判断param是否为空
                if (String.IsNullOrEmpty(param))
                {
                    throw (new ApiException(new ApiResult(ApiResultCodeEnum.ParameterError, "参数不能为空", param)));
                }
                #endregion
                try
                {
                    #region 参数解析
                    JObject paramJson = null;
                    try
                    {
                        paramJson = JsonConvert.DeserializeObject(param) as JObject;
                    }
                    catch
                    {
                        throw (new ApiException(new ApiResult(ApiResultCodeEnum.ParameterError, "param解析失败")));
                    }
                    #endregion

                    #region 获取必须的参数
                    try
                    {
                        HttpContext.Current.Session["ip"] = ip;
                        HttpContext.Current.Session["sjxt"] = paramJson["sjxt"].ToString();
                        HttpContext.Current.Session["bbh"] = paramJson["bbh"].ToString();
                        HttpContext.Current.Session["sjxh"] = paramJson["sjxh"].ToString();
                        try
                        {
                            HttpContext.Current.Session["gid"] = paramJson["gid"].ToString();
                        }
                        catch
                        {
                            HttpContext.Current.Session["gid"] = null;
                        }
                    }
                    catch
                    {
                        throw (new ApiException(new ApiResult(ApiResultCodeEnum.ParameterError, "获取不到必须的参数",param)));
                    }
                    #endregion

                    #region 添加参数列表
                    List<object> paramList = new List<object>();
                    foreach (var paramName in methodInfo.GetParameters())
                    {
                        if (paramJson[paramName.Name] == null)
                        {
                            if (!paramName.IsOptional)
                                throw (new ApiException(new ApiResult(ApiResultCodeEnum.ParameterError, "方法中有必须的参数为空", paramName.Name)));
                        }
                        else
                        {
                            if (paramJson[paramName.Name].ToString() == "")
                            {
                                if (!paramName.IsOptional)
                                {
                                    throw (new ApiException(new ApiResult(ApiResultCodeEnum.ParameterError, "方法中有必须的参数为空", paramName.Name)));
                                }
                                else {
                                    AddParam(paramJson, paramList, paramName);
                                }
                            }
                            else {
                                AddParam(paramJson, paramList, paramName);
                            }
                        }
                    }
                    #endregion
                    //调用函数
                    ApiResult apiResult = InvokeFunction(Method.ToLower(), paramList.ToArray());
                    //返回加密的程序
                    JsonSerializerSettings setting = new JsonSerializerSettings();
                    JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
                    {
                        //日期类型默认格式化处理
                        setting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                        setting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                        //空值不显示处理
                        //setting.NullValueHandling = NullValueHandling.Ignore;
                        return setting;
                    });
                    //LJSheng.Common.LogManager.WriteLog(Method, param);
                    return JsonConvert.SerializeObject(apiResult, Formatting.Indented, setting).Replace("null", "\"\"");
                }
                catch (ApiException e)
                {
                    Bug(e, Method, param, "des");
                    return JsonConvert.SerializeObject(e.ApiResult);
                }
                catch (Exception e)
                {
                    //异常统计
                    Bug(e, Method, param, "des");
                    //异常直接返回网络问题
                    return JsonConvert.SerializeObject(new ApiResult(ApiResultCodeEnum.Error, "你的网络有问题哦", "catch"));
                }
            }
            catch (ApiException e)
            {
                //异常统计
                Bug(e, Method, param, "des");
                return JsonConvert.SerializeObject(e.ApiResult);
            }
            finally
            {
                stopwatch.Stop();
                #region 接口访问日志
                if (Convert.ToBoolean(ConfigurationManager.AppSettings["apilog"]))
                {
                    appapi api = new appapi()
                    {
                        gid = Guid.NewGuid(),
                        rukusj = DateTime.Now,
                        Method = Method,
                        sjxt = ApiHelper.sjxt,
                        bbh = ApiHelper.bbh,
                        sjxh = ApiHelper.sjxh,
                        imei = null,
                        dizhi = ApiHelper.ip,
                        jingdu = null,
                        weidu = null,
                        haoshi = Convert.ToInt32(stopwatch.ElapsedMilliseconds)
                    };
                    db.appapi.Add(api);
                    db.SaveChanges();
                }
                HttpContext.Current.Session.RemoveAll();
                #endregion
            }
        }

        #region 添加参数到列表
        private static void AddParam(JObject paramJson, List<object> paramList, ParameterInfo paramName)
        {
            try
            {
                switch (paramName.ParameterType.Name)
                {
                    case "String":
                        paramList.Add(paramJson[paramName.Name].ToString());
                        break;
                    case "Byte":
                        paramList.Add(Convert.ToByte(paramJson[paramName.Name].ToString()));
                        break;
                    case "Int16":
                        paramList.Add(Convert.ToInt16(paramJson[paramName.Name].ToString()));
                        break;
                    case "Int32":
                        paramList.Add(Convert.ToInt32(paramJson[paramName.Name].ToString()));
                        break;
                    case "Int64":
                        paramList.Add(Convert.ToInt64(paramJson[paramName.Name].ToString()));
                        break;
                    case "Double":
                        paramList.Add(Convert.ToDouble(paramJson[paramName.Name].ToString()));
                        break;
                    case "Decimal":
                        paramList.Add(Convert.ToDecimal(paramJson[paramName.Name].ToString()));
                        break;
                    case "DateTime":
                        paramList.Add(Convert.ToDateTime(paramJson[paramName.Name].ToString()));
                        break;
                    case "Boolean":
                        paramList.Add(Convert.ToBoolean(paramJson[paramName.Name].ToString()));
                        break;
                    case "Guid":
                        paramList.Add(Guid.Parse(paramJson[paramName.Name].ToString()));
                        break;
                    default:
                        throw (new ApiException(new ApiResult("找不到指定的参数类型转换")));
                }
            }
            catch
            {
                throw (new ApiException(new ApiResult(ApiResultCodeEnum.ParameterError, "参数格式错误")));
            }
        }
        #endregion

        #region 异常统计
        private static void Bug(Exception e,string Method,string sourceParam,string DESKey)
        {
            EFDB db = new EFDB();
            string bugmsg = e.ToString();
            if (e.InnerException!= null)
            {
                bugmsg = e.InnerException.Message;
            }
            apibug bug = new apibug()
            {
                gid = Guid.NewGuid(),
                rukusj = DateTime.Now,
                Method = Method,
                mcheng = e.GetType().Name + "<hr />" + e.Message,
                xiaoxi = bugmsg,
                duizhai = e.Source + "<hr />" + e.StackTrace,
                canshu = sourceParam,
                deskey = DESKey
            };
            db.apibug.Add(bug);
            db.SaveChanges();
        }
        #endregion
    }
}
