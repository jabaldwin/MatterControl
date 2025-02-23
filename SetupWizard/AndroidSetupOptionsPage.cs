﻿/*
Copyright (c) 2016, Kevin Pope, John Lewin
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

using System;

using MatterHackers.Agg;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.PrinterCommunication;
using MatterHackers.MatterControl.CustomWidgets;

namespace MatterHackers.MatterControl
{
	public class SetupOptionsPage : WizardPage
	{
		public SetupOptionsPage()
			: base("Done")
		{
			headerLabel.Text = "Setup Options".Localize();

			textImageButtonFactory.borderWidth = 1;
			textImageButtonFactory.normalBorderColor = RGBA_Bytes.White;

			contentRow.AddChild(new SetupPrinterView(this.textImageButtonFactory) { WizardPage = this });
			contentRow.AddChild(new SetupAccountView(this.textImageButtonFactory));
			contentRow.AddChild(new EnterCodesView(this.textImageButtonFactory));

			footerRow.AddChild(new HorizontalSpacer());
			footerRow.AddChild(cancelButton);

			cancelButton.Text = "Back".Localize();
		}
	}

	public class EnterCodesView : SetupViewBase
	{
		public static EventHandler RedeemDesignCode;
		public static EventHandler EnterShareCode;

		public EnterCodesView(TextImageButtonFactory textImageButtonFactory) : base("")
		{
			this.textImageButtonFactory = textImageButtonFactory;
			FlowLayoutWidget buttonContainer = new FlowLayoutWidget()
			{
				HAnchor = HAnchor.ParentLeftRight,
				Margin = new BorderDouble(0, 14)
			};
			mainContainer.AddChild(buttonContainer);

			if (UserSettings.Instance.IsTouchScreen)
			{
				// the redeem design code button
				Button redeemPurchaseButton = textImageButtonFactory.Generate("Redeem Purchase".Localize());
				redeemPurchaseButton.Enabled = true; // The library selector (the first library selected) is protected so we can't add to it.
				redeemPurchaseButton.Name = "Redeem Code Button";
				redeemPurchaseButton.Margin = new BorderDouble(0, 0, 10, 0);
				redeemPurchaseButton.Click += (sender, e) =>
				{
					RedeemDesignCode?.Invoke(this, null);
				};
				buttonContainer.AddChild(redeemPurchaseButton);

				// the redeem a share code button
				Button redeemShareButton = textImageButtonFactory.Generate("Enter Share Code".Localize());
				redeemShareButton.Enabled = true; // The library selector (the first library selected) is protected so we can't add to it.
				redeemShareButton.Name = "Enter Share Code";
				redeemShareButton.Margin = new BorderDouble(0, 0, 3, 0);
				redeemShareButton.Click += (sender, e) =>
				{
					EnterShareCode?.Invoke(this, null);
				};

				buttonContainer.AddChild(redeemShareButton);
			}
		}
	}

	public class SetupPrinterView : SetupViewBase
	{
		internal WizardPage WizardPage { get; set; }

		private Button disconnectButton;
		private TextWidget connectionStatus;
		private event EventHandler unregisterEvents;

		public SetupPrinterView(TextImageButtonFactory textImageButtonFactory)
			: base("Printer Profile")
		{
			this.textImageButtonFactory = textImageButtonFactory;

			var buttonContainer = new FlowLayoutWidget()
			{
				HAnchor = HAnchor.ParentLeftRight,
				Margin = new BorderDouble (0, 14)
			};
			mainContainer.AddChild(buttonContainer);

			var printerSelectorAndEditButton = new FlowLayoutWidget()
			{
				HAnchor = HAnchor.ParentLeftRight,
			};
			buttonContainer.AddChild(printerSelectorAndEditButton);

			var printerSelector = new PrinterSelector();
			printerSelector.AddPrinter += (s, e) => WizardPage.WizardWindow.ChangeToSetupPrinterForm();
			printerSelectorAndEditButton.AddChild(printerSelector);

			var editButton = TextImageButtonFactory.GetThemedEditButton();
			editButton.ToolTipText = "Edit Selected Setting".Localize();
			editButton.VAnchor = VAnchor.ParentCenter;
			editButton.Click += UiNavigation.GoToEditPrinter_Click;
			printerSelectorAndEditButton.AddChild(editButton);

			disconnectButton = textImageButtonFactory.Generate("Disconnect");
			disconnectButton.Margin = new BorderDouble(left: 12);
			disconnectButton.VAnchor = VAnchor.ParentCenter;
			disconnectButton.Click += (sender, e) =>
			{
				PrinterConnectionAndCommunication.Instance.Disable();
				WizardPage.WizardWindow.ChangeToPage<SetupOptionsPage>();
			};
			buttonContainer.AddChild(disconnectButton);

			connectionStatus = new TextWidget("Status:", pointSize: 12, textColor: ActiveTheme.Instance.PrimaryTextColor)
			{
				HAnchor = HAnchor.ParentLeftRight
			};
			mainContainer.AddChild(connectionStatus);

			PrinterConnectionAndCommunication.Instance.CommunicationStateChanged.RegisterEvent(updateConnectedState, ref unregisterEvents);
			updateConnectedState(null, null);
		}

		private void updateConnectedState(object sender, EventArgs e)
		{
			if (disconnectButton != null)
			{
				disconnectButton.Visible = PrinterConnectionAndCommunication.Instance.PrinterIsConnected;
			}

			if (connectionStatus != null)
			{
				connectionStatus.Text = string.Format ("{0}: {1}", "Status".Localize().ToUpper(), PrinterConnectionAndCommunication.Instance.PrinterConnectionStatusVerbose);
			}

			this.Invalidate();
		}
	}

	public class SetupAccountView : SetupViewBase
	{
		private Button signInButton;
		private Button signOutButton;
		private TextWidget statusMessage;

		public SetupAccountView(TextImageButtonFactory textImageButtonFactory)
			: base("My Account")
		{
			this.textImageButtonFactory = textImageButtonFactory;

			bool signedIn = true;
			string username = ApplicationController.Instance.GetSessionUsername();
			if (username == null)
			{
				signedIn = false;
				username = "Not Signed In";
			}

			mainContainer.AddChild(new TextWidget(username, pointSize: 16, textColor: ActiveTheme.Instance.PrimaryTextColor));

			FlowLayoutWidget buttonContainer = new FlowLayoutWidget();
			buttonContainer.HAnchor = HAnchor.ParentLeftRight;
			buttonContainer.Margin = new BorderDouble(0, 14);

			signInButton = textImageButtonFactory.Generate("Sign In");
			signInButton.Margin = new BorderDouble(left: 0);
			signInButton.VAnchor = VAnchor.ParentCenter;
			signInButton.Visible = !signedIn;
			signInButton.Click +=  (s, e) => UiThread.RunOnIdle(() =>
			{
				signInButton.Visible = false;
				signOutButton.Visible = false;
				statusMessage.Visible = true;
				ApplicationController.Instance.StartLogin();
			});
			buttonContainer.AddChild(signInButton);

			signOutButton = textImageButtonFactory.Generate("Sign Out");
			signOutButton.Margin = new BorderDouble(left: 0);
			signOutButton.VAnchor = VAnchor.ParentCenter;
			signOutButton.Visible = signedIn;
			signOutButton.Click += (s, e) => UiThread.RunOnIdle(() =>
			{
				signInButton.Visible = false;
				signOutButton.Visible = false;
				statusMessage.Visible = true;
				ApplicationController.Instance.StartLogout();
			});
			buttonContainer.AddChild(signOutButton);

			statusMessage = new TextWidget("Please wait...", pointSize: 12, textColor: ActiveTheme.Instance.SecondaryAccentColor);
			statusMessage.Visible = false;
			buttonContainer.AddChild(statusMessage);

			mainContainer.AddChild(buttonContainer);
		}
	}

	public class SetupViewBase : AltGroupBox
	{
		protected TextImageButtonFactory textImageButtonFactory;
		protected FlowLayoutWidget mainContainer;

		public SetupViewBase(string title)
			: base(title != "" ? new TextWidget(title, pointSize: 18, textColor: ActiveTheme.Instance.SecondaryAccentColor) : null)
		{
			this.Margin = new BorderDouble(2, 10, 2, 0);

			mainContainer = new FlowLayoutWidget(Agg.UI.FlowDirection.TopToBottom)
			{
				HAnchor = HAnchor.ParentLeftRight,
				Margin = new BorderDouble(6, 0, 0, 6)
			};
			AddChild(mainContainer);
		}
	}
}
