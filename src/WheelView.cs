using System;
using System.Windows;
using System.Windows.Media;

namespace VolanteStudio {
    // Vector sports wheel: stays sharp at any display scale and rotates with live input.
    public sealed class WheelView : FrameworkElement {
        public double Angle { get; set; }
        static Brush Solid(string color) { return (Brush)new BrushConverter().ConvertFromString(color); }
        static Geometry Shape(string data) { return Geometry.Parse(data); }
        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            dc.DrawEllipse(null,new Pen(Solid("#1B2531"),1),new Point(200,200),190,190);
            dc.PushTransform(new RotateTransform(Angle,200,200));
            // Shift paddles behind the rim.
            for(int side=-1;side<=1;side+=2)
                dc.DrawRoundedRectangle(new LinearGradientBrush(Color.FromRgb(95,103,115),Color.FromRgb(27,33,43),0),new Pen(Solid("#748093"),1),new Rect(side<0?74:301,115,25,128),9,9);
            var rim=Shape("M 200,34 C 296,34 366,104 366,199 C 366,258 336,314 299,343 Q 285,355 264,355 L 136,355 Q 115,355 101,343 C 64,314 34,258 34,199 C 34,104 104,34 200,34 Z M 200,71 C 126,71 73,127 73,199 C 73,243 94,284 124,311 Q 130,317 144,317 L 256,317 Q 270,317 276,311 C 306,284 327,243 327,199 C 327,127 274,71 200,71 Z");
            dc.DrawGeometry(new LinearGradientBrush(Color.FromRgb(57,63,73),Color.FromRgb(13,17,24),90),new Pen(Solid("#667180"),1.5),rim);
            // Sculpted thumb grips.
            dc.DrawGeometry(Solid("#111720"),new Pen(Solid("#394352"),1),Shape("M 48,132 Q 63,122 80,143 L 104,172 Q 110,187 94,201 L 75,221 Q 54,234 40,220 Q 34,174 48,132 Z M 352,132 Q 337,122 320,143 L 296,172 Q 290,187 306,201 L 325,221 Q 346,234 360,220 Q 366,174 352,132 Z"));
            // Three machined spokes and flat lower section.
            var metal=new LinearGradientBrush(Color.FromRgb(74,83,97),Color.FromRgb(23,29,38),90);
            dc.DrawGeometry(metal,new Pen(Solid("#687589"),1),Shape("M 79,176 L 165,177 L 190,206 L 163,233 L 76,214 Z M 321,176 L 235,177 L 210,206 L 237,233 L 324,214 Z M 174,227 L 226,227 L 245,325 L 155,325 Z"));
            dc.DrawGeometry(Solid("#090E15"),new Pen(Solid("#384353"),1),Shape("M 188,254 L 212,254 L 220,307 L 180,307 Z"));
            // Perforated leather on both hand grips.
            for(int side=-1;side<=1;side+=2)
                for(int row=0;row<9;row++)
                    for(int col=0;col<3;col++)
                        dc.DrawEllipse(Solid("#070B10"),null,new Point(200+side*(145+col*5),153+row*7),1.1,1.1);
            for(int side=-1;side<=1;side+=2) {
                dc.DrawRoundedRectangle(Solid("#101721"),new Pen(Solid("#48566B"),1),new Rect(side<0?105:252,184,43,28),7,7);
                dc.DrawEllipse(Solid("#278DFA"),new Pen(Solid("#8BC7FF"),0.7),new Point(200+side*74,198),6,6);
            }
            dc.DrawEllipse(new LinearGradientBrush(Color.FromRgb(58,68,82),Color.FromRgb(16,23,33),90),new Pen(Solid("#7A879A"),1.5),new Point(200,206),47,47);
            dc.DrawEllipse(Solid("#101721"),new Pen(Solid("#36495F"),1),new Point(200,206),30,30);
            for(int i=0;i<6;i++) {
                double a=i*Math.PI/3;
                dc.DrawEllipse(Solid("#A1ADBD"),new Pen(Solid("#141A23"),1),new Point(200+38*Math.Cos(a),206+38*Math.Sin(a)),3,3);
            }
            // Center emblem and twelve-o'clock alignment stripe.
            dc.DrawGeometry(Solid("#329BFF"),null,Shape("M 185,198 L 215,198 L 211,204 L 197,204 L 193,210 L 209,210 L 205,216 L 181,216 Z"));
            dc.DrawGeometry(Solid("#2C98FF"),new Pen(Solid("#86C6FF"),1),Shape("M 192,35 Q 200,34 208,35 L 208,71 L 192,71 Z"));
            dc.DrawLine(new Pen(Solid("#2C98FF"),2),new Point(148,342),new Point(252,342));
            dc.Pop();
        }
    }
}
