using System.Collections.Generic;

namespace ExercicioAPI.HATEOAS
{
  public class HATEOAS
  {
    //Attributes
    private string url { get; set; }
    private string protocol = "https://";
    public List<Link> actions = new List<Link>();

    //Constructors
    public HATEOAS(string url)
    {
      this.url = url;
    }
    public HATEOAS(string url, string protocol)
    {
      this.protocol = protocol;
      this.url = url;
    }

    //Methods
    public void AddAction(string controller, string rel, string method)
    {
      actions.Add(new Link(this.protocol + this.url + controller, rel, method));
    }

    public Link[] GetActions(string sufix)
    {
      Link[] TLinks = new Link[actions.Count];
      for (int i = 0; i < actions.Count; i++)
      {
        TLinks[i] = new Link(actions[i].href, actions[i].rel, actions[i].method);
      }
      foreach (var TLink in TLinks)
      {
        TLink.href = TLink.href + "/" + sufix;
      }
      return TLinks;
    }
  }
}