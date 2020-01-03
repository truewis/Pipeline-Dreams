﻿namespace PipelineDreams{
    using MutableValue;
public struct DamagePacket
{
        /// <summary>
        /// The entity which performed this attack
        /// </summary>
    public Entity subject;
    public FunctionChainSingleUse damage;
    public FunctionChainSingleUse accuracy;
    public DamageCause damageCause;
}
    public enum DamageCause {
    Instruction, Item, Environment
    
    }
}