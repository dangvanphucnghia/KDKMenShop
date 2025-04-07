using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KDKMenShop.Models.DAO
{
    public class DAO
    {
        
        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // MD5 được sử dụng để tính toán giá trị băm của dữ liệu.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                //sử dụng cơ số 16 để băm
                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    //Mỗi byte được chuyển thành một chuỗi 2 ký tự 
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }
        public static void SendtoEmail(string to, string subject, string body)
        {
            var fromAddress = new MailAddress("kikyoutnt33@gmail.com", "KDK Men Shop");
            var toAddress = new MailAddress(to);
            const string fromPassword = "a m s j h a p u g c t b v s v a";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // máy chủ gmail
                Port = 587, // cổng mặc định 
                EnableSsl = true, // kích hoạt giao thức mật mã hóa(SSL,TLS) gửi gmail
                DeliveryMethod = SmtpDeliveryMethod.Network, //gửi gmail qua mạng
                UseDefaultCredentials = false,//cho biết thông tin đăng nhập được sử dụng bởi Credentials
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
        public static string GenerateOTP()
		{
			Random random = new Random();
			int otp = random.Next(100000, 1000000); // Sinh số ngẫu nhiên từ 100000 đến 999999
			return otp.ToString();
		}
	}
}
