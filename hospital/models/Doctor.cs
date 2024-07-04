namespace hospital.models
{
    public class Doctor
    {
        public int? Id { get; set; } = null;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Profession { get; set; }
        public string? Personalid { get; set; }
        public byte[]? Photo { get; set; }
        public byte[]? CV { get; set; }
    }
}
