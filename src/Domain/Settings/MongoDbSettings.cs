using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Settings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string NotesCollectionName { get; set; } = "Notes";
    }
}
