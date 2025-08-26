using System.Collections.Generic;

namespace TodoWeb.Models;

public record TodoViewModel(List<TodoItem> Items);