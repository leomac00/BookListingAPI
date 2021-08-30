namespace ExercicioAPI.HATEOAS
{
  public class Link
  {
    public string href { get; set; }
    public string rel { get; set; }
    public string method { get; set; }

    public Link(string href, string rel, string method)
    {
      this.method = method;
      this.href = href;
      this.rel = rel;
    }
  }
}