using DirectScaleBraintree;
using Dapper;
using DirectScale.Braintree.Models;
using DirectScale.Disco.Extension.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DirectScale.Braintree.Repositories
{
    public class DataRepository
    {
        const double SETTINGS_REFRESH_THRESHHOLD = 60.0;
        private readonly IDataService _dataService;
        private readonly ILoggingService _logger;
        private readonly ISettingsService _settingsService;

        private static Dictionary<int, BraintreeSettings> SettingStore;

        public DataRepository(IDataService dataService, ILoggingService logger, ISettingsService settingsService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            if (SettingStore == null) SettingStore = new Dictionary<int, BraintreeSettings>();
        }
        
        public BraintreeSettings GetSettings(int discoMerchId)
        {
            // Check for the populated settings and check their age.
            // If it's older than X minutes, re-fetch.
            // This is my attempt to ensure it's fresh across all instances.
            if (SettingStore.ContainsKey(discoMerchId))
            {
                double age = SettingStore[discoMerchId].GetSettingsAgeInSeconds();
                if (age > SETTINGS_REFRESH_THRESHHOLD) SettingStore.Remove(discoMerchId);
            }
            
            if (!SettingStore.ContainsKey(discoMerchId))
            {
                var newSettings = new BraintreeSettings(discoMerchId);
                var isLive = _settingsService.ExtensionContext().EnvironmentType == Disco.Extension.EnvironmentType.Live;
                newSettings = LoadBraintreeSettingsFromDB(newSettings, discoMerchId, isLive);
                SettingStore.Add(discoMerchId, newSettings);
            }
            return SettingStore[discoMerchId];
        }
        
        public static void ClearSettings(int discoMerchId)
        {
            SettingStore.Remove(discoMerchId);
        }

        private BraintreeSettings LoadBraintreeSettingsFromDB(BraintreeSettings settings, int discoMerchantId, bool isLive)
        {
            string settingGroupName = null;
            switch (discoMerchantId)
            {
                case BraintreeSettings.BRAINTREE_CAD_DISCO_MERCHANTID:
                    settingGroupName = "BRAINTREECAD";
                    break;
                case BraintreeSettings.BRAINTREE_USD_DISCO_MERCHANTID:
                    settingGroupName = "BRAINTREEUSD";
                    break;
                case BraintreeSettings.BRAINTREE_AUD_DISCO_MERCHANTID:
                    settingGroupName = "BRAINTREEAUD";
                    break;
                case BraintreeSettings.BRAINTREE_GBP_DISCO_MERCHANTID:
                    settingGroupName = "BRAINTREEGBP";
                    break;
                case BraintreeSettings.BRAINTREE_NZD_DISCO_MERCHANTID:
                    settingGroupName = "BRAINTREENZD";
                    break;
                case BraintreeSettings.BRAINTREE_EUR_DISCO_MERCHANTID:
                    settingGroupName = "BRAINTREEEUR";
                    break;
            }

            IEnumerable<ExtensionSetting> settingList = null;

            try
            {
                using (var dbConnection = new SqlConnection(_dataService.ClientConnectionString.ConnectionString))
                {
                    var query = $"SELECT SettingGroup, SettingKey, SettingValue FROM Client.Settings WHERE SettingGroup = '{settingGroupName}' AND (IsLive = {(isLive ? "1" : "0 OR IsLive IS NULL")})";
                    settingList = dbConnection.Query<ExtensionSetting>(query);
                }
            } catch (Exception)
            {
                _logger.LogInformation("The Braintree settings do not include a database table, or a SQL exception occurred.");
            }

            // If nothing is in the table, or if the table doesn't exist, just scrap it.
            if (settingList != null && settingList.Any())
            {
                _logger.LogInformation("Braintree: Database settings found. Updating settings accordingly.");
                PopulateBraintreeKeyValuesFromResults(settingList, settings);
            }
           
            return settings;
        }

        /// <summary>
        /// If you're going to use DB settings, you have to use them completely. If you set the Use DB Entries flag, you've got to set the rest too.
        /// If you do NOT set that flag, or there are no DB entries at all, the DB read will not override the values passed into the constructor.
        /// There are a few scenarios: 
        /// 1. User is calling this without DB entries, and has selected to use Hard Coded Test Creds in the constructor
        /// 2. User is calling with DB entries, and opted to not use DB settings.
        /// 3. User is calling with DB entries, and opted to use DB settings.
        /// </summary>
        /// <param name="settingList"></param>
        /// <param name="settings"></param>
        public void PopulateBraintreeKeyValuesFromResults(IEnumerable<ExtensionSetting> settingList, BraintreeSettings settings)
        {
            settings.UseDatabaseSettings = "1TRUE".Contains(settingList.FirstOrDefault(x => x.SettingKey == "BraintreeUseDBSettings").SettingValue?.ToUpper());
            settings.UseDirectScaleHardCodedCreds = !settings.UseDatabaseSettings; // Until we check to see if that's what the user wants in the DB settings :) 
            settings.HasLoadedDBValues = true;

            if (settings.UseDatabaseSettings) // If the client does NOT want to use these settings, then the ones passed in at Client Extension instantiation will be used. 
            {
                //_logger.LogInformation("Braintree: Using database settings. Clearing all passed-in values.");
                settings.ClearSettings(); // We're going to get settings from the DB. Clear out the ones passed in.
                foreach (var setting in settingList) 
                {
                    switch (setting.SettingKey)
                    {
                        case "BraintreeEnvironment":
                            settings.BraintreeEnvironment = setting.SettingValue;
                            break;
                        case "BraintreeUseDirectScaleSandbox":
                            settings.UseDirectScaleHardCodedCreds = "1TRUE".Contains(setting.SettingValue?.ToUpper());
                            break;
                        case "BraintreeMerchantId":
                            settings.BraintreeMerchantId = setting.SettingValue;
                            break;
                        case "BraintreePublicKey":
                            settings.PublicKey = setting.SettingValue;
                            break;
                        case "BraintreePrivateKey":
                            settings.PrivateKey = setting.SettingValue;
                            break;
                        case "BraintreeTokenizationKey":
                            settings.TokenizationKey = setting.SettingValue;
                            break;
                        case "BraintreePayPalClientId":
                            settings.PayPalClientId = setting.SettingValue;
                            break;
                        case "BraintreePayPalSecret":
                            settings.PayPalSecret = setting.SettingValue;
                            break;
                        case "BraintreeGooglePayMerchantId":
                            settings.GooglePayMerchantId = setting.SettingValue;
                            break;
                        case "BraintreeMerchantAccountId":
                            settings.BraintreeMerchantAccountId = setting.SettingValue;
                            break;
                        case "BraintreeDropInUiMethods":
                            settings.DropInUiMethods = setting.SettingValue;
                            break;
                        case "BraintreeIFrameDimensions":
                            settings.IFrameDimensions = setting.SettingValue;
                            break;
                        case "BraintreeMinimumCurrencyAmount":
                            double.TryParse(setting.SettingValue, out double currencyAmount);
                            settings.MinimumCurrencyAmount = currencyAmount;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
/*
 * IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'[Client].[FlexPay_Orders]' AND type = 'U')
                    BEGIN
	                    CREATE TABLE [Client].[Settings]
	                    (
		                    [recordnumber] int NOT NULL IDENTITY(1, 1),
		                    [last_modified] DATETIME CONSTRAINT DF_Settings_last_modified DEFAULT (GETDATE()) NOT NULL,
		                    [SettingGroup] varchar(20) NOT NULL,
		                    [SettingKey] varchar(30) NOT NULL,
		                    [SettingValue] varchar(200) NOT NULL,
		                    CONSTRAINT [ClientSettings_PrimaryKey] PRIMARY KEY CLUSTERED 
			                    (
				                    [recordnumber]
			                    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	                    );
                    END 

insert into Client.Settings(SettingGroup, SettingKey, SettingValue)
  values 
	('BRAINTREEUSD', 'BraintreeEnvironment', ''),
	('BRAINTREEUSD', 'BraintreeUseDirectScaleSandbox', ''),
	('BRAINTREEUSD', 'BraintreeMerchantId', ''),
	('BRAINTREEUSD', 'BraintreePublicKey', ''),
	('BRAINTREEUSD', 'BraintreePrivateKey', ''),
	('BRAINTREEUSD', 'BraintreeTokenizationKey', ''),
	('BRAINTREEUSD', 'BraintreePayPalClientId', ''),
	('BRAINTREEUSD', 'BraintreePayPalSecret', ''),
	('BRAINTREEUSD', 'BraintreeGooglePayMerchantId', ''),
	('BRAINTREEUSD', 'BraintreeMerchantAccountId', ''),
	('BRAINTREEUSD', 'BraintreeDropInUiMethods', ''),
	('BRAINTREEUSD', 'BraintreeIFrameDimensions', ''),
    ('BRAINTREEUSD', 'BraintreeUseDBSettings', '')

// Default DS Settings
insert into Client.Settings(SettingGroup, SettingKey, SettingValue)
  values 
	('BRAINTREEUSD', 'BraintreeUseDirectScaleSandbox', 'true'),
	('BRAINTREEUSD', 'BraintreeEnvironment', 'sandbox'),
	('BRAINTREEUSD', 'BraintreeMerchantId', 's8x92wg4v8qvq45j'),
	('BRAINTREEUSD', 'BraintreePublicKey', '8rfxmf6fh7bhkrqt'),
	('BRAINTREEUSD', 'BraintreePrivateKey', 'b6e3e6c098963479ad754e56fb7c52da'),
	('BRAINTREEUSD', 'BraintreeTokenizationKey', 'sandbox_ktv3dxf7_s8x92wg4v8qvq45j'),
	('BRAINTREEUSD', 'BraintreePayPalClientId', 'AVeR5GW2uCj2nbF-JJXUTGh3tnHMeatsti_ei_WCletXP75iQLPoQnEJhOO3JT9c82B-n1PcqzO5Hqtp'),
	('BRAINTREEUSD', 'BraintreePayPalSecret', 'EP5Mjrtg8Kl82HHiQFWidWMJIgIwrWmESF0a8-c9KjfjWTYOLb0VwFCLZJzc68QF9Chvg8GuJCU5JkFY'),
	('BRAINTREEUSD', 'BraintreeGooglePayMerchantId', ''),
	('BRAINTREEUSD', 'BraintreeMerchantAccountId', 'directscale'),
	('BRAINTREEUSD', 'BraintreeDropInUiMethods', 'PAYPAL,PPCREDIT,VENMO,APPLEPAY,GOOGLEPAY'),
	('BRAINTREEUSD', 'BraintreeIFrameDimensions', '400x550'),
    ('BRAINTREEUSD', 'BraintreeUseDBSettings', 'true')

*/

