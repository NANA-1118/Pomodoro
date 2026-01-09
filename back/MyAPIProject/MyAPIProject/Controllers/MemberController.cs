using Azure.Core;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MyAPIProject.Models;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MyAPIProject.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly string _connectString;

        public MemberController(IConfiguration configuration)
        {
            _connectString = configuration.GetConnectionString("PomoDb")
                             ?? throw new ArgumentNullException(nameof(configuration), "PomoDb connection string not found.");
        }

        /// <summary>
        /// 密碼加密:丟入帳號及未加密的密碼->回傳加密後的密碼
        /// </summary>
        public static string SecrectPwd(string Account, string Password)
        {
            string mySalt = Account.Substring(0, 1).ToLower(); //使用帳號第一碼當作密碼鹽
            System.Security.Cryptography.SHA256 mySha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(mySalt + Password); //將密碼鹽及原密碼組合
            byte[] hash = mySha256.ComputeHash(bytes); //組合後一起加密
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)  //將 hash 的每個元素轉換為兩位十六進位字元
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        // POST member/signup
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] Member input)
        {
            if (string.IsNullOrWhiteSpace(input.Account) || string.IsNullOrWhiteSpace(input.Password))
                return BadRequest("Account or Password cannot be empty");

            using var conn = new SqlConnection(_connectString);
            await conn.OpenAsync();

            // 檢查帳號是否存在
            var exists = await conn.QueryFirstOrDefaultAsync<int>(
                "SELECT 1 FROM Member WHERE Account = @Account",
                new { input.Account });

            if (exists == 1)
            {
                return Conflict(new { message = "已存在相同帳號" });
            }

            //加密密碼
            string encryptedPwd = SecrectPwd(input.Account, input.Password);

            //寫入資料庫
            string insertSql = @"       INSERT INTO Member (Account, Password, Isverified, UpdateTime, CreateTime)
                                                        VALUES (@Account, @Password, 0, @UpdateTime, @CreateTime)";

            await conn.ExecuteAsync(insertSql, new
            {
                Account = input.Account,
                Password = encryptedPwd,
                UpdateTime = DateTime.Now,
                CreateTime = DateTime.Now
            });

            return Ok(new { message = "註冊成功" });
        }


        // POST member/signin
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] Member input)
        {
            if (string.IsNullOrWhiteSpace(input.Account) || string.IsNullOrWhiteSpace(input.Password))
                return BadRequest("Account or Password cannot be empty");

            using var conn = new SqlConnection(_connectString);
            await conn.OpenAsync();

            // 查詢帳號
            var member = await conn.QueryFirstOrDefaultAsync<Member>(
                "SELECT * FROM Member WHERE Account = @Account",
                new { input.Account });

            if (member == null)
                return Unauthorized(new { message = "帳號不存在" });

            // 驗證密碼
            string encryptedPwd = SecrectPwd(input.Account, input.Password);
            if (member.Password != encryptedPwd)
                return Unauthorized(new { message = "密碼錯誤" });

            // 登入成功
            return Ok(new { message = "登入成功", memberId = member.MemberId, memberName = member.MemberName });
        }

        // POST member/forgot
        [HttpPost("forgot")]
        public async Task<IActionResult> Forgot([FromBody] Member input)
        {
            if (string.IsNullOrWhiteSpace(input.Account))
                return BadRequest("Account cannot be empty");

            using var conn = new SqlConnection(_connectString);
            await conn.OpenAsync();

            // 查詢帳號是否存在
            var member = await conn.QueryFirstOrDefaultAsync<Member>(
                "SELECT * FROM Member WHERE Account = @Account",
                new { input.Account });

            if (member == null)
            {
                return BadRequest(new { message = "帳號不存在" });
            }

            // 產生新密碼 (8 位英數亂數)
            string newPassword = GenerateRandomPassword(8);

            // 加密密碼
            string encryptedPwd = SecrectPwd(member.Account, newPassword);

            // 更新密碼
            string updateSql = @"UPDATE Member 
                         SET Password = @Password, UpdateTime = @UpdateTime 
                         WHERE Account = @Account";

            await conn.ExecuteAsync(updateSql, new
            {
                Password = encryptedPwd,
                UpdateTime = DateTime.Now,
                Account = member.Account
            });

            // 寄送郵件
            bool success = await SendMailAsync(member.Account, newPassword);

            if (success)
                return Ok(new { message = "新密碼已寄送至您的信箱" });
            else
                return BadRequest(new { message = "寄送失敗，請確認 Email 是否正確" });
        }




        // === 產生亂數 ===
        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }



        // === 使用 Gmail SMTP 寄信 ===
        private async Task<bool> SendMailAsync(string toEmail, string newPassword)
        {
            try
            {
                // Gmail SMTP 設定
                string gmailUser = "account@gmail.com";       // ←  Gmail帳號
                string gmailAppPassword = "password";    // ←  Gmail 密碼

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential(gmailUser, gmailAppPassword);

                    var mail = new MailMessage();
                    mail.From = new MailAddress(gmailUser, "系統管理員");
                    mail.To.Add(toEmail);
                    mail.Subject = "您的新密碼";
                    mail.Body = $@"
                                        親愛的使用者您好，

                                        您的新密碼為：{newPassword}

                                        請儘快登入並修改密碼。

                                        此信件由系統自動發送，請勿回覆。";

                    await smtp.SendMailAsync(mail);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mail Error: " + ex.Message);
                return false;
            }
        }
    }




}
}
