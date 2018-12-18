﻿using AppUIBasics.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AppUIBasics.ControlPages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SplitViewPage : Page
	{
		private ObservableCollection<NavLink> _navLinks = new ObservableCollection<NavLink>()
		{
			new NavLink() { Label = "People", Symbol = Windows.UI.Xaml.Controls.Symbol.People  },
			new NavLink() { Label = "Globe", Symbol = Windows.UI.Xaml.Controls.Symbol.Globe },
			new NavLink() { Label = "Message", Symbol = Windows.UI.Xaml.Controls.Symbol.Message },
			new NavLink() { Label = "Mail", Symbol = Windows.UI.Xaml.Controls.Symbol.Mail },
		};

		public ObservableCollection<NavLink> NavLinks
		{
			get { return _navLinks; }
		}

		public SplitViewPage()
		{
			this.InitializeComponent();
		}

		private void togglePaneButton_Click(object sender, RoutedEventArgs e)
		{
			if (Windows.UI.Xaml.Window.Current.Bounds.Width >= 640)
			{
				if (splitView.IsPaneOpen)
				{
					splitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
					splitView.IsPaneOpen = false;
				}
				else
				{
					splitView.IsPaneOpen = true;
					splitView.DisplayMode = SplitViewDisplayMode.Inline;
				}
			}
			else
			{
				splitView.IsPaneOpen = !splitView.IsPaneOpen;
			}
		}

		private void PanePlacement_Toggled(object sender, RoutedEventArgs e)
		{
			var ts = sender as ToggleSwitch;
			if (ts.IsOn)
			{
				splitView.PanePlacement = SplitViewPanePlacement.Right;
			}
			else
			{
				splitView.PanePlacement = SplitViewPanePlacement.Left;
			}
		}

		private void NavLinksList_ItemClick(object sender, ItemClickEventArgs e)
		{
			content.Text = (e.ClickedItem as NavLink).Label + " Page";
		}

		//private void displayModeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		//{
		//	splitView.DisplayMode = (SplitViewDisplayMode)Enum.Parse(typeof(SplitViewDisplayMode), (e.AddedItems[0] as ComboBoxItem).Content.ToString());
		//}
		//private void paneBackgroundCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		//{
		//	var colorString = (e.AddedItems[0] as ComboBoxItem).Content.ToString();

		//	VisualStateManager.GoToState(this, colorString, false);
		//}

		private void DisplayMode_Click(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement fe && Enum.TryParse<SplitViewDisplayMode>(fe.Tag?.ToString(), out var mode))
			{
				splitView.DisplayMode = mode;
			}
        }

		private void Background_Click(object sender, RoutedEventArgs e)
		{
			var colorMapping = new Dictionary<string, Color>
			{
				["LightGray"] = Colors.LightGray,
				["Red"] = Colors.Red,
				["Green"] = Colors.Green,
				["Blue"] = Colors.Blue,
			};

			if ((sender as FrameworkElement)?.Tag is string tag && colorMapping.TryGetValue(tag, out var color))
			{
				splitView.PaneBackground = new SolidColorBrush(color);
				PaneBackgroundRun.Text = tag;
			}
		}
    }

    public class NavLink
    {
        public string Label { get; set; }
        public Symbol Symbol { get; set; }
    }
}
