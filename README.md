
# Flight Invoice Importer

A lightweight .NET Core Worker that automates the import of PDF-based airline invoices, matches them against your reservation database, and sends summarized reports by email.

---

## 🚀 Features

- **Background Processing**  
  Continuously watches a source folder (SFTP or local drop) and picks up new invoices.  
- **PDF Parsing & Matching**  
  Extracts flight/date/seat-count lines and matches them to reservations in PostgreSQL.  
- **Error Handling & Audit**  
  Invalid, duplicate, or price-mismatch records are logged, moved to separate folders, and recorded in an audit table.  
- **Email Reporting**  
  Sends a structured HTML summary plus CSV attachments for unmatched, duplicate, or mismatch rows.  
- **Configurable via `appsettings.json`**  
  One file controls folder paths, SMTP credentials, and log settings—no code changes required.

---

## ⚙️ Configuration

Copy and adjust `appsettings.json` (or override via environment-specific files):

> **Note:**  
> Be sure to update the `DefaultConnection` string under `ConnectionStrings` with your own PostgreSQL database details.

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=YourDb;Username=postgres;Password=YourPassword;Pooling=true;"
  },
  "FileStorage": {
    "SourceDirectory":     "C:\\invoices_sftp",     // where new files arrive
    "TargetRootDirectory": "C:\\invoices"           // root for incoming, processed, error
  },
  "MailSettings": {
    "Host":       "smtp.gmail.com",
    "Port":       587,
    "User":       "your.email@example.com",
    "Password":   "<secure-app-password>",
    "FromEmail":  "no-reply@example.com",
    "Recipients": [ "ops@example.com", "finance@example.com" ]
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path":                   "C:\\invoice_logs\\log_.txt",
          "rollingInterval":        "Infinite",
          "rollOnFileSizeLimit":    true,
          "fileSizeLimitBytes":     5242880,
          "retainedFileCountLimit": 4
        }
      }
    ]
  }
}
```

**Folder structure under** `TargetRootDirectory`:  
- `incoming` → staged for processing  
- `processed` → successfully imported files  
- `error`     → faulty or unsupported files  

---

## ▶️ Quick Start

1. **Clone and build the project**
    ```bash
    git clone https://github.com/your-org/odeon-flight-invoice-importer.git
    cd odeon-flight-invoice-importer
    dotnet build
    ```

2. **Edit configuration**
    - Update `appsettings.json` with your folder paths, **connection string** (under `ConnectionStrings`), SMTP credentials, and log settings.

3. **Run the worker**
    ```bash
    dotnet run --project FlightInvoiceImporter.Worker
    ```

4. **Log**
    - Watch the console output or tail the log file at `C:\\invoice_logs\\log_20250708.txt`.

5. **Test**
    - Drop a PDF into your `SourceDirectory`  
    - It will appear in `incoming`, then move to `processed` or `error`.

---

## 📖 Further Reading

**Design & Architecture:**
- **Core Principles:** SOLID
- **Design Patterns:** Factory, Repository, Dependency Injection
- **Extensibility**: Add new invoice formats, custom validation rules, or alternative email transports  

---

OFII © 2025 – All rights reserved.  
