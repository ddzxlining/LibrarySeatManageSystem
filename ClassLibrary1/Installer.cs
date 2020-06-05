using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace ClassLibrary1
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            InitializeComponent();
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
            string str_ip = string.IsNullOrEmpty(this.Context.Parameters["IP"]) ? "(local)" : this.Context.Parameters["IP"].Trim();
            string str_db = string.IsNullOrEmpty(this.Context.Parameters["DB"]) ? "db_library" : this.Context.Parameters["DB"].Trim();
            string str_user = string.IsNullOrEmpty(this.Context.Parameters["USER"]) ? "sa" : this.Context.Parameters["USER"].Trim();
            string str_password = string.IsNullOrEmpty(this.Context.Parameters["PASSWORD"]) ? "@0812wgczysyefd" : this.Context.Parameters["PASSWORD"].Trim();

            string str_room = string.IsNullOrEmpty(this.Context.Parameters["ROOM"]) ? "-1" : this.Context.Parameters["ROOM"].Trim();
            string str_during = string.IsNullOrEmpty(this.Context.Parameters["DURING"]) ? "150" : this.Context.Parameters["DURING"].Trim();
            string str_redis = string.IsNullOrEmpty(this.Context.Parameters["REDIS"]) ? "123.206.26.20" : this.Context.Parameters["REDIS"].Trim();
            string str_bookstart = string.IsNullOrEmpty(this.Context.Parameters["BOOKSTART"]) ? "7:30" : this.Context.Parameters["BOOKSTART"].Trim();

            string currentDir = this.Context.Parameters["TARGETDIR"].Trim().ToString().Replace(@"\\",@"\");
            string path = currentDir + "LibrarySeatManageSystem.exe.Config";
            string text = File.ReadAllText(path);
            File.Delete(path);
            text = text.Replace("{{IP}}", str_ip);
            text = text.Replace("{{DB}}", str_db);
            text = text.Replace("{{USER}}", str_user);
            text = text.Replace("{{PASSWORD}}", str_password);

            text = text.Replace("{{ROOM}}", str_room);
            text = text.Replace("{{DURING}}", str_during);
            text = text.Replace("{{REDIS}}", str_redis);
            text = text.Replace("{{BOOKSTART}}", str_bookstart);
            File.WriteAllText(path, text);         
        }
    }
}
