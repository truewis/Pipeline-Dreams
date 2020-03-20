namespace PipelineDreams.Instruction
{
    public class Crystal : Instruction {

        public override IClockTask Operation(float startClock)
        {

            return PassParam(new InstructionCrystalTask(), startClock);
        }
        
    }
    public abstract partial class Instruction
    {
        /// <summary>
        /// Field instruction task used above.
        /// </summary>
        protected class InstructionCrystalTask : InstructionTask
        {
            protected override void OnRunStart()
            {
            }
        }
    }

}