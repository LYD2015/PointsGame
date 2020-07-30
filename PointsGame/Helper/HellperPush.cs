using ApiCore;
using ApiCore.Dto;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XYH.Core.Log;

namespace PointsGame.Helper
{
    public class HellperPush
    {
        private ILogger Logger = LoggerManager.GetLogger(nameof(HellperPush));
        private readonly IConfigurationRoot _config;
        private readonly RestClient _restClient;
        public HellperPush(IConfigurationRoot configuration, RestClient restClient)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _restClient = restClient;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="title">邮件标题</param>
        /// <param name="content">邮件内容(可以html)</param>
        /// <param name="respects">问候语</param>
        /// <param name="toName">发送</param>
        /// <param name="toEmailAddress"></param>
        /// <returns></returns>
        public async Task SendEmail(string title, string respects, string content, List<string> toEmailAddress)
        {
            Logger.Info($"{title}---收件人:{JsonHelper.ToJson(toEmailAddress)}");
            var messageSend = new MimeMessage
            {
                Sender = new MailboxAddress("PointsSystem", _config["SendEmailAddress"]),
                Subject = title,
            };
            foreach (var item in toEmailAddress)
            {
                if (Regex.IsMatch(item, @"^[A-Za-z0-9\u4e00-\u9fa5]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$"))
                    messageSend.To.Add(new MailboxAddress(item));
            }          

            //HTML 内容 您在《新空间积分系统第一赛季》中有新的动态：啊啊啊离开家连接看时间离开家度量科技杀戮空间
            messageSend.Body = new TextPart(TextFormat.Html) { Text = $@"<p>{respects}</p>
                                                                         <p>{content}</p>
                                                                         <p>传送门：<a href = '{_config["PointGameWebUrl"]}' target = '_blank' >{_config["PointGameWebUrl"]}</a></p>" };
            //纯文本 内容
            //messageSend.Body = new TextPart(TextFormat.Plain) { Text = "积分系统邮件提醒,你好啊？" };
            using (var client = new SmtpClient())
            {
                try
                {                  
                    await client.ConnectAsync("smtp.qq.com", int.Parse(_config["SendEmailPort"]), (SecureSocketOptions)int.Parse(_config["SendEmailSocket"]));
                    await client.AuthenticateAsync(_config["SendEmailAddress"], _config["SendEmailPassword"]);
                    await client.SendAsync(messageSend);
                    await client.DisconnectAsync(true);
                    Logger.Info($"邮件发送成功");
                }
                catch (Exception e)
                {

                    Logger.Error($"发送邮件失败：{e.Message}\r\n{e.StackTrace}");
                }

            }            
        }

        /// <summary>
        /// 向铺侦探员工端APP发送Push
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public async Task SendEmpPush(string title,string content,List<string> userIds)
        {
            try
            {
                SendMessageRequest sendMessageRequest = new SendMessageRequest();
                sendMessageRequest.AppName = "employee";
                sendMessageRequest.MessageTypeCode = "EmployeesAppPush";
                MessageItem messageEmployeeItem = new MessageItem();
                //接收人
                messageEmployeeItem.UserIds = userIds;
                messageEmployeeItem.MessageTypeItems = new List<TypeItem> {
                        new TypeItem{ Key="TITLE", Value=title},
                        new TypeItem { Key="CONTENTS",Value=content},
                        //关键数据，与前端约定的
                        new TypeItem{Key="attachData",Value=JsonHelper.ToJson(new
                            {
                                url =$"/zc/messageDetail/noticeDetail",                                
                                extData = new
                                {
                                    //组合前端系统消息列表需要的信息                                    
                                    desc =content,
                                    type=2
                                }
                            })
                        }
                    };
                sendMessageRequest.MessageList = new List<MessageItem> { messageEmployeeItem };
                Logger.Trace($"开始推送AppPush地址：{_config["MessageServerUrl"]}\r\n参数：{JsonHelper.ToJson(sendMessageRequest)}");
                //发送push
                var s= await _restClient.Post(_config["MessageServerUrl"], sendMessageRequest,"Post",null);                
            }
            catch (Exception e)
            {
                Logger.Error("推送AppPush出错：\r\n{0}", e.ToString());
            }
        }
    }
}
