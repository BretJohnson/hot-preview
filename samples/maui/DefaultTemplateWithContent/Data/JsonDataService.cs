using System.Text.Json;
using DefaultTemplateWithContent.Models;
using Microsoft.Extensions.Logging;

namespace DefaultTemplateWithContent.Data;
public class JsonDataService
{
    private readonly DatabaseManager _databaseManager;
    private readonly ProjectRepository _projectRepository;
    private readonly TaskRepository _taskRepository;
    private readonly TagRepository _tagRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly ILogger<JsonDataService> _logger;

    public JsonDataService(DatabaseManager databaseManager, ProjectRepository projectRepository, TaskRepository taskRepository, TagRepository tagRepository,
        CategoryRepository categoryRepository, ILogger<JsonDataService> logger)
    {
        _databaseManager = databaseManager;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _tagRepository = tagRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task LoadJsonDataAsync(string jsonFilePath)
    {
        await using Stream templateStream = await FileSystem.OpenAppPackageFileAsync(jsonFilePath);

        ProjectsJson? payload = null;
        try
        {
            payload = JsonSerializer.Deserialize(templateStream, JsonContext.Default.ProjectsJson);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deserializing seed data");
        }

        try
        {
            if (payload is not null)
            {
                foreach (var project in payload.Projects)
                {
                    if (project is null)
                    {
                        continue;
                    }

                    if (project.Category is not null)
                    {
                        await _categoryRepository.SaveItemAsync(project.Category);
                        project.CategoryID = project.Category.ID;
                    }

                    await _projectRepository.SaveItemAsync(project);

                    if (project?.Tasks is not null)
                    {
                        foreach (var task in project.Tasks)
                        {
                            task.ProjectID = project.ID;
                            await _taskRepository.SaveItemAsync(task);
                        }
                    }

                    if (project?.Tags is not null)
                    {
                        foreach (var tag in project.Tags)
                        {
                            await _tagRepository.SaveItemAsync(tag, project.ID);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving seed data");
            throw;
        }
    }

    private async void ClearTables()
    {
        try
        {
            await _databaseManager.ClearTablesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
