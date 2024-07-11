namespace hospital.models
{
    public class RegisterUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Personalid { get; set; }
        public string? Password { get; set; } 
        public int? Categorid { get; set; } = null;
        public IFormFile? Photo { get; set; }
        public IFormFile? CV { get; set; }
        public int? Roleid { get; set; }
    }
}
