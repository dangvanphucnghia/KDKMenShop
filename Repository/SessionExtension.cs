using System.Runtime.CompilerServices;
using Newtonsoft.Json;


namespace KDKMenShop.Repository
{
    public static class SessionExtension
    {
        //sử dụng SerializeObject và DeserializeObject giúp bạn dễ dàng làm việc với các đối tượng
        //phức tạp trong phiên mà không cần phải tự mình chuyển đổi giữa đối tượng và chuỗi JSON.
        public static void SetJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value)); // chuyển thành chuỗi Json
        }
        public static T GetJson<T>(this ISession session , string key)
        {
            var sessionData = session.GetString(key);
            return sessionData == null ? default : JsonConvert.DeserializeObject<T>(sessionData);//chuyển chuỗi Json thành đối tượng
        }
    }
}
