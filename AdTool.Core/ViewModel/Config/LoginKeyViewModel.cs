using CsLib;
using LogClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.IO;
using System.Linq;

namespace AdTool.Core
{
    public class LoginKeyViewModel : BaseViewModel
    {
        #region command
        public ICommand LoginKeyTypeChageCommand { get; set; }
        public ICommand LoginKeyReloadCommand { get; set; }
        public ICommand OpenFolderCommand { get; }
        public ICommand SaveCommand { get; set; }
        public ICommand PageLoadedCommand { get; set; }
        #endregion

        #region property
        public bool LoginKeyTypeOwnKey { get; set; }
        public ObservableCollection<loginKey> LoginKeyItems { get; set; }
        public loginKey SelectedLoginKey { get; set; }
        public bool LoginKeyTypeNewKey { get; set; }
        public string NewAuthenticationnKey { get; set; }
        public string NewAuthenticationKeySavePath { get; set; }
        public bool SaveIsRunning { get; set; }
        #endregion

        #region constructor
        public LoginKeyViewModel()
        {
            PageLoadedCommand = new RelayCommand(async () => await PageLoadedAsync());
            SaveCommand = new RelayParameterizedCommand(async (parameter) => await SaveAsync(parameter));
            LoginKeyTypeChageCommand = new RelayParameterizedCommand(parameter => LoginKeyTypeChage(parameter));
            LoginKeyReloadCommand = new RelayCommand(async () => await LoginKeyReloadAsync());
            OpenFolderCommand = new RelayCommand(() => OpenFolder());
        }
        #endregion

        #region Event
        public async Task PageLoadedAsync()
        {
            try
            {
                LoginKeyTypeOwnKey = true;
                LoginKeyTypeNewKey = false;
                NewAuthenticationKeySavePath = AppDomain.CurrentDomain.BaseDirectory;
                logClientConfig = LogClient.Config.Instance;
                await Task.Delay(1);
                await LoginKeyReloadAsync();
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
        private async Task SaveAsync(object parameter)
        {
            await RunCommandAsync(() => SaveIsRunning, async () =>
            {
                await Task.Delay(1);
                try
                {


                    if (LoginKeyTypeOwnKey)
                    {
                        if (SelectedLoginKey == null)
                            throw new Exception("Select LoginKey!");
                        dataManager.SetValue(DataManager.Category.LoginKey, DataManager.Key.Name, SelectedLoginKey.keyName);
                    }
                    else
                    {
                        if (NewAuthenticationnKey == null || NewAuthenticationnKey.Length <= 2)
                            throw new Exception("Authentication key name is too short!");

                        await CreateLoginKeyAndFileSave(NewAuthenticationKeySavePath.Trim(), NewAuthenticationnKey.Trim());
                        dataManager.SetValue(DataManager.Category.LoginKey, DataManager.Key.Name, NewAuthenticationnKey.Trim());
                    }
                    dataManager.SaveUserData();

                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "SUCCESS",
                        Message = "Saved!",
                        OkText = "OK"
                    });

                    var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C2");
                    menuItem.ProfilePictureRGB = "3483eb";
                }
                catch (Exception ex)
                {
                    var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C2");
                    menuItem.ProfilePictureRGB = "5c5c5c";
                    await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                    {
                        Title = "ERROR",
                        Message = ex.Message,
                        OkText = "OK"
                    });
                }
            });
        }
        private void LoginKeyTypeChage(object parameter)
        {
            var type = (string)parameter;
            if (type.Equals("LoginKeyTypeOwnKey"))
            {
                LoginKeyTypeOwnKey = true;
                LoginKeyTypeNewKey = false;
            }
            else
            {
                LoginKeyTypeOwnKey = false;
                LoginKeyTypeNewKey = true;
            }

        }
        private async Task LoginKeyReloadAsync()
        {
            try
            {
                LoginKeyItems = new ObservableCollection<loginKey>();
                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/getLoginKeyList";
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

                getLoginKeyList getLoginKeyList = JsonConvert.DeserializeObject<getLoginKeyList>(response, options);
                if (getLoginKeyList.getLoginKeyListResponse.returnCode.Equals("0"))
                {
                    LoginKeyItems.Clear();
                    foreach (var a in getLoginKeyList.getLoginKeyListResponse.loginKeyList)
                    {
                        LoginKeyItems.Add(new loginKey
                        {
                            fingerprint = a.fingerprint,
                            keyName = a.keyName,
                            createDate = a.createDate,
                        });
                    }
                }

                string loginKeyName = dataManager.GetValue(DataManager.Category.LoginKey, DataManager.Key.Name);

                foreach (var key in LoginKeyItems)
                {
                    if (key.keyName.Equals(loginKeyName))
                        SelectedLoginKey = key;
                }

            }
            catch (Exception ex)
            {
                var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C2");
                menuItem.ProfilePictureRGB = "5c5c5c";
                await IoC.UI.ShowMessage(new MessageBoxDialogViewModel
                {
                    Title = "ERROR",
                    Message = ex.Message,
                    OkText = "OK"
                });
            }
        }
        private void OpenFolder()
        {
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                NewAuthenticationKeySavePath = openFolderDialog.SelectedPath;
            }
        } 
        #endregion

        #region etc
        private async Task CreateLoginKeyAndFileSave(string path, string loginKeyName)
        {
            try
            {
                string privateKey = string.Empty;
                if (loginKeyName.Length < 3)
                    throw new Exception("The minimum length of the login key is three characters.");

                string endpoint = dataManager.GetValue(DataManager.Category.ApiGateway, DataManager.Key.Endpoint);
                string action = @"/server/v2/createLoginKey";
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                parameters.Add(new KeyValuePair<string, string>("responseFormatType", "json"));
                parameters.Add(new KeyValuePair<string, string>("keyName", loginKeyName));
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
                    createLoginKey createLoginKey = JsonConvert.DeserializeObject<createLoginKey>(response, options);
                    if (createLoginKey.createLoginKeyResponse.returnCode.Equals("0"))
                    {
                        privateKey = createLoginKey.createLoginKeyResponse.privateKey;
                    }

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    File.WriteAllText(Path.Combine(path, loginKeyName + ".pem"), privateKey);
                }
            }
            catch (Exception)
            {
                var menuItem = mConfigListDesignModel.Items.Single(x => x.Number == "C2");
                menuItem.ProfilePictureRGB = "5c5c5c";
                throw;
            }
        }

        LogClient.Config logClientConfig;
        DataManager dataManager = DataManager.Instance;
        ConfigListDesignModel mConfigListDesignModel;

        private string EndPoint { get; set; }
        private string AccessKey { get; set; }
        private string SecretKey { get; set; }
        private string Bucket { get; set; }

        #endregion
    }
}