namespace Domain.Models;

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string Speciality { get; set; } = string.Empty;
    public bool IsExpelled { get; set; }
    public int Sort { get; set; }
}