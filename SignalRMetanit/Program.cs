using SignalRApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(); //добавл€ет сервисы SignalR

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<ChatHub>("/chat"); //маршрут дл€ хаба „ат’аб

app.Run();