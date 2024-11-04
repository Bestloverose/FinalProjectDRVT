using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.AspNetCore.Identity;


namespace FinalProject.Areas.EmailSystem.Pages
{
    public class ReadEmailModel : PageModel
    {
        public class EmailInfo
        {
            public int EmailID;
            public String? EmailSubject;
            public String? EmailMessage;
            public DateTime EmailDate;
            public String? EmailSender;
            public String? EmailReceiver;
        }

        private readonly string _connectionString;

        public EmailInfo? emailInfo;

        public ReadEmailModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void OnGet()
        {
            Request.Query.TryGetValue("EmailId", out StringValues emailId);
            String idString = emailId.ToString();
            int id = int.Parse(idString);

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
                        emailInfo = new EmailInfo
                        {
                            EmailID = reader.GetInt32(0),
                            EmailSubject = reader.GetString(1),
                            EmailMessage = reader.GetString(2),
                            EmailDate = reader.GetDateTime(3),
                            EmailSender = reader.GetString(4),
                            EmailReceiver = reader.GetString(5)
                        };
                    }
                }

                string updateIsReadQuery = "UPDATE Emails SET EmailIsRead = 1 WHERE EmailId = @EmailId";
                SqlCommand command2 = new SqlCommand(updateIsReadQuery, connection);
                command2.Parameters.AddWithValue("@EmailId", id);
                command2.ExecuteNonQuery();
            }
        }
    }
}
