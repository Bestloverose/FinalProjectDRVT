using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;

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
            // ตรวจสอบว่ามีฟิลด์ Subject, Body, และ Receiver ว่างเปล่า
            if (string.IsNullOrWhiteSpace(Subject))
            {
                ModelState.AddModelError("Subject", "Please fill in the Subject field.");
            }

            if (string.IsNullOrWhiteSpace(Body))
            {
                ModelState.AddModelError("Body", "Please fill in the Body field.");
            }

            if (string.IsNullOrWhiteSpace(Receiver))
            {
                ModelState.AddModelError("Receiver", "Please fill in the Receiver field.");
            }

            // ถ้ามีข้อผิดพลาดใด ๆ ให้แสดงหน้าเดิมพร้อมข้อความแสดงข้อผิดพลาด
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ตรวจสอบว่า Sender และ Receiver ไม่เหมือนกัน
            Request.Query.TryGetValue("Sender", out StringValues senderValue);
            string sender = senderValue.ToString();

            if (sender == Receiver)
            {
                ModelState.AddModelError(string.Empty, "You cannot send an email to yourself.");
                return Page(); // แสดงหน้าเดิมพร้อมข้อความแสดงข้อผิดพลาด
            }

            // ตรวจสอบว่า Receiver มีในฐานข้อมูลหรือไม่
            bool receiverExists = false;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM AspNetUsers WHERE Email = @Receiver"; // เปลี่ยนเป็นชื่อเทเบิลที่ถูกต้อง

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Receiver", Receiver);
                    int count = (int)await command.ExecuteScalarAsync();

                    if (count > 0)
                    {
                        receiverExists = true;
                    }
                }
            }

            if (!receiverExists)
            {
                // แสดงข้อความผิดพลาดกรณีผู้รับไม่มีในฐานข้อมูล
                ModelState.AddModelError("Receiver", "The receiver does not exist. This user has not registered yet.");
                return Page(); // แสดงหน้าเดิมพร้อมข้อความแสดงข้อผิดพลาด
            }

            // ถ้าผ่านการตรวจสอบแล้ว ให้บันทึกลงฐานข้อมูล
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Emails (EmailSubject, EmailMessage, EmailDate, EmailIsRead, EmailSender, EmailReceiver) VALUES (@EmailSubject, @EmailMessage, @EmailDate, @EmailIsRead, @EmailSender, @EmailReceiver)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EmailSubject", Subject);
                    command.Parameters.AddWithValue("@EmailMessage", Body);
                    command.Parameters.AddWithValue("@EmailDate", DateTime.Now);
                    command.Parameters.AddWithValue("@EmailIsRead", false);
                    command.Parameters.AddWithValue("@EmailSender", sender);
                    command.Parameters.AddWithValue("@EmailReceiver", Receiver);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage("/Index"); // กลับไปหน้า Index หลังจากบันทึกข้อมูลเสร็จ
        }
    }
}
