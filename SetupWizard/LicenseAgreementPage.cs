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

using System;
using System.Collections.Generic;

using MatterHackers.Agg.PlatformAbstract;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl;
using MatterHackers.MatterControl.CustomWidgets;
using MatterHackers.Agg;

public class LicenseAgreementPage : WizardPage
{
	public LicenseAgreementPage()
		: base("Cancel")
	{
		string eulaText = StaticData.Instance.ReadAllText("MatterControl EULA.txt").Replace("\r\n", "\n");

		var scrollable = new ScrollableWidget(true);
		scrollable.AnchorAll();
		scrollable.ScrollArea.HAnchor = HAnchor.ParentLeftRight;
		contentRow.AddChild(scrollable);

		var textBox = new WrappedTextWidget(eulaText, 200, textColor: ActiveTheme.Instance.PrimaryTextColor);
		scrollable.AddChild(textBox);

		var acceptButton = textImageButtonFactory.Generate("Accept".Localize());
		acceptButton.Click += (s, e) =>
		{
			UserSettings.Instance.set("SoftwareLicenseAccepted", "true");
			UiThread.RunOnIdle(WizardWindow.Close);
		};

		acceptButton.Visible = true;
		cancelButton.Visible = true;

		cancelButton.Click += (s, e) => UiThread.RunOnIdle(WizardWindow.Close);

		//Add buttons to buttonContainer
		footerRow.AddChild(acceptButton);
		footerRow.AddChild(new HorizontalSpacer());
		footerRow.AddChild(cancelButton);

		footerRow.Visible = true;
	}

	public override void OnClosed(EventArgs e)
	{
		if (UserSettings.Instance.get("SoftwareLicenseAccepted") != "true")
		{
			UiThread.RunOnIdle(MatterControlApplication.Instance.Close);
		}

		base.OnClosed(e);
	}
}
