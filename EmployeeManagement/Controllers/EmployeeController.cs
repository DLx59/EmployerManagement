using EmployeeManagement.Data;
using EmployeeManagement.Data.Entities;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Controllers;

public class EmployeeController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly List<EmployeeModel> _employeeModels;

    public EmployeeController(AppDbContext appDbContext)
    {
        _dbContext = appDbContext;
        _employeeModels = ConvertEmpEntityToModel(_dbContext.Employees.ToList());
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Employees = ConvertEmpEntityToModel(await _dbContext.Employees.ToListAsync());
        ViewBag.TotalEmployee = ViewBag.Employees.Count;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmployeeModel model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Index));

        var employee = new Employee
        {
            Name = model.Name,
            Position = model.Position,
            Salary = model.Salary
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _dbContext.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        return View(new EmployeeModel
        {
            Id = employee.Id,
            Name = employee.Name,
            Position = employee.Position,
            Salary = employee.Salary
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EmployeeModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var employee = await _dbContext.Employees.FindAsync(model.Id);
            if (employee != null)
            {
                employee.Name = model.Name;
                employee.Position = model.Position;
                employee.Salary = model.Salary;
                _dbContext.Update(employee);
                await _dbContext.SaveChangesAsync();
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            if (EmployeeExists(model.Id))
                return NotFound();

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            NotFound();

        try
        {
            _dbContext.Employees.Remove(employee);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SearchByName(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            ViewBag.Employees = _employeeModels;
            ViewBag.TotalEmployee = ViewBag.Employees.Count;
            return View(nameof(Index));
        }

        ViewBag.Employees = ConvertEmpEntityToModel(
            await _dbContext.Employees.Where(e => e.Name.ToLower()
                .Contains(searchText)).ToListAsync());
        ViewBag.TotalEmployee = ViewBag.Employees.Count;
        return View(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ResetOverview()
    {
        ViewBag.Employees = ConvertEmpEntityToModel(await _dbContext.Employees.ToListAsync());
        ViewBag.TotalEmployee = ViewBag.Employees.Count;
        return View(nameof(Index));
    }

    private List<EmployeeModel> ConvertEmpEntityToModel(IEnumerable<Employee> employeesEntity)
    {
        return employeesEntity.Select(e => new EmployeeModel
        {
            Id = e.Id,
            Name = e.Name,
            Position = e.Position,
            Salary = e.Salary
        }).ToList();
    }

    private bool EmployeeExists(int id)
    {
        return _dbContext.Employees.Any(e => e.Id == id);
    }
}