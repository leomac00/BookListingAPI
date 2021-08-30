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
    private HATEOAS.HATEOAS HATEOAS;
    public BooksController(AppDbContext db)
    {
      this.database = db;

      this.HATEOAS = new HATEOAS.HATEOAS("localhost:5001/api/v1/");
      this.HATEOAS.AddAction("Books/Get", "BOOK_INFO", "GET");
      this.HATEOAS.AddAction("Books/Update", "UPDATE_BOOK_INFO", "PATCH");
      this.HATEOAS.AddAction("Delete", "DELETE_BOOK", "DELETE");
      this.HATEOAS.AddAction("users/AddFavorite", "ADD_AS_FAVORITE", "POST");
      this.HATEOAS.AddAction("users/RemoveFavorite", "REMOVE_FAVORITE", "DELETE");
    }


    //POST
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

        database.Add(book);
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

    //GET
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
            Links = HATEOAS.GetActions(book.Id.ToString())
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

    [HttpGet("Get/{Id}")]
    public IActionResult Get(int Id)
    {
      try
      {
        var book = database.Books.Where(book => book.Status == true).First(b => b.Id == Id);
        var bookContainer = new BookContainer()
        {
          Book = book,
          Links = HATEOAS.GetActions(book.Id.ToString())
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


    //PATCH
    [Authorize]
    [HttpPatch("Update/{id}")]
    public IActionResult Patch([FromBody] BookDTO updBook, int id)
    {

      try
      {
        var book = database.Books.Find(id);
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
    [Authorize]
    [HttpDelete("Delete/{Id}")]
    public IActionResult Delete(int Id)
    {
      try
      {
        var book = database.Books.Find(Id);

        book.Status = false;
        database.SaveChanges();

        return Ok("Book removed from DataBase!");
      }
      catch
      {
        Response.StatusCode = 404;
        return new ObjectResult("Id wasn´t found.");
      }
    }
  }
}