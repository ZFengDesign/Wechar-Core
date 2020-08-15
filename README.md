# Wechar-Core
使用C#语言开发的微信消息管理模块
功能比较基础，容易使用。
包含了信息的接收分析、以及被动回复信息。

# 基本配置
Web.Config添加2个基本参数
AppID：微信公众号的APPID
AppSecret：微信公众号的APPSECRET
# 源码文件说
MSGCore.aspx 用于处理微信接收消息以及被动回复
ProCore.cs 类文件，包含了常用的网络请求方法，例如GetAccessTokne等的方法封装。
