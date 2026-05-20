namespace WebApp.Entities;

public class Apartment
{
    public int Id { get; set; }
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string ZipCode { get; set; } = "";

    public ICollection<Employee> Employees { get; set; } = [];
}
