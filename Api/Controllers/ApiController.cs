using Api.Data;
using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class ApiController(DatabaseContext db) : ControllerBase
{
    private readonly DatabaseContext _database = db;

    [HttpGet("all/{window}", Name = "GetTodos")]
    public async Task<IEnumerable<Todo>> GetAll(int window)
    {
        return await _database.Todos
            .Skip(50 * window)
            .Take(50)
            .ToArrayAsync();
    }

    [HttpGet("{guid}", Name = "GetTodo")]
    public async Task<Todo?> GetAll(Guid guid)
    {
        return await _database.Todos
            .FindAsync(guid);
    }

    [HttpPost("new", Name = "CreateTodo")]
    public async Task<Todo> CreateTodo([FromBody] Todo todo)
    {
        await _database.Todos.AddAsync(todo);
        await _database.SaveChangesAsync();

        return todo;
    }

    [HttpPatch("{guid}", Name = "UpdateTodo")]
    public async Task<Todo?> UpdateTodo([FromRoute] Guid guid, [FromBody] Todo todo)
    {
        var dbTodo = await _database.Todos.FindAsync(guid);

        if (dbTodo == null)
            return null;

        dbTodo.Title = todo.Title;
        dbTodo.Description = todo.Description;

        await _database.SaveChangesAsync();
        return dbTodo;
    }

    [HttpDelete("{guid}", Name = "DeleteTodo")]
    public async Task<Todo?> DeleteTodo(Guid guid)
    {
        var todo = await _database.Todos.FindAsync(guid);

        if (todo is null)
            return null;

        _database.Todos.Remove(todo);
        await _database.SaveChangesAsync();

        return todo;
    }
}