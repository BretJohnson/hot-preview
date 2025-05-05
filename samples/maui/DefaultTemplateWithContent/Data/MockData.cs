using DefaultTemplateWithContent.Models;

namespace DefaultTemplateWithContent.Data;

public class MockData
{
    public Project Project { get; private set; }
    public ProjectTask ProjectTask { get; private set; }

    public static MockData Activate()
    {
        IServiceProvider serviceProvider = (Application.Current?.Handler?.MauiContext?.Services) ??
            throw new InvalidOperationException("ServiceProvider is not available.");

        return new MockData(serviceProvider);
    }

    private MockData(IServiceProvider serviceProvider)
    {
        MockDataService mockDataService = serviceProvider.GetRequiredService<MockDataService>();

        Task.Run(async () =>
        {
            MockDataService mockDataService = serviceProvider.GetRequiredService<MockDataService>();
            await mockDataService.ActivateAsync("MockData.json");
            Project = (await mockDataService.ProjectRepository.ListAsync()).First();
            ProjectTask = Project.Tasks.First();
        }).GetAwaiter().GetResult();

        if (Project == null)
            throw new InvalidOperationException("No projects found.");
        if (ProjectTask == null)
            throw new InvalidOperationException("No project tasks found.");
    }
}
