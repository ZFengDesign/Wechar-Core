using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using WecharCore.Properties;

namespace WecharCore
{

    public class ProModel
    {     
        /// <summary>
        /// 向微信服务器请求网页认证，返回用户基本信息
        /// </summary>
        /// <param name="code">请求地址</param>
        /// <returns>返回用户状态</returns>
        public UserInfor getBaseUser(string code)
        {
            //并发连接数量
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;
            //通过Appid获取网页授权
            UserInfor users = new UserInfor();
            //System.GC.Collect();

            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create("https://api.weixin.qq.com/sns/oauth2/access_token?appid=" + Settings.Default.AppID + "&secret=" + Settings.Default.AppSecret + "&code=" + code + "&grant_type=authorization_code");
            hr.Method = "GET";
            hr.Timeout = 30000;
            //开始获取用户信息
            try
            {
                WebResponse wr = hr.GetResponse();
                Stream s = wr.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                //微信服务器返回的用户授权
                string backContent = sr.ReadLine();
                sr.Dispose();
                s.Dispose();
                wr.Close();

                //分析用户信息
                JavaScriptSerializer json = new JavaScriptSerializer();
                users = (UserInfor)json.Deserialize(backContent, typeof(UserInfor));
            }
            catch
            {
                users.state = "timeout";
            }
            finally
            {
                hr.Abort();
                hr = null;
            }

            return users;
        }

        /// <summary>
        /// 向微信服务器请求获取AccessToken
        /// </summary>
        /// <returns>返回AccessToken</returns>
        public string getAccessToken()
        {
            //并发连接数量
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;
            //通过Appid获取网页授权
            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + Settings.Default.AppID + "&secret=" + Settings.Default.AppSecret);
            string backContent = "";
            string backMSG = "";
            try
            {
                WebResponse wr = hr.GetResponse();

                Stream s = wr.GetResponseStream();

                StreamReader sr = new StreamReader(s);
                backContent = sr.ReadLine();
                sr.Dispose();
                s.Dispose();
                wr.Close();
                
                //分析JSON信息
                JavaScriptSerializer json = new JavaScriptSerializer();

                appToken Token = (appToken)json.Deserialize(backContent, typeof(appToken));
                backMSG = Token.access_token;

                if (Token.errcode > 0)
                {
                    backMSG = Token.errmsg;
                }

            }
            catch
            {

            }
            finally
            {
                hr.Abort();
                hr = null;
            }
            return backMSG;
        }

        /// <summary>
        /// 向微信服务器请求获取客服列表
        /// </summary>
        /// <returns>返回客服列表</returns>
        public string getKF(string accessToken)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;

            //通过Appid获取网页授权
            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create("https://api.weixin.qq.com/cgi-bin/customservice/getkflist?access_token=" + accessToken);

            WebResponse wr = hr.GetResponse();

            Stream s = wr.GetResponseStream();

            StreamReader sr = new StreamReader(s);
            string backContent = sr.ReadLine();
            sr.Dispose();
            s.Dispose();
            wr.Close();
            
            return backContent;
        }
        
        /// <summary>
        /// 向微信服务器请求获取jsapi_ticket
        /// </summary>
        /// <returns>返回AccessToken</returns>
        public string getJSApiTicket(string AccessToken)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;

            //通过Appid获取网页授权
            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token=" + AccessToken + "&type=jsapi");

            WebResponse wr = hr.GetResponse();

            Stream s = wr.GetResponseStream();

            StreamReader sr = new StreamReader(s);
            string backContent = sr.ReadLine();
            sr.Dispose();
            s.Dispose();

            //分析JSON信息
            JavaScriptSerializer json = new JavaScriptSerializer();

            JSAPI Token = (JSAPI)json.Deserialize(backContent, typeof(JSAPI));
            string backMSG = Token.ticket;

            if (Token.errcode > 0)
            {
                backMSG = Token.errmsg;
            }
            return backMSG;
        }
        
        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="sendstring">要发送的模板JSON数据</param>
        /// <param name="accessToken">AccessToken</param>
        /// <returns></returns>
        public void sendTempletsMSG(string sendstring, string accessToken)
        {
            //并发连接数量
            System.Net.ServicePointManager.DefaultConnectionLimit = 150;
            //通过accessToken获取消息模板的内部ID
            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token=" + accessToken);
            hr.Method = "POST";
            hr.KeepAlive = true;

            try
            {
                //建立通讯数据流，然后写入POST的数据
                Stream w = hr.GetRequestStream();
                byte[] sendByte = ASCIIEncoding.UTF8.GetBytes(sendstring);
                w.Write(sendByte, 0, sendByte.Length);

                hr.GetResponse();

                w.Close();
                w.Dispose();
            }
            catch
            {

            }
            finally
            {
                hr.Abort();
                hr = null;

            }
           

        }

        /// <summary>
        /// 发送客服消息
        /// </summary>
        /// <param name="msg">消息格式</param>
        /// <param name="accessToken">ACCESSTOKEN</param>
        /// <returns>处理结果</returns>
        public string sendKFMsg(string msg, string accessToken)
        {
            string result = "err";

            //并发连接数量
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;

            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create("https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token=" + accessToken);
            
            hr.Method = "POST";
            hr.ContentType = "application/json; charset=utf-8";

            try
            {
                //向服务器发送请求
                Stream w = hr.GetRequestStream();
                //msg = System.Web.HttpUtility.UrlEncode(msg);
                byte[] msgBit =Encoding.Default.GetBytes(msg);
                
                w.Write(msgBit, 0, msg.Length);
                w.Close();

                //获取服务器返回数据
                WebResponse wr = hr.GetResponse();
                Stream s = wr.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                string backContent = sr.ReadLine();

                //关闭所有连接
                sr.Dispose();
                s.Dispose();
                wr.Close();

                result = backContent;
            }
            catch
            {
            }
            finally
            {
                hr.Abort();
                hr = null;
            }
            return result;
        }


        /// <summary>
        /// 建立自定义个性菜单
        /// </summary>
        /// <param name="menu">菜单JSON</param>
        /// <param name="accessToken">ACCESSTOKEN</param>
        /// <returns>处理结果</returns>
        public string setMenuSpecial(string menu, string accessToken)
        {
            string result = "err";

            //并发连接数量
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;

            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create("https://api.weixin.qq.com/cgi-bin/menu/addconditional?access_token=" + accessToken);

            hr.Method = "POST";
            hr.ContentType = "application/json;encoding=utf-8";

            try
            {
                //向服务器发送请求
                Stream w = hr.GetRequestStream();
                //msg = System.Web.HttpUtility.UrlEncode(msg);
                byte[] msgBit = Encoding.UTF8.GetBytes(menu);

                w.Write(msgBit, 0, menu.Length);
                w.Close();

                //获取服务器返回数据
                WebResponse wr = hr.GetResponse();
                Stream s = wr.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                string backContent = sr.ReadLine();

                //关闭所有连接
                sr.Dispose();
                s.Dispose();
                wr.Close();

                result = backContent;
            }
            catch
            {
            }
            finally
            {
                hr.Abort();
                hr = null;
            }
            return result;
        }
        
        /// <summary>
        /// 批量为用户移动后台分组（例如：用户业务员分组菜单的使用）
        /// </summary>
        /// <param name="userList">用户列表</param>
        /// <param name="accessToken">ACCESSTOKEN</param>
        /// <returns>处理结果</returns>
        public string setUserToGroup(string userJosn,string accessToken)
        {

            string result = "err";

            //并发连接数量
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;

            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create("https://api.weixin.qq.com/cgi-bin/tags/members/batchtagging?access_token=" + accessToken);

            hr.Method = "POST";
            hr.ContentType = "application/json;encoding=utf-8";

            try
            {
                //向服务器发送请求
                Stream w = hr.GetRequestStream();
                //msg = System.Web.HttpUtility.UrlEncode(msg);
                byte[] msgBit = Encoding.UTF8.GetBytes(userJosn);

                w.Write(msgBit, 0, userJosn.Length);
                w.Close();

                //获取服务器返回数据
                WebResponse wr = hr.GetResponse();
                Stream s = wr.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                string backContent = sr.ReadLine();

                //关闭所有连接
                sr.Dispose();
                s.Dispose();
                wr.Close();

                result = backContent;
            }
            catch
            {
            }
            finally
            {
                hr.Abort();
                hr = null;
            }
            return result;
        }


        public string getUserGroup(string accessToken)
        {

            string result = "err";

            //并发连接数量
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;

            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create("https://api.weixin.qq.com/cgi-bin/tags/get?access_token=" + accessToken);

            hr.Method = "POST";
            hr.ContentType = "application/json;encoding=utf-8";

            try
            {
                //获取服务器返回数据
                WebResponse wr = hr.GetResponse();
                Stream s = wr.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                string backContent = sr.ReadLine();

                //关闭所有连接
                sr.Dispose();
                s.Dispose();
                wr.Close();

                result = backContent;
            }
            catch
            {
            }
            finally
            {
                hr.Abort();
                hr = null;
            }
            return result;
        }


        public string delMenuSpecialGroup(string menuid,string accessToken)
        {
            string result = "err";

            //并发连接数量
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;

            HttpWebRequest hr = (HttpWebRequest)WebRequest.Create("https://api.weixin.qq.com/cgi-bin/menu/delconditional?access_token=" + accessToken);

            hr.Method = "POST";
            hr.ContentType = "application/json;encoding=utf-8";

            try
            {
                //向服务器发送请求
                Stream w = hr.GetRequestStream();
                //msg = System.Web.HttpUtility.UrlEncode(msg);
                byte[] msgBit = Encoding.UTF8.GetBytes(menuid);

                w.Write(msgBit, 0, menuid.Length);
                w.Close();

                //获取服务器返回数据
                WebResponse wr = hr.GetResponse();
                Stream s = wr.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                string backContent = sr.ReadLine();

                //关闭所有连接
                sr.Dispose();
                s.Dispose();
                wr.Close();

                result = backContent;
            }
            catch
            {
            }
            finally
            {
                hr.Abort();
                hr = null;
            }
            return result;
        }
    }
    

    
}