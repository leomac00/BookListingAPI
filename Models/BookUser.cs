using System.ComponentModel.DataAnnotations;
using ExercicioAPI.HATEOAS;

namespace ExercicioAPI.Models
{
  public class BookUser
  {
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public bool Finished { get; set; }
    public int Rating { get; set; }
    public string Commentary { get; set; }
    public bool Favorite { get; set; }
  }

  public class Reading
  {
    public Book Book { get; set; }
    public bool Favorite { get; set; }
    public bool Finished { get; set; }
    [Range(typeof(int), "0", "5", ErrorMessage = "Rating should be between 0 and 5.")]
    public int Rating { get; set; }
    [StringLength(280, ErrorMessage = "Commentary should have a maximum of 280 characters.")]
    public string Commentary { get; set; }
  }
  public class ReadingUpdateTransferObj
  {
    public bool Favorite { get; set; }
    public bool Finished { get; set; }
    [Range(typeof(int), "0", "5", ErrorMessage = "Rating should be between 0 and 5.")]
    public int Rating { get; set; }
    [StringLength(280, ErrorMessage = "Commentary should have a maximum of 280 characters.")]
    public string Commentary { get; set; }
  }
  public class ReadingContainer
  {
    public Reading Reading { get; set; }
    public Link[] Links { get; set; }
  }
  public class CommentContainer
  {
    public string UserName { get; set; }
    public string Comment { get; set; }
  }
}