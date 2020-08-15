using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using System.Data;
using WecharCore.Properties;

namespace WecharCore
{
    public partial class MSGCore : System.Web.UI.Page
    {

        userMsg uMsg = new userMsg();
        OutModel oMod = new OutModel();

        protected void Page_Load(object sender, EventArgs e)
        {
            //check();
            if (Request.HttpMethod == "POST")
            {
                Stream s = Request.InputStream;
                byte[] b = new byte[s.Length];//保存读取来的源码字节
                s.Read(b, 0, (int)s.Length);
                s.Dispose();

                
                string xmltext = Encoding.UTF8.GetString(b);//转换源码字节为XML

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xmltext);

                XmlNode xmln = xmldoc.FirstChild;
                XmlElement xel = (XmlElement)xmln;
                
                //判断消息类型，用对应的类保存数据
                switch (xel["MsgType"].InnerText)
                {
                    //文本消息
                    case "text":
                        if (intoText(uMsg, xel))
                        {
                            
                            string[] tmpString = uMsg.Content.Split('_');
                            switch (tmpString[0])
                            {
                                case "关键字":
                                    try
                                    {
                                        //oMod 即OutModel类
                                        Response.Write(oMod.outText(uMsg, "输出文本"));
                                    }
                                    catch (SqlException err)
                                    {
                                        Response.Write(oMod.outText(uMsg, err.Message));
                                    }
                                    break;
                                default:
                                    //交给客服
                                    Response.Write(oMod.outClient(uMsg));
                                    break;
                            }
                        }
                        break;
                    //事件信息
                    case "event":
                        if (intoEvent(uMsg, xel))
                        {
                            string scanResult = "";
                            //未关注的体现
                            if (uMsg.Event == "subscribe")
                            {
                                if (uMsg.EventKey != "")
                                {
                                    scanResult = uMsg.EventKey.Remove(0, 8);
                                    string unResult = Server.UrlDecode(scanResult);

                                    //发送注册页面
                                    newsClass[] nc = new newsClass[1];
                                    nc[0].Title = "商户注册";
                                    nc[0].PicUrl = "http://" + HttpContext.Current.Request.Url.Host + "/img/reg.jpg";
                                    nc[0].Description = "点击进入页面完善您的商户资料";
                                    nc[0].Url = "http://" + HttpContext.Current.Request.Url.Host + "/_Reg.aspx";

                                    Response.Write(oMod.outNews(uMsg, 1, nc));
                                }
                                else
                                {
                                    Response.Write(oMod.outText(uMsg, "欢迎关注我们，点击进入商城即可进行采购。"));
                                }
                            }
                            //已关注扫描带参数二维码
                            if(uMsg.Event=="SCAN")
                            {
                            }
                            
                        }                       
                        break;
                    default:
                        if (intoNull(uMsg, xel))
                        {
                            Response.Write(oMod.outText(uMsg, "这个我不懂是什么哟！"));
                        }
                        break;
                }
            }

        }

        /// <summary>
        /// 接入检查
        /// </summary>
        /// <returns></returns>
        private bool check()
        {
            string test = "";

            string Token = "-"; //微信公众号后台填写的Token
            string timestamp = Request["timestamp"];
            string nonce = Request["nonce"];
            string signature = Request["signature"];

            List<string> tl = new List<string>();
            tl.Add(nonce);
            tl.Add(Token);
            tl.Add(timestamp);

            tl.Sort();

            test = tl.ToString();

            byte[] rec = ASCIIEncoding.UTF8.GetBytes(test);
            
            Response.Write(Request["echostr"]);
            return true;
        }
        #region 处理XML到消息对象
        /// <summary>
        /// 接收到的文本数据输入到用户消息对象
        /// </summary>
        /// <param name="um">用户消息对象</param>
        /// <param name="xml">接收到的XML数据</param>
        /// <returns>无错误返回真</returns>
        private bool intoText(userMsg um, XmlElement xml)
        {
            try
            {
                um.ToUserName = xml["ToUserName"].InnerText;
                um.FromUserName = xml["FromUserName"].InnerText;
                um.CreateTime = xml["CreateTime"].InnerText;
                um.MsgType = xml["MsgType"].InnerText;
                um.Content = xml["Content"].InnerText;
                um.MsgId = xml["MsgId"].InnerText;
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 接收到的图片数据输入到用户消息对象
        /// </summary>
        /// <param name="um">用户消息对象</param>
        /// <param name="xml">接收到的XML数据</param>
        /// <returns>无错误返回真</returns>
        private bool intoImage(userMsg um, XmlElement xml)
        {
            try
            {
                um.ToUserName = xml["ToUserName"].InnerText;
                um.FromUserName = xml["FromUserName"].InnerText;
                um.CreateTime = xml["CreateTime"].InnerText;
                um.MsgType = xml["MsgType"].InnerText;
                um.PicUrl = xml["PicUrl"].InnerText;
                um.MediaId = xml["MediaId"].InnerText;
                um.MsgId = xml["MsgId"].InnerText;
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 接收到的语音数据输入到用户消息对象
        /// </summary>
        /// <param name="um">用户消息对象</param>
        /// <param name="xml">接收到的XML数据</param>
        /// <returns>无错误返回真</returns>
        private bool intoVoice(userMsg um, XmlElement xml)
        {
            try
            {
                um.ToUserName = xml["ToUserName"].InnerText;
                um.FromUserName = xml["FromUserName"].InnerText;
                um.CreateTime = xml["CreateTime"].InnerText;
                um.MsgType = xml["MsgType"].InnerText;
                um.MediaId = xml["MediaId"].InnerText;
                um.Format = xml["Format"].InnerText;
                um.MsgId = xml["MsgId"].InnerText;
                um.Recognition = xml["Recognition"].InnerText;
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 接收到的视频数据输入到用户消息对象
        /// </summary>
        /// <param name="um">用户消息对象</param>
        /// <param name="xml">接收到的XML数据</param>
        /// <returns>无错误返回真</returns>
        private bool intoVideo(userMsg um, XmlElement xml)
        {
            try
            {
                um.ToUserName = xml["ToUserName"].InnerText;
                um.FromUserName = xml["FromUserName"].InnerText;
                um.CreateTime = xml["CreateTime"].InnerText;
                um.MsgType = xml["MsgType"].InnerText;
                um.MediaId = xml["MediaId"].InnerText;
                um.ThumbMediaId = xml["ThumbMediaId"].InnerText;
                um.MsgId = xml["MsgId"].InnerText;
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 接收到的事件数据输入到用户消息对象
        /// </summary>
        /// <param name="um">用户消息对象</param>
        /// <param name="xml">接收到的XML数据</param>
        /// <returns>无错误返回真</returns>
        private bool intoEvent(userMsg um, XmlElement xml)
        {
            try
            {
                um.ToUserName = xml["ToUserName"].InnerText;
                um.FromUserName = xml["FromUserName"].InnerText;
                um.CreateTime = xml["CreateTime"].InnerText;
                um.MsgType = xml["MsgType"].InnerText;
                um.Event = xml["Event"].InnerText;
                

                if (um.Event == "scancode_waitmsg")
                {
                    um.EventKey = xml["EventKey"].InnerText;
                    um.ScanResult = xml["ScanCodeInfo"]["ScanResult"].InnerText;
                    um.ScanType = xml["ScanCodeInfo"]["ScanType"].InnerText;
                }


                if (um.Event == "subscribe" || um.Event == "SCAN")
                {
                    try
                    {
                        um.EventKey = xml["EventKey"].InnerText;
                        um.Ticket = xml["Ticket"].InnerText;
                    }
                    catch
                    {

                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 接收到的未知数据输入到用户消息对象
        /// </summary>
        /// <param name="um">用户消息对象</param>
        /// <param name="xml">接收到的XML数据</param>
        /// <returns>无错误返回真</returns>
        private bool intoNull(userMsg um, XmlElement xml)
        {
            try
            {
                um.ToUserName = xml["ToUserName"].InnerText;
                um.FromUserName = xml["FromUserName"].InnerText;
                um.CreateTime = xml["CreateTime"].InnerText;
                um.MsgType = xml["MsgType"].InnerText;
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }

    #region 输出类
    /// <summary>
    /// 输出类
    /// </summary>
    public class OutModel
    {
        /// <summary>
        /// 转发给客服
        /// </summary>
        /// <param name="um">用户消息对象</param>
        /// <returns>返回消息文本</returns>
        public string outClient(userMsg um)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<xml>");
            sb.AppendFormat("<ToUserName>{0}</ToUserName><FromUserName>{1}</FromUserName><CreateTime>{2}</CreateTime>", um.FromUserName, um.ToUserName, um.CreateTime);
            sb.AppendFormat("<MsgType>transfer_customer_service</MsgType>");
            sb.Append("</xml>");

            return sb.ToString();
        }
        /// <summary>
        /// 输出文本消息类型
        /// </summary>
        /// <param name="um">用户消息对象</param>
        /// <param name="outText">输出的文本</param>
        /// <returns>返回消息文本</returns>
        public string outText(userMsg um, string outText)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<xml>");
            sb.AppendFormat("<ToUserName>{0}</ToUserName><FromUserName>{1}</FromUserName><CreateTime>{2}</CreateTime>", um.FromUserName, um.ToUserName, um.CreateTime);
            sb.AppendFormat("<MsgType>text</MsgType>");
            sb.AppendFormat("<Content>{0}</Content>", outText);
            sb.Append("</xml>");

            return sb.ToString();
        }
        /// <summary>
        /// 输出图文消息类型
        /// </summary>
        /// <param name="um">用户消息对象</param>
        /// <param name="newsCount">图文数量，不得超过10条</param>
        /// <param name="news">新闻数组</param>
        /// <returns>返回消息文本</returns>
        public string outNews(userMsg um, int newsCount, newsClass[] news)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<xml>");
            sb.AppendFormat("<ToUserName>{0}</ToUserName><FromUserName>{1}</FromUserName><CreateTime>{2}</CreateTime>", um.FromUserName, um.ToUserName, um.CreateTime);
            sb.AppendFormat("<MsgType>news</MsgType>");
            sb.AppendFormat("<ArticleCount>{0}</ArticleCount>", newsCount);
            sb.Append("<Articles>");
            for (int i = 0; i < newsCount; i++)
            {
                sb.Append("<item>");
                sb.AppendFormat("<Title>{0}</Title>", news[i].Title, news[i].Url);

                //非必填项判断 不存在就不输出
                if (news[i].Description != null)
                {
                    sb.AppendFormat("<Description>{0}</Description>", news[i].Description);
                }
                if (news[i].PicUrl != null)
                {
                    sb.AppendFormat("<PicUrl>{0}</PicUrl>", news[i].PicUrl);
                }

                sb.AppendFormat("<Url>{0}</Url>", news[i].Url);
                sb.Append("</item>");
            }
            sb.Append("</Articles>");
            sb.Append("</xml>");

            return sb.ToString();
        }
    }
    #endregion

    #region 数据类
    /// <summary>
    /// 网页认证用户基本类
    /// </summary>
    [DataContract]
    public class UserInfor
    {
        [DataMember]
        public string access_token { get; set; }
        [DataMember]
        public int expires_in { get; set; }
        [DataMember]
        public string refresh_token { get; set; }
        [DataMember]
        public string openid { get; set; }
        [DataMember]
        public string scope { get; set; }
        [DataMember]
        public string state { get; set; }
    }

    /// <summary>
    /// 通过OpenID获取的用户高级类
    /// </summary>
    [DataContract]
    public class UserAdv
    {
        [DataMember]
        public string subscribe { get; set; }
        [DataMember]
        public string openid { get; set; }
        [DataMember]
        public string nickname { get; set; }
        [DataMember]
        public int sex { get; set; }
        [DataMember]
        public string language { get; set; }
        [DataMember]
        public string city { get; set; }
        [DataMember]
        public string province { get; set; }
        [DataMember]
        public string country { get; set; }
        [DataMember]
        public string headimgurl { get; set; }
        [DataMember]
        public int subscribe_time { get; set; }
        [DataMember]
        public string unionid { get; set; }
    }

    /// <summary>
    /// 授权秘钥基本类
    /// </summary>
    [DataContract]
    public class appToken
    {
        [DataMember]
        public string access_token { get; set; }

        [DataMember]
        public int expires_in { get; set; }

        [DataMember]
        public int errcode { get; set; }

        [DataMember]
        public string errmsg { get; set; }

        [DataMember]
        public string menu { get; set; }
    }
    /// <summary>
    /// JSAPI基本类
    /// </summary>
    [DataContract]
    public class JSAPI
    {
        [DataMember]
        public int errcode { get; set; }
        [DataMember]
        public string errmsg { get; set; }
        [DataMember]
        public string ticket { get; set; }
        [DataMember]
        public string expires_in { get; set; }
    }

    /// <summary>
    /// 模板ID基本类
    /// </summary>
    [DataContract]
    public class TemplateID
    {
        [DataMember]
        public string errcode { get; set; }
        [DataMember]
        public string errmsg { get; set; }
        [DataMember]
        public string template_id { get; set; }
    }
    /// <summary>
    /// 模板类
    /// </summary>
    [DataContract]
    public class Template
    {
        /// <summary>
        /// 用户OPENIF
        /// </summary>
        [DataMember]
        public string touser { get; set; }
        /// <summary>
        /// 公众平台获取的模板加密ID
        /// </summary>
        [DataMember]
        public string template_id { get; set; }
        /// <summary>
        /// 模板消息链接地址
        /// </summary>
        [DataMember]
        public string url { get; set; }
        /// <summary>
        /// 模板颜色
        /// </summary>
        [DataMember]
        public string topcolor { get; set; }
        /// <summary>
        /// 数据信息
        /// </summary>
        [DataMember]
        public TemplateData data { get; set; }
    }
    /// <summary>
    /// 模板数据类
    /// </summary>
    [DataContract]
    public class TemplateData
    {
        [DataMember]
        public TemplateBase first { get; set; }
        [DataMember]
        public TemplateBase keyword1 { get; set; }
        [DataMember]
        public TemplateBase keyword2 { get; set; }
        [DataMember]
        public TemplateBase keyword3 { get; set; }
        [DataMember]
        public TemplateBase keyword4 { get; set; }
        [DataMember]
        public TemplateBase keyword5 { get; set; }
        [DataMember]
        public TemplateBase remark { get; set; }
    }
    [DataContract]
    public class TemplateBase
    {
        [DataMember]
        public string value { get; set; }
        [DataMember]
        public string color { get; set; }
    }
    /// <summary>
    /// 网页授权秘钥基本类(暂不使用)
    /// </summary>
    [DataContract]
    public class webToken
    {
        [DataMember]
        public string access_token { get; set; }

        [DataMember]
        public int expires_in { get; set; }

        [DataMember]
        public int refresh_token { get; set; }

        [DataMember]
        public string openid { get; set; }

        [DataMember]
        public string scope { get; set; }
    }

    public class userMsg
    {
        //事件消息
        public string Event { get; set; }
        //通用属性
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public string CreateTime { get; set; }
        public string MsgType { get; set; }
        public string MsgId { get; set; }

        //文本消息
        public string Content { get; set; }

        //媒体消息专有属性
        public string MediaId { get; set; }

        //图片消息
        public string PicUrl { get; set; }
        //语音消息
        public string Format { get; set; }
        public string Recognition { get; set; }
        //视频消息
        public string ThumbMediaId { get; set; }

        //二维码
        public string EventKey { get; set; }
        public string ScanType { get; set; }
        public string ScanResult { get; set; }
        //带参数二维码
        public string Ticket { get; set; }

    }
    /// <summary>
    /// 图文消息数据结构
    /// </summary>
    public struct newsClass
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PicUrl { get; set; }
        public string Url { get; set; }

    }
    #endregion
}