//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Json;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.Serialization.Json;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app
// is first launched.

namespace AppUIBasics.Data
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class ControlInfoDataItem
    {
        public ControlInfoDataItem(string uniqueId, string title, string subtitle, string imagePath, string description, string content, bool isNew, bool isCurrentlySupported)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.IsNew = isNew;
            this.IsCurrentlySupported = isCurrentlySupported;
		}

        public string UniqueId { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string Description { get; }
        public string ImagePath { get; }
        public string Content { get; }
        public bool IsNew { get; }
        public bool IsCurrentlySupported { get; }
		public ObservableCollection<ControlInfoDocLink> Docs { get; } = new ObservableCollection<ControlInfoDocLink>();
		public ObservableCollection<string> RelatedControls { get; } = new ObservableCollection<string>();

		public override string ToString() => this.Title;
    }

    public class ControlInfoDocLink
    {
        public ControlInfoDocLink(string title, string uri)
        {
            this.Title = title;
            this.Uri = uri;
        }

        public string Title { get; }
        public string Uri { get; }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class ControlInfoDataGroup
    {
        public ControlInfoDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description, bool isHidden)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
			this.IsHidden = isHidden;
        }

        public string UniqueId { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string Description { get; }
        public string ImagePath { get; }
		public bool IsHidden { get; }
        public ObservableCollection<ControlInfoDataItem> Items { get; } = new ObservableCollection<ControlInfoDataItem>();

		public override string ToString() => this.Title;
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    ///
    /// ControlInfoSource initializes with data read from a static json file included in the
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class ControlInfoDataSource
    {
        private static readonly object _lock = new object();

        #region Singleton

        private static ControlInfoDataSource _instance;

        public static ControlInfoDataSource Instance
        {
            get
            {
                return _instance;
            }
        }

        static ControlInfoDataSource()
        {
            _instance = new ControlInfoDataSource();
        }

        private ControlInfoDataSource() { }

        #endregion

        private IList<ControlInfoDataGroup> _groups = new List<ControlInfoDataGroup>();
        public IList<ControlInfoDataGroup> Groups
        {
            get { return this._groups; }
        }

        public async Task<IEnumerable<ControlInfoDataGroup>> GetGroupsAsync()
        {
            await _instance.GetControlInfoDataAsync();

            return _instance.Groups;
        }

        public async Task<ControlInfoDataGroup> GetGroupAsync(string uniqueId)
        {
            await _instance.GetControlInfoDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _instance.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public async Task<ControlInfoDataItem> GetItemAsync(string uniqueId)
        {
            await _instance.GetControlInfoDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _instance.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() > 0) return matches.First();
            return null;
        }

        public async Task<ControlInfoDataGroup> GetGroupFromItemAsync(string uniqueId)
        {
            await _instance.GetControlInfoDataAsync();
            var matches = _instance.Groups.Where((group) => group.Items.FirstOrDefault(item => item.UniqueId.Equals(uniqueId)) != null);
            if (matches.Count() == 1) return matches.First();
            return null;
        }

		private async Task GetControlInfoDataAsync()
		{
			lock (_lock)
			{
				if (this.Groups.Count() != 0)
				{
					return;
				}
			}

#if true
			var name = this.GetType().GetTypeInfo().Assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("ControlInfoData.json"));

			if (name == null)
			{
				throw new InvalidOperationException($"Unable to find ControlInfoData.json in embedded resources");
			}

			using (var s = new StreamReader(this.GetType().GetTypeInfo().Assembly.GetManifestResourceStream(name)))
			{
				var jsonValue = JsonValue.Load(s);

				lock (_lock)
				{
					var groups = ParseJson(jsonValue);
					RemoveNotCurrentlySupportedControl(groups);

					groups.ForEach(Groups.Add);
				}
			}
#else
			Uri dataUri = new Uri("ms-appx:///DataModel/ControlInfoData.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);

            var jsonValue = JsonValue.Parse(jsonText);

            lock (_lock)
			{
				var groups = ParseJson(jsonValue);
				RemoveNotCurrentlySupportedControl(groups);

				groups.ForEach(Groups.Add);
			}
#endif
		}

		private List<ControlInfoDataGroup> ParseJson(JsonValue jsonValue)
		{
			var results = new List<ControlInfoDataGroup>();

			foreach (var groupObject in jsonValue["Groups"].OfType<JsonObject>())
			{
				var group = new ControlInfoDataGroup(
					groupObject["UniqueId"],
					groupObject["Title"],
					groupObject["Subtitle"],
					groupObject["ImagePath"],
					groupObject["Description"],
					groupObject["IsHidden"]
				);

				foreach (var itemObject in groupObject["Items"].OfType<JsonObject>())
				{
					var item = new ControlInfoDataItem(
						itemObject["UniqueId"],
						itemObject["Title"],
						itemObject["Subtitle"],
						itemObject["ImagePath"],
						itemObject["Description"],
						itemObject["Content"],
						itemObject["IsNew"],
						itemObject["IsCurrentlySupported"]
					);

					if (itemObject.ContainsKey("Docs"))
					{
						foreach (JsonValue docValue in itemObject["Docs"])
						{
							if (docValue is JsonObject docObject)
							{
								item.Docs.Add(new ControlInfoDocLink(docObject["Title"], docObject["Uri"]));
							}
						}
					}

					if (itemObject.ContainsKey("RelatedControls"))
					{
						foreach (JsonValue relatedControlValue in itemObject["RelatedControls"])
						{
							item.RelatedControls.Add(relatedControlValue);
						}
					}

					group.Items.Add(item);
				}

				if (!Groups.Any(g => g.Title == group.Title))
				{
					results.Add(group);
				}
			}

			return results;
		}

		private void RemoveNotCurrentlySupportedControl(IList<ControlInfoDataGroup> groups)
		{
			// remove hidden groups
			foreach	(var group in groups.Where(x => x.IsHidden).ToArray())
			{
				groups.Remove(group);
			}

			// remove not supported controls
			foreach (var group in groups)
			{
				foreach (var item in group.Items.Where(x => !x.IsCurrentlySupported).ToArray())
				{
					group.Items.Remove(item);
				}
			}

			// remove not supported controls from ControlInfoDataItem.RelatedControls
			var supportedControls = groups
				.SelectMany(x => x.Items)
				.Select(x => x.UniqueId)
				.ToList();
			foreach (var item in groups.SelectMany(x => x.Items))
			{
				foreach (var relatedControl in item.RelatedControls.Where(x => !supportedControls.Contains(x)).ToArray())
				{
					item.RelatedControls.Remove(relatedControl);
				}
			}
		}
	}
}