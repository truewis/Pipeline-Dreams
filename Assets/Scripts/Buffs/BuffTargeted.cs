﻿namespace PipelineDreams
{
    public class BuffTargeted : Buff {
        private void BuffTargeted_OnDamagePacketEvaluation(DamagePacket obj)
        {
            obj.damage.AddFunction(new MutableValue.Multiplication() { Value = 2f });
        }
        public override void SetEnabled(bool enabled)
        {
            base.SetEnabled(enabled);
            var h = Holder.GetComponent<EntityHealth>();
            if (h != null) 
            if(enabled)
                    h.OnDamagePacketEvaluation += BuffTargeted_OnDamagePacketEvaluation;
            else
                    h.OnDamagePacketEvaluation -= BuffTargeted_OnDamagePacketEvaluation;
        }
    }
}