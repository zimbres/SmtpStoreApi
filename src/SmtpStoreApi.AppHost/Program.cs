var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.SmtpStoreApi>("smtpstoreapi");

builder.AddProject<Projects.EmailDeliveryWorker>("emaildeliveryworker")
    .WithReference(api);

builder.AddProject<Projects.SmtpWebForm>("smtpwebform")
    .WithReference(api);

builder.AddContainer("smtp4aspire", "rnwood/smtp4dev").WithEndpoint(25, 25)
    .WithHttpEndpoint(8025, 80).WithContainerName("smtp4aspire");

builder.Build().Run();
