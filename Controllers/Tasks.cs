using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using prakt8_leshukov.Models;
using prakt8_leshukov.Services;
using System.Security.Claims;

namespace prakt8_leshukov.Controllers
{
    [Route("api/v1/tasks")]
    [Authorize]
    [ApiController]
    public class Tasks : ControllerBase
    {
        private LeshukovDbContext _db = DBService.Instance.Context;
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            return Ok(_db.Tasks.ToList());
        }
        [HttpGet]
        [Authorize(Roles = "Admin, User")]
        public IActionResult GetAllUser()
        {
            var user = User.Identity?.Name;
            return Ok(_db.Tasks.Where(o => o.OwnerLogin == user).ToList());
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, User")]
        public IActionResult GetOne(int id)
        {
            var user = User.Identity?.Name;
            if (User.IsInRole("User"))
            {

                var task = _db.Tasks.FirstOrDefault(t=>t.Id == id);
                if(task == null)
                {
                    return NotFound("Задача не найдена");
                }
                if(task.OwnerLogin != user) 
                {
                    return Forbid();
                }     
                return Ok(task);  
            }
            else
            {
                var task = _db.Tasks.FirstOrDefault(t => t.Id == id);
                if (task == null)
                {
                    return NotFound("Задача не найдена");
                }
                return Ok(task); 
            }
            
            
           
        }
        public record CreateOne(string title, string description);
        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public IActionResult Create(CreateOne taskCreate)
        {
            if (string.IsNullOrEmpty(taskCreate.title) || string.IsNullOrEmpty(taskCreate.description))
                return Conflict("Заполните поля");
            var task = new Models.Task();
            task.Id = 0;
            task.Title = taskCreate.title;
            task.Description = taskCreate.description;
            task.OwnerLogin = User.Identity?.Name;
            task.IsCompleted = false;
            _db.Tasks.Add(task);
            _db.SaveChanges();
            return Created(string.Empty, task);
        }
        public record PutOne(string title, string description, bool isCompleted);
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, User")]
        public IActionResult Put(int id, PutOne taskCreate)
        {
            var user = User.Identity?.Name;
            var task = _db.Tasks.FirstOrDefault(t=> t.Id == id);
           
            if (task == null)
                return NotFound("Задача отсутствует");
            if (User.IsInRole("User"))
            {
                if (task.OwnerLogin != user)
                    return Forbid();
                task.Description = taskCreate.description;
                task.Title = taskCreate.title;
                task.IsCompleted = taskCreate.isCompleted;
            }
            else
            {
                task.Description = taskCreate.description;
                task.Title = taskCreate.title;
                task.IsCompleted = taskCreate.isCompleted;
            }
            _db.SaveChanges();
            return Ok(task);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, User")]
        public IActionResult Delete(int id)
        {
            var user = User.Identity?.Name;
            var task = _db.Tasks.FirstOrDefault(t=> t.Id == id); 
            if (task == null)
                return NotFound("Задача отсутствует");
            if (User.IsInRole("User"))
            {
                if (task.OwnerLogin != user)
                    return Forbid();
                _db.Tasks.Remove(task);
            }
            else
            {
                _db.Tasks.Remove(task);
            }
            _db.SaveChanges();
            return NoContent();
        }
    }
}
