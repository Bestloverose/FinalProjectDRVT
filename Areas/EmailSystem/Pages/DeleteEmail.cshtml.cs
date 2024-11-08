using FinalProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace FinalProject.Areas.EmailSystem.Pages
{
    public class DeleteEmailModel : PageModel
    {
        private readonly string _connectionString;

        public DeleteEmailModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // �纤�� EmailId ����ź
        [BindProperty]
        public int EmailId { get; set; }

        public IActionResult OnGet()
        {
            // �Ѻ��� EmailId �ҡ QueryString
            if (Request.Query.TryGetValue("EmailId", out StringValues emailId))
            {
                EmailId = int.Parse(emailId.ToString());
                return Page(); // �ʴ�˹�� DeleteEmail.cshtml �����������׹�ѹ���ź
            }

            // �������� EmailId ����Ѻ价��˹�� Index
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                String query = "DELETE FROM Emails WHERE EmailId = @EmailId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EmailId", EmailId);
                    await command.ExecuteNonQueryAsync();
                }
            }

            // ��ѧ�ҡź���� ����Ѻ价��˹�� Index
            return RedirectToPage("Index");
        }
    }
}
