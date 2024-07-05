using hospital.models;
using hospital.packages;
using hospital.service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Numerics;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace hospital.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class userController : ControllerBase
    {
        private readonly IPKG_USERS _package;
        private readonly IEmailService _emailService; 
        private readonly ITokenService _tokenService;
        private readonly IFileService _fileService;


        public userController(IPKG_USERS package, IEmailService emailService, ITokenService tokenService, IFileService fileService)
        {
            _package = package;
            _emailService = emailService;
            _tokenService = tokenService;
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }


        [HttpPost("register_user")]
        public async Task<IActionResult> Register_user([FromForm]RegisterUser user)
        {
            string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".txt" };

            try
            {
                if (user != null && user.Photo != null && user.Photo.Length > 0)
                {
                    var fileName = await _fileService.SaveFileAsync(user.Photo, allowedExtensions);
                }
               

                _package.register_user(user);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }
        [HttpPost("logIn")]
        public IActionResult Log_in(LogIn user)
        {

                UserDbo loggedInUser = _package.Login_user(user);
                if (loggedInUser == null)
                {
                    return Unauthorized("Invalid credentials");
                }

                var token = _tokenService.CreateToken(loggedInUser);

            return Ok(new
            {
                token,
                user = new
                {
                    loggedInUser.Id,
                    loggedInUser.FirstName,
                    loggedInUser.LastName,
                    loggedInUser.Email,
                    loggedInUser.Personalid,
                    loggedInUser.Profession,
                    loggedInUser.Photo,
                    loggedInUser.CV,
                    loggedInUser.Roleid,
                    loggedInUser.Role
                }
            });

        }
        [HttpPost("logOut")]
        public IActionResult Log_out(int id)
        {
            _package.Log_out(id);

            return Ok();

        }
        [HttpDelete("delete_user")]
        public IActionResult Delete_user([FromQuery] int id)
        {
            _package.delete_user(id);
            return Ok();

        }

        [HttpPost("send-email")]
        public async void SendEmail([FromBody] EmailSendDto emailSendDto)
        {
         
            await _emailService.SendEmailAsync(emailSendDto);
     
        }
        [HttpPost("create_otp")]
        public IActionResult Create_otp(LogIn login)
        {
            string otp = _package.create_otp(login);

           // EmailSendDto emailSendDto = new EmailSendDto(login.Email, "Test", otp);
           // _emailService.SendEmailAsync(emailSendDto);
            return Ok(otp);

        }
        [HttpPost("check_otp")]
        public IActionResult Check_otp(string email, string otp)
        {

            bool IsLogedIn = _package.check_otp( email,  otp);



            return Ok(IsLogedIn);

        }
        [HttpPost("create_otp_password_reset")]
        public IActionResult Create_otp_password_reset(ResetPasswordOTP resetPasswordOTP)
        {
            string otp = _package.create_otp_password_reset(resetPasswordOTP);

            // EmailSendDto emailSendDto = new EmailSendDto(resetPasswordOTP.Email, "Test", otp);
            // _emailService.SendEmailAsync(emailSendDto);
            return Ok(otp);

        }
        [HttpPost("check_otp_password_reset")]
        public IActionResult Check_otp_password_reset(ResetPasswordOTP resetPasswordOTP)
        {

            bool IsLogedIn = _package.check_otp_password_reset(resetPasswordOTP);



            return Ok(IsLogedIn);

        }
        [HttpPost("reset_password")]
        public IActionResult Reset_password(LogIn resetPassword)
        {

            _package.reset_password(resetPassword);

            return Ok();

        }

        [HttpPost("place_appoitments")]
        public IActionResult Place_appointment(appointments appointments)
        {
            _package.place_appointment(appointments);
            return Ok();

        }

        [HttpPost("doctor_appoitments")]
        public IActionResult Get_Doctors_appoitments( int id)
        {
            
            return Ok(_package.get_Doctors_appoitments( id));

        }

        [HttpPost("Patients_appointments"), Authorize]
        public IActionResult GetPatientsAppointments(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            if (id.ToString() != userIdClaim)
            {
                return Forbid();
            }


            return Ok(_package.get_Patients_appoitments(id));
   
        }

        [HttpGet("get_categories")]
        public IActionResult Get_categories([FromQuery]int pageNumber, int rowsPerPpage)
        {
            return Ok(_package.paginate_categoriess(pageNumber, rowsPerPpage));

        }
        [HttpDelete("delete_category")]
        public IActionResult Delete_category([FromQuery] int id)
        {
            _package.delete_category(id);
            return Ok();

        }
        [HttpPost("add_category")]
        public IActionResult Add_category([FromBody] string profession)
        {
            _package.add_category(profession);
            return Ok();

        }
        [HttpGet("get_Doctors"), Authorize]
        public IActionResult Get_Doctors([FromQuery] int pageNumber, int rowsPerPpage)
        {
            return Ok(_package.get_Doctors(pageNumber, rowsPerPpage));

        }
        [HttpGet("Get_Category_tr")]
        public IActionResult Get_Categories()
        {
            return Ok(_package.get_categories());

        }
        [HttpGet("get_categories_geo")]
        public IActionResult Get_categories_geo()
        {
            return Ok(_package.get_categories_geo());

        }
        [HttpGet("get_categories_eng")]
        public IActionResult Get_categories_eng()
        {
            return Ok(_package.get_categories_eng());

        }
        [HttpGet("translate")]
        public IActionResult Translate(string language)
        {
            return Ok(_package.translate(language));

        }




    }
}
