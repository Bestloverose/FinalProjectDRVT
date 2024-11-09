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
            // ��Ǩ�ͺ����տ�Ŵ���ҧ����
            if (string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Body) || string.IsNullOrWhiteSpace(Receiver))
            {
                ModelState.AddModelError(string.Empty, "Please fill in the Subject, Body, and Receiver fields.");
                return Page(); // �ʴ�˹������������ͤ����ʴ���ͼԴ��Ҵ
            }

            // ��Ǩ�ͺ��� Sender ��� Receiver �������͹�ѹ
            Request.Query.TryGetValue("Sender", out StringValues senderValue);
            string sender = senderValue.ToString();

            if (sender == Receiver)
            {
                ModelState.AddModelError(string.Empty, "You cannot send an email to yourself.");
                return Page(); // �ʴ�˹������������ͤ����ʴ���ͼԴ��Ҵ
            }

            // ��Ҽ�ҹ��õ�Ǩ�ͺ���� ���ѹ�֡ŧ�ҹ������
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

            return RedirectToPage("/Index"); // ��Ѻ�˹�� Index ��ѧ�ҡ�ѹ�֡����������
        }
    }
}
