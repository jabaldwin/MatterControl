﻿using MatterHackers.Agg;
using MatterHackers.Agg.Image;
using MatterHackers.Agg.ImageProcessing;
using MatterHackers.Agg.PlatformAbstract;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.ConfigurationPage.PrintLeveling;
using MatterHackers.MatterControl.CustomWidgets;
using MatterHackers.MatterControl.EeProm;
using MatterHackers.MatterControl.PrinterCommunication;
using MatterHackers.MatterControl.SlicerConfiguration;
using System;
using System.Diagnostics;
using System.IO;

namespace MatterHackers.MatterControl.ConfigurationPage
{
	public class CalibrationSettingsWidget : SettingsViewBase
	{
		private DisableableWidget printLevelingContainer;
		private event EventHandler unregisterEvents;
		private EditLevelingSettingsWindow editLevelingSettingsWindow;
		private TextWidget printLevelingStatusLabel;

		public CalibrationSettingsWidget()
			: base("Calibration".Localize())
		{
			printLevelingContainer = new DisableableWidget();
			if (!ActiveSliceSettings.Instance.GetValue<bool>("has_hardware_leveling"))
			{
				printLevelingContainer.AddChild(GetAutoLevelControl());

				mainContainer.AddChild(printLevelingContainer);
			}

			mainContainer.AddChild(new HorizontalLine(separatorLineColor));

			AddChild(mainContainer);

			PrinterConnectionAndCommunication.Instance.CommunicationStateChanged.RegisterEvent(PrinterStatusChanged, ref unregisterEvents);
			PrinterConnectionAndCommunication.Instance.EnableChanged.RegisterEvent(PrinterStatusChanged, ref unregisterEvents);

			SetVisibleControls();
		}

		private FlowLayoutWidget GetAutoLevelControl()
		{
			FlowLayoutWidget buttonRow = new FlowLayoutWidget();
			buttonRow.Name = "AutoLevelRowItem";
			buttonRow.HAnchor = HAnchor.ParentLeftRight;
			buttonRow.Margin = new BorderDouble(0, 4);

			Button editButton = TextImageButtonFactory.GetThemedEditButton();
			editButton.Margin = new BorderDouble(2, 2, 2, 0);
			editButton.VAnchor = Agg.UI.VAnchor.ParentTop;

			editButton.VAnchor = VAnchor.ParentCenter;
			editButton.Click += (sender, e) =>
			{
				UiThread.RunOnIdle(() =>
				{
					if (editLevelingSettingsWindow == null)
					{
						editLevelingSettingsWindow = new EditLevelingSettingsWindow();
						editLevelingSettingsWindow.Closed += (sender2, e2) =>
						{
							editLevelingSettingsWindow = null;
						};
					}
					else
					{
						editLevelingSettingsWindow.BringToFront();
					}
				});
			};

			Button runPrintLevelingButton = textImageButtonFactory.Generate("Configure".Localize().ToUpper());
			runPrintLevelingButton.Margin = new BorderDouble(left: 6);
			runPrintLevelingButton.VAnchor = VAnchor.ParentCenter;
			runPrintLevelingButton.Click += (sender, e) =>
			{
				UiThread.RunOnIdle(() => LevelWizardBase.ShowPrintLevelWizard(LevelWizardBase.RuningState.UserRequestedCalibration));
			};

			ImageBuffer levelingImage = StaticData.Instance.LoadIcon("leveling_32x32.png", 24, 24).InvertLightness();

			if (!ActiveTheme.Instance.IsDarkTheme)
			{
				levelingImage.InvertLightness();
			}

			ImageWidget levelingIcon = new ImageWidget(levelingImage);
			levelingIcon.Margin = new BorderDouble(right: 6);

			CheckBox printLevelingSwitch = ImageButtonFactory.CreateToggleSwitch(ActiveSliceSettings.Instance.GetValue<bool>("print_leveling_enabled"));
			printLevelingSwitch.VAnchor = VAnchor.ParentCenter;
			printLevelingSwitch.Margin = new BorderDouble(left: 16);
			printLevelingSwitch.CheckedStateChanged += (sender, e) =>
			{
				ActiveSliceSettings.Instance.DoPrintLeveling(printLevelingSwitch.Checked);
			};

			printLevelingStatusLabel = new TextWidget("")
			{
				AutoExpandBoundsToText = true,
				TextColor = ActiveTheme.Instance.PrimaryTextColor,
				VAnchor = VAnchor.ParentCenter,
				Text = "Software Print Leveling".Localize()
			};

			ActiveSliceSettings.Instance.DoPrintLevelingChanged.RegisterEvent((sender, e) =>
			{
				printLevelingSwitch.Checked = ActiveSliceSettings.Instance.GetValue<bool>("print_leveling_enabled");
			}, ref unregisterEvents);

			buttonRow.AddChild(levelingIcon);
			buttonRow.AddChild(printLevelingStatusLabel);
			buttonRow.AddChild(editButton);
			buttonRow.AddChild(new HorizontalSpacer());
			buttonRow.AddChild(runPrintLevelingButton);

			// only show the switch if leveling can be turned off (it can't if it is required).
			if (!ActiveSliceSettings.Instance.GetValue<bool>("print_leveling_required_to_print"))
			{
				buttonRow.AddChild(printLevelingSwitch);
			}

			return buttonRow;
		}

		public override void OnClosed(EventArgs e)
		{
			unregisterEvents?.Invoke(this, null);
			base.OnClosed(e);
		}

		private void PrinterStatusChanged(object sender, EventArgs e)
		{
			SetVisibleControls();
			this.Invalidate();
		}

		private void SetVisibleControls()
		{
			var currentStatus = PrinterConnectionAndCommunication.Instance.CommunicationState;
			var connected =
					currentStatus == PrinterConnectionAndCommunication.CommunicationStates.Connected ||
					currentStatus == PrinterConnectionAndCommunication.CommunicationStates.FinishedPrint;

			if (ActiveSliceSettings.Instance == null || !connected)
			{
				printLevelingContainer.SetEnableLevel(DisableableWidget.EnableLevel.Disabled);
			}
			else
			{
				printLevelingContainer.SetEnableLevel(DisableableWidget.EnableLevel.Enabled);
			}
		}
	}
}