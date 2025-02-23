﻿/*
Copyright (c) 2014, Lars Brubaker
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

using System.Collections.Generic;

namespace MatterHackers.MatterControl.SlicerConfiguration
{
	public abstract class SliceEngineMapping
	{
		private string engineName;

		/// <summary>
		/// Application level settings control MatterControl behaviors but aren't used or passed through to the slice engine. Putting settings
		/// in this list ensures they show up for all slice engines and the lack of a MappedSetting for the engine guarantees that it won't pass
		/// through into the slicer config file
		/// </summary>
		protected HashSet<string> applicationLevelSettings = new HashSet<string>()
		{
			"bed_shape",
			"bed_size",
			"bed_temperature",
			"build_height",
			"cancel_gcode",
			"connect_gcode",
			"has_fan",
			"has_hardware_leveling",
			"has_heated_bed",
			"has_power_control",
			"has_sd_card_reader",
			"printer_name",
			"auto_connect",
			"baud_rate",
			"com_port",
			"delete_printer",
			"manual_probe_paper_width",
			"pause_gcode",
			"print_leveling_method",
			"print_leveling_required_to_print",
			"print_leveling_solution",
			"resume_first_layer_speed",
			"resume_position_before_z_home",
			"resume_gcode",
			"temperature",
			"z_can_be_negative",
			"z_homes_to_max",

			// TODO: merge the items below into the list above after some validation - setting that weren't previously mapped to Cura but probably should be. 
			"bed_remove_part_temperature",
			"extruder_wipe_temperature",
			"heat_extruder_before_homing",
			"include_firmware_updater",
			"layer_to_pause",
			"show_reset_connection"
		};

		public SliceEngineMapping(string engineName)
		{
			this.engineName = engineName;
		}

		public string Name { get { return engineName; } }

		public abstract bool MapContains(string canonicalSettingsName);
	}
}