using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace VolanteStudio {
    public sealed class WheelView : FrameworkElement {
        public double Angle { get; set; }
        static Brush Solid(string color) { return (Brush)new BrushConverter().ConvertFromString(color); }
        static Geometry Shape(string data) { return Geometry.Parse(data); }
        void Label(DrawingContext dc,string text,double size,string color,double x,double y,bool bold) {
            var label=new FormattedText(text,CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface(new FontFamily("Segoe UI"),FontStyles.Normal,bold?FontWeights.Bold:FontWeights.Normal,FontStretches.Normal),size,Solid(color),VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(label,new Point(x-label.Width/2,y));
        }
        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            // Fixed decorative instrument cluster; deliberately independent of live input.
            dc.PushOpacity(0.48);
            dc.DrawEllipse(Solid("#0D1622"),new Pen(Solid("#39495E"),1),new Point(200,128),151,111);
            for(int i=0;i<=24;i++) {
                double a=(150+i*10)*Math.PI/180;
                var outer=new Point(200+140*Math.Cos(a),137+103*Math.Sin(a));
                var inner=new Point(200+(i%4==0?125:133)*Math.Cos(a),137+(i%4==0?88:96)*Math.Sin(a));
                dc.DrawLine(new Pen(Solid(i>=20?"#B14D51":"#70859E"),i%4==0?2:1),outer,inner);
                if(i%4==0)Label(dc,(i*10).ToString(),10,"#92A4BA",200+110*Math.Cos(a),132+75*Math.Sin(a),false);
            }
            Label(dc,"km/h",9,"#7D8FA6",200,92,false);
            dc.DrawLine(new Pen(Solid("#C25C62"),2),new Point(200,137),new Point(110,111));
            dc.Pop();
            dc.PushTransform(new RotateTransform(Angle,200,210));
            // Simplified R35 silhouette with silver Y spokes and broad airbag hub.
            for(int side=-1;side<=1;side+=2)
                dc.DrawRoundedRectangle(Solid("#4C5562"),new Pen(Solid("#8792A1"),1),new Rect(side<0?78:303,139,19,100),5,5);
            var rim=Shape("M 200,48 C 292,48 362,117 362,210 C 362,301 294,370 200,370 C 106,370 38,301 38,210 C 38,117 108,48 200,48 Z M 200,79 C 126,79 70,135 70,210 C 70,283 125,338 200,338 C 275,338 330,283 330,210 C 330,135 274,79 200,79 Z");
            dc.DrawGeometry(new LinearGradientBrush(Color.FromRgb(51,56,64),Color.FromRgb(12,16,22),90),new Pen(Solid("#626B78"),1.4),rim);
            dc.DrawGeometry(Solid("#121820"),new Pen(Solid("#3B4655"),1),Shape("M 51,148 Q 68,137 84,158 L 99,184 Q 104,197 87,212 L 69,238 L 42,229 Q 37,185 51,148 Z M 349,148 Q 332,137 316,158 L 301,184 Q 296,197 313,212 L 331,238 L 358,229 Q 363,185 349,148 Z"));
            var silver=new LinearGradientBrush(Color.FromRgb(172,181,192),Color.FromRgb(65,75,89),90);
            dc.DrawGeometry(silver,new Pen(Solid("#B0BAC7"),1),Shape("M 76,184 L 150,185 L 182,225 L 153,251 L 73,216 Z M 324,184 L 250,185 L 218,225 L 247,251 L 327,216 Z M 166,245 L 234,245 L 247,336 L 223,350 L 177,350 L 153,336 Z"));
            dc.DrawGeometry(Solid("#111720"),new Pen(Solid("#414C5C"),1),Shape("M 184,270 L 216,270 L 224,332 L 176,332 Z"));
            for(int side=-1;side<=1;side+=2) {
                dc.DrawRoundedRectangle(Solid("#181F29"),new Pen(Solid("#525F70"),1),new Rect(side<0?101:257,192,42,30),5,5);
                for(int j=0;j<3;j++)dc.DrawLine(new Pen(Solid("#8895A5"),1),new Point(200+side*78-7,200+j*7),new Point(200+side*78+7,200+j*7));
            }
            dc.DrawGeometry(new LinearGradientBrush(Color.FromRgb(47,53,62),Color.FromRgb(18,23,31),90),new Pen(Solid("#697585"),1.2),Shape("M 157,175 Q 200,157 243,175 Q 260,188 256,222 L 241,254 Q 200,277 159,254 L 144,222 Q 140,188 157,175 Z"));
            // GT-R badge: silver GT above the red R, without extra ornament.
            Label(dc,"GT",18,"#DFE5EC",200,194,true);
            Label(dc,"R",28,"#D32C3D",200,210,true);
            dc.DrawLine(new Pen(Solid("#9EAABA"),0.7),new Point(185,213),new Point(215,213));
            dc.Pop();
        }
    }
}
