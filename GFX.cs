//
// Copyright (c) 2010 - 2019 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//

using System;
using System.Linq;
using Antmicro.Renode.Backends.Display;
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Peripherals.Memory;
using Antmicro.Renode.Utilities;

namespace Antmicro.Renode.Peripherals.Video
{
    public class GFX : AutoRepaintingVideo, IDoubleWordPeripheral, IProvidesRegisterCollection<DoubleWordRegisterCollection>, IKnownSize
    {
        public GFX(Machine machine) : base(machine)
        {
            this.machine = machine;

            RegistersCollection = new DoubleWordRegisterCollection(this);
            DefineRegisters();
        }

        public void WriteDoubleWord(long address, uint value)
        {
            RegistersCollection.Write(address, value);
        }

        public uint ReadDoubleWord(long offset)
        {
            return RegistersCollection.Read(offset);
        }

        public override void Reset()
        {
            RegistersCollection.Reset();
            bufferAddress = 0;
        }

        public long Size => 0x10000; //with palette memory and 2 tilemaps
        public DoubleWordRegisterCollection RegistersCollection { get; private set; }

        protected override void Repaint()
        {
            machine.SystemBus.ReadBytes(bufferAddress, buffer.Length, buffer, 0);
        }

        private void DefineRegisters()
        {
            var width = 512;
            var height = 320;
            Registers.FrameBufferAddress.Define(this)
                .WithValueField(0, 32, writeCallback: (_, val) =>
                {
                    if(val != 0)
                    {
                        bufferAddress = val;

                        Reconfigure(width, height, PixelFormat.A8);
                    }
                    else
                    {
                        // it stops the repainter by passing nulls
                        Reconfigure();
                    }
                })
            ;
        }

        private uint bufferAddress;

        private readonly Machine machine;

        private enum Registers
        {
            FrameBufferAddress = 0x0
        }
    }
}
