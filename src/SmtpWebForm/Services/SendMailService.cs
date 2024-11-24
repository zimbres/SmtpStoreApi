namespace SmtpWebForm.Services;

internal class SendMailService
{
    public string SendMail(
        string smtpHost,
        int port,
        string username,
        string password,
        string from,
        string to,
        string subject,
        string body,
        bool enableSsl)

    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(from, from));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = string.IsNullOrEmpty(subject) ? "No Subject" : subject;
        message.Body = new TextPart("html")
        {
            Text = string.IsNullOrEmpty(body) ? "No Body" : body
        };

        using var logStream = new MemoryStream();
        using var smtpClient = new SmtpClient(new ProtocolLogger(logStream));
        try
        {
            smtpClient.Connect(smtpHost, port, enableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None);
            if (username != "")
            {
                smtpClient.Authenticate(username, password);
            }
            smtpClient.Send(message);
            logStream.Position = 0;
            using var reader = new StreamReader(logStream);
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}