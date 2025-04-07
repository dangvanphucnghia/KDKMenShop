using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2.Service;
using Twilio.Types;

namespace KDKMenShop.Service
{
	public interface ISmsService
	{
		Task SendSmsAsync(string toPhoneNumber, string message);
	}
	public class TwilloSmsService : ISmsService
	{
		private readonly string  _accountSid;
		private readonly string _authToken;
		private readonly string _fromPhoneNumber;
		public TwilloSmsService(string accountSid, string authToken, string fromPhoneNumber)
		{
			_accountSid = accountSid;
			_authToken = authToken;
			_fromPhoneNumber = fromPhoneNumber;
		}
		public Task SendSmsAsync(string toPhoneNumber, string message)
		{
			TwilioClient.Init(_accountSid, _authToken);
			var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
			{
				From = new PhoneNumber(_fromPhoneNumber),
				Body = message
			};
			var msg = MessageResource.Create(messageOptions);
			return Task.FromResult(msg);
		}
	}
}
