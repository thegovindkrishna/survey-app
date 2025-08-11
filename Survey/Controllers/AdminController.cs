using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Survey.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/admin/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        // POST: api/admin/users/{id}/promote
        [HttpPost("{id}/promote")]
        public async Task<IActionResult> PromoteToAdmin(int id)
        {
            var result = await _userService.PromoteToAdmin(id);
            if (!result)
                return NotFound("User not found");
            return Ok("User promoted to admin");
        }

        // DELETE: api/admin/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUser(id);
            if (!result)
                return NotFound("User not found");
            return Ok("User deleted");
        }
    }
}
