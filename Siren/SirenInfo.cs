using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Siren
{
	public class SirenInfo : GH_AssemblyInfo
	{
		static SirenFileMenu gHSynthFileMenu;

		public SirenInfo()
		{
			gHSynthFileMenu = new SirenFileMenu();
			gHSynthFileMenu.AddToMenu();

			SirenSettings.SampleRate = 44100;
			SirenSettings.TimeScale = 10;
			SirenSettings.AmplitudeScale = 10;
			SirenSettings.Tempo = 120;
		}

		public override string Name
		{
			get
			{
				return "GHSynth";
			}
		}
		public override Bitmap Icon
		{
			get
			{
				//Return a 24x24 pixel bitmap to represent this GHA library.
				return null;
			}
		}
		public override string Description
		{
			get
			{
				//Return a short string describing the purpose of this GHA library.
				return "";
			}
		}
		public override Guid Id
		{
			get
			{
				return new Guid("9a3a4e75-6f0c-4cc3-b475-c2e28669d622");
			}
		}

		public override string AuthorName
		{
			get
			{
				//Return a string identifying you or your company.
				return "";
			}
		}
		public override string AuthorContact
		{
			get
			{
				//Return a string representing your preferred contact details.
				return "";
			}
		}
	}
}
