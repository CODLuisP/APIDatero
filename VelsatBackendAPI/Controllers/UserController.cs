using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _unitOfWork.UserRepository.GetAllUsers());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            return Ok(await _unitOfWork.UserRepository.GetDetails(id));
        
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody]Account account)
        {
            if(account == null)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _unitOfWork.UserRepository.InsertUser(account);

            return Created("Created", created);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] Account account)
        {
            if (account == null)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _unitOfWork.UserRepository.UpdateUser(account);

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await _unitOfWork.UserRepository.DeleteUser(new Account { AccountID = id });

            return NoContent();
        }
    }
}
