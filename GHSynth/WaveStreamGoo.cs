using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using NAudio.Wave;


namespace GHSynth
{
	public class WaveStreamGoo : GH_Goo<WaveStream>
	{
        public override bool IsValid => Value.Length > 0;

        public override string TypeName => "Gooey type name" + Value.GetType().ToString();

		public override string TypeDescription => "Gooey type description" + Value.WaveFormat.ToString();

        public WaveStreamGoo()
        {
            this.Value = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat());
        }
        public WaveStreamGoo(WaveStream stream)
        {
            if (stream == null) stream = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat());
            this.Value = stream;
        }

        #region casting methods
        public override bool CastTo<Q>(ref Q target)
        {
            if (typeof(Q).IsAssignableFrom(typeof(WaveStream)))
            {
                if (Value == null) target = default(Q);
                else target = (Q)(object)Value;
                return true;
            }
            else if (typeof(Q).IsAssignableFrom(typeof(RawSourceWaveStream)))
            {
                if (Value == null) target = default(Q);
                else target = (Q)(object)Value;
                return true;
            }
            target = default(Q);
            return false;
        }
        public override bool CastFrom(object source)
        {
            if (source == null) { return false; }
            if (typeof(WaveStream).IsAssignableFrom(source.GetType()))
            {
                Value = (WaveStream)source;
                return true;
            }
            return false;
        }
        #endregion

        public override IGH_Goo Duplicate()
		{
			RawSourceWaveStream stream;
			using (MemoryStream memoryStream = new MemoryStream()) 
			{
				Value.CopyTo(memoryStream);
				stream = new RawSourceWaveStream(memoryStream, new WaveFormat(44100, 16, 1));
			}
			return new WaveStreamGoo(stream);
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public class WaveStreamParameter : GH_Param<WaveStreamGoo>
    {
		public WaveStreamParameter()
			: base(new GH_InstanceDescription(
                "WaveStreamParameter",
                "WaveStreamParameter", 
				"Audio wave", 
				"GHSynth", 
				"Data")) { }

		protected override System.Drawing.Bitmap Icon => Properties.Resources.metronome;

		public override Guid ComponentGuid => new Guid("08a1577a-7dff-4163-a2c9-2dbd928626c4");
	}
}
