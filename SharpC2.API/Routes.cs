namespace SharpC2.API;

public static class Routes
{
    public static class V1
    {
        public const string Authentication = "/api/v1/auth";
        public const string Handlers = "/api/v1/handlers";
        public const string HostedFiles = "/api/v1/hosted_files";
        public const string Drones = "/api/v1/drones";
        public const string Tasks = "/api/v1/tasks";
        public const string Payloads = "/api/v1/payloads";
        public const string Events = "/api/v1/events";
        public const string Pivots = "/api/v1/pivots";

        //public const string ReversePortForwards = "/api/v1/fportfwds";
        //public const string Socks = "/api/v1/socks";
    }
}