namespace LJSheng.SMS
{
    public class SMS
    {
        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="toPhone">发送短信手机号码，群发逗号区分</param>
        /// <param name="templatedId">短信模板id，需通过审核240053</param>
        /// <param name="param">短信参数</param>
        /// <returns>返回调用结果</returns>
        /// <para name="result">200 是成功其他失败</para>
        /// <para name="data">结果提示</para>
        /// <remarks>
        /// 2017-08-18 林建生
        /// </remarks>
        public string SendSMS(string toPhone, string templatedId, string param)
        {
            string serverIp = "api.ucpaas.com";
            string serverPort = "443";
            string account = "f2ee0b5d24916ddeab31695c1edfb8bb";    //用户sid
            string token = "04ebd05bf3276bd7335915e0bb8b1657";      //用户sid对应的token
            string appId = "de2188bdf9a74bd286b3d685ea4ed9ae";      //对应的应用id，非测试应用需上线使用
            //string clientNum = "60000000000001";
            //string clientpwd = "";
            //string friendName = "";
            //string clientType = "0";
            //string charge = "0";
            //string phone = "";
            //string date = "day";
            //uint start = 0;
            //uint limit = 100;
            //string verifyCode = "1234";
            //string fromSerNum = "4000000000";
            //string toSerNum = "4000000000";
            //string maxallowtime = "60";

            UCSRestRequest api = new UCSRestRequest();
            api.init(serverIp, serverPort);
            api.setAccount(account, token);
            api.enabeLog(true);
            api.setAppId(appId);
            api.enabeLog(true);

            //发送短信
            return api.SendSMS(toPhone, templatedId, param);

            //查询主账号
            //api.QueryAccountInfo();

            //申请client账号
            //api.CreateClient(friendName, clientType, charge, phone);

            //查询账号信息(账号)
            //api.QueryClientNumber(clientNum);

            //查询账号信息(电话号码)
            //api.QueryClientMobile(phone);

            //查询账号列表
            //api.GetClient(start, limit);

            //删除一个账号
            //api.DropClient(clientNum);

            //查询应用话单
            //api.GetBillList(date);

            //查询账号话单
            //api.GetClientBillList(clientNum, date);

            //账号充值
            //api.ChargeClient(clientNum, clientType, charge);

            //回拨
            //api.CallBack(clientNum, toPhone, fromSerNum, toSerNum, maxallowtime);

            //语音验证码
            //api.VoiceCode(toPhone, "1234");
        }
    }
}