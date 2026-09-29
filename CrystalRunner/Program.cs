using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace CrystalRunner
{
    public class DelphiPayload
    {
        public string ReportPath { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        public List<ReportParameter> Parameters { get; set; }
    }

    public class ReportParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string debugLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crystal_bridge_debug.txt");

            if (args.Length == 0)
            {
                File.WriteAllText(debugLog, "Error: No payload arguments received from Delphi.");
                return;
            }

            try
            {
                // 1. Combine and decode the Base64 JSON payload
                string base64String = string.Concat(args);
                byte[] dataBytes = Convert.FromBase64String(base64String);
                string jsonText = Encoding.UTF8.GetString(dataBytes);

                File.WriteAllText(debugLog, $"Decoded JSON:\n{jsonText}\n");

                DelphiPayload payload = JsonConvert.DeserializeObject<DelphiPayload>(jsonText);

                if (!File.Exists(payload.ReportPath))
                {
                    File.AppendAllText(debugLog, $"Error: RPT file missing at {payload.ReportPath}");
                    return;
                }

                // 2. Load the modern Crystal Reports .NET engine
                ReportDocument rpt = new ReportDocument();
                rpt.Load(payload.ReportPath);

                // 3. Inject SQL Server logon parameters
                foreach (IConnectionInfo connection in rpt.DataSourceConnections)
                {
                    connection.SetConnection(payload.ServerName, payload.DatabaseName, payload.UserID, payload.Password);
                }

                // 4. Inject report parameters
                if (payload.Parameters != null)
                {
                    foreach (var param in payload.Parameters)
                    {
                        if (!string.IsNullOrEmpty(param.Name))
                        {
                            rpt.SetParameterValue(param.Name, param.Value);
                        }
                    }
                }

                // 5. Generate a unique PDF destination path inside the system temp directory
                string tempPdfPath = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(payload.ReportPath)}_{Guid.NewGuid().ToString().Substring(0, 8)}.pdf");

                // 6. Silent export to PDF
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, tempPdfPath);

                // Clean up engine resources out of memory
                rpt.Close();
                rpt.Dispose();

                File.AppendAllText(debugLog, $"Successfully exported report to: {tempPdfPath}\nLaunching PDF viewer...");

                // 7. Fire an OS instruction to open the generated PDF in the user's default viewer
                Process.Start(new ProcessStartInfo(tempPdfPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                string errorLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crystal_FATAL_error.txt");
                File.WriteAllText(errorLog, ex.ToString());
            }
        }
    }
}
