using System.ComponentModel.DataAnnotations;
using ExercicioAPI.HATEOAS;

namespace ExercicioAPI.Models
{
  public class Book
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public int Pages { get; set; }
    public string Author { get; set; }
    public bool Status { get; set; }
  }

  public class BookDTO
  {
    [Range(int.MinValue, int.MaxValue, ErrorMessage = "Please enter valid integer number")]
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Book´s name should be beetwen {2} and {1} characters")]
    public string Name { get; set; }

    [Required]
    [Range(0, 10000000, ErrorMessage = "Book´s page quantity should be beetwen {2} and {1} characters")]
    public int Pages { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Book´s name should be beetwen {2} and {1} characters")]
    public string Author { get; set; }
  }

  public class BookContainer
  {
    public Book Book { get; set; }
    public Link[] Links { get; set; }
  }
}