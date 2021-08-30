using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using ExercicioAPI.Data;
using ExercicioAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace ExercicioAPI.Controllers
{
  [ApiController]
  [Route("/api/v1/[controller]")]
  public class UsersController : ControllerBase
  {
    private readonly AppDbContext database;
    private HATEOAS.HATEOAS HATEOAS;

    public UsersController(AppDbContext context)
    {
      this.database = context;

      this.HATEOAS = new HATEOAS.HATEOAS("localhost:5001/api/v1/");
      this.HATEOAS.AddAction("Books/Get", "BOOK_INFO", "GET");
      this.HATEOAS.AddAction("Books/Update", "UPDATE_BOOK_INFO", "PATCH");
      this.HATEOAS.AddAction("Delete", "DELETE_BOOK", "DELETE");
      this.HATEOAS.AddAction("users/RemoveFavorite", "REMOVE_FAVORITE", "DELETE");
    }
    //POST
    [HttpPost("Register")]
    public IActionResult Register([FromBody] UserDTO userDTO)
    {
      try
      {
        var userFound = database.Users.Any(u => u.Email == userDTO.Email);
        if (!userFound)
        {
          var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);
          var user = new User()
          {
            Name = userDTO.Name,
            Email = userDTO.Email,
            Password = hashedPassword,
            Status = true
          };
          database.Users.Add(user);
          database.SaveChanges();
          Response.StatusCode = 201;
          return new ObjectResult(new { Message = "Success!", newUser = new { name = user.Name, email = user.Email } });
        }
        else
        {
          Response.StatusCode = 400;
          return new ObjectResult(new { Message = "User e-Mail already exists!" });
        }
      }
      catch (Exception e)
      {
        Response.StatusCode = 400;
        return new ObjectResult(new { Message = e.Message });
      }
    }

    [HttpPost("Login")]
    public IActionResult Login([FromBody] UserDTO userDTO)
    {
      User user;
      try
      {
        user = database.Users.First(u => u.Email == userDTO.Email);
        if (user != null && user.Status == true)
        {
          bool validPassword = BCrypt.Net.BCrypt.Verify(userDTO.Password, user.Password);
          if (validPassword)
          {
            int hoursValid = 1;

            //Encrypting security key
            string securityKey = "my_second_API-Now_about_BOOKS!";
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var encryptedCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

            //Claims
            var userClaims = new List<Claim>();
            userClaims.Add(new Claim("id", user.Id.ToString()));


            //Creating JWT
            var JWT = new JwtSecurityToken(
            issuer: "BookAPI",
            expires: DateTime.Now.AddHours(hoursValid),
            audience: "common_user",
            signingCredentials: encryptedCredentials,
            claims: userClaims
          );
            var token = new JwtSecurityTokenHandler().WriteToken(JWT);
            return Ok(new { Message = "Your token is valid for the next [ " + hoursValid + " ] Hour(s)", Token = token });
          }
          else
          {
            Response.StatusCode = 401;
            return new ObjectResult(new { Message = "Incorrect Password." });
          }
        }
        else
        {
          Response.StatusCode = 401;
          return new ObjectResult(new { Message = "User not found." });
        }
      }
      catch (Exception e)
      {
        return new ObjectResult(new { Message = e.Message });
      }


    }

    [Authorize]
    [HttpPost("AddFavorite/{id}")]
    public IActionResult AddFavorite(int Id)
    {
      try
      {
        var bookId = Id;
        var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type.ToString().Equals("id", StringComparison.InvariantCultureIgnoreCase)).Value);

        var newFavBook = new BookUser()
        {
          BookId = bookId,
          Book = database.Books.First(x => x.Id == bookId),
          UserId = userId,
          User = database.Users.First(x => x.Id == userId)
        };
        database.BooksUsers.Add(newFavBook);
        database.SaveChanges();

        Response.StatusCode = 200;
        return new ObjectResult(new { Message = "The book >>> " + newFavBook.Book.Name + " <<< added to favorites!" });
      }
      catch (Exception e)
      {
        return BadRequest(e.Message);
      }

    }

    //GET
    [Authorize]
    [HttpGet("GetFavorites")]
    public IActionResult GetFavorites()
    {
      try
      {
        var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type.ToString().Equals("id", StringComparison.InvariantCultureIgnoreCase)).Value);
        var booksUsers = database.BooksUsers.Where(bu => bu.UserId == userId).ToList();
        var favBooksContainer = new List<BookContainer>();


        foreach (var bookUser in booksUsers)
        {
          var book = database.Books.First(b => b.Id == bookUser.BookId);
          var bookContainer = new BookContainer
          {
            Book = book,
            Links = HATEOAS.GetActions(book.Id.ToString())
          };

          favBooksContainer.Add(bookContainer);
        }
        return favBooksContainer.Count > 0 ? Ok(favBooksContainer) : Ok(new { Message = "You dont have any favorite books yet. " });
      }
      catch (Exception e)
      {
        Response.StatusCode = 400;
        return new ObjectResult(new { Message = e.Message });
      }
    }

    [Authorize]
    [HttpGet("GetUsers")]
    public IActionResult GetUsers()
    {
      try
      {
        var usersContainer = new List<UserContainer>();
        var users = database.Users.Where(u => u.Status == true).ToList();
        foreach (var user in users)
        {
          var userContainer = new UserContainer()
          {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
          };

          usersContainer.Add(userContainer);
        }
        return usersContainer.Count > 0 ? new ObjectResult(usersContainer) : new ObjectResult(new { Message = "No users were found" });
      }
      catch (Exception e)
      {
        Response.StatusCode = 400;
        return new ObjectResult(new { Message = e.Message });
      }
    }

    //PATCH
    [Authorize]
    [HttpPatch("UpdateMe")]
    public IActionResult UpdateMe([FromBody] UserDTO userDTO)
    {
      try
      {
        var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type.ToString().Equals("id", StringComparison.InvariantCultureIgnoreCase)).Value);
        var user = database.Users.Find(userId);
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);

        user.Password = hashedPassword;
        user.Email = userDTO.Email;
        user.Name = userDTO.Name;

        database.SaveChanges();
        return Ok(new { Message = "User >>> " + user.Name + " <<< was updated successfully." });
      }
      catch (Exception e)
      {
        Response.StatusCode = 400;
        return new ObjectResult(new { Message = "An error has occurred while updating this user.", Error = e.Message });
      }
    }

    //DELETE
    [Authorize]
    [HttpDelete("DeleteMe")]
    public IActionResult DeleteMe()
    {
      try
      {
        var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type.ToString().Equals("id", StringComparison.InvariantCultureIgnoreCase)).Value);

        var user = database.Users.Find(userId);
        user.Status = false;
        database.SaveChanges();
        return Ok(new { Message = "User >>> " + user.Name + " <<< was deleted successfully." });
      }
      catch (Exception e)
      {
        return new ObjectResult(new { Message = "An error has occurred while deleting this user.", Error = e.Message });
      }
    }

    [Authorize]
    [HttpDelete("RemoveFavorite/{id}")]
    public IActionResult RemoveFavorite(int Id)
    {
      try
      {
        var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type.ToString().Equals("id", StringComparison.InvariantCultureIgnoreCase)).Value);
        var user = database.Users.Find(userId);
        var book = database.Books.Find(Id);
        var bookUser = database.BooksUsers.First(b => b.BookId == Id);

        user.BooksUsers.Remove(bookUser);
        database.SaveChanges();

        return Ok(new { Message = "The book >>> " + book.Name + " <<< was removed from favorites" });
      }
      catch (Exception e)
      {
        Response.StatusCode = 400;
        return new ObjectResult(new { Message = e.Message });
      }

    }
  }
}