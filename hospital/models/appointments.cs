namespace hospital.models
{
    public class appointments
    {
        public int Id {  get; set; }
        public int DoctorID { get; set; }
        public int PatientID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }


    }
}
