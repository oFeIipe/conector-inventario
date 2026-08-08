using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Management;
using System.Net;
using Formatting = Newtonsoft.Json.Formatting;
class Program
{
    static void Main()
    {
        var inventario = MontarInventario();
        EnviarRequisicao(inventario);
    }
    public static InventarioDTO MontarInventario()
    {
        DateTime localDate = GetDate();

        Notebook note = GetNotebook(localDate);
        List<SoftwareInstalado> installedSoftwares = SoftwareInstalado.GetInstalledSoftwares(note.NotebookId, localDate);
        Localizacao local = GetLocalizacao(note.NotebookId, localDate);

        return new InventarioDTO
        {
            Notebook = note,
            Localizacao = local,
            SoftwaresInstalados = installedSoftwares,
        };
    }
    public static void EnviarRequisicao(InventarioDTO inventario)
    {
        string inventarioJson = JsonConvert.SerializeObject(inventario, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        });
        try
        {
            
            /*
                Regra de envio removida
            */

            string jsons = JsonConvert.SerializeObject(inventario, Formatting.Indented);
            Console.WriteLine(jsons);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao enviar inventário: " + ex.Message);
        }
    }
    public static Localizacao GetLocalizacao(string notebookId, DateTime data)
    {
        LocationAPI local = new();
        try
        {
            using var client = new WebClient();
            var locationJson = client.DownloadString("http://ip-api.com/json/");
            local = JsonConvert.DeserializeObject<LocationAPI>(locationJson);
        }
        catch (WebException ex)
        {
            local.City = "Desconhecido";
            local.Lat = 0;
            local.Lon = 0;
        }

        return new Localizacao
        {
            Lat = local.Lat,
            Long = local.Lon,
            LocalIdentifier = local.City ?? "Desconhecido",
            DeviceId = notebookId,
            DataLog = data
        };
    }
    public static Notebook GetNotebook(DateTime data)
    {
        Notebook note = new();

        try
        {
            var cpuSearcher = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (var item in cpuSearcher.Get())
            {
                note.CPU = item["Name"].ToString() ?? "--";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter informações de nome do notebook: " + ex.Message);
            note.CPU = "--";
        }


        try
        {
            var lastBootSearcher = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            foreach (var item in lastBootSearcher.Get())
            {
                note.Inicializacao = item["LastBootUpTime"] != null ? ManagementDateTimeConverter.ToDateTime(item["LastBootUpTime"].ToString()).ToString("dd/MM/yyyy HH:mm:ss") : "--";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter informações de boot do notebook: " + ex.Message);
            note.Inicializacao = "--";
        }

        try
        {
            var biosSearcher = new ManagementObjectSearcher("select * from Win32_BIOS");
            foreach (var item in biosSearcher.Get())
            {
                note.BIOSVersion = item["Version"].ToString() ?? "--";
                note.BIOSSerialNumber = item["SerialNumber"].ToString() ?? "--";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter informações de placa-mãe do notebook: " + ex.Message);
            note.BIOSVersion = "--";
            note.BIOSSerialNumber = "--";
        }

        try
        {
            var gpuSearcher = new ManagementObjectSearcher("select * from Win32_VideoController");
            foreach (var item in gpuSearcher.Get())
            {
                if (item != null)
                {
                    var gpu = item["Name"].ToString() ?? "--";
                    var vram = 0.0;
                    var vramString = string.Empty;

                    if (item["AdapterRAM"] != null)
                    {
                        vram = Convert.ToDouble(item["AdapterRAM"]) / (1024 * 1024 * 1024);
                        vramString = vram != 0 ? $"| VRAM {vram:N2} GB" : "";
                    }
                    note.GPU = $"{gpu} {vramString}";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter informações de GPU do notebook: " + ex.Message);
            note.GPU = "--";
        }

        try
        {
            var ramSearcher = new ManagementObjectSearcher("select * from Win32_ComputerSystem");
            foreach (var item in ramSearcher.Get())
            {
                double totalMemory = Convert.ToDouble(item["TotalPhysicalMemory"]) / (1024 * 1024 * 1024);
                note.ComputerName = item["Name"].ToString() ?? "--";
                note.Manufacturer = item["Manufacturer"].ToString() ?? "--";
                note.MachineModel = item["Model"].ToString() ?? "--";
                note.RAM = totalMemory.ToString("F2") + " GB" ?? "--";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter informações de Hardware do notebook: " + ex.Message);
            note.ComputerName = "--";
            note.Manufacturer = "--";
            note.MachineModel = "--";
            note.RAM = "--";
        }

        try
        {
            var osSearcher = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            foreach (var item in osSearcher.Get())
            {
                note.OperatingSystem = item["Caption"].ToString() ?? "--";
                note.OSVersion = item["Version"].ToString() ?? "--";
                note.OSInstallDate = item["InstallDate"] != null ? ManagementDateTimeConverter.ToDateTime(item["InstallDate"].ToString()).ToString("dd/MM/yyyy") : "--";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter informações de OS do notebook: " + ex.Message);
            note.OperatingSystem = "--";
            note.OSVersion = "--";
            note.OSInstallDate = "--";
        }

        try
        {
            var diskSearcher = new ManagementObjectSearcher("select * from Win32_LogicalDisk");
            foreach (var item in diskSearcher.Get())
            {
                if (item["Name"].ToString() == "C:")
                {
                    double totalSize = Convert.ToDouble(item["Size"]) / (1024 * 1024 * 1024);
                    double freeSpace = Convert.ToDouble(item["FreeSpace"]) / (1024 * 1024 * 1024);
                    note.DiskSize = totalSize.ToString("F2") + " GB" ?? "--";
                    note.DiskFreeSpace = freeSpace.ToString("F2") + " GB" ?? "--";
                }
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter informações de Memória total do notebook: " + ex.Message);
            note.DiskSize = "--";
            note.DiskFreeSpace = "--";
        }

        try
        {
            var licenseSearcher = new ManagementObjectSearcher("select * from SoftwareLicensingService");
            foreach (var item in licenseSearcher.Get())
            {
                note.ProductKey = item["OA3xOriginalProductKey"]?.ToString() ?? "--";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter informações de chave de produto do windows: " + ex.Message);
            note.ProductKey = "--";
        }

        try
        {
            var adressSearcher = new ManagementObjectSearcher("select * from Win32_NetworkAdapterConfiguration where IPEnabled = true");
            foreach (var item in adressSearcher.Get())
            {
                note.IPAdress = item["IPAddress"] is string[] ipAddresses ? string.Join(", ", ipAddresses) : "--";
                note.MacAddress = item["MACAddress"]?.ToString() ?? "--";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter informações de rede do notebook: " + ex.Message);
            note.IPAdress = "--";
            note.MacAddress = "--";
        }


        try
        {
            string id = note.BIOSSerialNumber + note.MachineModel;

            note.NotebookId = GetHash(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter informações de ID do notebook: " + ex.Message);
            throw;
        }

        note.Data = data;
        note.CurrentUser = Environment.UserName;

        note.GetConnections();

        return note;
    }


    public static string GetHash(string input)
    {
        using var sha1 = System.Security.Cryptography.SHA1.Create();
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
        byte[] hash = sha1.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public static DateTime GetDate()
    {
        try
        {
            TimeZoneInfo timeZoneBrasilia;

            try
            {
                timeZoneBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                timeZoneBrasilia = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }

            DateTime utcNow = DateTime.UtcNow;
            DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneBrasilia);

            return localDate;

        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter data local: " + ex.Message);
            return DateTime.Now;
        }
    }
}