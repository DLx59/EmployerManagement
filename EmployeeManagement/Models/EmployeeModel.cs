using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models;

public class EmployeeModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public double Salary { get; set; }
}