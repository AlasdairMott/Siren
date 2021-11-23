using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NAudio.Wave.SampleProviders;

namespace Siren.Components
{
    public class UnityMixerComponent : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the MixerComponent class.
        /// </summary>
        public UnityMixerComponent()
          : base("Mixer", "Mixer",
              "Additively combines multiple signals into a single signal.",
              "Siren", "VCA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new CachedSoundParameter(), "Wave", "W1", "Wave input", GH_ParamAccess.list);
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
            var wavesIn = new List<CachedSound>();
            for (var i = 0; i < Params.Input.Count; i++)
            {
                var waves = new List<CachedSound>();
                DA.GetDataList(i, waves);
                wavesIn.AddRange(waves.Where(w => w != null));
            }
            if (wavesIn.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No waves were providided.");
                return;
            }

            var multiplier = 1 / wavesIn.Count;
            var mixer = new MixingSampleProvider(wavesIn.Select(s => s.ToSampleProvider()));

            DA.SetData(0, mixer);
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index) => side == GH_ParameterSide.Input;

        public bool CanRemoveParameter(GH_ParameterSide side, int index) => side == GH_ParameterSide.Input;

        public IGH_Param CreateParameter(GH_ParameterSide side, int index) => new CachedSoundParameter { NickName = "-" };

        public bool DestroyParameter(GH_ParameterSide side, int index) => true;

        //Parameter maintenance from https://github.com/andrewheumann/jSwan/blob/e92d4ded95fc6f307c06485855b45bb37bba5c8c/jSwan/Serialize.cs#L107
        public void VariableParameterMaintenance()
        {
            for (var i = 0; i < Params.Input.Count; i++)
            {
                var param = Params.Input[i];
                if (param.NickName == "-")
                {
                    param.Name = $"Wave {i + 1}";
                    param.NickName = $"W{i + 1}";
                }
                else
                {
                    param.Name = param.NickName;
                }
                param.Description = $"Input {i + 1}";
                param.Optional = true;
                param.MutableNickName = true;
                param.Access = GH_ParamAccess.list;
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.mixer;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("77093d78-0394-40ae-aea3-d3892d6ec1e1"); }
        }
    }
}
