using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace VolanteStudio {
    public sealed class SettingSpec {
        public string Key, Label, Help, Unit;
        public int Min, Max, DefaultValue, Scale;
        public SettingSpec(string key, string label, string help, int min, int max, int value, string unit, int scale) {
            Key=key; Label=label; Help=help; Min=min; Max=max; DefaultValue=value; Unit=unit; Scale=scale;
        }
        public int ToDevice(int value) {
            if (value < Min || value > Max) throw new ArgumentOutOfRangeException(Key);
            return checked(value * Scale);
        }
    }
    public static class SettingsCatalog {
        public static readonly SettingSpec[] All = {
            new SettingSpec("Angle", "Ângulo de rotação", "Curso total do volante, de batente a batente.", 30,1080,900,"°",1),
            new SettingSpec("Maxforce", "Intensidade", "Força máxima enviada ao motor.",0,100,30,"%",1),
            new SettingSpec("Minforce", "Força mínima", "Compensa a resistência inicial do motor.",0,100,0,"%",1),
            new SettingSpec("GetDamper", "Amortecimento", "Suaviza movimentos rápidos da direção.",0,100,25,"%",2),
            new SettingSpec("GetSoftlock", "Batente", "Resistência ao atingir o limite de rotação.",0,100,50,"%",1),
            new SettingSpec("GetFriction", "Atrito", "Adiciona resistência constante à direção.",0,100,0,"%",2),
            new SettingSpec("GetInertia", "Inércia", "Simula o peso do conjunto do volante.",0,100,0,"%",2)
        };
        public static SettingSpec Find(string key) { return All.First(s=>s.Key==key); }
        public static Dictionary<string,int> Defaults() { return All.ToDictionary(s=>s.Key,s=>s.DefaultValue); }
    }
    public sealed class Profile {
        public string Name { get; set; }
        public Dictionary<string,int> Values = SettingsCatalog.Defaults();
    }
    public sealed class ProfileStore {
        readonly string directory;
        public ProfileStore(string directory) { this.directory=directory; }
        public List<Profile> Load() {
            var result=new List<Profile>();
            if(!Directory.Exists(directory)) return result;
            foreach(var path in Directory.GetFiles(directory,"*.xml")) {
                var doc=XDocument.Load(path);
                if(doc.Root == null || doc.Root.Name!="profile") throw new InvalidDataException("Perfil inválido: "+Path.GetFileName(path));
                var profile=new Profile { Name=(string)doc.Root.Attribute("name") };
                ValidateName(profile.Name);
                foreach(var node in doc.Root.Elements("setting")) {
                    string key=(string)node.Attribute("key");
                    var spec=SettingsCatalog.All.FirstOrDefault(s=>s.Key==key);
                    if(spec==null) continue;
                    int value;
                    if(!int.TryParse((string)node.Attribute("value"), NumberStyles.Integer,CultureInfo.InvariantCulture,out value)) throw new InvalidDataException("Valor inválido no perfil "+profile.Name);
                    spec.ToDevice(value); profile.Values[key]=value;
                }
                result.Add(profile);
            }
            return result.OrderBy(p=>p.Name).ToList();
        }
        public void Save(Profile profile) {
            ValidateName(profile.Name);
            foreach(var spec in SettingsCatalog.All) spec.ToDevice(profile.Values[spec.Key]);
            Directory.CreateDirectory(directory);
            var doc=new XDocument(new XElement("profile",new XAttribute("name",profile.Name),new XAttribute("version",1), SettingsCatalog.All.Select(s=>new XElement("setting",new XAttribute("key",s.Key),new XAttribute("value",profile.Values[s.Key])))));
            // A fixed-length digest avoids traversal, reserved names and oversized Unicode filenames.
            var bytes=System.Text.Encoding.UTF8.GetBytes(profile.Name.ToUpperInvariant());
            string file;
            using(var hash=System.Security.Cryptography.SHA256.Create())file=BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-","")+".xml";
            string target=Path.Combine(directory,file), temp=target+".tmp";
            doc.Save(temp);
            if(File.Exists(target)) File.Replace(temp,target,null); else File.Move(temp,target);
        }
        public static void ValidateName(string name) {
            if(string.IsNullOrWhiteSpace(name)||name.Trim()!=name||name.Length>48||name.Any(char.IsControl)) throw new ArgumentException("Use um nome de 1 a 48 caracteres, sem espaços nas extremidades.");
        }
    }
    public static class InputMath {
        public static double Percent(int value, int min, int max) { return Math.Max(0,Math.Min(100,100.0*(value-min)/(max-min))); }
    }
}
