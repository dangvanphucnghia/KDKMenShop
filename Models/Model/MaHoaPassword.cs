using System.Security.Cryptography;
using System.Text;

namespace KDKMenShop.Models.Model
{
	public class MaHoaPassword
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
	}
}
