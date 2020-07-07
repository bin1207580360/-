using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ZBH_Model.Payments
{
    public class WxPayApi
    {
        /**
      * 根据当前系统时间加随机序列来生成订单号
       * @return 订单号
      */
        public static string GenerateOutTradeNo()
        {
            var ran = new Random();
            return string.Format("{0}{1}{2}", WxPayConfig.GetConfig().GetMchID(), DateTime.Now.ToString("yyyyMMddHHmmss"), ran.Next(999));
        }
        /**
       * 
       * 申请退款
       * @param WxPayData inputObj 提交给申请退款API的参数
       * @param int timeOut 超时时间
       * @throws Exception
       * @return 成功时返回接口调用结果，其他抛异常
       */
        //public static WxPayData Refund(WxPayData inputObj, int timeOut = 6)
        //{
        //    string url = "https://api.mch.weixin.qq.com/secapi/pay/refund";
        //    //检测必填参数
        //    if (!inputObj.IsSet("out_trade_no") && !inputObj.IsSet("transaction_id"))
        //    {
        //        throw new Exception("退款申请接口中，out_trade_no、transaction_id至少填一个！");
        //    }
        //    else if (!inputObj.IsSet("out_refund_no"))
        //    {
        //        throw new Exception("退款申请接口中，缺少必填参数out_refund_no！");
        //    }
        //    else if (!inputObj.IsSet("total_fee"))
        //    {
        //        throw new Exception("退款申请接口中，缺少必填参数total_fee！");
        //    }
        //    else if (!inputObj.IsSet("refund_fee"))
        //    {
        //        throw new Exception("退款申请接口中，缺少必填参数refund_fee！");
        //    }
        //    else if (!inputObj.IsSet("op_user_id"))
        //    {
        //        throw new Exception("退款申请接口中，缺少必填参数op_user_id！");
        //    }

        //    inputObj.SetValue("appid", WxPayConfig.GetConfig().GetAppID());//公众账号ID
        //    inputObj.SetValue("mch_id", WxPayConfig.GetConfig().GetMchID());//商户号
        //    inputObj.SetValue("nonce_str", GenerateNonceStr());//随机字符串
        //    inputObj.SetValue("sign_type", WxPayData.SIGN_TYPE_MD5);//签名类型
        //    inputObj.SetValue("sign", inputObj.MakeSign());//签名

        //    string xml = inputObj.ToXml();
        //    var start = DateTime.Now;

        //    Log.Debug("WxPayApi", "Refund request : " + xml);
        //    string response = HttpService.Post(xml, url, true, timeOut);//调用HTTP通信接口提交数据到API
        //    Log.Debug("WxPayApi", "Refund response : " + response);

        //    var end = DateTime.Now;
        //    int timeCost = (int)((end - start).TotalMilliseconds);//获得接口耗时

        //    //将xml格式的结果转换为对象以返回
        //    WxPayData result = new WxPayData();
        //    result.FromXml(response);

        //    ReportCostTime(url, timeCost, result);//测速上报

        //    return result;
        //}







        /**
       * 
       * 统一下单
       * @param WxPaydata inputObj 提交给统一下单API的参数
       * @param int timeOut 超时时间
       * @throws Exception
       * @return 成功时返回，其他抛异常
       */
        public static WxPayData UnifiedOrder(WxPayData inputObj, int timeOut = 6)
        {
            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            //检测必填参数
            if (!inputObj.IsSet("out_trade_no"))
            {
                throw new Exception("缺少统一支付接口必填参数out_trade_no！");
            }
            else if (!inputObj.IsSet("body"))
            {
                throw new Exception("缺少统一支付接口必填参数body！");
            }
            else if (!inputObj.IsSet("total_fee"))
            {
                throw new Exception("缺少统一支付接口必填参数total_fee！");
            }
            else if (!inputObj.IsSet("trade_type"))
            {
                throw new Exception("缺少统一支付接口必填参数trade_type！");
            }

            //关联参数
            if (inputObj.GetValue("trade_type").ToString() == "JSAPI" && !inputObj.IsSet("openid"))
            {
                throw new Exception("统一支付接口中，缺少必填参数openid！trade_type为JSAPI时，openid为必填参数！");
            }
            if (inputObj.GetValue("trade_type").ToString() == "NATIVE" && !inputObj.IsSet("product_id"))
            {
                throw new Exception("统一支付接口中，缺少必填参数product_id！trade_type为JSAPI时，product_id为必填参数！");
            }

            //异步通知url未设置，则使用配置文件中的url
            if (!inputObj.IsSet("notify_url"))
            {
                inputObj.SetValue("notify_url", WxPayConfig.GetConfig().GetNotifyUrl());//异步通知url
            }
            int ran = 0;
            byte[] r = new byte[32];
            Random rand = new Random((int)(DateTime.Now.Ticks % 1000000));
            //生成8字节原始数据
            for (int i = 0; i < 32; i++)
                //while循环剔除非字母和数字的随机数
                do
                {
                    //数字范围是ASCII码中字母数字和一些符号
                    ran = rand.Next(48, 122);
                    r[i] = Convert.ToByte(ran);
                } while ((ran >= 58 && ran <= 64) || (ran >= 91 && ran <= 96));
            //转换成8位String类型               
            string randomID = Encoding.ASCII.GetString(r);
            randomID = randomID.ToUpper();
            inputObj.SetValue("appid", WxPayConfig.GetConfig().GetAppID());//公众账号ID
            inputObj.SetValue("mch_id", WxPayConfig.GetConfig().GetMchID());//商户号
            inputObj.SetValue("spbill_create_ip", WxPayConfig.GetConfig().GetIp());//终端ip	  	    
            inputObj.SetValue("nonce_str", randomID);//随机字符串
            inputObj.SetValue("sign_type", WxPayData.SIGN_TYPE_MD5);//签名类型

            //签名
            inputObj.SetValue("sign", inputObj.MakeSign());
            string xml = inputObj.ToXml();

            var start = DateTime.Now;

            //Log.Debug("WxPayApi", "UnfiedOrder request : " + xml);
            string response = HttpService.Post(xml, url, false, timeOut);
           // Log.Debug("WxPayApi", "UnfiedOrder response : " + response);

            var end = DateTime.Now;
            int timeCost = (int)((end - start).TotalMilliseconds);

            WxPayData result = new WxPayData();
            result.FromXml(response);

            //ReportCostTime(url, timeCost, result);//测速上报

            return result;
        }
        /**
	    * 
	    * 测速上报
	    * @param string interface_url 接口URL
	    * @param int timeCost 接口耗时
	    * @param WxPayData inputObj参数数组
	    */
        /**
        private static void ReportCostTime(string interface_url, int timeCost, WxPayData inputObj)
        {
            //如果不需要进行上报
            if (WxPayConfig.GetConfig().GetReportLevel() == 0)
            {
                return;
            }

            //如果仅失败上报
            if (WxPayConfig.GetConfig().GetReportLevel() == 1 && inputObj.IsSet("return_code") && inputObj.GetValue("return_code").ToString() == "SUCCESS" &&
             inputObj.IsSet("result_code") && inputObj.GetValue("result_code").ToString() == "SUCCESS")
            {
                return;
            }

            //上报逻辑
            WxPayData data = new WxPayData();
            data.SetValue("interface_url", interface_url);
            data.SetValue("execute_time_", timeCost);
            //返回状态码
            if (inputObj.IsSet("return_code"))
            {
                data.SetValue("return_code", inputObj.GetValue("return_code"));
            }
            //返回信息
            if (inputObj.IsSet("return_msg"))
            {
                data.SetValue("return_msg", inputObj.GetValue("return_msg"));
            }
            //业务结果
            if (inputObj.IsSet("result_code"))
            {
                data.SetValue("result_code", inputObj.GetValue("result_code"));
            }
            //错误代码
            if (inputObj.IsSet("err_code"))
            {
                data.SetValue("err_code", inputObj.GetValue("err_code"));
            }
            //错误代码描述
            if (inputObj.IsSet("err_code_des"))
            {
                data.SetValue("err_code_des", inputObj.GetValue("err_code_des"));
            }
            //商户订单号
            if (inputObj.IsSet("out_trade_no"))
            {
                data.SetValue("out_trade_no", inputObj.GetValue("out_trade_no"));
            }
            //设备号
            if (inputObj.IsSet("device_info"))
            {
                data.SetValue("device_info", inputObj.GetValue("device_info"));
            }

            try
            {
                Report(data);
            }
            catch (Exception ex)
            {
                //不做任何处理
            }
        }*/
        /**
       * 
       * 测速上报接口实现
       * @param WxPayData inputObj 提交给测速上报接口的参数
       * @param int timeOut 测速上报接口超时时间
       * @throws Exception
       * @return 成功时返回测速上报接口返回的结果，其他抛异常
       */
        /*
         public static WxPayData Report(WxPayData inputObj, int timeOut = 1)
         {
             string url = "https://api.mch.weixin.qq.com/payitil/report";
             //检测必填参数
             if (!inputObj.IsSet("interface_url"))
             {
                 throw new Exception("接口URL，缺少必填参数interface_url！");
             }
             if (!inputObj.IsSet("return_code"))
             {
                 throw new Exception("返回状态码，缺少必填参数return_code！");
             }
             if (!inputObj.IsSet("result_code"))
             {
                 throw new Exception("业务结果，缺少必填参数result_code！");
             }
             if (!inputObj.IsSet("user_ip"))
             {
                 throw new Exception("访问接口IP，缺少必填参数user_ip！");
             }
             if (!inputObj.IsSet("execute_time_"))
             {
                 throw new Exception("接口耗时，缺少必填参数execute_time_！");
             }

             inputObj.SetValue("appid", WxPayConfig.GetConfig().GetAppID());//公众账号ID
             inputObj.SetValue("mch_id", WxPayConfig.GetConfig().GetMchID());//商户号
             inputObj.SetValue("user_ip", WxPayConfig.GetConfig().GetIp());//终端ip
             inputObj.SetValue("time", DateTime.Now.ToString("yyyyMMddHHmmss"));//商户上报时间	 
             inputObj.SetValue("nonce_str", GenerateNonceStr());//随机字符串
             inputObj.SetValue("sign_type", WxPayData.SIGN_TYPE_HMAC_SHA256);//签名类型
             inputObj.SetValue("sign", inputObj.MakeSign());//签名
             string xml = inputObj.ToXml();

             Log.Info("WxPayApi", "Report request : " + xml);

             string response = HttpService.Post(xml, url, false, timeOut);

             Log.Info("WxPayApi", "Report response : " + response);

             WxPayData result = new WxPayData();
             result.FromXml(response);
             return result;
         }*/
        /**
       * 生成随机串，随机串包含字母或数字
       * @return 随机串
       */
        public static string GenerateNonceStr()
        {
            int ran = 0;
            byte[] r = new byte[32];
            Random rand = new Random((int)(DateTime.Now.Ticks % 1000000));
            //生成8字节原始数据
            for (int i = 0; i < 32; i++)
                //while循环剔除非字母和数字的随机数
                do
                {
                    //数字范围是ASCII码中字母数字和一些符号
                    ran = rand.Next(48, 122);
                    r[i] = Convert.ToByte(ran);
                } while ((ran >= 58 && ran <= 64) || (ran >= 91 && ran <= 96));
            //转换成8位String类型               
            string nonceStr = Encoding.ASCII.GetString(r);
            nonceStr = nonceStr.ToUpper();

            //RandomGenerator randomGenerator = new RandomGenerator();
            //randomGenerator.GetRandomUInt().ToString();
            return nonceStr;
        }
    }
}