﻿/*
Copyright (c) 2016, John Lewin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using MatterHackers.Agg;
using MatterHackers.Agg.Image;
using MatterHackers.Agg.ImageProcessing;
using MatterHackers.Agg.PlatformAbstract;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.PrinterControls.PrinterConnections;
using MatterHackers.MatterControl.SlicerConfiguration;
using MatterHackers.VectorMath;
using System;
using System.Linq;

namespace MatterHackers.MatterControl
{
	public class PrinterSelector : DropDownList
	{
		public event EventHandler AddPrinter;

		private EventHandler unregisterEvents;

		public PrinterSelector() : base("Printers".Localize() + "... ", useLeftIcons: true)
		{
			Rebuild();

			this.SelectionChanged += (s, e) =>
			{
				string printerID = this.SelectedValue;
				if (printerID == "new")
				{
					if (AddPrinter != null)
					{
						UiThread.RunOnIdle(() => AddPrinter(this, null));
					}
				}
				else
				{
					ActiveSliceSettings.SwitchToProfile(printerID);
				}
			};

			SliceSettingsWidget.SettingChanged.RegisterEvent(SettingChanged, ref unregisterEvents);
			ProfileManager.ProfilesListChanged.RegisterEvent(SettingChanged, ref unregisterEvents);
		}

		public void Rebuild()
		{
			this.MenuItems.Clear();

			//Add the menu items to the menu itself
			foreach (var printer in ProfileManager.Instance.ActiveProfiles)
			{
				this.AddItem(printer.Name, printer.ID.ToString());
			}

			if (ActiveSliceSettings.Instance != null)
			{
				this.SelectedValue = ActiveSliceSettings.Instance.ID;
				this.mainControlText.Text = ActiveSliceSettings.Instance.GetValue(SettingsKey.printer_name);
			}

			this.AddItem(
				StaticData.Instance.LoadIcon("icon_plus.png", 32, 32),
				"Add New Printer...",
				"new");
		}

		private void SettingChanged(object sender, EventArgs e)
		{
			string settingsName = (e as StringEventArgs)?.Data;
			if (settingsName != null && settingsName == SettingsKey.printer_name.ToString())
			{
				if (ProfileManager.Instance.ActiveProfile != null)
				{
					ProfileManager.Instance.ActiveProfile.Name = ActiveSliceSettings.Instance.GetValue(SettingsKey.printer_name);
				}
			}

			Rebuild();
		}

		public override void OnClosed(EventArgs e)
		{
			unregisterEvents?.Invoke(this, null);
			base.OnClosed(e);
		}
	}
}