﻿using MatterHackers.Agg;
using MatterHackers.Agg.PlatformAbstract;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.CustomWidgets;
using MatterHackers.MatterControl.PrinterCommunication;
using MatterHackers.MatterControl.SlicerConfiguration;
using MatterHackers.SerialPortCommunication.FrostedSerial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MatterHackers.MatterControl.PrinterControls.PrinterConnections
{
	public class SetupStepComPortManual : ConnectionWizardPage
	{
		private static Regex linuxDefaultUIFilter = new Regex("/dev/ttyS*\\d+", RegexOptions.CultureInvariant | RegexOptions.Compiled);

		private Button nextButton;
		private Button connectButton;
		private Button refreshButton;
		private Button printerComPortHelpLink;

		private bool printerComPortIsAvailable = false;

		private TextWidget printerComPortHelpMessage;
		private TextWidget printerComPortError;

		private event EventHandler unregisterEvents;
		protected List<SerialPortIndexRadioButton> SerialPortButtonsList = new List<SerialPortIndexRadioButton>();

		public SetupStepComPortManual()
		{
			linkButtonFactory.fontSize = 8;

			FlowLayoutWidget printerComPortContainer = createComPortContainer();
			contentRow.AddChild(printerComPortContainer);

			//Construct buttons
			nextButton = textImageButtonFactory.Generate("Done".Localize());
			nextButton.Click += (s, e) => UiThread.RunOnIdle(Parent.Close);
			nextButton.Visible = false;

			connectButton = textImageButtonFactory.Generate("Connect".Localize());
			connectButton.Click += ConnectButton_Click;

			refreshButton = textImageButtonFactory.Generate("Refresh".Localize());
			refreshButton.Click += (s, e) => WizardWindow.ChangeToPage<SetupStepComPortManual>();

			//Add buttons to buttonContainer
			footerRow.AddChild(nextButton);
			footerRow.AddChild(connectButton);
			footerRow.AddChild(refreshButton);
			footerRow.AddChild(new HorizontalSpacer());
			footerRow.AddChild(cancelButton);

			PrinterConnectionAndCommunication.Instance.CommunicationStateChanged.RegisterEvent(onPrinterStatusChanged, ref unregisterEvents);
		}

		public override void OnClosed(EventArgs e)
		{
			unregisterEvents?.Invoke(this, null);
			base.OnClosed(e);
		}

		private FlowLayoutWidget createComPortContainer()
		{
			FlowLayoutWidget container = new FlowLayoutWidget(FlowDirection.TopToBottom);
			container.Margin = new BorderDouble(0);
			container.VAnchor = VAnchor.ParentBottomTop;
			BorderDouble elementMargin = new BorderDouble(top: 3);

			string serialPortLabel = LocalizedString.Get("Serial Port");
			string serialPortLabelFull = string.Format("{0}:", serialPortLabel);

			TextWidget comPortLabel = new TextWidget(serialPortLabelFull, 0, 0, 12);
			comPortLabel.TextColor = ActiveTheme.Instance.PrimaryTextColor;
			comPortLabel.Margin = new BorderDouble(0, 0, 0, 10);
			comPortLabel.HAnchor = HAnchor.ParentLeftRight;

			FlowLayoutWidget serialPortContainer = new FlowLayoutWidget(FlowDirection.TopToBottom);
			CreateSerialPortControls(serialPortContainer, null);

			FlowLayoutWidget comPortMessageContainer = new FlowLayoutWidget();
			comPortMessageContainer.Margin = elementMargin;
			comPortMessageContainer.HAnchor = HAnchor.ParentLeftRight;

			printerComPortError = new TextWidget("Currently available serial ports.".Localize(), 0, 0, 10);
			printerComPortError.TextColor = ActiveTheme.Instance.PrimaryTextColor;
			printerComPortError.AutoExpandBoundsToText = true;

			printerComPortHelpLink = linkButtonFactory.Generate("What's this?".Localize());
			printerComPortHelpLink.Margin = new BorderDouble(left: 5);
			printerComPortHelpLink.VAnchor = VAnchor.ParentBottom;
			printerComPortHelpLink.Click += (s, e) => printerComPortHelpMessage.Visible = !printerComPortHelpMessage.Visible;

			printerComPortHelpMessage = new TextWidget("The 'Serial Port' identifies which connected device is\nyour printer. Changing which usb plug you use may\nchange the associated serial port.\n\nTip: If you are uncertain, plug-in in your printer and hit\nrefresh. The new port that appears should be your\nprinter.".Localize(), 0, 0, 10);
			printerComPortHelpMessage.TextColor = ActiveTheme.Instance.PrimaryTextColor;
			printerComPortHelpMessage.Margin = new BorderDouble(top: 10);
			printerComPortHelpMessage.Visible = false;

			comPortMessageContainer.AddChild(printerComPortError);
			comPortMessageContainer.AddChild(printerComPortHelpLink);

			container.AddChild(comPortLabel);
			container.AddChild(serialPortContainer);
			container.AddChild(comPortMessageContainer);
			container.AddChild(printerComPortHelpMessage);

			container.HAnchor = HAnchor.ParentLeftRight;
			return container;
		}

		private void onPrinterStatusChanged(object sender, EventArgs e)
		{
			if (PrinterConnectionAndCommunication.Instance.PrinterIsConnected)
			{
				printerComPortHelpLink.Visible = false;
				printerComPortError.TextColor = ActiveTheme.Instance.PrimaryTextColor;
				printerComPortError.Text = "Connection succeeded".Localize() + "!";
				nextButton.Visible = true;
				connectButton.Visible = false;
			}
			else if (PrinterConnectionAndCommunication.Instance.CommunicationState != PrinterConnectionAndCommunication.CommunicationStates.AttemptingToConnect)
			{
				printerComPortHelpLink.Visible = false;
				printerComPortError.TextColor = RGBA_Bytes.Red;
				printerComPortError.Text = "Uh-oh! Could not connect to printer.".Localize();
				connectButton.Visible = true;
				nextButton.Visible = false;
			}
		}

		private void MoveToNextWidget(object state)
		{
			WizardWindow.ChangeToInstallDriverOrComPortOne();
		}

		private void ConnectButton_Click(object sender, EventArgs mouseEvent)
		{
			try
			{
				printerComPortHelpLink.Visible = false;
				printerComPortError.TextColor = ActiveTheme.Instance.PrimaryTextColor;

				printerComPortError.Text = "Attempting to connect".Localize() + "...";
				printerComPortError.TextColor = ActiveTheme.Instance.PrimaryTextColor;

				ActiveSliceSettings.Instance.SetComPort(GetSelectedSerialPort());
				PrinterConnectionAndCommunication.Instance.ConnectToActivePrinter();

				connectButton.Visible = false;
				refreshButton.Visible = false;
			}
			catch
			{
				printerComPortHelpLink.Visible = false;
				printerComPortError.TextColor = RGBA_Bytes.Red;
				printerComPortError.Text = "Oops! Please select a serial port.".Localize();
			}
		}

		protected void CreateSerialPortControls(FlowLayoutWidget comPortContainer, string activePrinterSerialPort)
		{
			int portIndex = 0;
			string[] allPorts = FrostedSerialPort.GetPortNames();
			IEnumerable<string> filteredPorts;

			if (OsInformation.OperatingSystem == OSType.X11)
			{
				// A default and naive filter that works well on Ubuntu 14
				filteredPorts = allPorts.Where(portName => portName != "/dev/tty" && !linuxDefaultUIFilter.Match(portName).Success);
			}
			else
			{
				// looks_like_mac -- serialPort.StartsWith("/dev/tty."); looks_like_pc -- serialPort.StartsWith("COM")
				filteredPorts = allPorts.Where(portName => portName.StartsWith("/dev/tty.") || portName.StartsWith("COM"));
			}

			IEnumerable<string> portsToCreate = filteredPorts.Any() ? filteredPorts : allPorts;

			// Add a radio button for each filtered port
			foreach (string portName in portsToCreate)
			{
				SerialPortIndexRadioButton comPortOption = createComPortOption(portName, activePrinterSerialPort == portName);
				if (comPortOption.Checked)
				{
					printerComPortIsAvailable = true;
				}

				SerialPortButtonsList.Add(comPortOption);
				comPortContainer.AddChild(comPortOption);

				portIndex++;
			}

			// Add a virtual entry for serial ports that were previously configured but are not currently connected
			if (!printerComPortIsAvailable && activePrinterSerialPort != null)
			{
				SerialPortIndexRadioButton comPortOption = createComPortOption(activePrinterSerialPort, true);
				comPortOption.Enabled = false;

				comPortContainer.AddChild(comPortOption);
				SerialPortButtonsList.Add(comPortOption);
				portIndex++;
			}

			//If there are still no com ports show a message to that effect
			if (portIndex == 0)
			{
				TextWidget comPortOption = new TextWidget(LocalizedString.Get("No COM ports available"));
				comPortOption.Margin = new BorderDouble(3, 6, 5, 6);
				comPortOption.TextColor = ActiveTheme.Instance.PrimaryTextColor;
				comPortContainer.AddChild(comPortOption);
			}
		}

		private SerialPortIndexRadioButton createComPortOption(string portName, bool isActivePrinterPort)
		{
			SerialPortIndexRadioButton comPortOption = new SerialPortIndexRadioButton(portName, portName)
			{
				HAnchor = HAnchor.ParentLeft,
				Margin = new BorderDouble(3, 3, 5, 3),
				TextColor = ActiveTheme.Instance.PrimaryTextColor,
				Checked = isActivePrinterPort
			};
			return comPortOption;
		}

		private string GetSelectedSerialPort()
		{
			foreach (SerialPortIndexRadioButton button in SerialPortButtonsList)
			{
				if (button.Checked)
				{
					return button.PortValue;
				}
			}

			throw new Exception(LocalizedString.Get("Could not find a selected button."));
		}

	}
}