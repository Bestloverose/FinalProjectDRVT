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
            // ��Ǩ�ͺ����տ�Ŵ� Subject, Body, ��� Receiver ��ҧ����
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

            // ����բ�ͼԴ��Ҵ� � ����ʴ�˹������������ͤ����ʴ���ͼԴ��Ҵ
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ��Ǩ�ͺ��� Sender ��� Receiver �������͹�ѹ
            Request.Query.TryGetValue("Sender", out StringValues senderValue);
            string sender = senderValue.ToString();

            if (sender == Receiver)
            {
                ModelState.AddModelError(string.Empty, "You cannot send an email to yourself.");
                return Page(); // �ʴ�˹������������ͤ����ʴ���ͼԴ��Ҵ
            }

            // ��Ǩ�ͺ��� Receiver ��㹰ҹ�������������
            bool receiverExists = false;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM AspNetUsers WHERE Email = @Receiver"; // ����¹�繪������ŷ��١��ͧ

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
                // �ʴ���ͤ����Դ��Ҵ�óռ���Ѻ�����㹰ҹ������
                ModelState.AddModelError("Receiver", "The receiver does not exist. This user has not registered yet.");
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
