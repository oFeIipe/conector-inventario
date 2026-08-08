using Microsoft.Win32;
public class SoftwareInstalado
{
    public string DeviceId { get; set; }
    public string Nome { get; set; }
    public string DataInstalacao { get; set; }
    public string Versao { get; set; }
    public DateTime DataLog { get; set; }
    public string Tamanho { get; set; }

    public static List<SoftwareInstalado> GetInstalledSoftwares(string notebookId, DateTime localDate)
    {
        List<SoftwareInstalado> installedSoftware = new();
        installedSoftware.AddRange(GetInstalledSoftwaresByKey(notebookId, localDate));
        installedSoftware.AddRange(GetInstalledSoftwaresFromProgramShortcuts(notebookId, localDate));
            
        var distinctSoftware = installedSoftware
            .GroupBy(s => s.Nome)
            .Select(g => g.First())
            .ToList();

        return distinctSoftware;
    }

    public static List<SoftwareInstalado> GetInstalledSoftwaresByKey(string notebookId, DateTime localDate)
    {
        string[] registryKeys =
    [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    ];

        List<SoftwareInstalado> installedSoftware = [];

        foreach (string keyPath in registryKeys)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key == null) continue;

                        
                    foreach (string subkeyName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                            {
                                string displayName = subkey?.GetValue("DisplayName") as string;
                                string displayDate = subkey?.GetValue("InstallDate") as string;
                                string version = subkey?.GetValue("DisplayVersion") as string;

                                object sizeObj = subkey?.GetValue("EstimatedSize");
                                string tamanho = "";

                                if (sizeObj != null && int.TryParse(sizeObj.ToString(), out int sizeKb))
                                {
                                    double sizeMb = sizeKb / 1024.0;

                                    if (sizeMb > 1024.0)
                                        tamanho = $"{sizeMb / 1024.0:F2} GB";
                                    else
                                        tamanho = $"{sizeMb:F2} MB";
                                }
                                else
                                {
                                    tamanho = "--";
                                }

                                string systemComponent = subkey?.GetValue("SystemComponent")?.ToString();
                                if (systemComponent == "1") continue;

                                if (!string.IsNullOrWhiteSpace(displayName))
                                {

                                    if (displayDate == null || displayDate.Length < 8 || displayDate == "")
                                    {
                                        displayDate = localDate.ToString("dd/MM/yyyy HH:mm");
                                    }
                                    else
                                    {
                                        displayDate = displayDate.Substring(6, 2) + "/" +
                                                        displayDate.Substring(4, 2) + "/" +
                                                        displayDate.Substring(0, 4);
                                    }
                                    installedSoftware.Add(new SoftwareInstalado
                                    {
                                        DeviceId = notebookId,
                                        Nome = displayName,
                                        DataInstalacao = displayDate,
                                        Versao = version ?? "--",
                                        DataLog = localDate,
                                        Tamanho = tamanho
                                    });
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Erro ao acessar subchave {subkeyName}: {e.Message}");
                            continue;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao acessar o registro: {e.Message}");
                continue;
            }
        }
        return installedSoftware;
    }

    public static List<SoftwareInstalado> GetInstalledSoftwaresFromProgramShortcuts(string notebookId, DateTime localDate)
    {
        List<SoftwareInstalado> installedSoftware = new();
        string[] programFilesPaths =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms)
        ];

        foreach (var path in programFilesPaths)
        {
            try
            {
                if (!Directory.Exists(path)) continue;

                var atalhos = Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories);
                foreach (var atalho in atalhos)
                {
                    try
                    {
                        string nome = Path.GetFileNameWithoutExtension(atalho);
                        if (!string.IsNullOrWhiteSpace(nome))
                        {
                            installedSoftware.Add(new SoftwareInstalado
                            {
                                DeviceId = notebookId,
                                Nome = nome,
                                DataInstalacao = localDate.ToString("dd/MM/yyyy HH:mm"),
                                Versao = "--",
                                DataLog = localDate,
                                Tamanho = "--"
                            });
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"Erro ao processar atalho {atalho}: {e.Message}");
                        continue;
                    }   
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao acessar o caminho {path}: {e.Message}");
                continue;
            }
        }

        return installedSoftware;
    }
}
