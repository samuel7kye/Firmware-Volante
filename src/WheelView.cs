using System;
using System.Windows;
using System.Windows.Media;

namespace VolanteStudio {
    public sealed class WheelView : FrameworkElement {
        public double Angle { get; set; }
        static Brush Solid(string color) { return (Brush)new BrushConverter().ConvertFromString(color); }
        static LinearGradientBrush Metal() {
            return new LinearGradientBrush(new GradientStopCollection { new GradientStop(Color.FromRgb(105,113,124),0),new GradientStop(Color.FromRgb(49,55,64),0.35),new GradientStop(Color.FromRgb(26,31,39),0.65),new GradientStop(Color.FromRgb(72,82,96),1) },new Point(0,0),new Point(1,1));
        }
        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            dc.DrawEllipse(null,new Pen(Solid("#1B2531"),0.8),new Point(200,200),184,184);
            dc.DrawEllipse(null,new Pen(Solid("#121D29"),0.8),new Point(200,200),198,198);
            dc.PushTransform(new RotateTransform(Angle,200,200));
            var shadow=new RadialGradientBrush(new GradientStopCollection { new GradientStop(Color.FromArgb(140,0,0,0),0.6),new GradientStop(Colors.Transparent,1) });
            dc.DrawEllipse(shadow,null,new Point(200,215),194,184);
            dc.DrawEllipse(Solid("#0A0D12"),new Pen(Solid("#333B46"),2),new Point(200,200),166,166);
            dc.DrawEllipse(null,new Pen(new LinearGradientBrush(Color.FromRgb(65,70,78),Color.FromRgb(13,16,21),90),28),new Point(200,200),150,150);
            dc.DrawEllipse(null,new Pen(Solid("#10141A"),1),new Point(200,200),136,136);
            dc.DrawEllipse(null,new Pen(Solid("#444C58"),1),new Point(200,200),164,164);
            for(int i=0;i<96;i++) {
                double a=i*Math.PI*2/96;
                dc.DrawLine(new Pen(Solid(i%2==0?"#454B53":"#202730"),0.7),new Point(200+155*Math.Cos(a),200+155*Math.Sin(a)),new Point(200+159*Math.Cos(a+0.008),200+159*Math.Sin(a+0.008)));
            }
            var metal=Metal();
            foreach(var shape in new[]{"M 158,178 L 65,150 Q 51,162 57,181 L 160,224 L 180,198 Z","M 242,178 L 335,150 Q 349,162 343,181 L 240,224 L 220,198 Z","M 174,225 L 178,338 Q 200,349 222,338 L 226,225 Z"})
                dc.DrawGeometry(metal,new Pen(Solid("#515B69"),1),Geometry.Parse(shape));
            dc.DrawGeometry(Solid("#10151D"),null,Geometry.Parse("M 82,167 L 145,187 L 144,199 L 79,178 Z M 318,167 L 255,187 L 256,199 L 321,178 Z M 194,260 L 206,260 L 207,321 L 193,321 Z"));
            for(int side=-1;side<=1;side+=2) {
                dc.DrawEllipse(Solid("#0D1117"),new Pen(Solid("#465262"),1),new Point(200+side*111,183),7,7);
                dc.DrawEllipse(Solid("#0D1117"),new Pen(Solid("#465262"),1),new Point(200+side*85,192),5,5);
            }
            dc.DrawEllipse(new LinearGradientBrush(Color.FromRgb(66,75,88),Color.FromRgb(17,23,31),90),new Pen(Solid("#657285"),1),new Point(200,200),49,49);
            dc.DrawEllipse(Solid("#131B26"),new Pen(Solid("#3D4D61"),1),new Point(200,200),31,31);
            for(int i=0;i<6;i++) {
                double a=i*Math.PI/3-Math.PI/2;var p=new Point(200+39*Math.Cos(a),200+39*Math.Sin(a));
                dc.DrawEllipse(Solid("#9BA5B3"),new Pen(Solid("#0C1016"),1),p,3.7,3.7);
                dc.DrawLine(new Pen(Solid("#272E39"),1),new Point(p.X-1.6,p.Y-1.6),new Point(p.X+1.6,p.Y+1.6));
            }
            dc.DrawGeometry(Solid("#258FFF"),null,Geometry.Parse("M 185,191 L 209,191 L 205,196 L 191,196 L 188,202 L 208,202 L 201,212 L 178,212 L 182,207 L 197,207 L 200,202 L 181,202 Z"));
            dc.DrawRoundedRectangle(Solid("#238FFF"),new Pen(Solid("#68BCFF"),1),new Rect(193,34,14,28),2,2);
            dc.DrawLine(new Pen(Solid("#449EFA"),2),new Point(194,64),new Point(206,64));
            dc.Pop();
        }
    }
}
