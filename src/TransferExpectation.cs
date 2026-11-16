using System;
using System.Collections.Generic;
namespace VolanteStudio {
    // These bytes identify receipts only. The original EMC component still generates and sends every report.
    // Observed using its public packet builder in memory, with a reserved nonexistent USB VID/PID.
    public sealed class TransferExpectation {
        readonly Dictionary<int,byte> fields=new Dictionary<int,byte>();
        public TransferExpectation(byte report,byte command) { fields[0]=report;fields[1]=command; }
        public bool Matches(byte[] bytes) {
            if(bytes==null)return false;
            foreach(var pair in fields)if(bytes.Length<=pair.Key||bytes[pair.Key]!=pair.Value)return false;
            return true;
        }
        TransferExpectation Word(int value) { fields[2]=(byte)(value>>8);fields[3]=(byte)(value&255);return this; }
        public static TransferExpectation Setting(string key,int value) {
            byte op;int offset=4;
            switch(key) {
                case "Angle":return new TransferExpectation(0xEE,0x02).Word(value);
                case "GetSetCpr":return new TransferExpectation(0xEE,0x01).Word(value);
                case "Maxforce":op=0x27;offset=6;break;
                case "Minforce":op=0x26;offset=6;break;
                case "GetDamper":op=0x20;break;
                case "GetSoftlock":op=0x06;break;
                case "GetFriction":op=0x21;break;
                case "GetInertia":op=0x22;break;
                default:throw new ArgumentException("Ajuste desconhecido: "+key);
            }
            var expected=new TransferExpectation(0xEE,op);expected.fields[offset]=checked((byte)value);return expected;
        }
        public static TransferExpectation Command(string parameter) {
            byte op;
            switch(parameter??"Center") {
                case "Center":op=0x07;break;case "Gasmin":op=0x09;break;case "Gasmax":op=0x0A;break;
                case "BrakeMin":op=0x0B;break;case "BrakeMax":op=0x0C;break;case "ClutchMin":op=0x0D;break;
                case "ClutchMax":op=0x0E;break;case "HandbrakeMin":op=0x0F;break;case "HandbrakeMax":op=0x10;break;
                default:throw new ArgumentException("Comando desconhecido");
            }
            return new TransferExpectation(0xEE,op);
        }
        public static TransferExpectation Bridge(string mode) {
            switch(mode) {
                case "2PWM + 1DIR":return new TransferExpectation(0x90,0x13);
                case "2PWM + 2DIR":return new TransferExpectation(0x90,0x14);
                case "1PWM + 1DIR":return new TransferExpectation(0x90,0x11);
                case "1PWM + 2DIR":return new TransferExpectation(0x90,0x12);
                default:throw new ArgumentException("Ponte H desconhecida");
            }
        }
    }
}
