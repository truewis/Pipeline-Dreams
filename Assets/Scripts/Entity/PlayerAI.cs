﻿namespace PipelineDreams.Entity {
    public class PlayerAI : AI {

        public override float EntityClock {
            get {
                return CM.Clock;
            }
            set {
                if (value != CM.Clock)
                    CM.AddTime(value - CM.Clock);
            }
        }

        protected override void Act() {
            //Do nothing.
        }

    }
}