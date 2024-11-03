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

        public async Task<IActionResult> OnGet()
        {
            Request.Query.TryGetValue("EmailId", out StringValues emailId);
            String idString = emailId.ToString();
            int id = int.Parse(idString);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                String query = "DELETE FROM Emails WHERE EmailId = @EmailId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EmailId", id);
                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage("Index");
        }
    }
}
