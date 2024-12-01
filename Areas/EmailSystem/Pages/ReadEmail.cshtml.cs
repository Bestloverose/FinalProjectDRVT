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
        private readonly UserManager<FinalProjectUser> _userManager; // �� UserManager ���͵�Ǩ�ͺ���������͡�Թ

        public EmailInfo? emailInfo;

        public ReadEmailModel(IConfiguration configuration, UserManager<FinalProjectUser> userManager)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // �Ѻ��Ҩҡ query string
            Request.Query.TryGetValue("EmailId", out StringValues emailId);
            string idString = emailId.ToString();
            int id = int.Parse(idString);

            // �֧�����ż��������͡�Թ����
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login"); // �������ռ��������͡�Թ�����价��˹�� login
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
                        // ��Ǩ�ͺ��� EmailReceiver �ç�Ѻ���������͡�Թ�������
                        string emailReceiver = reader.GetString(5);
                        if (emailReceiver != currentUser.UserName) // ��Ǩ�ͺ���������Ţͧ���������͡�Թ�����������
                        {
                            return Forbid(); // ������������ʴ���ͤ��������Ҷ֧�١����ʸ
                        }

                        // �����������Ŵ�����������
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
                        return NotFound(); // �����辺���������ʴ�˹�� NotFound
                    }
                }

                // �Ѿവʶҹ��������Ź��١��ҹ����
                string updateIsReadQuery = "UPDATE Emails SET EmailIsRead = 1 WHERE EmailId = @EmailId";
                SqlCommand command2 = new SqlCommand(updateIsReadQuery, connection);
                command2.Parameters.AddWithValue("@EmailId", id);
                command2.ExecuteNonQuery();
            }

            return Page();
        }
    }
}
