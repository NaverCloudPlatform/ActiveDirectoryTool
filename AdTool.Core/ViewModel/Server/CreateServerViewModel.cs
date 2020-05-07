using CsLib;
using LogClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Text;


namespace AdTool.Core
{
    public class CreateServerViewModel : BaseViewModel
    {
        #region command
        public ICommand PageLoadedCommand { get; set; }
        public ICommand HostnameCheckCommand { get; set; }
        public ICommand CreateServerCommand { get; set; }
        #endregion

        #region property

        public bool CreateIsRunning { get; set; }
        public string ServerHostname { get; set; }
        public ObservableCollection<region> RegionItems { get; set; }
        public region SelectedRegionItem
        {
            get
            {
                return mSelectedRegionItem;
            }
            set
            {
                if (mSelectedRegionItem != value)
                {
                    mSelectedRegionItem = value;
                    if (mSelectedRegionItem != null)
                    {
                        ZoneLoadAsync(mSelectedRegionItem.regionNo);
                        GetServerImageProductList(productCode: "", regionNo: mSelectedRegionItem.regionNo);
                        GetServerProductList(serverImageProductCode: mSelectedImgProductitem.productCode, regionNo: mSelectedRegionItem.regionNo, zoneNo: mSelectedZoneItem.zoneNo);
                    }
                    else
                    {
                        ZoneItems.Clear();
                        SelectedZoneItem = null; 
                        SrvProductItems.Clear();
                        SelectedSrvProductItem = null;
                        ImgProductItems.Clear();
                        SelectedImgProductitem = null;
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
                    if (mSelectedZoneItem != null)
                    {
                        GetServerImageProductList(productCode: "", regionNo: mSelectedRegionItem.regionNo);
                        GetServerProductList(serverImageProductCode: mSelectedImgProductitem.productCode, regionNo: mSelectedRegionItem.regionNo, zoneNo: mSelectedZoneItem.zoneNo);
                    }
                    else
                    {
                        SrvProductItems.Clear();
                        SelectedSrvProductItem = null;
                        ImgProductItems.Clear();
                        SelectedImgProductitem = null; 
                    }
                }
            }
        }
        public ObservableCollection<imgProduct> ImgProductItems { get; set; }
        public imgProduct SelectedImgProductitem 
        { 
            get => mSelectedImgProductitem;
            set
            {
                if (mSelectedImgProductitem != value)
                {
                    mSelectedImgProductitem = value; 
                    if (mSelectedImgProductitem != null)
                    {
                        GetServerProductList(serverImageProductCode: mSelectedImgProductitem.productCode, regionNo: mSelectedRegionItem.regionNo, zoneNo: mSelectedZoneItem.zoneNo);
                    }
                    else
                    {
                        SrvProductItems.Clear();
                        SelectedSrvProductItem = null;
                    }
                }
            }
        }
        public ObservableCollection<srvProduct> SrvProductItems { get; set; }
        public srvProduct SelectedSrvProductItem 
        {
            get => mSelectedSrvProductItem; 
            set
            {
                mSelectedSrvProductItem = value;
            }
        }
        public ObservableCollection<accessControlGroup> AccessControlGroupItems1 { get; set; }
        public ObservableCollection<accessControlGroup> AccessControlGroupItems2 { get; set; }
        public ObservableCollection<accessControlGroup> AccessControlGroupItems3 { get; set; }
        public ObservableCollection<accessControlGroup> AccessControlGroupItems4 { get; set; }
        public ObservableCollection<accessControlGroup> AccessControlGroupItems5 { get; set; }
        public accessControlGroup SelectedAccessControlGroupItems1 { get; set; }
        public accessControlGroup SelectedAccessControlGroupItems2 { get; set; }
        public accessControlGroup SelectedAccessControlGroupItems3 { get; set; }
        public accessControlGroup SelectedAccessControlGroupItems4 { get; set; }
        public accessControlGroup SelectedAccessControlGroupItems5 { get; set; }

        #endregion

        #region constructor
        public CreateServerViewModel()
        {
            PageLoadedCommand = new RelayCommand(async () => await PageLoadedLoadedAsync());
            HostnameCheckCommand = new RelayCommand(async () => await HostnameCheck());
            CreateServerCommand = new RelayCommand(async () => await CreateServerAsync());
        } 
        #endregion

        #region event
        public async Task PageLoadedLoadedAsync()
        {
            try
            {
                logClientConfig = LogClient.Config.Instance;
                await RegionLoadAsync();
                await ZoneLoadAsync(regionNo: "1");
                await GetServerImageProductList(productCode: "", regionNo: "1");
                //await GetServerProductList(serverImageProductCode: "SPSW0WINNTEN0016A", regionNo: "1", zoneNo: "2");
                await GetAccessControlGroupList();
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
        private async Task CreateServerAsync()
        {
            await RunCommandAsync(() => CreateIsRunning, async () =>
            {
                try
                {
                    if (!PreconditionCheck())
                        return;

                    bool isSuccessInputCheck = await InputCheck(false);

                    if (!isSuccessInputCheck)
                        return;

                    if (SelectedAccessControlGroupItems1.accessControlGroupName.Equals("ncloud-default-acg"))
                    {
                        var m = new MessageBoxDialogConfirmViewModel
                        {
                            Title = "CONFIRM",
                            Message = "ncloud-default-acg was selected. Please proceed only when all ACGs required for Active Directory creation have been added." + 
                            Environment.NewLine + "Otherwise, Active Directory creation will fail because of ACG issues after the server is created." +
                            Environment.NewLine + "Create the ACG required for Active Directory in the console and select it in ADTool."
                            ,
                        };
                        await IoC.UI.ShowMessage(m);
                        if (m.DialogResult == DialogResult.No)
                            return;
                    }


                    {
                        var m = new MessageBoxDialogConfirmViewModel
                        {
                            Title = "CONFIRM",
                            Message = "Did you confirm that all TCP/UDP ports used by Active Directory are open in the ACG settings? " +
                            Environment.NewLine + "For test use, open all TCP/UDP ports and create a server for Active Directory.",
                        };
                        await IoC.UI.ShowMessage(m);
                        if (m.DialogResult == DialogResult.No)
                            return;
                    }

                    List<KeyValuePair<string, string>> listKeyValueParameters = GetParameters();
                    listKeyValueParameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                    listKeyValueParameters.Add(new KeyValuePair<string, string>("userData", TranString.EncodeBase64(dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.userDataFinal))));
                    Dictionary<string, string> dicParameters = new Dictionary<string, string>();

                    foreach (var a in listKeyValueParameters)
                    {
                        if (a.Value == null || a.Value.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                        { }
                        else
                            dicParameters.Add(a.Key, a.Value);
                    }
                    JToken jt = JToken.Parse(JsonConvert.SerializeObject(dicParameters));
                    var command = jt.ToString(Newtonsoft.Json.Formatting.Indented).Replace("_", ".");
                    var action = @"/server/v2/createServerInstances";

                    string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);

                    List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(command);
                    foreach (var a in dict)
                        parameters.Add(new KeyValuePair<string, string>(a.Key.ToString(), a.Value.ToString()));

                    SoaCall soaCall = new SoaCall();
                    var task = soaCall.WebApiCall(endpoint, RequestType.POST, action, parameters, LogClient.Config.Instance.GetValue(Category.Api, Key.AccessKey), LogClient.Config.Instance.GetValue(Category.Api, Key.SecretKey));
                    string response = await task;

                    if (response.Length > 0)
                    {
                        JToken jt2 = JToken.Parse(response);
                        response = jt2.ToString(Newtonsoft.Json.Formatting.Indented);
                    }
                    else
                    {
                        throw new Exception("resonse is empty...");
                    }

                    await ResponseFileDbSave(response);

                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "SUCCESS",
                        Message = "CreateServer Requested, Please wait 10 min.",
                        OkText = "OK"
                    });
                    await Task.Delay(5000);
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

        private async Task ResponseFileDbSave(string response)
        {
            try
            {
                List<serverInstance> serverInstances = new List<serverInstance>();
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
                else
                {
                    createServerInstances createServerInstances = JsonConvert.DeserializeObject<createServerInstances>(response, options);
                    if (createServerInstances.createServerInstancesResponse.returnCode.Equals("0"))
                    {
                        serverInstances.Clear();
                        foreach (var a in createServerInstances.createServerInstancesResponse.serverInstanceList)
                        {
                            var item = new serverInstance
                            {
                                serverName = a.serverName,
                                serverInstanceNo = a.serverInstanceNo,
                                publicIp = a.publicIp,
                                privateIp = a.privateIp,
                                region = new region
                                {
                                    regionNo = a.region.regionNo,
                                    regionCode = a.region.regionCode,
                                    regionName = a.region.regionName
                                },
                                zone = new zone
                                {
                                    zoneNo = a.zone.zoneNo,
                                    zoneName = a.zone.zoneName,
                                    zoneCode = a.zone.zoneCode,
                                    zoneDescription = a.zone.zoneDescription,
                                    regionNo = a.zone.regionNo
                                },
                                serverImageProductCode = a.serverImageProductCode,
                                serverProductCode = a.serverProductCode,
                                feeSystemTypeCode = "FXSUM",
                                loginKeyName = a.loginKeyName,
                                // where is acg list ?
                            };
                            serverInstances.Add(item);
                        }
                        if (createServerInstances.createServerInstancesResponse.totalRows == 0)
                        {
                            CheckedServer = new serverInstance();
                            new Exception("createServerInstances response error.");
                        }
                        else
                        {
                            foreach (var a in serverInstances)
                            {
                                var p = new List<KeyValuePair<string, string>>();
                                p.Add(new KeyValuePair<string, string>("serverName", a.serverName));
                                p.Add(new KeyValuePair<string, string>("serverInstanceNo", a.serverInstanceNo));
                                p.Add(new KeyValuePair<string, string>("serverPublicIp", a.publicIp));
                                p.Add(new KeyValuePair<string, string>("serverPrivateIp", a.privateIp));
                                p.Add(new KeyValuePair<string, string>("regionNo", a.region.regionNo));
                                p.Add(new KeyValuePair<string, string>("zoneNo", a.zone.zoneNo));
                                p.Add(new KeyValuePair<string, string>("serverImageProductCode", a.serverImageProductCode));
                                p.Add(new KeyValuePair<string, string>("serverProductCode", a.serverProductCode));
                                p.Add(new KeyValuePair<string, string>("feeSystemTypeCode", a.feeSystemTypeCode));
                                p.Add(new KeyValuePair<string, string>("loginKeyName", a.loginKeyName));

                                if (SelectedAccessControlGroupItems1.accessControlGroupName.Equals(""))
                                    p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_1", "NULL"));
                                else
                                    p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_1", SelectedAccessControlGroupItems1.accessControlGroupConfigurationNo));

                                if (SelectedAccessControlGroupItems2.accessControlGroupName.Equals(""))
                                    p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_2", "NULL"));
                                else
                                    p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_2", SelectedAccessControlGroupItems2.accessControlGroupConfigurationNo));

                                if (SelectedAccessControlGroupItems3.accessControlGroupName.Equals(""))
                                    p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_3", "NULL"));
                                else
                                    p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_3", SelectedAccessControlGroupItems3.accessControlGroupConfigurationNo));

                                if (SelectedAccessControlGroupItems4.accessControlGroupName.Equals(""))
                                    p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_4", "NULL"));
                                else
                                    p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_4", SelectedAccessControlGroupItems4.accessControlGroupConfigurationNo));

                                if (SelectedAccessControlGroupItems5.accessControlGroupName.Equals(""))
                                    p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_5", "NULL"));
                                else
                                    p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_5", SelectedAccessControlGroupItems5.accessControlGroupConfigurationNo));

                                await fileDb.UpSertTable(FileDb.TableName.TBL_SERVER, p);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<KeyValuePair<string, string>> GetParameters()
        {
            var p = new List<KeyValuePair<string, string>>();
            p.Add(new KeyValuePair<string, string>("serverName", ServerHostname.Trim()));
            p.Add(new KeyValuePair<string, string>("regionNo", SelectedRegionItem.regionNo));
            p.Add(new KeyValuePair<string, string>("zoneNo", SelectedZoneItem.zoneNo));
            p.Add(new KeyValuePair<string, string>("serverImageProductCode", SelectedImgProductitem.productCode));
            p.Add(new KeyValuePair<string, string>("serverProductCode", SelectedSrvProductItem.productCode));
            p.Add(new KeyValuePair<string, string>("feeSystemTypeCode", "FXSUM"));
            p.Add(new KeyValuePair<string, string>("loginKeyName", dataManager.GetValue(DataManager.Category.LoginKey, DataManager.Key.Name)));

            if (SelectedAccessControlGroupItems1.accessControlGroupName.Equals(""))
                p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_1", "NULL"));
            else
                p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_1", SelectedAccessControlGroupItems1.accessControlGroupConfigurationNo));

            if (SelectedAccessControlGroupItems2.accessControlGroupName.Equals(""))
                p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_2", "NULL"));
            else
                p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_2", SelectedAccessControlGroupItems2.accessControlGroupConfigurationNo));

            if (SelectedAccessControlGroupItems3.accessControlGroupName.Equals(""))
                p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_3", "NULL"));
            else
                p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_3", SelectedAccessControlGroupItems3.accessControlGroupConfigurationNo));

            if (SelectedAccessControlGroupItems4.accessControlGroupName.Equals(""))
                p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_4", "NULL"));
            else
                p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_4", SelectedAccessControlGroupItems4.accessControlGroupConfigurationNo));

            if (SelectedAccessControlGroupItems5.accessControlGroupName.Equals(""))
                p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_5", "NULL"));
            else
                p.Add(new KeyValuePair<string, string>("accessControlGroupConfigurationNoList_5", SelectedAccessControlGroupItems5.accessControlGroupConfigurationNo));

            return p;
        }

        private async Task<bool> InputCheck(bool checkServerInstanceNo)
        {
            bool success = false;
            try
            {
                if (checkServerInstanceNo)
                    if (CheckedServer.serverInstanceNo == null)
                        throw new Exception("check server name first");

                if (ServerHostname == null || ServerHostname.Length < 3)
                    throw new Exception("check server name first");

                if (SelectedAccessControlGroupItems1.accessControlGroupName.Equals("")
                    && SelectedAccessControlGroupItems2.accessControlGroupName.Equals("")
                    && SelectedAccessControlGroupItems3.accessControlGroupName.Equals("")
                    && SelectedAccessControlGroupItems4.accessControlGroupName.Equals("")
                    && SelectedAccessControlGroupItems5.accessControlGroupName.Equals(""))
                    throw new Exception("check acg first");

                success = true;
            }
            catch (Exception ex)
            {
                await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                {
                    Title = "ERROR",
                    Message = ex.Message,
                    OkText = "OK"
                });
                success = false;
            }
            return success;
        }

        private bool PreconditionCheck()
        {
            string bucketName = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Bucket);
            string endPointObject = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Endpoint);
            string endPointApi = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
            string accessKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey);
            string secretKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey);
            string userData = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.userDataFinal);
            string psFileName = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.PsFileName);
            string loginKey = dataManager.GetValue(DataManager.Category.LoginKey, DataManager.Key.Name);
            bool isExistsAgent = false;
            bool isExistsLoginKey = false;

            if (userData.Trim().Length < 1)
                throw new Exception("[ERROR C1] Init script not saved.");

            WebClient client = new WebClient();
            var contents = AsyncHelpers.RunSync<byte[]>(() => client.DownloadDataTaskAsync(string.Format("{0}/{1}/{2}", endPointObject, bucketName, "AD.TXT")));
            string psScript = Encoding.Default.GetString(contents);
            if (psScript.Length < 1)
                new Exception("[ERROR C1] Remote powershell script error");

            ObjectStorage o = new ObjectStorage(accessKey, secretKey, endPointObject);
            var lists = AsyncHelpers.RunSync<List<ObjectStorageFile>>(() => o.List(bucketName, "Lazylog64.zip"));

            foreach (var a in lists)
            {
                if (a.Name.Equals("Lazylog64.zip", StringComparison.OrdinalIgnoreCase))
                {
                    isExistsAgent = true;
                }
            }
            if (!isExistsAgent)
                new Exception("[ERROR C1] Agent file does not exist in object storage.");


            string action = @"/server/v2/getLoginKeyList";
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
            SoaCall soaCall = new SoaCall();
            var response = AsyncHelpers.RunSync<string>(() => soaCall.WebApiCall(endPointApi, RequestType.POST, action, parameters, accessKey, secretKey));

            JsonSerializerSettings options = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            getLoginKeyList getLoginKeyList = JsonConvert.DeserializeObject<getLoginKeyList>(response, options);

            if (getLoginKeyList.getLoginKeyListResponse.returnCode.Equals("0"))
            {
                foreach (var a in getLoginKeyList.getLoginKeyListResponse.loginKeyList)
                {
                    if (loginKey.Equals(a.keyName, StringComparison.OrdinalIgnoreCase))
                    {
                        isExistsLoginKey = true;
                        break;
                    }
                }

                if (!isExistsLoginKey)
                    throw new Exception("[ERROR C2] LoginKey does not exists in Managemnet Console!");
            }

            return true;
        }

        private async Task GetServerProductList(
            string serverImageProductCode
            , string regionNo
            , string zoneNo
            )
        {
            try
            {
                SrvProductItems = new ObservableCollection<srvProduct>();
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/getServerProductList";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                parameters.Add(new KeyValuePair<string, string>("serverImageProductCode", serverImageProductCode));
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
                else
                {
                    getServerProductList getServerProductList = JsonConvert.DeserializeObject<getServerProductList>(response, options);
                    if (getServerProductList.getServerProductListResponse.returnCode.Equals("0"))
                    {
                        SrvProductItems.Clear();
                        foreach (var a in getServerProductList.getServerProductListResponse.productList)
                        {
                            if (a.productType.code.Equals("STAND"))
                            {
                                var item = new srvProduct
                                {
                                    productCode = a.productCode,
                                    productName = a.productName, // + " (" + a.productType.codeName + ")",
                                    productType = new codeCodeName
                                    {
                                        code = a.productType.code,
                                        codeName = a.productType.codeName
                                    },
                                    productDescription = a.productDescription,
                                    infraResourceType = new codeCodeName
                                    {
                                        code = a.infraResourceType.code,
                                        codeName = a.infraResourceType.codeName
                                    },
                                    cpuCount = a.cpuCount,
                                    memorySize = a.memorySize,
                                    baseBlockStorageSize = a.baseBlockStorageSize,

                                    osInformation = a.osInformation,
                                    diskType = new codeCodeName
                                    {
                                        code = a.diskType.code,
                                        codeName = a.diskType.codeName
                                    },
                                    dbKindCode = a.dbKindCode,
                                    addBlockStorageSize = a.addBlockStorageSize,
                                };
                                SrvProductItems.Add(item);
                                if (item.productName.Contains(" 2EA") && item.productName.Contains(" 4GB") && item.productName.Contains("SSD"))
                                    SelectedSrvProductItem = item;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        
        private async Task GetServerImageProductList(string productCode = "", string regionNo = "1")
        {
            try
            {
                ImgProductItems = new ObservableCollection<imgProduct>();
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/getServerImageProductList";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                parameters.Add(new KeyValuePair<string, string>("blockStorageSize", "100"));
                if (productCode.Length > 0)
                    parameters.Add(new KeyValuePair<string, string>("productCode", productCode));
                parameters.Add(new KeyValuePair<string, string>("regionNo", regionNo));
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
                else
                {
                    getServerImageProductList getServerImageProductList = JsonConvert.DeserializeObject<getServerImageProductList>(response, options);
                    if (getServerImageProductList.getServerImageProductListResponse.returnCode.Equals("0"))
                    {
                        ImgProductItems.Clear();
                        foreach (var a in getServerImageProductList.getServerImageProductListResponse.productList)
                        {
                            var item = new imgProduct
                            {
                                productCode = a.productCode,
                                productName = a.productName,
                                productType = new codeCodeName
                                {
                                    code = a.productType.code,
                                    codeName = a.productType.codeName
                                },
                                productDescription = a.productDescription,
                                infraResourceType = new codeCodeName
                                {
                                    code = a.infraResourceType.code,
                                    codeName = a.infraResourceType.codeName
                                },
                                cpuCount = a.cpuCount,
                                memorySize = a.memorySize,
                                baseBlockStorageSize = a.baseBlockStorageSize,
                                platformType = new codeCodeName
                                {
                                    code = a.platformType.code,
                                    codeName = a.platformType.codeName
                                },
                                osInformation = a.osInformation,
                                dbKindCode = a.dbKindCode,
                                addBlockStorageSize = a.addBlockStorageSize,
                            };
                            if (item.productName.ToUpper().Contains("WINDOWS SERVER 2016"))
                            {
                                ImgProductItems.Add(item);
                                SelectedImgProductitem = item;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private async Task HostnameCheck(bool message = true)
        {
            try
            {
                if (ServerHostname == null || ServerHostname.Trim().Length < 3)
                    throw new Exception("check server name first" + Environment.NewLine + "The host name is 3 or more characters.");

                if (SpecialChar.IsContainUpperChar(ServerHostname.Trim()))
                    throw new Exception("Hostnames only support lowercase letters");

                if (SpecialChar.IsContainsSpecialChar(ServerHostname.Trim()))
                    throw new Exception("Special characters are allowed only \"-\"");


                List<serverInstance> serverInstances = new List<serverInstance>();
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/getServerInstanceList";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                parameters.Add(new KeyValuePair<string, string>("searchFilterName", "serverName"));
                parameters.Add(new KeyValuePair<string, string>("searchFilterValue", ServerHostname.Trim()));
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
                else
                {
                    getServerInstanceList getServerInstanceList = JsonConvert.DeserializeObject<getServerInstanceList>(response, options);
                    if (getServerInstanceList.getServerInstanceListResponse.returnCode.Equals("0"))
                    {
                        serverInstances.Clear();
                        foreach (var a in getServerInstanceList.getServerInstanceListResponse.serverInstanceList)
                        {
                            var item = new serverInstance
                            {
                                serverInstanceNo = a.serverInstanceNo,
                                serverName = a.serverName,
                                publicIp = a.publicIp,
                                privateIp = a.privateIp,
                                serverInstanceStatus = new codeCodeName
                                {
                                    code = a.serverInstanceStatus.code,
                                    codeName = a.serverInstanceStatus.codeName
                                },
                                serverInstanceOperation = new codeCodeName
                                {
                                    code = a.serverInstanceOperation.code,
                                    codeName = a.serverInstanceOperation.codeName
                                }
                            };
                            serverInstances.Add(item);
                        }

                        bool matched = false;
                        foreach (var a in serverInstances)
                        {
                            if (a.serverName.Equals(ServerHostname.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                matched = true;
                                CheckedServer = a;
                                if (message)
                                    throw new Exception($"You have a server with that name. serverInstanceNo : {CheckedServer.serverInstanceNo}");
                            }
                        }
                        if (!matched || serverInstances.Count == 0)
                            if (message)
                                await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                                {
                                    Title = "SUCCESS",
                                    Message = $"You can use hostname : {ServerHostname.Trim()}",
                                    OkText = "OK"
                                });
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

        private async Task GetAccessControlGroupList()
        {
            try
            {
                AccessControlGroupItems1 = new ObservableCollection<accessControlGroup>();
                AccessControlGroupItems2 = new ObservableCollection<accessControlGroup>();
                AccessControlGroupItems3 = new ObservableCollection<accessControlGroup>();
                AccessControlGroupItems4 = new ObservableCollection<accessControlGroup>();
                AccessControlGroupItems5 = new ObservableCollection<accessControlGroup>();

                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/getAccessControlGroupList";
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

                if (response.Contains("responseError"))
                {
                    hasError hasError = JsonConvert.DeserializeObject<hasError>(response, options);
                    throw new Exception(hasError.responseError.returnMessage);
                }
                else
                {
                    getAccessControlGroupList getAccessControlGroupList = JsonConvert.DeserializeObject<getAccessControlGroupList>(response, options);
                    if (getAccessControlGroupList.getAccessControlGroupListResponse.returnCode.Equals("0"))
                    {
                        AccessControlGroupItems1.Clear();
                        AccessControlGroupItems2.Clear();
                        AccessControlGroupItems3.Clear();
                        AccessControlGroupItems4.Clear();
                        AccessControlGroupItems5.Clear();

                        foreach (var a in getAccessControlGroupList.getAccessControlGroupListResponse.accessControlGroupList)
                        {
                            var item = new accessControlGroup
                            {
                                accessControlGroupConfigurationNo = a.accessControlGroupConfigurationNo,
                                accessControlGroupName = a.accessControlGroupName,
                                accessControlGroupDescription = a.accessControlGroupDescription,
                                isDefault = a.isDefault,
                                createDate = a.createDate
                            };

                            AccessControlGroupItems1.Add(item);
                            AccessControlGroupItems2.Add(item);
                            AccessControlGroupItems3.Add(item);
                            AccessControlGroupItems4.Add(item);
                            AccessControlGroupItems5.Add(item);
                        }
                        var empty = new accessControlGroup { accessControlGroupName = "" };
                        AccessControlGroupItems1.Add(empty);
                        AccessControlGroupItems2.Add(empty);
                        AccessControlGroupItems3.Add(empty);
                        AccessControlGroupItems4.Add(empty);
                        AccessControlGroupItems5.Add(empty);

                        SelectedAccessControlGroupItems1 = empty;
                        SelectedAccessControlGroupItems2 = empty;
                        SelectedAccessControlGroupItems3 = empty;
                        SelectedAccessControlGroupItems4 = empty;
                        SelectedAccessControlGroupItems5 = empty;
                    }
                }
            }
            catch (Exception)
            {
                throw;
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
                    foreach (var a in getZoneList.getZoneListResponse.zoneList)
                    {

                        var cZone = new zone
                        {
                            zoneNo = a.zoneNo,
                            zoneName = a.zoneName,
                            zoneCode = a.zoneCode,
                            zoneDescription = a.zoneDescription,
                            regionNo = a.regionNo
                        };
                        ZoneItems.Add(cZone);
                        
                    }
                    SelectedZoneItem = ZoneItems.FirstOrDefault();
                }
            }
            catch { }
        }
        #endregion

        #region etc
        FileDb fileDb = FileDb.Instance;
        LogClient.Config logClientConfig;
        DataManager dataManager = DataManager.Instance;
        private serverInstance CheckedServer = new serverInstance();
        private string EndPoint { get; set; }
        private string AccessKey { get; set; }
        private string SecretKey { get; set; }
        private string Bucket { get; set; }
        private region mSelectedRegionItem;
        private zone mSelectedZoneItem;
        private imgProduct mSelectedImgProductitem;
        private srvProduct mSelectedSrvProductItem;
        #endregion
    }
}