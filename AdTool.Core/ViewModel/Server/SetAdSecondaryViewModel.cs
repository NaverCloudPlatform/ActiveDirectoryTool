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
    public class SetAdSecondaryViewModel : BaseViewModel
    {
        #region command
        public ICommand InstallCommand { get; set; }
        public ICommand PageLoadedCommand { get; set; }
        public ICommand AdGroupReloadCommand { get; set; }
        public ICommand LogClearCommand { get; set; }
        public ICommand CredVerifyCommand { get; set; }
        #endregion

        #region property
        public bool InstallIsRunning { get; set; }
        public bool AdGroupReloadIsRunning { get; set; }
        public bool CredVerifyIsRunning { get; set; }
        public string DomainAdminAccount 
        {
            get
            {
                var domainAdminAccount = "";
                try
                {
                    string LoginType = dataManager.GetValue(DataManager.Category.Login, DataManager.Key.LoginType);
                    if (LoginType.Equals("GOV"))
                        domainAdminAccount = DomainName.Split('.')[0] + "\\" + "ncloud";
                    else
                        domainAdminAccount = DomainName.Split('.')[0] + "\\" + "Administrator";
                }
                catch { }
                return domainAdminAccount;
            }
        }
        public string DomainAdminPassword { get; set; }
        public string StepName { get; set; }
        public int ProgressValue { get; set; } = 0;
        public string Log { get; set; } = "";
        public ObservableCollection<AdGroupItem> AdGroupItems { get; set; }
        public string DomainName { get; set; }
        public string SafeModePassword { get; set; }
        public string MstscPort { get; set; }
        #endregion

        #region constructor
        private static readonly Lazy<SetAdSecondaryViewModel> lazy =
           new Lazy<SetAdSecondaryViewModel>(() => new SetAdSecondaryViewModel(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static SetAdSecondaryViewModel Instance { get { return lazy.Value; } }

        public SetAdSecondaryViewModel()
        {
            AdGroupItems = new ObservableCollection<AdGroupItem>();

            CredVerifyCommand = new RelayCommand(async () => await CredVerifyAsync());
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

        private async Task CredVerifyAsync()
        {
            await RunCommandAsync(() => CredVerifyIsRunning, async () =>
            {
                try
                {
                    List<AdGroupItem> SeletedAdGroupItems = new List<AdGroupItem>();
                    SeletedAdGroupItems.Clear();
                    foreach (var item in AdGroupItems)
                    {
                        if (item.IsChecked)
                            SeletedAdGroupItems.Add(item);
                    }
                    if (SeletedAdGroupItems.Count != 1)
                        throw new Exception("Select One AD Group");
                    if (string.IsNullOrEmpty(DomainAdminAccount))
                        throw new Exception("Type DomainName");
                    if (string.IsNullOrEmpty(DomainAdminPassword))
                        throw new Exception("Type Domain Admin Password");

                    string response = string.Empty;
                    WcfResponse wcfResponse = new WcfResponse();
                    var step = 0;
                    var debugStep = 0;

                    step++;
                    if (debugStep <= step)
                    {
                        string psCmd = $@"dir c:\";
                        StepName = $"Cred Test STEP {step} : primary server communication test";
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
                            AppendVerifyLog($"{StepName} failed. check master server agent. ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed. check master server agent. ({wcfResponse.ErrorMessage})");
                        }

                        ProgressValue = 30;
                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    step++;
                    if (debugStep <= step)
                    {
                        string psCmd = $@"dir c:\";
                        StepName = $"Cred Test STEP {step} : secondary server communication test";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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
                            AppendVerifyLog($"{StepName} failed. check slave server agent. ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed. check slave server agent. ({wcfResponse.ErrorMessage})");
                        }

                        ProgressValue = 60;
                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    step++;
                    if (debugStep <= step)
                    {
                        string psCmd = $"(new-object directoryservices.directoryentry \"LDAP://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}\",\"{DomainAdminAccount}\",\"{DomainAdminPassword}\").psbase.name -ne $null";
                        StepName = $"Cred Test STEP {step} : AD credential verify";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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

                        if (wcfResponse.ResultMessage.ToUpper().Contains("TRUE"))
                        {
                            ProgressValue = 100;
                            AppendVerifyLog(StepName + " completed successfully");
                            await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                            {
                                Title = "SUCCESS",
                                Message = "AD credential verify completed successfully",
                                OkText = "OK"
                            });
                        }
                        else
                        {
                            ProgressValue = 100;
                            AppendVerifyLog(StepName + " AD credential verify failed");
                            throw new Exception("AD credential verify failed");
                        }
                    }
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

        public async Task InstallAsync()
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
                    if (string.IsNullOrEmpty(DomainAdminAccount))
                        throw new Exception("Type Domain Admin Account");
                    if (string.IsNullOrEmpty(DomainAdminPassword))
                        throw new Exception("Type Domain Admin Password");
                    if (string.IsNullOrEmpty(SafeModePassword))
                        throw new Exception("Type SafeMode Password");
                    if (SafeModePassword.Length <= 7)
                        throw new Exception("SafeMode Passord : minimum password length is 8");
                    if (!(SpecialChar.IsContainUpperChar(SafeModePassword) && SpecialChar.IsContainLowerChar(SafeModePassword) && SpecialChar.IsContainNumChar(SafeModePassword)))
                        throw new Exception("SafeMode Passord : A combination of uppercase and lowercase numbers is required");
                    if (string.IsNullOrEmpty(MstscPort))
                        throw new Exception("Type MSTSC Port(RDP)");
                    if (!int.TryParse(MstscPort, out int portNum))
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

                    step++;
                    if (debugStep <= step) // 1
                    {
                        string psCmd = $@"dir c:\";
                        StepName = $"Cred Test STEP {step} : primary server communication test";
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
                            AppendVerifyLog($"{StepName} failed. check master server agent. ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed. check master server agent. ({wcfResponse.ErrorMessage})");
                        }

                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    step++;
                    if (debugStep <= step) //2
                    {
                        string psCmd = $@"dir c:\";
                        StepName = $"Cred Test STEP {step} : secondary server communication test";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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
                            AppendVerifyLog($"{StepName} failed. check slave server agent. ({wcfResponse.ErrorMessage})");
                            throw new Exception($"{StepName} failed. check slave server agent. ({wcfResponse.ErrorMessage})");
                        }

                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    step++;
                    if (debugStep <= step) //3
                    {
                        string psCmd = $"(new-object directoryservices.directoryentry \"LDAP://{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}\",\"{DomainAdminAccount}\",\"{DomainAdminPassword}\").psbase.name -ne $null";
                        StepName = $"Cred Test STEP {step} : AD credential verify";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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

                        if (wcfResponse.ResultMessage.ToUpper().Contains("TRUE"))
                        {
                            ProgressValue = 10;
                            AppendVerifyLog(StepName + " completed successfully");
                        }
                        else
                        {
                            AppendVerifyLog(StepName + " AD credential verify failed");
                            throw new Exception("AD credential verify failed");
                        }
                    }



                    step++;
                    if (debugStep <= step) //4
                    {
                        string psCmd = $"Clear-Content -Path \"C:\\Windows\\System32\\drivers\\etc\\hosts\"" + Environment.NewLine
                        + $"Add-Content -Path \"C:\\Windows\\System32\\drivers\\etc\\hosts\" -Value \"{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}    {SeletedAdGroupItems.FirstOrDefault().MasterServerName}\"" + Environment.NewLine
                        + $"Add-Content -Path \"C:\\Windows\\System32\\drivers\\etc\\hosts\" -Value \"{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}    {SeletedAdGroupItems.FirstOrDefault().SlaveServerName}\"";
                        StepName = $"STEP {step} : slave hosts add ip";
                        AppendVerifyLog($"{StepName} started");
                        AppendVerifyLog($"{StepName} + {psCmd}");
                        var task = dataManager.Execute
                        ("ExecuterPs"
                        , "out-string"
                        , psCmd
                        , CsLib.RequestType.POST
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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







































                    step++;
                    if (debugStep <= step) //4
                    {
                        string psCmd = $"Clear-Content -Path \"C:\\Windows\\System32\\drivers\\etc\\hosts\"" + Environment.NewLine
                        + $"Add-Content -Path \"C:\\Windows\\System32\\drivers\\etc\\hosts\" -Value \"{SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}    {SeletedAdGroupItems.FirstOrDefault().MasterServerName}\"" + Environment.NewLine
                        + $"Add-Content -Path \"C:\\Windows\\System32\\drivers\\etc\\hosts\" -Value \"{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}    {SeletedAdGroupItems.FirstOrDefault().SlaveServerName}\"";
                        StepName = $"STEP {step} : master hosts add ip";
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
                    if (debugStep <= step) //5
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
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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
                    if (debugStep <= step) //5
                    {
                        string psCmd = ""

+ $"$contents=[System.Text.Encoding]::Unicode.GetString([System.Convert]::FromBase64String(\"UABhAHIAYQBtACgADQAKACAAIAAgACAAWwBzAHQAcgBpAG4AZwBdACQAUABSAEkARABOAFMADQAKACkADQAKACMAJABQAFIASQBEAE4AUwAgAD0AIAAiADEAMAAuADMAMwAuADMAOQAuADUANQAiAA0ACgAkAGEAZABhAHAAdABlAHIAIAA9ACAARwBlAHQALQBOAGUAdABBAGQAYQBwAHQAZQByACAAfAAgAD8AIAB7ACQAXwAuAFMAdABhAHQAdQBzACAALQBlAHEAIAAiAHUAcAAiAH0ADQAKACQASQBQACAAPQAgACgAJABhAGQAYQBwAHQAZQByACAAfAAgAEcAZQB0AC0ATgBlAHQASQBQAEMAbwBuAGYAaQBnAHUAcgBhAHQAaQBvAG4AKQAuAEkAUAB2ADQAQQBkAGQAcgBlAHMAcwAuAEkAUABBAGQAZAByAGUAcwBzAA0ACgAkAEcAQQBUAEUAVwBBAFkAIAA9ACAAKAAkAGEAZABhAHAAdABlAHIAIAB8ACAARwBlAHQALQBOAGUAdABJAFAAQwBvAG4AZgBpAGcAdQByAGEAdABpAG8AbgApAC4ASQBQAHYANABEAGUAZgBhAHUAbAB0AEcAYQB0AGUAdwBhAHkALgBOAGUAeAB0AEgAbwBwAA0ACgAkAEIASQBUAE0AQQBTAEsAIAA9ACAAKAAkAGEAZABhAHAAdABlAHIAIAB8ACAARwBlAHQALQBOAGUAdABJAFAAQwBvAG4AZgBpAGcAdQByAGEAdABpAG8AbgApAC4ASQBQAHYANABBAGQAZAByAGUAcwBzAC4AUAByAGUAZgBpAHgATABlAG4AZwB0AGgADQAKACQARABOAFMAIAA9ACAAIgAxADIANwAuADAALgAwAC4AMQAiAA0ACgAkAEkAUABUAHkAcABlACAAPQAgACIASQBQAHYANAAiAA0ACgAjACAAUgBlAG0AbwB2AGUAIABhAG4AeQAgAGUAeABpAHMAdABpAG4AZwAgAEkAUAAsACAAZwBhAHQAZQB3AGEAeQAgAGYAcgBvAG0AIABvAHUAcgAgAGkAcAB2ADQAIABhAGQAYQBwAHQAZQByAA0ACgBJAGYAIAAoACgAJABhAGQAYQBwAHQAZQByACAAfAAgAEcAZQB0AC0ATgBlAHQASQBQAEMAbwBuAGYAaQBnAHUAcgBhAHQAaQBvAG4AKQAuAEkAUAB2ADQAQQBkAGQAcgBlAHMAcwAuAEkAUABBAGQAZAByAGUAcwBzACkAIAB7AA0ACgAgACQAYQBkAGEAcAB0AGUAcgAgAHwAIABSAGUAbQBvAHYAZQAtAE4AZQB0AEkAUABBAGQAZAByAGUAcwBzACAALQBBAGQAZAByAGUAcwBzAEYAYQBtAGkAbAB5ACAAJABJAFAAVAB5AHAAZQAgAC0AQwBvAG4AZgBpAHIAbQA6ACQAZgBhAGwAcwBlAA0ACgB9AA0ACgBJAGYAIAAoACgAJABhAGQAYQBwAHQAZQByACAAfAAgAEcAZQB0AC0ATgBlAHQASQBQAEMAbwBuAGYAaQBnAHUAcgBhAHQAaQBvAG4AKQAuAEkAcAB2ADQARABlAGYAYQB1AGwAdABHAGEAdABlAHcAYQB5ACkAIAB7AA0ACgAgACQAYQBkAGEAcAB0AGUAcgAgAHwAIABSAGUAbQBvAHYAZQAtAE4AZQB0AFIAbwB1AHQAZQAgAC0AQQBkAGQAcgBlAHMAcwBGAGEAbQBpAGwAeQAgACQASQBQAFQAeQBwAGUAIAAtAEMAbwBuAGYAaQByAG0AOgAkAGYAYQBsAHMAZQANAAoAfQANAAoAJABhAGQAYQBwAHQAZQByACAAfAAgAE4AZQB3AC0ATgBlAHQASQBQAEEAZABkAHIAZQBzAHMAIAAtAEEAZABkAHIAZQBzAHMARgBhAG0AaQBsAHkAIAAkAEkAUABUAHkAcABlACAALQBJAFAAQQBkAGQAcgBlAHMAcwAgACQASQBQACAALQBQAHIAZQBmAGkAeABMAGUAbgBnAHQAaAAgACQAQgBJAFQATQBBAFMASwAgAC0ARABlAGYAYQB1AGwAdABHAGEAdABlAHcAYQB5ACAAJABHAEEAVABFAFcAQQBZAA0ACgAjACAAQwBvAG4AZgBpAGcAdQByAGUAIAB0AGgAZQAgAEQATgBTACAAYwBsAGkAZQBuAHQAIABzAGUAcgB2AGUAcgAgAEkAUAAgAGEAZABkAHIAZQBzAHMAZQBzAA0ACgAkAGEAZABhAHAAdABlAHIAIAB8ACAAUwBlAHQALQBEAG4AcwBDAGwAaQBlAG4AdABTAGUAcgB2AGUAcgBBAGQAZAByAGUAcwBzACAALQBTAGUAcgB2AGUAcgBBAGQAZAByAGUAcwBzAGUAcwAgACgAJABQAFIASQBEAE4AUwAsACQARABOAFMAKQA=\"))" + Environment.NewLine
+ $"New-Item -Path \"c:\\script\\nicSettingMaster.ps1\"  -ItemType \"file\" -Force" + Environment.NewLine
+ $"Add-Content -Path \"c:\\script\\nicSettingMaster.ps1\" -Value $contents" + Environment.NewLine
+ $"try" + Environment.NewLine
+ $"<<" + Environment.NewLine
+ $"    powershell \"c:\\script\\nicSettingMaster.ps1 -PRIDNS {SeletedAdGroupItems.FirstOrDefault().MasterServerPublicIp}\"  -ErrorAction Stop" + Environment.NewLine
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
                            , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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
                    if (debugStep <= step) // 6
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
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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
                    if (debugStep <= step) // 7
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
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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
                    if (debugStep <= step) // 8
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
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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
                    if (debugStep <= step) // 9
                    {
                        int retry = 20;
                        for (int i = 0; i < retry; i++)
                        {
                            string psCmd = $""
    + $"$passwrod = ConvertTo-SecureString -String \"{DomainAdminPassword}\" -AsPlainText -Force" + Environment.NewLine
    + $"$cred = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList (\"{DomainAdminAccount}\", $passwrod)" + Environment.NewLine
    + $"Install-ADDSDomainController -NoGlobalCatalog:$false -CreateDnsDelegation:$false -Credential $cred -CriticalReplicationOnly:$false -DatabasePath \"C:\\Windows\\NTDS\" -DomainName \"{DomainName}\" -InstallDns:$true -LogPath \"C:\\Windows\\NTDS\" -NoRebootOnCompletion:$false -SiteName \"Default-First-Site-Name\" -SysvolPath \"C:\\Windows\\SYSVOL\" -SafeModeAdministratorPassword (Convertto-SecureString -AsPlainText \"{SafeModePassword}\" -Force) -Force:$true" + Environment.NewLine;

                            StepName = $"STEP {step} : Install-ADDSDomainController";
                            AppendVerifyLog($"{StepName} started");
                            AppendVerifyLog($"{StepName} + {psCmd}");
                            var task = dataManager.Execute
                            ("ExecuterPs"
                            , "out-string"
                            , psCmd
                            , CsLib.RequestType.POST
                            , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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
                                AppendVerifyLog($"{StepName} failed (Try the staff again. The number of retries is {i + 1}. Retry up to {retry} times.)");

                                await Task.Delay(15000);
                                if (i == retry - 1)
                                {
                                    AppendVerifyLog($"{StepName} failed (Try installing again, or check the log and delete the server and try again.)");
                                    throw new Exception($"{StepName} failed ({wcfResponse.ErrorMessage})");
                                }
                            }
                            else
                                break;
                        }
                        ProgressValue = 90;
                        AppendVerifyLog(StepName + " completed successfully");
                    }

                    step++;
                    if (debugStep <= step && !MstscPort.Equals("3389")) // 10
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
                        , $"https://{SeletedAdGroupItems.FirstOrDefault().SlaveServerPublicIp}:9090"
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
                    if (debugStep <= step) // 11
                    {
                        try
                        {
                            StepName = $"STEP {step} : Restart";
                            AppendVerifyLog($"{StepName} started");
                            string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                            string action = @"/server/v2/rebootServerInstances";
                            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                            parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                            parameters.Add(new KeyValuePair<string, string>("serverInstanceNoList.1", SeletedAdGroupItems.FirstOrDefault().SlaveServerInstanceNo));
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
                        AppendVerifyLog(StepName + " You can access all active directory servers using the password of the master server.");
                    }



                    step++;
                    if (debugStep <= step) // 12
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
                                parameters.Add(new KeyValuePair<string, string>("serverInstanceNoList.1", SeletedAdGroupItems.FirstOrDefault().SlaveServerInstanceNo));
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
                        Message = "Active Directory installation on the secondary server completed successfully.",
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
                throw;
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