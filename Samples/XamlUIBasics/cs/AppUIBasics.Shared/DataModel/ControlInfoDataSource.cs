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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

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
        public ControlInfoDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, bool isNew)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.IsNew = isNew;
            this.Docs = new ObservableCollection<ControlInfoDocLink>();
            this.RelatedControls = new ObservableCollection<string>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }
        public bool IsNew { get; private set; }
        public ObservableCollection<ControlInfoDocLink> Docs { get; private set; }
        public ObservableCollection<string> RelatedControls { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public class ControlInfoDocLink
    {
        public ControlInfoDocLink(string title, string uri)
        {
            this.Title = title;
            this.Uri = uri;
        }
        public string Title { get; private set; }
        public string Uri { get; private set; }
    }


    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class ControlInfoDataGroup
    {
        public ControlInfoDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<ControlInfoDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<ControlInfoDataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
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

			var name = this.GetType().GetTypeInfo().Assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("ControlInfoData.json"));

			if(name == null)
			{
				throw new InvalidOperationException($"Unable to find ControlInfoData.json in embedded resources");
			}

			using (var s = new StreamReader(this.GetType().GetTypeInfo().Assembly.GetManifestResourceStream(name)))
			{
				var jsonObject = JObject.Parse(s.ReadToEnd());

				var jsonArray = jsonObject["Groups"];

				lock (_lock)
				{
					foreach (JObject groupObject in jsonArray)
					{
						var group = new ControlInfoDataGroup(groupObject["UniqueId"].Value<string>(),
																			  groupObject["Title"].Value<string>(),
																			  groupObject["Subtitle"].Value<string>(),
																			  groupObject["ImagePath"].Value<string>(),
																			  groupObject["Description"].Value<string>());

						foreach (JObject itemValue in groupObject["Items"])
						{
							var itemObject = itemValue;
							var item = new ControlInfoDataItem(itemObject["UniqueId"].Value<string>(),
																	itemObject["Title"].Value<string>(),
																	itemObject["Subtitle"].Value<string>(),
																	itemObject["ImagePath"].Value<string>(),
																	itemObject["Description"].Value<string>(),
																	itemObject["Content"].Value<string>(),
																	itemObject["IsNew"].Value<bool>());

							if (itemObject.ContainsKey("Docs"))
							{
								foreach (JObject docValue in itemObject["Docs"])
								{
									var docObject = docValue;
									item.Docs.Add(new ControlInfoDocLink(docObject["Title"].Value<string>(), docObject["Uri"].Value<string>()));
								}
							}

							if (itemObject.ContainsKey("RelatedControls"))
							{
								foreach (JValue relatedControlValue in itemObject["RelatedControls"])
								{
									item.RelatedControls.Add(relatedControlValue.Value<string>());
								}
							}

							group.Items.Add(item);
						}

						if (!Groups.Any(g => g.Title == group.Title))
						{
							Groups.Add(group);
						}
					}
				}
			}

#if false
			lock (_lock)
            {
                if (this.Groups.Count() != 0)
                {
                    return;
                }
            }

            Uri dataUri = new Uri("ms-appx:///DataModel/ControlInfoData.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);

            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Groups"].GetArray();

            lock (_lock)
            {
                foreach (JsonValue groupValue in jsonArray)
                {
                    JsonObject groupObject = groupValue.GetObject();
                    ControlInfoDataGroup group = new ControlInfoDataGroup(groupObject["UniqueId"].Value<string>(),
                                                                          groupObject["Title"].Value<string>(),
                                                                          groupObject["Subtitle"].Value<string>(),
                                                                          groupObject["ImagePath"].Value<string>(),
                                                                          groupObject["Description"].Value<string>());

                    foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                    {
                        JsonObject itemObject = itemValue.GetObject();
                        var item = new ControlInfoDataItem(itemObject["UniqueId"].Value<string>(),
                                                                itemObject["Title"].Value<string>(),
                                                                itemObject["Subtitle"].Value<string>(),
                                                                itemObject["ImagePath"].Value<string>(),
                                                                itemObject["Description"].Value<string>(),
                                                                itemObject["Content"].Value<string>(),
                                                                itemObject["IsNew"].GetBoolean());

                        if (itemObject.ContainsKey("Docs"))
                        {
                            foreach (JsonValue docValue in itemObject["Docs"].GetArray())
                            {
                                JsonObject docObject = docValue.GetObject();
                                item.Docs.Add(new ControlInfoDocLink(docObject["Title"].Value<string>(), docObject["Uri"].Value<string>()));
                            }
                        }

                        if (itemObject.ContainsKey("RelatedControls"))
                        {
                            foreach (JsonValue relatedControlValue in itemObject["RelatedControls"].GetArray())
                            {
                                item.RelatedControls.Add(relatedControlValue.Value<string>());
                            }
                        }

                        group.Items.Add(item);
                    }

                    if (!Groups.Any(g => g.Title == group.Title))
                    {
                        Groups.Add(group);
                    }
                }
            }
#endif
		}
    }
}
