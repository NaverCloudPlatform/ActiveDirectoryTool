using CsLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;
using System.Linq;

namespace AdTool.Core
{
    public class ConfigCheckViewModel : BaseViewModel
    {
        #region command 
        public ICommand CheckCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand PageLoadedCommand { get; set; }
        #endregion

        #region property
        public bool CheckIsRunning { get; set; }
        public string Log { get; set; } = "";
        #endregion

        #region constructor
        private static readonly Lazy<ConfigCheckViewModel> lazy =
            new Lazy<ConfigCheckViewModel>(() => new ConfigCheckViewModel(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static ConfigCheckViewModel Instance { get { return lazy.Value; } }
        public ConfigCheckViewModel()
        {
            CheckCommand = new RelayCommand(async () => await CheckAsync());
            ClearCommand = new RelayCommand(() => Clear());
            PageLoadedCommand = new RelayCommand(async () => await PageLoadedAsync());
        }
        #endregion

        #region event
        public async Task PageLoadedAsync()
        {
            try
            {
                logClientConfig = LogClient.Config.Instance;
                await Task.Delay(1);
                sbResults.Clear();
                EndPoint = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Endpoint);
                AccessKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey);
                SecretKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey);
                Bucket = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Bucket);
                mConfigListDesignModel = ConfigListDesignModel.Instance;
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
        public void Clear()
        {
            var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
            menuItem.ProfilePictureRGB = "5c5c5c";
            menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C2");
            menuItem.ProfilePictureRGB = "5c5c5c";

            sbResults = new StringBuilder();
            Log = "";
        }

        public async Task CheckAsync( )
        {
            await RunCommandAsync(() => CheckIsRunning, async () =>
            {
                await Task.Delay(1);
                try
                {
                    AppendVerifyLog("");
                    AppendVerifyLog("Configuration Check Started!");
                    AppendVerifyLog("");


                    CheckObjectStorage();
                    CheckLoginKey();
                    CheckInitScript();

                    AppendVerifyLog("");
                    AppendVerifyLog("Configuration Check Completed Successfully.");
                    AppendVerifyLog("");


                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "SUCCESS",
                        Message = "Check completed successfully!",
                        OkText = "OK"
                    });
                }
                catch (Exception ex)
                {
                    AppendVerifyLog(ex.Message);
                    AppendVerifyLog("Configuration check failed.");

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
        private void CheckObjectStorage()
        {
            AppendVerifyLog("*. Object Stoage");
            try
            {
                string bucketName = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Bucket);
                string endPoint = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Endpoint);
                string accessKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey);
                string secretKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey);

                AppendVerifyLog($"   EndPoint : {endPoint}");
                AppendVerifyLog($"   BucketName : {bucketName}");

                if (bucketName.Length < 1)
                    throw new Exception("   [ERROR] The object storage bucket name is too short.");

                if (bucketName != bucketName.ToLower())
                    throw new Exception("   [ERROR] The object storage bucket name is must be lowercase!");

                ObjectStorage o = new ObjectStorage(accessKey, secretKey, endPoint);
                if (!AsyncHelpers.RunSync<bool>(() => o.IsExistsBucket(bucketName)))
                {
                    throw new Exception("   Bucket Connection Result : Failed");
                }
                AppendVerifyLog($"   Object Storage Check Result : Success");
                var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
                menuItem.ProfilePictureRGB = "3483eb";
            }
            catch (Exception ex)
            {
                var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
                menuItem.ProfilePictureRGB = "5c5c5c";
                AppendVerifyLog(ex.Message);
                AppendVerifyLog("   Object Storage Help Message...");
                AppendVerifyLog("   -----------------------------------------------");
                AppendVerifyLog("   1. Create a bucket with non-duplicate names in AD Tool > Config > C1");
                AppendVerifyLog("   2. Save.");
                AppendVerifyLog("   -----------------------------------------------");
                throw new Exception($"C1 Error : {ex.Message}");
            }
        }
        private void CheckLoginKey()
        {
            AppendVerifyLog("*. LoginKey");
            try
            {
                string endPoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string accessKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey);
                string secretKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey);
                string loginKey = dataManager.GetValue(DataManager.Category.LoginKey, DataManager.Key.Name);
                bool isExistsLoginKey = false;

                string action = @"/server/v2/getLoginKeyList";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                SoaCall soaCall = new SoaCall();
                var response = AsyncHelpers.RunSync<string>(() => soaCall.WebApiCall(endPoint, RequestType.POST, action, parameters, accessKey, secretKey));

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
                    AppendVerifyLog($"   LoginKey : {loginKey}");

                    if (!isExistsLoginKey)
                        throw new Exception("   LoginKey does not exists in Managemnet Console!");
                }

                AppendVerifyLog($"   LoginKey Check Result : Success");
                {
                    var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C2");
                    menuItem.ProfilePictureRGB = "3483eb";
                }
            }
            catch (Exception ex)
            {
                var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C2");
                menuItem.ProfilePictureRGB = "5c5c5c";
                AppendVerifyLog(ex.Message);
                AppendVerifyLog("   LoginKey Help Message...");
                AppendVerifyLog("   -----------------------------------------------");
                AppendVerifyLog("   1. Select and save the login key saved in AD Tool > Config > C2 or create a new login key.");
                AppendVerifyLog("   -----------------------------------------------");
                throw new Exception($"C2 Error : {ex.Message}");
            }
        }
        private void CheckInitScript()
        {
            AppendVerifyLog("*. InitScript");
            try
            {
                string bucketName = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Bucket);
                string endPoint = dataManager.GetValue(DataManager.Category.ObjectStorage, DataManager.Key.Endpoint);
                string accessKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.AccessKey);
                string secretKey = logClientConfig.GetValue(LogClient.Category.Api, LogClient.Key.SecretKey);
                string userData = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.userDataFinal);
                string psFileName = dataManager.GetValue(DataManager.Category.InitScript, DataManager.Key.PsFileName);
                bool isExistsAgent = false;
                if (userData.Trim().Length < 1)
                {
                    {
                        var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
                        menuItem.ProfilePictureRGB = "5c5c5c";
                        throw new Exception("   Init script not saved.");
                    }
                }

                WebClient client = new WebClient();
                var contents = AsyncHelpers.RunSync<byte[]>(() => client.DownloadDataTaskAsync(string.Format("{0}/{1}/{2}", endPoint, bucketName, "AD.TXT")));
                string psScript = Encoding.Default.GetString(contents);
                if (psScript.Length < 1)
                    new Exception("   Remote powershell script error");

                ObjectStorage o = new ObjectStorage(accessKey, secretKey, endPoint);
                var lists = AsyncHelpers.RunSync<List<ObjectStorageFile>>(() => o.List(bucketName, "Lazylog64.zip"));

                foreach (var a in lists)
                {
                    if (a.Name.Equals("Lazylog64.zip", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendVerifyLog($"   Agent file name : {a.Name}, size : {a.Length}, date : {a.LastWriteTime}");
                        isExistsAgent = true;
                    }
                }
                if (!isExistsAgent)
                    new Exception("   [ERROR] Agent file does not exist in object storage.");

                AppendVerifyLog($"   Init Script Check Result : Success");
                {
                    var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
                    menuItem.ProfilePictureRGB = "3483eb";
                }

            }
            catch (Exception ex)
            {
                var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C1");
                menuItem.ProfilePictureRGB = "5c5c5c";
                AppendVerifyLog(ex.Message);
                AppendVerifyLog("   InitScript Help Message...");
                AppendVerifyLog("   -----------------------------------------------");
                AppendVerifyLog("   1. Upload the initialization script in AD Tool > Config > C1");
                AppendVerifyLog("   -----------------------------------------------");
                throw new Exception("Init Script Error!");
            }
        }
        private void AppendVerifyLog(string log)
        {

            DateTime logTime = DateTime.Now;
            sbResults.Append($"[{logTime.ToString("hh:mm:ss")}] : {log}" + Environment.NewLine);
            Log = sbResults.ToString();
        }

        DataManager dataManager = DataManager.Instance;
        LogClient.Config logClientConfig = null;
        ConfigListDesignModel mConfigListDesignModel;

        private string EndPoint { get; set; }
        private string AccessKey { get; set; }
        private string SecretKey { get; set; }
        private string Bucket { get; set; }
        private StringBuilder sbResults { get; set; } = new StringBuilder();
        #endregion
    }
}