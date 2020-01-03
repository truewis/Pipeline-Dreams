namespace PipelineDreams
{
    public class InstructionIntersect : Instruction {

        public override IClockTask Operation(float startClock)
        {

            return PassParam(new InstructionIntersectTask());
        }
        
    }
    public abstract partial class Instruction
    {
        /// <summary>
        /// Field instruction task used above.
        /// </summary>
        protected class InstructionIntersectTask : InstructionTask
        {
            protected override void OnRunStart()
            {
            }
        }
    }

}
