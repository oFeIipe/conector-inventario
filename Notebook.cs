using NativeWifi;
using System.Text;

public class Notebook
{
    public string NotebookId { get; set; }
    public DateTime Data { get; set; }
    public string CPU { get; set; }
    public string BIOSVersion { get; set; }
    public string BIOSSerialNumber { get; set; }
    public string RAM { get; set; }
    public string ComputerName { get; set; }
    public string Manufacturer { get; set; }
    public string MachineModel { get; set; }
    public string OperatingSystem { get; set; }
    public string OSVersion { get; set; }
    public string OSInstallDate { get; set; }
    public string DiskSize { get; set; }
    public string DiskFreeSpace { get; set; }
    public string ProductKey { get; set; }
    public string IPAdress { get; set; }
    public string Inicializacao { get; set; }
    public string MacAddress { get; set; }
    public string CurrentUser { get; set; }
    public string SSID { get; set; }
    public string BSSID { get; set; }
    public string GPU {  get; set; }

    public void GetConnections()
    {
        WlanClient client = new WlanClient();
        try
        {
            foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
            {
                var connection = wlanIface.CurrentConnection;

                if (connection.isState == Wlan.WlanInterfaceState.Connected)
                {
                    this.SSID = GetStringForSSID(connection.wlanAssociationAttributes.dot11Ssid);
                    this.BSSID = GetStringForBSSID(connection.wlanAssociationAttributes.dot11Bssid);
                }
                else
                {
                    this.SSID = "Nenhuma conexão Wi-Fi ativa";
                    this.BSSID = "N/A";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter conexões Wi-Fi: " + ex.Message);
            this.SSID = "Nenhuma conexão Wi-Fi ativa";
            this.BSSID = "N/A";
        }

    }
    static string GetStringForSSID(Wlan.Dot11Ssid ssid)
    {
        return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
    }
    static string GetStringForBSSID(byte[] bssid)
    {
        return BitConverter.ToString(bssid).Replace("-", ":");
    }
}
