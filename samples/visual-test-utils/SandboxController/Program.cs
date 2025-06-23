using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using Sharprompt;
using SandboxServer.Services;
using SandboxServer.ViewModels;
using System.Net.NetworkInformation;
using System.Net;

Console.WriteLine("SandboxController");

Ioc.Default.ConfigureServices(
    new ServiceCollection()
        .AddLogging((factory) =>
        {
            factory.AddConsole();
        })
        .AddSingleton<IErrorHandlerService>(new ServerErrorHandler())
        .AddSingleton<IAppDispatcher>(new AppDispatcher())
        .AddSingleton<MainViewModel>()
        .BuildServiceProvider());

MainViewModel vm = Ioc.Default.GetService<MainViewModel>()!;

await vm.OnLoad();

/*
NetworkInterface selectedInterface = Prompt.Select("Select Network Interface", vm.NetworkInterfaces, textSelector: (netInterface)
    => $"{netInterface.Name} - {netInterface.GetIPProperties().UnicastAddresses.Where(y => y.Address.AddressFamily == AddressFamily.InterNetwork).Select(y => y.Address.ToString()).FirstOrDefault() ?? "(Empty)"}");

Console.WriteLine(selectedInterface.Name);

var port = Prompt.Input<int>("Enter Port Number", 8888);

Console.WriteLine(port);

vm.SelectedInterface = selectedInterface;
vm.Port = port;
*/

await vm.StartServerAsync(IPAddress.Loopback, 8888);

Console.WriteLine("Press enter to send a ping; press any other key to exit.");

int counter = 1;
while (true)
{
    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

    if (keyInfo.Key != ConsoleKey.Enter)
    {
        break;
    }

    string senderName = $"ping{counter}";
    Console.WriteLine($"Sending ping: {senderName}...");
    string response = await vm.Controller!.AppService.PingAsync();
    Console.WriteLine($"Ping response: {response}");
    ++counter;
}

