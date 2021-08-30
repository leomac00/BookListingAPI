using System.Linq;
using ExercicioAPI.Data;
using ExercicioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExercicioAPI.Controllers
{
  [ApiController]
  [Route("/api/v1/[controller]")]
  public class BooksUsersController : ControllerBase
  {
    private readonly AppDbContext database;
    public BooksUsersController(AppDbContext context)
    {
      this.database = context;
    }

    /*
    //Test function for getting all BookUser data is below 
    [HttpGet]
    public IActionResult GetAll()
    {
      try
      {
        var list = database.BooksUsers
        .Include(bu => bu.Book)
        .Include(bu => bu.User)
        .ToList();
        return Ok(list);
      }
      catch (Exception e)
      {
        Response.StatusCode = 400;
        return new ObjectResult(new { Message = e.Message });
      }
    }
    */
  }
}