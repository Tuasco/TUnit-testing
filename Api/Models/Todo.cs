using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Api.Models;

[method: SetsRequiredMembers]
public class Todo(string title, string description)
{
    public required Guid Id { get; init; } = Guid.NewGuid();

    [MaxLength(100)]
    public required string Title { get; set; } = title;

    [MaxLength(100)]
    public required string Description { get; set; } = description;
}