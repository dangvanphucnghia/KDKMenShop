using System.ComponentModel.DataAnnotations;

namespace KDKMenShop.Repository.Validation
{
	//hình ảnh ...
	public class FileExtensionAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if(value is IFormFile file)
			{
				var extension = Path.GetExtension(file.FileName);
				string[] extensions = { "jpg", "png", "jpeg" };
				bool result = extensions.Any(x=>extension.EndsWith(x));
				if(!result)
				{
					return new ValidationResult("Đuôi ảnh phải là jpg hoặc png ,jpeg");
				}
			}
			return ValidationResult.Success;
		}
	}
}
