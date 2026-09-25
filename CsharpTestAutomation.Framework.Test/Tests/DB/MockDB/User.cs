using Newtonsoft.Json;

namespace CsharpTestAutomation.Framework.Test.Tests.DB.MockDB;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string SirName { get; set; }
    public string Title { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Email { get; set; }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var comparedUserObject = (User)obj;

        var result =
            Id == comparedUserObject.Id &&
            FirstName == comparedUserObject.FirstName &&
            SirName == comparedUserObject.SirName &&
            Title == comparedUserObject.Title &&
            Country == comparedUserObject.Country &&
            City == comparedUserObject.City &&
            Email == comparedUserObject.Email;

        return result;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, FirstName, SirName, Email);
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
