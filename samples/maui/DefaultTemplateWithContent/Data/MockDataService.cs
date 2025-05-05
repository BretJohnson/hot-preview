namespace DefaultTemplateWithContent.Data;

/// <summary>
/// This is work in progress.
/// </summary>
public class MockDataService
{
    private DatabaseManager _databaseManager;
    private JsonDataService _jsonDataService;

    public MockDataService(DatabaseManager databaseManager, JsonDataService jsonDataService, ProjectRepository projectRepository,
        TaskRepository taskRepository, TagRepository tagRepository, CategoryRepository categoryRepository)
    {
        _databaseManager= databaseManager;
        _jsonDataService = jsonDataService;

        ProjectRepository = projectRepository;
        TaskRepository = taskRepository;
        TagRepository = tagRepository;
        CategoryRepository = categoryRepository;
    }

    public ProjectRepository ProjectRepository { get; private set; }
    public TaskRepository TaskRepository { get; private set; }
    public TagRepository TagRepository { get; private set; }
    public CategoryRepository CategoryRepository { get; private set; }

    public async Task ActivateAsync(string jsonFile)
    {
        await _databaseManager.SwitchToMockDataDatabaseAsync();
        await _jsonDataService.LoadJsonDataAsync(jsonFile);
    }
}
