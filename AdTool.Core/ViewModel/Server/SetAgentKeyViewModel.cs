using CsLib;
using LogClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace AdTool.Core
{
    public class SetAgentKeyViewModel : BaseViewModel
    {
        #region command
        public ICommand ServerReloadCommand { get; set; }
        public ICommand SetKeyCommand { get; set; }
        public ICommand KeyChangeCommand { get; set; }
        public ICommand PageLoadedCommand { get; set; }
        #endregion

        #region property
        public bool ServerReloadIsRunning { get; set; }
        public bool SetKeyIsRunning { get; set; }
        public bool KeyChangeIsRunning { get; set; }

        public string EndPoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string OldAccessKey { get; set; } = "";
        public string OldSecretKey { get; set; } = "";
        public string Bucket { get; set; }

        public ObservableCollection<ServerInstanceItem> ServerInstanceItems { get; set; }
        public ObservableCollection<ServerOperationComboBox> ServerOperationItems { get; set; }
        public ServerOperationComboBox SelectedServerOperationItem
        {
            get => mSelectedServerOperationItem;
            
            set
            {
                List<string> instanceNoList = new List<string>();
                mSelectedServerOperationItem = value;
                if (mSelectedServerOperationItem != null)
                {

                    try
                    {
                        if (mSelectedServerOperationItem.Display.Equals("Stop"))
                        {
                            foreach (var item in ServerInstanceItems)
                            {
                                if (item.IsChecked)
                                    instanceNoList.Add(item.InstanceNo);
                            }
                            if (instanceNoList.Count > 0)
                                StopServerInstances(instanceNoList);
                        }

                        if (mSelectedServerOperationItem.Display.Equals("Start"))
                        {
                            foreach (var item in ServerInstanceItems)
                            {
                                if (item.IsChecked)
                                    instanceNoList.Add(item.InstanceNo);
                            }
                            if (instanceNoList.Count > 0)
                                StartServerInstances(instanceNoList);
                        }

                        if (mSelectedServerOperationItem.Display.Equals("Terminate"))
                        {
                            foreach (var item in ServerInstanceItems)
                            {
                                if (item.IsChecked)
                                    instanceNoList.Add(item.InstanceNo);
                            }
                            if (instanceNoList.Count > 0)
                                TerminateServerInstances(instanceNoList);
                        }
                    }
                    catch (Exception ex)
                    {
                        IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                        {
                            Title = "ERROR",
                            Message = ex.Message,
                            OkText = "OK"
                        });

                    }
                    finally
                    {
                        if (!mSelectedServerOperationItem.Display.Equals("Select"))
                            mSelectedServerOperationItem = new ServerOperationComboBox { Display = "Select" };

                        if (instanceNoList.Count > 0)
                            ServerReloadAsync();
                    }
                }
            }
        }
        private async Task StopServerInstances(List<string> instanceNoList)
        {
            try
            {
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/stopServerInstances";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));

                int i = 0;
                foreach (var instanceNo in instanceNoList)
                {
                    i++;
                    string serverInstanceNoListKey = "serverInstanceNoList." + i;
                    string serverInstanceNoListValue = instanceNo;
                    parameters.Add(new KeyValuePair<string, string>(serverInstanceNoListKey, serverInstanceNoListValue));
                }

                SoaCall soaCall = new SoaCall();
                var task = soaCall.WebApiCall(endpoint, RequestType.POST, action, parameters, LogClient.Config.Instance.GetValue(Category.Api, Key.AccessKey), LogClient.Config.Instance.GetValue(Category.Api, Key.SecretKey));
                string response = await task;

                JsonSerializerSettings options = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                if (response.Contains("responseError"))
                {
                    hasError hasError = JsonConvert.DeserializeObject<hasError>(response, options);
                    throw new Exception(hasError.responseError.returnMessage);
                }

                stopServerInstances stopServerInstances = JsonConvert.DeserializeObject<stopServerInstances>(response, options);
                if (stopServerInstances.stopServerInstancesResponse.returnCode.Equals("0"))
                {
                    ServerInstanceItems.Clear();
                    foreach (var a in stopServerInstances.stopServerInstancesResponse.serverInstanceList)
                    {
                        var item = new ServerInstanceItem
                        {
                            InstanceNo = a.serverInstanceNo,
                            Name = a.serverName,
                            PublicIp = a.publicIp,
                            PrivateIp = a.privateIp,
                            Status = a.serverInstanceStatus.code,
                            Operation = a.serverInstanceOperation.code
                        };
                        ServerInstanceItems.Add(item);
                    }
                    if (stopServerInstances.stopServerInstancesResponse.totalRows == 0)
                    {
                        MessageBox.Show("server not founds");
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
        }
        private async Task StartServerInstances(List<string> instanceNoList)
        {
            try
            {
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/startServerInstances";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));

                int i = 0;
                foreach (var instanceNo in instanceNoList)
                {
                    i++;
                    string serverInstanceNoListKey = "serverInstanceNoList." + i;
                    string serverInstanceNoListValue = instanceNo;
                    parameters.Add(new KeyValuePair<string, string>(serverInstanceNoListKey, serverInstanceNoListValue));
                }

                SoaCall soaCall = new SoaCall();
                var task = soaCall.WebApiCall(endpoint, RequestType.POST, action, parameters, LogClient.Config.Instance.GetValue(Category.Api, Key.AccessKey), LogClient.Config.Instance.GetValue(Category.Api, Key.SecretKey));
                string response = await task;

                JsonSerializerSettings options = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                if (response.Contains("responseError"))
                {
                    hasError hasError = JsonConvert.DeserializeObject<hasError>(response, options);
                    throw new Exception(hasError.responseError.returnMessage);
                }

                startServerInstances startServerInstances = JsonConvert.DeserializeObject<startServerInstances>(response, options);
                if (startServerInstances.startServerInstancesResponse.returnCode.Equals("0"))
                {
                    ServerInstanceItems.Clear();
                    foreach (var a in startServerInstances.startServerInstancesResponse.serverInstanceList)
                    {
                        var item = new ServerInstanceItem
                        {
                            InstanceNo = a.serverInstanceNo,
                            Name = a.serverName,
                            PublicIp = a.publicIp,
                            PrivateIp = a.privateIp,
                            Status = a.serverInstanceStatus.code,
                            Operation = a.serverInstanceOperation.code
                        };
                        ServerInstanceItems.Add(item);
                    }
                    if (startServerInstances.startServerInstancesResponse.totalRows == 0)
                    {
                        throw new Exception("server not founds");
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
        }
        private async Task TerminateServerInstances(List<string> instanceNoList)
        {
            try
            {
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/terminateServerInstances";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));

                int i = 0;
                foreach (var instanceNo in instanceNoList)
                {
                    i++;
                    string serverInstanceNoListKey = "serverInstanceNoList." + i;
                    string serverInstanceNoListValue = instanceNo;
                    parameters.Add(new KeyValuePair<string, string>(serverInstanceNoListKey, serverInstanceNoListValue));
                }

                SoaCall soaCall = new SoaCall();
                var task = soaCall.WebApiCall(endpoint, RequestType.POST, action, parameters, LogClient.Config.Instance.GetValue(Category.Api, Key.AccessKey), LogClient.Config.Instance.GetValue(Category.Api, Key.SecretKey));
                string response = await task;

                JsonSerializerSettings options = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                if (response.Contains("responseError"))
                {
                    hasError hasError = JsonConvert.DeserializeObject<hasError>(response, options);
                    throw new Exception(hasError.responseError.returnMessage);
                }

                terminateServerInstances terminateServerInstances = JsonConvert.DeserializeObject<terminateServerInstances>(response, options);
                if (terminateServerInstances.terminateServerInstancesResponse.returnCode.Equals("0"))
                {
                    ServerInstanceItems.Clear();
                    foreach (var a in terminateServerInstances.terminateServerInstancesResponse.serverInstanceList)
                    {
                        var item = new ServerInstanceItem
                        {
                            InstanceNo = a.serverInstanceNo,
                            Name = a.serverName,
                            PublicIp = a.publicIp,
                            PrivateIp = a.privateIp,
                            Status = a.serverInstanceStatus.code,
                            Operation = a.serverInstanceOperation.code
                        };
                        ServerInstanceItems.Add(item);
                    }
                    if (terminateServerInstances.terminateServerInstancesResponse.totalRows == 0)
                    {
                        throw new Exception("server not founds");
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
        }

        #endregion

        #region constructor
        public SetAgentKeyViewModel()
        {
            ServerOperationItems = new ObservableCollection<ServerOperationComboBox>
            {
                new ServerOperationComboBox { Display = "Select" },
                new ServerOperationComboBox { Display = "Stop" },
                new ServerOperationComboBox { Display = "Start" },
                new ServerOperationComboBox { Display = "Terminate" },
            };
            SelectedServerOperationItem = new ServerOperationComboBox { Display = "Select" };
            ServerInstanceItems = new ObservableCollection<ServerInstanceItem>();

            ServerReloadCommand = new RelayCommand(async () => await ServerReloadAsync());
            SetKeyCommand = new RelayCommand(async () => await SetKeyAsync());
            PageLoadedCommand = new RelayCommand(async () => await PageLoadedAsync());
            KeyChangeCommand = new RelayParameterizedCommand(async (parameter) => await KeyChangeAsync(parameter));
        }
        #endregion

        #region event
        public async Task PageLoadedAsync()
        {
            try
            {
                await ServerReloadAsync();
                logClientConfig = LogClient.Config.Instance;
                EndPoint = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Endpoint);
                AccessKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey);
                SecretKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey);
                Bucket = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Bucket);
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

        #region methods
        private async Task SetKeyAsync()
        {
            await RunCommandAsync(() => SetKeyIsRunning, async () =>
            {
                try
                {
                    int checkBoxCount = 0;
                    WcfResponse wcfResponse;
                    string response = string.Empty;
                    // checkbox count
                    foreach (var item in ServerInstanceItems)
                    {
                        if (item.IsChecked)
                            checkBoxCount++;
                    }
                    if (checkBoxCount == 0)
                        throw new Exception("select a server");

                    // check public ip 
                    foreach (var item in ServerInstanceItems)
                    {
                        if (item.IsChecked)
                        {
                            if (item.PublicIp.Length == 0)
                                throw new Exception("public ip error");
                        }
                    }

                    // set key
                    foreach (var item in ServerInstanceItems)
                    {
                        string errorMessage = "";


                        if (item.IsChecked)
                        {
                            // A task was canceled. then retry 5 times
                            for (int i = 0; i < 5; i++)
                            {
                                try
                                {
                                    if (item.IsChecked && item.PublicIp.Length > 1)
                                    {

                                        string command = AccessKey + ":::" + SecretKey;
                                        var task = dataManager.Execute
                                            ("ExecuterRest"
                                            , "TypeKeySetting"
                                            , command
                                            , CsLib.RequestType.POST
                                            , $"https://{item.PublicIp}:9090"
                                            , @"/LazyServer/LazyCommand/PostCmd"
                                            , AccessKey
                                            , SecretKey
                                            , 10);


                                        response = await task;
                                        wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                                        JsonSerializerSettings options = new JsonSerializerSettings
                                        {
                                            NullValueHandling = NullValueHandling.Ignore,
                                            MissingMemberHandling = MissingMemberHandling.Ignore
                                        };

                                        TypeKeySetting TypeKeySetting = JsonConvert.DeserializeObject<TypeKeySetting>(response, options);
                                        if (TypeKeySetting.IsSuccess)
                                        {
                                            errorMessage = TypeKeySetting.ErrorMessage;
                                            break;
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    if (ex.Message.ToString().Contains("A task was canceled."))
                                    {
                                        if (i == 4)
                                        {
                                            throw new Exception(ex.Message);
                                        }
                                        Task delay = Task.Delay(1000);
                                        await delay;
                                    }
                                    else
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }
                            }

                            // unregister
                            {
                                var agentFolder = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.AgentFolder);

                                string psCmd = ""
                                + "Unregister-ScheduledTask -TaskName \"LazylogStart\" -ErrorAction SilentlyContinue -Confirm:$false";

                                psCmd = psCmd.Replace("DP_AGENT_FOLDER_DP", agentFolder);

                                var task = dataManager.Execute
                                    ("ExecuterPs"
                                    , "out-string"
                                    , psCmd
                                    , CsLib.RequestType.POST
                                    , $"https://{item.PublicIp}:9090"
                                    , @"/LazyServer/LazyCommand/PostCmd"
                                    , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                                    , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                                    , 30
                                    );
                                response = await task;
                                wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);
                            }

                            // register
                            {
                                var agentFolder = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.AgentFolder);

                                string psCmd = ""
                                + "#Unregister-ScheduledTask -TaskName \"LazylogStart\" -ErrorAction SilentlyContinue -Confirm:$false" + Environment.NewLine
                                + "$action = New-ScheduledTaskAction -Execute 'Powershell.exe' `" + Environment.NewLine
                                + "  -Argument '-NoProfile -WindowStyle Hidden -command \"if((get-process LazyLog) -eq $Null) { Start-Process ''C:\\DP_AGENT_FOLDER_DP\\Lazylog\\Lazylog.exe'' -ErrorAction SilentlyContinue }\"'" + Environment.NewLine
                                + "$triggers = @()" + Environment.NewLine
                                + "$triggers += New-ScheduledTaskTrigger -AtStartup" + Environment.NewLine
                                + "$triggers += New-ScheduledTaskTrigger -Daily -At 15:00" + Environment.NewLine
                                + "$triggers += New-ScheduledTaskTrigger -Once -At 9am -RepetitionInterval (New-TimeSpan -Minutes 5) -RepetitionDuration (New-TimeSpan -Days 9999)" + Environment.NewLine
                                + "Register-ScheduledTask -Action $action -Trigger $triggers -TaskName \"LazylogStart\" -Description \"startup Lazylog\" -User SYSTEM" + Environment.NewLine
                                + "Start-ScheduledTask -TaskName \"LazylogStart\"";

                                psCmd = psCmd.Replace("DP_AGENT_FOLDER_DP", agentFolder);

                                var task = dataManager.Execute
                                    ("ExecuterPs"
                                    , "out-string"
                                    , psCmd
                                    , CsLib.RequestType.POST
                                    , $"https://{item.PublicIp}:9090"
                                    , @"/LazyServer/LazyCommand/PostCmd"
                                    , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey)
                                    , LogClient.Config.Instance.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey)
                                    , 30
                                    );
                                response = await task;
                                wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);
                                if (!wcfResponse.IsSuccess)
                                    throw new Exception(wcfResponse.ErrorMessage);
                            }
                        }
                    }
                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "SUCCESS",
                        Message = "key setting has been completed successfully",
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

        public async Task ServerReloadAsync()
        {

            await RunCommandAsync(() => ServerReloadIsRunning, async () =>
            {
                try
                {

                    await Task.Delay(2000);
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


                    try
                    {
                        List<string> deleteServerNameList = new List<string>();

                        ServerInstanceItems.Clear();
                        foreach (var a in fileDb.TBL_SERVER.Data)
                        {
                            var serverInstance = serverInstances.Find(x => x.serverName == a.Key.serverName);
                            if (serverInstance != null)
                            {
                                var item = new ServerInstanceItem
                                {
                                    IsChecked = false,
                                    Name = a.Key.serverName,
                                    //PublicIp = a.Value.serverPublicIp,
                                    PublicIp = serverInstance.publicIp,
                                    PrivateIp = a.Value.serverPrivateIp,
                                    ZoneNo = a.Value.zoneNo + "(" + serverInstance.zone.zoneCode + ")",
                                    InstanceNo = a.Value.serverInstanceNo,
                                    Status = serverInstance.serverInstanceStatus.code,
                                    Operation = serverInstance.serverInstanceOperation.code
                                };
                                ServerInstanceItems.Add(item);
                            }
                            else
                            {
                                deleteServerNameList.Add(a.Key.serverName);
                            }
                        }
                        foreach (var a in deleteServerNameList)
                        {
                            var p = new List<KeyValuePair<string, string>>();
                            p.Add(new KeyValuePair<string, string>("serverName", a));
                            await fileDb.DeleteTable(FileDb.TableName.TBL_SERVER, p);
                        }
                    }
                    catch { }
                }
                catch { }
            });


        }

        private async Task KeyChangeAsync(object parameter)
        {
            await RunCommandAsync(() => KeyChangeIsRunning, async () =>
            {
                try
                {



                    int checkBoxCount = 0;

                    // checkbox count
                    foreach (var item in ServerInstanceItems)
                    {
                        if (item.IsChecked)
                            checkBoxCount++;
                    }
                    if (checkBoxCount == 0)
                        throw new Exception("select a server");

                    // old key length chcek

                    if (OldAccessKey.Length < 1)
                        throw new Exception("type old AccessKey");

                    var OldSecretKey = (parameter as IHavePassword).OldSecurePassword.Unsecure();

                    if (OldSecretKey.Length < 1)
                        throw new Exception("type old SecretKey");


                    // check public ip 
                    foreach (var item in ServerInstanceItems)
                    {
                        if (item.IsChecked)
                        {
                            if (item.PublicIp.Length == 0)
                                throw new Exception("public ip error");
                        }
                    }

                    // set key
                    foreach (var item in ServerInstanceItems)
                    {

                        bool isSuccess = false;
                        string errorMessage = "";
                        // A task was canceled. then retry 5 times
                        for (int i = 0; i < 5; i++)
                        {
                            try
                            {
                                if (item.IsChecked && item.PublicIp.Length > 1)
                                {
                                    WcfResponse wcfResponse;
                                    string response = string.Empty;
                                    string command = AccessKey + ":::" + SecretKey;
                                    var task = dataManager.Execute
                                        ("ExecuterRest"
                                        , "TypeKeySetting"
                                        , command
                                        , CsLib.RequestType.POST
                                        , $"https://{item.PublicIp}:9090"
                                        , @"/LazyServer/LazyCommand/PostCmd"
                                        , OldAccessKey
                                        , OldSecretKey
                                        , 10);


                                    response = await task;
                                    wcfResponse = JsonConvert.DeserializeObject<WcfResponse>(response);

                                    JsonSerializerSettings options = new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore
                                    };

                                    TypeKeySetting TypeKeySetting = JsonConvert.DeserializeObject<TypeKeySetting>(response, options);
                                    if (TypeKeySetting.IsSuccess)
                                    {
                                        isSuccess = true;
                                        errorMessage = TypeKeySetting.ErrorMessage;
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.ToString().Contains("A task was canceled."))
                                {
                                    Task delay = Task.Delay(1000);
                                    await delay;
                                }
                                else
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }
                        if (!isSuccess)
                            if (errorMessage.Length == 0)
                                throw new Exception("old key is not corrent, try again!");
                            else
                                throw new Exception(errorMessage);
                    }
                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "SUCCESS",
                        Message = "key setting has been completed successfully",
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
        #endregion

        #region etc
        FileDb fileDb = FileDb.Instance;
        LogClient.Config logClientConfig;
        DataManager dataManager = DataManager.Instance;
        private ServerOperationComboBox mSelectedServerOperationItem;
        #endregion 
    }
}