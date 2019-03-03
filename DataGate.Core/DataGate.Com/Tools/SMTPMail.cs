using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Configuration;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataGate.Com
{

    /// <summary>
    /// 邮件发送的帮助类
    /// </summary>
    public class SMTPMail
    {
        /// <summary>
        /// 发件人
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// 收件人
        /// </summary>
        public String To { get; set; }

        /// <summary>
        /// 抄送
        /// </summary>
        public string CC { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 附件文件
        /// </summary>
        public String[] Files { get; set; }

        /// <summary>
        /// 邮件服务器
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 发件人账号的密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 创建一个邮件发送类
        /// </summary>
        public SMTPMail(string mailSettings) { GetAccount(mailSettings); }


        /// <summary>
        /// 异步发送时发送完成事件委托
        /// </summary>
        public AsyncCompletedEventHandler SendCompleted { get; set; }

        /// <summary>
        /// 邮件异步发送时传送给发送完成时事件的对象
        /// </summary>
        public object UserState { get; set; }

        /// <summary>
        /// 获取账号信息
        /// </summary>
        void GetAccount(string mailSettings)
        {
            StringSpliter hp = new StringSpliter(mailSettings, "&", "=");

            Server = hp["Server"];
            Port = hp["Port"].ToInt();
            UserName = hp["UserName"];
            Password = hp["Password"];
            From = hp["From"];

            if (Port == 0) Port = 25;
        }

        /// <summary>
        /// 生成邮件信息对象
        /// </summary>
        /// <returns>邮件信息对象</returns>
        protected System.Net.Mail.MailMessage MakeMessage()
        {
            //MailMessage message = new MailMessage(From, To, Subject, Body);
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.From = new MailAddress(From);
            string[] emails = To.Replace(',', ';').Split(';');
            foreach (string email in emails)
            {
                if (!string.IsNullOrEmpty(email) && CommOp.IsEmail(email))
                {
                    message.To.Add(email);
                }
            }
            CC = CC ?? "";
            emails = CC.Replace(',', ';').Split(';');
            foreach (string email in emails)
            {
                if (!string.IsNullOrEmpty(email) && CommOp.IsEmail(email))
                {
                    message.CC.Add(email);
                }
            }

            message.Subject = Subject;
            //message.SubjectEncoding = Encoding.UTF8;
            message.Body = Body;
            //message.BodyEncoding = Encoding.UTF8;

            //添加文件附件
            if (!Files.IsEmpty())
            {
                foreach (string file in Files)
                {
                    Attachment att = new Attachment(file);

                    //att.NameEncoding = Encoding.UTF8;
                    //att.Name = new FileInfo(file).Name;

                    message.Attachments.Add(att);
                }
            }
            message.IsBodyHtml = true;
            return message;
        }

        /// <summary>
        /// 异步发送邮件, 通过SendCompleted捕捉回调事件
        /// </summary>
        public void SendAsync()
        {
            System.Net.Mail.MailMessage message = MakeMessage();

            SmtpClient client = new SmtpClient(Server)
            {
                Port = Port,
                Credentials = new NetworkCredential(UserName, Password)
            };
            client.SendCompleted += new SendCompletedEventHandler(client_SendCompleted);

            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            client.SendAsync(message, UserState);
        }

        //// <summary>
        /// 组织邮件内容并发送邮件
        /// </summary>
        /// <returns>错误信息，无错误返回空</returns>
        public async Task SendMailAsync()
        {
            System.Net.Mail.MailMessage message = MakeMessage();
            SmtpClient client = new SmtpClient(Server)
            {
                Port = Port,
                EnableSsl = true,
                Credentials = new NetworkCredential(UserName, Password)
            };
            try
            {
                await client.SendMailAsync(message);
            }
            finally
            {
                foreach (Attachment att in message.Attachments)
                {
                    att.Dispose();
                }
                client.Dispose();
            }
        }

        /// <summary>
        /// 异步发信时发完以后的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            SendCompleted?.Invoke(sender, e);
        }
    }
}