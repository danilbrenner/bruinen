namespace Bruinen.E2ETests;

public static class E2ESettings
{
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://auth.home.bruinen";

    public static string TestUsername =>
        Environment.GetEnvironmentVariable("E2E_USERNAME") ?? "e2e_test_user";

    public static string TestPassword =>
        Environment.GetEnvironmentVariable("E2E_PASSWORD") ?? "E2eTestPassword1!";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("E2E_CONNECTION_STRING")
            ?? "Host=localhost;Port=5437;Database=bruinen;Username=postgres;Password=postgres";
}
