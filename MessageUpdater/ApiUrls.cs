namespace MessageUpdater;

public static class ApiUrls
{
    public static string RoomerStatusApiUrl(int externalId)
    {
        #if DEBUG
            return $"http://192.168.2.71:8080/student/dormitories/api/v1/roomers/status/{externalId}";
        #else
            return $"https://system.fgoupsk.ru/student/dormitories/api/v1/roomers/status/{externalId}";
        #endif
    }
}