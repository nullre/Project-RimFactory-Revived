﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ProjectRimFactory.Common
{
    public class CompEffecter : ThingComp, ITicker
    {
        public CompProperties_Effecter Props => (CompProperties_Effecter)this.props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            this.InitializeEffecter();

            this.parent.Map.GetComponent<PRFMapComponent>()?.AddTicker(this);
        }

        private Effecter effecter;

        private Sustainer sound;

        public void Tick()
        {
            if (this.Enable && !this.powerOff)
            {
                this.effecter?.EffectTick(this.parent, this.parent);
                this.sound?.SustainerUpdate();
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            this.FinalizeEffecter();
            map.GetComponent<PRFMapComponent>()?.RemoveTicker(this);
        }

        private bool enable = true;

        private void InitializeEffecter()
        {
            this.effecter = this.Props?.effect?.Spawn();
            this.sound = this.Props?.sound?.TrySpawnSustainer(this.parent);
        }

        private void FinalizeEffecter()
        {
            this.effecter?.Cleanup();
            this.sound?.End();
            this.effecter = null;
            this.sound = null;
        }

        public bool Enable
        {
            get
            {
                return this.enable;
            }
            set
            {
                if (this.enable != value)
                {
                    this.enable = value;
                    if (this.enable)
                    {
                        this.InitializeEffecter();
                    }
                    else
                    {
                        this.FinalizeEffecter();
                    }
                }
            }
        }

        private bool powerOff = false;

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (CompPowerTrader.PowerTurnedOffSignal == signal)
            {
                this.powerOff = true;
                this.FinalizeEffecter();
            }
            else if(CompPowerTrader.PowerTurnedOnSignal == signal)
            {
                this.powerOff = false;
                this.InitializeEffecter();
            }
        }
    }

    public class CompProperties_Effecter : CompProperties
    {
        public EffecterDef effect;

        public SoundDef sound;
        
        public CompProperties_Effecter()
        {
            this.compClass = typeof(CompEffecter);
        }
    }
}
