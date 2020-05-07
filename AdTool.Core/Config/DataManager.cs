using CsLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AdTool.Core
{
    public class DataManager
    {
        #region constructor
        private static readonly Lazy<DataManager> lazy =
            new Lazy<DataManager>(() => new DataManager(), LazyThreadSafetyMode.ExecutionAndPublication);
        public static DataManager Instance { get { return lazy.Value; } }

        public DataManager()
        {
            listData = new List<string>();
            dicData = new Dictionary<Tuple<Category, Key>, string>();
        }
        #endregion

        #region methods

        private string GetRandomKey()
        {
            string key = string.Empty;
            if (!File.Exists(encryptionKeyFullFilename))
            {
                // create key file
                //int randomKey = new Random().Next(1, 100000);
                //key = TranString.EncodeBase64Unicode(randomKey.ToString());
                key = new RandomString().GetString(16);
                File.WriteAllText(encryptionKeyFullFilename, key, Encoding.Default);
                key = ("0123456789012345" + key);
                key = key.Substring(key.Length - 16);
            }
            else
            {
                // read key file
                key = File.ReadAllText(encryptionKeyFullFilename, Encoding.Default);
                //string InitFile = File.ReadAllText(dataManagerContentsInitFullFilename, Encoding.Default);
                key = ("0123456789012345" + key);
                key = key.Substring(key.Length - 16);
            }
            return key;
        }

        public void LoadUserData()
        {
            lock (LockObj)
            {
                if (!File.Exists(encryptionKeyFullFilename))
                {
                    key = GetRandomKey();

                    // write user initial config file
                    string strDataManagerContentsInitFullFilename = File.ReadAllText(dataManagerContentsInitFullFilename, Encoding.Default);
                    string strEncryptedDataManagerContentsInit = TranString.EncodeAES(strDataManagerContentsInitFullFilename, key);
                    // 암호키를 지우면 데이터 파일을 다시 만든다.
                    if (File.Exists(encryptedDataManagerContentsFullFilename))
                        File.Delete(encryptedDataManagerContentsFullFilename);
                    File.WriteAllText(encryptedDataManagerContentsFullFilename, strEncryptedDataManagerContentsInit);
                }

                // read user config file 
                key = GetRandomKey();
                string CryptedConfigFile = File.ReadAllText(Path.Combine(path, encryptedDataManagerContentsFullFilename), Encoding.Default);
                string PlainConfigFile = TranString.DecodeAES(CryptedConfigFile, key);
                string2List(PlainConfigFile, listData);
                LoadDataList2Dictionary(listData, dicData);
                var logintype = dicData[new Tuple<Category, Key>(Category.Login, Key.LoginType)];
                LoginType = (LoginType)Enum.Parse(typeof(LoginType), logintype);
            }
        }

        public void SaveUserData()
        {

            string fullFilename = encryptedDataManagerContentsFullFilename;
            List<string> listName = listData;

            lock (LockObj)
            {
                if (File.Exists(fullFilename))
                    File.Delete(fullFilename);


                if (LoginType.GOV == LoginType)
                    dicData[new Tuple<Category, Key>(Category.Login, Key.LoginType)] = "GOV";
                else
                    dicData[new Tuple<Category, Key>(Category.Login, Key.LoginType)] = "Default";

                var contents = new StringBuilder();

                foreach (string line in listName)
                {
                    try
                    {
                        if (line.StartsWith(@"#") || (line == null) || (line.Trim().Equals("")))
                        {
                            contents.Append(line + Environment.NewLine);
                        }
                        else
                        {
                            string[] lineValues = line.Split(new string[] { ":::" }, StringSplitOptions.None);
                            string data = string.Empty;
                            data = dicData[new Tuple<Category, Key>((Category)Enum.Parse(typeof(Category), lineValues[1]), (Key)Enum.Parse(typeof(Key), lineValues[2]))];
                            if (lineValues[0].ToString().ToUpper().Equals("Base64Unicode".ToUpper()))
                                data = TranString.EncodeBase64Unicode(data);
                            else if (lineValues[0].ToString().ToUpper().Equals("Base64Ascii".ToUpper()))
                                data = TranString.EncodeBase64(data);
                            contents.Append(lineValues[0] + ":::" + lineValues[1] + ":::" + lineValues[2] + ":::" + data + Environment.NewLine);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }


                File.WriteAllText(fullFilename, TranString.EncodeAES(contents.ToString(), key));
            }
        }

        private void string2List(string contents, List<string> listName)
        {
            try
            {
                string[] lines = Regex.Split(contents, Environment.NewLine);
                listName.Clear();
                foreach (string line in lines)
                {
                    try
                    {
                        listName.Add(line);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void LoadDataList2Dictionary(List<string> listName, Dictionary<Tuple<Category, Key>, string> dicName)
        {
            lock (LockObj)
            {
                dicData.Clear();
                string data = string.Empty;
                foreach (string line in listName)
                {
                    try
                    {
                        if (!line.StartsWith(@"#") && !(line == null) && !(line.Trim().Equals("")))
                        {
                            string[] lineValues = line.Split(new string[] { ":::" }, StringSplitOptions.None);

                            if (lineValues[0].ToString().Equals("Base64Unicode", StringComparison.OrdinalIgnoreCase))
                                data = TranString.DecodeBase64Unicode(lineValues[3]);
                            else if (lineValues[0].ToString().Equals("Base64Ascii", StringComparison.OrdinalIgnoreCase))
                                data = TranString.DecodeBase64(lineValues[3]);
                            else
                                data = lineValues[3];

                            dicName.Add
                                (
                                    new Tuple<Category, Key>
                                    (
                                        (Category)Enum.Parse(typeof(Category), lineValues[1]),
                                        (Key)Enum.Parse(typeof(Key), lineValues[2])
                                    ), data
                                );
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                
                //var logintype = dicData[new Tuple<Category, Key>(Category.Login, Key.LoginType)];
                //LoginType = (LoginType)Enum.Parse(typeof(LoginType), logintype);

            }
        }

        public void SetValue(Category category, Key key, string value)
        {
            try
            {
                lock (LockObj)
                {
                    if (LoginType.GOV == LoginType)
                    {
                        if (category == Category.ObjectStorage && key == Key.Endpoint)
                            key = Key.EndpointGov;
                        if (category == Category.ApiGateway && key == Key.Endpoint)
                            key = Key.EndpointGov;
                    }
                    dicData[new Tuple<Category, Key>(category, key)] = value;
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Categoy : {category}, Key : {key}, Value : {value}", ex);
            }
        }

        public string GetValue(Category category, Key key)
        {
            string sReturn = string.Empty;
            try
            {
                lock (LockObj)
                {
                    if (LoginType.GOV == LoginType)
                    {
                        if (category == Category.ObjectStorage && key == Key.Endpoint)
                            key = Key.EndpointGov;
                        if (category == Category.ApiGateway && key == Key.Endpoint)
                            key = Key.EndpointGov;
                    }

                    sReturn = dicData[new Tuple<Category, Key>(category, key)];
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Categoy : {category}, Key : {key}", ex);
            }
            return sReturn;
        }

        public async Task<string> Execute(string cmd, string cmdType, string cmdText, CsLib.RequestType type, string endPoint, string action, string accessKey, string secretKey, int timeoutSec = 0)
        {
            var json = new
            {
                cmd = cmd,
                cmdType = cmdType,
                cmdText = TranString.EncodeBase64Unicode(cmdText)
            };

            string responseString = string.Empty;
            try
            {
                string jsonCmd = JsonConvert.SerializeObject(json);
                Task<string> response = new SoaCall().WebApiCall(
                    endPoint,
                    type,
                    action,
                    jsonCmd,
                    accessKey,
                    secretKey,
                    timeoutSec
                    );
                string temp = await response;
                if (temp.Length > 0)
                {
                    JToken jt = JToken.Parse(temp);
                    responseString = jt.ToString(Newtonsoft.Json.Formatting.Indented);
                }
                else
                    responseString = "response is empty...";
            }
            catch (Exception)
            {
                throw;
            }
            return responseString;
        }

        #endregion

        #region etc
        object LockObj = new object();
        static string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
        static string dataManagerContentsInitFilename = "DataManagerContentsInit.txt";
        static string encryptedDataManagerContentsFilename = "DataManagerContentsUser.txt";
        static string encryptionKeyFilename = "CryptionKey.txt";
        string key = string.Empty;
        string encryptionKeyFullFilename = Path.Combine(path, encryptionKeyFilename);
        string dataManagerContentsInitFullFilename = Path.Combine(path, dataManagerContentsInitFilename);
        string encryptedDataManagerContentsFullFilename = Path.Combine(path, encryptedDataManagerContentsFilename);

        private List<string> listData;
        private Dictionary<Tuple<Category, Key>, string> dicData;
        #endregion

        #region enum
        public enum DataType { Plain, Base64Unicode }
        public enum Category
        {
            Login,
            ObjectStorage,
            ApiGateway,
            LoginKey,
            InitScript,
            SetSql,
            SetDisk,
            LoadBalancer,
            HighAvailability,


            Server, GeneralInfo, ServerInfo,
            ServerSetting,
            createServerInstances,
            createLoadBalancerInstance,
            getLoadBalancerInstanceList,
            getServerInstanceList,
            createPublicIpInstance,
            getRegionList,
            getZoneList,
            getServerImageProductList,
            getServerProductList,
            getLoginKeyList,
            createLoginKey,
            getAccessControlGroupList,
            SqlSetting,
            CreateServer,
            createBlockStorageInstance,
            changeLoadBalancedServerInstances,
            getLoadBalancerTargetServerInstanceList,
            SetHa
        }
        public enum Key
        {
            IsSaveKeyYn,
            Endpoint,
            EndpointGov,
            LoginType,
            Bucket,
            Name,
            AgentFolder, PsFileName,
            userData, PsContents, userDataFinal,
            Id, Password, Port, Collation, TraceFlags, Sp_configure, PsTemplate,
            Size, Type, PsPartitionFormat, /*PsGetDisk,*/
            Protocol, LoadBalancerPort,
            BackupRestorePath,
            AccessKey, SecretKey,
            zoneNo, regionNo, serverProductCode, serverImageProductCode, serverName, loginKeyName, feeSystemTypeCode,
            accessControlGroupConfigurationNoList_1, accessControlGroupConfigurationNoList_2, accessControlGroupConfigurationNoList_3, accessControlGroupConfigurationNoList_4, accessControlGroupConfigurationNoList_5,


            AclGroupNo, LoginKeyName, RegionNo, ServerProductCode, ServerImageProductCode,
            FeeSystemTypeCode,
            HaGroupName, HaDomainName, Server1Name, Server1ZoneNo,
            Server2Name, Server2ZoneNo, AddDiskSizeGB,
            Action, Template,

            checkdisk, loadBalancerName, protocolTypeCode,
            serverNameA, serverNameB, masterServerName
        }

        public LoginType LoginType
        {
            get;set;

        }
        #endregion
    }
}
