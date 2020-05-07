using CsLib;
using LogClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace AdTool.Core
{
    public class CreateIpViewModel : BaseViewModel
    {
        #region command
        public ICommand CreateIpCommand { get; set; }
        public ICommand ServerReloadCommand { get; set; }
        public ICommand PublicIpReloadCommand { get; set; }
        public ICommand PageLoadedCommand { get; set; }
        #endregion

        #region property
        public bool ServerReloadIsRunning { get; set; }
        public bool PublicIpReloadIsRunning { get; set; }
        public bool CreateIpIsRunning { get; set; }
        public ObservableCollection<ServerInstanceItem> ServerInstanceItems { get; set; }
        public ObservableCollection<PublicIpInstanceItem> PublicIpInstanceItems { get; set; }
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
                        {
                            ServerReloadAsync();
                        }
                    }
                }
            }
        }
        public ObservableCollection<PublicIpOperationComboBox> PublicIpOperationItems { get; set; }
        public PublicIpOperationComboBox SelectedPublicIpOperationItem
        {
            get => mSelectedPublicIpOperationItem;
            set
            {
                List<string> instanceNoList = new List<string>();
                mSelectedPublicIpOperationItem = value;
                if (mSelectedPublicIpOperationItem != null)
                {

                    try
                    {
                        if (mSelectedPublicIpOperationItem.Display.Equals("Delete"))
                        {
                            foreach (var item in PublicIpInstanceItems)
                            {
                                if (item.IsChecked)
                                    instanceNoList.Add(item.InstanceNo);
                            }
                            if (instanceNoList.Count < 1)
                                throw new Exception("Check PublicIp");

                            DeletePublicIpInstances(instanceNoList);
                        }

                        if (mSelectedPublicIpOperationItem.Display.Equals("DisAssociate"))
                        {
                            foreach (var item in PublicIpInstanceItems)
                            {
                                if (item.IsChecked)
                                    instanceNoList.Add(item.InstanceNo);
                            }
                            if (instanceNoList.Count != 1)
                                throw new Exception("Check one PublicIp");

                            var selectedPublicIpInstanceNo = instanceNoList.FirstOrDefault();

                            DisassociatePublicIpFromServerInstance(selectedPublicIpInstanceNo);
                        }

                        if (mSelectedPublicIpOperationItem.Display.Equals("Associate"))
                        {
                            foreach (var item in PublicIpInstanceItems)
                            {
                                if (item.IsChecked)
                                    instanceNoList.Add(item.InstanceNo);
                            }
                            if (instanceNoList.Count != 1)
                                throw new Exception("Check one PublicIp");


                            List<string> serverInstanceNo = new List<string>();
                            foreach (var item in ServerInstanceItems)
                            {
                                if (item.IsChecked)
                                    serverInstanceNo.Add(item.InstanceNo);
                            }
                            if (serverInstanceNo.Count != 1)
                                throw new Exception("Check one Server");


                            var selectedPublicIpInstanceNo = instanceNoList.FirstOrDefault();
                            var selectedServerInstanceNo = serverInstanceNo.FirstOrDefault();

                            AssociatePublicIpAndServer(selectedServerInstanceNo, selectedPublicIpInstanceNo);
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
                        if (!mSelectedPublicIpOperationItem.Display.Equals("Select"))
                            mSelectedPublicIpOperationItem = new PublicIpOperationComboBox { Display = "Select" };

                        if (instanceNoList.Count > 0)
                            PublicIpReloadAsync(SelectedRegionItem.regionNo, SelectedZoneItem.zoneNo);

                        ServerReloadAsync();
                    }
                }
            }
        }
        public ObservableCollection<region> RegionItems { get; set; }
        public region SelectedRegionItem
        {
            get => mSelectedRegionItem;
            set
            {
                if (mSelectedRegionItem != value)
                {
                    mSelectedRegionItem = value;
                    if (mSelectedRegionItem != null)
                    {
                        // region 이 변경되면 다른 정보들도 바꾸어준다. 
                        ZoneLoadAsync(mSelectedRegionItem.regionNo);
                        //GetServerImageProductList(productCode: "SPSW0WINNTEN0016A", regionNo: mSelectedRegionItem.regionNo);
                        //if (SelectedZoneItem != null)
                        //    GetServerProductList(serverImageProductCode: "SPSW0WINNTEN0016A", regionNo: mSelectedRegionItem.regionNo, zoneNo: SelectedZoneItem.zoneNo);
                    }
                }
            }
        }
        public ObservableCollection<zone> ZoneItems { get; set; }
        public zone SelectedZoneItem
        {
            get => mSelectedZoneItem;
            set
            {
                if (mSelectedZoneItem != value)
                {
                    mSelectedZoneItem = value;
                    if (mSelectedZoneItem != null && mSelectedRegionItem != null)
                    {
                        //GetServerImageProductList(productCode: "SPSW0WINNTEN0016A", regionNo: mSelectedRegionItem.regionNo);
                        //GetServerProductList(serverImageProductCode: "SPSW0WINNTEN0016A", regionNo: mSelectedRegionItem.regionNo, zoneNo: SelectedZoneItem.zoneNo);
                        PublicIpReloadAsync(regionNo: mSelectedRegionItem.regionNo, zoneNo: mSelectedZoneItem.zoneNo);
                    }
                }
            }
        }
        #endregion
        
        #region constructor
        public CreateIpViewModel()
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
            PublicIpInstanceItems = new ObservableCollection<PublicIpInstanceItem>();
            PublicIpOperationItems = new ObservableCollection<PublicIpOperationComboBox>
            {
                new PublicIpOperationComboBox { Display = "Select"},
                new PublicIpOperationComboBox { Display = "Associate"},
                new PublicIpOperationComboBox { Display = "DisAssociate"},
                new PublicIpOperationComboBox { Display = "Delete"},
            };
            SelectedPublicIpOperationItem = new PublicIpOperationComboBox { Display = "Select" };

            ServerReloadCommand = new RelayCommand(async () => await ServerReloadAsync());
            CreateIpCommand = new RelayCommand(async () => await CreateIpAsync());
            PageLoadedCommand = new RelayCommand(async () => await PageLoadedAsync());
            PublicIpReloadCommand = new RelayCommand(async () => await PublicIpReloadAsync(SelectedRegionItem.regionNo, SelectedZoneItem.zoneNo));
        }
        #endregion

        #region event
        public async Task PageLoadedAsync()
        {
            try
            {
                await ServerReloadAsync();
                await PublicIpReloadAsync(regionNo: "1", zoneNo: "2");
                logClientConfig = LogClient.Config.Instance;
                EndPoint = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Endpoint);
                AccessKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey);
                SecretKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey);
                Bucket = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Bucket);
                await RegionLoadAsync();
                await ZoneLoadAsync(regionNo: "1");
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
        private async Task DisassociatePublicIpFromServerInstance(string instanceNo)
        {
            try
            {
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/disassociatePublicIpFromServerInstance";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                parameters.Add(new KeyValuePair<string, string>("publicIpInstanceNo", instanceNo));

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

                disassociatePublicIpFromServerInstance disassociatePublicIpFromServerInstance = JsonConvert.DeserializeObject<disassociatePublicIpFromServerInstance>(response, options);
                if (disassociatePublicIpFromServerInstance.disassociatePublicIpFromServerInstanceResponse.returnCode.Equals("0"))
                {
                    if (disassociatePublicIpFromServerInstance.disassociatePublicIpFromServerInstanceResponse.totalRows == 0)
                    {
                        throw new Exception("ip not founds");
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

        private async Task AssociatePublicIpAndServer(string checkedServerInstanceNo, string checkedIpInstanceNo)
        {
            try
            {
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/associatePublicIpWithServerInstance";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                parameters.Add(new KeyValuePair<string, string>("serverInstanceNo", checkedServerInstanceNo));
                parameters.Add(new KeyValuePair<string, string>("publicIpInstanceNo", checkedIpInstanceNo));

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

                associatePublicIpWithServerInstance associatePublicIpWithServerInstance = JsonConvert.DeserializeObject<associatePublicIpWithServerInstance>(response, options);
                if (associatePublicIpWithServerInstance.associatePublicIpWithServerInstanceResponse.returnCode.Equals("0"))
                {
                    if (associatePublicIpWithServerInstance.associatePublicIpWithServerInstanceResponse.totalRows == 0)
                    {
                        throw new Exception("associatePublicIpWithServerInstance failed");
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

        private async Task DeletePublicIpInstances(List<string> instanceNoList)
        {
            try
            {
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/deletePublicIpInstances";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));

                int i = 0;
                foreach (var instanceNo in instanceNoList)
                {
                    i++;
                    string InstanceNoListKey = "publicIpInstanceNoList." + i;
                    string InstanceNoListValue = instanceNo;
                    parameters.Add(new KeyValuePair<string, string>(InstanceNoListKey, InstanceNoListValue));
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

                deletePublicIpInstances deletePublicIpInstances = JsonConvert.DeserializeObject<deletePublicIpInstances>(response, options);
                if (deletePublicIpInstances.deletePublicIpInstancesResponse.returnCode.Equals("0"))
                {
                    if (deletePublicIpInstances.deletePublicIpInstancesResponse.totalRows == 0)
                    {
                        throw new Exception("ip not founds");
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

        private async Task RegionLoadAsync()
        {
            try
            {
                RegionItems = new ObservableCollection<region>();
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/getRegionList";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                SoaCall soaCall = new SoaCall();
                var task = soaCall.WebApiCall(endpoint, RequestType.POST, action, parameters, LogClient.Config.Instance.GetValue(Category.Api, Key.AccessKey), LogClient.Config.Instance.GetValue(Category.Api, Key.SecretKey));
                string response = await task;

                JsonSerializerSettings options = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                getRegionList getRegionList = JsonConvert.DeserializeObject<getRegionList>(response, options);
                if (getRegionList.getRegionListResponse.returnCode.Equals("0"))
                {
                    RegionItems.Clear();
                    foreach (var a in getRegionList.getRegionListResponse.regionList)
                    {
                        RegionItems.Add(new region
                        {
                            regionNo = a.regionNo,
                            regionCode = a.regionCode,
                            regionName = a.regionName
                        });
                    }
                }

                foreach (var key in RegionItems)
                {
                    if (key.regionCode.Equals("KR"))
                    {
                        SelectedRegionItem = key;
                    }
                }

            }
            catch { }
        }

        private async Task ZoneLoadAsync(string regionNo = "1")
        {
            try
            {
                ZoneItems = new ObservableCollection<zone>();
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/getZoneList";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                parameters.Add(new KeyValuePair<string, string>("regionNo", regionNo));
                SoaCall soaCall = new SoaCall();
                var task = soaCall.WebApiCall(endpoint, RequestType.POST, action, parameters, LogClient.Config.Instance.GetValue(Category.Api, Key.AccessKey), LogClient.Config.Instance.GetValue(Category.Api, Key.SecretKey));
                string response = await task;

                JsonSerializerSettings options = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                getZoneList getZoneList = JsonConvert.DeserializeObject<getZoneList>(response, options);
                if (getZoneList.getZoneListResponse.returnCode.Equals("0"))
                {
                    ZoneItems.Clear();
                    zone cZone = new zone();
                    foreach (var a in getZoneList.getZoneListResponse.zoneList)
                    {

                        cZone = new zone
                        {
                            zoneNo = a.zoneNo,
                            zoneName = a.zoneName,
                            zoneCode = a.zoneCode,
                            zoneDescription = a.zoneDescription,
                            regionNo = a.regionNo
                        };
                        ZoneItems.Add(cZone);

                    }
                    SelectedZoneItem = cZone;
                }
            }
            catch { }
        }

        private async Task CreateIpAsync()
        {
            await RunCommandAsync(() => CreateIpIsRunning, async () =>
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
                    {
                        await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                        {
                            Title = "ERROR",
                            Message = "select server",
                            OkText = "OK"
                        });
                        return;
                    }

                    foreach (var item in ServerInstanceItems)
                    {
                        if (item.IsChecked)
                        {
                            string publicIp = await CreatePublicIpInstance(item.InstanceNo);
                            if (publicIp != null && publicIp.Length > 0)
                            {
                                var p = new List<KeyValuePair<string, string>>();
                                p.Add(new KeyValuePair<string, string>("serverName", item.Name));
                                p.Add(new KeyValuePair<string, string>("serverPublicIp", publicIp));
                                await fileDb.UpSertTable(FileDb.TableName.TBL_SERVER, p);
                            }
                        }
                    }
                    await ServerReloadAsync();
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

        private async Task<string> CreatePublicIpInstance(string serverInstanceNo)
        {
            string publicIp = string.Empty;
            try
            {
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"server/v2/createPublicIpInstance";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                parameters.Add(new KeyValuePair<string, string>("serverInstanceNo", serverInstanceNo));

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

                createPublicIpInstance createPublicIpInstance = JsonConvert.DeserializeObject<createPublicIpInstance>(response, options);
                if (createPublicIpInstance.createPublicIpInstanceResponse.returnCode.Equals("0"))
                {
                    foreach (var a in createPublicIpInstance.createPublicIpInstanceResponse.publicIpInstanceList)
                        publicIp = a.publicIp;

                    if (createPublicIpInstance.createPublicIpInstanceResponse.totalRows == 0)
                        throw new Exception("createPublicIpInstance error");
                }

            }
            catch (Exception)
            {
                throw;
            }
            return publicIp;
        }

        private async Task ServerReloadAsync()
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
                        {
                            // 
                        }
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
                    catch
                    {

                    }

                }
                catch
                {

                }
            });


        }

        private async Task PublicIpReloadAsync(string regionNo, string zoneNo)
        {
            await RunCommandAsync(() => PublicIpReloadIsRunning, async () =>
            {
                try
                {
                    await Task.Delay(2000);

                    if (regionNo == null)
                        regionNo = SelectedRegionItem.regionNo;
                    if (zoneNo == null)
                        zoneNo = SelectedZoneItem.zoneNo;

                    string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                    string action = @"/server/v2/getPublicIpInstanceList";
                    List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                    parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                    parameters.Add(new KeyValuePair<string, string>("regionNo", regionNo));
                    parameters.Add(new KeyValuePair<string, string>("zoneNo", zoneNo));

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

                    getPublicIpInstanceList getPublicIpInstanceList = JsonConvert.DeserializeObject<getPublicIpInstanceList>(response, options);
                    if (getPublicIpInstanceList.getPublicIpInstanceListResponse.returnCode.Equals("0"))
                    {
                        PublicIpInstanceItems.Clear();
                        foreach (var a in getPublicIpInstanceList.getPublicIpInstanceListResponse.publicIpInstanceList)
                        {
                            PublicIpInstanceItem item = new PublicIpInstanceItem
                            {
                                IsChecked = false,
                                InstanceNo = a.publicIpInstanceNo,
                                PublicIp = a.publicIp,
                                ServerInstanceNo = a.serverInstanceAssociatedWithPublicIp.serverInstanceNo,
                                ServerName = a.serverInstanceAssociatedWithPublicIp.serverName,
                                Status = a.publicIpInstanceStatus.code,
                                Operation = a.publicIpInstanceOperation.code
                            };
                            PublicIpInstanceItems.Add(item);
                        }

                        //if (getPublicIpInstanceList.getPublicIpInstanceListResponse.totalRows == 0)
                        //{
                        //    throw new Exception("ip not founds");
                        //}
                    }

                }
                catch
                {

                }
            });

        }

        #endregion

        #region etc

        FileDb fileDb = FileDb.Instance;
        LogClient.Config logClientConfig;
        DataManager dataManager = DataManager.Instance;

        private string EndPoint { get; set; }
        private string AccessKey { get; set; }
        private string SecretKey { get; set; }
        private string Bucket { get; set; }

        private ServerOperationComboBox mSelectedServerOperationItem;
        private PublicIpOperationComboBox mSelectedPublicIpOperationItem;
        private region mSelectedRegionItem;
        private zone mSelectedZoneItem; 
        #endregion
    }
}