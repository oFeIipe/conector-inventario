# conector-inventario

Agente Windows para coleta automática de dados de ativos de TI. Roda em background e coleta informações detalhadas do dispositivo, enviando para um endpoint configurável.

## O que coleta
* **Hardware:** CPU, GPU, RAM, disco, BIOS, número de série
* **Rede:** IP, MAC Address, SSID e BSSID do Wi-Fi
* **Sistema:** OS, versão, data de instalação, produto key
* **Softwares:** lista completa via registro do Windows e atalhos
* **Geolocalização:** cidade e coordenadas via IP
## Tecnologias
C# .NET | WMI (System.Management) | NativeWifi | Newtonsoft.Json
