using TUnit.Aspire;

namespace TriTrainer.PlaywrightTests;

/// <summary>
/// Aspire fixture that starts the full AppHost stack (web, api, postgres)
/// before running Playwright smoke tests.
/// </summary>
public class AppFixture : AspireFixture<Projects.TriTrainer_AppHost>
{
    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
    }
}
