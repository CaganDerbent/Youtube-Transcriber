﻿namespace backend.Models
{
    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
