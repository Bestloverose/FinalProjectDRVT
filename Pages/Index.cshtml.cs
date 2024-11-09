using FinalProject.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using System.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace FinalProject.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly string _connectionString;

        public List<EmailInfo> listEmails = new List<EmailInfo>();

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void OnGet()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string username = "";
                    if (User.Identity?.Name == null)
                    {
                        username = "";
                    } else
                    {
                        username = User.Identity.Name;
                    }

                    String sql = "SELECT * FROM emails WHERE EmailReceiver='"+username+"'";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                EmailInfo emailInfo = new EmailInfo();
                                emailInfo.EmailID = reader.GetInt32(0);
                                emailInfo.EmailSubject = reader.GetString(1);
                                emailInfo.EmailMessage = reader.GetString(2);
                                emailInfo.EmailDate = reader.GetDateTime(3);
                                emailInfo.EmailIsRead = reader.GetBoolean(4);
                                emailInfo.EmailSender = reader.GetString(5);
                                emailInfo.EmailReceiver = reader.GetString(6);

                                listEmails.Add(emailInfo);
                            }
                        }
                    }

                    Console.WriteLine(listEmails);
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
    public class EmailInfo
    {
        public int EmailID;
        public String? EmailSubject;
        public String? EmailMessage;
        public DateTime EmailDate;
        public bool EmailIsRead;
        public String? EmailSender;
        public String? EmailReceiver;
    }

}
