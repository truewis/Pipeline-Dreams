namespace PipelineDreams
{
    public class InstructionSpatialRotation : Instruction {
        public InstructionSpatialRotation(EntityDataContainer eM, Entity player, CommandsContainer pC, InstructionData data, string variant) : base(eM, player, pC, data, variant) {
        }

        public override IClockTask Operation(float startClock)
        {

            return new InstructionSpatialRotationTask();
        }
        
    }
    public abstract partial class Instruction
    {
        /// <summary>
        /// Field instruction task used above.
        /// </summary>
        protected class InstructionSpatialRotationTask : InstructionTask
        {
            protected override void OnRunStart()
            {
            }
        }
    }

}