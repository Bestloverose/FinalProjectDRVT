using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;

namespace FinalProject.Areas.EmailSystem.Pages
{
    public class ComposeMailModel : PageModel
    {
        [BindProperty]
        public string? Subject { get; set; }

        [BindProperty]
        public string? Body { get; set; }

        [BindProperty]
        public string? Receiver { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            String connectionString = "Server=tcp:sprint1.database.windows.net,1433;Initial Catalog=sprint1;Persist Security Info=False;User ID=dbadmin;Password=P@ssw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                String query = "INSERT INTO Emails (EmailSubject, EmailMessage, EmailDate, EmailIsRead, EmailSender, EmailReceiver) VALUES (@EmailSubject, @EmailMessage, @EmailDate, @EmailIsRead, @EmailSender, @EmailReceiver)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    Request.Query.TryGetValue("Sender", out StringValues senderValue);
                    String Sender = senderValue.ToString();

                    command.Parameters.AddWithValue("@EmailSubject", Subject);
                    command.Parameters.AddWithValue("@EmailMessage", Body);
                    command.Parameters.AddWithValue("@EmailDate", DateTime.Now);
                    command.Parameters.AddWithValue("@EmailIsRead", false);
                    command.Parameters.AddWithValue("@EmailSender", Sender);
                    command.Parameters.AddWithValue("@EmailReceiver", Receiver);

                    await command.ExecuteNonQueryAsync();
                }
            }
            return RedirectToPage("/Index");
        }
    }
}