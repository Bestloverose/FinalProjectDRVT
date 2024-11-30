using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;
using System.Text.RegularExpressions;

namespace FinalProject.Areas.EmailSystem.Pages
{
    public class ComposeMailModel : PageModel
    {
        private readonly string _connectionString;

        public ComposeMailModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [BindProperty]
        public string? Subject { get; set; }

        [BindProperty]
        public string? Body { get; set; }

        [BindProperty]
        public string? Receiver { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate input fields
            if (string.IsNullOrWhiteSpace(Subject))
                ModelState.AddModelError(nameof(Subject), "Please fill in the Subject field.");

            if (string.IsNullOrWhiteSpace(Body))
                ModelState.AddModelError(nameof(Body), "Please fill in the Body field.");

            if (string.IsNullOrWhiteSpace(Receiver))
                ModelState.AddModelError(nameof(Receiver), "Please fill in the Receiver field.");

            // Validate Receiver username format (no special characters, just alphanumeric)
            if (!string.IsNullOrWhiteSpace(Receiver) &&
                !Regex.IsMatch(Receiver, @"^[a-zA-Z0-9_]+$"))
            {
                ModelState.AddModelError(nameof(Receiver), "Please enter a valid username.");
            }

            if (!ModelState.IsValid)
                return Page();

            // Retrieve and validate the sender
            Request.Query.TryGetValue("Sender", out StringValues senderValue);
            string sender = senderValue.ToString();

            if (string.IsNullOrWhiteSpace(sender) || sender != User.Identity?.Name)
            {
                ModelState.AddModelError(string.Empty, "You cannot send an email as another person.");
                return Page();
            }

            if (sender == Receiver)
            {
                ModelState.AddModelError(string.Empty, "You cannot send an email to yourself.");
                return Page();
            }

            try
            {
                // ตรวจสอบว่า receiver มีอยู่ในฐานข้อมูล
                bool receiverExists;
                string receiverFromDatabase = string.Empty; // ตัวแปรสำหรับเก็บข้อมูลชื่อผู้รับจากฐานข้อมูล

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT Username FROM AspNetUsers WHERE Username = @Receiver"; // ดึงแค่ Username ของ receiver
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Receiver", Receiver);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                receiverFromDatabase = reader.GetString(0); // รับข้อมูลจากฐานข้อมูล
                                receiverExists = true;
                            }
                            else
                            {
                                receiverExists = false;
                            }
                        }
                    }
                }

                // ถ้า receiver ไม่มีในฐานข้อมูล
                if (!receiverExists)
                {
                    ModelState.AddModelError(nameof(Receiver), "The receiver does not exist. This user has not registered yet.");
                    return Page();
                }

                // เปรียบเทียบ Receiver กับชื่อผู้รับในฐานข้อมูลแบบ case-sensitive
                if (!string.Equals(Receiver, receiverFromDatabase, StringComparison.Ordinal))
                {
                    ModelState.AddModelError(nameof(Receiver), "Your cannot send an email as another person.");
                    return Page();
                }

                // ส่วนการส่งอีเมล
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                                INSERT INTO Emails 
                                (EmailSubject, EmailMessage, EmailDate, EmailIsRead, EmailSender, EmailReceiver) 
                                VALUES 
                                (@EmailSubject, @EmailMessage, @EmailDate, @EmailIsRead, @EmailSender, @EmailReceiver)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        TimeZoneInfo bangkokTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                        DateTimeOffset bangkokTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, bangkokTimeZone);

                        command.Parameters.AddWithValue("@EmailSubject", Subject);
                        command.Parameters.AddWithValue("@EmailMessage", Body);
                        command.Parameters.AddWithValue("@EmailDate", bangkokTime);
                        command.Parameters.AddWithValue("@EmailIsRead", false);
                        command.Parameters.AddWithValue("@EmailSender", sender);
                        command.Parameters.AddWithValue("@EmailReceiver", Receiver); // ไม่ต้องแปลงตัวพิมพ์

                        await command.ExecuteNonQueryAsync();

                        TempData["SuccessMessage"] = "Your email has been sent successfully.";
                    }
                }

                return RedirectToPage();
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while sending the email.");
                return Page();
            }
        }
    }
}
