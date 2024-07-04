namespace hospital.models
{
    public class UserDbo
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Personalid { get; set; }
        public string? Password { get; set; }
        public int? Categorid { get; set; }
        public string? Profession { get; set; }
        public byte[]? Photo { get; set; }
        public byte[]? CV { get; set; }
        public int Roleid { get; set; }
        public bool? Emailconfiremed { get; set; }
        public bool? Twofactorenable { get; set; }
        public bool? Login { get; set; }
        public string? Role { get; set; }
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }


    }
}
