using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using NAudio.Wave;

namespace Siren
{
    public class CachedSoundGoo : GH_Goo<CachedSound>
    {
        public override bool IsValid => Value.Length > 0;

        public override string TypeName => "Sound";

        public override string TypeDescription => "Siren Audio data";

        public CachedSoundGoo()
        {
            Value = CachedSound.Empty;
        }
        public CachedSoundGoo(CachedSound cachedSound)
        {
            if (cachedSound == null) cachedSound = CachedSound.Empty;
            Value = cachedSound;
        }
        public CachedSoundGoo(ISampleProvider sampleProvider)
        {
            Value = new CachedSound(sampleProvider);
        }

        #region casting methods
        public override bool CastTo<Q>(ref Q target)
        {
            if (typeof(Q).IsAssignableFrom(typeof(CachedSound)))
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
            if (typeof(CachedSound).IsAssignableFrom(source.GetType()))
            {
                Value = (CachedSound)source;
                return true;
            }
            else if (typeof(ISampleProvider).IsAssignableFrom(source.GetType()))
            {
                Value = new CachedSound(source as ISampleProvider);
                return true;
            }
            return false;
        }
        #endregion

        public override IGH_Goo Duplicate() => new CachedSoundGoo(Value.Clone());

        public override string ToString() => Value.ToString();

        public override bool Write(GH_IWriter writer)
        {
            if (Value != null)
            {
                writer.SetByteArray("cachedWaveBuffer", Value.ToByteArray());
            }
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("cachedWaveBuffer"))
            {
                var buffer = reader.GetByteArray("cachedWaveBuffer");
                Value = CachedSound.FromByteArray(buffer);
            }
            else Value = null;
            return true;
        }
    }
}
