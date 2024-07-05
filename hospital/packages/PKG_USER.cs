using hospital.models;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Numerics;
using System.Security.Cryptography;

namespace hospital.packages
{
    public interface IPKG_USERS
    {
        public void register_user(RegisterUser user);
        public UserDbo Login_user(LogIn user);
        public void Log_out(int id);
        public void delete_user(int id);
        public List<appointments> get_Doctors_appoitments(int id);
        public List<appointments> get_Patients_appoitments(int id);
        public void place_appointment(appointments appointments);
        public string create_otp(LogIn login);
        public bool check_otp(string email, string otp);
        public string create_otp_password_reset(ResetPasswordOTP resetPasswordOTP);
        public bool check_otp_password_reset(ResetPasswordOTP resetPasswordOTP);
        public void reset_password(LogIn resetPassword);
        public  List<Category> paginate_categoriess(int pageNumber, int rowsPerPpage);
        public void delete_category(int id);
        public void add_category(string profession);
        public List<Doctor> get_Doctors(int pageNumber, int rowsPerPpage);
        public List<Category> get_categories_eng();
        public List<Category> get_categories_geo();
        public List<CategoryDbo> get_categories();
        public List<Translate> translate(string language);






    }
    public class PKG_USER : PKG_BASE, IPKG_USERS
    {
        IConfiguration configuration;
        public PKG_USER(IConfiguration configuration) : base(configuration)
        {

        }

        public void register_user(RegisterUser user)
        {
            CreatePasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var photoBytes = GetFileBytes( user.Photo);
            var cvBytes = GetFileBytes(user.CV);


            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_user.registre_user";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_firstName", OracleDbType.Varchar2).Value = user.FirstName;
            cmd.Parameters.Add("p_lastName", OracleDbType.Varchar2).Value = user.LastName;
            cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = user.Email;
            cmd.Parameters.Add("p_personalID", OracleDbType.Varchar2).Value = user.Personalid;
            cmd.Parameters.Add("p_categorID", OracleDbType.Int32).Value = (user.Categorid.HasValue && user.Categorid.Value != 0) ? (object)user.Categorid.Value : DBNull.Value;
            cmd.Parameters.Add("p_photo", OracleDbType.Blob).Value = photoBytes ?? (object)DBNull.Value;
            cmd.Parameters.Add("p_CV", OracleDbType.Blob).Value = cvBytes ?? (object)DBNull.Value;
            cmd.Parameters.Add("p_roleId", OracleDbType.Int32).Value = user.Roleid;
            cmd.Parameters.Add("p_passwordHash", OracleDbType.Varchar2).Value = Convert.ToBase64String(passwordHash);
            cmd.Parameters.Add("p_passwordSalt", OracleDbType.Varchar2).Value = Convert.ToBase64String(passwordSalt);

            cmd.ExecuteNonQuery();

            conn.Close();
        }
        public UserDbo Login_user(LogIn user)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_user.login_user";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = user.Email;

            cmd.Parameters.Add("user_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            OracleDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                UserDbo User = new UserDbo();

                User.Id = int.Parse(reader["id"].ToString());
                User.FirstName = reader["firstname"].ToString();
                User.LastName = reader["lastname"].ToString();
                User.Email = reader["email"].ToString();
                User.Personalid = reader["personalid"].ToString();
                User.Categorid = reader["categorid"] == DBNull.Value ? (int?)null : int.Parse(reader["categorid"].ToString());
                User.Profession = reader["profession"].ToString();
                User.Photo = GetBytesFromOracleBlob(reader, "photo");
                User.CV = GetBytesFromOracleBlob(reader, "cv");
                User.Roleid = int.Parse(reader["roleid"].ToString());
                User.Role = reader["role"].ToString();
                User.PasswordHash = Convert.FromBase64String(reader["passwordhash"].ToString());
                User.PasswordSalt = Convert.FromBase64String(reader["passwordsalt"].ToString());

                if (VerifyPasswordHash(user.Password, User.PasswordHash, User.PasswordSalt))
                {
                    return User;
                }

            }
            return null;



        }
        public void Log_out(int id)
        {

            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_user.log_out";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_id", OracleDbType.Varchar2).Value = id;


            cmd.ExecuteNonQuery();

            conn.Close();

        }
        public void delete_user(int id)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_user.delete_user";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = id;

            cmd.ExecuteNonQuery();

            conn.Close();
        }


        public List<appointments> get_Doctors_appoitments(int id)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;
            List<appointments> appointments = new List<appointments>();


            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_appointments.get_appoitments_by_doctor";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_doctorID", OracleDbType.Int64).Value = id;

            cmd.Parameters.Add("apptm_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            OracleDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                appointments Appointment = new appointments();

                Appointment.Id = int.Parse(reader["id"].ToString());
                Appointment.DoctorID = int.Parse(reader["doctorid"].ToString());
                Appointment.PatientID = int.Parse(reader["patientid"].ToString());
                Appointment.StartDate = DateTime.Parse(reader["startdate"].ToString());
                Appointment.EndDate = DateTime.Parse(reader["enddate"].ToString());


                appointments.Add(Appointment);

            }
            return appointments;

        }

        public List<appointments> get_Patients_appoitments(int id)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;
            List<appointments> appointments = new List<appointments>();


            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_appointments.get_appoitments_by_patient";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_patientID", OracleDbType.Int64).Value = id;

            cmd.Parameters.Add("apptm_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            OracleDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                appointments Appointment = new appointments();

                Appointment.Id = int.Parse(reader["id"].ToString());
                Appointment.DoctorID = int.Parse(reader["doctorid"].ToString());
                Appointment.PatientID = int.Parse(reader["patientid"].ToString());
                Appointment.StartDate = DateTime.Parse(reader["startdate"].ToString());
                Appointment.EndDate = DateTime.Parse(reader["enddate"].ToString());

                appointments.Add(Appointment);

            }
            return appointments;

        }

        public void place_appointment(appointments appointments)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_appointments.place_appointment";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_startDate", OracleDbType.Date).Value = appointments.StartDate;
            cmd.Parameters.Add("p_endDate", OracleDbType.Date).Value = appointments.EndDate;
            cmd.Parameters.Add("p_doctorID", OracleDbType.Int32).Value = appointments.DoctorID;
            cmd.Parameters.Add("p_patientID", OracleDbType.Int32).Value = appointments.PatientID;

            cmd.ExecuteNonQuery();

            conn.Close();
        }

        public string create_otp(LogIn login)
        {
            Random random = new Random();
            int randomNumber = random.Next(1000, 10000);
            string Otp = randomNumber.ToString();

            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_2fa_authentication.create_otp";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_EMAIL", OracleDbType.Varchar2).Value = login.Email;
            cmd.Parameters.Add("p_otp", OracleDbType.Varchar2).Value = Otp;

            cmd.Parameters.Add("password_curs", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            OracleDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                byte[] PasswordHash = Convert.FromBase64String(reader["passwordhash"].ToString());
                byte[] PasswordSalt = Convert.FromBase64String(reader["passwordsalt"].ToString());

                if (VerifyPasswordHash(login.Password, PasswordHash, PasswordSalt))
                {
                    return Otp;
                }

            }


            cmd.ExecuteNonQuery();

            conn.Close();
            return "0";
        }
        public bool check_otp(string email, string otp)
        {

            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_2fa_authentication.check_otp";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_EMAIL", OracleDbType.Varchar2).Value = email;
            cmd.Parameters.Add("p_otp", OracleDbType.Varchar2).Value = otp;

            cmd.Parameters.Add("login_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            OracleDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string loginString = reader["login"].ToString().Trim();
                bool l = loginString == "1" || loginString.Equals("true", StringComparison.OrdinalIgnoreCase);
                return l;
            }
            cmd.ExecuteNonQuery();

            conn.Close();
            return false;

        }
        public string create_otp_password_reset(ResetPasswordOTP resetPasswordOTP)
        {
            Random random = new Random();
            int randomNumber = random.Next(10000, 100000);
            resetPasswordOTP.Otp = randomNumber.ToString();

            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_user.create_otp_password_reset";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_EMAIL", OracleDbType.Varchar2).Value = resetPasswordOTP.Email;
            cmd.Parameters.Add("p_otp", OracleDbType.Varchar2).Value = resetPasswordOTP.Otp;

            cmd.ExecuteNonQuery();

            conn.Close();
            return resetPasswordOTP.Otp;
        }
        public bool check_otp_password_reset(ResetPasswordOTP resetPasswordOTP)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_user.check_otp_password_reset";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_EMAIL", OracleDbType.Varchar2).Value = resetPasswordOTP.Email;
            cmd.Parameters.Add("p_otp", OracleDbType.Varchar2).Value = resetPasswordOTP.Otp;

            cmd.Parameters.Add("enable_password_reset_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            OracleDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string loginString = reader["enable_password_reset"].ToString().Trim();
                bool l = loginString == "1" || loginString.Equals("true", StringComparison.OrdinalIgnoreCase);
                return l;
            }
            cmd.ExecuteNonQuery();

            conn.Close();
            return false;

        }
        public void reset_password(LogIn resetPassword)
        {
            CreatePasswordHash(resetPassword.Password, out byte[] passwordHash, out byte[] passwordSalt);

            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_user.forgot_password";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_new_passwordHash", OracleDbType.Varchar2).Value = Convert.ToBase64String(passwordHash);
            cmd.Parameters.Add("p_new_passwordSalt", OracleDbType.Varchar2).Value = Convert.ToBase64String(passwordSalt);
            cmd.Parameters.Add("p_EMAIL", OracleDbType.Varchar2).Value = resetPassword.Email;

            cmd.ExecuteNonQuery();

            conn.Close();

        }
        public List<Category> paginate_categoriess(int pageNumber, int rowsPerPpage)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;
            List<Category> categories = new List<Category>();


            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_categories.paginate_categories";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_page_number", OracleDbType.Int32).Value = pageNumber;
            cmd.Parameters.Add("p_rows_per_page", OracleDbType.Int32).Value = rowsPerPpage;

            cmd.Parameters.Add("p_categ_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            OracleDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Category category = new Category();

                category.Id = int.Parse(reader["id"].ToString());
                category.Professin = reader["profession"].ToString();

                categories.Add(category);

            }
            return categories;

        }
        public void delete_category(int id)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_categories.delete_category";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = id;

            cmd.ExecuteNonQuery();

            conn.Close();

        }
        public void add_category(string profession)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;

            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_categories.add_category";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_profession", OracleDbType.Varchar2).Value = profession;

            cmd.ExecuteNonQuery();

            conn.Close();
        }
        public List<Doctor> get_Doctors(int pageNumber, int rowsPerPpage)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;
            List<Doctor> doctors = new List<Doctor>();


            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_doctors.paginate_doctors";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_page_number", OracleDbType.Int32).Value = pageNumber;
            cmd.Parameters.Add("p_rows_per_page", OracleDbType.Int32).Value = rowsPerPpage;

            cmd.Parameters.Add("p_categ_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            OracleDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Doctor doctor = new Doctor
                {
                    Id = int.Parse(reader["id"].ToString()),
                    FirstName = reader["firstname"].ToString(),
                    LastName = reader["lastname"].ToString(),
                    Email = reader["email"].ToString(),
                    Profession = reader["profession"].ToString(),
                    Personalid = reader["personalid"].ToString(),
                    Photo = GetBytesFromOracleBlob(reader, "photo"),
                    CV = GetBytesFromOracleBlob(reader, "cv")
                };

                doctors.Add(doctor);
            }
            return doctors;

        }
        public List<Category> get_categories_eng()
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;
            List<Category> categories = new List<Category>();


            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_categories.get_categories_eng";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_categ_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            OracleDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Category category = new Category();

                category.Id = int.Parse(reader["id"].ToString());
                category.Professin = reader["profession"].ToString();

                categories.Add(category);

            }
            return categories;

        }
        public List<Category> get_categories_geo()
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;
            List<Category> categories = new List<Category>();


            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_categories.get_categories_geo";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_categ_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            OracleDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Category category = new Category();

                category.Id = int.Parse(reader["id"].ToString());
                category.Professin = reader["profession_geo"].ToString();

                categories.Add(category);

            }
            return categories;

        }
        public List<CategoryDbo> get_categories()
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;
            List<CategoryDbo> categories = new List<CategoryDbo>();


            conn.Open();

            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning.pkg_marita_categories.get_categories";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_categ_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            OracleDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                CategoryDbo category = new CategoryDbo();

                category.Id = int.Parse(reader["id"].ToString());
                category.Profession = reader["profession"].ToString();
                category.profession_geo = reader["profession_geo"].ToString();


                categories.Add(category);

            }
            return categories;

        }
        public List<Translate> translate(string language)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = Connstr;
            List<Translate> translations = new List<Translate>();

            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "olerning. pkg_marita_translate.translate";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_language", OracleDbType.Varchar2).Value = language;
            cmd.Parameters.Add("p_curse", OracleDbType.RefCursor).Direction= ParameterDirection.Output;
            OracleDataReader reader = cmd.ExecuteReader();  
            while (reader.Read())
            {
                Translate translate = new Translate();
                translate.Code = reader["code"].ToString();
                translate.Word = reader["word"].ToString();

                translations.Add(translate);
            }
            return translations;


        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private byte[] GetFileBytes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();


            }
        }
        private byte[] GetBytesFromOracleBlob(OracleDataReader reader, string columnName)
        {
            var blob = reader.GetOracleBlob(reader.GetOrdinal(columnName));
            if (blob == null || blob.IsNull)
            {
                return null;
            }

            byte[] buffer = new byte[blob.Length];
            blob.Read(buffer, 0, buffer.Length);
            return buffer;
        }

   

    }



}
