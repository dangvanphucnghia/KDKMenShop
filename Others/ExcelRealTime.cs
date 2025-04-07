
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace KDKMenShop.Others
{
	public class ExcelRealTime : BackgroundService
	{
		private readonly string _logFilePath;
		private readonly string _excelFilePath;
		private readonly TimeSpan _updateInterval;

		public ExcelRealTime()
		{
			_logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "log.txt");
			_excelFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "LogData-Moi.xlsx");
			_updateInterval = TimeSpan.FromSeconds(5); // Update Excel file every 5 seconds
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Console.WriteLine("RealTimeExcelLogger started.");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					CreateExcelFile(_logFilePath);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error updating Excel file: {ex.Message}");
				}

				await Task.Delay(_updateInterval, stoppingToken);
			}

			Console.WriteLine("RealTimeExcelLogger stopped.");
		}

		private void CreateExcelFile(string logFilePath)
		{
			Console.WriteLine($"Cập nhập dữ liệu Excel từ file: {logFilePath}");

			if (!File.Exists(logFilePath))
			{
				Console.WriteLine("Log file not found.");
				return;
			}

			// Ensure EPPlus is licensed for non-commercial use
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Logs");

				// Add headers
				worksheet.Cells[1, 1].Value = "Controller & Action";
				worksheet.Cells[1, 2].Value = "Timestamp";
				worksheet.Cells[1, 3].Value = "Query Execution Time (ms)";
				worksheet.Cells[1, 4].Value = "Query";

				// Apply bold styling to headers
				using (var range = worksheet.Cells[1, 1, 1, 4])
				{
					range.Style.Font.Bold = true;
				}

				try
				{
					// Open the log file with shared access
					using (var stream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					using (var reader = new StreamReader(stream))
					{
						var logs = reader.ReadToEnd().Split(Environment.NewLine);
						int row = 2; // Start from the second row for data
						int currentIndex = 0;

						while (currentIndex < logs.Length)
						{
							string log = logs[currentIndex];
							bool isRowPopulated = false;

							if (log.Contains("KDKMenShop.Controllers.")) // Extract Controller & Action
							{
								string lastControllerAction = ExtractControllerAction(log);
								worksheet.Cells[row, 1].Value = lastControllerAction;
								isRowPopulated = true;
							}

							if (log.Contains("[INF] Executed DbCommand")) // Log Entry with SQL Query
							{
								worksheet.Cells[row, 2].Value = ExtractTimestamp(log).ToString("yyyy-MM-dd HH:mm:ss");

								string query = ExtractQuery(log, logs, currentIndex);
								worksheet.Cells[row, 3].Value = ExtractExecutionTime(log);
								worksheet.Cells[row, 4].Value = query;

								isRowPopulated = true;
							}

							currentIndex++;

							if (isRowPopulated)
							{
								row++;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error reading log file: {ex.Message}");
					return;
				}

				// Auto-fit columns to match content
				worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

				// Save the Excel file
				try
				{
					Directory.CreateDirectory(Path.GetDirectoryName(_excelFilePath)!);
					package.SaveAs(new FileInfo(_excelFilePath));
					Console.WriteLine($"Excel file đã được cập nhập và ở: {_excelFilePath}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error saving Excel file: {ex.Message}");
				}
			}
		}

	private string ExtractControllerAction(string log)
		{
			try
			{
				int startIndex = log.IndexOf("KDKMenShop.Controllers");
				if (startIndex == -1) return "Unknown Controller & Action";

				int endIndex = log.IndexOf("'", startIndex);
				if (endIndex == -1)
				{
					endIndex = log.IndexOf(" ", startIndex);
				}

				return endIndex > startIndex ? log.Substring(startIndex, endIndex - startIndex).Trim() : "Unknown Controller & Action";
			}
			catch
			{
				return "Unknown Controller & Action";
			}
		}

		private double ExtractExecutionTime(string log)
		{
			try
			{
				int startIndex = log.IndexOf("(");
				int endIndex = log.IndexOf("ms", startIndex);

				if (startIndex >= 0 && endIndex > startIndex)
				{
					string timeSubstring = log.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
					return double.TryParse(timeSubstring, out var result) ? result : 0;
				}

				return 0;
			}
			catch
			{
				return 0;
			}
		}

		private string ExtractQuery(string log, string[] logs, int currentIndex)
		{
			string query = string.Empty;

			int queryStart = log.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
			if (queryStart != -1)
			{
				query = log.Substring(queryStart).Trim();
			}

			while (currentIndex + 1 < logs.Length && !logs[currentIndex + 1].Contains("[INF] Executed DbCommand"))
			{
				currentIndex++;
				query += " " + logs[currentIndex].Trim();
			}

			query = query.Replace("[Parameters=[], CommandType='\"Text\"', CommandTimeout='30']", "").Trim();

			return query;
		}

		private DateTime ExtractTimestamp(string log)
		{
			try
			{
				string timestampPart = log.Substring(0, log.IndexOf(" ["));
				return DateTime.TryParse(timestampPart, out var result) ? result : DateTime.MinValue;
			}
			catch
			{
				return DateTime.MinValue;
			}
		}
	}

}
