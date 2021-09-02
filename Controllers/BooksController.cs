using System;
using System.Collections.Generic;
using System.Linq;
using ExercicioAPI.Data;
using ExercicioAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExercicioAPI.Controllers
{
  [ApiController]
  [Route("/api/v1/[controller]")]
  public class BooksController : ControllerBase
  {
    private readonly AppDbContext database;
    private HATEOAS.HATEOAS BooksHATEOAS;
    public BooksController(AppDbContext db)
    {
      this.database = db;

      this.BooksHATEOAS = new HATEOAS.HATEOAS("localhost:5001/api/v1/");
      this.BooksHATEOAS.AddAction("Books/Get", "BOOK_INFO", "GET");
      this.BooksHATEOAS.AddAction("Books/Update", "UPDATE_BOOK_INFO", "PATCH");
      this.BooksHATEOAS.AddAction("Books/Delete", "DELETE_BOOK", "DELETE");
      this.BooksHATEOAS.AddAction("Books/AddReading", "ADD_READING", "POST");
      this.BooksHATEOAS.AddAction("Books/GetComments", "GET_COMMENTS", "GET");
      this.BooksHATEOAS.AddAction("Books/GetRating", "GET_RATING", "GET");
    }


    //POST
    ///<summary>Insert new book to Database based on BODY information.</summary>
    ///<remarks>!!! This method requires the use of a token to allow the access of a valid user !!!</remarks>
    [Authorize]
    [HttpPost("AddBook")]
    public IActionResult AddBook(BookDTO bookDTO)
    {
      try
      {
        var book = new Book()
        {
          Name = bookDTO.Name,
          Author = bookDTO.Author,
          Pages = bookDTO.Pages,
          Status = true
        };

        database.Books.Add(book);
        database.SaveChanges();

        Response.StatusCode = 201;
        return Ok(new { Message = "The book >>> " + book.Name + " <<< was added to the library." });
      }
      catch
      {
        Response.StatusCode = 400;
        return new ObjectResult(new { Message = "We weren´t able to add the book to the database." });
      }
    }

    ///<summary>Add specified book based on ID given to current user´s ReadingList</summary>
    ///<remarks>!!! This method requires the use of a token to allow the access of a valid user !!!</remarks>
    [Authorize]
    [HttpPost("AddReading/{id}")]
    public IActionResult AddReading(int Id)
    {
      try
      {
        var bookId = Id;
        var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type.ToString().Equals("id", StringComparison.InvariantCultureIgnoreCase)).Value);

        if (database.BooksUsers.Where(bu => bu.BookId == bookId && bu.UserId == userId).ToList().Count > 0)
        {
          return BadRequest(new { Message = "Reading already existis in your account, try updating it or removing and creating a new one." });
        }
        else
        {
          var newReading = new BookUser()
          {
            BookId = bookId,
            Book = database.Books.First(x => x.Id == bookId),
            UserId = userId,
            User = database.Users.First(x => x.Id == userId),
            Favorite = false,
            Commentary = "",
            Finished = false,
            Rating = 5,
          };
          database.BooksUsers.Add(newReading);
          database.SaveChanges();

          Response.StatusCode = 200;
          return new ObjectResult(new { Message = "The book >>> " + newReading.Book.Name + " <<< was added to your readings!" });
        }

      }
      catch (Exception e)
      {
        return BadRequest(e.Message);
      }

    }

    //GET
    ///<summary>Get all active books from Database.</summary>
    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
      try
      {
        var books = database.Books.Where(book => book.Status == true).ToList();
        var bookContainersCollection = new List<BookContainer>();
        foreach (var book in books)
        {
          BookContainer bookContainer = new BookContainer()
          {
            Book = book,
            Links = BooksHATEOAS.GetActions(book.Id.ToString())
          };
          bookContainersCollection.Add(bookContainer);
        }

        return Ok(bookContainersCollection);
      }
      catch (Exception e)
      {
        Response.StatusCode = 400;
        return new ObjectResult(new { Message = e.Message });
      }
    }

    ///<summary>Get specified book from Database based on the ID informed.</summary>
    [HttpGet("Get/{Id}")]
    public IActionResult Get(int Id)
    {
      try
      {
        var book = database.Books.Where(book => book.Status == true).First(b => b.Id == Id);
        var bookContainer = new BookContainer()
        {
          Book = book,
          Links = BooksHATEOAS.GetActions(book.Id.ToString())
        };
        return Ok(bookContainer);
      }
      catch (Exception e)
      {
        Response.StatusCode = 400;
        return new ObjectResult(new
        {
          Message = e.Message
        });
      }
    }

    ///<summary>Get all comments from a specified book based on its ID.</summary>
    [HttpGet("GetComments/{id}")]
    public IActionResult GetComments(int Id)
    {
      try
      {
        var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type.ToString().Equals("id", StringComparison.InvariantCultureIgnoreCase)).Value);
        var booksUsers = database.BooksUsers.Where(bu => bu.BookId == Id).ToList();
        var comments = new List<CommentContainer>();


        foreach (var bookUser in booksUsers)
        {
          var comment = new CommentContainer()
          {
            UserName = database.Users.Find(bookUser.UserId).Name,
            Comment = bookUser.Commentary,
          };
          comments.Add(comment);
        }
        return !comments.All(c => c.Comment.Equals("")) ? Ok(comments) : Ok(new { Message = "This book currently have no comments." });
      }
      catch (Exception e)
      {
        Response.StatusCode = 400;
        return new ObjectResult(new { Message = e.Message });
      }
    }

    ///<summary>Get average user rating from a specified book based on its ID.</summary>
    [HttpGet("GetRating/{id}")]
    public IActionResult GetRating(int Id)
    {
      try
      {
        var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type.ToString().Equals("id", StringComparison.InvariantCultureIgnoreCase)).Value);
        var booksUsers = database.BooksUsers.Where(bu => bu.BookId == Id).ToList();
        var book = database.Books.Find(Id);

        if (booksUsers.Count == 0)
        {
          return Ok(new { Message = "The book >>> " + book.Name + " <<< has no ratings. yet" }); ;
        }
        else
        {

        }

        var ratings = new List<double>();
        foreach (var bookUser in booksUsers)
        {

          ratings.Add(bookUser.Rating);
        }
        double avgRating = ratings.Average();
        return Ok(new { Message = "The book >>> " + book.Name + " <<< has currently a rating of " + avgRating }); ;
      }
      catch (Exception e)
      {
        Response.StatusCode = 400;
        return new ObjectResult(new { Message = e.Message });
      }
    }

    //PATCH
    ///<summary>Update book´s info based on BODY information from a specified book based on its ID.</summary>
    ///<remarks>!!! This method requires the use of a token to allow the access of a valid user !!!</remarks>
    [Authorize]
    [HttpPatch("Update/{id}")]
    public IActionResult Update([FromBody] BookDTO updBook, int Id)
    {

      try
      {
        var book = database.Books.Where(b => b.Status).First(b => b.Id == Id);
        book.Name = updBook.Name;
        book.Author = updBook.Author;
        book.Pages = updBook.Pages;

        database.SaveChanges();

        return Ok(new { Message = "The book >>> " + book.Name + " <<< was updated successfully.", book = book });
      }
      catch (Exception e)
      {
        Response.StatusCode = 400;
        return new ObjectResult(new { Message = e.Message });
      }
    }

    //DELETE
    ///<summary>Boolean deletion of specified book based on its ID.</summary>
    ///<remarks>!!! This method requires the use of a token to allow the access of a valid user !!!</remarks>
    [Authorize]
    [HttpDelete("Delete/{Id}")]
    public IActionResult Delete(int Id)
    {
      try
      {
        var book = database.Books.Where(b => b.Status).First(b => b.Id == Id);

        book.Status = false;
        database.SaveChanges();

        return Ok(new { Message = "The book >>> " + book.Name + " <<< has been deleted." });
      }
      catch
      {
        Response.StatusCode = 404;
        return new ObjectResult(new { Message = "Id wasn´t found." });
      }
    }
  }
}