﻿using Roadkill.Core.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// Contains all settings that require an application (appdomain) restart when changed - typically stored in a .config file.
	/// </summary>
	public class ApplicationSettings
	{
		private string _attachmentsFolder;
		private string _attachmentsDirectoryPath;
		private string _attachmentsUrlPath;
		private string _attachmentsRoutePath;
		//private readonly HttpContextBase _httpContext;

		/// <summary>
		/// The name of the role or Active Directory security group that users should belong to in order to create,edit,delete pages,
		/// manage users, manage site settings and use the admin tools.
		/// </summary>
		public string AdminRoleName { get; set; }

		/// <summary>
		/// The path to the App_Data folder.
		/// </summary>
		public string AppDataPath { get; set; }

		/// <summary>
		/// The path to the App_Data/Internal folder (used by roadkill only, no user files are stored here).
		/// </summary>
		public string AppDataInternalPath { get; private set; }

		/// <summary>
		/// Contains a list API keys for the REST api. If this is empty, then the REST api is disabled.
		/// </summary>
		public IEnumerable<string> ApiKeys { get; set; }

		/// <summary>
		/// The folder where all uploads (typically image files) are saved to. This is taken from the web.config.
		/// Use AttachmentsDirectoryPath for the absolute directory path.
		/// </summary>
		public string AttachmentsFolder
		{
			get
			{
				return _attachmentsFolder;
			}
			set
			{
				_attachmentsFolder = value;
				_attachmentsDirectoryPath = "";
			}
		}

		/// <summary>
		/// The absolute file path for the attachments folder. If the AttachmentsFolder uses "~/" then the path is 
		/// translated into one that is relative to the site root, otherwise it is assumed to be an absolute file path.
		/// This property always contains a trailing slash (or / on Unix based systems).
		/// </summary>
		public string AttachmentsDirectoryPath
		{
			get
			{
				if (string.IsNullOrEmpty(_attachmentsDirectoryPath))
				{
					// TODO: NETStandard
					if (false)//AttachmentsFolder.StartsWith("~") && _httpContext != null)
					{
						//_attachmentsDirectoryPath = _httpContext.Server.MapPath(AttachmentsFolder);
					}
					else
					{
						_attachmentsDirectoryPath = AttachmentsFolder;
					}
				}

				if (!_attachmentsDirectoryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
					_attachmentsDirectoryPath += Path.DirectorySeparatorChar.ToString();

				return _attachmentsDirectoryPath;
			}
		}

		/// <summary>
		/// Gets the full URL path for the attachments folder, including any extra application paths from the url.
		/// Contains a "/" the start and does not contain a trailing "/".
		/// </summary>
		public string AttachmentsUrlPath
		{
			get
			{
				if (string.IsNullOrEmpty(_attachmentsUrlPath))
				{
					_attachmentsUrlPath = CreateAttachmentsPath();
				}

				return _attachmentsUrlPath;
			}
		}

		/// <summary>
		/// The route used for all attachment HTTP requests. This contains no starting or ending "/".
		/// </summary>
		public string AttachmentsRoutePath
		{
			get
			{
				return _attachmentsRoutePath;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("AttachmentsRoutePath", "The AttachmentsRoutePath cannot be null. Please ensure it's set in roadkill.config.");
				}

				_attachmentsRoutePath = StripSlashesFromAttachmentRoute(value);
				_attachmentsUrlPath = CreateAttachmentsPath();
			}
		}

		/// <summary>
		/// ConnectionString for azure blob storage
		/// TODO: comments + tests
		/// </summary>
		public string AzureConnectionString { get; set; }

		/// <summary>
		/// Azure storage container for attachments
		/// TODO: comments + tests
		/// </summary>
		public string AzureContainer { get; set; }

		/// <summary>
		/// The connection string to the Roadkill database.
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// The connection string name (held in the connection strings section of the config file) for the Roadkill database.
		/// </summary>
		public string ConnectionStringName { get; set; }

		/// <summary>
		/// The file path for the custom tokens file.
		/// </summary>
		public string CustomTokensPath { get; set; }

		/// <summary>
		/// The database used for storage.
		/// </summary>
		public string DatabaseName { get; set; }

		/// <summary>
		/// The name of the role or Active Directory security group that users should belong to in order to create and edit pages.
		/// </summary>
		public string EditorRoleName { get; set; }

		/// <summary>
		/// The path to the email templates folder, ~/App_Data/EmailTemplates/ by default.
		/// </summary>
		public string EmailTemplateFolder { get; set; }

		/// <summary>
		/// The file path for the html element white list file.
		/// </summary>
		public string HtmlElementWhiteListPath { get; set; }

		/// <summary>
		/// Whether errors in updating the lucene index throw exceptions or are just ignored.
		/// </summary>
		public bool IgnoreSearchIndexErrors { get; set; }

		/// <summary>
		/// Whether the site is public, i.e. all pages are visible by default. This is optional in the web.config and the default is true.
		/// </summary>
		public bool IsPublicSite { get; set; }

		/// <summary>
		/// If this instance is running on the demo site.
		/// </summary>
		internal bool IsDemoSite
		{
			get
			{
				return false;
				// TODO: NETStandard
				//return ConfigurationManager.AppSettings["DemoSite"] == "true";
			}
		}

		/// <summary>
		/// Whether the REST api is available - if api keys are set in the config.
		/// </summary>
		public bool IsRestApiEnabled
		{
			get
			{
				return ApiKeys != null && ApiKeys.Any();
			}
		}

		/// <summary>
		/// Indicates whether the installation has been completed previously.
		/// </summary>
		public bool Installed { get; set; }

		/// <summary>
		/// The connection string for Active Directory server if <see cref="UseWindowsAuthentication"/> is true.
		/// This should start with LDAP:// in uppercase.
		/// </summary>
		public string LdapConnectionString { get; set; }

		/// <summary>
		/// The username to authenticate against the Active Directory with, if <see cref="UseWindowsAuthentication"/> is true.
		/// </summary>
		public string LdapUsername { get; set; }

		/// <summary>
		/// The password to authenticate against the Active Directory with, if <see cref="UseWindowsAuthentication"/> is true.
		/// </summary>
		public string LdapPassword { get; set; }

		/// <summary>
		/// The number of characters each password should be.
		/// </summary>
		public int MinimumPasswordLength { get; set; }

		/// <summary>
		/// The full path to the nlog.config file - this defaults to ~/App_Data/NLog.Config (the ~ is replaced with the base web directory).
		/// </summary>
		public string NLogConfigFilePath { get; set; }

		/// <summary>
		/// The full path to the text plugins directory. This is where plugins are stored after 
		/// download (including their nuget files), and are copied to the bin folder.
		/// </summary>
		public string PluginsPath { get; internal set; }

		/// <summary>
		/// The directory within the /bin folder that the plugins are stored. They are 
		/// copied here on application start, so they can be loaded into the application domain with shadow 
		/// copy support and also monitored by the ASP.NET file watcher.
		/// </summary>
		public string PluginsBinPath { get; internal set; }

		/// <summary>
		/// The path to the folder that contains the Lucene index - ~/App_Data/Internal/Search.
		/// </summary>
		public string SearchIndexPath { get; set; }

		/// <summary>
		/// Indicates whether to use Local storage or Azure for attachments
		/// </summary>
		public bool UseAzureFileStorage { get; set; }

		/// <summary>
		/// Indicates whether server-based page object caching is enabled.
		/// </summary>
		public bool UseObjectCache { get; set; }

		/// <summary>
		/// Indicates whether to send HTTP cache headers to the browser (304 not modified)
		/// </summary>
		public bool UseBrowserCache { get; set; }

		/// <summary>
		/// Gets a value indicating whether the html that is converted from the markup is 
		/// cleaned for tags, using the App_Data/htmlwhitelist.xml file.
		/// </summary>
		public bool UseHtmlWhiteList { get; set; }

		/// <summary>
		/// The type for the <see cref="UserServiceBase"/>. If the setting for this is blank
		/// in the web.config, then the <see cref="UseWindowsAuthentication"/> is checked and if false
		/// a <see cref="FormsAuthUserService"/> is created. The format of this setting can be retrieved by
		/// using <code>typeof(YourUserService).FullName.</code>
		/// </summary>
		public string UserServiceType { get; set; }

		/// <summary>
		/// Gets a value indicating whether this windows authentication is being used.
		/// </summary>
		public bool UseWindowsAuthentication { get; set; }

		/// <summary>
		/// The human-friendly current Roadkill product version, e.g. "1.7.0-Beta3".
		/// </summary>
		public static string ProductVersion
		{
			get
			{
				return "NETSTandard";
				// TODO: NETStandard
				//return FileVersionInfo.GetVersionInfo(typeof(ApplicationSettings).Assembly.Location).ProductVersion;
			}
		}

		/// <summary>
		/// The file version of the Roadkill product version, e.g. "1.7.0.0"
		/// </summary>
		public static string FileVersion
		{
			get
			{
				return "999";
				// TODO: NETStandard
				//return FileVersionInfo.GetVersionInfo(typeof(ApplicationSettings).Assembly.Location).FileVersion;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationSettings"/> class.
		/// </summary>
		public ApplicationSettings()
		{
			// TODO: NETStandard
			//if (HttpContext.Current != null)
			//	_httpContext = new HttpContextWrapper(HttpContext.Current);

			AppDataPath = "";//Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
			AppDataInternalPath = Path.Combine(AppDataPath, "Internal");
			ApiKeys = new List<string>();
			CustomTokensPath = Path.Combine(AppDataPath, "customvariables.xml");
			EmailTemplateFolder = Path.Combine(AppDataPath, "EmailTemplates");
			HtmlElementWhiteListPath = Path.Combine(AppDataInternalPath, "htmlwhitelist.xml");
			MinimumPasswordLength = 6;
			NLogConfigFilePath = "~/App_Data/NLog.config";
			DatabaseName = SupportedDatabases.SqlServer2008.Id;
			AttachmentsRoutePath = "Attachments";
			AttachmentsFolder = "~/App_Data/Attachments";
			SearchIndexPath = Path.Combine(AppDataInternalPath, "Search");

			// TODO: NETStandard
			PluginsBinPath = "";//Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Plugins");
			PluginsPath = "";//Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationSettings"/> class.
		/// </summary>
		/// <param name="httpContext">The HTTP context.</param>
		//internal ApplicationSettings(HttpContextBase httpContext) : this()
		//{
		//	// TODO: NETStandard
		//	//_httpContext = httpContext;
		//}

		private string CreateAttachmentsPath()
		{
			string attachmentsPath = "/" + AttachmentsRoutePath;
			string applicationPath = "";

			// TODO: NETStandard
			if (true)//_httpContext != null)
			{
				//applicationPath = _httpContext.Request.ApplicationPath;
			}

			if (!applicationPath.EndsWith("/"))
				applicationPath += "/";

			if (attachmentsPath.StartsWith("/"))
				attachmentsPath = attachmentsPath.Remove(0, 1);

			attachmentsPath = applicationPath + attachmentsPath;

			return attachmentsPath;
		}

		private string StripSlashesFromAttachmentRoute(string route)
		{
			if (route.StartsWith("/"))
			{
				route = route.Remove(0, 1);
			}

			if (route.EndsWith("/"))
			{
				route = route.Remove(route.Length - 1, 1);
			}

			return route;
		}
	}
}