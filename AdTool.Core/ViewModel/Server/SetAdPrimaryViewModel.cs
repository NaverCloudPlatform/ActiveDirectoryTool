using CsLib;
using LogClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;

namespace AdTool.Core
{
    public class SetAdPrimaryViewModel : BaseViewModel
    {
        #region command
        public ICommand AdGroupReloadCommand { get; set; }        
        public ICommand InstallCommand { get; set; }
        public ICommand PageLoadedCommand { get; set; }        
        public ICommand LogClearCommand { get; set; }
        #endregion

        #region property
        public bool InstallIsRunning { get; set; }
        public bool AdGroupReloadIsRunning { get; set; }        
        public string StepName { get; set; }
        public int ProgressValue { get; set; } = 0;
        public string Log { get; set; } = "";      
        public ObservableCollection<AdGroupItem> AdGroupItems { get; set; }
        public string DomainName { get; set; }
        public string NetBiosName 
        {
            get
            {
                var str = string.Empty; 
                try
                {
                    str = DomainName.Split('.')[0];

                }
                catch
                { }
                return str; 
            }
        }
        public string SafeModePassword { get; set; }
        public string MstscPort { get; set; }
        public ObservableCollection<DomianMode> DomainModeItems { get; set; }
        public DomianMode SelectedDomainModeItem { get; set; } 
        #endregion

        #region constructor
        private static readonly Lazy<SetAdPrimaryViewModel> lazy =
            new Lazy<SetAdPrimaryViewModel>(() => new SetAdPrimaryViewModel(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static SetAdPrimaryViewModel Instance { get { return lazy.Value; } }

        public SetAdPrimaryViewModel()
        {
            DomainModeItems = new ObservableCollection<DomianMode>{
                new DomianMode { Display = "Win2008" },
                new DomianMode { Display = "Win2008R2" },
                new DomianMode { Display = "Win2012" },
                new DomianMode { Display = "Win2012R2" },
                new DomianMode { Display = "WinThreshold" },
            };
            SelectedDomainModeItem = new DomianMode { Display = "Win2008R2" };

            AdGroupItems = new ObservableCollection<AdGroupItem>();
            InstallCommand = new RelayCommand(async () => await InstallAsync());
            AdGroupReloadCommand = new RelayCommand(async () => await AdGroupReloadAsync());
            PageLoadedCommand = new RelayCommand(async () => await PageLoadedAsync());
            LogClearCommand = new RelayCommand(() => LogClear());
        }
        #endregion

        #region methods
        private void LogClear()
        {
            sbResults.Clear();
            Log = string.Empty;
            ProgressValue = 0;
        }

        private async Task InstallAsync()
        {
            await RunCommandAsync(() => InstallIsRunning, async () =>
            {
                try
                {
                    {
                        var m = new MessageBoxDialogConfirmViewModel
                        {
                            Title = "CONFIRM",
                            Message = "Do you really want to run?",
                        };
                        await IoC.UI.ShowMessage(m);
                        if (m.DialogResult == DialogResult.No)
                            return;
                    }

                    List<AdGroupItem> SeletedAdGroupItems = new List<AdGroupItem>();
                    foreach (var item in AdGroupItems)
                    {
                        if (item.IsChecked)
                            SeletedAdGroupItems.Add(item);
                    }
                    if (SeletedAdGroupItems.Count != 1)
                        throw new Exception("Select One AD Group");
                    if (string.IsNullOrEmpty(DomainName))
                        throw new Exception("Type DomainName");
                    if (!DomainName.Contains("."))
                        throw new Exception("Please enter the domain name in FQDN format. '.' This should be included. Ex) ncpdom.local, ncp.local, myad.local");
                    //if (string.IsNullOrEmpty(NetBiosName))
                    //    throw new Exception("Type NetBiosName");
                    //if (NetBiosName.Length > 15)
                    //    throw new Exception("NetBiosName should be less than 15 characters");
                    if (string.IsNullOrEmpty(SafeModePassword))
                        throw new Exception("Type SafeMode Password");
                    if (SafeModePassword.Length <= 7)
                        throw new Exception("SafeMode Passord : minimum password length is 8");
                    if (!(SpecialChar.IsContainUpperChar(SafeModePassword) && SpecialChar.IsContainLowerChar(SafeModePassword) && SpecialChar.IsContainNumChar(SafeModePassword)))
                        throw new Exception("SafeMode Passord : A combination of uppercase and lowercase numbers is required");
                    if (string.IsNullOrEmpty(MstscPort))
                        throw new Exception("Type MSTSC Port(RDP)");
                    if(!int.TryParse(MstscPort, out int portNum))
                        throw new Exception("MSTSC port number must be numeric.");
                    if (int.TryParse(MstscPort, out int portNum2))
                        if (portNum2 > 20000)
                            throw new Exception("MSTSC port is recommended to be 10000 ~ 20000.");

                    if (MstscPort == "3389")
                    {
                        var m = new MessageBoxDialogConfirmViewModel
                        {
                            Title = "CONFIRM",
                            Message = "The MSTSC port is the default port. Targeted for RDP attacks, would you like to proceed?",
                        };
                        await IoC.UI.ShowMessage(m);
                        if (m.DialogResult == DialogResult.No)
                            return;
                    }

                    // step 1 
                    StepName = "Install Started";
                    ProgressValue = 0;
                    AppendVerifyLog(StepName);

                    string response = string.Empty;
                    WcfResponse wcfResponse = new WcfResponse();

                    var step = 0;
                    var debugStep = 0;

                    // dir test
                    step++;
                    if (debugStep <= step)
                    {
                        string psCmd = $@"dir c:\";
                        StepName = $"STEP {step} : communication test";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}:9090"
                        , @"/LazyServer/LazyCommand/PostCmd"
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                        , 30
                        );
                        response = await task;
                        AppendVerifyLog($"{StepName} {response}");
                        wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                        if (!wcfResponse.IsSuccess)
                        {
                            AppendVerifyLog($"{StepName} failed ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed ({wcfResponse.ErrorMessage})");
                        }

                        ProgressValue = 10;
                        AppendVerifyLog(StepName + " completed successfully");
                    }



                    step++;
                    if (debugStep <= step) //added
                    {
                        string psCmd = $"Set-Service RemoteRegistry -StartupType Automatic" + Environment.NewLine
                        + $"Start-Service RemoteRegistry";

                        StepName = $"STEP {step} : RemoteRegistry Enable";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}:9090"
                        , @"/LazyServer/LazyCommand/PostCmd"
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                        , 30
                        );
                        response = await task;
                        AppendVerifyLog(response);
                        wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                        if (!wcfResponse.IsSuccess)
                        {
                            AppendVerifyLog($"{StepName} failed ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed ({wcfResponse.ErrorMessage})");

                        }
                        ProgressValue = 20;
                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    // hosts add ip
                    step++;
                    if (debugStep <= step)
                    {
                        string psCmd = $"Clear-Content -Path \"C:\\Windows\\System32\\drivers\\etc\\hosts\"" + Environment.NewLine
                        + $"Add-Content -Path \"C:\\Windows\\System32\\drivers\\etc\\hosts\" -Value \"{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}    {SeletedAdGroupItems.FirstOrDefault().MasterServerName}\"" + Environment.NewLine
                        + $"Add-Content -Path \"C:\\Windows\\System32\\drivers\\etc\\hosts\" -Value \"{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}    {SeletedAdGroupItems.FirstOrDefault().SlaveServerName}\"";
                        StepName = $"STEP {step} : hosts add ip";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}:9090"
                        , @"/LazyServer/LazyCommand/PostCmd"
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                        , 30
                        );
                        response = await task;
                        AppendVerifyLog(response);
                        wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                        if (!wcfResponse.IsSuccess)
                        {
                            AppendVerifyLog($"{StepName} failed ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed ({wcfResponse.ErrorMessage})");

                        }
                        ProgressValue = 20;
                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    // hosts add ip confirm 
                    step++;
                    if (debugStep <= step)
                    {
                        string psCmd = $"get-content  \"C:\\Windows\\System32\\drivers\\etc\\hosts\"";
                        StepName = $"STEP {step} : hosts add ip confirm";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}:9090"
                        , @"/LazyServer/LazyCommand/PostCmd"
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                        , 30
                        );
                        response = await task;
                        AppendVerifyLog(response);
                        wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                        if (!wcfResponse.IsSuccess)
                        {
                            AppendVerifyLog($"{StepName} failed ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed ({wcfResponse.ErrorMessage})");

                        }
                        ProgressValue = 30;
                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    step++;
                    if (debugStep <= step) //4
                    {
                        string psCmd = ""

+ $"$contents=[System.Text.Encoding]::Unicode.GetString([System.Convert]::FromBase64String(\"JABhAGQAYQBwAHQAZQByACAAPQAgAEcAZQB0AC0ATgBlAHQAQQBkAGEAcAB0AGUAcgAgAHwAIAA/ACAAewAkAF8ALgBTAHQAYQB0AHUAcwAgAC0AZQBxACAAIgB1AHAAIgB9AA0ACgAkAEkAUAAgAD0AIAAoACQAYQBkAGEAcAB0AGUAcgAgAHwAIABHAGUAdAAtAE4AZQB0AEkAUABDAG8AbgBmAGkAZwB1AHIAYQB0AGkAbwBuACkALgBJAFAAdgA0AEEAZABkAHIAZQBzAHMALgBJAFAAQQBkAGQAcgBlAHMAcwANAAoAJABHAEEAVABFAFcAQQBZACAAPQAgACgAJABhAGQAYQBwAHQAZQByACAAfAAgAEcAZQB0AC0ATgBlAHQASQBQAEMAbwBuAGYAaQBnAHUAcgBhAHQAaQBvAG4AKQAuAEkAUAB2ADQARABlAGYAYQB1AGwAdABHAGEAdABlAHcAYQB5AC4ATgBlAHgAdABIAG8AcAANAAoAJABCAEkAVABNAEEAUwBLACAAPQAgACgAJABhAGQAYQBwAHQAZQByACAAfAAgAEcAZQB0AC0ATgBlAHQASQBQAEMAbwBuAGYAaQBnAHUAcgBhAHQAaQBvAG4AKQAuAEkAUAB2ADQAQQBkAGQAcgBlAHMAcwAuAFAAcgBlAGYAaQB4AEwAZQBuAGcAdABoAA0ACgAkAEQATgBTACAAPQAgACIAMQAyADcALgAwAC4AMAAuADEAIgANAAoAJABJAFAAVAB5AHAAZQAgAD0AIAAiAEkAUAB2ADQAIgANAAoAIwAgAFIAZQBtAG8AdgBlACAAYQBuAHkAIABlAHgAaQBzAHQAaQBuAGcAIABJAFAALAAgAGcAYQB0AGUAdwBhAHkAIABmAHIAbwBtACAAbwB1AHIAIABpAHAAdgA0ACAAYQBkAGEAcAB0AGUAcgANAAoASQBmACAAKAAoACQAYQBkAGEAcAB0AGUAcgAgAHwAIABHAGUAdAAtAE4AZQB0AEkAUABDAG8AbgBmAGkAZwB1AHIAYQB0AGkAbwBuACkALgBJAFAAdgA0AEEAZABkAHIAZQBzAHMALgBJAFAAQQBkAGQAcgBlAHMAcwApACAAewANAAoAJABhAGQAYQBwAHQAZQByACAAfAAgAFIAZQBtAG8AdgBlAC0ATgBlAHQASQBQAEEAZABkAHIAZQBzAHMAIAAtAEEAZABkAHIAZQBzAHMARgBhAG0AaQBsAHkAIAAkAEkAUABUAHkAcABlACAALQBDAG8AbgBmAGkAcgBtADoAJABmAGEAbABzAGUADQAKAH0ADQAKAEkAZgAgACgAKAAkAGEAZABhAHAAdABlAHIAIAB8ACAARwBlAHQALQBOAGUAdABJAFAAQwBvAG4AZgBpAGcAdQByAGEAdABpAG8AbgApAC4ASQBwAHYANABEAGUAZgBhAHUAbAB0AEcAYQB0AGUAdwBhAHkAKQAgAHsADQAKACQAYQBkAGEAcAB0AGUAcgAgAHwAIABSAGUAbQBvAHYAZQAtAE4AZQB0AFIAbwB1AHQAZQAgAC0AQQBkAGQAcgBlAHMAcwBGAGEAbQBpAGwAeQAgACQASQBQAFQAeQBwAGUAIAAtAEMAbwBuAGYAaQByAG0AOgAkAGYAYQBsAHMAZQANAAoAfQANAAoAJABhAGQAYQBwAHQAZQByACAAfAAgAE4AZQB3AC0ATgBlAHQASQBQAEEAZABkAHIAZQBzAHMAIAAtAEEAZABkAHIAZQBzAHMARgBhAG0AaQBsAHkAIAAkAEkAUABUAHkAcABlACAALQBJAFAAQQBkAGQAcgBlAHMAcwAgACQASQBQACAALQBQAHIAZQBmAGkAeABMAGUAbgBnAHQAaAAgACQAQgBJAFQATQBBAFMASwAgAC0ARABlAGYAYQB1AGwAdABHAGEAdABlAHcAYQB5ACAAJABHAEEAVABFAFcAQQBZAA0ACgAjACAAQwBvAG4AZgBpAGcAdQByAGUAIAB0AGgAZQAgAEQATgBTACAAYwBsAGkAZQBuAHQAIABzAGUAcgB2AGUAcgAgAEkAUAAgAGEAZABkAHIAZQBzAHMAZQBzAA0ACgAkAGEAZABhAHAAdABlAHIAIAB8ACAAUwBlAHQALQBEAG4AcwBDAGwAaQBlAG4AdABTAGUAcgB2AGUAcgBBAGQAZAByAGUAcwBzACAALQBTAGUAcgB2AGUAcgBBAGQAZAByAGUAcwBzAGUAcwAgACQARABOAFMADQAKAA==\"))" + Environment.NewLine
+ $"New-Item -Path \"c:\\script\\nicSettingMaster.ps1\"  -ItemType \"file\" -Force" + Environment.NewLine
+ $"Add-Content -Path \"c:\\script\\nicSettingMaster.ps1\" -Value $contents" + Environment.NewLine
+ $"try" + Environment.NewLine
+ $"<<" + Environment.NewLine
+ $"    powershell \"c:\\script\\nicSettingMaster.ps1\"  -ErrorAction Stop" + Environment.NewLine
+ $"    Write-Output \"completed\" " + Environment.NewLine
+ $">>" + Environment.NewLine
+ $"catch " + Environment.NewLine
+ $"<<" + Environment.NewLine
+ $"    Write-Output \"error\" " + Environment.NewLine
+ $">>";
                        psCmd = psCmd.Replace("<<", "{");
                        psCmd = psCmd.Replace(">>", "}");


                        StepName = $"STEP {step} : nic setting";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        try
                        {
                            var task = dataManager.Execute
                            ("ExecuterPs"
                            , "out-string"
                            , psCmd
                            , CsLib.RequestType.POST
                            , $"https://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}:9090"
                            , @"/LazyServer/LazyCommand/PostCmd"
                            , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                            , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                            , 10
                            );
                            response = await task;
                            AppendVerifyLog(response);
                            wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                        }
                        catch { }
                        AppendVerifyLog($"{StepName} completed");
                    }

                    // hosts add ip confirm 
                    step++;
                    if (debugStep <= step) // 5
                    {
                        string psCmd = $"ipconfig /all";
                        StepName = $"STEP {step} : dns ip confirm";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}:9090"
                        , @"/LazyServer/LazyCommand/PostCmd"
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                        , 30
                        );
                        response = await task;
                        AppendVerifyLog(response);
                        wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                        if (!wcfResponse.IsSuccess)
                        {
                            AppendVerifyLog($"{StepName} failed ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed ({wcfResponse.ErrorMessage})");

                        }
                        ProgressValue = 40;
                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    step++;
                    if (debugStep <= step) // 6
                    {
                        string psCmd = $"Install-windowsfeature AD-domain-services -IncludeManagementTools";
                        StepName = $"STEP {step} : Install-windowsfeature AD-domain-services -IncludeManagementTools";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}:9090"
                        , @"/LazyServer/LazyCommand/PostCmd"
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                        , 600
                        );
                        response = await task;
                        AppendVerifyLog(response);
                        wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                        if (!wcfResponse.IsSuccess)
                        {
                            AppendVerifyLog($"{StepName} failed ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed ({wcfResponse.ErrorMessage})");

                        }
                        ProgressValue = 70;
                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    step++;
                    if (debugStep <= step) // 7
                    {
                        string psCmd = $"Import-Module ADDSDeployment";
                        StepName = $"STEP {step} : Import-Module ADDSDeployment";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}:9090"
                        , @"/LazyServer/LazyCommand/PostCmd"
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                        , 300
                        );
                        response = await task;
                        AppendVerifyLog(response);
                        wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                        if (!wcfResponse.IsSuccess)
                        {
                            AppendVerifyLog($"{StepName} failed ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed ({wcfResponse.ErrorMessage})");

                        }
                        ProgressValue = 80;
                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    step++;
                    if (debugStep <= step) // 8
                    {
                        string psCmd = $"Install-ADDSForest -CreateDnsDelegation:$false -DatabasePath \"C:\\Windows\\NTDS\" -DomainMode \"{SelectedDomainModeItem.Display}\" -DomainName \"{DomainName}\" -DomainNetbiosName \"{NetBiosName}\" -ForestMode \"{SelectedDomainModeItem.Display}\" -InstallDns:$true -LogPath \"C:\\Windows\\NTDS\" -NoRebootOnCompletion:$false -SysvolPath \"C:\\Windows\\SYSVOL\" -SafeModeAdministratorPassword (Convertto-SecureString -AsPlainText \"{SafeModePassword}\" -Force) -Force:$true";
                        StepName = $"STEP {step} : Import-Module ADDSDeployment";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}:9090"
                        , @"/LazyServer/LazyCommand/PostCmd"
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                        , 600
                        );
                        response = await task;
                        AppendVerifyLog(response);
                        wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                        if (!wcfResponse.IsSuccess)
                        {
                            AppendVerifyLog($"{StepName} failed ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed ({wcfResponse.ErrorMessage})");

                        }
                        ProgressValue = 90;
                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    step++;
                    if (debugStep <= step && !MstscPort.Equals("3389")) // 9
                    {
                        string psCmd = $"Set-ItemProperty -Path \"HKLM:\\System\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp\\\" -Name PortNumber -Value {MstscPort}";

                        StepName = $"STEP {step} : MSTSC Port Chagne";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}:9090"
                        , @"/LazyServer/LazyCommand/PostCmd"
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                        , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                        , 200
                        );
                        response = await task;
                        AppendVerifyLog(response);
                        wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                        if (!wcfResponse.IsSuccess)
                        {
                            AppendVerifyLog($"{StepName} failed ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed ({wcfResponse.ErrorMessage})");

                        }
                        ProgressValue = 95;
                        AppendVerifyLog(StepName + " completed successfully");
                        AppendVerifyLog(StepName + " Restart Started");
                    }

                    step++;
                    if (debugStep <= step) // 10
                    {
                        try
                        {
                            StepName = $"STEP {step} : Restart";
                            AppendVerifyLog($"{StepName} started");
                            string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                            string action = @"/server/v2/rebootServerInstances";
                            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                            parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                            parameters.Add(new KeyValuePair<string, string>("serverInstanceNoList.1", SeletedAdGroupItems.FirstOrDefault().MasterServerInstanceNo));
                            SoaCall soaCall = new SoaCall();
                            var task = soaCall.WebApiCall(endpoint, RequestType.POST, action, parameters, LogClient.Config.Instance.GetValue(Category.Api, Key.AccessKey), LogClient.Config.Instance.GetValue(Category.Api, Key.SecretKey));
                            response = await task;

                            JsonSerializerSettings options = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            };

                            rebootServerInstances rebootServerInstances = JsonConvert.DeserializeObject<rebootServerInstances>(response, options);
                            if (rebootServerInstances.rebootServerInstancesResponse.returnCode.Equals("0"))
                            {
                                AppendVerifyLog(StepName + response);
                            }
                        }
                        catch (Exception ex)
                        {
                            AppendVerifyLog(StepName + $"Restart Request Failed : {ex.Message}");
                            throw new Exception($"Restart Request Failed : {ex.Message}");
                        }
                        ProgressValue = 99;
                        AppendVerifyLog(StepName + " completed successfully");
                        AppendVerifyLog(StepName + " Target Server Restart Started");
                        AppendVerifyLog(StepName + " After rebooting, you can check whether the domain was successfully created with the following command.");
                        AppendVerifyLog(StepName + " CMD >> systeminfo | findstr /B /C:\"Domain\"");
                    }

                    step++;
                    if (debugStep <= step) // 11
                    {
                        try
                        {
                            StepName = $"STEP {step} : Restart Check";
                            AppendVerifyLog($"{StepName} started");

                            for (int i = 0; i < 30; i++)
                            {
                                await Task.Delay(5000);

                                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                                string action = @"/server/v2/getServerInstanceList";
                                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                                parameters.Add(new KeyValuePair<string, string>("serverInstanceNoList.1", SeletedAdGroupItems.FirstOrDefault().MasterServerInstanceNo));
                                SoaCall soaCall = new SoaCall();
                                var task = soaCall.WebApiCall(endpoint, RequestType.POST, action, parameters, LogClient.Config.Instance.GetValue(Category.Api, Key.AccessKey), LogClient.Config.Instance.GetValue(Category.Api, Key.SecretKey));
                                response = await task;

                                JsonSerializerSettings options = new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                };

                                getServerInstanceList getServerInstanceList = JsonConvert.DeserializeObject<getServerInstanceList>(response, options);
                                if (getServerInstanceList.getServerInstanceListResponse.returnCode.Equals("0"))
                                {
                                    var Status = "";
                                    var Operation = "";
                                    foreach (var a in getServerInstanceList.getServerInstanceListResponse.serverInstanceList)
                                    {
                                        Status = a.serverInstanceStatus.code;
                                        Operation = a.serverInstanceOperation.code;
                                    }

                                    if (Status.Equals("RUN") && (Operation.Equals("NULL")))
                                    {
                                        AppendVerifyLog(StepName + $"Restart Completed");
                                        break;
                                    }
                                    else
                                    {
                                        AppendVerifyLog(StepName + $"Current Status : {Status}, Operation : {Operation}, Please wait.");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            AppendVerifyLog(StepName + $"Restart Check Failed : {ex.Message}");
                            throw new Exception($"Restart Check Failed : {ex.Message}");
                        }
                        ProgressValue = 100;
                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    ProgressValue = 100;
                    StepName = "completed";

                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "SUCCESS",
                        Message = "Active Directory installation on the primary server completed successfully.",
                        OkText = "OK"
                    });

                }
                catch (Exception ex)
                {
                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "ERROR",
                        Message = ex.Message,
                        OkText = "OK"
                    });
                }
            });
        }

        private void AppendVerifyLog(string log)
        {

            DateTime logTime = DateTime.Now;
            sbResults.Append($"[{logTime.ToString("hh:mm:ss")}] : {log}" + Environment.NewLine);
            Log = sbResults.ToString();
        }

        private async Task AdGroupReloadAsync()
        {
            await RunCommandAsync(() => AdGroupReloadIsRunning, async () =>
            {
                try
                {
                    await ReloadAdGroupItemInfo();
                }
                catch (Exception ex)
                {
                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "ERROR",
                        Message = ex.Message,
                        OkText = "OK"
                    });
                }
            });
        }

        private async Task ReloadAdGroupItemInfo()
        {
            try
            {
                await fileDb.ReadTable(FileDb.TableName.TBL_CLUSTER);
                await fileDb.ReadTable(FileDb.TableName.TBL_CLUSTER_SERVER);
                await fileDb.ReadTable(FileDb.TableName.TBL_SERVER);

                List<string> instanceNoList = new List<string>();
                foreach (var a in fileDb.TBL_SERVER.Data)
                {
                    if (a.Value.serverInstanceNo != "NULL")
                        instanceNoList.Add(a.Value.serverInstanceNo);
                }

                List<serverInstance> serverInstances = new List<serverInstance>();
                try
                {
                    serverInstances = await ServerOperation.GetServerInstanceList(instanceNoList);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("server not found"))
                    { }
                    else
                        throw new Exception(ex.Message);
                }

                AdGroupItems.Clear();
                foreach (var cluster in fileDb.TBL_CLUSTER.Data)
                {
                    var adGroupItem = new AdGroupItem();
                    adGroupItem.IsChecked = false;
                    adGroupItem.GroupName = cluster.Key.clusterName;
                    foreach (var cluster_server in fileDb.TBL_CLUSTER_SERVER.Data)
                    {
                        if (cluster_server.Key.clusterName.Equals(adGroupItem.GroupName, StringComparison.OrdinalIgnoreCase) && cluster_server.Value.serverRole.Equals("MASTER"))
                        {
                            try
                            {
                                adGroupItem.MasterServerName = cluster_server.Key.serverName;
                                var serverInstance = serverInstances.Find(x => x.serverName == cluster_server.Key.serverName);
                                adGroupItem.MasterServerPublicIp = serverInstance.publicIp;
                                adGroupItem.MasterServerInstanceNo = fileDb.TBL_SERVER.Data[new TBL_SERVER_KEY { serverName = cluster_server.Key.serverName }].serverInstanceNo;
                            }
                            catch { }
                        }
                        if (cluster_server.Key.clusterName.Equals(adGroupItem.GroupName, StringComparison.OrdinalIgnoreCase) && cluster_server.Value.serverRole.Equals("SLAVE"))
                        {
                            try
                            {
                                adGroupItem.SlaveServerName = cluster_server.Key.serverName;
                                var serverInstance = serverInstances.Find(x => x.serverName == cluster_server.Key.serverName);
                                adGroupItem.SlaveServerPublicIp = serverInstance.publicIp;
                                adGroupItem.SlaveServerInstanceNo = fileDb.TBL_SERVER.Data[new TBL_SERVER_KEY { serverName = cluster_server.Key.serverName }].serverInstanceNo;
                            }
                            catch { }
                        }
                    }
                    AdGroupItems.Add(adGroupItem);
                }
            }
            catch { }
        }
        #endregion

        #region event
        public async Task PageLoadedAsync()
        {
            try
            {
                await AdGroupReloadAsync();
                logClientConfig = LogClient.Config.Instance;
            }
            catch (Exception ex)
            {
                await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                {
                    Title = "ERROR",
                    Message = ex.Message,
                    OkText = "OK"
                });
            }
        }
        #endregion

        #region etc
        FileDb fileDb = FileDb.Instance;
        LogClient.Config logClientConfig;
        DataManager dataManager = DataManager.Instance;

        private StringBuilder sbResults { get; set; } = new StringBuilder(); 
        #endregion
    }
}