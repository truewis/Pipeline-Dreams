namespace PipelineDreams.Instruction
{
    public class Reverse : Instruction {

        public override IClockTask Operation(float startClock)
        {

            return PassParam(new InstructionReverseTask(), startClock);
        }
        
    }
    public abstract partial class Instruction
    {
        /// <summary>
        /// Field instruction task used above.
        /// </summary>
        protected class InstructionReverseTask : InstructionTask
        {
            protected override void OnRunStart()
            {
            }
        }
    }

}