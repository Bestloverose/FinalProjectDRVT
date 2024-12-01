using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Identity;
using FinalProject.Areas.Identity.Data;

namespace FinalProject.Areas.EmailSystem.Pages
{
    public class ReadEmailModel : PageModel
    {
        public class EmailInfo
        {
            public int EmailID;
            public string? EmailSubject;
            public string? EmailMessage;
            public DateTime EmailDate;
            public string? EmailSender;
            public string? EmailReceiver;
        }

        private readonly string _connectionString;
        private readonly UserManager<FinalProjectUser> _userManager; // ใช้ UserManager เพื่อตรวจสอบผู้ใช้ที่ล็อกอิน

        public EmailInfo? emailInfo;

        public ReadEmailModel(IConfiguration configuration, UserManager<FinalProjectUser> userManager)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // รับค่าจาก query string
            Request.Query.TryGetValue("EmailId", out StringValues emailId);
            string idString = emailId.ToString();
            int id = int.Parse(idString);

            // ดึงข้อมูลผู้ใช้ที่ล็อกอินอยู่
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login"); // ถ้าไม่มีผู้ใช้ที่ล็อกอินให้ส่งไปที่หน้า login
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT EmailId, EmailSubject, EmailMessage, EmailDate, EmailSender, EmailReceiver FROM Emails WHERE EmailId = @EmailId";
                SqlCommand command1 = new SqlCommand(query, connection);
                command1.Parameters.AddWithValue("@EmailId", id);

                using (SqlDataReader reader = command1.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // ตรวจสอบว่า EmailReceiver ตรงกับผู้ใช้ที่ล็อกอินหรือไม่
                        string emailReceiver = reader.GetString(5);
                        if (emailReceiver != currentUser.UserName) // ตรวจสอบว่าเป็นอีเมลของผู้ใช้ที่ล็อกอินอยู่หรือไม่
                        {
                            return Forbid(); // ถ้าไม่ใช่ให้แสดงข้อความการเข้าถึงถูกปฏิเสธ
                        }

                        // ถ้าใช่ก็ให้โหลดข้อมูลอีเมล
                        emailInfo = new EmailInfo
                        {
                            EmailID = reader.GetInt32(0),
                            EmailSubject = reader.GetString(1),
                            EmailMessage = reader.GetString(2),
                            EmailDate = reader.GetDateTime(3),
                            EmailSender = reader.GetString(4),
                            EmailReceiver = emailReceiver
                        };
                    }
                    else
                    {
                        return NotFound(); // ถ้าไม่พบอีเมลให้แสดงหน้า NotFound
                    }
                }

                // อัพเดตสถานะว่าอีเมลนี้ถูกอ่านแล้ว
                string updateIsReadQuery = "UPDATE Emails SET EmailIsRead = 1 WHERE EmailId = @EmailId";
                SqlCommand command2 = new SqlCommand(updateIsReadQuery, connection);
                command2.Parameters.AddWithValue("@EmailId", id);
                command2.ExecuteNonQuery();
            }

            return Page();
        }
    }
}
