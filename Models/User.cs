using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExercicioAPI.Models
{
  public class User
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public bool Status { get; set; }
    //Navigation
    [JsonIgnore]
    public List<BookUser> BooksUsers { get; set; }
  }

  public class UserDTO
  {
    [StringLength(100, MinimumLength = 3, ErrorMessage = "User´s name should be beetwen {2} and {1} characters")]
    public string Name { get; set; }


    [Required]
    [DataType(DataType.EmailAddress, ErrorMessage = "Invalid e-Mail format.")]
    public string Email { get; set; }


    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "User´s password should be beetwen {2} and {1} characters")]
    public string Password { get; set; }
  }

  public class UserContainer
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
  }
}