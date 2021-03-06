using System;
using Grasshopper.Kernel;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Siren.Audio
{
    public class FMOscillatorComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FMOscillatorComponent class.
        /// </summary>
        public FMOscillatorComponent()
          : base("FMOscillatorComponent", "fmOsc",
              "Frequency Modulation Oscillator",
              "Siren", "Oscillators")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new CachedSoundParameter(), "Frequency", "F", "Frequency of the note", GH_ParamAccess.item);
            pManager.AddParameter(new CachedSoundParameter(), "OP-Depth", "Dp", "Operator depth", GH_ParamAccess.item);
            pManager.AddParameter(new CachedSoundParameter(), "Operator", "OP", "Operator wave", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new CachedSoundParameter(), "Wave", "W", "Wave output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var cvIn = CachedSound.Empty;
            if (!DA.GetData(0, ref cvIn)) return;

            var opDepthIn = CachedSound.Empty;
            if (!DA.GetData(1, ref opDepthIn))
            {
                opDepthIn = new CachedSound(SampleProviders.SignalGenerator.CreateSilence(cvIn.TotalTime));
            }

            ISampleProvider op;
            var operatorIn = CachedSound.Empty;
            if (!DA.GetData(2, ref operatorIn)){
                var silence = SampleProviders.SignalGenerator.CreateSilence(cvIn.TotalTime);

                var feedbackOperator = new SampleProviders.FMOscillatorProvider(cvIn.ToSampleProvider(), silence, opDepthIn.ToSampleProvider());
                op = new SampleProviders.FMOscillatorProvider(cvIn.ToSampleProvider(), feedbackOperator, opDepthIn.ToSampleProvider());
            }
            else
            {
                op = new SampleProviders.FMOscillatorProvider(cvIn.ToSampleProvider(), operatorIn.ToSampleProvider(), opDepthIn.ToSampleProvider());
            }

            DA.SetData(0, op);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.fm;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5c3de0b5-437a-4136-a0ac-282e39b64c28"); }
        }
    }
}
