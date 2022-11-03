using Mark.HtmlToPdf;
using Mark.MarkdownToHtml;

namespace Mark.Web;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Contains("install"))
        {
            Microsoft.Playwright.Program.Main(new[]
            {
                "install",
                "chromium",
                "--with-deps"
            });

            return;
        }

        var program = new Program();
        program.RunApplication(args);
    }

    public void RunApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddRazorPages();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddTransient<IJobFileStorage, JobFileStorage>();
        builder.Services.AddTransient<IMarkdownConverter, PandocMarkdownConverter>();
        builder.Services.AddTransient<IPrinter, Printer>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.MapControllers();
        app.MapRazorPages();

        app.Run();
    }
}
