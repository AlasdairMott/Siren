using System;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Siren.Utilities;

namespace Siren.Audio
{
    public class AttenuverterComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AttenuverterComponent class.
        /// </summary>
        public AttenuverterComponent()
          : base("Attenuverter", "AttenU",
                "Reduces or polarises (inverts) a signal's level/amplitude.",
                "Siren", "VCA")
        {
        }

        public override void CreateAttributes()
        {
            m_attributes = new GH_KnobAttributes(this, "-       +", 40f, 50f);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
            pManager.AddNumberParameter("Attenuation", "A", "Attenuation", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var waveIn = CachedSound.Empty;
            if (!DA.GetData(0, ref waveIn)) return;

            var attributes = m_attributes as GH_KnobAttributes;

            var attenuation = 1.0;
            if (!DA.GetData(1, ref attenuation)) 
            {
                attenuation = attributes.P;
                attenuation = SirenUtilities.Remap((float)attenuation, attributes.Min, attributes.Max, -2.0f, 2.0f);
                attributes.Locked = false;
            }
            else
            {
                attributes.P = SirenUtilities.Clamp((float)attenuation, attributes.Min, attributes.Max);
                attributes.Locked = true;
            }

            var attenuverter = new SampleProviders.AttenuverterProvider(waveIn.ToSampleProvider(), (float)attenuation);

            DA.SetData(0, attenuverter);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.attenuverter;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("aa66bb45-be86-4aa7-83ad-f3cc2666ecab"); }
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetDouble("attenuation", (m_attributes as GH_KnobAttributes).P);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            double att = 0.0;
            if (reader.TryGetDouble("attenuation", ref att))
            {
                (m_attributes as GH_KnobAttributes).P = (float)att;
            }
            return base.Read(reader);
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            base.RemovedFromDocument(document);
            (m_attributes as GH_KnobAttributes).Knob.DisposeGraphics();
        }
    }
}
