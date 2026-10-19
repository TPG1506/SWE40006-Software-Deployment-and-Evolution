using Microsoft.AspNetCore.Mvc;
using TaskTracker.Models;

namespace TaskTracker.Controllers;

public class TasksController : Controller
{
    private static readonly List<TaskItem> _tasks = new()
    {
        new TaskItem { Id = 1, Title = "Create Azure account", IsDone = true },
        new TaskItem { Id = 2, Title = "Deploy my C# app", DueDate = DateTime.Today.AddDays(-1) },
        new TaskItem { Id = 3, Title = "Complete report", Notes = "Include all recent updates", DueDate = DateTime.Today }
    };
    private static int _nextId = 4;

    public IActionResult Index() =>
        View(_tasks.OrderBy(t => t.IsDone).ThenBy(t => t.DueDate));

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(TaskItem task)
    {
        if (!ModelState.IsValid) return View(task);
        task.Id = _nextId++;
        _tasks.Add(task);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Toggle(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task != null) task.IsDone = !task.IsDone;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _tasks.RemoveAll(t => t.Id == id);
        return RedirectToAction(nameof(Index));
    }
}